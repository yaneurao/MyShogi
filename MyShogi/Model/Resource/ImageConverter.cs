using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Resource
{
    public static class ImageConverter
    {
        /// <summary>
        /// 駒素材のバージョン
        /// </summary>
        public static readonly int version = 3;

        /// <summary>
        /// 駒素材の画像から駒の部分を集めて書き出す。
        /// (開発時にのみ必要)
        /// </summary>
        public static void ConvertPieceImage()
        {
            ImageLoader Load(string name)
            {
                var img = new ImageLoader();
                img.Load(Path.Combine("image", name));
                return img;
            }

            // 素材の駒画像を移動させてひとまとめになった画像を作る処理
            var PieceOmote = Load($"piece_v{version}_omote.png");
            var PieceUra = Load($"piece_v{version}_ura.png");
            var omote = PieceOmote.image;
            var ura = PieceUra.image;

            int x = 96;
            int y = 106;
            var bmp = new Bitmap(x * 8, y * 4); // Piece.NB = 32 == 8*4

            // 駒画像のコピー
            void copy(Image image, int from_x, int from_y, int to_x, int to_y, bool is_white)
            {
                // 後手の駒
                if (is_white)
                    from_y = 8 - from_y;

                var g = Graphics.FromImage(bmp);
                // 盤面の升は(526,53)が左上。
                int ox = from_x * x + 526;
                int oy = from_y * y + 53;

                // 元素材、ベースラインがずれているのでそれを修正するコード

                switch (to_x)
                {
                    case 0: ox += +1; break;
                    case 1: ox += -2; break;
                    case 2: ox += -2; break;
                    case 3: ox += -2; break;
                    case 4: ox += -2; break;
                    case 5: ox += -3; break;
                    case 6: ox += -1; break;
                    case 7: ox += -3; break;
                }

                switch (from_y)
                {
                    case 1: oy += 2; break;
                    case 2: oy += 6; break;
                    case 6: oy -= 6; break;
                    case 7: oy -= 2; break;
                }

                var srcRect = new Rectangle(0 + ox, 0 + oy, x, y);
                int ox2 = to_x * x;
                int oy2 = to_y * y;
                var destRect = new Rectangle(0 + ox2, 0 + oy2, x, y);
                g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
                g.Dispose();
            }

            for (int i = 0; i < 4; ++i)
            {
                var img = ((i % 2) == 0) ? omote : ura;
                var img2 = (img == omote) ? ura : omote; // 逆側
                var c = i >= 2; // IsWhite?
                copy(img2, 4, 8, 0, i, c); // 王   59の王を、(0,0)に移動。
                copy(img, 0, 6, 1, i, c);  // 歩   97の歩を、(1,0)に移動。以下、同様。
                copy(img, 0, 8, 2, i, c);  // 香
                copy(img, 1, 8, 3, i, c);  // 桂
                copy(img, 2, 8, 4, i, c);  // 銀
                copy(img, 1, 7, 5, i, c);  // 角
                copy(img, 7, 7, 6, i, c);  // 飛
                copy(img, 3, 8, 7, i, c);  // 金
            }


            // (96*8 , 106 * 4)= (768,424)
            bmp.Save(Path.Combine("image",$"piece_v{version}_768_424.png"), System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();

        }
    }
}
