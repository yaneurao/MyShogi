using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// ToolStripExと同じ理由により作ったもの。
    /// </summary>
    class MenuStripEx : MenuStrip
    {
        /// <summary>
        /// このToolStripのクリックされたときの動作。
        /// 上の4つの定数から選ぶ。MA_NOACTIVATEがこのToolStripExのデフォルト。
        /// </summary>
        public ClickActionEnum ClickAction { get; set; } = ClickActionEnum.MA_NOACTIVATE;

        protected const int WM_MOUSEACTIVATE = 0x0021;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_MOUSEACTIVATE && m.Result == (IntPtr)ClickActionEnum.MA_ACTIVATEANDEAT)
            {
                // このメッセージを書き換える。

                m.Result = (IntPtr)ClickAction;
            }
        }

#if MACOS
        /// <summary>
        /// Mac用のMonoだと、ToolStripMenuItem.Fontがambient propertyになっていないので、
        /// MenuStripのFontを置き換えたときに反映しない。(Monoの実装上のバグ)
        /// これを回避するためにFont propertyを生やす。
        /// </summary>
        public override Font Font
        {
            get { return base.Font; }
            set {
                base.Font = value;
                foreach (var item in Items)
                {
                    if (item is ToolStripMenuItem menuItem)
                    {
                        menuItem.Font = value;
                    }
                }
            }
        }
#endif
    }
}
