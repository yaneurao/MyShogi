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

        // 他、また気が向いたら書く。
    }
}
