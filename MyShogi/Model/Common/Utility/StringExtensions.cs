namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// string型に対する、LeftやRightを提供するextensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 左からn文字切り出して返す。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string Left(this string s , int n)
        {
            return s == null ? null : s.Substring(0, System.Math.Min(s.Length, 6));
        }

        /// <summary>
        /// 右からn文字切り出して返す
        /// 
        ///  // 未デバッグ
        ///  
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string Right(this string s , int n)
        {
            if (s == null)
                return null;
            int m = s.Length - n; // 切り出す文字数 
            if (m < 0)
                m = 0;
            return s.Substring(s.Length - m , m);
        }

        // 他、また気が向いたら書く。

        /// <summary>
        /// string.PadLeft()と同じだが、全角スペースは2文字分として扱ってPadLeftする。
        /// n : 半角何文字分にして返すか。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string PadLeftUnicode(this string s , int n)
        {
            // まず全角を1文字として文字数を数える
            int len = 0;
            foreach (var c in s)
                len += (c < 256) ? 1 : 2;

            // 全角文字の数だけ減らしてPadLeft()する。
            return s.PadLeft(System.Math.Max(n - (len - s.Length), 0));
        }

        /// <summary>
        /// string.PadRight()と同じだが、全角スペースは2文字分として扱ってPadRightする。
        /// n : 半角何文字分にして返すか。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string PadRightUnicode(this string s, int n)
        {
            // まず全角を1文字として文字数を数える
            int len = 0;
            foreach (var c in s)
                len += (c < 256) ? 1 : 2;

            // 全角文字の数だけ減らしてPadRight()する。
            return s.PadRight(System.Math.Max(n - (len - s.Length), 0));
        }


        /// <summary>
        /// s と tの間に半角スペースを、全体が半角n文字になるようにpaddingする。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string PadMidUnicode(this string s, string t , int n)
        {
            // まず全角を1文字として全体の文字数を数える
            int len = 0;
            foreach (var c in s)
                len += (c < 256) ? 1 : 2;
            foreach (var c in t)
                len += (c < 256) ? 1 : 2;

            // n - lenの数だけスペースを放り込む
            return $"{s}{new string(' ',System.Math.Max(n - len, 0))}{t}";
        }

    }
}
