using System.Text;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// string型に対する、LeftやRightを提供するextensions
    /// 
    /// nullに対して呼び出してもnullを返す。
    /// (System.Text.PadLeft()などはこの仕様になっていないので呼び出す前のnullチェックが必要になって使いづらい。)
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
            if (s == null)
                return null;

            return s.Substring(0, System.Math.Min(s.Length, n));
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

        /// <summary>
        /// n文字目以降をm文字切り出す。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string Mid(this string s , int n , int m)
        {
            if (s == null)
                return null;

            // 切り出し始める箇所がsの長さを超えている
            if (s.Length < n)
                return string.Empty;

            // 切り出す文字数が多すぎてsの末尾を超えている
            if (s.Length < n + m)
                return s.Substring(n);

            return s.Substring(n,m);
        }


        /// <summary>
        /// string.Left()と同じだが、全角スペースは2文字分として扱ってLeftする。
        /// n : 半角何文字分にして返すか。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string LeftUnicode(this string s, int n)
        {
            if (s == null)
                return null;

            var sb = new StringBuilder();
            foreach (var c in s)
            {
                n -= (c < 256) ? 1 : 2;
                if (n < 0)
                    return sb.ToString();
                sb.Append(c);
            }
            return s; // 文字列丸ごとが、nの範囲に収まった。
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
            if (s == null)
                return null;

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
            if (s == null)
                return null;

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
            if (s == null)
                return null;

            // まず全角を1文字として全体の文字数を数える
            int len = 0;
            foreach (var c in s)
                len += (c < 256) ? 1 : 2;
            foreach (var c in t)
                len += (c < 256) ? 1 : 2;

            // n - lenの数だけスペースを放り込む
            return $"{s}{new string(' ',System.Math.Max(n - len, 0))}{t}";
        }

        /// <summary>
        /// 先頭の1文字を返す。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static char FirstChar(this string s)
        {
            if (s == null || s.Length == 0)
                return (char)0;

            return s[0];
        }
    }
}
