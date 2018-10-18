using System;
using System.Collections.Generic;
using MyShogi.App;
using MyShogi.Model.Common.Collections;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.Player;
using MyShogi.Model.Shogi.Usi;

namespace MyShogi.Model.Shogi.LocalServer
{
    public partial class LocalGameServer
    {
        #region public properties

        /// <summary>
        /// 局面。これはimmutableであることが保証されているので、メインウインドウの対局画面にdata bindする。
        /// </summary>
        public Position Position
        {
            get { return GetValue<Position>("Position"); }
            set { SetValue("Position", value); }
        }

        /// <summary>
        /// 棋譜。これをメインウィンドウの棋譜ウィンドウとdata bindする。
        /// </summary>
        public List<KifuListRow> KifuList
        {
            get { return GetValue<List<KifuListRow>>("KifuList"); }
            set { SetValue("KifuList", value); }
        }

        // 仮想プロパティ。棋譜が1行追加/削除された時に発生するイベント。

        //public string KifuListAdded { }
        //public void KifuListRemoved { }

        /// <summary>
        /// 対局中であるかなどを示すフラグ。
        /// </summary>
        public GameModeEnum GameMode
        {
            get { return GetValue<GameModeEnum>("GameMode"); }

            // [Worker Thread] : このsetterはworker thread側からしかsetterは呼び出されない。
            private set {
                var old = GetValue<GameModeEnum>("GameMode");
                var next = value;
                if (old == next)
                    return; // 値が同じなので何もしない

                // 次のモードがエンジンを使った検討モードであるなら局面の合法性のチェックが必要。

                if (value.IsConsiderationWithEngine())
                {
                    // 現在の局面が不正でないかをチェック。
                    var error = Position.IsValid(next == GameModeEnum.ConsiderationWithMateEngine);
                    if (error != null)
                    {
                        TheApp.app.MessageShow(error, MessageShowType.Error);
                        return;
                    }
                }

                // エンジンを用いた検討モードを抜ける or 入るのであれば、そのコマンドを叩く。

                if (old.IsConsiderationWithEngine())
                    EndConsideration();
                if (value.IsConsiderationWithEngine())
                {
                    var success = StartConsiderationWithEngine(value /* 次のgameMode */);
                    if (!success)
                        return;
                }

                // 次のモードに移行できることが確定したので値を変更する。
                SetValue<GameModeEnum>("GameMode", next);

                // 依存プロパティの更新
                SetValue<bool>("InTheGame", next == GameModeEnum.InTheGame);
                SetValue<bool>("InTheBoardEdit", next == GameModeEnum.InTheBoardEdit);
            }
        }

        /// <summary>
        /// 対局中であるかを返す。これは、GameModeに依存している依存プロパティ。
        /// </summary>
        public bool InTheGame
        {
            get { return GetValue<bool>("InTheGame"); }
        }

        /// <summary>
        /// 盤面編集中であるかを返す。これは、GameModeに依存している依存プロパティ。
        /// </summary>
        public bool InTheBoardEdit
        {
            get { return GetValue<bool>("InTheBoardEdit"); }
        }

        /// <summary>
        /// ユーザーがUI上で操作できるのか？
        /// ただし、EngineInitializingなら動かしてはならない。
        /// 
        /// また、EnableUserMoveがfalseにされているとこのCanUserMoveはtrueにならない。
        /// </summary>
        public bool CanUserMove
        {
            get { return canUserMove && EnableUserMove; }
            private set { canUserMove = value; }
        }
        private bool canUserMove; // ↑からしかアクセスしない

        /// <summary>
        /// ユーザーの操作を受け付けるのか。
        /// これをfalseにすると、盤面が閲覧専用になる。
        /// (CanUserMoveがtrueを返さなくなる。)
        /// </summary>
        public bool EnableUserMove { get; set; } = true;

        /// <summary>
        /// 思考エンジンが考え中であるか。
        /// Engineの手番であればtrue
        /// </summary>
        public bool EngineTurn { get; private set; }

        // 仮想プロパティ。Turnが変化した時に"TurnChanged"ハンドラが呼び出される。
        public bool TurnChanged;

        /// <summary>
        /// エンジンの初期化中であるか。
        /// 
        /// これは、依存プロパティで、UpdateInitializing()から更新される。
        /// </summary>
        public bool EngineInitializing
        {
            get { return GetValue<bool>("EngineInitializing"); }
        }

        /// <summary>
        /// 画面上に表示する名前を取得する。
        /// 文字数制限はないので注意。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string DisplayName(Color c)
        {
            // 棋譜上の名前
            //return kifuManager.KifuHeader.GetPlayerName(c);

            // 対局ダイアログの設定を活かす
            //return GameSetting.PlayerSetting(c).PlayerName;

            // →　GameStart()時の名前
            // (対局終了時にGameSettingをGameStart()時のものに復元するので、
            // そのときに連続対局や振り駒のためにプレイヤーを入れ替えていると困るため)
            return continuousGame.DisplayName(c);

        }

        /// <summary>
        /// プレイヤー名を設定する。
        ///
        /// このメソッドは、対局開始前に( GameStart(),GameRestart()から)呼び出される。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="name"></param>
        public void SetPlayerName(Color c , string name)
        {
            // 1) kifuManager.KifuHeader     : View-ModelのModel(これがmaster)
            // 2) continuousGame.DisplayName : ↑のcache
            // と考えられる。

            // KifuHeader.SetPlayerName()はこのメソッド以外からは呼び出さない。
            // この名前の変更は棋譜の保存の時に必要となる。
            kifuManager.KifuHeader.SetPlayerName(c, name);

            // これはDisplayName()に反映される。
            continuousGame.SetDisplayName(c, name);
        }

        /// <summary>
        /// 思考エンジンのpreset名も含めた名前を返す。
        /// 文字数制限はない。
        ///
        /// 棋譜ファイルのファイル名や対局結果一覧に用いるのは良いが、棋譜ファイルの対局者名を
        /// これにしてしまうと読み込んだときに対局設定ダイアログの対局者名が「tanuki- 2018(二段)」のように
        /// なってしまうのでまずい。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string DisplayNameWithPreset(Color c)
        {
            var name = DisplayName(c);
            // CPUのときはPreset名も取得する。
            var preset = continuousGame.PresetName(c);
            if (preset != null)
            name += $"({preset})";
            return name;
        }

        /// <summary>
        /// 棋譜をファイルに名前をつけて保存するときのデフォルトファイル名。
        /// 対局者名 + 手合割 + 対局結果 + 日付
        /// 
        /// 拡張子は含まず。
        /// </summary>
        /// <returns></returns>
        public string DefaultKifuFileName()
        {
            // 手合割
            var board_type = GameSetting.BoardSetting.BoardType;
            var board_type_string = board_type == BoardType.Current ? null /* 任意局面。これはファイル名に含めない */ : $"_{board_type.Pretty()}";

            // 対局の結果
            var game_result_string = continuousGame == null ? null : $"_{continuousGame.GetGameResultStringForLastGame()}";

            var name = $"{DisplayNameWithPreset(Color.BLACK)}_{DisplayNameWithPreset(Color.WHITE)}{board_type_string}{game_result_string}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            // プレイヤー名としてファイルに使えない文字列が含まれている可能性があるのでここでescapeする。
            name = Utility.EscapeFileName(name);
            return name;
        }

        /// <summary>
        /// 画面上に表示する短い名前を取得する。
        /// 先頭の16文字(半角換算)だけ返ってくる。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string ShortDisplayName(Color c)
        {
            var name = DisplayName(c);
            return name.LeftUnicode(16); // 半角換算で16文字まで
        }

        /// <summary>
        /// ShortDisplayName()と同等だが、
        /// 先頭に手番文字列が付与された形で表示名を返す。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string ShortDisplayNameWithTurn(Color c)
        {
            string turn = null; ;
            switch(TheApp.app.Config.DisplayNameTurnVersion)
            {
                case 0: break;
                case 1: turn = c == Color.BLACK ? "☗" : "☖"; break;
                case 2: turn = c == Color.BLACK ? "▲" : "△"; break;
                
                // 他のものは考え中
                default: break;
            }

            var name = $"{turn}{DisplayName(c)}";
            return name.LeftUnicode(16); // 半角換算で16文字まで
        }

        /// <summary>
        /// c側のプレイヤー
        /// 
        /// エンジンを用いた検討モードの時、
        /// Player(0)は検討用のエンジン/詰将棋用のエンジン
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
        /// GameStart()で渡された設定。
        /// そこ以外では勝手に値をsetしないものとする。
        /// </summary>
        public GameSetting GameSetting
        {
            get { return GetValue<GameSetting>("GameSetting"); }
            private set { SetValue<GameSetting>("GameSetting", value); }
        }

        /// <summary>
        /// c側の持ち時間設定の文字列
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string TimeSettingString(Color c)
        {
            // 対局中に毎回文字列を生成するの馬鹿らしいのでGameStart()で初期化をする。
            // return PlayTimer(c).KifuTimeSetting.ToShortString();
            return timeSettingStrings[(int)c];
        }
        private string[] timeSettingStrings = new string[2];

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
        //public bool RestTimeChanged { get; }

        /// <summary>
        /// 棋譜読み込み時など、こちら側の要請により、棋譜ウィンドウを指定行に移動させる
        /// この値をdatabindによって棋譜ControlのViewModel.KifuListSelectedIndexに紐づけておくこと。
        ///
        /// 必ず変更通知イベントを発生させたいなら、このsetterを用いずに、UpdateKifuSelectedIndex()のほうを用いること。
        /// </summary>
        public int KifuListSelectedIndex
        {
            set { SetValue<int>("KifuListSelectedIndex", value); }
        }

        /// <summary>
        /// 盤面反転
        /// Viewごとに変更できるので、このクラスが保持している。
        /// </summary>
        public bool BoardReverse
        {
            get { return GetValue<bool>("BoardReverse"); }
            set { SetValue<bool>("BoardReverse", value); }
        }

        /// <summary>
        /// Start()でworker threadを作らない。
        /// CPU対戦をせずに単に盤面だけ描画したい場合はworkerは不要なのでこれをtrueにしてStart()を呼び出すと良い。
        /// </summary>
        public bool NoThread { get; set; } = false;

        /// <summary>
        /// エンジンの読み筋などを検討用のダイアログに出力する。
        /// 
        /// デフォルトtrue。
        /// これをtrueにすると、要所要所でこのクラスのpropertyであるThinkReportのsetterが呼び出されるので、
        /// 必要ならば外部から変更イベントを捕捉すれば良い。
        /// </summary>
        public bool ThinkReportEnable { get; set; } = true;

        /// <summary>
        /// ThinkReportEnableがtrueの時に、エンジンの読み筋などを出力するためのハンドラ。
        /// </summary>
        public UsiThinkReportMessage ThinkReport
        {
            get { return GetValue<UsiThinkReportMessage>("ThinkReport"); }
            private set { SetValue<UsiThinkReportMessage>("ThinkReport", value); }
        }

        /// <summary>
        /// エンジンのプリセット名 + 手合割(上手側のみ)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string PresetWithBoardTypeString(Color c)
        {
            // エンジンのプリセット名
            var presetString = continuousGame.PresetName(c);

            // 手合割
            var boardTypeString = BoardTypeString(c);

            // 段位プリセット + 手合割(もしあれば)
            return $"{presetString}{boardTypeString}";
        }

        /// <summary>
        /// 手合割(上手側のみ)を文字列化して返す。
        /// 下手側ならばnullが返る。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string BoardTypeString(Color c)
        {
            string boardTypeString = null;
            if (c == Color.WHITE /*上手側*/)
            {
                var boardType = kifuManager.Tree.rootBoardType;
                if (boardType != BoardType.NoHandicap && boardType.IsSfenOk())
                    boardTypeString = boardType.Pretty();
            }
            return boardTypeString;
        }

        /// <summary>
        /// 最後に棋譜を保存してから棋譜が更新されたかのフラグ
        /// </summary>
        public bool KifuDirty
        {
            get { return kifuManager.Tree.KifuDirty; }
            set { kifuManager.Tree.KifuDirty = value; }
        }

        /// <summary>
        /// 秒読みの読み上げたをどこまでチェックしたか。
        /// </summary>
        public int LastCheckedByoyomiReadOut;

        /// <summary>
        /// ゲームが開始された時に飛んでくる仮想イベント。
        /// 対局開始のAnimatorを表示するなどすると良いと思う。
        /// </summary>
        public object GameStartEvent
        {
            get { return GetValue<object>("GameStartEvent"); }
        }

        /// <summary>
        /// 終局時に飛んでくる仮想イベント。
        /// 対局終了のAnimatorを表示すると良いと思う。
        ///
        /// この値は、人間 vs CPUのとき、人間側から見て、どちらが勝利したかを表現している。
        /// 人間 vs 人間のときや、CPU vs CPUのときは、MoveGameResult.Unknownが入っている。
        /// </summary>
        public MoveGameResult GameEndEvent
        {
            get { return GetValue<MoveGameResult>("GameEndEvent"); }
        }

        /// <summary>
        /// 各PlayerのEngineDefine
        /// </summary>
        public EngineDefineEx GetEngineDefine(Color c) { return EngineDefineExes[(int)c]; }
        private EngineDefineEx[] EngineDefineExes = new EngineDefineEx[2];

        /// <summary>
        /// 連続対局のための情報に関する構造体
        /// </summary>
        public ContinuousGame continuousGame = new ContinuousGame();

        #endregion

        #region 依存性のあるプロパティの処理

        /// <summary>
        /// GameStart()のあと、各プレイヤーの初期化中であるか(Initializing,EngineInitializing)を更新する。
        /// このメソッドは、worker threadから定期的に呼び出される。
        /// </summary>
        private void UpdateInitializing()
        {
            // すべてのプレイヤーが、Player.Initializing == falseになったら、準備が完了したということで、
            // NotifyTurnChanged()を呼び出してやる。
            {
                var init = false;
                foreach (var c in All.Colors())
                    init |= Player(c).Initializing;
                if (Initializing && !init)
                {
                    // 状態がtrueからfalseに変わった
                    // 両方の対局準備ができたということなので対局スタート

                    NotifyTurnChanged();

                    if (GameMode == GameModeEnum.InTheGame)
                    {
                        // 「対局開始」の画面素材を表示するためのイベントを発生させる

                        var handicapped = kifuManager.Tree.position.Handicapped;
                        continuousGame.Handicapped = handicapped;
                        RaisePropertyChanged("GameStartEvent", continuousGame);

                        continuousGame.StartTime = DateTime.Now;
                    }
                }
                Initializing = init; // 前回の値を代入しておく。
            }

            // EngineInitializingプロパティの更新
            UpdateEngineInitializing();
        }

        /// <summary>
        /// エンジンの初期化中であることを通知するためのメソッド。GameStart()内からのみ呼び出される。
        ///
        /// Players[]の生成が終わったあと、必要ならば画面に「エンジン初期化中」の画像を描画する。
        /// UpdateInitializing()を呼び出すと人間同士の対局のときに早いタイミングでNotifyTurnChanged()を呼び出してしまうので駄目。
        /// NotifyTurnChanged()はGameModeの変更後に呼び出されて欲しい。
        /// </summary>
        private void UpdateEngineInitializing()
        {
            var engineInit = false;
            foreach (var c in All.Colors())
            {
                var player = Player((Color)c);
                engineInit |= player.PlayerType == PlayerTypeEnum.UsiEngine && player.Initializing;
            }
            SetValue<bool>("EngineInitializing", engineInit);
        }

        private bool Initializing = false;


        /// <summary>
        /// 棋譜ウィンドウの選択行を変更する。
        /// ply を指定しなければ(-1のとき)、現在のkifuManager.Treeに同期させる。
        ///
        /// KifuListSelectedIndexの変更イベントを必ず発生させるので、使い勝手が良い。
        /// </summary>
        private void UpdateKifuSelectedIndex(int ply = -1)
        {
            if (ply == -1)
                ply = kifuManager.Tree.pliesFromRoot;

            SetValueAndRaisePropertyChanged("KifuListSelectedIndex", ply);
        }

        /// <summary>
        /// UsiEnginePlayerのHash , Threadの自動マネージメントのためのクラス
        /// </summary>
        private UsiEngineHashManager UsiEngineHashManager = new UsiEngineHashManager();

        #endregion
    }
}
