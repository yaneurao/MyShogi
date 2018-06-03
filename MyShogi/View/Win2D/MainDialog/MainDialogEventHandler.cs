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
            // まだ初期化(GameScreenのbind)が終わっていない段階での呼び出しは無視。
            // ※　InitalizeComponent()から抜ける前にresizeイベントが発生したなど。
            if (gameScreen == null)
                return;

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
            gameScreen.ResizeKifuControl();
        }

        /// <summary>
        /// 定期的に呼び出されるタイマー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            // 自分が保有しているScreenがdirtyになっていることを検知したら、Invalidateを呼び出す。
            if (gameScreen.Dirty)
                Invalidate();

            // ここでInvalidate()にScreenに対応する(Screenのなかに含まれる)Rectangleを渡して、
            // 特定のgameScreenだけを再描画するようにしないと、GameScreenを画面上に16個ぐらい
            // 描画するときに非常に重くなる。

            // TODO : マルチスクリーン対応のときにちゃんと書く
        }

        // -- 以下、マウスのクリック、ドラッグ(による駒移動)を検知するためのハンドラ
        // クリックイベントは使えないので、MouseDown,MouseUp,MouseMoveからクリックとドラッグを判定する。

        private void MainDialog_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLastDown = e.Location;
        }

        private void MainDialog_MouseUp(object sender, MouseEventArgs e)
        {
            var p = e.Location;
            
            // 移動がないので、これはクリックイベントとして扱う
            if (mouseLastDown == p)
                gameScreen.OnClick(p);
            else
                gameScreen.OnDrag(mouseLastDown, p);

            mouseLastDown = new Point(-1, -1); // また意味のない地点を指すようにしておく
        }

        private void MainDialog_MouseMove(object sender, MouseEventArgs e)
        {
            // ドラッグ中。
            // これは無視する
        }

        /// <summary>
        /// MouseDownが最後に発生した場所
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
            var config = TheApp.app.config;
            config.BoardReverse = !config.BoardReverse;
        }
    }
}
