using System;
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
    }
}
