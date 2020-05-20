using System.Text;

namespace MyShogi.Model.Common.Collections
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
        /// string.IsNullOrEmpty()のショートカット。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool Empty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        // これ用意するとLinqのほうとごっちゃになるのでやめとく。
#if false
        /// <summary>
        /// Empty()の否定
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool Any(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
#endif

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
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string Right(this string s , int n)
        {
            if (s == null || n <= 0)
                return null;

            return s.Substring(s.Length - n , n);
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

        /// <summary>
        /// LeftUnicode()と同等だが、sの文字数がn - t.UnicodeLengthを超えたときにはtを出力する(「..」などを出力したい時に用いる)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string LeftUnicode(this string s, int n , string t)
        {
            n -= t.UnicodeLength();
            var length = s.UnicodeLength();
            return (length <= n) ?
                s.LeftUnicode(n):
                $"{s.LeftUnicode(n)}{t}";
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
        /// 全角文字を2文字、半角文字を1文字としてカウントするLength
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int UnicodeLength(this string s)
        {
            if (s == null)
                return 0;

            int n = 0;
            foreach (var c in s)
                n += (c < 256) ? 1 : 2;
            return n;
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

        /// <summary>
        /// 全角文字を2文字、半角文字を1文字としてカウントして、lengthごとに tを挿入する。
        /// 改行文字列が入っていないstringに対して、改行文字列を一定文字数ごとに挿入したいときなどに用いる。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string UnicodeInsertEvery(this string s , string t , int length)
        {
            var sb = new StringBuilder();
            var len = 0;
            var next_len = length;
            foreach (var c in s)
            {
                len += (c < 256) ? 1 : 2;
                sb.Append(c);

                // tの挿入位置になったのか？
                if (len >= next_len)
                {
                    sb.Append(t);
                    next_len += length;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 文字列を整数化する。ただし、整数化に失敗した場合は、引数で指定されているdefaultValueの値を返す。
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(this string s , int defaultValue)
        {
            int result;
            return int.TryParse(s, out result) ? result : defaultValue;
        }
    }
}
