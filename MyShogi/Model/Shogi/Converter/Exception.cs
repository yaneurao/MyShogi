using System;

namespace MyShogi.Model.Shogi.Converter
{
    /// <summary>
    /// Converterの処理時に発生する例外
    /// </summary>
    public class ConverterException : Exception
    {
        public ConverterException() {}

        public ConverterException(string msg) : base(msg) {}

        public ConverterException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }

}
