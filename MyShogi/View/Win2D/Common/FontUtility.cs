using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Dependent;

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
                    c.Font.Dispose();
                    c.Font = null;

                    // 複数のControlから共有しているFontはすでに解放済みで例外が出ることがあるが、
                    // 複数のControlで親のFont以外を共有すべきではない。(それは論理的なバグである)
                    // だからその例外はここで捕捉しない。
                }
            }
            c.Font = font;
        }

        /// <summary>
        /// Controlを渡して、そのフォントをその環境用に一括置換する。
        /// </summary>
        /// <param name="control"></param>
        public static void ReplaceFont(Control control)
        {
            if (control is ToolStrip)
            {
                var menu = control as ToolStrip;
                foreach (ToolStripItem c in menu.Items)
                {
                    // まずcのフォントを置換
                    var oldFont = c.Font;
                    var newFont = FontReplacer.ReplaceFont(c.Font);
                    if (oldFont != newFont)
                    {
                        c.Font = newFont;
                        if (oldFont != menu.Font)
                            oldFont.Dispose();
                    }

                    // TODO : メニュー項目に対して再帰的にフォントを変更していく。
                    // ここ難しい。あとで考える。
#if false
                    if (c is ToolStripMenuItem)
                    {
                        var toolStrip = c as ToolStripMenuItem;
                        
                        foreach (ToolStripItem c2 in toolStrip.DropDownItems)
                            ReplaceFont(c2.);
                    }
#endif                    
                }
            }
            else
            {
                foreach (Control c in control.Controls)
                {
                    if (c.Controls.Count != 0)
                    {
                        // 子持ちのControlであれば、このFontは置換せず、その子たちのFontを置換する。
                        ReplaceFont(c);
                    }
                    else
                    {
                        var oldFont = c.Font;
                        var newFont = FontReplacer.ReplaceFont(c.Font);
                        if (oldFont != newFont)
                        {
                            c.Font = newFont;

                            // 前のフォントがc.Parent.Fontでないなら解体すべき。
                            // 所有権の問題があって難しいが…。
                            if (oldFont != c.Parent.Font)
                                oldFont.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// フォントをこの環境用のフォントで置き換える。
        /// 古いほうのフォントはDispose()を呼び出して解放する。
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        public static Font ReplaceFont(Font font)
        {
            var oldFont = font;
            var newFont = FontReplacer.ReplaceFont(font);
            if (oldFont != newFont)
                oldFont.Dispose();

            return newFont;
        }

    }
}
