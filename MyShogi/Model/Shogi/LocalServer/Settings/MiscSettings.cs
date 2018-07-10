using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    public class MiscSettings : NotifyObject
    {
        public MiscSettings()
        {
            MaxMovesToDrawEnable = false;
            MaxMovesToDraw = 256;
        }

        public MiscSettings Clone()
        {
            var clone = (MiscSettings)this.MemberwiseClone();

            return clone;
        }

        /// <summary>
        /// 最大手数で引き分けが有効であるか
        /// これがtrueだとMaxMovesToDrawの値が有効になる。
        /// </summary>
        public bool MaxMovesToDrawEnable
        {
            get { return GetValue<bool>("MaxMovesToDrawEnable"); }
            set { SetValue("MaxMovesToDrawEnable", value); }
        }

        /// <summary>
        /// 引き分けになる手数。この手数に達した時に引き分けになる。
        /// </summary>
        public int MaxMovesToDraw
        {
            get { return GetValue<int>("MaxMovesToDraw"); }
            set { SetValue("MaxMovesToDraw", value); }
        }
    }
}
