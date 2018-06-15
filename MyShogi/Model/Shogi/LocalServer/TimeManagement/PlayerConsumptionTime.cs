using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// プレイヤーの消費時間を管理する。
    /// 片側のプレイヤー分
    /// </summary>
    public class PlayerConsumptionTime
    {
        public PlayerConsumptionTime()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Stop();
        }

        /// <summary>
        /// 持ち時間設定
        /// </summary>
        public TimeSetting TimeSetting { set; get; }

        /// <summary>
        /// ゲーム開始なので、TimeSettingの時間をRestTimeに反映させる。
        /// </summary>
        public void GameStart()
        {
            RestTime = new TimeSpan(TimeSetting.Hour, TimeSetting.Minute, TimeSetting.Second);
            first = true; // 初期化のIncTimeをなくすためのフラグ
        }

        /// <summary>
        /// 自分の手番になった。
        /// IncTimeを加算して、タイマーを開始する。
        /// </summary>
        public void ChangeToOurTurn()
        {
            // 初手はIncTimeしない。
            if (TimeSetting.IncTimeEnable && !first)
                RestTime += new TimeSpan(0,0,TimeSetting.IncTime);
            first = false;

            // byoyomiありかも知れないのでいったんリセットする。
            if (RestTime < TimeSpan.Zero)
                RestTime = TimeSpan.Zero;

            StartTimer();
        }

        /// <summary>
        /// 自分の手番が終わり。
        /// タイマーを終了する。
        /// </summary>
        public TimeSpan ChageToThemTurn()
        {
            StopTimer();
            var consume = ConsumptionTime();
            // この繰り上げた時間を消費して、RestTimeから減らす
            RestTime -= consume;
            if (RestTime < TimeSpan.Zero)
                RestTime = TimeSpan.Zero;

            return consume;
        }

        /// <summary>
        /// 時間切れであるかの判定
        /// </summary>
        /// <returns></returns>
        public bool IsTimeUp()
        {
            // 時間切れを負けにしない
            if (TimeSetting.IgnoreTime)
                return false;

            var rest = RestTime - new TimeSpan(0, 0, (int)(ElapsedTime() / 1000));

            // ここに秒読み時間も加算して負かどうかを調べる
            if (TimeSetting.ByoyomiEnable)
                rest += new TimeSpan(0, 0, TimeSetting.Byoyomi);

            return (rest <= TimeSpan.Zero);
        }

        /// <summary>
        /// タイマーの開始。持ち時間を消費していく。
        /// </summary>
        public void StartTimer()
        {
            startTime = Stopwatch.ElapsedMilliseconds;
            Stopwatch.Start();
        }

        /// <summary>
        /// タイマーの終了。
        /// </summary>
        public void StopTimer()
        {
            Stopwatch.Stop();
            endTime = Stopwatch.ElapsedMilliseconds;

            // ここで時間切れであるかどうかは、呼び出し元でIsTimeUp()で判定すべき。
        }

        /// <summary>
        /// StartTimer()～StopTimer()までの消費時間を繰り上げたもの
        /// 今回の消費時間
        /// </summary>
        /// <returns></returns>
        public TimeSpan ConsumptionTime()
        {
            return new TimeSpan(0,0,RoundUp(endTime - startTime));
        }

        /// <summary>
        /// StartTimer()を呼び出して、
        /// StopTimer()をまだ呼び出していない時の経過時刻
        /// </summary>
        /// <returns></returns>
        public long ElapsedTime()
        {
            return Stopwatch.ElapsedMilliseconds - startTime;
        }

        /// <summary>
        /// 1秒未満を繰り上げた経過時間[s]
        /// ConsumptionTime()やElapsedTime()を繰り上げるのに使う。
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        public int RoundUp(long elapsedTime)
        {
            return (int)((elapsedTime + 999) / 1000);
        }

        /// <summary>
        /// 残り時間を表現する時間を
        /// </summary>
        /// <returns></returns>
        public string DisplayShortString()
        {
            var elapsed = RoundUp(ElapsedTime());
            var r = RestTime - new TimeSpan(0, 0, elapsed);

            // 秒読みが有効でないなら、残りの時、分、秒だけを描画しておく。
            if (!TimeSetting.ByoyomiEnable || TimeSetting.Byoyomi == 0)
            {
                if (r < TimeSpan.Zero)
                    r = TimeSpan.Zero;
                return $"{r.Hours:D2}:{r.Minutes:D2}:{r.Seconds:D2}";
            }
            else
            {
                var rn = -r;
                if (r < TimeSpan.Zero)
                    r = TimeSpan.Zero;
                if (rn < TimeSpan.Zero)
                    rn = TimeSpan.Zero;

                return $"{r.Hours:D2}:{r.Minutes:D2}:{r.Seconds:D2} {rn.TotalSeconds}/{TimeSetting.Byoyomi}";
            }

        }

        /// <summary>
        /// 残り持ち時間
        /// </summary>

        public TimeSpan RestTime { get; set; }
        

        // -- private members

        /// <summary>
        /// 前回StartTimer()を呼び出した時刻
        /// </summary>
        private long startTime;

        /// <summary>
        /// 前回StopTimer()を呼び出した時刻
        /// </summary>
        private long endTime;

        /// <summary>
        /// 時間計測用のタイマー
        /// </summary>
        private Stopwatch Stopwatch;

        /// <summary>
        /// GameStart()した直後であるか。
        /// 初手はIncTimeしてはいけないのでそのためのフラグ
        /// </summary>
        private bool first;
    }
}
