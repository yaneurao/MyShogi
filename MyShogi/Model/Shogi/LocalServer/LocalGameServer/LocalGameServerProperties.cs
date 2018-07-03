using System.Collections.Generic;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Player;

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
            private set {
                SetValue<GameModeEnum>("GameMode", value);

                // 依存プロパティの更新
                InTheGame = value == GameModeEnum.InTheGame;
                InTheBoardEdit = value == GameModeEnum.InTheBoardEdit;
            }
        }

        /// <summary>
        /// 対局中であるかを返す。これは、GameModeに依存している依存プロパティ。
        /// このsetterを呼び出してはならない。
        /// </summary>
        public bool InTheGame
        {
            get { return GetValue<bool>("InTheGame"); }
            private set { SetValue<bool>("InTheGame",value); }
        }

        /// <summary>
        /// 盤面編集中であるかを返す。これは、GameModeに依存している依存プロパティ。
        /// このsetterを呼び出してはならない。
        /// </summary>
        public bool InTheBoardEdit
        {
            get { return GetValue<bool>("InTheBoardEdit"); }
            private set {
                SetValue<bool>("InTheBoardEdit", value);

                // 依存プロパティの更新
                TheApp.app.config.InTheBoardEdit = value;
            }
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
        // このクラスのStart()が呼び出された時に呼び出される。
        //public bool GameServerStarted { get; }

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
        /// デフォルトfalse。これをtrueにすると、要所要所でこのクラスのpropertyであるEngineInfoが
        /// 設定されるので、必要ならば外部から変更イベントを捕捉すれば良い。
        /// </summary>
        public bool EngineInfoEnable { get; set; }

        /// <summary>
        /// EngineInfoEnableがtrueの時に、エンジンの読み筋などを出力するためのハンドラ。
        /// </summary>
        public EngineInfo EngineInfo
        {
            get { return GetValue<EngineInfo>("EngineInfo"); }
            private set { SetValue<EngineInfo>("EngineInfo", value); }
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
        /// GameStart()のあと、各プレイヤーの初期化中であるか。
        /// </summary>
        private bool Initializing
        {
            get { return initializing; }
            set
            {
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
