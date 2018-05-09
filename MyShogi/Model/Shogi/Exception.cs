using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// sfen形式のデータの読み込み時に発生する例外
    /// </summary>
    public class SfenException : Exception
    {
        public SfenException(){ } 

        public SfenException(string msg) : base(msg) { }

        public SfenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// PositionのDoMove()などで発生する例外
    /// </summary>
    public class PositionException : Exception
    {
        public PositionException() { }

        public PositionException(string msg) : base(msg) { }

        public PositionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}
