using System.Diagnostics;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// Model.Shogi用のヘルパークラス
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// 移動元、もしくは移動先の升のrankを与えたときに、そこが成れるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="fromOrToRank"></param>
        /// <returns></returns>
        public static bool CanPromote(Color c, Rank fromOrToRank)
        {
            // ASSERT_LV1(is_ok(c) && is_ok(fromOrToRank));
            // 先手9bit(9段) + 後手9bit(9段) = 18bitのbit列に対して、判定すればいい。
            // ただし ×9みたいな掛け算をするのは嫌なのでbit shiftで済むように先手16bit、後手16bitの32bitのbit列に対して判定する。
            // このcastにおいて、VC++2015ではwarning C4800が出る。
            return (0x1c00007u & (1 << ((c.ToInt() << 4) + fromOrToRank.ToInt()))) != 0;
        }

        /// <summary>
        // 移動元、もしくは移動先の升sqを与えたときに、そこが成れるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="fromOrTo"></param>
        /// <returns></returns>
        public static bool CanPromote(Color c, Square fromOrTo)
        {
        	return CanPromote(c, fromOrTo.ToRank());
        }

        /// <summary>
        /// 筋と段から升を表す値を返す。
        /// </summary>
        /// <param name="f"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Square MakeSquare(File f , Rank r)
        {
            return (Square)(f.ToInt() * 9 + r.ToInt());
        }

        /// <summary>
        /// pcとして先手の駒を渡し、cが後手なら後手の駒を返す。cが先手なら先手の駒のまま。
        /// pcとしてNO_PIECEは渡してはならない。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Piece MakePiece(Color c , Piece pt)
        {
            Debug.Assert(pt.PieceColor() == Color.BLACK && pt != Piece.NO_PIECE);
            return (Piece)((c.ToInt() << 4) + pt.ToInt());
        }
    }

}
