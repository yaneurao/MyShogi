using System.Drawing;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 画面のフィットさせるコード
    /// </summary>
    public partial class MainDialog
    {
        /// <summary>
        /// メニュー高さとToolStripの高さをあわせたもの。
        /// これはClientSize.Heightに含まれてしまうので実際の描画エリアはこれを減算したもの。
        /// </summary>
        public int menu_height
        {
            get
            {
                return System.Windows.Forms.SystemInformation.MenuHeight + toolStrip1.Height;
            }
        }

        /// <summary>
        /// 現在のスクリーンの大きさに合わせたウィンドウサイズにする。(起動時)
        /// </summary>
        public void FitToScreenSize()
        {
            // ディスプレイに収まるサイズのスクリーンにする必要がある。
            // プライマリスクリーンを基準にして良いのかどうかはわからん…。
            int w = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int h = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - menu_height;

            // いっぱいいっぱいだと邪魔なので90%ぐらい使うか。
            w = (int)(w * 0.9);
            h = (int)(h * 0.9);

            // 縦(h)を基準に横方向のクリップ位置を決める
            // 1920 * 1080が望まれる比率
            int w2 = h * 1920 / 1080;

            if (w > w2)
            {
                w = w2;
                // 横幅が余りつつも画面にぴったりなのでこのサイズで生成する。
            }
            else
            {
                int h2 = w * 1080 / 1920;
                h = h2;
            }
            ClientSize = new Size(w, h + menu_height);

            //set_komadai();
        }

        /// <summary>
        /// 現在のウィンドウサイズに収まるようにaffine変換の係数を決定する。
        /// </summary>
        public void FitToClientSize()
        {
            var screenSize = new Rectangle(
                0, menu_height,
                ClientSize.Width,
                ClientSize.Height - menu_height
                );

            gameScreen.FitToClientSize(screenSize);
        }

    }
}
