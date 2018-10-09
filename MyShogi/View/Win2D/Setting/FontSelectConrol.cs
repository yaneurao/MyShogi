using MyShogi.Model.Common.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Setting
{
    public partial class FontSelectConrol : UserControl
    {
        /// <summary>
        /// フォント選択のためのControl。
        ///
        /// DisplaySettingControlで設定ダイアログ、検討ウインドウ、棋譜ウインドウ
        /// それぞれのフォントを変更できるので、そのための下請けとなるControl。
        /// </summary>
        public FontSelectConrol()
        {
            InitializeComponent();

            InitViewModel();
        }

        public class FontSelectViewModel : NotifyObject
        {
            /// <summary>
            /// 選択されているフォント名
            /// </summary>
            public string FontName
            {
                get { return GetValue<string>("FontName"); }
                set { SetValue("FontName",value); }
            }

            /// <summary>
            /// 選択されているフォントサイズ
            /// </summary>
            public float FontSize
            {
                get { return GetValue<float>("FontSize"); }
                set { SetValue("FontSize", value); }
            }

            // その他、Italic、Boldぐらいは選べてもいいかも。

            // Colorは選べるようにすると色々ややこしくなるのでやめたほうがいいような…。
            // Colorは描画する時のPropertyだしな…。
        }

        private void InitViewModel()
        {

        }
    }
}
