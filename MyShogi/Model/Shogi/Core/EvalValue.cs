using System;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 評価値として用いる値。
    /// 探索部を用意していないので、表示に使う程度であるが…。
    /// </summary>
    public enum EvalValue : Int32
    {
        // この値は使わない。
        Unknow = Int32.MaxValue,

        // "score mate+"を表現する手数不明の詰み。
        MatePlus = Int32.MaxValue - 1,

        // 現局面で(敵玉が)詰んでいる時の評価値
        // N手詰めのときは、(ValueMate - N)
        Mate = Int32.MaxValue -2,

        Zero = 0,

        // 現局面で(自玉が)詰んでいる時の評価値
        // N手で詰まされるときは、(ValueMate + N)
        Mated = Int32.MinValue +2,

        // "score mate-"を表現する手数不明の詰まされ。
        MatedMinus = Int32.MinValue +1,

        // この局面の評価値が存在しないことを意味する値
        // 形勢グラフなどには、この値のところは描画してはならない。
        NoValue = Int32.MinValue,
    }

    public static class EvalValueExtensions
    {
        /// <summary>
        /// 評価値の値をわかりやすく文字列化する。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Pretty(this EvalValue value)
        {
            if (value.IsSpecialValue())
            {
                switch(value)
                {
                    case EvalValue.Unknow    : return "不明";
                    case EvalValue.MatePlus  : return "MATE(手数不明)";
                    case EvalValue.MatedMinus: return "MATED(手数不明)";
                    case EvalValue.NoValue   : return ""; // これ表示するとおかしくなるので表示なしにしとく。 
                }

                if (value > 0)
                    return $"MATE({EvalValue.Mate - (int)value}手)";
                if (value < 0)
                    return $"MATED({value - EvalValue.Mated}手)";
            }

            return value.ToString();
        }

        /// <summary>
        /// EvalValueが通常の評価値の値ではなく、特殊な意味を持つ値であるかを判定する。
        /// ※　通常の評価値の値は -1000000 ～ +1000000までであるものとする。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsSpecialValue(this EvalValue value)
        {
            return !(-1000000 <= (int)value && (int)value <= +1000000);
        }
    }
}
