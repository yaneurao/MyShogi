using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局設定のうちのその他の設定
    /// </summary>
    public class MiscSettings : NotifyObject
    {
        // -- DataMembers

        /// <summary>
        /// 最大手数で引き分けが有効であるか
        /// これがtrueだとMaxMovesToDrawの値が有効になる。
        /// </summary>
        [DataMember]
        public bool MaxMovesToDrawEnable
        {
            get { return GetValue<bool>("MaxMovesToDrawEnable"); }
            set { SetValue("MaxMovesToDrawEnable", value); }
        }

        /// <summary>
        /// 引き分けになる手数。この手数に達した時に引き分けになる。
        /// </summary>
        [DataMember]
        public int MaxMovesToDraw
        {
            get { return GetValue<int>("MaxMovesToDraw"); }
            set { SetValue("MaxMovesToDraw", value); }
        }

        /// <summary>
        /// 入玉ルールの設定
        ///
        /// 0 : 入玉ルールなし
        /// 1 : 27点法(CSAルール)
        /// 2 : 24点法(CSAルール)
        /// 3 : トライルール
        ///
        /// EnteringKingRule enumと同じ値。
        /// </summary>
        [DataMember]
        public int EnteringKingRule
        {
            get { return GetValue<int>("EnteringKingRule"); }
            set { SetValue("EnteringKingRule", value); }
        }

        // -- public members

        public MiscSettings()
        {
            MaxMovesToDrawEnable = false;
            MaxMovesToDraw = 256;
            EnteringKingRule = 1; // デフォルトでは27点法
        }

        public MiscSettings Clone()
        {
            var clone = (MiscSettings)this.MemberwiseClone();

            return clone;
        }

        public MiscSettingsMin ToMiscSettingsMin()
        {
            return new MiscSettingsMin()
            {
                MaxMovesToDrawEnable = MaxMovesToDrawEnable,
                MaxMovesToDraw = MaxMovesToDraw,
                EnteringKingRule = EnteringKingRule,
            };
        }

        public static MiscSettings FromMiscSettingsMin(MiscSettingsMin min)
        {
            return new MiscSettings()
            {
                MaxMovesToDrawEnable = min.MaxMovesToDrawEnable,
                MaxMovesToDraw = min.MaxMovesToDraw,
                EnteringKingRule = min.EnteringKingRule,
            };
        }
    }

    [DataContract]
    public class MiscSettingsMin
    {
        [DataMember] public bool MaxMovesToDrawEnable;
        [DataMember] public int MaxMovesToDraw;
        [DataMember] public int EnteringKingRule;
    }
}
