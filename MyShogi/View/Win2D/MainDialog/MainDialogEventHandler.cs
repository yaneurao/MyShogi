using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using SCore = MyShogi.Model.Shogi.Core;

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

        // -- WM_PAINTのハンドラ

        /// <summary>
        /// 対局盤面の描画関係のコード一式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_Paint(object sender, PaintEventArgs e)
        {
            // メニューが変更された時には、メニューのRegionに対して再描画イベントが来る。
            // その時は、gameScreen.OnDraw()で処理してはならない。
            // (更新してしまうと、Dirtyフラグをfalseにするのだが、実際はe.ClipRectangleがメニューの領域に
            // 設定されているので画面に描画されない。このため、画面に描画していないのにDirtyフラグをfalseにしてしまう。)

            var rect = e.ClipRectangle;

            // メニューより下の領域が更新対象であるかのチェック
            if (rect.Y >= menu_height)
            {
                // 描画は丸ごと、GameScreenに移譲してある。
                // マルチスクリーン対応にするときは、GameScreenのインスタンスを作成して、それぞれに移譲すれば良い。
                gameScreen.OnDraw(e.Graphics);
            }
        }

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
        /// 
        /// このタイマーは15msごとに呼び出される。
        /// dirtyフラグが立っていなければ即座に帰るのでさほど負荷ではないという考え。
        /// 
        /// 1000ms / 60fps ≒ 16.67 ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (first_tick)
            {
                // コンストラクタでの初期化が間に合わなかったコントロールの初期化はここで行う。
                first_tick = false;
                ResizeKifuControl();
            }

            // 自分が保有しているScreenがdirtyになっていることを検知したら、Invalidateを呼び出す。
            if (gameScreen.Dirty)
                Invalidate();

            // ここでInvalidate()にScreenに対応する(Screenのなかに含まれる)Rectangleを渡して、
            // 特定のgameScreenだけを再描画するようにしないと、GameScreenを画面上に16個ぐらい
            // 描画するときに非常に重くなる。

            // TODO : マルチスクリーン対応のときにちゃんと書く
        }

        private bool first_tick = true;

        /// <summary>
        /// フォームが閉じるときのハンドラ
        /// ViewModel側のDispose()を呼び出して終了処理を行う。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.Dispose();
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
            gameScreen.OnMouseMove(e.Location);
        }

        /// <summary>
        /// MouseDownが最後に発生した場所
        /// </summary>
        private Point mouseLastDown = new Point(-1,-1); // 意味のない地点

        // -- 以下、ToolStripのハンドラ

        /// <summary>
        /// ボタンの有効/無効を切り替えるためのハンドラ
        /// ボタンの番号が変わった時に呼び出し側を書き直すのが大変なので、
        /// 名前で解決するものとする。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="enable"></param>
        private void SetButton(MainDialogButtonEnum name , bool enable)
        {
            ToolStripButton btn;
            switch (name)
            {
                case MainDialogButtonEnum.RESIGN: btn = this.toolStripButton1; break;
                case MainDialogButtonEnum.UNDO_MOVE: btn = this.toolStripButton2; break;
                case MainDialogButtonEnum.MOVE_NOW: btn = this.toolStripButton3; break;
                case MainDialogButtonEnum.INTERRUPT: btn = this.toolStripButton4; break;
                default: btn = null; break;
            }

            // 希望する状態と現在の状態が異なるなら、この時だけ更新する。
            if (btn.Enabled != enable)
            {
                Invoke(new Action(() =>
                {
                    btn.Enabled = enable;
                }));
            }
        }

        /// <summary>
        /// 「投」ボタン。投了の処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, System.EventArgs e)
        {
            var gameServer = ViewModel.gameServer;
            // 受理されるかどうかは知らん
            gameServer.DoMoveFromUI(SCore.Move.RESIGN);
        }

        /// <summary>
        /// 「待」ボタン。待ったの処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, System.EventArgs e)
        {
            var gameServer = ViewModel.gameServer;
            gameServer.UserUndo();
        }

        /// <summary>
        /// 「急」ボタン。いますぐに指させる。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, System.EventArgs e)
        {
            var gameServer = ViewModel.gameServer;
            gameServer.MoveNow();
        }

        /// <summary>
        /// 「転」ボタン。盤面反転の処理。
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
