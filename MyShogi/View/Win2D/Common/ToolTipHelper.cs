using MyShogi.App;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 標準のTooltipはEnableがfalseのものに対して反応しない。これを拡張する。
    /// </summary>
    public class ToolTipHelper
    {
        /// <summary>
        /// Mouseの移動イベントに対するハンドラ。
        ///
        /// private void EngineOptionSettingControl_MouseMove(object sender, MouseEventArgs e)
        /// {
        ///    tooltipHelper.OnMouseMove(this, this.toolTip1, e.Location);
        /// }
        /// のようにして使う。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="toolTip"></param>
        /// <param name="mouseLocation"></param>
        public void OnMouseMove(Control control , ToolTip toolTip , Point mouseLocation)
        {
            // 無効化されているcontrolの上でtooltipが無効化されるので、それをなんとかする。
            // cf. https://stackoverflow.com/questions/1732140/displaying-tooltip-over-a-disabled-control

            // Visible == falseにして重ねてあるとGetChildAtPointが使えない。なんぞこれ…。自前で判定する。

            Control child = null;
            foreach (Control c in control.Controls)
                if (c.Visible /* 可視でないと駄目 */ && c.Bounds.Contains(mouseLocation))
                {
                    child = c;
                    break;
                }

            if (child != null)
            {
                if (!child.Enabled && currentTooptipControl == null)
                {
                    var tipText = toolTip.GetToolTip(child);
                    if (tipText != null && tipText.Length > 0)
                    {
                        // 邪魔にならないように真ん中あたりに表示する。
                        toolTip.Show(tipText, child, child.Width / 2, child.Height / 2);
                        currentTooptipControl = child;
                    }
                }
            }
            else
            {
                if (currentTooptipControl != null)
                {
                    toolTip.Hide(currentTooptipControl);
                    currentTooptipControl = null;
                }
            }
        }

        /// <summary>
        /// いま表示しているTooltip
        /// </summary>
        private Control currentTooptipControl;

    }

}
