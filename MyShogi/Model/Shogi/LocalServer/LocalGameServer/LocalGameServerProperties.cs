using System.Collections.Generic;
using System.Diagnostics;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.Player;
using MyShogi.Model.Shogi.Usi;

namespace MyShogi.Model.Shogi.LocalServer
{
    public partial class LocalGameServer
    {
        #region public properties

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
                        TheApp.app.MessageShow(error , MessageShowType.Error);
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
                SetValue<bool>("InTheBoardEdit" , next == GameModeEnum.InTheBoardEdit);
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
            get { return canUserMove && EnableUserMove;}
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
            return GameSetting.PlayerSetting(c).PlayerName;
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
        /// 最後に棋譜を保存してから棋譜が更新されたかのフラグ
        /// </summary>
        public bool KifuDirty
        {
            get { return kifuManager.Tree.KifuDirty; }
            set { kifuManager.Tree.KifuDirty = value; }
        }

        /// <summary>
        /// 各PlayerのEngineDefine
        /// </summary>
        public EngineDefineEx GetEngineDefine(Color c) { return EngineDefineExes[(int)c]; }
        private EngineDefineEx[] EngineDefineExes = new EngineDefineEx[2];

        /// <summary>
        /// 通常対局のときにエンジンの選択しているPreset名。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string PresetName(Color c) { return presetNames[(int)c]; }
        private string[] presetNames = new string[2];

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
                }
                Initializing = init; // 前回の値を代入しておく。
            }

            // EngineInitializingプロパティの更新
            {
                var engineInit = false;
                foreach (var c in All.Colors())
                {
                    var player = Player((Color)c);
                    engineInit |= player.PlayerType == PlayerTypeEnum.UsiEngine && player.Initializing;
                }
                SetValue<bool>("EngineInitializing", engineInit);
            }
        }
        private bool Initializing = false;

        /// <summary>
        /// Positionプロパティの更新。
        /// immutableにするためにCloneしてセットする。
        /// 自動的にViewに通知される。
        /// 
        /// ※　KifuTreeのほうでPositionが更新された時に通知が来る。
        /// </summary>
        private void PositionChanged(PropertyChangedEventArgs args)
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
        /// ※　KifuTreeのほうでPositionが更新された時に通知が来るので、このメソッドでトラップして、
        /// このクラスのNotifyObjectによって、このことを棋譜ウィンドウに通知する。
        /// </summary>
        private void KifuListChanged(PropertyChangedEventArgs args)
        {
            // このイベントをトラップしている。
            Debug.Assert(args.name == "KifuList");

            // Cloneしたものをセットする。
            args.value = new List<string>(kifuManager.KifuList);

            // このクラスのNotifyObjectによる通知がなされる。
            // "KifuList"プロパティの変更通知が飛ぶ。
            SetValue<List<string>>(args);
        }

        /// <summary>
        /// KifuListが1行増えた時に飛んでくるイベントをtrapする。
        /// args.value == string : 増えた1行
        /// </summary>
        /// <param name="args"></param>
        private void KifuListAdded(PropertyChangedEventArgs args)
        {
            Debug.Assert(args.name == "KifuListAdded");

            // このクラスのNotifyObjectによる通知がなされる。
            // "KifuListAdded"プロパティの変更通知が飛ぶ。
            // SetValue()ではなくRaise..()のほうにしておかないと変化がないときに変更通知こない。
            RaisePropertyChanged(args);
        }

        /// <summary>
        /// KifuListが1行減った時に飛んでくるイベントをtrapする。
        /// </summary>
        /// <param name="args"></param>
        private void KifuListRemoved(PropertyChangedEventArgs args)
        {
            Debug.Assert(args.name == "KifuListRemoved");

            // このクラスのNotifyObjectによる通知がなされる。
            // "KifuListAdded"プロパティの変更通知が飛ぶ。
            RaisePropertyChanged(args);
        }

        /// <summary>
        /// 棋譜ウィンドウの選択行を変更する。
        /// ply を指定しなければ(-1のとき)、現在のkifuManager.Treeに同期させる。
        /// </summary>
        private void UpdateKifuSelectedIndex(int ply = -1)
        {
            if (ply == -1)
                ply = kifuManager.Tree.pliesFromRoot;

            KifuListSelectedIndex = ply;
        }

        /// <summary>
        /// UsiEnginePlayerのHash , Threadの自動マネージメントのためのクラス
        /// </summary>
        private UsiEngineHashManager UsiEngineHashManager = new UsiEngineHashManager();

        #endregion
    }
}
