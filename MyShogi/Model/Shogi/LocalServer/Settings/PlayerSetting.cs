using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 各プレイヤーごとの対局設定
    /// LocalGameServer.GameStart()の引数に渡す、対局条件などを一式書いた設定データの片側のプレイヤー分。
    /// </summary>
    public class PlayerSetting : NotifyObject
    {
        // -- DataMembers

        /// <summary>
        /// プレイヤー名
        /// </summary>
        [DataMember]
        public string PlayerName
        {
            get { return GetValue<string>("PlayerName"); }
            set { SetValue("PlayerName", value); }
        }

        /// <summary>
        /// 対局相手は人間か？
        /// </summary>
        [DataMember]
        public bool IsHuman
        {
            get { return GetValue<bool>("IsHuman"); }
            set { SetValue("IsHuman", value); }
        }

        /// <summary>
        /// 対局相手はコンピュータか？
        /// </summary>
        [DataMember]
        public bool IsCpu
        {
            get { return GetValue<bool>("IsCpu"); }
            set { SetValue("IsCpu", value); }
        }

        /// <summary>
        /// (CPUだとして)
        /// エンジンの設定ファイルのfolder path(相対)
        ///     EngineDefineEx.FolderPath
        /// 先後分。
        /// </summary>
        [DataMember]
        public string EngineDefineFolderPath
        {
            get { return GetValue<string>("EngineDefineFolderPath"); }
            set { SetValue("EngineDefineFolderPath", value); }
        }

        /// <summary>
        /// プリセットの選択している番号 + 1。(0 = カスタム)
        /// </summary>
        [DataMember]
        public int SelectedEnginePreset
        {
            get { return GetValue<int>("SelectedEnginePreset"); }
            set { SetValue("SelectedEnginePreset", value); }
        }

        /// <summary>
        /// ponder(コンピュータが相手の手番でも考えるのか)の設定。
        /// </summary>
        [DataMember]
        public bool Ponder
        {
            get { return GetValue<bool>("Ponder"); }
            set { SetValue("Ponder", value); }
        }

        // -- public members

        public PlayerSetting()
        {
            IsHuman = true;
            IsCpu = false;
        }

        /// <summary>
        /// このインスタンスのClone()
        /// </summary>
        /// <returns></returns>
        public PlayerSetting Clone()
        {
            return (PlayerSetting)this.MemberwiseClone();
        }

        public PlayerSettingMin ToPlayerSettingMin()
        {
            return new PlayerSettingMin()
            {
                PlayerName = PlayerName,
                IsHuman = IsHuman,
                IsCpu = IsCpu,
                EngineDefineFolderPath = EngineDefineFolderPath,
                SelectedEnginePreset = SelectedEnginePreset,
                Ponder = Ponder,
            };
        }

        public static PlayerSetting FromPlayerSettingMin(PlayerSettingMin min)
        {
            return new PlayerSetting()
            {
                PlayerName = min.PlayerName,
                IsHuman = min.IsHuman,
                IsCpu = min.IsCpu,
                EngineDefineFolderPath = min.EngineDefineFolderPath,
                SelectedEnginePreset = min.SelectedEnginePreset,
                Ponder = min.Ponder,
            };
        }
    }

    [DataContract]
    public class PlayerSettingMin
    {
        [DataMember] public string PlayerName;
        [DataMember] public bool IsHuman;
        [DataMember] public bool IsCpu;
        [DataMember] public string EngineDefineFolderPath;
        [DataMember] public int SelectedEnginePreset;
        [DataMember] public bool Ponder;
    }
}
