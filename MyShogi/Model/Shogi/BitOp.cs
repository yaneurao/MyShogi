using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi
{
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

    }
}
