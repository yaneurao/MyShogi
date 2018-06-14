using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Player;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 各プレイヤーごとの対局設定
    /// LocalGameServer.GameStart()の引数に渡す、対局条件などを一式書いた設定データの片側のプレイヤー分。
    /// </summary>
    public class PlayerGameSetting
    {
        /// <summary>
        /// 生成するプレイヤーの型
        /// </summary>
        public PlayerTypeEnum PlayerType;

        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string PlayerName;
        
    }

    /// <summary>
    /// LocalGameServer.GameStart()の引数に渡す、対局条件などを一式書いた設定データ
    /// </summary>
    public class GameSetting
    {
        /// <summary>
        /// c側の設定情報を取得する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public PlayerGameSetting Player(Color c) { return players[(int)c]; }

        /// <summary>
        /// 開始局面。
        /// これがCurrentであれば、現在の局面を初期化せずに開始。
        /// これがOthersであれば、別途、Sfenから初期化。
        /// </summary>
        public BoardType BoardType;

        // -- private members

        private PlayerGameSetting[] players = new PlayerGameSetting[2] { new PlayerGameSetting(), new PlayerGameSetting() };

    }
}
