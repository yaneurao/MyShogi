using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 各プレイヤーごとの対局設定
    /// LocalGameServer.GameStart()の引数に渡す、対局条件などを一式書いた設定データの片側のプレイヤー分。
    /// </summary>
    public class PlayerSetting : NotifyObject
    {
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

        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string PlayerName
        {
            get { return GetValue<string>("PlayerName"); }
            set { SetValue("PlayerName", value); }
        }

        /// <summary>
        /// 対局相手は人間か？
        /// </summary>
        public bool IsHuman
        {
            get { return GetValue<bool>("IsHuman"); }
            set { SetValue("IsHuman", value); }
        }

        /// <summary>
        /// 対局相手はコンピュータか？
        /// </summary>
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
        public string EngineDefineFolderPath
        {
            get { return GetValue<string>("EngineDefineFolderPath"); }
            set { SetValue("EngineDefineFolderPath", value); }
        }

        /// <summary>
        /// プリセットの選択している番号。
        /// </summary>
        public int SelectedEnginePreset
        {
            get { return GetValue<int>("SelectedEnginePreset"); }
            set { SetValue("SelectedEnginePreset", value); }
        }

    }
}
