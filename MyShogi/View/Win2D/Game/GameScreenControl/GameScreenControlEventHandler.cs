using MyShogi.App;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// GameScreenControlのWindowsメッセージ用のイベントハンドラ。
    /// </summary>
    public partial class GameScreenControl
    {
        // 描画用のハンドラ
        private void GameScreenControl_Paint(object sender, PaintEventArgs e)
        {
            // ここで弾いておかないとVisual Studioのデザイナで貼り付けた時にエラーになる。
            if (!TheApp.app.DesignMode)
                OnDraw(e.Graphics);
        }

        private void GameScreenControl_SizeChanged(object sender, System.EventArgs e)
        {
            if (TheApp.app.DesignMode)
                return;

            // 画面サイズに合わせてaffine行列を設定する。
            FitToClientSize();

            // 画面サイズに合わせて棋譜ウィンドウを設定する。
            ResizeKifuControl();

            // affine行列を変更したので今回dirtyになっていないrectに関しても強制的な再描画が必要。
            ForceRedraw();
        }

        private void GameScreenControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseLastDown = e.Location;

                // この時点でクリックイベントとして扱って良いのでは…。
                OnClick(e.Location);
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 右クリックによるドラッグは操作として存在しない。
                // これはクリックイベントとみなす。
                OnRightClick(e.Location);
            }
        }

        private void GameScreenControl_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e.Location);
        }

        private void GameScreenControl_MouseUp(object sender, MouseEventArgs e)
        {
            var p = e.Location;

            if (e.Button == MouseButtons.Left)
            {
#if false
                // 移動がないので、これはクリックイベントとして扱う
                if (mouseLastDown == p)
                    OnClick(p);
                else
                    OnDrag(mouseLastDown, p);
#endif

                if (mouseLastDown != p)
                {
                    OnClick(p , true); // 2点クリックされたかのように扱う
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 右クリックによるドラッグは操作として存在しない。
            }

            mouseLastDown = new Point(-1, -1); // また意味のない地点を指すようにしておく
        }


        /// <summary>
        /// MouseDownが最後に発生した場所
        /// </summary>
        private Point mouseLastDown = new Point(-1, -1); // 意味のない地点

    }
}
