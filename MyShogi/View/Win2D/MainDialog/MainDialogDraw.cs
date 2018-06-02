using System.Drawing;
using MyShogi.Model.Resource;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// MainDialogでaffine変換して描画する部分のコード
    /// </summary>
    public partial class MainDialog
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
            return ViewInstance.AffineMatrix.Affine(p);
        }

        /// <summary>
        /// Sizeに対してaffine変換を行う。
        /// offsetの加算は行わない。scaleのみ。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private Size AffineScale(Size s)
        {
            return ViewInstance.AffineMatrix.AffineScale(s);
        }

        private Rectangle Affine(Point p,Size s)
        {
            return ViewInstance.AffineMatrix.Affine(p,s);
        }

        /// <summary>
        /// 上記のAffine()の逆変換
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Point InverseAffine(Point p)
        {
            return ViewInstance.AffineMatrix.InverseAffine(p);
        }

        /// <summary>
        /// DrawSprite(),DrawString()に毎回引数で指定するの気持ち悪いので、
        /// この２つの関数を呼び出す前にこの変数にコピーしておくものとする。
        /// </summary>
        private Graphics graphics;

        /// <summary>
        // スプライトを描画するコード
        // 以下の描画を移植性を考慮してすべてスプライトの描画に抽象化しておく。
        // pの地点に等倍でSpriteを描画する。(描画するときにaffine変換を行うものとする)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="img"></param>
        /// <param name="destRect"></param>
        /// <param name="sourceRect"></param>
        private void DrawSprite(Point p, Sprite src)
        {
            // null sprite
            if (src == null)
                return;

            var dstRect = Affine(p, new Size(src.rect.Width, src.rect.Height));
            // dstRect.Width = 転送先width×scale_xなのだが、等倍なので転送先width == 転送元width
            // heightについても上記と同様。

            graphics.DrawImage(src.image, dstRect, src.rect, GraphicsUnit.Pixel);

            // 連結スプライトならば続けてそれを描画する。
            if (src.next != null)
                DrawSprite(p, src.next);
        }

        /// <summary>
        /// scale_x,scale_y、offset_x,offset_yを用いてアフィン変換してから文字列を描画する。
        /// </summary>
        /// <param name="g"></param>
        /// <param name="dstPoint"></param>
        /// <param name="mes"></param>
        private void DrawString(Point dstPoint, string mes, int font_size)
        {
            // 文字フォントサイズは、scaleの影響を受ける。
            var scale = ViewInstance.AffineMatrix.Scale.X;

            var size = (int)(font_size * scale);
            // こんな小さいものは視認できないので描画しなくて良い。
            if (size <= 2)
                return;

            using (var font = new Font("MSPゴシック", size, GraphicsUnit.Pixel))
            {
                graphics.DrawString(mes, font, Brushes.Black, Affine(dstPoint));
            }
        }

    }
}
