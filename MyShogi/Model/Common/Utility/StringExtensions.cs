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
    }
}
