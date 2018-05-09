using System;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 先手・後手という手番を表す定数
    /// </summary>
    public enum Color : UInt32
    {
        BLACK = 0,
        WHITE = 1,

        ZERO = 0,
        NB = 2,
    }

    /// <summary>
    /// Colorに関するextension methodsを書いておくクラス
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// 正常な値であるかを検査する。assertで使う用。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsOk(this Color c)
        {
            return Color.ZERO <= c && c < Color.NB;
        }

        /// <summary>
        /// 日本語文字列に変換する。(USI文字列ではない)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string Pretty(this Color c)
        {
            return (c == Color.BLACK) ? "先手" : "後手";
        }

        /// <summary>
        /// USI形式で手番を出力する
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ToUSI(this Color c)
        {
            return (c == Color.BLACK) ? "b" : "w";
        }

        /// <summary>
        /// UInt32型に変換する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static UInt32 ToInt(this Color c)
        {
            return (UInt32)c;
        }

        /// <summary>
        /// 手番を相手の手番に変更する。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color Flip(this Color color)
        {
            return (Color)(color.ToInt() ^ 1);
        }

    }

}
