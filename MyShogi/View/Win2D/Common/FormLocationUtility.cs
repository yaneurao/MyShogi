using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// Windows.Formの位置調整などに用いるUtility
    /// </summary>
    public static class FormLocationUtility
    {
        /// <summary>
        /// あるformをthis_のFormに対してセンタリングする。
        /// formをShowDialog()する前に呼び出す。
        ///
        /// 画面外には出ないように慎重に位置決めしている。
        /// </summary>
        /// <param name="form"></param>
        public static void CenteringToThisForm(Form form, Form this_)
        {
            form.StartPosition = FormStartPosition.Manual;

            var x = this_.DesktopLocation.X + (this_.Width - form.Width) / 2;
            var y = this_.DesktopLocation.Y + (this_.Height - form.Height) / 2;

            // 入り切らないときは左上にめり込むのは許せないので(ウインドウをドラッグする操作、閉じる操作ができなくなるので)
            // 左と上にはめりこまないようにする。

            // そのためには、まず下と右にめり込んでいたなら押し返して、そのあと上と左にめり込んでいたなら押し返す。

            var screenWorkingArea = ScreenWorkingArea(this_);

            if (x + form.Width > screenWorkingArea.Width)
                x = screenWorkingArea.Width - form.Width;
            if (x < screenWorkingArea.X)
                x = screenWorkingArea.X;

            if (y + form.Height > screenWorkingArea.Height)
                y = screenWorkingArea.Height - form.Height;
            if (y < screenWorkingArea.Y)
                y = screenWorkingArea.Y;

            form.DesktopLocation = new Point(x, y);
        }

        /// <summary>
        /// 点p(デスクトップ座標において)が属するスクリーン(≒ディスプレイ)の矩形領域を返す。
        ///
        /// 属するスクリーンが見つからない時はメインスクリーンを返す。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Rectangle ScreenWorkingArea(Point p)
        {
            Rectangle result = Screen.PrimaryScreen.WorkingArea;
            foreach(var r in Screen.AllScreens)
                if (r.WorkingArea.Contains(p))
                {
                    result = r.WorkingArea;
                    break;
                }

            return result;
        }

        /// <summary>
        /// あるControl(Form)が存在するスクリーン(≒ディスプレイ)の矩形領域を返す。
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Rectangle ScreenWorkingArea(Control f)
        {
            // そのフォームの中央らへんがどのスクリーンに属するのか判定して、そのScreenAreaを返せば良い。
            var p = new Point(f.Location.X + f.Width / 2, f.Location.Y + f.Height / 2);
            return ScreenWorkingArea(p);
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
