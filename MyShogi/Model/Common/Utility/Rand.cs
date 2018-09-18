
using MyShogi.Model.Shogi.Core;
using System;

namespace MyShogi.Model.Common.Utility
{
    public class Rand
    {
        public Rand(UInt64 seed) { s = seed; }
        public Rand()
        {
            // 時刻などでseedを初期化する。
            s = (UInt32)System.Environment.TickCount;

            // 32bit seedなので10回ほど回して均しておく。
            foreach (var i in All.Int(10))
                rand64();
        }

        /// <summary>
        /// 0からmax-1までの乱数を生成して返す。
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Next(int max)
        {
            return (int)rand.Next64((UInt64)max);
        }

        /// <summary>
        /// bool型の乱数を得る。
        /// (trueかfalseが、それぞれ1/2の確率で返ってくる)
        /// </summary>
        /// <returns></returns>
        public static bool NextBool()
        {
            return Next(2) != 0;
        }

        /// <summary>
        /// 
        /// 0からn-1までの乱数を返す。(一様分布ではないが現実的にはこれで十分)
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        UInt64 Next64(UInt64 n) { return rand64() % n; }

        /// <summary>
        /// 64bitの乱数の生成。Stockfishで使われているもの。
        /// </summary>
        /// <returns></returns>
        private UInt64 rand64()
        {
            s ^= s >> 12;
            s ^= s << 25;
            s ^= s >> 27;
            return s * 2685821657736338717L;
        }

        // 内部で使用している乱数のseed
        private UInt64 s;

        /// <summary>
        /// singleton instance
        /// </summary>
        private static Rand rand = new Rand();
    }
}
