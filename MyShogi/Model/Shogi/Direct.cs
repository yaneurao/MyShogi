using System;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 方角を表す。遠方駒の利きや、玉から見た方角を表すのに用いる。
    /// bit0..右上、bit1..右、bit2..右下、bit3..上、bit4..下、bit5..左上、bit6..左、bit7..左下
    /// 同時に複数のbitが1であることがありうる。
    /// </summary>
    public enum Directions : Byte
    {
        ZERO = 0, RU = 1, R = 2, RD = 4,
        U = 8, D = 16, LU = 32, L = 64, LD = 128,
        CROSS = U | D | R | L,
        DIAG = RU | RD | LU | LD,
    }

    /// <summary>
    /// Directionsをpopしたもの。複数の方角を同時に表すことはない。
    /// おまけで桂馬の移動も追加しておく。
    /// </summary>
    public enum Direct : Byte
    {
        RU, R, RD, U, D, LU, L, LD,
        NB, ZERO = 0, RUU = 8, LUU, RDD, LDD, NB_PLUS4
    };

    /// <summary>
    /// Direct,Directionsに関するextension methods
    /// </summary>
    public static class DirectExtensions {
        /// <summary>
        /// DirectからDirectionsへの逆変換
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Directions ToDirections(this Direct d)
        {
            return (Directions)(1 << (int)d);
        }

        public static SquareWithWall ToDeltaWW(this Direct d)
        {
            /* ASSERT_LV3(is_ok(d)); */
            return Util.DirectToDeltaWW_[(int)d];
        }
    }

    public partial class Util
    {
        /// <summary>
        /// DirectionsOf()で使われるテーブル。
        /// Bitboard.Init()で初期化される。
        /// </summary>
        public static Directions[,] direc_table; // = new Directions[(int)Square.NB_PLUS1 , (int)Square.NB_PLUS1];

        /// <summary>
        /// DirectをSquareWithWall型の差分値で表現したもの。
        /// ToDeltaWW(this Direct d)で用いる。
        /// Bitboard.Init()で初期化される。
        /// </summary>
        public static SquareWithWall[] DirectToDeltaWW_  =
            { SquareWithWall.SQWW_RU , SquareWithWall.SQWW_R  , SquareWithWall.SQWW_RD , SquareWithWall.SQWW_U,
              SquareWithWall.SQWW_D  , SquareWithWall.SQWW_LU , SquareWithWall.SQWW_L  , SquareWithWall.SQWW_LD, };

        /// <summary>
        /// sq1にとってsq2がどのdirectionにあるか。
        /// "Direction"ではなく"Directions"を返したほうが、縦横十字方向や、斜め方向の位置関係にある場合、
        /// DIRECTIONS_CROSSやDIRECTIONS_DIAGのような定数が使えて便利。
        /// </summary>
        /// <param name="sq1"></param>
        /// <param name="sq2"></param>
        /// <returns></returns>
        public static Directions DirectionsOf(Square sq1, Square sq2) { return direc_table[(int)sq1,(int)sq2]; }

        /// <summary>
        /// 与えられた3升が縦横斜めの1直線上にあるか。駒を移動させたときに開き王手になるかどうかを判定するのに使う。
        /// 例) 王がsq1, pinされている駒がsq2にあるときに、pinされている駒をsq3に移動させたときにaligned(sq1,sq2,sq3)であれば、
        ///  pinされている方向に沿った移動なので開き王手にはならないと判定できる。
        /// ただし玉はsq3として、sq1,sq2は同じ側にいるものとする。(玉を挟んでの一直線は一直線とはみなさない)
        /// </summary>
        /// <param name="sq1"></param>
        /// <param name="sq2"></param>
        /// <param name="sq3"></param>
        /// <returns></returns>
        public static bool IsAligned(Square sq1, Square sq2, Square sq3/* is ksq */)
        {
            var d1 = DirectionsOf(sq1, sq3);
            return d1!=Directions.ZERO ? d1 == DirectionsOf(sq2, sq3) : false;
        }

    }

#if false

    // Directionsに相当するものを引数に渡して1つ方角を取り出す。
    inline Direct pop_directions(Directions& d) { return (Direct)pop_lsb(d); }
#endif
}
