using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
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

    public static class FontExtensions
    {
        public static string Pretty(this FontStyle style)
        {
            switch(style)
            {
                case FontStyle.Regular  : return "標準";
                case FontStyle.Italic   : return "斜体";
                case FontStyle.Bold     : return "太字";
                case FontStyle.Bold+(int)FontStyle.Italic:
                    return "太字 斜体";
                default: return "不明なスタイル";
            }
        }
    }

    public static class FontUtility
    {
        /// <summary>
        /// control.FontをfontDataのFontに置換する。
        /// 
        /// control == nullなら何もせず帰る。(ことを保証する)
        /// </summary>
        /// <param name="control"></param>
        public static void ReplaceFont(Control control , FontData fontData)
        {
            if (control == null)
                return;

            // まず、Control本体のフォントだけ置換する。
            var newFontSize = fontData.FontSize <= 0 ? 9 : fontData.FontSize;
            var newFont = fontData.CreateFont();
            control.Font = newFont;

            // 子コントロールに対して、UserControl絡みだけ置換する。
            ReplaceUserControlFont(control , newFont);

#if MONO
            // Linux(Monoでメインメニューのフォントが途中から置換されない。Monoのbugくさい。自前で置換する。
            if (control is MenuStrip)
            {
                // そこにぶら下がっているToolStripMenuItemに対してFontの置換を実施する。
                var menu = control as MenuStrip;
                foreach (var item in menu.Items)
                {
                    if (item is ToolStripMenuItem)
                        (item as ToolStripMenuItem).Font = newFont;
                }
            }
#endif

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
                    var userControl = c as UserControl;
                    userControl.Font = font;
                    // このUserControlがUpdateFont()を持つなら、それを呼び出して、Fontの再計算をさせてやるべき。
                    if (userControl is IFontUpdate)
                        (userControl as IFontUpdate).UpdateFont();
                }
                else if (c is ToolStrip)
                {
                    /*
                    // ToolStripはambient propertyではないので明示的な設定が必要。
                    ToolStrip toolStrip = c as ToolStrip;
                    toolStrip.Font = font;
                    */
                    // →　これ、巻き込まないほうがいいか…。そうか…。
                }
            }
        }
    }

    /// <summary>
    /// フォントに設定する補助をするクラス
    /// </summary>
    public class FontSetter : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fontDataName_">FontManager上の変数名</param>
        public FontSetter(Control control_ , string fontDataName_)
        {
            if (TheApp.app.DesignMode)
                return;

            control = control_;
            fontDataName = fontDataName_;

            var fm = TheApp.app.Config.FontManager;
            fm.AddPropertyChangedHandler("FontChanged",FontDataChanged);
            fm.RaisePropertyChanged("FontChanged",fontDataName);
        }

        public void Dispose()
        {
            TheApp.app.Config.FontManager.RemovePropertyChangedHandler("FontChanged", FontDataChanged);
        }

        /// <summary>
        /// FontManager上の変数名
        /// </summary>
        private string fontDataName;
        /// <summary>
        /// フォントを設定する対象
        /// </summary>
        private Control control;

        private void FontDataChanged(Model.Common.ObjectModel.PropertyChangedEventArgs args)
        {
            var fm = TheApp.app.Config.FontManager;
            var s = (string)args.value;
            if (s == fontDataName)
                FontUtility.ReplaceFont(control, fm.GetValue<FontData>(s));
        }
    }
}
