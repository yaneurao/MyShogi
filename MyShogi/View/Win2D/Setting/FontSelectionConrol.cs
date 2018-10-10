using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Tool;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Setting
{
    public partial class FontSelectionConrol : UserControl
    {
        /// <summary>
        /// フォント選択のためのControl。
        ///
        /// DisplaySettingControlで設定ダイアログ、検討ウインドウ、棋譜ウインドウ
        /// それぞれのフォントを変更できるので、そのための下請けとなるControl。
        /// </summary>
        public FontSelectionConrol()
        {
            InitializeComponent();

            InitViewModel();
        }

        public class FontSelectionViewModel : NotifyObject
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

            /// <summary>
            /// フォントのスタイル
            /// </summary>
            public FontStyle FontStyle
            {
                get { return GetValue<FontStyle>("FontStyle"); }
                set { SetValue("FontStyle", value); }
            }

            // Colorは選べるようにすると色々ややこしくなるのでやめたほうがいいような…。
            // Colorは描画する時のPropertyだしな…。
        }

        public FontSelectionViewModel ViewModel = new FontSelectionViewModel();

        /// <summary>
        /// 説明書き
        /// </summary>
        public string Description
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        /// <summary>
        /// このControlにBindする。
        /// </summary>
        /// <param name="font"></param>
        public void Bind(FontData font)
        {
            ViewModel.FontName = font.FontName;
            ViewModel.FontSize = font.FontSize;
            ViewModel.FontStyle = font.FontStyle;

            ViewModel.AddPropertyChangedHandler("FontName", (args) => { font.FontName = args.value as string; });
            ViewModel.AddPropertyChangedHandler("FontSize", (args) => { font.FontSize = (float)args.value; });
            ViewModel.AddPropertyChangedHandler("FontStyle", (args) => { font.FontStyle = (FontStyle)args.value; });
        }

        private void InitViewModel()
        {
            ViewModel.AddPropertyChangedHandler("FontName", (args) => { textBox1.Text = args.value as string; });
            ViewModel.AddPropertyChangedHandler("FontSize", (args) => { textBox3.Text = ((float)args.value).ToString() + "pt"; });
            ViewModel.AddPropertyChangedHandler("FontStyle", (args) => { textBox2.Text = ((FontStyle)args.value).Pretty(); });
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            using (var fd = new FontDialog())
            {
                // デフォルトのフォント
                fd.Font = new Font(ViewModel.FontName , ViewModel.FontSize , ViewModel.FontStyle);

                // fontの最小、最大
                fd.MinSize = 6;
                fd.MaxSize = 30;

                //存在しないフォントの選択禁止
                fd.FontMustExist = true;

                // 縦書きフォントも選べるようにしとく。
                fd.AllowVerticalFonts = true;

                //色の選択は不可
                fd.ShowColor = false;

                //取り消し線、下線、テキストの色などのオプションを指定可能にする
                fd.ShowEffects = true;

                //固定ピッチフォント以外も表示
                fd.FixedPitchOnly = false;

                //ベクタ フォントを選択できるようにする
                fd.AllowVectorFonts = true;

                //ダイアログを表示する
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    if (fd.Font.Name != null && fd.Font.Size > 0)
                    {
                        ViewModel.FontName = fd.Font.Name;
                        ViewModel.FontSize = fd.Font.Size;
                        ViewModel.FontStyle = fd.Font.Style;
                    }
                }
            }
        }
    }
}
