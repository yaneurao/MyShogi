using System;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// .NET FrameworkのToolStrip、非アクティブ状態からだとボタンクリックが反応しなくて不便なので
    /// 非アクティブ状態からでも反応するものを用意。
    /// 
    /// cf.
    /// http://bbs.wankuma.com/index.cgi?mode=al2&namber=69912&KLOG=119
    /// https://blogs.msdn.microsoft.com/rickbrew/2006/01/09/how-to-enable-click-through-for-net-2-0-toolstrip-and-menustrip/
    /// </summary>
    public partial class ToolStripEx : ToolStrip
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
