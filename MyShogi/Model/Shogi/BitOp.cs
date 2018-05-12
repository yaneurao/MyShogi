using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// BitOperation一式
    /// </summary>
    public static class BitOp
    {
        /// <summary>
        /// 2進数で見て1になっている一番下位のbit位置を返し、そのbitを0にする。
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static int LSB64(ref UInt64 n)
        {
            // cf. Bit Twiddling Hacks : http://graphics.stanford.edu/~seander/bithacks.html

            // Count the consecutive zero bits (trailing) on the right with multiply and lookup
            UInt32 v = (UInt32)n;
            if (v != 0)
            {
                int r = MultiplyDeBruijnBitPosition[((UInt32)((v & -v) * 0x077CB531U)) >> 27];
                n ^= (1UL << r);
                return r;
            } else
            {
                v = (UInt32)(n >> 32);

                // 上位32bitのどこかが非0であるはず
                // assert(v != 0);

                int r = MultiplyDeBruijnBitPosition[((UInt32)((v & -v) * 0x077CB531U)) >> 27];
                r += 32;
                n ^= (1UL << r);
                return r;
            }
        }

        private static readonly int[] MultiplyDeBruijnBitPosition =
        {
          0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
          31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        /// for non-AVX2 : software emulationによるpext実装(やや遅い。とりあえず動くというだけ。)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static UInt64 PEXT64(UInt64 val , UInt64 mask)
        {
            UInt64 res = 0;
            for (UInt64 bb = 1; mask != 0 ; bb += bb)
            {
                if (((Int64)val & (Int64)mask & -(Int64)mask) != 0)
                    res |= bb;
                // マスクを1bitずつ剥がしていく実装なので処理時間がbit長に依存しない。
                // ゆえに、32bit用のpextを別途用意する必要がない。
                mask &= mask - 1;
            }
            return res;
        }
    }

    /// <summary>
    /// BitOpに関するextension methods
    /// </summary>
    public static class BitOpExtensions
    {
        /// <summary>
        /// 2進数として見たときに1になっているbitの数を数える。
        /// ソフトウェア実装なのでそこそこ遅い。
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int PopCount(this UInt64 n)
        {
            UInt32 n0 = (UInt32)n;
            UInt32 n1 = (UInt32)(n >> 32);

            // cf. Checking CPU Popcount from C# : https://stackoverflow.com/questions/6097635/checking-cpu-popcount-from-c-sharp?lq=1
            ulong result0 = n0 - ((n0 >> 1) & 0x5555555555555555UL);
            result0 = (result0 & 0x3333333333333333UL) + ((result0 >> 2) & 0x3333333333333333UL);
            var r0 = (int)(unchecked(((result0 + (result0 >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);

            ulong result1 = n1 - ((n1 >> 1) & 0x5555555555555555UL);
            result1 = (result1 & 0x3333333333333333UL) + ((result1 >> 2) & 0x3333333333333333UL);
            var r1 = (int)(unchecked(((result1 + (result1 >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);

            return r0 + r1;
        }
    }

}
