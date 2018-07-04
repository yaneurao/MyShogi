using System.Collections.Generic;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
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
                if (old == value)
                    return; // 値が同じなので何もしない
                SetValue<GameModeEnum>("GameMode", value);

                // エンジンを用いた検討モードを抜ける or 入るのであれば、そのコマンドを叩く。
                if (old.IsWithEngine())
                    EndConsideration();

                if (value.IsWithEngine())
                    StartConsideration();

                // 依存プロパティの更新
                SetValue<bool>("InTheGame", value == GameModeEnum.InTheGame);
                SetValue<bool>("InTheBoardEdit" , value == GameModeEnum.InTheBoardEdit);
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
        //public bool TurnChanged { }

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
            return kifuManager.KifuHeader.GetPlayerName(c);
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
            return name.Left(8); // 最大で8文字まで
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
            return PlayTimer(c).KifuTimeSetting.ToShortString();
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
        //public bool RestTimeChanged { get; }

        // 仮想プロパティ
        // 棋譜読み込み時など、こちら側の要請により、棋譜ウィンドウを指定行に移動させるイベント
        //public int SetKifuListIndex { get; }

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
        public bool NoThread = false;

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

        #endregion

        #region 依存性のあるプロパティの処理

        /// <summary>
        /// GameStart()のあと、各プレイヤーの初期化中であるかを更新する。
        /// </summary>
        private void UpdateInitializing()
        {
            // すべてのプレイヤーが、Player.Initializing == falseになったら、準備が完了したということで、
            // NotifyTurnChanged()を呼び出してやる。
            {
                var init = false;
                foreach (var c in All.Colors())
                    init |= Player(c).Initializing;
                if (lastInitializing && !init)
                {
                    // 状態がtrueからfalseに変わった
                    // 両方の対局準備ができたということなので対局スタート
                    NotifyTurnChanged();
                }
                lastInitializing = init;
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
        private bool lastInitializing = false;

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

        /// <summary>
        /// 棋譜ウィンドウの選択行を変更する。
        /// ply を指定しなければ(-1のとき)、現在のkifuManager.Treeに同期させる。
        /// </summary>
        private void UpdateKifuSelectedIndex(int ply = -1)
        {
            if (ply == -1)
                ply = kifuManager.Tree.pliesFromRoot;
            RaisePropertyChanged("SetKifuListIndex", ply);
            KifuSelectedIndexChangedCommand(ply);
        }

        #endregion
    }
}
