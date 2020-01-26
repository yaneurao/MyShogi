using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.Model.Shogi.Usi;
using SCore = MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局盤面などがあるメインウィンドゥ
    ///
    /// イベントハンドラだけここに集めてある。
    /// </summary>
    public partial class MainDialog : Form
    {
        // -- 以下、Windows Messageのイベントハンドラ

        /// <summary>
        /// [UI thread] : 定期的に呼び出されるタイマー
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

                // マウスのホイールイベントまわりの初期化
                InitMouseWheel();

                // 棋譜ウィンドウの更新通知のタイミングがなかったのでupdate
                gameServer.RaisePropertyChanged("KifuList", gameServer.KifuList);

                // メニューもGameServerが初期化されているタイミングで更新できていなかったのでupdate
                UpdateMenuItems();

                // コマンドラインでファイルの読み込みが予約されていればそれを実行する。
                CommandLineCheck();

                // 棋譜ウインドウ・検討ウインドウのfloating状態を起動時に復元する。
                var config = TheApp.app.Config;
                config.KifuWindowDockManager.RaisePropertyChanged("DockState", config.KifuWindowDockManager.DockState);
                config.EngineConsiderationWindowDockManager.RaisePropertyChanged("DockState", config.EngineConsiderationWindowDockManager.DockState);
                config.MiniShogiBoardDockManager.RaisePropertyChanged("DockState", config.MiniShogiBoardDockManager.DockState);
                config.EvalGraphDockManager.RaisePropertyChanged("DockState", config.EvalGraphDockManager.DockState);

                // メインウインドウが表示されるまで、棋譜ウインドウの座標設定などを抑制していたのでここでメインウインドウ相対で移動させてやる。
                UpdateDockedWindowLocation();
            }

            // 自分が保有しているScreenがdirtyになっていることを検知したら、Invalidateを呼び出す。
            if (gameScreenControl1.Dirty)
                gameScreenControl1.Invalidate();

            // 検討ウインドウはmessage dispatcherのために自前でOnIdle()を呼び出す必要があるる
            engineConsiderationMainControl.OnIdle();

            // 持ち時間描画だけの部分更新
            // あとでちゃんと書き直す
            //if (gameScreen.DirtyRestTime)
            //    Invalidate(new Rectangle(100, 100, 1, 1));

            // TODO : マルチスクリーン対応のときにちゃんと書く
            // GameScreenControlをきちんとコンポーネント化したので、書きやすいはず…。
        }

        private bool first_tick = true;

#if false
        /// <summary>
        /// メインウインドウには棋譜ウインドウしかないので、そのキーイベントを取得したい。
        /// PreviewKeyDownで取らないとSpaceキーが取れない。(ListViewに食われてしまう)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_KeyDown(object sender, KeyEventArgs e)
        {
            // メインウインドウのメニューに登録されているキーボードショートカットをハンドルする。
            TheApp.app.KeyShortcut.KeyDown(sender, e);
        }
#endif

        /// <summary>
        /// KeyDownではカーソルキーがControlなどに食われてしまって処理できない。
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // cf. https://stackoverflow.com/questions/1646998/up-down-left-and-right-arrow-keys-do-not-trigger-keydown-event
            if (TheApp.app.KeyShortcut.ProcessCmdKey(ref msg, keyData))
                return true; // 処理済みとして扱う。

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// マウスのWheelイベントの初期化
        /// </summary>
        private void InitMouseWheel()
        {
            this.MouseWheel += MainDialog_MouseWheel;
        }

        // --

        private void MainDialog_MouseWheel(object sender, MouseEventArgs e)
        {
            // ShogiGUIのように逆方向スクロールに設定
            if (e.Delta > 0)
                toolStripButton9_Click(sender, e); // 1手戻るボタンに委譲
            else if (e.Delta < 0)
                toolStripButton10_Click(sender, e); // 1手進むボタンに委譲
        }

        public void MainDialog_Move(object sender, System.EventArgs e)
        {
            SizeOrLocationChanged();
        }

        private void MainDialog_Resize(object sender, System.EventArgs e)
        {
            SizeOrLocationChanged();
            ResizeConsiderationControl();
        }

        private void SizeOrLocationChanged()
        {
            UpdateDockedWindowLocation();
            SaveWindowSizeAndPosition();
        }

        /// <summary>
        /// ウィンドウを移動させたときなどに、そこの左下に検討ウィンドウを追随させる。
        /// </summary>
        private void UpdateDockedWindowLocation()
        {
            // まだメインウインドウが表示される前なので他のウインドウをいまのメインウインドウの位置を基準として移動させるとまずいことになる。
            if (first_tick)
                return;

            // 棋譜ウインドウ
            {
                if (kifuDockWindow != null)
                {
                    var dockManager = TheApp.app.Config.KifuWindowDockManager;
                    dockManager.UpdateDockWindowLocation(this, kifuDockWindow);
                }
            }

            // 検討ウインドウ
            {
                if (engineConsiderationDockWindow != null)
                {
                    var dockManager = TheApp.app.Config.EngineConsiderationWindowDockManager;
                    dockManager.UpdateDockWindowLocation(this, engineConsiderationDockWindow);
                }
            }

            // ミニ盤面
            {
                if (miniShogiBoardDockWindow != null)
                {
                    var dockManager = TheApp.app.Config.MiniShogiBoardDockManager;
                    dockManager.UpdateDockWindowLocation(this, miniShogiBoardDockWindow);
                }
            }

            // 評価値グラフ
            {
                if (evalGraphDialog != null)
                {
                    var dockManager = TheApp.app.Config.EvalGraphDockManager;
                    dockManager.UpdateDockWindowLocation(this, evalGraphDialog);
                }
            }

        }

        /// <summary>
        /// メニュー高さとToolStripの高さをあわせたもの。
        /// これはClientSize.Heightに含まれてしまうので実際の描画エリアはこれを減算したもの。
        /// </summary>
        private int menu_height
        {
            get
            {
                return SystemInformation.MenuHeight + toolStrip1.Height;
            }
        }

        /// <summary>
        /// 現在のデスクトップのサイズに合わせて画面サイズにしてやる。(起動時用)
        /// </summary>
        public void FitToScreenSize()
        {
            // 前回起動時のサイズが記録されているならそれを復元してやる。
            var size = TheApp.app.Config.MainDialogClientSize;
            if (size.Width < 192 || size.Height < 108)
                size = Size.Empty;

            if (size.IsEmpty)
            {
                // ディスプレイに収まるサイズのスクリーンにする必要がある。

                // プライマリスクリーンを基準にして良いのかどうかはわからんが、
                // 初回起動なのでとりあえずプライマリスクリーンに表示させるしかないので、そこを基準に考える。

                int w = Screen.PrimaryScreen.Bounds.Width;
                int h = Screen.PrimaryScreen.Bounds.Height - menu_height;

                // いっぱいいっぱいだと邪魔なので70%ぐらい使う。(検討ウィンドウのこともあるので…)
                w = (int)(w * 0.7);
                h = (int)(h * 0.7);

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
            }
            else
            {
                ClientSize = size;
            }

            MinimumSize = new Size(192 * 2, 108 * 2 + menu_height);

            // 前回のスクリーンの表示位置を復元する。
            var desktopLocation = TheApp.app.Config.DesktopLocation;
            bool reset = false; // 位置を初期化するのかのフラグ

            if (desktopLocation == null)
            {
                reset = true;

            }
            else
            {

                // これが現在のいずれかの画面上であることを保証しなくてはならない。
                foreach (var s in Screen.AllScreens)
                    if (s.Bounds.Left <= desktopLocation.Value.X && desktopLocation.Value.X < s.Bounds.Right &&
                        s.Bounds.Top  <= desktopLocation.Value.Y && desktopLocation.Value.Y < s.Bounds.Bottom)
                        goto Ok;
                reset = true;
            }

            Ok:

            if (reset)
                // 表示される位置があまりデスクトップの下の方だとウィンドウが画面下にめり込んでしまうのでデスクトップに対してセンタリングする。
                // →　検討ウィンドウの表示のことを考えて、少し上らへんにする。
                desktopLocation = FormLocationUtility.DesktopLocation(this, 50, 25); // Desktopに対して左から50%(center),25%(上寄り)にする。

            DesktopLocation = desktopLocation.Value;
        }

        /// <summary>
        /// Formの描画前の初期化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_Load(object sender, System.EventArgs e)
        {
            // 現在のデスクトップの画面サイズに合わせてリサイズ
            UpdateMenuItems(); // これを先にやっておかないとメニュー高さの計算が狂う。
            FitToScreenSize();
        }

        /// <summary>
        /// LocalGameServerから送られてくるエンジンの読み筋などのハンドラ。
        /// </summary>
        private void ThinkReportChanged(PropertyChangedEventArgs args)
        {
            var message = args.value as UsiThinkReportMessage;
            engineConsiderationMainControl.EnqueueThinkReportMessage(message);

            // 評価値グラフの更新など
            gameServer.ThinkReportChangedCommand(message);

            var dockManager = TheApp.app.Config.EvalGraphDockManager;
            if (!dockManager.Visible)
            {
                return;
            }

            if (dockManager.DockState == Model.Common.Tool.DockState.InTheMainWindow)
            {
                var graphData = gameServer.GetEvaluationGraphDataCommand(Model.Shogi.Data.EvaluationGraphType.TrigonometricSigmoid, false);
                evalGraphControl.OnEvalDataChanged(new Model.Common.ObjectModel.PropertyChangedEventArgs("EvalData", graphData));
            }
            else if (!evalGraphDialog.IsDisposed)
            {
                evalGraphDialog.DispatchEvalGraphUpdate(gameServer);
            }
        }

        /// <summary>
        /// 棋譜ウィンドウの横幅が設定で変更になった時に棋譜ウィンドウを実際にリサイズする。
        /// </summary>
        public void ResizeKifuControl(PropertyChangedEventArgs args)
        {
            gameScreenControl1.ResizeKifuControl();
        }

        /// <summary>
        /// [UI Thread] : Ctrl+V による棋譜の貼り付け
        /// </summary>
        public void CopyFromClipboard()
        {
            if (gameScreenControl1.gameServer.GameMode == GameModeEnum.ConsiderationWithoutEngine)
            {
                if (gameScreenControl1.gameServer.KifuDirty)
                {
                    if (TheApp.app.MessageShow("未保存の棋譜が残っていますが、本当に棋譜を貼り付けますか？", MessageShowType.WarningOkCancel)
                        != DialogResult.OK)
                        return;
                }

                // クリップボードからテキスト取得

                // GetText()はUI Threadの制約があるので注意。
                var text = SafeClipboard.GetText();
                gameServer.KifuReadCommand(text);
                ViewModel.LastFileName = null;
            }
        }

        // -- 以下、ToolStripのハンドラ

        /// <summary>
        /// [UI thread] : ボタンの有効/無効を切り替えるためのハンドラ
        /// ボタンの番号が変わった時に呼び出し側を書き直すのが大変なので、
        /// 名前で解決するものとする。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="enable"></param>
        private void SetButton(MainDialogToolStripButtonEnum name, bool enable)
        {
            ToolStripButton btn;
            switch (name)
            {
                case MainDialogToolStripButtonEnum.RESIGN: btn = this.toolStripButton1; break;
                case MainDialogToolStripButtonEnum.UNDO_MOVE: btn = this.toolStripButton2; break;
                case MainDialogToolStripButtonEnum.MOVE_NOW: btn = this.toolStripButton3; break;
                case MainDialogToolStripButtonEnum.INTERRUPT: btn = this.toolStripButton4; break;
                case MainDialogToolStripButtonEnum.REWIND: btn = this.toolStripButton9; break;
                case MainDialogToolStripButtonEnum.FORWARD: btn = this.toolStripButton10; break;
                case MainDialogToolStripButtonEnum.MAIN_BRANCH: btn = this.toolStripButton11; break;
                default: btn = null; break;
            }

            // 希望する状態と現在の状態が異なるなら、この時だけ更新する。
            if (btn.Enabled != enable)
                btn.Enabled = enable;
        }

        /// <summary>
        /// 「投」ボタン。投了の処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, System.EventArgs e)
        {
            // 受理されるかどうかは知らん
            gameScreenControl1.DoMoveCommand(SCore.Move.RESIGN);
        }

        /// <summary>
        /// 「待」ボタン。待ったの処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, System.EventArgs e)
        {
            gameServer.UndoCommand();
        }

        /// <summary>
        /// 「急」ボタン。いますぐに指させる。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, System.EventArgs e)
        {
            gameServer.MoveNowCommand();
        }

        /// <summary>
        /// 「中」ボタン。対局の中断。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, System.EventArgs e)
        {
            gameServer.GameInterruptCommand();
        }

        /// <summary>
        /// 「検」ボタン。検討モードに入る。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, System.EventArgs e)
        {
            ToggleConsideration();
        }

        /// <summary>
        /// 「解」ボタン。棋譜解析。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, System.EventArgs e)
        {
            // とりま未実装なので取り除いておいた。
            // あとで実装する。
        }

        /// <summary>
        /// 「詰」ボタン。詰みがあるかどうかを調べる。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton7_Click(object sender, System.EventArgs e)
        {
            ToggleMateConsideration();
        }

        /// <summary>
        /// 「転」ボタン。盤面反転の処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, System.EventArgs e)
        {
            gameServer.BoardReverse ^= true;
        }

        /// <summary>
        /// ◁　ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton9_Click(object sender, System.EventArgs e)
        {
            kifuControl.RewindKifuListIndex();
        }

        /// <summary>
        /// ▷　ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton10_Click(object sender, System.EventArgs e)
        {
            kifuControl.ForwardKifuListIndex();
        }

        /// <summary>
        /// 本譜ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton11_Click(object sender, System.EventArgs e)
        {
            kifuControl.ViewModel.RaisePropertyChanged("MainBranchButtonClicked");
        }

        /// <summary>
        /// ◀ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton12_Click(object sender, System.EventArgs e)
        {
            kifuControl.ViewModel.KifuListSelectedIndex = 0;
        }

        /// <summary>
        /// ▶ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton13_Click(object sender, System.EventArgs e)
        {
            //kifuControl.ViewModel.KifuListSelectedIndex = int.MaxValue /* clipされて末尾に移動するはず */;
            // →　これ末尾の局面にいても無駄にイベント生起するのでやめとく。

            kifuControl.ViewModel.KifuListSelectedIndex = kifuControl.ViewModel.KifuListCount - 1;
        }

        /// <summary>
        /// Drag & Dropのためのハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_DragEnter(object sender, DragEventArgs e)
        {
            // 対局中は受け付けない。
            if (gameScreenControl1.gameServer.GameMode != GameModeEnum.ConsiderationWithoutEngine)
                return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // ドラッグ中のファイルやディレクトリの取得
                var drags = (string[])e.Data.GetData(DataFormats.FileDrop);

                // ファイル以外であればイベント・ハンドラを抜ける
                foreach (string d in drags)
                    if (!System.IO.File.Exists(d))
                        return;

                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// Drag & Dropによる棋譜ファイルの貼り付け
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_DragDrop(object sender, DragEventArgs e)
        {
            if (gameScreenControl1.gameServer.GameMode != GameModeEnum.ConsiderationWithoutEngine)
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0)
                return;
            var file = files[0];

            // このファイルを読み込みたいということはわかった。

            if (gameScreenControl1.gameServer.KifuDirty)
            {
                if (TheApp.app.MessageShow("未保存の棋譜が残っていますが、本当に棋譜を貼り付けますか？", MessageShowType.WarningOkCancel)
                    != DialogResult.OK)
                    return;
            }

            ReadKifuFile(file);
        }

        /// <summary>
        /// 閉じるときに本当に終了しますかの確認を出す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 例外か何かのために終了しているなら、もうどうしようもない。
            if (TheApp.app.Exiting)
                return;

            e.Cancel = false; // DockWindow側でtrueにしていたはず。
            if (gameScreenControl1.gameServer.InTheGame)
            {
                if (TheApp.app.MessageShow("対局中ですが本当に終了しますか？", MessageShowType.WarningOkCancel)
                    != DialogResult.OK)
                    e.Cancel = true;
            }
            else if (gameScreenControl1.gameServer.KifuDirty)
            {
                if (TheApp.app.MessageShow("未保存の棋譜が残っていますが、本当に終了しますか？", MessageShowType.ConfirmationOkCancel)
                    != DialogResult.OK)
                    e.Cancel = true;
            }

            // cancelが確定したら、ここでリターン
            if (e.Cancel)
                return;

            // 閉じるのをcancelしないことが確定したので、これにて終了する。
            TheApp.app.ApplicationExit();

        }

        /// <summary>
        /// メインウインドウ、検討ウィンドウが移動、リサイズしたときに呼び出されるべきハンドラ。
        /// それらの位置・サイズを保存する。
        /// </summary>
        private void SaveWindowSizeAndPosition()
        {
            // Windowが最小化されているときのことを考慮して、DesktopLocationではなくRestoreBoundsを用いるようにしようと思ったが、
            // 最小化されていると2つ目のFormのRestoreBound、嘘の値になったままだ…。この時は保存しないでおこう。
            // この理由から、終了時にだけ保存するのはNG。ResizeとMoveイベントに応じてこのメソッドを呼び出す必要がある。

            var minimized = WindowState != FormWindowState.Normal; // 最小化、最大化時
            if (minimized)
                return;

            if (first_tick) // Form生成前やろ。
                return;

            var config = TheApp.app.Config;

            // メインウインドウの位置と大きさの保存
            var location = /*minimized ? RestoreBounds.Location :*/ DesktopLocation;
            var size = /*minimized ? RestoreBounds.Size :*/ ClientSize;

            config.DesktopLocation = location;

            if (size.Width >= 100 && size.Height >= 100)
                config.MainDialogClientSize = size;

        }

    }
}
