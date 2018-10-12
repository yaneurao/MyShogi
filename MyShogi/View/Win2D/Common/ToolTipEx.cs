using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 標準のToolTipだとフォントを変更できないため、拡張する。
    /// </summary>
    public class ToolTipEx : ToolTip
    {
        public ToolTipEx()
        {
            Init();
        }

        public ToolTipEx(IContainer container) : base(container)
        {
            Init();
        }

        /// <summary>
        /// フォント
        /// これpublic setterを持つとデザイナがnullを突っ込むので迷惑。
        /// このフォントを変更したいことはないと思うのでprivateにしておく。
        /// </summary>
        private Font Font { get; set; }

        private void Init()
        {
            if (TheApp.app.DesignMode)
                return;

            OwnerDraw = true;
            Draw += new DrawToolTipEventHandler(OnDraw);
            Popup += new PopupEventHandler(OnPopup);

            // デフォルトで、このクラスは設定されているフォントにしておく。
            Font = TheApp.app.Config.FontManager.ToolTip.CreateFont();

            //Debug.Assert(Font != null);
        }

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();

            e.DrawBorder();

            // カスタムテキストの描画


            using (var sf = new StringFormat())
            {

                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                //var rect = new Rectangle(Point.Empty, this.Size);

#if false
                // e.Fontは元のフォント。使っては駄目。
                e.Graphics.DrawString(e.ToolTipText, Font, SystemBrushes.ActiveCaptionText, /*rect*/
                    e.Bounds, sf);
#endif

                // →　paddingするか、DrawText()を用いるかしないとはみ出る。なんぞこれ…。
                //    DrawString()用のサイズではないということか…。
                // cf.
                // Custom OwnerDraw ToolTip size issue
                // https://stackoverflow.com/questions/49199417/custom-ownerdraw-tooltip-size-issue

                TextRenderer.DrawText(e.Graphics, e.ToolTipText, Font, e.Bounds,
                                      ForeColor, Color.Empty,
                                      TextFormatFlags.Default);


            }
        }

        private void OnPopup(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = TextRenderer.MeasureText(this.GetToolTip(e.AssociatedControl), Font);
            Size = e.ToolTipSize;
        }

        /// <summary>
        /// 表示サイズ。
        /// これはPopupハンドラで確定する。
        /// </summary>
        private Size Size;
    }

}
