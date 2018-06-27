using System.Diagnostics;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 盤上の升と手駒の位置を表現する型
    /// 
    /// Square型だと手駒の場所が表現できないので、マウスでクリックした駒を表現するのに表現力が不足している。
    /// このため、手駒も含めて表現できる型が必要となる。
    /// 
    /// 例)
    /// Hand : 先手の駒台の駒以外の升の領域を表現する
    /// HandBlack + Piece.PAWN = 先手の駒台の歩
    /// 同様に
    /// Piece pcに対して PieceBox+(int)pc は駒箱のpc
    /// </summary>
    public enum SquareHand
    {
        // 0～80まではSquare型と同じ
        SquareZero = 0,     // 盤上の升
        SquareNB = 81,      // 盤上の升の終端+1

        // 手駒
        Hand = 81,          // 手駒のスタート
        HandBlack = 81,     // 先手の手駒のスタート
        HandWhite = 81 + 8, // 後手の手駒のスタート
        HandNB = 81 + 16,   // 手駒の終端+1

        // 駒箱

        PieceBox = HandNB,        // 駒箱の駒(NO_PIECE、歩、香、桂、銀、角、飛、金、玉の9種)
        PieceBoxNB = PieceBox + 9,// 駒箱の終端+1

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
        /// 手駒に対しては、Piece.NO_PIECE ～ Piece.GOLDまでの値が返る。(Piece.KINGは返らない)
        /// 駒箱の駒に対しては、Piece.NO_PIECE ～ Piece.KINGまでの値が返る。
        /// </returns>
        public static Piece ToPiece(this SquareHand sq)
        {
            Debug.Assert(! IsBoardPiece(sq) );

            if (IsHandPiece(sq))
                return (sq < SquareHand.HandWhite)
                    ? (Piece)((sq - SquareHand.HandBlack))
                    : (Piece)((sq - SquareHand.HandWhite));
            // is BoxPiece
            return (Piece)(sq - SquareHand.PieceBox);

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
        /// 引数で指定したColorとPieceに相当するSquareHand型の駒台の駒の値を生成する。
        /// 
        /// pc == NO_PIECEも許容する。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static SquareHand ToHandPiece(Color c , Piece pc)
        {
            Debug.Assert(Piece.NO_PIECE <= pc && pc < Piece.KING);
            return (c == Color.BLACK ? SquareHand.HandBlack : SquareHand.HandWhite) + (int)pc;
        }

        /// <summary>
        /// 引数で指定したColorとPieceに相当するSquareHand型の駒箱の駒の値を生成する。
        /// 
        /// pc == NO_PIECEも許容する。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static SquareHand ToPieceBoxPiece(Piece pc)
        {
            Debug.Assert(Piece.NO_PIECE <= pc && pc <= Piece.KING);
            return SquareHand.PieceBox + (int)pc;
        }

    }

}