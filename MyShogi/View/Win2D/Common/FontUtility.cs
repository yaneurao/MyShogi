using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public static class FontUtility
    {
        /// <summary>
        /// Fontを自分でnewしたものであることがわかっているときに用いる。
        /// 前のFontを開放してから新しいFontを設定する。
        ///
        /// Fontをusing(var font = new Font(... ) )のように書けないときは、
        /// なるべくこのメソッドを用いて、前使っていたFontを解放すること。
        /// </summary>
        /// <param name="c"></param>
        public static void SetFont(Control c, Font font)
        {
            if (c.Font != null)
            {
                if (c.Font != c.Parent.Font /* 親Fontに紐付いているFontだと解放してはまずい */)
                {
                    try
                    {
                        c.Font.Dispose();
                    }
                    catch { } // 複数のControlから共有しているFontはすでに解放済みで例外が出ることはある。
                    c.Font = null;
                }
            }
            c.Font = font;
        }

    }
}
