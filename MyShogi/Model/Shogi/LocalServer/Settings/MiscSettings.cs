namespace MyShogi.Model.Shogi.LocalServer
{
    public class MiscSettings
    {
        public MiscSettings()
        {
            DetailEnable = false;
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
    }
}
