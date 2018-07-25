using System;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 評価値として用いる値。
    /// 探索部を用意していないので、表示に使う程度であるが…。
    ///
    /// MatePlus = -MatedMinus
    /// Mate = -Mated
    ///
    /// のように符号を反転させると先後入替えた側から見た評価値になることが保証されている。
    /// </summary>
    public enum EvalValue : Int32
    {
        // "score mate+"を表現する手数不明の詰み。
        MatePlus = Int32.MaxValue - 1,

        // 現局面で(敵玉が)詰んでいる時の評価値
        // N手詰めのときは、(Mate - N)
        Mate = Int32.MaxValue - 2,

        Zero = 0,

        // 現局面で(自玉が)詰んでいる時の評価値
        // N手で詰まされるときは、(Mate + N)
        Mated = Int32.MinValue + 3,

        // "score mate-"を表現する手数不明の詰まされ。
        MatedMinus = Int32.MinValue + 2,

        // この局面の評価値が存在しないことを意味する値
        // 形勢グラフなどには、この値のところは描画してはならない。
        NoValue = Int32.MinValue + 1,

        // この値は使わない。
        Unknown = Int32.MinValue,
    }

    /// <summary>
    /// ある評価値が、探索のupperbound(上界) , lowerbound(下界)の値であるかなどを表現する。
    /// </summary>
    public enum ScoreBound
    {
        /// <summary>
        /// 上界
        /// </summary>
        Upper ,

        /// <summary>
        /// 下界
        /// </summary>
        Lower ,

        /// <summary>
        /// ぴったり
        /// </summary>
        Exact ,
    }

    /// <summary>
    /// EvalValueの値とScoreBoundの値を一纏めにした構造体
    /// </summary>
    public class EvalValueEx
    {
        public EvalValueEx(EvalValue eval , ScoreBound bound)
        {
            Eval = eval;
            Bound = bound;
        }

        public EvalValue Eval;
        public ScoreBound Bound;

        /// <summary>
        /// 評価値を反転する。
        /// </summary>
        public EvalValueEx negate()
        {
            EvalValue eval = (Eval != EvalValue.Unknown && Eval != EvalValue.NoValue) ? (EvalValue)(-(Int32)Eval) : Eval;
            ScoreBound bound;
            switch (Bound)
            {
                case ScoreBound.Upper: bound = ScoreBound.Lower; break;
                case ScoreBound.Lower: bound = ScoreBound.Upper; break;
                default: bound = Bound; break;
            }
            return new EvalValueEx(eval, bound);
        }
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
                    case EvalValue.Unknown   : return "不明";
                    case EvalValue.MatePlus  : return "MATE(手数不明)";
                    case EvalValue.MatedMinus: return "MATED(手数不明)";
                    case EvalValue.NoValue   : return ""; // これ表示するとおかしくなるので表示なしにしとく。
                }

                // int にキャストしないと 0手 が Zero手 と出力される
                if (value > 0)
                    return $"MATE({(int)(EvalValue.Mate - value)}手)";
                if (value < 0)
                    return $"MATED({(int)(value - EvalValue.Mated)}手)";
            }

            // 0以外は符号付きで出力
            return ((int)value).ToString("+0;-0;0");
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

        /// <summary>
        /// ScoreBoundの値を文字列で表現する。
        ///
        /// Chessの記法に倣う。
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        public static string Pretty(this ScoreBound bound)
        {
            switch(bound)
            {
                case ScoreBound.Exact: return "";
                case ScoreBound.Upper: return "++";
                case ScoreBound.Lower: return "--";
            }
            return "??";
        }

    }
}
