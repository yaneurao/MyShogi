using System.Drawing;
using MyShogi.App;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Resource
{
    /// <summary>
    /// スプライトを返すbuilder
    /// </summary>
    public static class SpriteManager
    {
        /// <summary>
        /// 駒画像
        /// 
        /// 盤面に打つ用。
        /// 後手の駒なら、180度回転させた画像が返る。
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static Sprite PieceImage(Piece pc)
        {
            var srcRect = new Rectangle(
                    ((int)pc % 8) * piece_img_width,
                    ((int)pc / 8) * piece_img_height,
                    piece_img_width, piece_img_height);

            var image = TheApp.app.imageManager.PieceImage.image;

            return new Sprite(image, srcRect);
        }

        /// <summary>
        /// 着手の移動先、移動元などのエフェクト画像の取得。
        /// ImageManager.PieceMoveImgに格納されている画像。
        /// pc = 左から何番目の画像か。
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static Sprite PieceMoveImage(int pc)
        {
            var srcRect = new Rectangle(
                    ((int)pc % 8) * piece_img_width,
                    ((int)pc / 8) * piece_img_height,
                    piece_img_width, piece_img_height);

            var image = TheApp.app.imageManager.PieceMoveImage.image;

            return new Sprite(image, srcRect);
        }

        /// <summary>
        /// 盤面画像
        /// </summary>
        /// <returns></returns>
        public static Sprite BoardImage()
        {
            var srcRect = new Rectangle(0, 0, board_img_width, board_img_height);
            var image = TheApp.app.imageManager.BoardImage.image;

            return new Sprite(image, srcRect);
        }

        // -- 以下、画像絡みの定数。
        // これはMainDialogConst.csのほうにも同様の定義がある。

        // 盤面素材の画像サイズ
        public static readonly int board_img_width = 1920;
        public static readonly int board_img_height = 1080;

        // 駒素材の画像サイズ(駒1つ分)
        // これが横に8つ、縦に4つ、計32個並んでいる。
        public static readonly int piece_img_width = 97;
        public static readonly int piece_img_height = 106;

    }
}
