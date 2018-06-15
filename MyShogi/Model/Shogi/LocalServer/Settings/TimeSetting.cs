namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局時間設定
    /// </summary>
    public class TimeSetting
    {
        public TimeSetting()
        {
            Minute = 15;
            ByoyomiEnable = true;
        }

        /// <summary>
        /// 持ち時間の[時]
        /// </summary>
        public int Hour;

        /// <summary>
        /// 持ち時間の[分]
        /// </summary>
        public int Minute;

        /// <summary>
        /// 持ち時間を使い切ったときの
        /// 秒読みの[秒]
        /// </summary>
        public int Byoyomi;

        /// <summary>
        /// Byoyomiは有効か？
        /// これがfalseならByoyomiの値は無効。
        /// </summary>
        public bool ByoyomiEnable;

        /// <summary>
        /// 1手ごとの加算
        /// </summary>
        public int IncTime;

        /// <summary>
        /// IncTimeは有効か？
        /// これがfalseならIncTimeの値は無効。
        /// </summary>
        public bool IncTimeEnable;

    }
}
