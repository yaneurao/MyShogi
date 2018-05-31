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
        /// 盤面画像
        /// </summary>
        /// <returns></returns>
        public static Sprite Board()
        {
            var srcRect = new Rectangle(0, 0, board_img_width, board_img_height);
            var image = TheApp.app.imageManager.BoardImage.image;

            return new Sprite(image, srcRect);
        }

        /// <summary>
        /// 駒画像
        /// 
        /// 盤面に配置する用。
        /// 後手の駒なら、180度回転させた画像が返る。
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static Sprite Piece(Piece pc)
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
        public static Sprite PieceMove(int pc)
        {
            var srcRect = new Rectangle(
                    ((int)pc % 8) * piece_img_width,
                    ((int)pc / 8) * piece_img_height,
                    piece_img_width, piece_img_height);

            var image = TheApp.app.imageManager.PieceMoveImage.image;

            return new Sprite(image, srcRect);
        }

        /// <summary>
        /// 同一種の手駒の枚数のための数字
        /// 
        /// 例えば、count == 1なら「1」という数字が返る。
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Sprite HandNumber(int count)
        {
            var srcRect = new Rectangle(48 * (count - 1), 0, 48, 48);

            var image = TheApp.app.imageManager.HandNumberImage.image;

            return new Sprite(image, srcRect);
        }

        /// <summary>
        /// 盤面の上に表示する「筋」を表現する数字画像
        /// </summary>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static Sprite BoardNumberFile(bool reverse)
        {
            var img = TheApp.app.imageManager;
            var file_img = (!reverse) ? img.BoardNumberImageFile.image : img.BoardNumberImageRevFile.image;
            var srcRect = new Rectangle(0, 0, file_img.Width, file_img.Height); // 画像丸ごとなので大きさのことは知らん。

            return new Sprite(file_img, srcRect);
        }

        /// <summary>
        /// 盤面の上に表示する「段」を表現する数字画像
        /// </summary>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static Sprite BoardNumberRank(bool reverse)
        {
            var img = TheApp.app.imageManager;
            var rank_img = (!reverse) ? img.BoardNumberImageRank.image : img.BoardNumberImageRevRank.image;
            var srcRect = new Rectangle(0, 0, rank_img.Width, rank_img.Height); // 画像丸ごとなので大きさのことは知らん。

            return new Sprite(rank_img, srcRect);
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
