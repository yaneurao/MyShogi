#if false
// 使おうと思って作成したものの、これではまずいことに気づいた。

using System.Drawing;
using System.Drawing.Drawing2D;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// Font種別
    /// </summary>
    public enum FontType
    {
        MS_GOTHIC ,
        MS_MINCHO ,
        MS_UI_GOTHIC,
    }

    /// <summary>
    /// dpi非依存のfontを生成する。
    /// またGraphicsに対してワールド変換を適用する。(本当は、.NET FrameoworkのAutoScalingがやるべきだと思う)
    /// 
    /// cf. https://qiita.com/felis_silv/items/efee4b1a397b0b95100a
    /// </summary>
    public class DpiScaler
    {
        /// <summary>
        /// 現在のdpiに依存したワールド変換を設定する。
        /// </summary>
        /// <param name="g"></param>
        public DpiScaler(Graphics g)
        {
            scale = g.DpiX / 96f;

            org_matrix = g.Transform;
            g.ScaleTransform(scale, scale);
            org_g = g;
        }

        public void Dispose()
        {
            org_g.Transform = org_matrix;
        }

        /// <summary>
        /// SetGraphics()でワールド変換行列を変更してしまっているのでFontをscaleに即して生成しないといけない。
        /// そのためのFontBuilder
        /// </summary>
        /// <param name="fontType"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Font CreateFont(FontType fontType , int pt)
        {
            return new Font(FontNameOf(fontType), pt / scale);
        }

        /// <summary>
        /// FontTypeからフォント名を返す。
        /// </summary>
        /// <param name="fontType"></param>
        /// <returns></returns>
        public static string FontNameOf(FontType fontType)
        {
            switch(fontType)
            {
                case FontType.MS_GOTHIC: return "ＭＳ ゴシック";
                case FontType.MS_MINCHO: return "ＭＳ 明朝";
                case FontType.MS_UI_GOTHIC: return "MS UI Gothic";
                default: return null;
            }
        }

        private float scale;
        private Matrix org_matrix; // Graphicsの元のワールド変換行列
        private Graphics org_g; // Graphicsオブジェクト
    }
}
#endif
