using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Player;
using System.Collections.Generic;

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
        /// 対局中であるかを示すフラグ。
        /// これがfalseであれば対局中ではないので自由に駒を動かせる。(ようにするかも)
        /// </summary>
        public bool InTheGame
        {
            get { return GetValue<bool>("InTheGame"); }
            private set { SetValue<bool>("InTheGame", value); }
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
            kifuManager.Tree.SetKifuMoveTimes( PlayTimers.GetKifuMoveTimes() );

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

        #endregion
    }
}
