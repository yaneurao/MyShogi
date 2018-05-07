using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace MyShogi.Model
{
#if false
    /// <summary>
    /// 探索時間などを制限するときの種別
    /// </summary>
    public enum ThinkLimitType
    {
        /// <summary>
        /// 無制限
        /// </summary>
        [LabelDescription(Label = "無制限")]
        Infinite,
        /// <summary>
        /// 思考時間
        /// </summary>
        [LabelDescription(Label = "時間(秒)")]
        Time,
        /// <summary>
        /// ノード数
        /// </summary>
        [LabelDescription(Label = "ノード数")]
        Node,
        /// <summary>
        /// 探索深さ
        /// </summary>
        [LabelDescription(Label = "探索深さ")]
        Depth,
    }

    /// <summary>
    /// 将棋エンジンの思考制限を行うためのデータです。
    /// </summary>
    public sealed class ThinkLimit : NotifyObject
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ThinkLimit()
        {
            LimitType = ThinkLimitType.Infinite;
            Time = TimeSpan.FromSeconds(300);
            Nodes = 1000 * 10000;
            Depth = 20;
        }

        /// <summary>
        /// 制限の種類を取得または設定します。
        /// </summary>
        public ThinkLimitType LimitType
        {
            get { return GetValue<ThinkLimitType>("LimitType"); }
            set { SetValue("LimitType", value); }
        }

        /// <summary>
        /// 探索を無制限で行うかどうかを取得します。
        /// </summary>
        [DependOnProperty("LimitType")]
        public bool IsInfinite
        {
            get { return (LimitType == ThinkLimitType.Infinite); }
        }

        /// <summary>
        /// 時間制限を取得または設定します。
        /// </summary>
        public TimeSpan Time
        {
            get { return GetValue<TimeSpan>("Time"); }
            set { SetValue("Time", value); }
        }

        /// <summary>
        /// 時間制限を秒で取得または設定します。
        /// </summary>
        [DependOnProperty("Time")]
        public int TimeSeconds
        {
            get { return (int)Math.Floor(Time.TotalSeconds); }
            set { Time = TimeSpan.FromSeconds(value); }
        }

        /// <summary>
        /// 探索を時間で制限するかどうかを取得します。
        /// </summary>
        [DependOnProperty("LimitType")]
        public bool IsLimitTime
        {
            get { return (LimitType == ThinkLimitType.Time); }
        }

        /// <summary>
        /// 制限するノード数を取得または設定します。
        /// </summary>
        public long Nodes
        {
            get { return GetValue<long>("Nodes"); }
            set { SetValue("Nodes", value); }
        }

        /// <summary>
        /// 探索をノード数で制限するかどうかを取得します。
        /// </summary>
        [DependOnProperty("LimitType")]
        public bool IsLimitNode
        {
            get { return (LimitType == ThinkLimitType.Node); }
        }

        /// <summary>
        /// 制限する探索深さを取得または設定します。
        /// </summary>
        public int Depth
        {
            get { return GetValue<int>("Depth"); }
            set { SetValue("Depth", value); }
        }

        /// <summary>
        /// 探索を探索深さで制限するかどうかを取得します。
        /// </summary>
        [DependOnProperty("LimitType")]
        public bool IsLimitDepth
        {
            get { return (LimitType == ThinkLimitType.Depth); }
        }
    }
#endif
}
