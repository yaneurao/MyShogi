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
        /// 対局開始のためにGameSettingの設定に従い、ゲームを初期化する。
        /// </summary>
        /// <param name="gameSetting"></param>
        private void GameStart(GameSetting gameSetting)
        {
            // 初期化中である。
            Initializing = true;

            // プレイヤーの生成
            foreach (var c in All.Colors())
            {
                var playerType = gameSetting.Player(c).IsHuman ? PlayerTypeEnum.Human : PlayerTypeEnum.UsiEngine;
                Players[(int)c] = PlayerBuilder.Create(playerType);
            }

            // 局面の設定
            kifuManager.EnableKifuList = true;
            if (gameSetting.Board.BoardTypeCurrent)
            {
                // 現在の局面からなので、いま以降の局面を削除する。
                // ただし、いまの局面と棋譜ウィンドウとが同期しているとは限らない。
                // まず現在局面以降の棋譜を削除しなくてはならない。

                // 元nodeが、special moveであるなら、それを削除しておく。
                if (kifuManager.Tree.IsLastMoveSpecialMove())
                    kifuManager.Tree.UndoMove();

                kifuManager.Tree.ClearForward();
            }
            else // if (gameSetting.Board.BordTypeEnable)
            {
                kifuManager.Init();
                kifuManager.InitBoard(gameSetting.Board.BoardType);
            }

            // 現在の時間設定を、KifuManager.Treeに反映させておく(棋譜保存時にこれが書き出される)
            kifuManager.Tree.KifuTimeSettings = gameSetting.KifuTimeSettings;

            // 対局者氏名の設定
            // 人間の時のみ有効。エンジンの時は、エンジン設定などから取得することにする。(TODO:あとで考える)
            foreach (var c in All.Colors())
            {
                var player = Player(c);
                string name;
                switch (player.PlayerType)
                {
                    case PlayerTypeEnum.Human:
                        name = gameSetting.Player(c).PlayerName;
                        break;

                    default:
                        name = c.Pretty();
                        break;
                }

                kifuManager.KifuHeader.SetPlayerName(c, name);
            }

            // 持ち時間などの設定が必要なので、コピーしておく。
            GameSetting = gameSetting;

            // 消費時間計算用
            foreach (var c in All.Colors())
            {
                var pc = PlayTimer(c);
                pc.KifuTimeSetting = GameSetting.KifuTimeSettings.Player(c);
                pc.GameStart();
            }

            // rootの持ち時間設定をここに反映させておかないと待ったでrootまで持ち時間が戻せない。
            // 途中の局面からだとここではなく、現局面のところに書き出す必要がある。
            kifuManager.Tree.SetKifuMoveTimes(PlayTimers.GetKifuMoveTimes());

            // コンピュータ vs 人間である場合、人間側を手前にしてやる。
            foreach (var c in All.Colors())
                if (gameSetting.Player(c).IsHuman && gameSetting.Player(c.Not()).IsCpu)
                    BoardReverse = (c == Color.WHITE);

            InTheGame = true;
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

                    // 受理できる性質の指し手であることは検証済み
                    // special moveであってもDoMove()してしまう。
                    kifuManager.DoMove(bestMove);
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
            kifuManager.Tree.DoMove(m);

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
                    // speical moveでもDoMoveできることは保証されている。
                    kifuManager.Tree.DoMove(m);

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
