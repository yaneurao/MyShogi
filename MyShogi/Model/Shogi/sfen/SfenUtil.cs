using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi
{
    public static class SfenUtil
    {
        /// <summary>
        /// sfenの駒文字に対応する駒定数
        /// 駒定数のほうは、やねうら王に合わせておく。
        /// </summary>
        private static readonly string SfenPieceList = "?PLNSBRGK";

        /*
        '?', // None        = 0
        'P', // Pawn   : 歩 = 1
        'L', // Lance  : 香 = 2
        'N', // kNight : 桂 = 3
        'S', // Silver : 銀 = 4
        'B', // Bishop : 角 = 5
        'R', // Rook : 飛車 = 6
        'G', // Gold   : 金 = 7
        'K', // King   : 玉 = 8
        */

        /// <summary>
        /// SFEN形式の対応する駒文字を取得する。
        /// 先手ならば大文字、後手なら小文字。
        /// </summary>
        public static string PieceToSfen(Piece p)
        {
            var c = SfenPieceList[p.RawPieceType().ToInt()];

            // 後手であれば小文字
            if (p.PieceColor() == Color.WHITE)
                c = char.ToLower(c);

            // 成り駒ならば先頭に"+"
            return (p.IsPromote() ? "+" : "") + c;
        }
    }

/*
    public static Piece SfenToPieceType(char piece)
    {
    // この設計、いいのかな…。あとでよく考える。

    }
*/

#if false

        /// <summary>
        /// 文字をSFEN形式の駒として解釈します。
        /// </summary>
        /// <remarks>
        /// 大文字小文字は無視します。
        /// </remarks>
        public static PieceType SfenToPieceType(char piece)
        {
            for (var i = 0; i < SfenPieceList.Count(); ++i)
            {
                if (char.ToUpper(piece) == SfenPieceList[i])
                {
                    return (PieceType)i;
                }
            }

            return PieceType.None;
        }

        /// <summary>
        /// 文字をSFEN形式の駒として解釈します。
        /// </summary>
        /// <remarks>
        /// 大文字の場合は先手、小文字の場合は後手となります。
        /// </remarks>
        public static BoardPiece SfenToPiece(char piece)
        {
            for (var i = 0; i < SfenPieceList.Count(); ++i)
            {
                if (piece == SfenPieceList[i])
                {
                    return new BoardPiece((PieceType)i, false, BWType.Black);
                }

                if (piece == char.ToLower(SfenPieceList[i]))
                {
                    return new BoardPiece((PieceType)i, false, BWType.White);
                }
            }

            return null;
        }
    }
}

#endif

}
