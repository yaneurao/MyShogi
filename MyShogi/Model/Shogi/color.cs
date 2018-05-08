namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 先手・後手という手番を表す定数
    /// </summary>
    public enum Color : int
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
        /// 手番を相手の手番に変更する。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color Flip(this Color color)
        {
            return (Color)((int)color ^ 1);
        }

    }

}
