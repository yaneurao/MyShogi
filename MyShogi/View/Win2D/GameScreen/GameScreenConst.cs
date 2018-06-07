using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 盤面の描画位置に関する各種定数
    /// </summary>
    public partial class GameScreen
    {
        // -- 各種定数

        // 盤面素材の画像サイズ
        public static readonly Size board_img_size = new Size(1920,1080);

        // 盤面素材における、駒を配置する升の左上。
        public static readonly Point board_location = new Point(524, 53);

        // 駒素材の画像サイズ(駒1つ分)
        // これが横に8つ、縦に4つ、計32個並んでいる。
        public static readonly Size piece_img_size = new Size(97,106);

        // 駒台の手駒の表示順
        private static readonly Piece[] hand_piece_list = {
            Piece.ROOK,
            Piece.BISHOP,
            Piece.GOLD,
            Piece.SILVER,
            Piece.KNIGHT,
            Piece.LANCE,
            Piece.PAWN,
            };
            
        /// <summary>
        /// 駒台の手駒の表示場所(駒台を左上とする)
        /// 
        /// 順番は、Piece.PAWNからPieceの定数の順になっているので注意。
        /// </summary>
        private static readonly Point[,] hand_piece_pos =
        {
            // 普通の駒台の場合
            {
                // 10(margin)+96(piece_width)+30(margin)+96(piece_width)+28(margin) = 260(駒台のwidth)
                new Point(/*Piece.PAWN  ,*/  10,280),
                new Point(/*Piece.LANCE ,*/ 135,190),
                new Point(/*Piece.KNIGHT,*/  10,190),
                new Point(/*Piece.SILVER,*/ 135,100),
                new Point(/*Piece.BISHOP,*/ 135,  5),
                new Point(/*Piece.ROOK  ,*/  10,  5),
                new Point(/*Piece.GOLD  ,*/  10,100),
            },
            // 縦長の駒台の場合
            {
                new Point(/*Piece.PAWN  ,*/ -5,549),
                new Point(/*Piece.LANCE ,*/ -5,459),
                new Point(/*Piece.KNIGHT,*/ -5,369),
                new Point(/*Piece.SILVER,*/ -5,279),
                new Point(/*Piece.BISHOP,*/ -5, 95),
                new Point(/*Piece.ROOK  ,*/ -5,  0),
                new Point(/*Piece.GOLD  ,*/ -5,187),
            }
        };

        /// <summary>
        /// 駒台の画面上の位置(通常の駒台)
        /// </summary>
        private static readonly Point[,] hand_table_pos =
        {
            // 普通の駒台
            {
                new Point(1431,643), // 先手の駒台
                new Point(229 , 32), // 後手の駒台
            },
            // 細長の駒台
            {
                new Point(1431,368), // 先手の駒台
                new Point( 404 ,32), // 後手の駒台
            }
        };

        /// <summary>
        /// 駒台の幅と高さ
        /// </summary>
        private static Size[] hand_table_size =
       {
            new Size(260 , 388) , // 駒台 Ver.1
            new Size(95  , 663) , // 駒台 Ver.2
        };

        /// <summary>
        /// 駒台で、同種の駒が複数あるときの数字の描画のための(当該駒からの)オフセット値
        /// </summary>
        private static Size hand_number_offset = new Size(60, 20);

        /// <summary>
        /// 盤の筋と段を表す素材の表示位置
        /// </summary>
        private static readonly Point[] board_number_pos =
        {
            new Point( 526, 32), // 筋
            new Point(1397, 49), // 段
        };

        /// <summary>
        /// 通常の駒台用のネームプレートの氏名用の座標
        /// </summary>
        private static readonly Point[] name_plate_name =
        {
            new Point(1437+2,485+2), // 先手のネームプレート
            new Point(239+2,446+2),  // 後手のネームプレート
        };

        /// <summary>
        /// 細長い駒台用のネームプレートの氏名用の座標
        /// </summary>
        private static readonly Point[] name_plate_slim_name =
        {
            new Point(430 + 65 +1057/2 + 155 ,1030+10), // 先手のネームプレート
            new Point(430 + 65               ,1030+10), // 後手のネームプレート
        };

        /// <summary>
        /// 通常の駒台用の手番素材の表示場所
        /// </summary>
        private static readonly Point[] turn_normal_pos =
        {
            new Point(1680 - 100,479),  // 先手手番
            new Point(490 - 100,438),   // 後手手番
        };

        /// <summary>
        /// 細長いの駒台用の手番素材の表示場所
        /// </summary>
        private static readonly Point turn_slim_pos = new Point(430, 1030);

    }
}
