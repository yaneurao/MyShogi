using System;

namespace MyShogi.Model.Shogi.Core
{

    // --------------------
    //   壁つきの升表現
    // --------------------

    // This trick is invented by yaneurao in 2016.

    // 長い利きを更新するときにある升からある方向に駒にぶつかるまでずっと利きを更新していきたいことがあるが、
    // sqの升が盤外であるかどうかを判定する簡単な方法がない。そこで、Squareの表現を拡張して盤外であることを検出
    // できるようにする。

    // bit 0..7   : Squareと同じ意味
    // bit 8      : Squareからのborrow用に1にしておく
    // bit 9..13  : いまの升から盤外まで何升右に升があるか(ここがマイナスになるとborrowでbit13が1になる)
    // bit 14..18 : いまの升から盤外まで何升上に(略
    // bit 19..23 : いまの升から盤外まで何升下に(略
    // bit 24..28 : いまの升から盤外まで何升左に(略
    public enum SquareWithWall : Int32
    {
        // 相対移動するときの差分値
        SQWW_R = Square.SQ_R - (1 << 9) + (1 << 24),
        SQWW_U = Square.SQ_U - (1 << 14) + (1 << 19),
        SQWW_D = -(SQWW_U),
        SQWW_L = -(SQWW_R),
        SQWW_RU = (SQWW_R) + (SQWW_U),
        SQWW_RD = (SQWW_R) + (SQWW_D),
        SQWW_LU = (SQWW_L) + (SQWW_U),
        SQWW_LD = (SQWW_L) + (SQWW_D),

        // SQ_11の地点に対応する値(他の升はこれ相対で事前に求めテーブルに格納)
        SQWW_11 = Square.SQ_11 | (1 << 8) /* bit8 is 1 */ | (0 << 9) /*右に0升*/ |
            (0 << 14) /*上に0升*/ | (8 << 19) /*下に8升*/ | (8 << 24) /*左に8升*/,

        // SQWW_RIGHTなどを足して行ったときに盤外に行ったときのborrow bitの集合
        SQWW_BORROW_MASK = (1 << 13) | (1 << 18) | (1 << 23) | (1 << 28),
    };


    public static class SquareWithWallExtensions
    {
        /// <summary>
        /// int型への変換
        /// </summary>
        /// <param name="sqww"></param>
        /// <returns></returns>
        public static Int32 ToInt(this SquareWithWall sqww)
        {
            return (int)sqww;
        }

        /// <summary>
        /// Square型への型変換
        /// </summary>
        /// <param name="sqww"></param>
        /// <returns></returns>
        public static Square ToSquare(this SquareWithWall sqww)
        {
            return (Square)(sqww.ToInt() & 0xff);
        }

        /// <summary>
        /// 盤内か。壁(盤外)だとfalseになる。
        /// </summary>
        /// <param name="sqww"></param>
        /// <returns></returns>
        public static bool IsOk(this SquareWithWall sqww)
        {
            return (sqww.ToInt() & (int)SquareWithWall.SQWW_BORROW_MASK) == 0;
        }

        /// <summary>
        /// 型変換。Square型からSquareWithWall型に。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static SquareWithWall ToSqww(this Square sq) { return sqww_table[sq.ToInt()]; }

        /// <summary>
        /// ToSqww()で必要となるテーブル
        /// [SQ_NB_PLUS1]まで
        /// Bitboard.init()で初期化される。
        /// </summary>
        public static SquareWithWall[] sqww_table = new SquareWithWall[(int)Square.NB_PLUS1];

        /// <summary>
        /// SQの示す升を出力する
        /// </summary>
        /// <param name="sqww"></param>
        /// <returns></returns>
        public static string Pretty(this SquareWithWall sqww)
        {
            return sqww.ToSquare().Pretty();
        }
    }

}
