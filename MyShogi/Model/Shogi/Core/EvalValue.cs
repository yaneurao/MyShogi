using System;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 評価値として用いる値。
    /// 探索部を用意していないので、表示に使う程度であるが…。
    /// 
    /// Mateの時は以下の値になる。
    /// 
    ///   Mate 0 = int.MaxValue - 1
    ///   Mate 1 = int.MaxValue - 2
    ///    ..
    ///   Mate -2 = int.MinValue  +3
    ///   Mate -1 = int.MinValue  +2
    /// </summary>
    public enum EvalValue : Int32
    {
        // 現局面で(敵玉が)詰んでいる時の評価値
        ValueMate = Int32.MaxValue -1,

        Zero = 0,

        // 現局面で(自玉が)詰んでいる時の評価値
        ValueMated = Int32.MinValue +1,

        // この局面の評価値が存在しないことを意味する値
        // 形勢グラフなどには、この値のところは描画してはならない。
        NoValue = Int32.MaxValue,
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
            // 大きな数はmateを意味しているはず..
            if ((int)value > 1000000)
                return $"MATE({EvalValue.ValueMate - (int)value}手)";
            if ((int)value < -1000000)
                return $"MATED({value - EvalValue.ValueMated}手)";

            return value.ToString();
        }
    }
}
