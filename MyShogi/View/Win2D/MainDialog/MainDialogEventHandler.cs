using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// MainDialogのイベントハンドラ関係
    /// </summary>
    public partial class MainDialog
    {
        // -- 以下、このフォームの管理下にあるDialog

        /// <summary>
        /// 「やねうら王について」のダイアログ
        /// </summary>
        public Form AboutDialog;

        // -- 以下、Windows Messageのイベントハンドラ

        // 画面のフルスクリーン化/ウィンドゥ化がなされたので、OnPaintが呼び出されるようにする。
        private void MainDialog_SizeChanged(object sender, EventArgs e)
        {
            ScreenRedraw();
        }

        // 画面がリサイズされたときにそれに収まるように盤面を描画する。
        private void MainDialog_Resize(object sender, EventArgs e)
        {
            ScreenRedraw();
        }

        /// <summary>
        /// ウィンドウのリサイズ、最大化、窓化したときに
        /// このFormに配置してあるコントロールの位置などを調整する。
        /// </summary>
        private void ScreenRedraw()
        {
            // 画面に合わせたaffine行列を求める
            FitToClientSize();

            // 棋譜コントロールの移動とリサイズ
            ResizeKifuControl();

            // OnPaintが発生するようにする。
            Invalidate();
        }

        /// <summary>
        /// Formのリサイズに応じて棋譜コントロールの移動などを行う。
        /// </summary>
        private void ResizeKifuControl()
        {
            var point = new Point(229, 600);
            kifuControl1.Location = Affine(point);
            var size = new Size(265, 423);
            kifuControl1.Size = Affine(size);

            // kifuControl内の文字サイズも変更しないといけない。
            // あとで考える。
        }

        // -- 以下、ToolStripのハンドラ

        /// <summary>
        /// 待ったボタンが押されたときのハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            var config = TheApp.app.config;
            config.BoardReverse = !config.BoardReverse;
        }
    }
}
