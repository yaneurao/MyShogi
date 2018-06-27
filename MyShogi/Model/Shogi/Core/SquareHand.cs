using System.Diagnostics;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 盤上の升と手駒の位置を表現する型
    /// 
    /// Square型だと手駒の場所が表現できないので、マウスでクリックした駒を表現するのに表現力が不足している。
    /// このため、手駒も含めて表現できる型が必要となる。
    /// </summary>
    public enum SquareHand
    {
        // 0～80まではSquare型と同じ
        SquareZero = 0,     // 盤上の升
        SquareNB = 81,      // 盤上の升の終端+1

        // 手駒
        Hand = 81,          // 手駒のスタート
        HandBlack = 81,     // 先手の手駒のスタート
        HandWhite = 81 + 7, // 後手の手駒のスタート
        HandNB = 81 + 14,   // 手駒の終端+1

        // 駒箱

        PieceBox = HandNB,        // 駒箱の駒(歩、香、桂、銀、角、飛、金、玉の8種)
        PieceBoxNB = PieceBox + 8,// 駒箱の終端+1

        // ゼロと末尾
        ZERO = 0, NB = PieceBoxNB,
    }

    /// <summary>
    /// Square型のためのextension methods
    /// </summary>
    public static class SquareHandExtensions
    {
        /// <summary>
        /// 値の範囲が正常か調べる。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static bool IsOk(this SquareHand sq)
        {
            return SquareHand.ZERO <= sq && sq <= SquareHand.NB;
        }

        /// <summary>
        /// sqに対してどちらのColorの手駒を表現しているのかを返す。
        /// 盤上、駒箱の升に対して呼び出してはならない。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Color PieceColor(this SquareHand sq)
        {
            Debug.Assert(IsHandPiece(sq));

            return (sq < SquareHand.HandWhite) ? Color.BLACK : Color.WHITE;
        }

        /// <summary>
        /// 盤上の升であるかを判定する。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static bool IsBoardPiece(this SquareHand sq)
        {
            return sq < SquareHand.SquareNB;
        }

        /// <summary>
        /// 手駒であるかを判定する
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static bool IsHandPiece(this SquareHand sq)
        {
            return SquareHand.Hand <= sq && sq < SquareHand.HandNB;
        }

        /// <summary>
        /// 駒箱の駒であるか判定する
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static bool IsPieceBox(this SquareHand sq)
        {
            return SquareHand.PieceBox <= sq && sq < SquareHand.PieceBoxNB;
        }

        /// <summary>
        /// sqの手駒に対して、その駒種を返す
        /// sqは手駒か駒箱の駒でないといけない。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns>
        /// 返し値について先後の区別はない。
        /// 手駒に対しては、Piece.PAWN ～ Piece.GOLDまでの値が返る。
        /// 駒箱の駒に対しては、Piece.PAWN ～ Piece.KINGまでの値が返る。
        /// </returns>
        public static Piece ToPiece(this SquareHand sq)
        {
            Debug.Assert(! IsBoardPiece(sq) );

            if (IsHandPiece(sq))
                return (sq < SquareHand.HandWhite)
                    ? (Piece)((sq - SquareHand.HandBlack) + Piece.PAWN)
                    : (Piece)((sq - SquareHand.HandWhite) + Piece.PAWN);
            // is BoxPiece
            return (Piece)((sq - SquareHand.PieceBox) + Piece.PAWN);

        }

        /// <summary>
        /// Squareを綺麗に出力する(USI形式ではない)
        /// 日本語文字での表示になる。例 → ８八 , 先手歩 (先手の手駒の歩)
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static string Pretty(this SquareHand sq)
        {
            var c = PieceColor(sq);

            if (c == Color.NB)
                return ((Square)sq).Pretty();

            return c.Pretty() + ToPiece(sq).Pretty();
        }

    }

    public static partial class Util
    {
        /// <summary>
        /// 引数で指定したColorとPieceに相当するSquareHand型の値を生成する。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static SquareHand ToSquareHand(Color c , Piece pc)
        {
            Debug.Assert(Piece.PAWN <= pc && pc < Piece.KING);
            return 
                (SquareHand)((int)(c == Color.BLACK ? SquareHand.HandBlack : SquareHand.HandWhite) + pc - Piece.PAWN);
        }

    }

}