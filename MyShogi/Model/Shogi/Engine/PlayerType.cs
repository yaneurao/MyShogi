using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi.Engine
{
    /// <summary>
    /// プレイヤーの種類を表す定数。
    /// 人間も思考エンジンも同一のインターフェースで取り扱う。
    /// </summary>
    public enum PlayerType
    {
        /// <summary>
        /// ダミーエンジンです。
        /// </summary>
        Null,

        /// <summary>
        /// 人間が代わりに操作します。
        /// </summary>
        Human,
        
        /// <summary>
        /// USIプロトコルでやりとりするエンジン。
        /// </summary>
        UsiEngine,
    }
}
