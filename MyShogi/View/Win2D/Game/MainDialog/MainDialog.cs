using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.String;
using MyShogi.Model.Common.Tool;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.Model.Shogi.Usi;
using MyShogi.View.Win2D.Setting;
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

            // ToolStripのフォントを設定しなおす。
            var fm = TheApp.app.Config.FontManager;
            FontUtility.ReplaceFont(toolStrip1 , fm.MainToolStrip);

            // フォントの変更は即時反映にする。
            // メインウインドウが解体されるときは終了する時だから、このハンドラのRemoveはしてない。
            fm.AddPropertyChangedHandler("FontChanged", (args) =>
            {
                var s = (string)args.value;
                if (s == "MainToolStrip")
                    FontUtility.ReplaceFont(toolStrip1, fm.MainToolStrip);

                // メニューのフォント
                else if (s == "MenuStrip")
                    FontUtility.ReplaceFont(old_menu, fm.MenuStrip);

                // メインウインドウは、再描画をしてフォント変更を反映させる。
                else if (s == "MainWindow")
                    gameScreenControl1?.ForceRedraw();
            });

            // 子コントロールへのキー入力も、まずはこのフォームが受け取らなければならない。
            KeyPreview = true;
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

            ViewModel.AddPropertyChangedHandler("LastFileName", _ => UpdateMenuItems() );

            // -- それ以外のハンドラの設定

            var config = TheApp.app.Config;

            config.KifuWindowDockManager.AddPropertyChangedHandler("DockState", UpdateKifuWindowDockState );
            config.KifuWindowDockManager.AddPropertyChangedHandler("DockPosition", UpdateKifuWindowDockState);

            config.EngineConsiderationWindowDockManager.AddPropertyChangedHandler("DockState", UpdateEngineConsiderationWindowDockState);
            config.EngineConsiderationWindowDockManager.AddPropertyChangedHandler("DockPosition", UpdateEngineConsiderationWindowDockState);

            // 棋譜ウインドウのfloating状態を起動時に復元する。
            //config.RaisePropertyChanged("KifuWindowFloating", config.KifuWindowFloating);
            // →　このタイミングで行うと、メインウインドウより先に棋譜ウインドウが出て気分が悪い。first_tickの処理で行うようにする。

            // 検討ウインドウ(に埋め込んでいる内部Control)の初期化
            engineConsiderationMainControl = new EngineConsiderationMainControl();
            engineConsiderationMainControl.Init(gameServer.BoardReverse /* これ引き継ぐ。以降は知らん。*/);
            engineConsiderationMainControl.ConsiderationInstance(0).ViewModel.AddPropertyChangedHandler("MultiPV", (h) => {
                gameServer.ChangeMultiPvCommand((int)h.value);
            });

            // ToolStripのShortcutを設定する。
            // これは、engineConsiderationMainControlの初期化が終わっている必要がある。
            UpdateToolStripShortcut();
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

        /// <summary>
        /// 棋譜ウインドウをフローティングモードで使っているとき用。
        /// </summary>
        public DockWindow kifuDockWindow { get; set; }

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
        /// 検討ウインドウを埋めて使うための入れ物。
        /// エンジンの思考出力用。
        /// </summary>
        public DockWindow engineConsiderationDockWindow;

        /// <summary>
        /// これが検討ウインドウ本体。これを↑のに埋めて使う。
        /// </summary>
        public EngineConsiderationMainControl engineConsiderationMainControl;

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

            // いまから検討を開始するのに検討ウインドウが非表示なら表示させる。
            var dock = TheApp.app.Config.EngineConsiderationWindowDockManager;
            if (!consideration && !dock.Visible)
            {
                dock.Visible ^= true;
                dock.RaisePropertyChanged("DockState", dock.DockState);
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

            // いまから検討を開始するのに検討ウインドウが非表示なら表示させる。
            var dock = TheApp.app.Config.EngineConsiderationWindowDockManager;
            if (!mate_consideration && !dock.Visible)
            {
                dock.Visible ^= true;
                dock.RaisePropertyChanged("DockState", dock.DockState);
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

        /// <summary>
        /// 棋譜ウインドウをfloating modeにする/戻す
        /// </summary>
        private void UpdateKifuWindowDockState(PropertyChangedEventArgs args)
        {
            var dockState = (DockState)args.value;
            kifuControl.ViewModel.DockState = dockState;

            var dockManager = TheApp.app.Config.KifuWindowDockManager;
            dockManager.DockState = dockState; // 次回起動時のためにここに保存しておく。

            // 何にせよ、いったん解除する。
            if (kifuDockWindow != null)
            {
                kifuDockWindow.RemoveControl();
                kifuDockWindow.Dispose();
                kifuDockWindow = null;
            }
            if (gameScreenControl1.Controls.Contains(kifuControl))
                gameScreenControl1.Controls.Remove(kifuControl);

            // dockManager.Visibleは反映させないと駄目。
            if (!dockManager.Visible)
            {
                // フォーカス移動されてると困るので戻す。
                this.Focus();
                return;
            }

            if (dockState == DockState.InTheMainWindow)
            {
                gameScreenControl1.Controls.Add(kifuControl);
                gameScreenControl1.ResizeKifuControl(); // フォームに埋めたあとリサイズする。

                // 細長い駒台のときはVisibleにしないのでここで制御しない。
            }
            else
            {
                kifuDockWindow = new DockWindow();
                kifuDockWindow.ViewModel.AddPropertyChangedHandler("MenuUpdated", _ => UpdateMenuItems());
                kifuDockWindow.Owner = this;

                //kifuDockWindow.TopMost = true; // Ownerを設定してしまうと、main windowを×で閉じたときに先にこのFormClosingが呼び出されてしまう。
                // →　かと言ってTopMostは、ファイルダイアログを開くときなど常に最前面に出てくるので罪が重い。
                // MainWindowのOnMoveなどに対してだけ前面に持ってきてはどうか。
                // →　resize,move,…、イベントを捕捉するだけでは、漏れがあるようで棋譜ウインドウが前面に来ない瞬間がある。
                // 結論的には、OwnerをMainWindowにして、Close()のキャンセル処理はしないようにする。

                kifuDockWindow.ViewModel.Caption = "棋譜ウインドウ";

                // デフォルト位置とサイズにする。
                if (dockManager.Size.IsEmpty)
                {
                    // メインウインドウに埋め込み時の棋譜ウインドウのサイズをデフォルトとしておいてやる。
                    dockManager.Size = gameScreenControl1.CalcKifuWindowSize();
                    var pos = gameScreenControl1.CalcKifuWindowLocation();
                    // これクライアント座標なので、スクリーン座標にいったん変換する。
                    pos = gameScreenControl1.PointToScreen(pos);

                    dockManager.LocationOnDocked = new Point(pos.X - this.Location.X, pos.Y - this.Location.Y);
                    dockManager.LocationOnFloating = pos;
                }

                // Showで表示とサイズが確定してからdockManagerを設定しないと、
                // Showのときの位置とサイズがdockManagerに記録されてしまう。
                kifuControl.Visible = true; // 細長い駒台モードのため非表示にしていたかも知れないので。

                kifuDockWindow.AddControl(kifuControl, this, dockManager);
                dockManager.InitDockWindowLocation(this, kifuDockWindow);

                kifuDockWindow.Show();
            }
        }

        /// <summary>
        /// UpdateKifuWindowDockState()の検討ウインドウ用。
        /// だいたい同じ感じの処理。
        /// </summary>
        private void UpdateEngineConsiderationWindowDockState(PropertyChangedEventArgs args)
        {
            var dockState = (DockState)args.value;
            //kifuControl.ViewModel.DockState = dockState;

            var dockManager = TheApp.app.Config.EngineConsiderationWindowDockManager;
            dockManager.DockState = dockState; // 次回起動時のためにここに保存しておく。

            if (engineConsiderationDockWindow != null)
            {
                engineConsiderationDockWindow.RemoveControl();
                engineConsiderationDockWindow.Dispose();
                engineConsiderationDockWindow = null;
            }
            if (this.Controls.Contains(engineConsiderationMainControl))
            {
                this.Controls.Remove(engineConsiderationMainControl);
                ResizeConsiderationControl(); // フォームに埋めたあとリサイズする。
            }

            // dockManager.Visibleは反映させないと駄目。
            if (!dockManager.Visible)
            {
                // フォーカス移動されてると困るので戻す。
                this.Focus();
                return;
            }

            if (dockState == DockState.InTheMainWindow)
            {
                this.Controls.Add(engineConsiderationMainControl);
                ResizeConsiderationControl(); // フォームに埋めたあとリサイズする。
            }
            else
            {
                engineConsiderationDockWindow = new DockWindow();
                engineConsiderationDockWindow.ViewModel.AddPropertyChangedHandler("MenuUpdated", _ => UpdateMenuItems());
                engineConsiderationDockWindow.Owner = this;

                engineConsiderationDockWindow.ViewModel.Caption = "検討ウインドウ";

                // デフォルト位置とサイズにする。
                if (dockManager.Size.IsEmpty)
                {
                    // デフォルトでは、このウインドウサイズに従う
                    dockManager.Size = new Size(Width , Height /4);

                    var pos = gameScreenControl1.CalcKifuWindowLocation();
                    // これクライアント座標なので、スクリーン座標にいったん変換する。
                    pos = gameScreenControl1.PointToScreen(pos);

                    dockManager.LocationOnDocked = new Point(pos.X - this.Location.X, pos.Y - this.Location.Y);
                    dockManager.LocationOnFloating = pos;
                }

                // Showで表示とサイズが確定してからdockManagerを設定しないと、
                // Showのときの位置とサイズがdockManagerに記録されてしまう。
                engineConsiderationMainControl.Visible = true;
                
                engineConsiderationDockWindow.AddControl(engineConsiderationMainControl, this, dockManager);
                dockManager.InitDockWindowLocation(this, engineConsiderationDockWindow);

                engineConsiderationDockWindow.Show();
            }
        }

        /// <summary>
        /// 検討ウインドウのControlのサイズを調整。
        /// (このウインドウに埋め込んでいるとき)
        /// </summary>
        public void ResizeConsiderationControl(PropertyChangedEventArgs args= null)
        {
            if (first_tick)
                return;
            // first_tick前だとengineConsiderationMainControl == nullだったりしてまずいのだ。

            // 検討ウインドウをこのウインドウに埋め込んでいるときに、検討ウインドウをリサイズする。
            using (var slb1 = new SuspendLayoutBlock(this))
            using (var slb2 = new SuspendLayoutBlock(gameScreenControl1))
            {

                int w = ClientSize.Width;
                int h = ClientSize.Height - gameScreenControl1.Location.Y; // メニューとToolStripの高さを引き算する。

                var config = TheApp.app.Config;
                var dockManager = config.EngineConsiderationWindowDockManager;

                // 非表示のときはないものとして扱う。
                // すなわち、DockWindow側にあるものとして、gameScreenControl1をDockStyle.Fillにする必要がある。

                if (dockManager.DockState == DockState.InTheMainWindow && dockManager.Visible)
                {
                    // メインウインドウに埋め込み時の検討ウインドウ高さの倍率
                    float height_rate = 1 + 0.25f * config.ConsiderationWindowHeightType;

                    // 検討ウインドウの縦幅
                    var ch = (int)(height_rate * h / 5);

                    DockUtility.Change(gameScreenControl1            , DockStyle.None , new Size(w, h - ch) , null /*いまの場所*/ );
                    DockUtility.Change(engineConsiderationMainControl, DockStyle.None , new Size(w, ch)     , new Point(0, ClientSize.Height - ch));
                }
                else
                {
                    DockUtility.Change(gameScreenControl1            , DockStyle.Fill , new Size(w, h), null);
                    DockUtility.Change(engineConsiderationMainControl, DockStyle.Fill , null , null );
                }
            }
        }

        #endregion

        #region privates

        /// <summary>
        /// コマンドラインで何か指示されていたら、それを行う。
        ///
        /// コマンドの例
        ///   -f [棋譜ファイル名] : 棋譜ファイルの読み込み予約(fileのf)
        /// </summary>
        private void CommandLineCheck()
        {
            var parser = new CommandLineParser();
            var firstToken = true;

            while (true)
            {
                var token = parser.GetText();
                if (token == null)
                    break;

                switch (token.ToLower())
                {
                    // "-f"はそのあとに続くファイルを読み込むためのコマンド。
                    case "-f":
                        if (parser.PeekText() == null)
                            TheApp.app.MessageShow("コマンドライン引数の-fの次にファイル名が指定されていません。", MessageShowType.Error);
                        else
                            ReadKifuFile(parser.GetText());
                        break;

                    default:
                        // 一つ目のtokenとして直接ファイル名が指定されている可能性がある。
                        // この処理は拡張子関連付けのために必要。
                        if (firstToken)
                            ReadKifuFile(token);
                        else
                            TheApp.app.MessageShow("コマンドライン引数に解釈できない文字列がありました。", MessageShowType.Error);
                        break;
                }

                firstToken = false;
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
                toolStripButton9_Click(sender,e); // 1手戻るボタンに委譲
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

            // 検討ウインドウ
            {
                if (engineConsiderationDockWindow != null)
                {
                    var dockManager = TheApp.app.Config.EngineConsiderationWindowDockManager;
                    dockManager.UpdateDockWindowLocation(this, engineConsiderationDockWindow);
                }
            }

            // 棋譜ウインドウも。
            {
                if (kifuDockWindow != null)
                {
                    var dockManager = TheApp.app.Config.KifuWindowDockManager;
                    dockManager.UpdateDockWindowLocation(this, kifuDockWindow);
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
        /// LocalGameServerから送られてくるエンジンの読み筋などのハンドラ。
        /// </summary>
        private void ThinkReportChanged(PropertyChangedEventArgs args)
        {
            var message = args.value as UsiThinkReportMessage;
            engineConsiderationMainControl.EnqueueThinkReportMessage(message);

#if false // このデバッグをしているとマスターアップに間に合わなさそう。後回し。
            // 評価値グラフの更新など
            gameServer.ThinkReportChangedCommand(message);

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
            gameScreenControl1.DoMoveCommand(SCore.Move.RESIGN );
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

            // 検討ウインドウのdockのためのリファクタリング中
#if false
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
#endif
        }

#endregion

#region update menu

        /// <summary>
        /// 棋譜ファイルを読み込む。
        /// </summary>
        /// <param name="path"></param>
        private void ReadKifuFile(string path)
        {
            try
            {
                var kifu_text = FileIO.ReadText(path);
                gameServer.KifuReadCommand(kifu_text);
                ViewModel.LastFileName = path; // 最後に開いたファイルを記録しておく。

                // このファイルを用いたのでMRUFに記録しておく。
                UseKifuFile(path);
            }
            catch
            {
                TheApp.app.MessageShow($"ファイル読み込みエラー。ファイル {path} の読み込みに失敗しました。", MessageShowType.Error);
            }
        }

        /// <summary>
        /// 棋譜ファイルを使った時に呼び出されるべきハンドラ。
        /// MRUFを更新する。
        /// </summary>
        private void UseKifuFile(string path)
        {
            // このファイルを用いたのでMRUFに記録しておく。
            if (TheApp.app.Config.MRUF.UseFile(path))
                UpdateMenuItems();
        }

        /// <summary>
        /// 前回にUpdateMenuItems()が呼び出された時のGameMode。
        /// </summary>
        private GameModeEnum lastGameMode = GameModeEnum.ConsiderationWithoutEngine;

        /// <summary>
        /// メインウインドウのCaption部分を更新する。
        /// </summary>
        /// <param name="args"></param>
        public void UpdateCaption(PropertyChangedEventArgs args = null)
        {
            var config = TheApp.app.Config;

            // 読み込んでいる棋譜ファイル名(メインウインドウの上に表示しておく)
            var readfilename = ViewModel.LastFileName == null ? null : $" - {Path.GetFileName(ViewModel.LastFileName)}";

            // Commercial Version GUI
            bool CV_GUI = config.CommercialVersion != 0;
            if (CV_GUI)
                Text = "将棋神やねうら王" + readfilename;
            else
                Text = "MyShogi" + readfilename;
            // 商用版とどこで差別化するのか考え中
        }


        /// <summary>
        /// 対局盤面の再描画を促す。
        /// </summary>
        /// <param name="args"></param>
        public void ForceRedraw(PropertyChangedEventArgs args)
        {
            gameScreenControl1.ForceRedraw();
        }

#endregion

    }
}
