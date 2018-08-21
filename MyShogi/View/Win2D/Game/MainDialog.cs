using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.Model.Shogi.Usi;
using MyShogi.View.Win2D.Common;
using MyShogi.View.Win2D.Setting;
using ObjectModel = MyShogi.Model.Common.ObjectModel;
using SCore = MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局盤面などがあるメインウィンドゥ
    /// </summary>
    public partial class MainDialog : Form
    {
        public MainDialog()
        {
            InitializeComponent();
        }

        #region ViewModel

        public class MainDialogViewModel : NotifyObject
        {
            /// <summary>
            /// 棋譜の上書き保存のために、前回保存したときの名前を保持しておく。
            /// この値がnullになると、ファイルの上書き保存ができなくなるので
            /// この値の変更イベントをハンドルしてメニューの更新を行う。
            /// </summary>
            public string LastFileName
            {
                get { return GetValue<string>("LastFileName"); }
                set { SetValue<string>("LastFileName", value); }
            }

        }
        public MainDialogViewModel ViewModel = new MainDialogViewModel();

        #endregion

        #region public methods
        /// <summary>
        /// LocalGameServerを渡して、このウィンドウに貼り付けているGameScreenControlを初期化してやる。
        /// </summary>
        /// <param name="gameServer"></param>
        public void Init(LocalGameServer gameServer_)
        {
            // GameScreenControlの初期化
            var setting = new GameScreenControlSetting()
            {
                SetButton = SetButton,
                gameServer = gameServer_,
                UpdateMenuItems = UpdateMenuItems,
            };
            gameScreenControl1.Setting = setting;
            gameScreenControl1.Init();

            // エンジンの読み筋などを、検討ダイアログにリダイレクトする。
            gameScreenControl1.ThinkReportChanged = ThinkReportChanged;

            // -- ViewModelのハンドラの設定

            ViewModel.AddPropertyChangedHandler("LastFileName", (_) => UpdateMenuItems() );
        }

        #endregion

        #region properties
        /// <summary>
        /// activeなGameScreenControlに関連付けられてるLocalGameServerのインスタンスを返す。
        /// 現状、GameScreenControlは一つしかインスタンスを生成していないので、それがactiveである。
        /// </summary>
        public LocalGameServer gameServer { get { return gameScreenControl1.gameServer; } }


        /// <summary>
        /// activeなGameScreenControlに関連付けられているKifuControlのインスタンスを返す。
        /// 現状、GameScreenControlは一つしかインスタンスを生成していないので、それがactiveである。
        /// </summary>
        public KifuControl kifuControl { get { return gameScreenControl1.kifuControl; } }

        // -- メニューが生成しうるダイアログ

        /// modal dialogとして表示するするものはコメントアウトした。

        /// <summary>
        /// 「やねうら王について」のダイアログ
        /// </summary>
        //public Form aboutDialog;

        /// <summary>
        /// 「通常対局」の設定ダイアログ
        /// </summary>
        //public Form gameSettingDialog;

        /// <summary>
        /// CPU infoを表示するダイアログ
        /// </summary>
        //public Form cpuInfoDialog;

        /// <summary>
        /// デバッグウィンドウ
        /// </summary>
        public Form debugDialog;

        /// <summary>
        /// ・検討エンジン設定ダイアログ
        /// ・詰将棋エンジン設定ダイアログ
        /// 共通。
        /// </summary>
        //public Form ConsiderationEngineSettingDialog;

        /// <summary>
        /// エンジンの思考出力用
        /// </summary>
        public EngineConsiderationDialog engineConsiderationDialog;

#if false
        /// <summary>
        /// 評価値グラフの出力用
        /// </summary>
        public Info.EvalGraphDialog evalGraphDialog;
#endif

        #endregion

        #region dialog

        /// <summary>
        /// エンジンによる検討を開始する。
        /// </summary>
        private void ToggleConsideration()
        {
            var consideration = gameServer.GameMode == GameModeEnum.ConsiderationWithEngine;

            if (!consideration && TheApp.app.Config.ConsiderationEngineSetting.EngineDefineFolderPath == null)
            {
                // 検討エンジン設定されてないじゃん…。
                ShowConsiderationEngineSettingDialog();

                // ↑のメソッド内であとは勝手にやってくれるじゃろ…。
                return;
            }

            gameServer.ChangeGameModeCommand(
                consideration ?
                GameModeEnum.ConsiderationWithoutEngine :
                GameModeEnum.ConsiderationWithEngine
            );
        }

        /// <summary>
        /// エンジンによる詰検討を開始する。
        /// </summary>
        private void ToggleMateConsideration()
        {
            var mate_consideration = gameServer.GameMode == GameModeEnum.ConsiderationWithMateEngine;

            if (!mate_consideration && TheApp.app.Config.MateEngineSetting.EngineDefineFolderPath == null)
            {
                // 検討エンジン設定されてないじゃん…。
                ShowMateEngineSettingDialog();

                // ↑のメソッド内であとは勝手にやってくれるじゃろ…。
                return;
            }

            gameServer.ChangeGameModeCommand(
                mate_consideration ?
                GameModeEnum.ConsiderationWithoutEngine :
                GameModeEnum.ConsiderationWithMateEngine
            );
        }

        /// <summary>
        /// 検討エンジンの設定ダイアログを表示する。
        /// (イベントハンドラを適切に設定した上で)
        /// </summary>
        private void ShowConsiderationEngineSettingDialog()
        {
            using (var dialog = new ConsiderationEngineSettingDialog())
            {
                FormLocationUtility.CenteringToThisForm(dialog, this);
                var setting = TheApp.app.Config.ConsiderationEngineSetting;
                dialog.ViewModel.DialogType = ConsiderationEngineSettingDialogType.ConsiderationSetting;
                dialog.ViewModel.AddPropertyChangedHandler("StartButtonClicked", _ => ToggleConsideration());
                dialog.Bind(setting);
                dialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// 詰検討エンジンの設定ダイアログを表示する。
        /// (イベントハンドラを適切に設定した上で)
        /// </summary>
        private void ShowMateEngineSettingDialog()
        {
            using (var dialog = new ConsiderationEngineSettingDialog())
            {
                FormLocationUtility.CenteringToThisForm(dialog, this);
                var setting = TheApp.app.Config.MateEngineSetting;
                dialog.ViewModel.DialogType = ConsiderationEngineSettingDialogType.MateSetting;
                dialog.ViewModel.AddPropertyChangedHandler("StartButtonClicked", _ => ToggleMateConsideration());
                dialog.Bind(setting);
                dialog.ShowDialog(this);
            }
        }

        #endregion

        #region event handlers

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

                // 棋譜ウィンドウの更新通知のタイミングがなかったのでupdate
                gameServer.RaisePropertyChanged("KifuList", gameServer.KifuList);

                // メニューもGameServerが初期化されているタイミングで更新できていなかったのでupdate
                UpdateMenuItems();
            }

            // 自分が保有しているScreenがdirtyになっていることを検知したら、Invalidateを呼び出す。
            if (gameScreenControl1.Dirty)
                gameScreenControl1.Invalidate();

            // 持ち時間描画だけの部分更新
            // あとでちゃんと書き直す
            //if (gameScreen.DirtyRestTime)
            //    Invalidate(new Rectangle(100, 100, 1, 1));

            // TODO : マルチスクリーン対応のときにちゃんと書く
            // GameScreenControlをきちんとコンポーネント化したので、書きやすいはず…。
        }

        private bool first_tick = true;

        // --

        public void MainDialog_Move(object sender, System.EventArgs e)
        {
            UpdateEngineConsiderationDialogLocation();
            SaveWindowSizeAndPosition();
        }

        private void MainDialog_Resize(object sender, System.EventArgs e)
        {
            UpdateEngineConsiderationDialogLocation();
            SaveWindowSizeAndPosition();
        }

        /// <summary>
        /// ウィンドウを移動させたときなどに、そこの左下に検討ウィンドウを追随させる。
        /// </summary>
        private void UpdateEngineConsiderationDialogLocation()
        {
            if (TheApp.app.Config.ConsiderationWindowFollowMainWindow)
            {
                if (engineConsiderationDialog != null)
                {
                    var loc = Location;
                    engineConsiderationDialog.Location =
                        new Point(loc.X, loc.Y + Height);
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
                // プライマリスクリーンを基準にして良いのかどうかはわからん…。
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
            } else
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

            } else {

                // これが現在のいずれかの画面上であることを保証しなくてはならない。
                foreach (var s in Screen.AllScreens)
                    if (s.Bounds.Left <= desktopLocation.Value.X && desktopLocation.Value.X < s.Bounds.Right &&
                        s.Bounds.Top <= desktopLocation.Value.Y && desktopLocation.Value.Y < s.Bounds.Bottom)
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
        /// [UI Thread] : LocalGameServerから送られてくるエンジンの読み筋などのハンドラ。
        /// </summary>
        private void ThinkReportChanged(PropertyChangedEventArgs args)
        {
            var message = args.value as UsiThinkReportMessage;

            // 評価値グラフの更新など
            gameServer.ThinkReportChangedCommand(message);

#if false // このデバッグをしているとマスターアップに間に合わなさそう。後回し。
            if (evalGraphDialog == null)
            {
                evalGraphDialog = new Info.EvalGraphDialog();
                // ToDo: 要らない時は形勢グラフウィンドウを開かないようにするべき？
                evalGraphDialog.Visible = true;
            }
            else if (evalGraphDialog.IsDisposed || !evalGraphDialog.Visible)
            {
                goto cancelEvalGraph;
            }
            evalGraphDialog.DispatchEvalGraphUpdate(gameServer);
            cancelEvalGraph:;
#endif

            if (engineConsiderationDialog == null)
            {
                var dialog = new EngineConsiderationDialog();
                dialog.Init(gameServer.BoardReverse /* これ引き継ぐ。以降は知らん。*/);
                // ウィンドウ幅を合わせておく。

                // 前回起動時のサイズが記録されているならそれを復元してやる。
                var size = TheApp.app.Config.ConsiderationDialogClientSize;
                if (size.Width < 192 || size.Height < 108)
                    size = Size.Empty;
                if (size.IsEmpty)
                    size = new Size(Width, (int)(Width * 0.2)); /* メインウィンドウの20%ぐらいの高さ */
                dialog.Size = size;
                dialog.Show(/*this*/);
                // 検討ウィンドウはClosingイベントをキャンセルして非表示にしているのでメインウインドウにぶら下げると
                // アプリを終了できなくなってしまう。また、メインウインドウを動かした時に検討ウィンドウは自動追随するので
                // 現状、普通に使用していてメインウインドウで検討ウィンドウが隠れることはないため、これで良しとする。

                var offset = TheApp.app.Config.ConsiderationDialogClientLocation;
                if (offset.IsEmpty)
                    dialog.Location = new Point(Location.X, Location.Y + Height);
                else
                    dialog.Location = new Point(Location.X + offset.X, Location.Y + offset.Y);

                dialog.Visible = false;

                dialog.ConsiderationInstance(0).Notify.AddPropertyChangedHandler("MultiPV", (h) =>
                 { gameServer.ChangeMultiPvCommand((int)h.value); });

                // 検討ウィンドウを×ボタンで非表示にした時にメニューの検討ウィンドウのところが更新になるのでメニューのrefreshが必要。
                dialog.ViewModel.AddPropertyChangedHandler("CloseButtonClicked", (_) =>
                {
                    // ConsiderationModeなら、解除しておく。
                    if (gameServer.GameMode.IsConsideration())
                       gameServer.ChangeGameModeCommand(GameModeEnum.ConsiderationWithoutEngine);

                    UpdateMenuItems();
                });

                // MoveとResizeに応じて、それを記録しなければならない。
                dialog.Move += (sender,args_) => SaveWindowSizeAndPosition();
                dialog.Resize += (sender, args_) => SaveWindowSizeAndPosition();

                engineConsiderationDialog = dialog;
                // 何にせよ、インスタンスがなくては話にならないので生成だけしておく。

            } else
            {
                // 検討ウィンドウが非表示になっていたら、PVのメッセージ無視していいや…。
                // (処理に時間かかるし…)
                if (!engineConsiderationDialog.Visible && message.type == UsiEngineReportMessageType.UsiThinkReport)
                        return;
            }

            var visible_old = engineConsiderationDialog.Visible;
            engineConsiderationDialog.DispatchThinkReportMessage(message);
            // Dispatchした結果、Visible状態が変化したならメニューを更新してやる。
            if (visible_old != engineConsiderationDialog.Visible)
                UpdateMenuItems();
        }

        /// <summary>
        /// 棋譜ウィンドウの横幅が設定で変更になった時に棋譜ウィンドウを実際にリサイズする。
        /// </summary>
        public void ResizeKifuControl(PropertyChangedEventArgs args)
        {
            gameScreenControl1.ResizeKifuControl();
        }

        /// <summary>
        /// Ctrl+V による棋譜の貼り付け
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

                //クリップボードからテキスト取得
                var text = Clipboard.GetText();
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
        private void SetButton(ToolStripButtonEnum name, bool enable)
        {
            ToolStripButton btn;
            switch (name)
            {
                case ToolStripButtonEnum.RESIGN: btn = this.toolStripButton1; break;
                case ToolStripButtonEnum.UNDO_MOVE: btn = this.toolStripButton2; break;
                case ToolStripButtonEnum.MOVE_NOW: btn = this.toolStripButton3; break;
                case ToolStripButtonEnum.INTERRUPT: btn = this.toolStripButton4; break;
                case ToolStripButtonEnum.REWIND: btn = this.toolStripButton9; break;
                case ToolStripButtonEnum.FORWARD: btn = this.toolStripButton10; break;
                case ToolStripButtonEnum.MAIN_BRANCH: btn = this.toolStripButton11; break;
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
            gameServer.DoMoveCommand(SCore.Move.RESIGN);
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
            kifuControl.ViewModel.KifuListSelectedIndex = int.MaxValue /* clipされて末尾に移動するはず */;
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

            ReadKifFile(file);
        }

        /// <summary>
        /// 閉じるときに本当に終了しますかの確認を出す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
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

            // 検討ウィンドウの位置と大きさの保存
            if (engineConsiderationDialog != null && engineConsiderationDialog.Width >= 100 && engineConsiderationDialog.Height >= 100)
            {
                var c_location = /* minimized ? engineConsiderationDialog.RestoreBounds.Location : */ engineConsiderationDialog.Location;
                var c_size = /* minimized ? engineConsiderationDialog.RestoreBounds.Size : */ engineConsiderationDialog.ClientSize;

                config.ConsiderationDialogClientSize = c_size;
                // 検討ウィンドウの位置はメインウィンドウ相対で記録
                config.ConsiderationDialogClientLocation =
                    new Point(
                        c_location.X - location.X,
                        c_location.Y - location.Y
                    );
            }
        }

        #endregion

        #region update menu

        private void ReadKifFile(string filename)
        {
            try
            {
                var kifu_text = FileIO.ReadText(filename);
                gameServer.KifuReadCommand(kifu_text);
                ViewModel.LastFileName = filename; // 最後に開いたファイルを記録しておく。
            }
            catch
            {
                TheApp.app.MessageShow("ファイル読み込みエラー", MessageShowType.Error);
            }
        }

        /// <summary>
        /// 前回にUpdateMenuItems()が呼び出された時のGameMode。
        /// </summary>
        private GameModeEnum lastGameMode = GameModeEnum.ConsiderationWithoutEngine;

        /// <summary>
        /// [UI thread] : メニューのitemを動的に追加する。
        /// 商用版とフリーウェア版とでメニューが異なるのでここで動的に追加する必要がある。
        /// </summary>
        public void UpdateMenuItems(ObjectModel.PropertyChangedEventArgs args = null)
        {
            // 頑張れば高速化出来るが、対局中はこのメソッド呼び出されていないし、
            // ToolStripも、CPU×CPUの対局中は更新は発生していないし、
            // CPU×人間のときは多少遅くても誤差だし、まあいいか…。

            var config = TheApp.app.Config;

            // Commercial Version GUI
            bool CV_GUI = config.CommercialVersion != 0;
            if (CV_GUI)
                Text = "将棋神やねうら王";
            // 商用版とどこで差別化するのか考え中

            // -- メニューの追加。
            {

                var menu = new MenuStrip();

                //レイアウトロジックを停止する
                SuspendLayout();
                menu.SuspendLayout();

                // 前回設定されたメニューを除去する
                if (old_menu != null)
                    Controls.Remove(old_menu);

                // -- LocalGameServerの各フラグ。
                // ただし、初期化時にgameServer == nullで呼び出されることがあるのでnull checkが必要。

                // LocalGameServer.GameModeは値がいま書き換わっている可能性があるので、イベントを除かしてしまう可能性がある。
                // ゆえに、引数で渡ってきたargs.value (GameModeEnum)を用いる必要があるが、しかし、args.valueが他の型である可能性もある。(BoardReverseなどを渡すとき)
                // このため、args.valueがGameModeEnumなら、これを用いて、さもなくば仕方ないので前回渡されたものをそのまま用いる。
                // (LocalGameServerの値はメニューには直接使わない)
                var gameMode =
                    (args != null && args.value != null && args.value is GameModeEnum) ? (GameModeEnum)args.value :
                    gameServer == null           ?  GameModeEnum.NotInit    :
                    lastGameMode;
                lastGameMode = gameMode;

                // 検討モード(通常エンジン)
                var consideration = gameMode == GameModeEnum.ConsiderationWithEngine;
                // 検討モード(詰将棋用)
                var mate_consideration = gameMode == GameModeEnum.ConsiderationWithMateEngine;
                // 対局中
                var inTheGame = gameMode == GameModeEnum.InTheGame;
                // 盤面編集中
                var inTheBoardEdit = gameMode == GameModeEnum.InTheBoardEdit;
                // 盤面反転
                var boardReverse = gameServer == null ? false : gameServer.BoardReverse;

                // 再起動するように警告表示
                void WarningRestart() { TheApp.app.MessageShow("この変更が反映するのは次回起動時です。",MessageShowType.Confirmation ); };

                var item_file = new ToolStripMenuItem();
                item_file.Text = "ファイル(&F)";
                menu.Items.Add(item_file);

                // 対局中などは、ファイルメニュー項目は丸ごと無効化
                item_file.Enabled = gameMode == GameModeEnum.ConsiderationWithoutEngine;

                // -- 「ファイル」配下のメニュー
                {
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜を開く(&O)";
                        item.ShortcutKeys = Keys.Control | Keys.O;
                        item.ShowShortcutKeys = true;
                        item.Click += (sender, e) =>
                        {
                            using (var fd = new OpenFileDialog())
                            {
                                // [ファイルの種類]に表示される選択肢を指定する
                                // 指定しないとすべてのファイルが表示される
                                fd.Filter = string.Join("|", new string[]
                                {
                                "棋譜ファイル|*.kif;*.kifu;*.ki2;*.kif2;*.ki2u;*.kif2u;*.csa;*.psn;*.psn2;*.sfen;*.json;*.jkf;*.txt",
                                "KIF形式|*.kif;*.kifu",
                                "KIF2形式|*.ki2;*.kif2;*.ki2u;*.kif2u",
                                "CSA形式|*.csa",
                                "PSN形式|*.psn",
                                "PSN2形式|*.psn2",
                                "SFEN形式|*.sfen",
                                "すべてのファイル|*.*",
                                });
                                fd.FilterIndex = 1;
                                fd.Title = "開く棋譜ファイルを選択してください";

                                // ダイアログを表示する
                                if (fd.ShowDialog() == DialogResult.OK)
                                    ReadKifFile(fd.FileName);
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜の上書き保存(&S)";
                        item.ShortcutKeys = Keys.Control | Keys.S;
                        item.ShowShortcutKeys = true;
                        item.Enabled = ViewModel.LastFileName != null; // 棋譜を読み込んだ時などにしか有効ではない。
                        item.Click += (sender, e) =>
                        {
                            try
                            {
                                // 「開く」もしくは「名前をつけて保存無したファイルに上書きする。
                                // 「局面の保存」は棋譜ではないのでこれは無視する。
                                // ファイル形式は、拡張子から自動判別する。
                                gameServer.KifuWriteCommand(ViewModel.LastFileName,
                                    KifuFileTypeExtensions.StringToKifuFileType(ViewModel.LastFileName));
                            }
                            catch
                            {
                                TheApp.app.MessageShow("ファイル書き出しエラー" , MessageShowType.Error);
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜に名前をつけて保存(&N)";
                        item.ShortcutKeys = Keys.Control | Keys.S | Keys.Shift;
                        item.ShowShortcutKeys = true;
                        item.Click += (sender, e) =>
                        {
                            using (var fd = new SaveFileDialog())
                            {

                                // [ファイルの種類]に表示される選択肢を指定する
                                // 指定しないとすべてのファイルが表示される
                                fd.Filter = "KIF形式(*.KIF)|*.KIF|KIF2形式(*.KI2)|*.KI2|CSA形式(*.CSA)|*.CSA"
                                    + "|PSN形式(*.PSN)|*.PSN|PSN2形式(*.PSN2)|*.PSN2"
                                    + "|SFEN形式(*.SFEN)|*.SFEN|すべてのファイル(*.*)|*.*";
                                fd.FilterIndex = 1;
                                fd.Title = "棋譜を保存するファイル形式を選択してください";
                                // デフォルトでは、先手名 + 後手名 + YYYYMMDDhhmmss.kif
                                // 柿木やkifu for Windowsがこの形式らしい。
                                var default_filename = $"{gameServer.DisplayName(SCore.Color.BLACK)}_{gameServer.DisplayName(SCore.Color.WHITE)}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.KIF";
                                fd.FileName = Utility.EscapeFileName(default_filename);

                                // ダイアログを表示する
                                if (fd.ShowDialog() == DialogResult.OK)
                                {
                                    var filename = fd.FileName;
                                    try
                                    {
                                        KifuFileType kifuType;
                                        switch (fd.FilterIndex)
                                        {
                                            case 1: kifuType = KifuFileType.KIF; break;
                                            case 2: kifuType = KifuFileType.KI2; break;
                                            case 3: kifuType = KifuFileType.CSA; break;
                                            case 4: kifuType = KifuFileType.PSN; break;
                                            case 5: kifuType = KifuFileType.PSN2; break;
                                            case 6: kifuType = KifuFileType.SFEN; break;

                                            // ファイル名から自動判別すべき
                                            default:
                                                kifuType = KifuFileTypeExtensions.StringToKifuFileType(filename);
                                                if (kifuType == KifuFileType.UNKNOWN)
                                                    kifuType = KifuFileType.KIF; // わからんからKIF形式でいいや。
                                                break;
                                        }

                                        gameServer.KifuWriteCommand(filename, kifuType);
                                        ViewModel.LastFileName = filename; // 最後に保存したファイルを記録しておく。
                                        gameServer.KifuDirty = false; // 棋譜綺麗になった。
                                    }
                                    catch
                                    {
                                        TheApp.app.MessageShow("ファイル書き出しエラー", MessageShowType.Error);
                                    }
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "局面の保存(&I)"; // Pは印刷(Print)で使いたいため、positionの"I"をショートカットキーにする。
                        item.Click += (sender, e) =>
                        {
                            using (var fd = new SaveFileDialog())
                            {

                                // [ファイルの種類]に表示される選択肢を指定する
                                // 指定しないとすべてのファイルが表示される
                                fd.Filter = "KIF形式(*.KIF)|*.KIF|KIF2形式(*.KI2)|*.KI2|CSA形式(*.CSA)|*.CSA"
                                    + "|PSN形式(*.PSN)|*.PSN|PSN2形式(*.PSN2)|*.PSN2"
                                    + "|SFEN形式(*.SFEN)|*.SFEN|SVG形式(*.SVG)|*.SVG|すべてのファイル(*.*)|*.*";
                                fd.FilterIndex = 1;
                                fd.Title = "局面を保存するファイル形式を選択してください";

                                // ダイアログを表示する
                                if (fd.ShowDialog() == DialogResult.OK)
                                {
                                    var filename = fd.FileName;
                                    try
                                    {
                                        KifuFileType kifuType;
                                        switch (fd.FilterIndex)
                                        {
                                            case 1: kifuType = KifuFileType.KIF; break;
                                            case 2: kifuType = KifuFileType.KI2; break;
                                            case 3: kifuType = KifuFileType.CSA; break;
                                            case 4: kifuType = KifuFileType.PSN; break;
                                            case 5: kifuType = KifuFileType.PSN2; break;
                                            case 6: kifuType = KifuFileType.SFEN; break;
                                            case 7: kifuType = KifuFileType.SVG; break;

                                            // ファイル名から自動判別すべき
                                            default:
                                                kifuType = KifuFileTypeExtensions.StringToKifuFileType(filename);
                                                if (kifuType == KifuFileType.UNKNOWN)
                                                    kifuType = KifuFileType.KIF; // わからんからKIF形式でいいや。
                                                break;
                                        }

                                        gameServer.PositionWriteCommand(filename, kifuType);
                                    }
                                    catch
                                    {
                                        TheApp.app.MessageShow("ファイル書き出しエラー", MessageShowType.Error);
                                    }
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    item_file.DropDownItems.Add(new ToolStripSeparator());

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "クリップボードに棋譜/局面をコピー(&C)";

                        var itemk1 = new ToolStripMenuItem();
                        itemk1.Text = "棋譜KIF形式(&1)";
                        itemk1.ShortcutKeys = Keys.Control | Keys.C;
                        itemk1.ShowShortcutKeys = true;
                        // このショートカットキーを設定すると対局中などにも書き出せてしまうが、書き出しはまあ問題ない。
                        itemk1.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.KIF); };
                        item.DropDownItems.Add(itemk1);

                        var itemk2 = new ToolStripMenuItem();
                        itemk2.Text = "棋譜KI2形式(&2)";
                        itemk2.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.KI2); };
                        item.DropDownItems.Add(itemk2);

                        var itemk3 = new ToolStripMenuItem();
                        itemk3.Text = "棋譜CSA形式(&3)";
                        itemk3.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.CSA); };
                        item.DropDownItems.Add(itemk3);

                        var itemk4 = new ToolStripMenuItem();
                        itemk4.Text = "棋譜SFEN形式(&4)";
                        itemk4.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.SFEN); };
                        item.DropDownItems.Add(itemk4);

                        var itemk5 = new ToolStripMenuItem();
                        itemk5.Text = "棋譜PSN形式(&5)";
                        itemk5.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.PSN); };
                        item.DropDownItems.Add(itemk5);

                        var itemk6 = new ToolStripMenuItem();
                        itemk6.Text = "棋譜PSN2形式(&6)";
                        itemk6.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.PSN2); };
                        item.DropDownItems.Add(itemk6);

                        item.DropDownItems.Add(new ToolStripSeparator());

                        var itemp1 = new ToolStripMenuItem();
                        itemp1.Text = "局面KIF(BOD)形式(&A)";
                        itemp1.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.KI2); };
                        item.DropDownItems.Add(itemp1);

                        var itemp2 = new ToolStripMenuItem();
                        itemp2.Text = "局面CSA形式(&B)";
                        itemp2.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.CSA); };
                        item.DropDownItems.Add(itemp2);

                        var itemp3 = new ToolStripMenuItem();
                        itemp3.Text = "局面SFEN形式(&C)";
                        itemp3.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.SFEN); };
                        item.DropDownItems.Add(itemp3);

                        var itemp4 = new ToolStripMenuItem();
                        itemp4.Text = "局面PSN形式(&D)";
                        itemp4.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.PSN); };
                        item.DropDownItems.Add(itemp4);

                        var itemp5 = new ToolStripMenuItem();
                        itemp5.Text = "局面PSN2形式(&E)";
                        itemp5.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.PSN2); };
                        item.DropDownItems.Add(itemp5);

                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "クリップボードから棋譜/局面を貼り付け(&P)";
                        // このショートカットキーを設定すると対局中などにも貼り付けが出来てしまうが、
                        // GameModeを見て、対局中などには処理しないようにしてある。
                        item.ShortcutKeys = Keys.Control | Keys.V;
                        item.ShowShortcutKeys = true;
                        item.Click += (sender, e) => { CopyFromClipboard(); };
                        item_file.DropDownItems.Add(item);
                    }

                    item_file.DropDownItems.Add(new ToolStripSeparator());

                    // -- 設定の初期化
                    {
                        var item_init = new ToolStripMenuItem();
                        item_init.Text = "設定の初期化(&I)";
                        item_file.DropDownItems.Add(item_init);

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "各エンジン設定の初期化";
                            item.Click += (sender, e) => {
                                if (TheApp.app.MessageShow("すべてのエンジン設定を初期化しますか？「OK」を押すと初期化され、次回起動時に反映されます。",MessageShowType.ConfirmationOkCancel) == DialogResult.OK)
                                {
                                    TheApp.app.DeleteEngineOption = true;
                                }
                            };
                            item_init.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "各表示設定などの初期化";
                            item.Click += (sender, e) => {
                                if (TheApp.app.MessageShow("すべての表示設定・音声設定を初期化しますか？「OK」を押すと初期化され、次回起動時に反映されます。", MessageShowType.ConfirmationOkCancel) == DialogResult.OK)
                                {
                                    TheApp.app.DeleteGlobalOption = true;
                                }
                            };
                            item_init.DropDownItems.Add(item);
                        }
                    }

                    item_file.DropDownItems.Add(new ToolStripSeparator());

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "終了(&X)";
                        item.Click += (sender, e) => { TheApp.app.ApplicationExit(); };
                        item_file.DropDownItems.Add(item);
                    }

                }

                var item_playgame = new ToolStripMenuItem();
                item_playgame.Text = "対局(&P)"; // PlayGame
                item_playgame.Enabled = gameServer != null && !gameServer.InTheGame; // 対局中はこのメニューを無効化
                menu.Items.Add(item_playgame);

                // -- 「対局」配下のメニュー
                {
                    { // -- 通常対局
                        var item = new ToolStripMenuItem();
                        item.Text = "通常対局(&N)"; // NormalGame
                        item.ShortcutKeys = Keys.Control | Keys.N; // NewGameのN
                        item.ShowShortcutKeys = true;
                        item.Click += (sender, e) =>
                        {
                            using (var gameSettingDialog = new GameSettingDialog(this))
                            {
                                FormLocationUtility.CenteringToThisForm(gameSettingDialog, this);
                                gameSettingDialog.ShowDialog(this); // Modal Dialogにしておく。
                            }
                        };

                        item_playgame.DropDownItems.Add(item);
                    }

                    item_playgame.DropDownItems.Add(new ToolStripSeparator());

                    { // -- 検討モード

                        var item = new ToolStripMenuItem();
                        item.Text = consideration ? "検討モードを終了する(&C)" : "検討エンジン設定(&C)"; // ConsiderationMode

                        // toolStripのボタンのテキストを検討モードであるかどうかにより変更する。
                        toolStripButton5.Text = consideration ? "終" : "検";
                        toolStripButton5.ToolTipText = consideration ? "検討モードを終了します。" : "検討モードに入ります。";
                        toolStripButton5.Enabled = !inTheGame;
                        item.Click += (sender, e) => {
                            if (consideration)
                                ToggleConsideration(); // 検討モードを終了させる
                            else
                                ShowConsiderationEngineSettingDialog(); // 検討エンジンの選択画面に
                        };

                        item_playgame.DropDownItems.Add(item);
                    }


                    // 「解」ボタン : 棋譜解析
                    //toolStripButton6.Enabled = !inTheGame;

                    { // -- 検討モード

                        var item = new ToolStripMenuItem();
                        item.Text = mate_consideration ? "詰検討モードを終了する(&M)" : "詰検討エンジン設定(&M)"; // MateMode

                        // toolStripのボタンのテキストを検討モードであるかどうかにより変更する。
                        toolStripButton7.Text = mate_consideration ? "終" : "詰";
                        toolStripButton7.ToolTipText = mate_consideration ? "詰検討モードを終了します。" : "詰検討モードに入ります。";
                        // 「詰」ボタン : 詰将棋ボタン
                        toolStripButton7.Enabled = !inTheGame;
                        item.Click += (sender, e) => {
                            if (mate_consideration)
                                ToggleMateConsideration();
                            else
                                ShowMateEngineSettingDialog(); // 詰検討エンジンの選択画面に

                        };

                        item_playgame.DropDownItems.Add(item);
                    }
                }


                var item_display = new ToolStripMenuItem();
                item_display.Text = "表示(&D)"; // Display
                menu.Items.Add(item_display);

                // -- 「表示」配下のメニュー
                {
                    { // -- 盤面反転
                        var item = new ToolStripMenuItem();
                        item.Text = "盤面反転(&R)"; // Reverse
                        item.Checked = boardReverse;
                        item.Click += (sender, e) => { gameServer.BoardReverse ^= true; };

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 段・筋の画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "筋・段の表示(&F)"; // FileRank

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "非表示(&I)"; // Invisible
                        item1.Checked = config.BoardNumberImageVersion == 0;
                        item1.Click += (sender, e) => { config.BoardNumberImageVersion = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "標準(&N)"; // Normal
                        item2.Checked = config.BoardNumberImageVersion == 1;
                        item2.Click += (sender, e) => { config.BoardNumberImageVersion = 1; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "Chess式(&C)"; // Chess
                        item3.Checked = config.BoardNumberImageVersion == 2;
                        item3.Click += (sender, e) => { config.BoardNumberImageVersion = 2; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    {
                        // -- 棋譜ウィンドウの棋譜の形式

                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜ウィンドウの棋譜の表示形式(&K)"; // Kifu

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "標準(KIF2形式)「６七金右」(&K)"; // Kif
                        item1.Checked = config.KifuWindowKifuVersion == 0;
                        item1.Click += (sender, e) => { if (config.KifuWindowKifuVersion != 0) { WarningRestart(); config.KifuWindowKifuVersion = 0; } };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "簡易(KIF形式)「６七金(58)」(&I)"; // kif形式
                        item2.Checked = config.KifuWindowKifuVersion == 1;
                        item2.Click += (sender, e) => { if (config.KifuWindowKifuVersion != 1) { WarningRestart(); config.KifuWindowKifuVersion = 1; } };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "Chess式(SFEN形式)「5h6g」(&S)";    // Sfen形式
                        item3.Checked = config.KifuWindowKifuVersion == 2;
                        item3.Click += (sender, e) => { if (config.KifuWindowKifuVersion != 2) { WarningRestart(); config.KifuWindowKifuVersion = 2; } };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    {
                        // -- 棋譜ウィンドウの棋譜の形式

                        var item = new ToolStripMenuItem();
                        item.Text = "検討ウィンドウの棋譜の表示形式(&K)"; // Kifu

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "標準(KIF2形式)「６七金右」(&K)"; // Kif
                        item1.Checked = config.ConsiderationWindowKifuVersion == 0;
                        item1.Click += (sender, e) => { if (config.ConsiderationWindowKifuVersion != 0) { WarningRestart(); config.ConsiderationWindowKifuVersion = 0; } };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "簡易(KIF形式)「６七金(58)」(&I)"; // kif形式 
                        item2.Checked = config.ConsiderationWindowKifuVersion == 1;
                        item2.Click += (sender, e) => { if (config.ConsiderationWindowKifuVersion != 1) { WarningRestart(); config.ConsiderationWindowKifuVersion = 1; } };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "Chess式(SFEN形式)「5h6g」(&S)";    // Sfen形式
                        item3.Checked = config.ConsiderationWindowKifuVersion == 2;
                        item3.Click += (sender, e) => { if (config.ConsiderationWindowKifuVersion != 2) { WarningRestart(); config.ConsiderationWindowKifuVersion = 2; } };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 盤画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "盤画像(&B)"; // BoardImage

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "白色(&W)"; // White
                        item1.Checked = config.BoardImageVersion == 1;
                        item1.Click += (sender, e) => { config.BoardImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "黄色(&Y)"; // Yellow
                        item2.Checked = config.BoardImageVersion == 2;
                        item2.Click += (sender, e) => { config.BoardImageVersion = 2; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 盤画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "畳画像(&I)"; // tatamI

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "薄い(&L)"; // Light Color
                        item1.Checked = config.TatamiImageVersion == 1;
                        item1.Click += (sender, e) => { config.TatamiImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "濃い(&D)"; // Dark Color
                        item2.Checked = config.TatamiImageVersion == 2;
                        item2.Click += (sender, e) => { config.TatamiImageVersion = 2; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 駒画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "駒画像(&P)"; // PieceImage

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "一文字駒(&1)"; // 1文字
                        item1.Checked = config.PieceImageVersion == 2;
                        item1.Click += (sender, e) => { config.PieceImageVersion = 2; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "二文字駒(&2)"; // 2文字
                        item2.Checked = config.PieceImageVersion == 1;
                        item2.Click += (sender, e) => { config.PieceImageVersion = 1; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "英文字駒(&A)"; // Alphabet
                        item3.Checked = config.PieceImageVersion == 3;
                        item3.Click += (sender, e) => { config.PieceImageVersion = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 成駒の画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "成駒の色(&R)"; // pRomote piece

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "黒(&B)"; // Black
                        item1.Checked = config.PromotePieceColorType == 0;
                        item1.Click += (sender, e) => { config.PromotePieceColorType = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "赤(&R)"; // Red
                        item2.Checked = config.PromotePieceColorType == 1;
                        item2.Click += (sender, e) => { config.PromotePieceColorType = 1; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    // -- 最終手のエフェクト
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "最終手の移動元(&F)"; // From

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし(&N)"; // None
                        item0.Checked = config.LastMoveFromColorType == 0;
                        item0.Click += (sender, e) => { config.LastMoveFromColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "朱色(&R)"; // Red
                        item1.Checked = config.LastMoveFromColorType == 1;
                        item1.Click += (sender, e) => { config.LastMoveFromColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色(&B)"; // Blue
                        item2.Checked = config.LastMoveFromColorType == 2;
                        item2.Click += (sender, e) => { config.LastMoveFromColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色(&G)"; // Green
                        item3.Checked = config.LastMoveFromColorType == 3;
                        item3.Click += (sender, e) => { config.LastMoveFromColorType = 3; };
                        item.DropDownItems.Add(item3);

#if false
                        var item4 = new ToolStripMenuItem();
                        item4.Text = "駒の影のみ(&S)"; // Shadow
                        item4.Checked = config.LastMoveFromColorType == 4;
                        item4.Click += (sender, e) => { config.LastMoveFromColorType = 4; };
                        item.DropDownItems.Add(item4);
#endif

                        item_display.DropDownItems.Add(item);
                    }
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "最終手の移動先(&O)"; // tO

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし(&N)"; // None
                        item0.Checked = config.LastMoveToColorType == 0;
                        item0.Click += (sender, e) => { config.LastMoveToColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "朱色(&R)"; // Red
                        item1.Checked = config.LastMoveToColorType == 1;
                        item1.Click += (sender, e) => { config.LastMoveToColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色(&B)"; // Blue
                        item2.Checked = config.LastMoveToColorType == 2;
                        item2.Click += (sender, e) => { config.LastMoveToColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色(&G)"; // Green
                        item3.Checked = config.LastMoveToColorType == 3;
                        item3.Click += (sender, e) => { config.LastMoveToColorType = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "駒を掴んだ時の移動元(&I)"; // pIcked from

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし(&N)"; // None
                        item0.Checked = config.PickedMoveFromColorType == 0;
                        item0.Click += (sender, e) => { config.PickedMoveFromColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "黄色(&Y)"; // Yellow
                        item1.Checked = config.PickedMoveFromColorType == 1;
                        item1.Click += (sender, e) => { config.PickedMoveFromColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色(&B)"; // Blue
                        item2.Checked = config.PickedMoveFromColorType == 2;
                        item2.Click += (sender, e) => { config.PickedMoveFromColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色(&G)"; // Green
                        item3.Checked = config.PickedMoveFromColorType == 3;
                        item3.Click += (sender, e) => { config.PickedMoveFromColorType = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "駒を掴んだ時の移動候補(&C)"; // piCked to

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "エフェクトなし(&N)"; // None
                        item0.Checked = config.PickedMoveToColorType == 0;
                        item0.Click += (sender, e) => { config.PickedMoveToColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "移動できない升を少し暗くする(&1)"; // dark 1
                        item1.Checked = config.PickedMoveToColorType == 1;
                        item1.Click += (sender, e) => { config.PickedMoveToColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "移動できない升を暗くする(&2)"; // dark 2
                        item2.Checked = config.PickedMoveToColorType == 2;
                        item2.Click += (sender, e) => { config.PickedMoveToColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "移動できない升をかなり暗くする(&3)"; // dark 3
                        item3.Checked = config.PickedMoveToColorType == 3;
                        item3.Click += (sender, e) => { config.PickedMoveToColorType = 3; };
                        item.DropDownItems.Add(item3);

                        var item4 = new ToolStripMenuItem();
                        item4.Text = "移動できる升を少し明るくする(&4)"; // dark 4
                        item4.Checked = config.PickedMoveToColorType == 4;
                        item4.Click += (sender, e) => { config.PickedMoveToColorType = 4; };
                        item.DropDownItems.Add(item4);

                        var item5 = new ToolStripMenuItem();
                        item5.Text = "移動できる升を明るくする(&5)"; // dark 5
                        item5.Checked = config.PickedMoveToColorType == 5;
                        item5.Click += (sender, e) => { config.PickedMoveToColorType = 5; };
                        item.DropDownItems.Add(item5);

#if false
                        var item6 = new ToolStripMenuItem();
                        item6.Text = "駒の影のみ(&6)"; // dark 6
                        item6.Checked = config.PickedMoveToColorType == 6;
                        item6.Click += (sender, e) => { config.PickedMoveToColorType = 6; };
                        item.DropDownItems.Add(item6);
#endif

                        item_display.DropDownItems.Add(item);
                    }

                    // 駒の移動方向
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "移動方角マーカー(&M)"; // Marker

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし(&N)"; // None
                        item0.Checked = config.PieceAttackImageVersion == 0;
                        item0.Click += (sender, e) => { config.PieceAttackImageVersion = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "あり(&E)"; // Enable
                        item1.Checked = config.PieceAttackImageVersion == 1;
                        item1.Click += (sender, e) => { config.PieceAttackImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 手番プレートの表示

                        var item = new ToolStripMenuItem();
                        item.Text = "手番表示(&T)"; // Turn

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "なし(&N)"; // None
                        item1.Checked = config.TurnDisplay == 0;
                        item1.Click += (sender, e) => { config.TurnDisplay = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "あり(&E)"; // Visible
                        item2.Checked = config.TurnDisplay == 1;
                        item2.Click += (sender, e) => { config.TurnDisplay = 1; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 検討ウィンドウで思考エンジンが後手番のときに評価値を反転させるか(自分から見た評価値にするか)

                        var item = new ToolStripMenuItem();
                        item.Text = "後手番のCPUの評価値を反転表示させるか(&V)"; // reVerse eval

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "通常(手番側から見た評価値)(&N)"; // None
                        item1.Checked = !config.NegateEvalWhenWhite;
                        item1.Click += (sender, e) => { config.NegateEvalWhenWhite = false; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "反転(先手側から見た評価値)(&R)"; // Reverse
                        item2.Checked = config.NegateEvalWhenWhite;
                        item2.Click += (sender, e) => { config.NegateEvalWhenWhite = true; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                }

                // 「音声」
                {
                    var item_sounds = new ToolStripMenuItem();
                    item_sounds.Text = "音声(&S)"; // Sound
                    menu.Items.Add(item_sounds);

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "対局時の駒音(&P)"; // Piece sound
                        item1.Checked = config.PieceSoundInTheGame == 1;
                        item1.Click += (sender, e) => { config.PieceSoundInTheGame ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

#if false
                        // あまりいい効果音作れなかったのでコメントアウトしとく。
                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "王手などの駒音を衝撃音に(&C)"; // Check sound
                        item1.Checked = config.CrashPieceSoundInTheGame == 1;
                        item1.Click += (sender, e) => { config.CrashPieceSoundInTheGame ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }
#endif

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "検討時の駒音(&Q)"; // Piece soundのPの(アルファベット的に)次の文字。
                        item1.Checked = config.PieceSoundOffTheGame == 1;
                        item1.Click += (sender, e) => { config.PieceSoundOffTheGame ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }


                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "対局時の棋譜読み上げ(&R)"; // Read out
                        item1.Checked = config.KifuReadOut == 1;
                        item1.Enabled = config.CommercialVersion != 0; // 商用版のみ選択可
                        item1.Click += (sender, e) => { config.KifuReadOut ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "「先手」「後手」を毎回読み上げる(&E)"; // Everytime
                        item1.Checked = config.ReadOutSenteGoteEverytime == 1;
                        item1.Enabled = config.CommercialVersion != 0; // 商用版のみ選択可
                        item1.Click += (sender, e) => { config.ReadOutSenteGoteEverytime ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "終局時に以降の音声読み上げをキャンセルする。(&C)"; // Cancel
                        item1.Checked = config.ReadOutCancelWhenGameEnd == 1;
                        item1.Enabled = config.CommercialVersion != 0; // 商用版のみ選択可
                        item1.Click += (sender, e) => { config.ReadOutCancelWhenGameEnd ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

                }

                var item_boardedit = new ToolStripMenuItem();
                item_boardedit.Text = "盤面編集(&E)"; // board Edit
                item_boardedit.Enabled = !inTheGame;
                menu.Items.Add(item_boardedit);

                // 盤面編集の追加
                {
                    {   // -- 盤面編集の開始
                        var item = new ToolStripMenuItem();
                        item.Text = inTheBoardEdit ? "盤面編集の終了(&B)" : "盤面編集の開始(&B)"; // Board edit
                        item.ShortcutKeys = Keys.Control | Keys.E; // boardEdit
                        item.ShowShortcutKeys = true;
                        item.Click += (sender, e) => {
                            gameServer.ChangeGameModeCommand(
                                inTheBoardEdit ?
                                    GameModeEnum.ConsiderationWithoutEngine :
                                    GameModeEnum.InTheBoardEdit
                            );
                        };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 手番の変更
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "手番の変更(&T)"; // Turn change
                        item.Click += (sender, e) =>
                        {
                            var raw_pos = gameServer.Position.CreateRawPosition();
                            raw_pos.sideToMove = raw_pos.sideToMove.Not();
                            var sfen = Position.SfenFromRawPosition(raw_pos);
                            gameServer.SetSfenCommand(sfen);
                        };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 平手の初期局面
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "平手の初期局面配置(&N)"; // No handicaped
                        item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.NoHandicap.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 駒落ちの局面
                        var item_handicap = new ToolStripMenuItem();
                        item_handicap.Enabled = inTheBoardEdit;
                        item_handicap.Text = "駒落ち初期局面配置(&H)"; // Handicaped
                        item_boardedit.DropDownItems.Add(item_handicap);

                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "香落ち(&1)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "右香落ち(&2)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapRightKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "角落ち(&3)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapKaku.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "飛車落ち(&4)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapHisya.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "飛香落ち(&5)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapHisyaKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "二枚落ち(&6)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap2.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "三枚落ち(&7)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap3.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "四枚落ち(&8)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap4.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "五枚落ち(&9)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap5.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "左五枚落ち(&A)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapLeft5.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "六枚落ち(&B)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap6.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "八枚落ち(&C)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap8.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "十枚落ち(&D)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap10.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "歩三枚(&E)";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HANDICAP_PAWN3.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }

                    }

                    {   // -- 詰将棋用の配置(駒箱に)
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "詰将棋用に配置(&M)"; // Mate
                        item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Mate1.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 双玉詰将棋用の局面
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "双玉詰将棋用に配置(&D)"; // Dual king mate
                        item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Mate2.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {
                        // - 双玉で玉以外すべて駒箱に

                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "双玉で玉以外すべて駒箱に配置(&U)"; // dUal king
                        item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Mate3.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }
                }

                var item_window = new ToolStripMenuItem();
                item_window.Text = "ウインドウ(&W)"; // Window
                menu.Items.Add(item_window);

                // -- 「ウインドウ」配下のメニュー
                {
                    { // ×ボタンで消していた検討ウィンドウの復活

                        var item_ = new ToolStripMenuItem();
                        item_.Text = "検討ウィンドウ(&C)"; // Consideration window
                        item_window.DropDownItems.Add(item_);

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "再表示(&V)"; // Visible
                            // 生成済みで非表示である時のみ
                            item.Enabled = engineConsiderationDialog != null && !engineConsiderationDialog.Visible;
                            item.Click += (sender, e) =>
                            {
                                if (engineConsiderationDialog != null)
                                {
                                    if (!engineConsiderationDialog.Visible)
                                        engineConsiderationDialog.Visible = true;
                                }
                            };
                            item_.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "メインウィンドウの移動に追随する(&F)"; // Follow the main window
                            item.Checked = config.ConsiderationWindowFollowMainWindow;
                            item.Click += (sender, e) =>
                            {
                                config.ConsiderationWindowFollowMainWindow ^= true;
                            };
                            item_.DropDownItems.Add(item);
                        }


                    }

                    { // -- 棋譜ウィンドウ


                        var item_ = new ToolStripMenuItem();
                        item_.Text = "棋譜ウィンドウ(&K)"; // Kifu window

                        item_window.DropDownItems.Add(item_);

                        { // 横幅
                            var item = new ToolStripMenuItem();
                            item.Text = "横幅(&W)"; // Width
                            item_.DropDownItems.Add(item);

                            {
                                var item1 = new ToolStripMenuItem();
                                item1.Text = "100%(通常)(&1)"; // None
                                item1.Checked = config.KifuWindowWidthType == 0;
                                item1.Click += (sender, e) => { config.KifuWindowWidthType = 0; };
                                item.DropDownItems.Add(item1);

                                var item2 = new ToolStripMenuItem();
                                item2.Text = "125%(&2)";
                                item2.Checked = config.KifuWindowWidthType == 1;
                                item2.Click += (sender, e) => { config.KifuWindowWidthType = 1; };
                                item.DropDownItems.Add(item2);

                                var item3 = new ToolStripMenuItem();
                                item3.Text = "150%(&3)";
                                item3.Checked = config.KifuWindowWidthType == 2;
                                item3.Click += (sender, e) => { config.KifuWindowWidthType = 2; };
                                item.DropDownItems.Add(item3);

                                var item4 = new ToolStripMenuItem();
                                item4.Text = "175%(&4)";
                                item4.Checked = config.KifuWindowWidthType == 3;
                                item4.Click += (sender, e) => { config.KifuWindowWidthType = 3; };
                                item.DropDownItems.Add(item4);

                                var item5 = new ToolStripMenuItem();
                                item5.Text = "200%(&5)";
                                item5.Checked = config.KifuWindowWidthType == 4;
                                item5.Click += (sender, e) => { config.KifuWindowWidthType = 4; };
                                item.DropDownItems.Add(item5);
                            }
                        }
                    }

                    item_window.DropDownItems.Add(new ToolStripSeparator());

                    {
                        // デバッグウィンドウ

                        var item_ = new ToolStripMenuItem();
                        item_.Text = "デバッグウィンドウ(&D)"; // Debug window

                        item_window.DropDownItems.Add(item_);

                        {
                            {
                                // メモリへのロギング

                                var item1 = new ToolStripMenuItem();
                                item1.Text = config.MemoryLoggingEnable ? "デバッグ終了(&B)" : "デバッグ開始(&B)"; // deBug
                                item1.Checked = config.MemoryLoggingEnable;
                                item1.ToolTipText = "思考エンジンとのやりとりを表示する機能です。";
                                item1.Click += (sender, e) =>
                                {
                                    config.MemoryLoggingEnable ^= true;
                                    if (!config.MemoryLoggingEnable && debugDialog != null)
                                    {
                                        debugDialog.Dispose(); // 終了させておく。
                                        debugDialog = null;
                                    }
                                };
                                item_.DropDownItems.Add(item1);
                            }

                            {
                                // デバッグウィンドウ

                                var item1 = new ToolStripMenuItem();
                                item1.Text = "デバッグウィンドウの表示(&D)"; // Debug Window
                                var enabled = config.MemoryLoggingEnable;
                                item1.Enabled = enabled;
                                if (!enabled)
                                    item1.ToolTipText = "デバッグウィンドウを表示するためには「デバッグの開始」を行う必要があります。";
                                item1.Click += (sender, e) =>
                                {
                                    if (debugDialog != null)
                                    {
                                        debugDialog.Dispose();
                                        debugDialog = null;
                                    }

                                    var log = Log.log1;
                                    if (log != null)
                                    {
                                        // セットされているはずなんだけどなぁ…。おかしいなぁ…。
                                        debugDialog = new DebugWindow((MemoryLog)log);
                                    }

                                    if (debugDialog != null)
                                    {
                                        FormLocationUtility.CenteringToThisForm(debugDialog, this);
                                        debugDialog.Show();
                                    }
                                };
                                item_.DropDownItems.Add(item1);
                            }

                            {
                                // ファイルへのロギング

                                var item1 = new ToolStripMenuItem();
                                var enabled = config.FileLoggingEnable;
                                item1.Text = enabled ? "ファイルへのロギング終了(&L)" : "ファイルへのロギング開始(&L)"; // Logging
                                item1.Checked = enabled;

                                item1.Click += (sender, e) => { config.FileLoggingEnable ^= true; };
                                item_.DropDownItems.Add(item1);
                            }

                            //item_.DropDownItems.Add(new ToolStripSeparator());

                        }

                    }

#if false // マスターアップに間に合わなさそう。
                    { // ×ボタンで消していた形勢グラフウィンドウの復活

                        var item = new ToolStripMenuItem();
                        item.Text = "形勢グラフウィンドウの表示(&G)"; // eval Graph
                        item.Click += (sender, e) =>
                        {
                            if (evalGraphDialog == null || evalGraphDialog.IsDisposed)
                            {
                                evalGraphDialog = new Info.EvalGraphDialog();
                            }
                            evalGraphDialog.DispatchEvalGraphUpdate(gameServer);
                            evalGraphDialog.Visible = true;
                        };
                        item_window.DropDownItems.Add(item);
                    }
#endif
                }

                // 「情報」
                {
                    var item_others = new ToolStripMenuItem();
                    item_others.Text = "情報(&I)"; // Infomation
                    menu.Items.Add(item_others);

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "よくある質問 (&F)"; // Faq
                        item1.Click += (sender, e) =>
                        {
                            // MyShogi公式のFAQ
                            var url = "https://github.com/yaneurao/MyShogi/tree/master/MyShogi/docs/faq.md";

                            System.Diagnostics.Process.Start(url);
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "操作説明(オンラインマニュアル) (&M)"; // Manual
                        item1.Click += (sender, e) =>
                        {
                            // MyShogi公式のonline manual
                            var url = "https://github.com/yaneurao/MyShogi/tree/master/MyShogi/docs/online_manual.md";

                            System.Diagnostics.Process.Start(url);
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    item_others.DropDownItems.Add(new ToolStripSeparator());

                    {
                        // aboutダイアログ

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "バージョン情報(&V)"; // Version
                        item1.Click += (sender, e) =>
                        {
                            using (var aboutDialog = new AboutYaneuraOu())
                            {
                                FormLocationUtility.CenteringToThisForm(aboutDialog, this);
                                aboutDialog.ShowDialog(this);
                            }
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    {
                        // システム情報ダイアログ

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "システム情報(&S)"; // System Infomation
                        item1.Click += (sender, e) =>
                        {
                            using (var cpuInfoDialog = new SystemInfo())
                            {
                                FormLocationUtility.CenteringToThisForm(cpuInfoDialog, this);
                                cpuInfoDialog.ShowDialog(this);
                            }
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    item_others.DropDownItems.Add(new ToolStripSeparator());

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "アップデートの確認(&U)"; // Update
                        item1.Click += (sender, e) =>
                        {
                            // ・オープンソース版は、MyShogiのプロジェクトのサイト
                            // ・商用版は、マイナビの公式サイトのアップデートの特設ページ
                            // が開くようにしておく。
                            var url = config.CommercialVersion == 0 ?
                                "https://github.com/yaneurao/MyShogi" :
                                "https://book.mynavi.jp/ec/products/detail/id=92007"; // 予定地

                            System.Diagnostics.Process.Start(url);
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                }

                // 開発時にだけオンにして使う。
#if false //DEBUG

                // デバッグ用にメニューにテストコードを実行する項目を追加する。
                {
                    var item_debug = new ToolStripMenuItem();
                    item_debug.Text = "デバッグ(&G)"; // debuG

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "DevTest1.Test1()";
                        item.Click += (sender, e) => { DevTest1.Test1(); };
                        item_debug.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "DevTest1.Test2()";
                        item.Click += (sender, e) => { DevTest1.Test2(); };
                        item_debug.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "DevTest1.Test3()";
                        item.Click += (sender, e) => { DevTest1.Test3(); };
                        item_debug.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "DevTest1.Test4()";
                        item.Click += (sender, e) => { DevTest1.Test4(); };
                        item_debug.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "DevTest1.Test5()";
                        item.Click += (sender, e) =>
                        {
                            // 何か実験用のコード、ここに書く。
                        };
                        item_debug.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "DevTest2.Test1()";
                        item.Click += (sender, e) => { DevTest2.Test1(); };
                        item_debug.DropDownItems.Add(item);
                    }

                    menu.Items.Add(item_debug);
                }
#endif

                Controls.Add(menu);
                // フォームのメインメニューとする
                MainMenuStrip = menu;
                old_menu = menu;

                // レイアウトロジックを再開する
                menu.ResumeLayout(false);
                menu.PerformLayout();
                ResumeLayout(false);
                PerformLayout();
            }

            // 画面の描画が必要になるときがあるので..
            gameScreenControl1.ForceRedraw();
        }

        private MenuStrip old_menu { get; set; } = null;

        #endregion


    }
}
