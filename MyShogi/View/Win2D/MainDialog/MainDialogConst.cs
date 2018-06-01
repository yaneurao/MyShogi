using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// MainDialogで用いる、盤面の描画位置に関する各種定数
    /// </summary>
    public partial class MainDialog
    {
        // -- 各種定数

        // 盤面素材の画像サイズ
        public static readonly int board_img_width = 1920;
        public static readonly int board_img_height = 1080;

        // 盤面素材における、駒を配置する升の左上。
        public static readonly int board_left = 524;
        public static readonly int board_top = 53;

        // 駒素材の画像サイズ(駒1つ分)
        // これが横に8つ、縦に4つ、計32個並んでいる。
        public static readonly int piece_img_width = 97;
        public static readonly int piece_img_height = 106;

        // 手駒の表示場所(駒台を左上とする)
        public class HandLocation
        {
            public HandLocation(Piece piece_, int x_, int y_)
            {
                piece = piece_;
                x = x_;
                y = y_;
            }

            public Piece piece;
            public int x;
            public int y;
        };

        /// <summary>
        /// 手駒の表示場所(駒台を左上とする)
        /// </summary>
        private static readonly HandLocation[] hand_location1 =
        {
            // 10(margin)+96(piece_width)+30(margin)+96(piece_width)+28(margin) = 260(駒台のwidth)
            new HandLocation(Piece.ROOK,10,5),
            new HandLocation(Piece.BISHOP, 135,5),
            new HandLocation(Piece.GOLD,10,100),
            new HandLocation(Piece.SILVER,135,100),
            new HandLocation(Piece.KNIGHT,10,190),
            new HandLocation(Piece.LANCE, 135,190),
            new HandLocation(Piece.PAWN,10,280),
        };

        /// <summary>
        /// 手駒の表示場所(駒台を左上とする)
        /// 縦長の駒台の場合
        /// </summary>
        private static readonly HandLocation[] hand_location2 =
        {
            new HandLocation(Piece.ROOK,-5,0),
            new HandLocation(Piece.BISHOP, -5,95),
            new HandLocation(Piece.GOLD,-5,187),
            new HandLocation(Piece.SILVER,-5,279),
            new HandLocation(Piece.KNIGHT,-5,369),
            new HandLocation(Piece.LANCE, -5,459),
            new HandLocation(Piece.PAWN, -5,549),
        };

        /// <summary>
        /// 駒台の画面上の位置(通常の駒台)
        /// </summary>
        private static readonly Point[] hand_table_pos1 =
        {
            new Point(1431,643), // 先手の駒台
            new Point(229 , 32), // 後手の駒台
        };

        /// <summary>
        /// 駒台の画面上の位置(細い駒台)
        /// </summary>
        private static readonly Point[] hand_table_pos2 =
        {
            new Point(1431,368), // 先手の駒台
            new Point( 404 ,32), // 後手の駒台
        };

        /// <summary>
        /// 駒台の幅と高さ
        /// </summary>
        private static int hand_table_width1 = 260;
        private static int hand_table_width2 = 95;
        private static int hand_table_height1 = 388;
        private static int hand_table_height2 = 663;

        /// <summary>
        /// ネームプレートの座標
        /// </summary>
        private static readonly Point[] name_plate =
        {
            new Point(1437+2,485+2), // 先手のネームプレート
            new Point(239+2,446+2),  // 後手のネームプレート
        };

    }
}
