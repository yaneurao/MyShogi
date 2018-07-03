using System;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// 持ち時間の情報を保持します。
    /// </summary>
    /// <remarks>
    /// 情報の保持のみで、実際の時間管理は他のクラスで行う。
    /// </remarks>
    public class UsiTimeData
    {
        /// <summary>
        /// 持ち時間のhourを取得または設定します。
        /// </summary>
        public int TotalTimeHours
        {
            get;
            set;
        }

        /// <summary>
        /// 持ち時間のminuteを取得または設定します。
        /// </summary>
        public int TotalTimeMinutes
        {
            get;
            set;
        }

        /// <summary>
        /// 持ち時間のsecondを取得または設定します。
        /// </summary>
        public int TotalTimeSeconds
        {
            get;
            set;
        }

        /// <summary>
        /// 秒読み時用の時間を取得または設定します。
        /// </summary>
        public int ByoyomiSeconds
        {
            get;
            set;
        }

        /// <summary>
        /// 持ち時間を取得または設定します。
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                return TimeSpan.FromSeconds(
                    TotalTimeHours * 60 * 60 +
                    TotalTimeMinutes * 60 +
                    TotalTimeSeconds);
            }
            set
            {
                TotalTimeHours = value.Hours + value.Days * 24;
                TotalTimeMinutes = value.Minutes;
                TotalTimeSeconds = value.Seconds;
            }
        }

        /// <summary>
        /// 秒読み時間を取得または設定します。
        /// (秒読み設定で対局する時用)
        /// </summary>
        public TimeSpan Byoyomi
        {
            get { return TimeSpan.FromSeconds(ByoyomiSeconds); }
            set { ByoyomiSeconds = (int)value.TotalSeconds; }
        }

        /// <summary>
        /// 対局中は時間切れでも負けない設定があるので、
        /// その場合に該当するかどうかを取得または設定します。
        /// 
        /// IsTimeoutable == trueのときは、残り時間は関係ないので利用側では無視される。
        /// </summary>
        /// この設定はシリアライズしません。
        /// <remarks>
        public bool IsTimeoutable
        {
            get;
            set;
        }

        /// <summary>
        /// 時間が正しく設定されているか取得します。
        /// </summary>
        public bool IsValid()
        {
            if (TotalTime < TimeSpan.Zero)
            {
                return false;
            }

            if (Byoyomi < TimeSpan.Zero)
            {
                return false;
            }

            if (!IsTimeoutable &&
                TotalTime == TimeSpan.Zero && Byoyomi == TimeSpan.Zero)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// オブジェクトのコピーを作成します。
        /// </summary>
        public UsiTimeData Clone()
        {
            return (UsiTimeData)this.MemberwiseClone();
        }
    }
}
