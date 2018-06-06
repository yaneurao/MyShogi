using System;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// USIプロトコル用の例外クラスです。
    /// </summary>
    public class UsiException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiException()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
