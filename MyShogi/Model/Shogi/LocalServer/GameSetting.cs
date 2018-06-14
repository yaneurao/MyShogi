using System;
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
    /// 
    /// 対局ダイアログの設定情報。
    /// GlobalConfigに保存されていて、ここから次回起動時に対局ダイアログの設定を復元もできる。
    /// </summary>
    public class GameSetting
    {
        public GameSetting()
        {
            // 初回起動時などデシリアライズに失敗した時のためにデフォルト値をきちんと設定しておく。

            BoardType = BoardType.NoHandicap;

            Players = new PlayerGameSetting[2]
            { new PlayerGameSetting(), new PlayerGameSetting() };

            var black = Player(Color.BLACK);
            var white = Player(Color.WHITE);

            black.PlayerName = "あなた";
            white.PlayerName = "わたし";

            black.PlayerType = PlayerTypeEnum.Human;
            white.PlayerType = PlayerTypeEnum.Human;
        }

        /// <summary>
        /// 開始局面。
        /// これがCurrentであれば、現在の局面を初期化せずに開始。
        /// これがOthersであれば、別途、Sfenから初期化。
        /// </summary>
        public BoardType BoardType;

        /// <summary>
        /// c側の設定情報を取得する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public PlayerGameSetting Player(Color c) { return Players[(int)c]; }

        /// <summary>
        /// このメンバーには直接アクセスせずに、Player(Color)のほうを用いて欲しい。
        /// XmlSerializerでシリアライズするときにpublicにしておかないとシリアライズ対象とならないので
        /// publicにしてある。
        /// </summary>
        public PlayerGameSetting[] Players;

    }
}
