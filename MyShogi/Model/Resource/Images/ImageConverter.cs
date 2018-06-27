using System.Drawing;
using System.IO;

namespace MyShogi.Model.Resource.Images
{
    public static class ImageConverter
    {
        /// <summary>
        /// 駒素材の画像をすべて1枚絵に変換する。
        /// </summary>
        public static void ConvertPieceImage()
        {
            ConvertPieceImage_(0); // shadow
            ConvertPieceImage_(1);
            ConvertPieceImage_(2);
            ConvertPieceImage_(3);
        }

        private static ImageLoader Load(string name)
        {
            var img = new ImageLoader();
            img.Load(Path.Combine("image_source", name));
            return img;
        }

        /// <summary>
        /// 駒素材の画像から駒の部分を集めて書き出す。
        /// (開発時にのみ必要)
        /// </summary>
        private static void ConvertPieceImage_(int version)
        {
            // 素材の駒画像を移動させてひとまとめになった画像を作る処理
            var PieceOmote = Load($"piece_v{version}_omote.png");
            var PieceUra = Load($"piece_v{version}_ura.png"); // 黒い成り駒
            var PieceAka = Load($"piece_v{version}_aka.png"); // 赤い成り駒
            var omote = PieceOmote.image;
            var ura = PieceUra.image;
            var aka = PieceAka.image;

            // 駒の横・縦のサイズ[px]
            int x = 97;
            int y = 106;

            // 先手の駒(8*2)、後手の駒(8*2)、先手の赤い成駒(8*1)、後手の赤い成駒(8*1)
            // のように並んでいる。
            var bmp = new Bitmap(x * 8, y * 6); // Piece.NB = 32 == 8*4

            // 駒画像のコピー
            void copy(Image image, int from_x, int from_y, int to_x, int to_y, bool is_white)
            {
                // 後手の駒
                if (is_white)
                {
                    from_x = 8 - from_x; // x方向もミラーしておかないと角と飛車の位置が違う。
                    from_y = 8 - from_y;
                }

                var g = Graphics.FromImage(bmp);
                // 盤面の升は(526,53)が左上。
                int ox = from_x * x + 524;
                int oy = from_y * y + 53;

                // 元素材、ベースラインがずれているのでそれを修正するコード

#if false
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
#endif

#if false
                switch (from_y)
                {
                    case 1: oy += 2; break;
                    case 2: oy += 6; /*ox += 3;*/  break;
                    case 6: oy -= 6; /*ox -= 3;*/  break;
                    case 7: oy -= 2; break;
                }
#endif

#if true
                // さらに駒ごとの微調整
                int pc2 = to_x + to_y * 8;
                switch (pc2 & ~ 8/* Piece.PROMOTE*/ ) // 成り駒に関しては同様
                {
                    case 1     : oy -= 5; break;
                    case 16 + 1: oy += 5; break;
                }
#endif

                // さらに駒ごとの微調整
                var y2 = to_y;
                if (y2 == 2 || y2 == 3 || y2 == 5)
                    y2 = 2; // 後手
                else
                    y2 = 0;
                int pc = to_x + y2 * 8;
                switch (pc)
                {
                    case 1: ox += 1; break;
                    case 2: ox += 2; break;
                    case 3: ox += 2; break;
                    case 16 + 1: ox -= 2; break;
                    case 17 + 1: ox -= 2; break;
                    case 18 + 1: ox -= 2; break;
                }

                var srcRect = new Rectangle(0 + ox, 0 + oy, x, y);
                int ox2 = to_x * x;
                int oy2 = to_y * y;
                var destRect = new Rectangle(0 + ox2, 0 + oy2, x, y);
                g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
                g.Dispose();
            }

            for (int i = 0; i < 6; ++i)
            {
                var img = ((i % 2) == 0) ? omote : ura;
                var img2 = (img == omote) ? ura : omote; // 逆側

                // 赤い成駒
                if (i >= 4)
                    img = img2 = aka;

                var c = i == 2 || i==3 || i==5; // IsWhite?

                if (i==1 || i==3 || i==5)
                    copy(img2, 4, 8, 0, i, c); // 王   59の王を、(0,0)に移動。

                copy(img, 1, 6, 1, i, c);  // 歩   87の歩を、(1,0)に移動。以下、同様。
                copy(img, 0, 8, 2, i, c);  // 香
                copy(img, 1, 8, 3, i, c);  // 桂
                copy(img, 2, 8, 4, i, c);  // 銀
                copy(img, 1, 7, 5, i, c);  // 角
                copy(img, 7, 7, 6, i, c);  // 飛
                copy(img, 3, 8, 7, i, c);  // 金
            }

            {
                // 左上の塗りつぶし配置
                var g = Graphics.FromImage(bmp);
                var b = new SolidBrush(Color.FromArgb((int)(255*0.3f),0,0,0));
                g.FillRectangle(b, 0,0 , x ,y);

                // Piece.WHITEのところに最終手の着手を意味する画像を生成

                //b = new SolidBrush(Color.FromArgb((int)(255 * 0.45), 0xff, 0x7f, 0x50));
                //g.FillRectangle(b, 0 + 0, y * 2 + 0, x , y );
 
                b.Dispose();
                g.Dispose();
            }

            // (97*8 , 106 * 6)= (776,636)
            bmp.Save(Path.Combine("image",$"piece_v{version}_776_636.png"), System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();

        }

        /// <summary>
        /// 盤の端に表示する段と筋の画像の抽出
        /// </summary>
        public static void ConvertBoardNumberImage()
        {
            ConvertBoardNumberImage_(1 ,false);
            ConvertBoardNumberImage_(1, true);
            ConvertBoardNumberImage_(2, false);
            ConvertBoardNumberImage_(2, true);
        }

        /// <summary>
        /// 上の補助関数。reverse == trueだと盤面反転用の素材を生成
        /// </summary>
        /// <param name="version"></param>
        /// <param name="reverse"></param>
        private static void ConvertBoardNumberImage_(int version , bool reverse)
        {
            // 駒の横・縦のサイズ[px]
            int x = 97;
            int y = 106;

            var img = Load($"number_v{version}.png");

            var bmp1 = new Bitmap(x * 9, 19); // 横長の画像(筋を表示する)
            var bmp2 = new Bitmap(22, y * 9); // 縦長の画像(段を表示する)

            // 黒地に黒文字だと画像ビュアーで黒にしか見えないのでデバッグ時は背景を白にしておく。
//            Fill(bmp1, 255, 255, 255, 255);
//            Fill(bmp2, 255, 255, 255, 255);

            void Draw(Image dst,Rectangle srcRect, int dest_x, int dest_y)
            {
                var g = Graphics.FromImage(dst);
                var destRect = new Rectangle(dest_x,dest_y,srcRect.Width,srcRect.Height);
                g.DrawImage(img.image , destRect, srcRect, GraphicsUnit.Pixel);
                g.Dispose();
            }

            var src1 = new Rectangle(526, 53 - 19 - 2, x * 9, 19);
            Draw(bmp1, src1, 0, 0);
            bmp1.Save(Path.Combine("image", $"number_v{version}_873_19.png"), System.Drawing.Imaging.ImageFormat.Png);

            // 1 <-> 9を入れ替えたものを用意する
            bmp1 = new Bitmap(x * 9, 19); // 横長の画像(筋を表示する)
            for (int i=0;i<9;++i)
            {
                var src = new Rectangle(526 + x * i, 53 - 19 - 2, x , 19);
                Draw(bmp1, src, x * (8-i), 0);
            }

            bmp1.Save(Path.Combine("image", $"number_v{version}Rev_873_19.png"), System.Drawing.Imaging.ImageFormat.Png);
            bmp1.Dispose();

            var src2 = new Rectangle(1419 - 22 - 2, 53, 22, y * 9);
            Draw(bmp2, src2, 0, 0);
            bmp2.Save(Path.Combine("image", $"number_v{version}_22_954.png"), System.Drawing.Imaging.ImageFormat.Png);

            // 1 <-> 9を入れ替えたものを用意する
            bmp2 = new Bitmap(22, y * 9); // 横長の画像(筋を表示する)
            for (int i = 0; i < 9; ++i)
            {
                var src = new Rectangle(1419 - 22 - 2 , 53  + y * i, 22, y);
                Draw(bmp2, src,  0 , y * (8 - i));
            }

            bmp2.Save(Path.Combine("image", $"number_v{version}Rev_22_954.png"), System.Drawing.Imaging.ImageFormat.Png);
            bmp2.Dispose();


        }

        /// <summary>
        /// Bitmap全体を指定色で塗りつぶす
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="a"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        private static void Fill(Image bmp , int a_ , int r_, int g_, int b_)
        {
            // 左上の塗りつぶし配置
            var g = Graphics.FromImage(bmp);
            var b = new SolidBrush(Color.FromArgb(a_, r_, g_, b_));
            g.FillRectangle(b, 0, 0, bmp.Width, bmp.Height);
            b.Dispose();
            g.Dispose();

        }
    }
}
