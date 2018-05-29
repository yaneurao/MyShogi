using System.Drawing;
using System.Drawing.Imaging;

namespace MyShogi.Model.Resource
{
    /// <summary>
    /// 画像を動的に読み込んだりするクラス。1つの画像のみを管理する。
    /// Windows用。他の環境であれば、このクラスを差し替える。
    /// </summary>
    public class ImageLoader
    {
        /// <summary>
        /// ファイル名のファイル(画像)を読み込む。
        /// 読み込みに失敗した場合、例外は投げずにimage = nullにする。
        /// 
        /// noAlpha == trueならARGBではなくRGBのBitmapに読み込む。
        /// </summary>
        /// <param name="filename"></param>
        public void Load(string filename , bool noAlpha = false)
        {
            try
            {
                if (noAlpha)
                {
                    using (var orig = System.Drawing.Image.FromFile(filename))
                    {
                        // Image.FromFile()はpngファイルだとalphaなしで読み込めない。
                        // PiexelFormat.Format24bppRgbにcloneしてそちらを使う。

                        Bitmap clone = new Bitmap(orig.Width, orig.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        using (Graphics gr = Graphics.FromImage(clone))
                        {
                            gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
                        }
                        image = clone;
                    }
                }
                else
                {
                    image = System.Drawing.Image.FromFile(filename);
                }
            }catch
            {
                image = null;
            }
        }

        /// <summary>
        /// 読み込んでいる画像を(明示的に)開放する。
        /// </summary>
        public void Release()
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
        }

        /// <summary>
        /// 読み込んでいる画像を開放する。
        /// 内部的にRelease()を呼び出す。
        /// </summary>
        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// 読み込んでいる画像。
        /// </summary>
        public System.Drawing.Image image
        {
           get; private set;
        }
    }
}
