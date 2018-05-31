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
        /// 読み込みに失敗した場合、例外は投げずにimageに「×」画像を設定する。
        /// 
        /// </summary>
        /// <param name="filename"></param>
        public void Load(string filename)
        {
            try {

                image = System.Drawing.Image.FromFile(filename);

            } catch
            {
                // 画像の読み込みに失敗した。これを例外として投げると、オプションを変更して、その設定に対応する
                // 画像が足りないときに例外で落ちてしまい、また、その設定が終了時に保存されるので、次回以降ソフト自体が
                // 起動しないことになるので、それは防ぎたい。

                // そこでここでは64×64の赤で「×」印を描画したBitmapを用意する。
                // デザインパターンで言うところのNullObjectみたいなものである。
                // (ファイル名も描画しておくと存在しないファイルのファイル名がわかっていいかも)

                image = new Bitmap(64, 64 , PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(image))
                {
                    var pen = new Pen(Color.Red,5);
                    g.DrawLine(pen, new Point(0, 0), new Point(63, 63));
                    g.DrawLine(pen, new Point(63, 0), new Point(0, 63));
                }
            }
        }

        /// <summary>
        /// width×heightサイズのbitmapを用意する。
        /// 初期化はしないので何が描かれているかは不定とする。
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        public void CreateBitmap(int width,int height , PixelFormat format)
        {
            if (image == null)
            {
                image = new Bitmap(width, height, format);
            } else if (image.Width != width || image.Height != height)
            {
                Release();
                image = new Bitmap(width, height, format);
            }
        }
        
        /// <summary>
        /// 外部で生成されたBitmapのインスタンスを渡して、
        /// このクラスの管理下におく。
        /// </summary>
        /// <param name="bmp"></param>
        public void SetBitmap(Bitmap bmp)
        {
            Release();
            image = bmp;
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
