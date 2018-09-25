using MyShogi.Model.Common.Collections;
using System;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// Exceptionに対して整形などをするためのExtensions
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// 人が見てわかる形に整形する。
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string Pretty(this Exception ex)
        {
            if (ex.StackTrace.Empty())
                return $"例外内容 : {ex.Message}\r\n";

            var stackTrace = ex.StackTrace;
            return $"例外内容 : {ex.Message}\r\nスタックトレース : \r\n{stackTrace}\r\n";
        }
    }
}
