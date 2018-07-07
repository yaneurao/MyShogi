using System.Drawing;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 盤面の描画位置に関する各種定数
    /// </summary>
    public partial class GameScreenControl
    {
        // -- 各種定数

        // 盤面素材の画像サイズ
        public static readonly Size board_img_size = new Size(1920, 1080);

        // 盤面素材における、駒を配置する升の左上。
        public static readonly Point board_location = new Point(524, 53);

        // 駒素材の画像サイズ(駒1つ分)
        // これが横に8つ、縦に4つ、計32個並んでいる。
        public static readonly Size piece_img_size = new Size(97, 106);

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

        // 駒箱の表示順
        private static readonly Piece[] piece_box_list = {
        Piece.KING,
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
        /// 駒台の画面上の位置
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
        /// 駒箱の画面上の位置
        /// </summary>
        private static readonly Point[] piece_box_pos =
        {
        new Point(230,698), // 駒箱 Ver.1
        new Point(403,700), // 駒箱 Ver.2
    };

        /// <summary>
        /// 駒箱の幅と高さ
        /// </summary>
        private static Size[] piece_box_size =
        {
        new Size(268 , 305) , // 駒箱 Ver.1
        new Size(92  , 350) , // 駒箱 Ver.2
    };

        /// <summary>
        /// 駒台で、同種の駒が複数あるときの数字の描画のための(当該駒からの)オフセット値
        /// </summary>
        private static Size hand_number_offset = new Size(60, 20);

        /// <summary>
        /// 駒箱で、同種の駒が複数あるときの数字の描画のための(当該駒からの)オフセット値
        /// </summary>
        private static Size[] hand_number_offset2 = {
        new Size(50, 15),
        new Size(30, 10),
    };

        /// <summary>
        /// 駒箱の画面上の位置
        /// </summary>
        private static readonly Point[] hand_box_pos =
        {
        // 普通の駒箱
        new Point(229,643+52),
        // 細長の駒箱
        new Point(404-10,730),
    };


        /// <summary>
        /// 盤の筋と段を表す素材の表示位置
        /// </summary>
        private static readonly Point[] board_number_pos =
        {
        new Point( 526, 32), // 筋
        new Point(1397, 49), // 段
    };

        /// <summary>
        /// ネームプレートの氏名用の座標
        /// 通常の駒台用
        /// </summary>
        private static readonly Point[] name_plate_name =
        {
        new Point(1437+2,485+2), // 先手のネームプレート
        new Point(239+2,446+2),  // 後手のネームプレート
    };

        /// <summary>
        /// ネームプレートの氏名用の座標
        /// 細長い駒台用
        /// </summary>
        private static readonly Point[] name_plate_slim_name =
        {
        new Point(430 + 65 +1057/2 + 400 ,1030+10), // 先手のネームプレート
        new Point(430 + 65               ,1030+10), // 後手のネームプレート
    };

        /// <summary>
        /// 手番素材の表示場所
        /// 通常の駒台用
        /// </summary>
        private static readonly Point[] turn_normal_pos =
        {
        new Point(1680 - 100,479),  // 先手手番
        new Point(490 - 100,438),   // 後手手番
    };

        /// <summary>
        /// 手番素材の表示場所
        /// 細長い駒台用
        /// </summary>
        private static readonly Point turn_slim_pos = new Point(430, 1030);

        /// <summary>
        /// 対局時間の表示
        /// 通常の駒台用
        /// </summary>
        private static readonly Point[] time_setting_pos =
        {
        new Point(1437+2,485+2 + 50), // 先手
        new Point(239+2,446+2 + 50),  // 後手
    };

        /// <summary>
        /// 対局時間の表示
        /// 細長い駒台用
        /// 
        /// 表示する場所がなさげ..
        /// </summary>
        private static readonly Point[] time_setting_slim_pos =
        {
        new Point(430 + 65 +1057/2 + 400 - 150 ,1030+10 ), // 先手
        new Point(430 + 65               + 150 ,1030+10 ), // 後手
    };

        /// <summary>
        /// 対局時間の表示  残り時間
        /// 通常の駒台用
        /// </summary>
        private static readonly Point[] time_setting_pos2 =
        {
        new Point(1437+2+ 120,485+2 + 85), // 先手
        new Point(239+2 + 120,446+2 + 85),  // 後手
    };

        /// <summary>
        /// 対局時間の表示  残り時間
        /// 細長い駒台用
        /// </summary>
        private static readonly Point[] time_setting_slim_pos2 =
        {
        new Point(430 + 65 +1057/2 + 400 - 340 ,1030 +12 ), // 先手
        new Point(430 + 65               + 340 ,1030 +12 ), // 後手
    };

        /// <summary>
        /// エンジン初期化時のダイアログ
        /// </summary>
        private static readonly Point engine_init_pos = new Point(534 + 230, 53 + 360);
    }
}
