using System.Text;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局時間設定
    /// 片側のプレイヤー分
    /// </summary>
    public class TimeSetting
    {
        public TimeSetting()
        {
            Minute = 15;
            ByoyomiEnable = true;
        }

        /// <summary>
        /// このインスタンスのClone()
        /// </summary>
        /// <returns></returns>
        public TimeSetting Clone()
        {
            return (TimeSetting)this.MemberwiseClone();
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
        /// 持ち時間の[秒]
        /// </summary>
        public int Second;

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
        /// 1手ごとの加算(秒)
        /// </summary>
        public int IncTime;

        /// <summary>
        /// IncTimeは有効か？
        /// これがfalseならIncTimeの値は無効。
        /// </summary>
        public bool IncTimeEnable;

        /// <summary>
        /// 時間切れを負けにしない
        /// </summary>
        public bool IgnoreTime;

        /// <summary>
        /// この持ち時間設定を文字列化する。
        /// </summary>
        /// <returns></returns>
        public string ToShortString()
        {
            var sb = new StringBuilder();
            if (Hour != 0 || Minute != 0)
            {
                //sb.Append("持ち時間");
                if (Hour != 0)
                    sb.Append($"{Hour}時間");
                if (Minute != 0)
                    sb.Append($"{Minute}分");
                if (Second != 0)
                    sb.Append($"{Second}秒");
            }
            if (ByoyomiEnable)
            {
                if (Byoyomi == 0)
                {
                    if (sb.Length != 0)
                        sb.Append("切れ負け");
                } else
                {
                    if (sb.Length != 0)
                        sb.Append(" "); // 前の文字があるならスペースを放り込む
                    sb.Append($"秒読み{Byoyomi}秒");
                }
            }
            if (IncTimeEnable && IncTime != 0)
            {
                if (sb.Length != 0)
                    sb.Append(" "); // 前の文字があるならスペースを放り込む
                sb.Append($"1手{IncTime}秒加算");
            }
            return sb.ToString();
        }
    }
}
