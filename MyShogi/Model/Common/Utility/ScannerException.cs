using System;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// 解析失敗時に投げられる例外です。
    /// </summary>
    public class ScannerException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScannerException()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScannerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScannerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
