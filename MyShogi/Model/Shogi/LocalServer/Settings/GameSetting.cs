using System.Runtime.Serialization;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Common.Utility;

namespace MyShogi.Model.Shogi.LocalServer
{

    /// <summary>
    /// LocalGameServer.GameStart()の引数に渡す、対局条件などを一式書いた設定データ
    /// 
    /// 対局ダイアログの設定情報。
    /// GlobalConfigに保存されていて、ここから次回起動時に対局ダイアログの設定を復元もできる。
    /// 
    /// ・BoardSetting
    /// ・MiscSetting
    /// ・TimeSetting
    /// ・PlayerSettings
    /// の集合
    /// 
    /// </summary>
    public class GameSetting
    {
        // -- DataMember

        /// <summary>
        /// 開始盤面
        /// </summary>
        [DataMember]
        public BoardSetting BoardSetting;

        /// <summary>
        /// 持ち時間設定
        /// </summary>
        [DataMember]
        public KifuTimeSettings KifuTimeSettings;

        /// <summary>
        /// その他の細かい設定
        /// </summary>
        [DataMember]
        public MiscSettings MiscSettings;

        /// <summary>
        /// このメンバーには直接アクセスせずに、Player(Color)のほうを用いて欲しい。
        /// XmlSerializerでシリアライズするときにpublicにしておかないとシリアライズ対象とならないので
        /// publicにしてある。
        /// </summary>
        [DataMember]
        public PlayerSetting[] PlayerSettings;

        // -- public methods

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public GameSetting()
        {
            // 初回起動時などデシリアライズに失敗した時のためにデフォルト値をきちんと設定しておく。

            BoardSetting = new BoardSetting();
            PlayerSettings = new PlayerSetting[2] { new PlayerSetting(), new PlayerSetting() };

            // 先後入れ替えるので名前が「先手」「後手」がデフォルトだと紛らわしい。
            // 名前を「わたし」と「あなた」にしとく。
            PlayerSetting(Color.BLACK).PlayerName = "わたし";
            PlayerSetting(Color.WHITE).PlayerName = "あなた";

            KifuTimeSettings = new KifuTimeSettings();
            MiscSettings = new MiscSettings();
        }

        /// <summary>
        /// Clone()用のコンストラクタ
        /// </summary>
        /// <param name="players"></param>
        /// <param name="board"></param>
        /// <param name="timeSetting"></param>
        private GameSetting(PlayerSetting[] players, BoardSetting board,
            KifuTimeSettings kifuTimeSettings, MiscSettings miscSettings)
        {
            PlayerSettings = players;
            BoardSetting = board;
            KifuTimeSettings = kifuTimeSettings;
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
                new PlayerSetting[2] { PlayerSettings[0].Clone(), PlayerSettings[1].Clone() },
                BoardSetting.Clone(),
                KifuTimeSettings.Clone(),
                MiscSettings.Clone()
            );
        }

        /// <summary>
        /// 対局条件の正当性をチェックする。
        /// 
        /// おかしい場合は、メッセージ文字列を返す。
        /// 問題ないければnullを返す。
        /// </summary>
        public string IsValid()
        {
            foreach (var c in All.Colors())
            {
                var player = PlayerSetting(c);
                if (player.IsCpu)
                {
                    if (player.EngineDefineFolderPath == null)
                        return $"{c.Pretty()}側のソフトが選ばれていません。";
                }

                // 他、持ち時間設定など何か矛盾があるかチェックする(といいかも)
            }

            return null;
        }

        /// <summary>
        /// c側の設定情報を取得する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public PlayerSetting PlayerSetting(Color c) { return PlayerSettings[(int)c]; }

        /// <summary>
        /// 先手と後手のプレイヤーを入れ替える。
        /// TimeSettingsのほうも入れ替える。
        /// </summary>
        public void SwapPlayer()
        {
            Utility.Swap(ref PlayerSettings[0], ref PlayerSettings[1]);
            KifuTimeSettings.SwapPlayer();
        }

        public GameSettingMin ToGameSettingMin()
        {
            return new GameSettingMin()
            {
                BoardSetting = BoardSetting.ToBoardSettingMin(),
                KifuTimeSettings = KifuTimeSettings.ToKifuTimeSettingsMin(),
                MiscSettings = MiscSettings.ToMiscSettingsMin(),
                PlayerSettings = new PlayerSettingMin[2] { PlayerSettings[0].ToPlayerSettingMin() , PlayerSettings[1].ToPlayerSettingMin() },
            };
        }

        public static GameSetting FromGameSettingMin(GameSettingMin min)
        {
            return new GameSetting()
            {
                BoardSetting = BoardSetting.FromBoardSettingMin(min.BoardSetting),
                KifuTimeSettings = KifuTimeSettings.FromKifuTimeSettingsMin(min.KifuTimeSettings),
                MiscSettings = MiscSettings.FromMiscSettingsMin(min.MiscSettings),
                PlayerSettings = new PlayerSetting[2] { LocalServer.PlayerSetting.FromPlayerSettingMin(min.PlayerSettings[0]) , LocalServer.PlayerSetting.FromPlayerSettingMin(min.PlayerSettings[1]) },
            };
        }
    }

    [DataContract]
    public class GameSettingMin
    {
        public BoardSettingMin BoardSetting;
        public KifuTimeSettingsMin KifuTimeSettings;
        public MiscSettingsMin MiscSettings;
        public PlayerSettingMin[] PlayerSettings;
    }
}
