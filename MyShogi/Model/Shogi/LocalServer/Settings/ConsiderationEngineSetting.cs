using System;
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
        public ConsiderationEngineSetting()
        {
            Limitless = true;

            // 他のデフォルト値も設定しておいてやる。
            Second = 5;
            Nodes = 10000000;
            Depth = 20;
        }

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

        /// <summary>
        /// Node制限ありか
        /// </summary>
        public bool NodesLimitEnable
        {
            get { return GetValue<bool>("NodesLimitEnable"); }
            set { SetValue("NodesLimitEnable", value); }
        }

        /// <summary>
        /// Node制限あり(NodesLimitEnable == true)のときに、その1局面のnode数。
        /// エンジン側がint64_tになっているはずなので、UInt64にせんでもええやろ。
        /// </summary>
        public Int64 Nodes
        {
            get { return GetValue<Int64>("Nodes"); }
            set { SetValue<Int64>("Nodes", value); }
        }

        /// <summary>
        /// Depth制限ありか
        /// </summary>
        public bool DepthLimitEnable
        {
            get { return GetValue<bool>("DepthLimitEnable"); }
            set { SetValue("DepthLimitEnable", value); }
        }

        /// <summary>
        /// Depth制限あり(DepthLimitEnable == true)のときに、その1局面のdepth
        /// </summary>
        public int Depth
        {
            get { return GetValue<int>("Depth"); }
            set { SetValue("Depth", value); }
        }

    }
}
