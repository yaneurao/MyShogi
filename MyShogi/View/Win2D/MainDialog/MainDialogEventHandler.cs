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

            // 駒台が縦長のモードのときは、このコントロールは非表示にする。
            // (何か別の方法で描画する)
            kifuControl1.Visible = TheApp.app.config.KomadaiImageVersion == 1;
        }

        // -- 以下、マウスのクリック、ドラッグ(による駒移動)を検知するためのハンドラ
        // クリックイベントは使えないので、MouseDown,MouseUp,MouseMoveからクリックとドラッグを判定する。

        private void MainDialog_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLastDown = InverseAffine(e.Location);
        }

        private void MainDialog_MouseUp(object sender, MouseEventArgs e)
        {
            // affine変換(逆変換)して、盤面座標系にして渡してやる。
            var p = InverseAffine(e.Location);
            
            // 移動がないので、これはクリックイベントとして扱う
            if (mouseLastDown == p)
                BoardClick(p);
            else
                BoardDrag(mouseLastDown, p);
            mouseLastDown = new Point(-1, -1); // また意味のない地点を指すようにしておく
        }

        private void MainDialog_MouseMove(object sender, MouseEventArgs e)
        {
            // ドラッグ中。
            // これは無視する
        }

        /// <summary>
        /// MouseDownが最後に発生した場所
        /// InverseAffine()を通した値になっている。
        /// </summary>
        private Point mouseLastDown = new Point(-1,-1); // 意味のない地点

        // -- 以下、ToolStripのハンドラ

        /// <summary>
        /// 待ったボタンが押されたときのハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            ViewModel.BoardReverse = !ViewModel.BoardReverse;
        }
    }
}
