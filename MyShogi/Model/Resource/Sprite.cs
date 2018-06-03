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
        /// <summary>
        /// Spriteの転送元画像と転送元矩形を指定してSpriteを初期化する。
        /// </summary>
        /// <param name="srcImageLoader"></param>
        /// <param name="srcRect"></param>
        public Sprite(Image srcImageLoader , Rectangle srcRect)
        {
            image = srcImageLoader;
            rect = srcRect;
            dstOffset = new Size(0, 0);
        }

        /// <summary>
        /// dstOffset_で転送先オフセットが指定されている場合のSprite。
        /// </summary>
        /// <param name="srcImageLoader"></param>
        /// <param name="srcRect"></param>
        /// <param name="dstOffset_"></param>
        public Sprite(Image srcImageLoader, Rectangle srcRect , Size dstOffset_)
        {
            image = srcImageLoader;
            rect = srcRect;
            dstOffset = dstOffset_;
        }

        /// <summary>
        /// 他のSpriteに対してdstOffsetだけが異なるSpriteとしてこのSpriteを初期化する。
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="dstOffset_"></param>
        public Sprite(Sprite sprite,Size dstOffset_)
        {
            image = sprite.image;
            rect = sprite.rect;
            dstOffset = dstOffset_;
        }

        /// <summary>
        /// 転送元画像
        /// </summary>
        public Image image { get; private set; }

        /// <summary>
        /// 転送元矩形
        /// </summary>
        public Rectangle rect { get; private set; }

        /// <summary>
        /// 転送先オフセット。この値の分だけ(affine変換されたあと)移動させたところに描画すべき。
        /// </summary>
        public Size dstOffset { get; private set; }

        /// <summary>
        /// 連結スプライト。これがnullでないなら、次のスプライトも続けて描画される。
        /// </summary>
        public Sprite next { get; set; }
    }
}
