using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Dependency;

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
                if (c.Parent == null || c.Font != c.Parent.Font /* 親Fontに紐付いているFontだと解放してはまずい */)
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
        /// Controlを渡して、そのすべてのフォントをその環境用に一括置換する。
        /// </summary>
        /// <param name="control"></param>
        public static void ReplaceFont(Control control)
        {
            if (control is ToolStrip)
            {
                // MenuStripなどの時。

                var menu = control as ToolStrip;
                foreach (ToolStripItem c in menu.Items)
                {
                    ReplaceFontSimple(menu ,c);

                    // 本当はメニュー項目に対して再帰的にフォントを変更していく必要があるが、
                    // 途中でフォント変更してないのでやる必要なさげ。
                }
            }
            else
            {
                // 通常のControlのとき

                foreach (Control c in control.Controls)
                {
                    ReplaceFontSimple(c);

                    // 子持ちのControlであれば、その子たちのFontも再帰的に置換する。
                    if (c.Controls.Count != 0)
                        ReplaceFont(c);
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

        /// <summary>
        /// フォントをこの環境用のフォントで置き換える。
        /// 古いほうのフォントはc.Parent.Fontと異なるならDispose()を呼び出して解放する。
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        private static void ReplaceFontSimple(Control c)
        {
            var oldFont = c.Font;
            var newFont = FontReplacer.ReplaceFont(c.Font);
            var parentFont = c.Parent == null ? null : c.Parent.Font;
            if (oldFont!= null && oldFont != newFont && oldFont != parentFont)
                oldFont.Dispose();
            c.Font = newFont;
        }

        /// <summary>
        /// フォントをこの環境用のフォントで置き換える。
        /// 古いほうのフォントはparent.Fontと異なるならDispose()を呼び出して解放する。
        /// </summary>
        private static void ReplaceFontSimple(Control parent , ToolStripItem c)
        {
            var oldFont = c.Font;
            var newFont = FontReplacer.ReplaceFont(c.Font);
            var parentFont = parent == null ? null : parent.Font;
            if (oldFont != null && oldFont != newFont && oldFont != parentFont)
                oldFont.Dispose();
            c.Font = newFont;
        }

    }
}
