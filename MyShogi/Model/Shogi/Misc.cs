using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 疑似乱数生成
    /// やねうら王で用いている疑似乱数と同一にしておく。
    /// </summary>
    public class PRNG
    {
        public PRNG(UInt64 seed) { s = seed; }

        /// <summary>
        /// 時刻などでseedを初期化する。
        /// </summary>
        public PRNG()
        {
            // time値とか、thisとか色々加算しておく。
            s = (UInt64)DateTime.Now.ToBinary();
        }

        /// <summary>
        /// 乱数を一つ取り出す。
        /// </summary>
        /// <returns></returns>
        public UInt64 Rand() { return rand64(); }

        /// <summary>
        /// 0からn-1までの乱数を返す。(一様分布ではないが現実的にはこれで十分)
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public UInt64 Rand(UInt64 n) { return Rand() % n; }

        /// <summary>
        /// 内部で使用している乱数seedを返す。
        /// </summary>
        /// <returns></returns>
        public UInt64 GetSeed() { return s;  }

	    private UInt64 s;
        private UInt64 rand64()
        {
            s ^= s >> 12;
            s ^= s << 25;
            s ^= s >> 27;
            return s * 2685821657736338717UL;
        }
    }
}
