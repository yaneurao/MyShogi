using System.Threading;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.Player;
using MyShogi.Model.Common.ObjectModel;
using System;

namespace MyShogi.Model.LocalServer
{
    /// <summary>
    /// 対局を管理するクラス
    /// 
    /// 内部に局面(Position)を持つ
    /// 内部に棋譜管理クラス(KifuManager)を持つ
    /// 
    /// 思考エンジンへの参照を持つ
    /// プレイヤー入力へのインターフェースを持つ
    /// 時間を管理している。
    /// 
    /// </summary>ない)
    public class LocalGameServer : NotifyObject
    {
        public LocalGameServer()
        {
            kifuManager = new KifuManager();
            Position = kifuManager.Position.Clone(); // immutableでなければならないので、Clone()してセットしておく。

            // 起動時に平手の初期局面が表示されるようにしておく。
            kifuManager.Init();

            KifuList = new List<string>();


#if true
            //GameStart(new HumanPlayer(), new HumanPlayer());

            // デバッグ中 後手をUsiEnginePlayerにしてみる。
            GameStart(new HumanPlayer(), new UsiEnginePlayer());

            //GameStart(new UsiEnginePlayer(), new UsiEnginePlayer());
#endif

            // 対局監視スレッドを起動して回しておく。
            var thread = new Thread(thread_worker);
            thread.Start();
        }

        public void Dispose()
        {
            // スレッドを停止させる。
            workerStop = true;
        }

        // -- public properties

        /// <summary>
        /// 局面。これはimmutableなので、メインウインドウの対局画面にdata bindする。
        /// </summary>
        public Position Position
        {
            get { return GetValue<Position>("Position"); }
            set { SetValue<Position>("Position", value); }
        }

        /// <summary>
        /// 棋譜。これをメインウィンドウの棋譜ウィンドウとdata bindする。
        /// </summary>
        public List<string> KifuList
        {
            get { return GetValue<List<string>>("KifuList"); }
            set { SetValue<List<string>>("KifuList", value); }
        }

        /// <summary>
        /// 対局棋譜管理クラス
        /// </summary>
        public KifuManager kifuManager { get; private set; }

        /// <summary>
        /// ユーザーがUI上で操作できるのか？
        /// ただし、EngineInitializingなら動かしてはならない。
        /// </summary>
        public bool CanUserMove { get; set; }

        /// <summary>
        /// 思考エンジンが考え中であるか。
        /// Engineの手番であればtrue
        /// </summary>
        public bool EngineTurn { get; set; }

        // 仮想プロパティ。Turnが変化した時に"TurnChanged"ハンドラが呼び出される。
        //public bool TurnChanged { }

        /// <summary>
        /// エンジンの初期化中であるか。
        /// </summary>
        public bool EngineInitializing
        {
            get { return GetValue<bool>("EngineInitializing"); }
            set { SetValue<bool>("EngineInitializing", value); }
        }

        /// <summary>
        /// 対局しているプレイヤー
        /// </summary>
        public Player[] Players = new Player[2];

        /// <summary>
        /// c側のプレイヤー
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Player Player(Color c)
        {
            return Players[(int)c];
        }

        // -- public methods

        /// <summary>
        /// 対局スタート
        /// </summary>
        /// <param name="player1">先手プレイヤー(駒落ちのときは下手)</param>
        /// <param name="player2">後手プレイヤー(駒落ちのときは上手)</param>
        public void GameStart(Player player1 , Player player2 /* 引数、あとで考える */)
        {
            // いったんリセット
            GameEnd();

            Initializing = true;
            EngineInitializing = player1.PlayerType == PlayerTypeEnum.UsiEngine || player2.PlayerType == PlayerTypeEnum.UsiEngine;

            Players[0] = player1;
            Players[1] = player2;

            KifuList = new List<string>(kifuManager.KifuList);

            inTheGame = true;

            // エンジンの初期化が終わったタイミングで自動的にNotifyTurnChanged()が呼び出されるはず。
        }

        /// <summary>
        /// ユーザーから指し手が指されたときにUI側から呼び出す。
        /// 
        /// ユーザーがマウス操作によってmの指し手を入力した。
        /// ユーザーはこれを合法手だと思っているが、これが受理されるかどうかは別の話。
        /// (時間切れなどがあるので)
        /// 
        /// 注意 : これを受理するのは、UIスレッドではない。
        /// </summary>
        /// <param name="m"></param>
        public void DoMoveFromUI(Move m)
        {
            var stm = Position.sideToMove;
            var stmPlayer = Player(stm);

            // Human以外であれば受理しない。
            if (stmPlayer.PlayerType == PlayerTypeEnum.Human)
            {
                // これを積んでおけばworker_threadのほうでいずれ処理される。
                stmPlayer.BestMove = m;
            }
        }

        /// <summary>
        /// エンジンに対して、いますぐに指させる。
        /// 受理されるかどうかは別。
        /// </summary>
        public void MoveNow()
        {
            var stm = Position.sideToMove;
            var stmPlayer = Player(stm);

            // エンジン以外であれば受理しない。
            if (stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine)
            {
                var enginePlayer = stmPlayer as UsiEnginePlayer;
                enginePlayer.MoveNow();
            }
        }

        /// <summary>
        /// ユーザーによる対局中の2手戻し
        /// 受理できるかどうかは別
        /// </summary>
        public void UserUndo()
        {
            var stm = Position.sideToMove;
            var stmPlayer = Player(stm);

            // 人間の手番でなければ受理しない
            if (stmPlayer.PlayerType == PlayerTypeEnum.Human)
            {
                // 棋譜を消すUndo()
                kifuManager.UndoMoveInTheGame();
                kifuManager.UndoMoveInTheGame();

                // 盤面に反映
                Position = kifuManager.Position.Clone();

                // 棋譜ウィンドウに反映。
                KifuList = new List<string>(kifuManager.KifuList); // よくわからんから丸ごと反映させておく。

                // これにより、2手目の局面などであれば1手しかundoできずに手番が変わりうるので手番の更新を通知。
                NotifyTurnChanged();
            }

        }

        // -- private members

        /// <summary>
        /// スレッドの終了フラグ。
        /// 終了するときにtrueになる。
        /// </summary>
        private bool workerStop = false;

        /// <summary>
        /// 対局中であるかを示すフラグ。
        /// これがfalseであれば対局中ではないので自由に駒を動かせる。
        /// </summary>
        private bool inTheGame = true;

        /// <summary>
        /// GameStart()のあと、各プレイヤーの初期化中であるか。
        /// </summary>
        private bool Initializing;

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

                // 指し手が指されたかのチェック
                CheckMove();

                // 時間消費のチェック
                CheckTime();

                // 10msごとに各種処理を行う。
                Thread.Sleep(10);
            }
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

            // 今回の思考時間
            var thinkingTime = new TimeSpan(0, 0, 1);

            if (bestMove != Move.NONE)
            {
                stmPlayer.BestMove = Move.NONE; // クリア

                // 駒が動かせる状況でかつ合法手であるなら、受理する。
                if (inTheGame)
                {
                    // 送信されうる特別な指し手であるか？
                    bool specialMove = bestMove.IsSpecial();

                    // エンジンから送られてきた文字列が、illigal moveであるならエラーとして表示する必要がある。

                    if (specialMove)
                    {
                        switch (bestMove)
                        {
                            case Move.WIN:
                                if (Position.DeclarationWin(EnteringKingRule.POINT27) != Move.WIN)
                                    // 入玉宣言条件を満たしていない入玉宣言
                                    goto ILLEGAL_MOVE;
                                break;
                            case Move.RESIGN:
                                break; // 手番側の投了は無条件で受理

                            default:
                                // それ以外は受理しない
                                goto ILLEGAL_MOVE;
                        }
                    }
                    else if (!Position.IsLegal(bestMove))
                        // 合法手ではない
                        goto ILLEGAL_MOVE;


                    // -- bestMoveを受理して、局面を更新する。

                    kifuManager.Tree.AddNode(bestMove, thinkingTime);

                    if (specialMove)
                    {
                        // 受理できる性質の指し手であることは検証済み
                        // ゲーム終了
                        inTheGame = false;
                    }
                    else
                    {
                        kifuManager.Tree.DoMove(bestMove);
                        // このDoMoveの結果、特殊な局面に至ることはあるが…
                    }

                    // -- このクラスのpropertyのPositionを更新する

                    // immutableにするためにClone()してからセットする。
                    // これを更新すると自動的にViewに通知される。

                    Position = kifuManager.Position.Clone();

                    // -- 棋譜が1行追加になったはずなので、それを反映させる。

                    // immutableにするためにClone()してセットしてやり、
                    // 末尾にのみ更新があったことをViewに通知する。

                    var kifuList = new List<string>(kifuManager.KifuList);
                    SetValue("KifuList", kifuList, kifuList.Count - 1); // 末尾が変更になったことを通知

                }

                // -- 次のPlayerに、自分のturnであることを通知してやる。

                if (inTheGame)
                    NotifyTurnChanged();
                else
                    GameEnd();
            }

            return;

        ILLEGAL_MOVE:
            // これ、棋譜に記録すべき
            Move m = Move.ILLEGAL;
            kifuManager.Tree.AddNode(m, thinkingTime);
            kifuManager.Tree.AddNodeComment(m , stmPlayer.BestMove.ToUsi() /* String あとでなおす*/ /* 元のテキスト */);

            var kifuList2 = new List<string>(kifuManager.KifuList);
            SetValue("KifuList", kifuList2, kifuList2.Count - 1); // 末尾が変更になったことを通知

        }

        /// <summary>
        /// 手番側のプレイヤーに自分の手番であることを通知するためにThink()を呼び出す。また、CanMove = trueにする。
        /// 非手番側のプレイヤーに対してCanMove = falseにする。
        /// </summary>
        private void NotifyTurnChanged()
        {
            var stm = Position.sideToMove;
            var stmPlayer = Player(stm);

            // 手番が変わった時に特殊な局面に至っていないかのチェック

            // -- このDoMoveの結果、千日手や詰み、持将棋など特殊な局面に至ったか？
            Move m = Move.NONE;
            var rep = Position.IsRepetition();

            // この指し手の結果、詰みの局面に至ったか
            if (Position.IsMated(moves))
                m = Move.MATED;
            else if (rep != RepetitionState.NONE)
            {
                // 千日手関係の局面に至ったか
                switch (rep)
                {
                    case RepetitionState.DRAW: m = Move.REPETITION_DRAW; break;
                    case RepetitionState.LOSE: m = Move.REPETITION_LOSE; break;
                    case RepetitionState.WIN : m = Move.REPETITION_WIN; break;
                    default: break;
                }
            }
            if (m != Move.NONE)
            {
                // この特殊な状況を棋譜に書き出して終了。
                kifuManager.Tree.AddNode(m, TimeSpan.Zero);
                inTheGame = false;
                GameEnd();
                return;
            }

            // USIエンジンのときだけ、"position"コマンドに渡す形で局面図が必要であるから、
            // 生成して、それをPlayer.Think()の引数として渡してやる。
            var isUsiEngine = stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine;
            string usiPosition = isUsiEngine ? kifuManager.UsiPositionString : null;

            stmPlayer.BestMove = stmPlayer.PonderMove = Move.NONE;
            stmPlayer.CanMove = true;
            stmPlayer.Think(usiPosition);

            // 非手番側のCanMoveをfalseに

            var nextPlayer = Player(stm.Not());
            nextPlayer.CanMove = false;

            // -- 手番が変わった時の各種propertyの更新

            EngineTurn = stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine;
            CanUserMove = stmPlayer.PlayerType == PlayerTypeEnum.Human;

            // 値が変わっていなくとも変更通知を送りたいので自力でハンドラを呼び出す。
            RaisePropertyChanged("TurnChanged", CanUserMove); // 仮想プロパティ"TurnChanged"
        }

        /// <summary>
        /// 時間チェック
        /// </summary>
        private void CheckTime()
        {
            // エンジンの初期化中であるか。この時は、時間消費は行わない。
            bool initializing = Player(Color.BLACK).Initializing || Player(Color.WHITE).Initializing;

            if (Initializing && !initializing && inTheGame)
            {
                // エンジンの初期化終了したはず
                EngineInitializing = false;

                // 両方の対局準備ができたので対局スタート
                NotifyTurnChanged();
            }

            Initializing = initializing;
        }

        /// <summary>
        /// ゲームの終了処理
        /// </summary>
        private void GameEnd()
        {
            // Playerの終了処理をしてNullPlayerを突っ込んでおく。
            for(var c = Color.ZERO; c < Color.NB; ++c)
            {
                var player = Player(c);
                if (player != null)
                    player.Dispose();

                Players[(int)c] = new NullPlayer(); 
            }

            EngineTurn = false;
            CanUserMove = false;
            RaisePropertyChanged("TurnChanged", CanUserMove); // 仮想プロパティ"TurnChanged"
        }

        /// <summary>
        /// 詰みの判定のために用いる指し手生成バッファ
        /// </summary>
        private Move[] moves = new Move[(int)Move.MAX_MOVES];
    }

}
