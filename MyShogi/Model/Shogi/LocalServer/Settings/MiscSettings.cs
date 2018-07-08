using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    public class MiscSettings : NotifyObject
    {
        public MiscSettings()
        {
            DetailEnable = false;
            MaxMovesToDrawEnable = false;
            MaxMovesToDraw = 256;
        }

        public MiscSettings Clone()
        {
            var clone = (MiscSettings)this.MemberwiseClone();

            return clone;
        }

        /// <summary>
        /// 詳細設定ダイアログが有効であるか
        /// </summary>
        public bool DetailEnable
        {
            get { return GetValue<bool>("DetailEnable"); }
            set { SetValue("DetailEnable", value); }
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
