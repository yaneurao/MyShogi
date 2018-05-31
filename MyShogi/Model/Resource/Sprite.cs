using System.Drawing;

namespace MyShogi.Model.Resource
{
    /// <summary>
    /// スプライト。
    /// 画像の転送元のImageLoaderとその矩形をペアにしておき、擬似的に1枚のbitmapとして扱う。
    /// 描画の転送元の表現の一種。
    /// </summary>
    public class Sprite
    {
        public Sprite(Image srcImageLoader , Rectangle srcRect)
        {
            image = srcImageLoader;
            rect = srcRect;
        }

        /// <summary>
        /// 転送元画像
        /// </summary>
        public Image image { get; private set; }

        /// <summary>
        /// 転送元矩形
        /// </summary>
        public Rectangle rect { get; private set; }
    }
}
