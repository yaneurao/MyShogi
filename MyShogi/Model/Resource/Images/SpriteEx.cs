using System.Drawing;

namespace MyShogi.Model.Resource.Images
{
    /// <summary>
    /// SpriteとdstPointなどを一纏めにしたもの。
    /// </summary>
    public class SpriteEx
    {
        public SpriteEx(Sprite sprite_,Point dstPoint_)
        {
            sprite = sprite_;
            dstPoint = dstPoint_;
            ratio = 1.0f;
        }

        public SpriteEx(Sprite sprite_, Point dstPoint_ , float ratio_)
        {
            sprite = sprite_;
            dstPoint = dstPoint_;
            ratio = ratio_;
        }

        public Sprite sprite;
        public Point dstPoint;
        public float ratio;
    }
}
