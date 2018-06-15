namespace MyShogi.Model.Shogi.LocalServer
{
    public class MiscSettings
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
        public bool DetailEnable;

        /// <summary>
        /// 最大手数で引き分けが有効であるか
        /// これがtrueだとMaxMovesToDrawの値が有効になる。
        /// </summary>
        public bool MaxMovesToDrawEnable;

        /// <summary>
        /// 引き分けになる手数。この手数に達した時に引き分けになる。
        /// </summary>
        public int MaxMovesToDraw;
    }
}
