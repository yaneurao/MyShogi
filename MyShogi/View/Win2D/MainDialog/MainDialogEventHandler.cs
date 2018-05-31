using System;
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

        private void MainDialog_SizeChanged(object sender, EventArgs e)
        {
            // 画面のフルスクリーン化/ウィンドゥ化がなされたので、OnPaintが呼び出されるようにする。
            Invalidate();
        }

        private void MainDialog_Resize(object sender, EventArgs e)
        {
            // 画面がリサイズされたときにそれに収まるように盤面を描画する。
            FitToClientSize();
            Invalidate();
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
