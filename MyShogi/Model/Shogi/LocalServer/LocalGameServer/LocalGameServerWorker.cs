using System.Threading;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Player;
using MyShogi.Model.Shogi.Kifu;

namespace MyShogi.Model.Shogi.LocalServer
{
    public partial class LocalGameServer
    {

        #region 対局監視スレッド

        /// <summary>
        /// スレッドによって実行されていて、対局を管理している。
        /// pooling用のthread。少しダサいが、通知によるコールバックモデルでやるよりコードがシンプルになる。
        /// どのみち持ち時間の監視などを行わないといけないので、このようなworker threadは必要だと思う。
        /// </summary>
        private void thread_worker()
        {
            while (!workerStop)
            {
                foreach (var player in Players)
                {
                    player.OnIdle();
                }

                // UI側からのコマンドがあるかどうか
                CheckUICommand();

                // 指し手が指されたかのチェック
                CheckMove();

                // 時間消費のチェック
                CheckTime();

                // 10msごとに各種処理を行う。
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// UI側からのコマンドがあるかどうかを調べて、あれば実行する。
        /// </summary>
        private void CheckUICommand()
        {
            List<UICommand> commands = null;
            lock (UICommandLock)
            {
                if (UICommands.Count != 0)
                {
                    // コピーしてからList.Clear()を呼ぶぐらいなら参照をすげ替えて、newしたほうが速い。
                    commands = UICommands;
                    UICommands = new List<UICommand>();
                }
            }
            // lockの外側で呼び出さないとdead lockになる。
            if (commands != null)
                foreach (var command in commands)
                    command();
        }

        /// <summary>
        /// 指し手が指されたかのチェックを行う
        /// </summary>
        private void CheckMove()
        {
            // 現状の局面の手番側
            var stm = Position.sideToMove;
            var stmPlayer = Player(stm);

            // 指し手
            var bestMove = stmPlayer.BestMove;

            if (bestMove != Move.NONE)
            {
                PlayTimer(stm).ChageToThemTurn(bestMove == Move.TIME_UP);

                stmPlayer.BestMove = Move.NONE; // クリア

                // 駒が動かせる状況でかつ合法手であるなら、受理する。

                bool specialMove = false;
                if (InTheGame)
                {
                    // 送信されうる特別な指し手であるか？
                    specialMove = bestMove.IsSpecial();

                    // エンジンから送られてきた文字列が、illigal moveであるならエラーとして表示する必要がある。

                    if (specialMove)
                    {
                        switch (bestMove)
                        {
                            // 入玉宣言勝ち
                            case Move.WIN:
                                if (Position.DeclarationWin(EnteringKingRule.POINT27) != Move.WIN)
                                    // 入玉宣言条件を満たしていない入玉宣言
                                    goto ILLEGAL_MOVE;
                                break;

                            // 中断
                            // コンピューター同士の対局の時にも人間判断で中断できなければならないので
                            // 対局中であればこれを無条件で受理する。
                            case Move.INTERRUPT:
                            // 時間切れ
                            // 時間切れになるとBestMoveに自動的にTIME_UPが積まれる。これも無条件で受理する。
                            case Move.TIME_UP:
                                break;

                            // 投了
                            case Move.RESIGN:
                                break; // 手番側の投了は無条件で受理

                            // それ以外
                            default:
                                // それ以外は受理しない
                                goto ILLEGAL_MOVE;
                        }
                    }
                    else if (!Position.IsLegal(bestMove))
                        // 合法手ではない
                        goto ILLEGAL_MOVE;


                    // -- bestMoveを受理して、局面を更新する。

                    kifuManager.Tree.AddNode(bestMove, PlayTimers.GetKifuMoveTimes());

                    if (specialMove)
                    {
                        // 受理できる性質の指し手であることは検証済み
                        // ゲーム終了
                    }
                    else
                    {
                        kifuManager.DoMove(bestMove);
                        // このDoMoveの結果、特殊な局面に至ることはあるが…
                    }
                }

                // -- 次のPlayerに、自分のturnであることを通知してやる。

                if (!specialMove)
                    NotifyTurnChanged();
                else
                    // 特殊な指し手だったので、これにてゲーム終了
                    GameEnd();
            }

            return;

            ILLEGAL_MOVE:
            // これ、棋譜に記録すべき
            Move m = Move.ILLEGAL_MOVE;
            kifuManager.Tree.AddNode(m, PlayTimers.GetKifuMoveTimes());
            kifuManager.Tree.AddNodeComment(m, stmPlayer.BestMove.ToUsi() /* String あとでなおす*/ /* 元のテキスト */);

            GameEnd(); // これにてゲーム終了。
        }

        /// <summary>
        /// 手番側のプレイヤーに自分の手番であることを通知するためにThink()を呼び出す。また、CanMove = trueにする。
        /// 非手番側のプレイヤーに対してCanMove = falseにする。
        /// </summary>
        private void NotifyTurnChanged()
        {
            var stm = Position.sideToMove;
            var stmPlayer = Player(stm);

            var isHuman = stmPlayer.PlayerType == PlayerTypeEnum.Human;

            // 手番が変わった時に特殊な局面に至っていないかのチェック
            if (InTheGame)
            {
                var misc = GameSetting.MiscSettings;

                // -- このDoMoveの結果、千日手や詰み、持将棋など特殊な局面に至ったか？
                Move m = Move.NONE;
                var rep = Position.IsRepetition();

                // 手数による引き分けの局面であるか
                if (misc.MaxMovesToDrawEnable && misc.MaxMovesToDraw < Position.gamePly)
                {
                    m = Move.MAX_MOVES_DRAW;
                }
                // この指し手の結果、詰みの局面に至ったか
                else if (Position.IsMated(moves))
                {
                    m = Move.MATED;
                }
                // 千日手絡みの局面であるか？
                else if (rep != RepetitionState.NONE)
                {
                    // 千日手関係の局面に至ったか
                    switch (rep)
                    {
                        case RepetitionState.DRAW: m = Move.REPETITION_DRAW; break;
                        case RepetitionState.LOSE: m = Move.REPETITION_LOSE; break; // 実際にはこれは起こりえない。
                        case RepetitionState.WIN: m = Move.REPETITION_WIN; break;
                        default: break;
                    }
                }
                // 人間が入玉局面の条件を満たしているなら自動的に入玉局面して勝ちとする。
                // コンピューターの時は、これをやってしまうとコンピューターが入玉宣言の指し手(Move.WIN)をちゃんと指せているかの
                // チェックが出来なくなってしまうので、コンピューターの時はこの処理を行わない。
                else if (isHuman && Position.DeclarationWin(EnteringKingRule.POINT27) == Move.WIN)
                {
                    m = Move.WIN;
                }

                // 上で判定された特殊な指し手であるか？
                if (m != Move.NONE)
                {
                    // この特殊な状況を棋譜に書き出して終了。
                    kifuManager.Tree.AddNode(m, KifuMoveTimes.Zero);

                    InTheGame = false;
                    GameEnd();
                    return;
                }
            }

            // USIエンジンのときだけ、"position"コマンドに渡す形で局面図が必要であるから、
            // 生成して、それをPlayer.Think()の引数として渡してやる。
            var isUsiEngine = stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine;
            string usiPosition = isUsiEngine ? kifuManager.UsiPositionString : null;

            stmPlayer.BestMove = stmPlayer.PonderMove = Move.NONE;
            stmPlayer.CanMove = true;
            stmPlayer.Think(usiPosition);

            // 手番側のプレイヤーの時間消費を開始
            if (InTheGame)
            {
                // InTheGame == trueならば、PlayerTimeSettingは適切に設定されているはず。
                // (対局開始時に初期化するので)

                PlayTimer(stm).ChangeToOurTurn();
            }

            // 非手番側のCanMoveをfalseに

            var nextPlayer = Player(stm.Not());
            nextPlayer.CanMove = false;

            // -- 手番が変わった時の各種propertyの更新

            EngineTurn = stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine;
            CanUserMove = stmPlayer.PlayerType == PlayerTypeEnum.Human && InTheGame;

            // 値が変わっていなくとも変更通知を送りたいので自力でハンドラを呼び出す。
            RaisePropertyChanged("TurnChanged", CanUserMove); // 仮想プロパティ"TurnChanged"
        }

        /// <summary>
        /// 時間チェック
        /// </summary>
        private void CheckTime()
        {
            // エンジンの初期化中であるか。この時は、時間消費は行わない。
            Initializing = Player(Color.BLACK).Initializing || Player(Color.WHITE).Initializing;

            // 双方の残り時間表示の更新
            UpdateTimeString();

            // 時間切れ判定(対局中かつ手番側のみ)
            var stm = Position.sideToMove;
            if (InTheGame && PlayTimer(stm).IsTimeUp())
                Player(stm).BestMove = Move.TIME_UP;
        }

        /// <summary>
        /// 残り時間の更新
        /// </summary>
        /// <param name="c"></param>
        private void UpdateTimeString()
        {
            // 前回と同じ文字列であれば実際は描画ハンドラが呼び出されないので問題ない。
            foreach (var c in All.Colors())
            {
                var ct = PlayTimer(c);
                SetRestTimeString(c, ct.DisplayShortString());
            }
        }

        /// <summary>
        /// ゲームの終了処理
        /// </summary>
        private void GameEnd()
        {
            InTheGame = false;

            // 時間消費、停止
            foreach (var c in All.Colors())
                PlayTimer(c).StopTimer();

            // 棋譜ウィンドウ、勝手に書き換えられると困るのでこれでfixさせておく。
            kifuManager.EnableKifuList = false;

            // 連続対局が設定されている時はDisconnect()はせずに、ここで次の対局のスタートを行う。
            // (エンジンを入れ替えたりしないといけない)

            // 連続対局でないなら..
            Disconnect();
        }

        /// <summary>
        /// エンジンなどの切断処理
        /// </summary>
        private void Disconnect()
        {
            InTheGame = false;

            // Playerの終了処理をしてNullPlayerを突っ込んでおく。
            foreach (var c in All.Colors())
            {
                var player = Player(c);
                if (player != null)
                    player.Dispose();

                Players[(int)c] = new NullPlayer();
            }

            NotifyTurnChanged();
        }

        #endregion
    }
}
