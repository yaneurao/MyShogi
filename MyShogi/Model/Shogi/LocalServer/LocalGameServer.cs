using System;
using System.Threading;
using System.Collections.Generic;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.Player;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局を管理するクラス
    /// 
    /// ・内部に棋譜管理クラス(KifuManager)を持つ
    /// ・思考エンジンへの参照を持つ
    /// ・プレイヤーからの入力を受け付けるコマンドインターフェースを持つ
    /// ・対局時間を管理している。
    /// 
    /// MainDialogにとってのViewModelの一部に相当すると考えられるが、MainDialogとは1:Nで対応するため、
    /// MainDialogViewModelとは切り離してある。
    /// 
    /// </summary>
    public class LocalGameServer : NotifyObject
    {
        #region 設計のガイドライン

        /*
         * 設計的には、このクラスのpropertyは、immutable objectになるようにClone()されたものをセットする。
         * このpropertyに対して、NotifyObjectの通知の仕組みを用いて、UI側はイベントを捕捉する。
         * UI側は、このpropertyを見ながら、画面を描画する。immutable objectなのでスレッド競合の問題は起きない。
         * 
         * UIからのコマンドを受け付けるのは、対局を監視するworker threadを一つ回してあり、このスレッドがコマンドを受理する。
         * このスレッドは1つだけであり、局面をDoMove()で進めるのも、このスレッドのみである。
         * ゆえに局面を進めている最中に他のスレッドがコマンドを受理してしまう、みたいなことは起こりえないし、
         * 対局時間が過ぎて、対局が終了しているのに、ユーザーからの指し手コマンドを受理してしまうというようなことも起こりえない。
         */

        public LocalGameServer()
        {
            // Position,KifuListは、kifuManagerから半自動でデータバインドする。
            // (それぞれがimmutable objectではないため、Clone()が必要になるので)

            kifuManager.Tree.AddPropertyChangedHandler("Position", PositionUpdate);
            kifuManager.Tree.AddPropertyChangedHandler("KifuList", KifuListUpdate);

            // 起動時に平手の初期局面が表示されるようにしておく。
            kifuManager.Init();

            // ゲームの対局設定。GameStart()を呼び出すまでdefaultで何かを埋めておかなくてはならない。
            // 前回の対局時のものを描画するのもおかしいので、defaultのものを設定しておく。
            GameSetting = new GameSetting();

            // 対局監視スレッドを起動して回しておく。
            new Thread(thread_worker).Start();
        }

        public void Dispose()
        {
            // 対局中であればエンジンなどを終了させる。
            Disconnect();

            // 対局監視用のworker threadを停止させる。
            workerStop = true;
        }

        #endregion

        #region public properties
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
        /// 対局中であるかを示すフラグ。
        /// これがfalseであれば対局中ではないので自由に駒を動かせる。(ようにするかも)
        /// </summary>
        public bool InTheGame
        {
            get { return GetValue<bool>("InTheGame"); }
            private set { SetValue<bool>("InTheGame",value); }
        }

        /// <summary>
        /// ユーザーがUI上で操作できるのか？
        /// ただし、EngineInitializingなら動かしてはならない。
        /// </summary>
        public bool CanUserMove { get; private set; }

        /// <summary>
        /// 思考エンジンが考え中であるか。
        /// Engineの手番であればtrue
        /// </summary>
        public bool EngineTurn { get; private set; }

        // 仮想プロパティ。Turnが変化した時に"TurnChanged"ハンドラが呼び出される。
        //public bool TurnChanged { }

        /// <summary>
        /// エンジンの初期化中であるか。
        /// </summary>
        public bool EngineInitializing
        {
            get { return GetValue<bool>("EngineInitializing"); }
            private set { SetValue<bool>("EngineInitializing", value); }
        }

        /// <summary>
        /// 画面上に表示する名前を取得する。
        /// 文字数制限はないので注意。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string DisplayName(Color c)
        {
            return kifuManager.GetPlayerName(c);
        }

        /// <summary>
        /// 画面上に表示する短い名前を取得する。
        /// 先頭の8文字だけ返ってくる。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string ShortDisplayName(Color c)
        {
            var name = DisplayName(c);
            return name.PadLeft(8); // 最大で8文字まで
        }

        /// <summary>
        /// c側のプレイヤー
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Player.Player Player(Color c)
        {
            return Players[(int)c];
        }

        /// <summary>
        /// 対局設定
        /// 
        /// GameStart()で渡された設定。immutableであるものとする。(呼び出し側で勝手に値を変更しないものとする)
        /// </summary>
        public GameSetting GameSetting
        {
            get { return GetValue<GameSetting>("GameSetting"); }
            set { SetValue<GameSetting>("GameSetting", value); }
        }

        /// <summary>
        /// c側の持ち時間設定の文字列
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string TimeSettingString(Color c)
        {
            return GameSetting.TimeSettings.Player(c).ToShortString();
        }

        /// <summary>
        /// プレイヤーの消費時間を計測する用
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public PlayTimer PlayTimer(Color c)
        {
            return PlayTimers.Player(c);
        }

        /// <summary>
        /// 残り時間
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string RestTimeString(Color c)
        {
            return restTimeStrings[(int)c];
        }

        // 仮想プロパティ
        // 残り消費時間が変更になった時に呼び出される。
        //public bool RestTimeChanged { get; set; }

        /// <summary>
        /// 盤面反転
        /// Viewごとに変更できるので、このクラスが保持している。
        /// </summary>
        public bool BoardReverse
        {
            get { return GetValue<bool>("BoardReverse"); }
            set { SetValue<bool>("BoardReverse", value); }
        }

        #endregion

        #region UI側からのコマンド

        /*
         * UI側からのコマンドは、 delegateで渡され、対局監視スレッド側で実行される。
         * delegateのなかのkifuManager.PositionやkifuManager.KifuListは、無名関数の束縛の性質から、
         * 現在のものであって過去のPositionやKifuListのへ参照ではない。
         * 
         * また、処理するのは対局監視スレッドであるから(対局監視スレッドはシングルスレッドでかつ、対局監視スレッド側でしか
         * Position.DoMove()は行わないので)、これが処理されるタイミングでは、kifuManager.Positionは最新のPositionであり、
         * これを調べているときに他のスレッドが勝手にPosition.DoMove()を行ったり、他のコマンドを受け付けたり、持ち時間切れに
         * なったりすることはない。
         */

        /// <summary>
        /// 対局スタート
        /// </summary>
        public void GameStartCommand(GameSetting gameSetting)
        {
            AddCommand(
            () =>
            {
                // いったんリセット
                GameEnd();
                GameStart(gameSetting);

                // エンジンの初期化が終わったタイミングで自動的にNotifyTurnChanged()が呼び出される。
            });
        }

        /// <summary>
        /// ユーザーから指し手が指されたときにUI側から呼び出す。
        /// 
        /// ユーザーがマウス操作によってmの指し手を入力した。
        /// ユーザーはこれを合法手だと思っているが、これが受理されるかどうかは別の話。
        /// (時間切れなどがあるので)
        /// </summary>
        /// <param name="m"></param>
        public void DoMoveCommand(Move m)
        {
            AddCommand(
            () =>
            {
                var stm = kifuManager.Position.sideToMove;
                var stmPlayer = Player(stm);

                // Human以外であれば受理しない。
                if (stmPlayer.PlayerType == PlayerTypeEnum.Human)
                {
                    // これを積んでおけばworker_threadのほうでいずれ処理される。(かも)
                    // 仮に、すでに次の局面になっていたとしても、次にこのユーザーの手番になったときに
                    // BestMove = Move.NONEとされるのでその時に破棄される。
                    stmPlayer.BestMove = m;
                }
            }
            );
        }

        /// <summary>
        /// エンジンに対して、いますぐに指させる。
        /// 受理されるかどうかは別。
        /// </summary>
        public void MoveNowCommand()
        {
            AddCommand(
            () =>
            {
                var stm = kifuManager.Position.sideToMove;
                var stmPlayer = Player(stm);

                // エンジン以外であれば受理しない。
                if (stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine)
                {
                    var enginePlayer = stmPlayer as UsiEnginePlayer;
                    enginePlayer.MoveNow();
                }
            });
        }

        /// <summary>
        /// ユーザーによる対局中の2手戻し
        /// 受理できるかどうかは別
        /// </summary>
        public void UndoCommand()
        {
            AddCommand(
            () =>
            {
                var stm = kifuManager.Position.sideToMove;
                var stmPlayer = Player(stm);

                // 人間の手番でなければ受理しない
                if (stmPlayer.PlayerType == PlayerTypeEnum.Human)
                {
                    // 棋譜を消すUndo()
                    kifuManager.UndoMoveInTheGame();
                    kifuManager.UndoMoveInTheGame();

                    // 時刻を巻き戻さないといけない
                    PlayTimers.SetKifuMoveTimes(kifuManager.Tree.GetKifuMoveTime());

                    // これにより、2手目の局面などであれば1手しかundoできずに手番が変わりうるので手番の更新を通知。
                    NotifyTurnChanged();
                }
            });
        }

        /// <summary>
        /// UI側からの中断要求。
        /// </summary>
        public void GameInterruptCommand()
        {
            AddCommand(
            () =>
            {
                // コンピューター同士の対局中であっても人間判断で中断できなければならないので常に受理する。
                var stm = kifuManager.Position.sideToMove;
                var stmPlayer = Player(stm);

                // 中断の指し手
                stmPlayer.BestMove = Move.INTERRUPT;
            });
        }

        /// <summary>
        /// 棋譜の選択行が変更になった。
        /// 対局中でなければ、現在局面をその棋譜の局面に変更する。
        /// </summary>
        public void KifuSelectedIndexChangedCommand(int selectedIndex)
        {
            AddCommand(
            () =>
            {
                if (!InTheGame)
                {
                    // 現在の局面と違う行であるかを判定して、同じ行である場合は、
                    // このイベントを処理してはならない。

                    // 無理やりではあるが棋譜のN行目に移動出来るのであった…。
                    kifuManager.Tree.GotoSelectedIndex(selectedIndex);

                }
            });
        }

        #endregion

        #region 依存性のあるプロパティの処理

        /// <summary>
        /// EngineInitializingはInitializingとPlayer()に依存するので、
        /// どちらかに変更があったときにそれらのsetterで、このUpdateEngineInitializing()を呼び出してもらい、
        /// このなかでEngineInitializingのsetterを呼び出して、その結果、"EngineInitializing"のイベントハンドラが起動する。
        /// </summary>
        private void UpdateEngineInitializing()
        {
            EngineInitializing = Initializing &&
            (EngineInitializing = Player(Color.BLACK).PlayerType == PlayerTypeEnum.UsiEngine || Player(Color.WHITE).PlayerType == PlayerTypeEnum.UsiEngine);
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

                kifuManager.Tree.ClearForward();

            }
            else // if (gameSetting.Board.BordTypeEnable)
            {
                kifuManager.Init();
                kifuManager.InitBoard(gameSetting.Board.BoardType);
            }

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

                kifuManager.SetPlayerName(c, name);
            }

            // 持ち時間などの設定が必要なので、コピーしておく。
            GameSetting = gameSetting;

            // 消費時間計算用
            foreach (var c in All.Colors())
            {
                var pc = PlayTimer(c);
                pc.TimeSetting = GameSetting.TimeSettings.Player(c);
                pc.GameStart();
            }
            // rootの持ち時間設定をここに反映させておかないと待ったでrootまで持ち時間が戻せない。
            kifuManager.Tree.RootKifuMoveTimes = PlayTimers.GetKifuMoveTimes();

            // コンピュータ vs 人間である場合、人間側を手前にしてやる。
            foreach (var c in All.Colors())
                if (gameSetting.Player(c).IsHuman && gameSetting.Player(c.Not()).IsCpu)
                    BoardReverse = (c == Color.WHITE);

            InTheGame = true;
        }

        /// <summary>
        /// GameStart()のあと、各プレイヤーの初期化中であるか。
        /// </summary>
        private bool Initializing
        {
            get { return initializing; }
            set {
                if (initializing && !value)
                {
                    // 状態がtrueからfalseに変わった
                    // 両方の対局準備ができたということなので対局スタート
                    NotifyTurnChanged();
                }
                initializing = value;

                // このプロパティに依存しているプロパティの更新
                UpdateEngineInitializing();
            }
        }
        private bool initializing;

        /// <summary>
        /// Positionプロパティの更新。
        /// immutableにするためにCloneしてセットする。
        /// 自動的にViewに通知される。
        /// 
        /// ※　KifuTreeのほうでPositionが更新された時に通知が来る。
        /// </summary>
        private void PositionUpdate(PropertyChangedEventArgs args)
        {
            // immutableでなければならないので、Clone()してセットしておく。
            // セットした時に、このクラスのNotifyObjectによる通知がなされる。

            Position = kifuManager.Position.Clone();
        }

        /// <summary>
        /// KifuListの末尾のみが更新があったことがわかっているときに呼び出す更新。
        /// immutableにするためにCloneしてセットする。
        /// 全行が丸ごと更新通知が送られるので部分のみの更新通知を送りたいなら自前で更新すべし。
        /// 
        /// ※　KifuTreeのほうでPositionが更新された時に通知が来る。
        /// </summary>
        private void KifuListUpdate(PropertyChangedEventArgs args)
        {
            // Cloneしたものをセットする。
            args.value = new List<string>(kifuManager.KifuList);

            // このクラスのNotifyObjectによる通知がなされる。
            SetValue<List<string>>(args);
        }

        #endregion

        #region private members

        /// <summary>
        /// 対局棋譜管理クラス
        /// </summary>
        private KifuManager kifuManager { get; set; } = new KifuManager();

        /// <summary>
        /// 対局しているプレイヤー
        /// 設定するときはSetPlayer()を用いるべし。
        /// 取得するときはPlayer()のほうを用いるべし。
        /// </summary>
        private Player.Player[] Players = new Player.Player[2] { new NullPlayer(), new NullPlayer() };

        /// <summary>
        /// プレイヤーの消費時間管理クラス
        /// </summary>
        private PlayTimers PlayTimers = new PlayTimers();
        
        /// <summary>
        /// 残り時間を表現する文字列
        /// </summary>
        private string[] restTimeStrings = new string[2];
        private void SetRestTimeString(Color c , string time)
        {
            var changed = restTimeStrings[(int)c] != time;
            restTimeStrings[(int)c] = time;
            if (changed)
                RaisePropertyChanged("RestTimeChanged",c);
        }

        /// <summary>
        /// スレッドの終了フラグ。
        /// 終了するときにtrueになる。
        /// </summary>
        private bool workerStop = false;

        /// <summary>
        /// UIから渡されるコマンド
        /// </summary>
        private delegate void UICommand();
        private List<UICommand> UICommands = new List<UICommand>();
        // commandsのlock用
        private object UICommandLock = new object();

        /// <summary>
        /// 詰みの判定のために用いる指し手生成バッファ
        /// </summary>
        private Move[] moves = new Move[(int)Move.MAX_MOVES];

        #endregion

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
        /// UI側から、worker threadで実行して欲しいコマンドを渡す。
        /// View-ViewModelアーキテクチャにおいてViewからViewModelにcommandを渡す感じ。
        /// ここで渡されたコマンドは、CheckUICommand()で吸い出されて実行される。
        /// </summary>
        /// <param name="command"></param>
        private void AddCommand(UICommand command)
        {
            lock (UICommandLock)
            {
                UICommands.Add(command);
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
                PlayTimer(stm).ChageToThemTurn();

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
            kifuManager.Tree.AddNodeComment(m , stmPlayer.BestMove.ToUsi() /* String あとでなおす*/ /* 元のテキスト */);

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

            if (InTheGame)
            {
                // 両方の残り時間を更新しておく。
                // 前回と同じ文字列であれば実際は描画ハンドラが呼び出されないので問題ない。
                foreach (var c in All.Colors())
                {
                    var ct = PlayTimer(c);
                    // 時間切れ判定は手番側のみ
                    if (c == Position.sideToMove && ct.IsTimeUp())
                        Player(c).BestMove = Move.TIME_UP;

                    SetRestTimeString(c, ct.DisplayShortString());
                }
            }
        }

        private void UpdateTimeString(Color c)
        {

        }

        /// <summary>
        /// ゲームの終了処理
        /// </summary>
        private void GameEnd()
        {
            InTheGame = false;

            // 時間消費、停止
            foreach(var c in All.Colors())
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
