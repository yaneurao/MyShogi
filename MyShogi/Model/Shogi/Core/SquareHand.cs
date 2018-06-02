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
        BlackHand = 81,     // 先手の手駒のスタート
        WhiteHand = 81 + 7, // 後手の手駒のスタート
        HandNB = 81 + 14,   // 手駒の終端+1

        // ゼロと末尾
        ZERO = 0, NB = HandNB,
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
        /// 盤上の升ならColor.NBを返す。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Color HandColorOf(this SquareHand sq)
        {
            if (sq < SquareHand.SquareNB)
                return Color.NB; // 盤面

            return (sq < SquareHand.WhiteHand) ? Color.BLACK : Color.WHITE;
        }

        /// <summary>
        /// sqの手駒に対して、その駒種を返す
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Piece HandPieceOf(this SquareHand sq)
        {
            Debug.Assert(HandColorOf(sq) != Color.NB);

            return (sq < SquareHand.WhiteHand)
                ? (Piece)((sq - SquareHand.BlackHand) + Piece.PAWN)
                : (Piece)((sq - SquareHand.WhiteHand) + Piece.PAWN);

        }

        /// <summary>
        /// Squareを綺麗に出力する(USI形式ではない)
        /// 日本語文字での表示になる。例 → ８八 , 先手歩 (先手の手駒の歩)
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static string Pretty(this SquareHand sq)
        {
            var c = HandColorOf(sq);

            if (c == Color.NB)
                return ((Square)sq).Pretty();

            return c.Pretty() + HandPieceOf(sq).Pretty();
        }

    }

}