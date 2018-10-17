using System.ComponentModel;
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

            // フォントの変更は即時反映に。
            TheApp.app.Config.FontManager.AddPropertyChangedHandler("FontChanged", FontChanged);

            //Debug.Assert(Font != null);
        }

        public new void Dispose()
        {
            base.Dispose();

            // 追加したハンドラ、削除しとかないと。
            TheApp.app.Config.FontManager.RemovePropertyChangedHandler("FontChanged", FontChanged);
        }

        /// <summary>
        /// フォント変更を即時反映にするためのハンドラ
        /// </summary>
        /// <param name="args"></param>
        private void FontChanged(Model.Common.ObjectModel.PropertyChangedEventArgs args)
        {
            var s = (string)args.value;
            if (s == "ToolTip")
                Font = TheApp.app.Config.FontManager.ToolTip.CreateFont();
        }

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();

            e.DrawBorder();

            // カスタムテキストの描画

#if false
            using (var sf = new StringFormat())
            {

                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                //var rect = new Rectangle(Point.Empty, this.Size);

                // e.Fontは元のフォント。使っては駄目。
                e.Graphics.DrawString(e.ToolTipText, Font, SystemBrushes.ActiveCaptionText, /*rect*/
                    e.Bounds, sf);

            // →　paddingするか、DrawText()を用いるかしないとはみ出る。なんぞこれ…。
            //    DrawString()用のサイズではないということか…。
            // cf.
            // Custom OwnerDraw ToolTip size issue
            // https://stackoverflow.com/questions/49199417/custom-ownerdraw-tooltip-size-issue
        }
#endif

            var flags = TextFormatFlags.LeftAndRightPadding | TextFormatFlags.VerticalCenter;
                TextRenderer.DrawText(e.Graphics, e.ToolTipText, Font, e.Bounds,
                                      ForeColor, Color.Empty,
                                      flags /*TextFormatFlags.Default*/
                                      );

        }

        private void OnPopup(object sender, PopupEventArgs e)
        {
            var size = TextRenderer.MeasureText(this.GetToolTip(e.AssociatedControl), Font);
            size = new Size(size.Width + 8 , size.Height + 8); // 少しpaddingしておく。
            e.ToolTipSize = size;
        }

    }

}
