using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.LocalServer
{

    /// <summary>
    /// LocalGameServer.GameStart()の引数に渡す、対局条件などを一式書いた設定データ
    /// 
    /// 対局ダイアログの設定情報。
    /// GlobalConfigに保存されていて、ここから次回起動時に対局ダイアログの設定を復元もできる。
    /// </summary>
    public class GameSetting
    {
        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public GameSetting()
        {
            // 初回起動時などデシリアライズに失敗した時のためにデフォルト値をきちんと設定しておく。

            Board = new BoardSetting();
            Players = new PlayerSetting[2] { new PlayerSetting(), new PlayerSetting() };

            foreach (var c in All.Colors())
                Player(c).PlayerName = c.Pretty();

            TimeSettings = new TimeSettings();
            MiscSettings = new MiscSettings();
        }

        /// <summary>
        /// Clone()用のコンストラクタ
        /// </summary>
        /// <param name="players"></param>
        /// <param name="board"></param>
        /// <param name="timeSetting"></param>
        private GameSetting(PlayerSetting[] players , BoardSetting board ,
            TimeSettings timeSettings , MiscSettings miscSettings )
        {
            Players = players;
            Board = board;
            TimeSettings = timeSettings;
            MiscSettings = miscSettings;
        }

        /// <summary>
        /// このインスタンスのClone()
        /// </summary>
        /// <returns></returns>
        public GameSetting Clone()
        {
            // premitive typeでないとMemberwiseClone()でClone()されないので自前でCloneする。

            return new GameSetting(
                new PlayerSetting[2] { Players[0].Clone(), Players[1].Clone() },
                Board.Clone(),
                TimeSettings.Clone(),
                MiscSettings.Clone()
            );
        }

        /// <summary>
        /// 開始盤面
        /// </summary>
        public BoardSetting Board;

        /// <summary>
        /// c側の設定情報を取得する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public PlayerSetting Player(Color c) { return Players[(int)c]; }

        /// <summary>
        /// 持ち時間設定
        /// </summary>
        public TimeSettings TimeSettings;

        /// <summary>
        /// その他の細かい設定
        /// </summary>
        public MiscSettings MiscSettings;

        /// <summary>
        /// このメンバーには直接アクセスせずに、Player(Color)のほうを用いて欲しい。
        /// XmlSerializerでシリアライズするときにpublicにしておかないとシリアライズ対象とならないので
        /// publicにしてある。
        /// </summary>
        public PlayerSetting[] Players;
    }
}
