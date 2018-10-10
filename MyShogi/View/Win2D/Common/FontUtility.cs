using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Common.Tool;

namespace MyShogi.View.Win2D
{
    interface IFontUpdate
    {
        /// <summary>
        /// Control.Fontが変更になったので、このフォント相対でサイズを決定していたフォントを更新する。
        /// </summary>
        void UpdateFont();
    }

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
                // 古いフォントを解放する。
                // メインウインドウにぶら下がっているフォントは解放してはならない。
                // (これはambient propertyであり、他のControlから暗黙的に参照されている)
                var parentFont = c.Parent?.Font;
                if (parentFont != null /* 独立したウインドウなら解放しない */ && c.Font != parentFont)
                    c.Font.Dispose();

                // 複数のControlから共有しているFontはすでに解放済みで例外が出ることがあるが、
                // 複数のControlで親のFont以外を共有すべきではない。(それは論理的なバグである)
                // だからその例外はここで捕捉しない。
            }
            c.Font = font;
        }

        /// <summary>
        /// control.FontをfontDataのFontに置換する。
        /// </summary>
        /// <param name="control"></param>
        public static void ReplaceFont(Control control , FontData fontData)
        {
            // まず、Control本体のフォントだけ置換する。
            var newFontSize = fontData.FontSize <= 0 ? 9 : fontData.FontSize;
            var newFont = new Font(fontData.FontName, newFontSize, fontData.FontStyle);
            SetFont(control, newFont);

            // 子コントロールに対して、UserControl絡みだけ置換する。
            ReplaceUserControlFont(control , newFont);
        }

        /// <summary>
        /// Controlに対して、その子コントロールを調べてUSerControlがあれば置換する。
        /// </summary>
        private static void ReplaceUserControlFont(Control control , Font font)
        {
            foreach (var c in control.Controls)
            {
                if (c is SplitContainer)
                {
                    // このコンテナ内にあるUserControlも置換する必要がある。
                    // 再帰的に呼び出す
                    var split = c as SplitContainer;
                    ReplaceUserControlFont(split.Panel1 , font);
                    ReplaceUserControlFont(split.Panel2 , font);
                }
                else if (c is UserControl)
                {
                    UserControl userControl = c as UserControl;
                    userControl.Font = font;
                    // このUserControlがUpdateFont()を持つなら、それを呼び出して、Fontの再計算をさせてやるべき。
                    if (userControl is IFontUpdate)
                        (userControl as IFontUpdate).UpdateFont();
                }
            }
        }

    }
}
