using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 検討設定
    /// 検討設定ダイアログにdata-bindして使う用。
    /// 
    /// 単独エンジン設定用。
    /// </summary>
    public class ConsiderationSetting : NotifyObject
    {
        /// <summary>
        /// エンジン名
        /// </summary>
        [DataMember]
        public string PlayerName
        {
            get { return GetValue<string>("PlayerName"); }
            set { SetValue("PlayerName", value); }
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
        /// 時間制限なしか？
        /// </summary>
        public bool TimeLimitless
        {
            get { return GetValue<bool>("TimeLimitless"); }
            set { SetValue("TimeLimitless", value); }
        }

        /// <summary>
        /// 時間制限あり(TimeLimitless == false)のときに、その1局面に使う秒数。
        /// </summary>
        public int Second
        {
            get { return GetValue<int>("Second"); }
            set { SetValue("Second", value); }
        }
    }
}
