using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 検討時の設定
    /// 検討エンジン設定ダイアログにdata-bindして使う用。
    /// 
    /// 単独エンジン設定用。
    /// </summary>
    public class ConsiderationEngineSetting : NotifyObject
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
        /// 思考時間等の制限なしか？
        /// </summary>
        public bool Limitless
        {
            get { return GetValue<bool>("Limitless"); }
            set { SetValue("Limitless", value); }
        }

        /// <summary>
        /// 時間制限ありか
        /// </summary>
        public bool TimeLimitEnable
        {
            get { return GetValue<bool>("TimeLimitEnable"); }
            set { SetValue("TimeLimitEnable", value); }
        }

        /// <summary>
        /// 制限あり(TimeLimitEnable == true)のときに、その1局面に使う秒数。
        /// </summary>
        public int Second
        {
            get { return GetValue<int>("Second"); }
            set { SetValue("Second", value); }
        }

        // その他、node制限なども入れるかも..

        public ConsiderationEngineSetting()
        {
            Limitless = true;
        }
    }
}
