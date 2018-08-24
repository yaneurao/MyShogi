using System.Drawing;
using MyShogi.Model.Resource.Images;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// MainDialogでaffine変換して描画する部分のコード
    /// </summary>
    public partial class GameScreenControl
    {
        // -------------------------------------------------------------------------
        //  affine変換してのスクリーンへの描画
        // -------------------------------------------------------------------------

        /// <summary>
        /// ViewInstanceのOffsetX,OffsetY,ScaleX,ScaleY
        /// の値に基づいてaffine変換を行う
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Point Affine(Point p)
        {
            return AffineMatrix.Affine(p);
        }

        /// <summary>
        /// Sizeに対してaffine変換を行う。
        /// offsetの加算は行わない。scaleのみ。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private Size AffineScale(Size s)
        {
            return AffineMatrix.AffineScale(s);
        }

        private Rectangle Affine(Point p, Size s)
        {
            return AffineMatrix.Affine(p, s);
        }

        /// <summary>
        /// 上記のAffine()の逆変換
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Point InverseAffine(Point p)
        {
            return AffineMatrix.InverseAffine(p);
        }

        /// <summary>
        /// DrawSprite(),DrawString()に毎回引数で指定するの気持ち悪いので、
        /// この２つの関数を呼び出す前にこの変数にコピーしておくものとする。
        /// </summary>
        public Graphics graphics;

        /// <summary>
        /// スプライトを描画するコード
        /// 以下の描画を移植性を考慮してすべてスプライトの描画に抽象化しておく。
        /// pの地点に等倍でSpriteを描画する。(描画するときにaffine変換を行うものとする)
        /// ratioは表示倍率。デフォルトでは1.0
        /// </summary>
        /// <param name="g"></param>
        /// <param name="img"></param>
        /// <param name="destRect"></param>
        /// <param name="sourceRect"></param>
        private void DrawSprite(Point p, Sprite src, float ratio = 1.0f)
        {
            // null sprite
            if (src == null)
                return;

            var dstRect = Affine(p, new Size((int)(src.rect.Width * ratio), (int)(src.rect.Height * ratio)));
            // dstRect.Width = 転送先width×scale_xなのだが、等倍なので転送先width == 転送元width
            // heightについても上記と同様。

            // dstOffsetが指定されていれば、この分だけ(affine変換してから)ずらした場所に描画する。
            var dstOffset = AffineScale(src.dstOffset);
            dstRect.X += dstOffset.Width;
            dstRect.Y += dstOffset.Height;

            graphics.DrawImage(src.image, dstRect, src.rect, GraphicsUnit.Pixel);

            // 連結スプライトならば続けてそれを描画する。
            if (src.next != null)
                DrawSprite(p, src.next, ratio);
        }

        /// <summary>
        /// SpriteEx型のスプライトを渡して、それを描画する。
        /// </summary>
        /// <param name="sprite"></param>
        private void DrawSprite(SpriteEx sprite)
        {
            DrawSprite(sprite.dstPoint, sprite.sprite, sprite.ratio);
        }

        /// <summary>
        /// scale_x,scale_y、offset_x,offset_yを用いてアフィン変換してから文字列を描画する。
        /// </summary>
        /// <param name="g"></param>
        /// <param name="dstPoint"></param>
        /// <param name="mes"></param>
        private void DrawString(Point dstPoint, string mes, int font_size, DrawStringOption option = null)
        {
            // 文字フォントサイズは、scaleの影響を受ける。
            var scale = AffineMatrix.Scale.X;

            var size = (float)(font_size * scale);
            // こんな小さいものは視認できないので描画しなくて良い。
            if (size <= 2)
                return;

            using (var font = new Font("MSPゴシック", size, GraphicsUnit.Pixel))
            {
                var brush = option != null ? option.brush : Brushes.Black;
                var sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Near;
                var align = option != null ? option.align : 0;
                switch (align)
                {
                    // 左寄せ
                    case 0: sf.Alignment = StringAlignment.Near; break;

                    // センタリング
                    case 1: sf.Alignment = StringAlignment.Center; break;

                    // 右寄せ
                    case 2: sf.Alignment = StringAlignment.Far; break;
                }
                graphics.DrawString(mes, font, brush, Affine(dstPoint), sf);
            }
        }

        /// <summary>
        /// DrawString()で指定するオプション
        /// </summary>
        private class DrawStringOption
        {
            public DrawStringOption(Brush brush_, int align_)
            {
                brush = brush_;
                align = align_;
            }

            // テキストの色
            public Brush brush;

            // テキストの描画位置
            // 0 = 左寄せ , 1 = 中央 , 2 右寄せ
            public int align;

        }
    }
}
