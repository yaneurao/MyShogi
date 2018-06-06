using System;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// USIエンジン用の例外クラスです。
    /// </summary>
    public class UsiEngineException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiEngineException()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiEngineException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiEngineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
