using System;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Player;

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
        public GameSetting()
        {
            // 初回起動時などデシリアライズに失敗した時のためにデフォルト値をきちんと設定しておく。

            BoardTypeEnable = true;
            BoardType = BoardType.NoHandicap;

            Players = new PlayerSetting[2] { new PlayerSetting(), new PlayerSetting() };

            foreach (var c in All.Colors())
                Player(c).PlayerName = c.Pretty();

            TimeSetting = new TimeSetting();
        }

        /// <summary>
        /// 開始局面。
        /// BoardCurrentがtrueなら、この値は無視される。
        /// この値がCurrent,Othersは許容しない。
        /// </summary>
        public BoardType BoardType;

        /// <summary>
        /// BoardTypeの局面から開始するのかのフラグ
        /// BoardTypeEnableかBoardCurrentのどちらかがtrueのはず。
        /// </summary>
        public bool BoardTypeEnable;

        /// <summary>
        /// 現在の局面から開始するのかのフラグ
        /// </summary>
        public bool BoardTypeCurrent;

        /// <summary>
        /// c側の設定情報を取得する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public PlayerSetting Player(Color c) { return Players[(int)c]; }

        /// <summary>
        /// 持ち時間設定
        /// </summary>
        public TimeSetting TimeSetting;

        /// <summary>
        /// このメンバーには直接アクセスせずに、Player(Color)のほうを用いて欲しい。
        /// XmlSerializerでシリアライズするときにpublicにしておかないとシリアライズ対象とならないので
        /// publicにしてある。
        /// </summary>
        public PlayerSetting[] Players;

    }
}
