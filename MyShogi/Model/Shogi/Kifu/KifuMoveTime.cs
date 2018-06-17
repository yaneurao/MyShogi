using MyShogi.Model.Shogi.Core;
using System;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 1手の消費時間、その指し手の時点での残り持ち時間、総消費時間を表現する。
    /// 片側のプレイヤー分。
    /// 
    /// immutable object。
    /// </summary>
    public class KifuMoveTime
    {
        /// <summary>
        /// 今回の指し手の消費時間
        /// ・1秒未満1秒に繰り上げ。
        /// ・1秒以上は秒未満繰り下げ。
        /// 計測秒。
        /// 
        /// immutable objectっぽく使うこと。
        /// </summary>
        public TimeSpan ThinkingTime { get; }

        /// <summary>
        /// 今回の実際の消費時間。ミリ秒まで保持している。
        /// </summary>
        public TimeSpan RealThinkingTime { get; }

        /// <summary>
        /// 総消費時間
        /// </summary>
        public TimeSpan TotalTime { get; }

        /// <summary>
        /// 残り持ち時間
        /// </summary>
        public TimeSpan RestTime { get; }

        public KifuMoveTime()
        {
            ThinkingTime = TimeSpan.Zero;
            RealThinkingTime = TimeSpan.Zero;
            TotalTime = TimeSpan.Zero;
            RestTime = TimeSpan.Zero;
        }

        public KifuMoveTime Clone()
        {
            return (KifuMoveTime)MemberwiseClone();
        }

        /// <summary>
        /// コンストラクタ
        /// 今回の消費時間、実消費時間、トータルの消費時間、残り時間を引数に渡す。
        /// </summary>
        /// <param name="ConsumptionTime_"></param>
        /// <param name="TotalTime_"></param>
        /// <param name="RestTime_"></param>
        public KifuMoveTime(TimeSpan ThinkingTime_, TimeSpan RealThinkingTime_, TimeSpan TotalTime_, TimeSpan RestTime_)
        {
            ThinkingTime = ThinkingTime_;
            RealThinkingTime = RealThinkingTime_;
            TotalTime = TotalTime_;
            RestTime = RestTime_;
        }
    }

    /// <summary>
    /// KifuMoveTimeの二人分
    /// 
    /// 各KifuNodeにおいてその指し手の消費時間、その時点での残り時間・総消費時間を
    /// 記録していくためのクラス。(待ったをした時などにその時点に戻す必要があるため)
    /// </summary>
    public class KifuMoveTimes
    {
        public KifuMoveTimes(KifuMoveTime black, KifuMoveTime white)
        {
            Players = new KifuMoveTime[2] { black, white };
        }

        /// <summary>
        /// c側のプレイヤー分
        /// </summary>
        /// <param name="c"></param>
        public KifuMoveTime Player(Color c) { return Players[(int)c]; }

        public KifuMoveTime[] Players;

        /// <summary>
        /// ゼロに相当するobject
        /// </summary>
        public static KifuMoveTimes Zero = new KifuMoveTimes(new KifuMoveTime(), new KifuMoveTime());
    }

}
