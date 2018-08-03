using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Common
{
    /// <summary>
    /// Windows.Formの位置調整などに用いるUtility
    /// </summary>
    public static class FormLocationUtility
    {
        /// <summary>
        /// あるformをthis_のFormに対してセンタリングする。
        /// formをShowDialog()する前に呼び出す。
        /// </summary>
        /// <param name="form"></param>
        public static void CenteringToThisForm(Form form, Form this_)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.DesktopLocation = new Point(
                this_.DesktopLocation.X + (this_.Width - form.Width) / 2,
                this_.DesktopLocation.Y + (this_.Height - form.Height) / 2
                );
        }

        /// <summary>
        /// Desktopに対して、formを、上からx_percent , 左から y_percentの位置に配置するための座標を返す。
        /// x_percent = y_percent = 50ならデスクトップに対してセンタリング。
        /// 
        /// ※　form.Sizeが確定後に呼び出すこと。
        /// </summary>
        /// <param name="form"></param>
        /// <param name="x_percent"></param>
        /// <param name="y_percent"></param>
        /// <returns></returns>
        public static Point DesktopLocation(Form form , float x_percent,float y_percent)
        {
            return new Point(
                    (int)((Screen.PrimaryScreen.WorkingArea.Width  - form.Width ) * x_percent / 100 ),
                    (int)((Screen.PrimaryScreen.WorkingArea.Height - form.Height) * y_percent / 100 )
                );
        }

    }
}
