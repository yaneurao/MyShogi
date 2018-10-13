using MyShogi.Model.Common.Collections;
using System;
using System.Drawing;
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
    /// 
    /// あと、標準のToolStripだとToolTipをカスタマイズできないのでToolTipを自前で持つ。
    /// cf.
    /// https://www.codeproject.com/Articles/376643/ToolStrip-with-Custom-ToolTip
    /// </summary>
    public partial class ToolStripEx : ToolStrip
    {
        /// <summary>
        /// このToolStripのクリックされたときの動作。
        /// 上の4つの定数から選ぶ。MA_NOACTIVATEがこのToolStripExのデフォルト。
        /// </summary>
        public ClickActionEnum ClickAction { get; set; } = ClickActionEnum.MA_NOACTIVATE;

        // これMonoでもうまく動くのかどうかわからんが、たぶん動くのかな…。

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

        // -- 以下、Custom Tooltipのための実装

        /// <summary>
        /// ToolTipが表示されている時間[ms]
        /// </summary>
        public int ToolTipInterval = int.MaxValue;
        
        /// <summary>
        /// ToolTip本体
        /// 
        /// null突っ込むの禁止。
        /// デフォルトでToolTipExが代入されている。
        /// </summary>
        public ToolTipEx ToolTip { get; set; }

        public ToolStripEx() : base()
        {
            // 標準のToolTip
            ShowItemToolTips = false;
            timer = new Timer();
            timer.Enabled = false;

            // ToolTipが表示されるまでに要する時間。[ms]
            timer.Interval = SystemInformation.MouseHoverTime;
            timer.Tick += new EventHandler(timer_Tick);

            ToolTip = new ToolTipEx();
        }

        protected override void OnMouseMove(MouseEventArgs mea)
        {
            base.OnMouseMove(mea);

            // マウスを留めているところにあるItemの取得
            var newMouseOverItem = this.GetItemAt(mea.Location);
            if (mouseOverItem != newMouseOverItem ||
                (Math.Abs(mouseOverPoint.X - mea.X) > SystemInformation.MouseHoverSize.Width ||
                (Math.Abs(mouseOverPoint.Y - mea.Y) > SystemInformation.MouseHoverSize.Height))
                )
            {
                mouseOverItem = newMouseOverItem;
                mouseOverPoint = mea.Location;
                ToolTip.Hide(this);
                timer.Stop();
                timer.Start();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            var newMouseOverItem = this.GetItemAt(e.Location);
            if (newMouseOverItem != null)
                ToolTip.Hide(this);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            timer.Stop();
            ToolTip.Hide(this);
            mouseOverPoint = new Point(-50, -50);
            mouseOverItem = null;
        }

        /// <summary>
        /// マウスを一定時間留めたのでToolTipを表示する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            try
            {
                var currentMouseOverPoint = this.PointToClient(new Point(
                    Control.MousePosition.X,
                    Control.MousePosition.Y + Cursor.Current.Size.Height - Cursor.Current.HotSpot.Y));

                if (mouseOverItem != null && !mouseOverItem.ToolTipText.Empty())
                {
                    if ((!(mouseOverItem is ToolStripDropDownButton) && !(mouseOverItem is ToolStripSplitButton)) ||
                        ((mouseOverItem is ToolStripDropDownButton) && !((ToolStripDropDownButton)mouseOverItem).DropDown.Visible) ||
                        (((mouseOverItem is ToolStripSplitButton) && !((ToolStripSplitButton)mouseOverItem).DropDown.Visible)))
                    {
                        ToolTip.Show(mouseOverItem.ToolTipText, this, currentMouseOverPoint, ToolTipInterval);
                    }
                }
            }
            catch
            { }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                timer.Dispose();
                ToolTip.Dispose();
            }
        }

        private Timer timer;
        private Point mouseOverPoint;
        private ToolStripItem mouseOverItem = null;
    }
}
