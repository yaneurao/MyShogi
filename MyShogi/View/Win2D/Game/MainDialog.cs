using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.String;
using MyShogi.Model.Common.Tool;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
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
        /// [UI thread] : メニューのitemを動的に追加する。
        /// 商用版とフリーウェア版とでメニューが異なるのでここで動的に追加する必要がある。
        /// </summary>
        public void UpdateMenuItems(PropertyChangedEventArgs args = null)
        {
            // 頑張れば高速化出来るが、対局中はこのメソッド呼び出されていないし、
            // ToolStripも、CPU×CPUの対局中は更新は発生していないし、
            // CPU×人間のときは多少遅くても誤差だし、まあいいか…。

            var config = TheApp.app.Config;
            var shortcut = TheApp.app.KeyShortcut;
            shortcut.OnKeyDown = null; // このdelegateにShortcutキーのハンドラを登録していく。

            // 使ったファイル名をメインウインドウのText部に描画する必要がある。
            UpdateCaption();

            // -- メニューの追加。
            {
                //レイアウトロジックを停止する
                using (var slb = new SuspendLayoutBlock(this))
                {
                    // MenuStripだと非アクティブ状態からのクリックで反応しないのでMenuStripExを使う。
                    var menu = new MenuStripEx();

                    // 前回設定されたメニューを除去する
                    // 古いほうのmenu、removeしないと駄目
                    if (old_menu != null)
                    {
                        Controls.Remove(old_menu);
                        old_menu.Dispose();
                        old_menu = null;
                    }

                    // -- LocalGameServerの各フラグ。
                    // ただし、初期化時にgameServer == nullで呼び出されることがあるのでnull checkが必要。

                    // LocalGameServer.GameModeは値がいま書き換わっている可能性があるので、イベントを除かしてしまう可能性がある。
                    // ゆえに、引数で渡ってきたargs.value (GameModeEnum)を用いる必要があるが、しかし、args.valueが他の型である可能性もある。(BoardReverseなどを渡すとき)
                    // このため、args.valueがGameModeEnumなら、これを用いて、さもなくば仕方ないので前回渡されたものをそのまま用いる。
                    // (LocalGameServerの値はメニューには直接使わない)
                    var gameMode =
                        (args != null && args.value != null && args.value is GameModeEnum) ? (GameModeEnum)args.value :
                        gameServer == null ? GameModeEnum.NotInit :
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
                            // サブウインドウでのショートカットキーの処理
                            shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.O) item.PerformClick(); };
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
                                        ReadKifuFile(fd.FileName);
                                }
                            };
                            item_file.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "棋譜の上書き保存(&S)";
                            item.ShortcutKeys = Keys.Control | Keys.S;
                            // サブウインドウでのショートカットキーの処理
                            shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.S) item.PerformClick(); };
                            item.Enabled = ViewModel.LastFileName != null; // 棋譜を読み込んだ時などにしか有効ではない。
                            item.Click += (sender, e) =>
                            {
                                var path = ViewModel.LastFileName;

                            // 「開く」もしくは「名前をつけて保存無したファイルに上書きする。
                            // 「局面の保存」は棋譜ではないのでこれは無視する。
                            // ファイル形式は、拡張子から自動判別する。
                            gameServer.KifuWriteCommand(path, KifuFileTypeExtensions.StringToKifuFileType(path));

                            //UseKifuFile(path);
                            // 上書き保存の直前にこのファイルを開いていて、そのときにMRUFに記録されているはず。
                        };
                            item_file.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "棋譜に名前をつけて保存(&N)";
                            item.ShortcutKeys = Keys.Control | Keys.S | Keys.Shift;
                            shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.S && e.Shift) item.PerformClick(); };
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
                                var default_filename = $"{gameServer.DefaultKifuFileName()}.KIF";
                                    fd.FileName = default_filename;
                                // これでescapeされているし、ダイアログが使えないファイル名は返さないから、以降のescapeは不要。

                                // ダイアログを表示する
                                if (fd.ShowDialog() == DialogResult.OK)
                                    {
                                        var path = fd.FileName;
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
                                                    kifuType = KifuFileTypeExtensions.StringToKifuFileType(path);
                                                    if (kifuType == KifuFileType.UNKNOWN)
                                                        kifuType = KifuFileType.KIF; // わからんからKIF形式でいいや。
                                                break;
                                            }

                                            gameServer.KifuWriteCommand(path, kifuType);
                                            ViewModel.LastFileName = path; // 最後に保存したファイルを記録しておく。
                                        UseKifuFile(path);
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
                                        var path = fd.FileName;
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
                                                    kifuType = KifuFileTypeExtensions.StringToKifuFileType(path);
                                                    if (kifuType == KifuFileType.UNKNOWN)
                                                        kifuType = KifuFileType.KIF; // わからんからKIF形式でいいや。
                                                break;
                                            }

                                            gameServer.PositionWriteCommand(path, kifuType);

                                        // このファイルを用いたのでMRUFに記録しておく。
                                        UseKifuFile(path);
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
                            shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.C) item.PerformClick(); };

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
                            shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.V) item.PerformClick(); };
                            item.Click += (sender, e) => { CopyFromClipboard(); };
                            item_file.DropDownItems.Add(item);
                        }

                        item_file.DropDownItems.Add(new ToolStripSeparator());

                        // -- 「棋譜編集」

                        var kifu_edit = new ToolStripMenuItem();
                        kifu_edit.Text = "棋譜編集(&K)"; // Kifu edit
                        kifu_edit.Enabled = !inTheGame;
                        item_file.DropDownItems.Add(kifu_edit);

                        // -- 「棋譜編集」配下のメニュー
                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "本譜以外の分岐をクリアする(&C)"; // Clear
                            item.Click += (sender, e) =>
                            {
                                if (TheApp.app.MessageShow("この操作により現在の棋譜上の本譜以外の分岐は削除されます。",
                                    MessageShowType.WarningOkCancel) == DialogResult.OK)
                                {
                                    gameServer.ClearSubKifuTreeCommand();
                                }
                            };
                            kifu_edit.DropDownItems.Add(item);
                        }

                        item_file.DropDownItems.Add(new ToolStripSeparator());

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "終了(&X)";
                            item.Click += (sender, e) => { TheApp.app.ApplicationExit(); };
                            item_file.DropDownItems.Add(item);
                        }

                        // MRUF : 最近使ったファイル

                        {
                            var mruf = TheApp.app.Config.MRUF;
                            ToolStripMenuItem sub_item = null;
                            for (int i = 0; i < mruf.Files.Count; ++i)
                            {
                                var display_name = mruf.GetDisplayFileName(i);
                                if (display_name == null)
                                    break;

                                if (i == 0)
                                    item_file.DropDownItems.Add(new ToolStripSeparator());
                                else if (i == 3)
                                {
                                    sub_item = new ToolStripMenuItem();
                                    sub_item.Text = "ファイルヒストリーのつづき(&R)";
                                    item_file.DropDownItems.Add(sub_item);
                                }

                                {
                                    var item = new ToolStripMenuItem();
                                    item.Text = display_name;
                                    var kifu_file_path = mruf.Files[i];
                                    item.Click += (sender, e) => { ReadKifuFile(kifu_file_path); };
                                    if (i < 3)
                                        item_file.DropDownItems.Add(item);
                                    else
                                        sub_item.DropDownItems.Add(item);
                                }

                                if (i == mruf.Files.Count - 1) // 最後の要素
                                {
                                    var item = new ToolStripMenuItem();
                                    item.Text = "ファイルヒストリーのクリア(&T)";
                                    item.Click += (sender, e) =>
                                    {
                                        if (TheApp.app.MessageShow("ファイルヒストリーをクリアしますか？「OK」を押すとクリアされます。", MessageShowType.ConfirmationOkCancel) == DialogResult.OK)
                                        {
                                            mruf.Clear();
                                            UpdateMenuItems();
                                        }
                                    };
                                    item_file.DropDownItems.Add(item);
                                }
                            }
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
                            shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.N) item.PerformClick(); };
                            item.Click += (sender, e) =>
                            {
                                using (var dialog = new GameSettingDialog(this))
                                {
                                    FormLocationUtility.CenteringToThisForm(dialog, this);
                                    dialog.ShowDialog(this); // Modal Dialogにしておく。
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
                            item.Click += (sender, e) =>
                            {
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
                            item.Click += (sender, e) =>
                            {
                                if (mate_consideration)
                                    ToggleMateConsideration();
                                else
                                    ShowMateEngineSettingDialog(); // 詰検討エンジンの選択画面に

                            };

                            item_playgame.DropDownItems.Add(item);
                        }
                    }

                    // 「設定」
                    var item_settings = new ToolStripMenuItem();
                    item_settings.Text = "設定(&S)"; // Settings
                    menu.Items.Add(item_settings);
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "音声設定 (&S)"; // Sound setting
                        item.Enabled = config.CommercialVersion != 0; // 商用版のみ選択可
                        item.Click += (sender, e) =>
                        {
                            using (var dialog = new SoundSettingDialog())
                            {
                                FormLocationUtility.CenteringToThisForm(dialog, this);
                                dialog.ShowDialog(this);
                            }
                        };
                        item_settings.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "表示設定 (&D)"; // Display setting
                        item.Click += (sender, e) =>
                        {
                            using (var dialog = new DisplaySettingDialog())
                            {
                                FormLocationUtility.CenteringToThisForm(dialog, this);
                                dialog.ShowDialog(this);
                            }
                        };
                        item_settings.DropDownItems.Add(item);
                    }

                    item_settings.DropDownItems.Add(new ToolStripSeparator());

                    // -- 設定の初期化
                    {
                        var item_init = new ToolStripMenuItem();
                        item_init.Text = "設定の初期化(&I)";
                        item_settings.DropDownItems.Add(item_init);

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "各エンジン設定の初期化";
                            item.Click += (sender, e) =>
                            {
                                if (TheApp.app.MessageShow("すべてのエンジン設定を初期化しますか？「OK」を押すと初期化され、次回起動時に反映されます。", MessageShowType.ConfirmationOkCancel) == DialogResult.OK)
                                {
                                    TheApp.app.DeleteEngineOption = true;
                                }
                            };
                            item_init.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "各表示設定などの初期化";
                            item.Click += (sender, e) =>
                            {
                                if (TheApp.app.MessageShow("すべての表示設定・音声設定を初期化しますか？「OK」を押すと初期化され、次回起動時に反映されます。", MessageShowType.ConfirmationOkCancel) == DialogResult.OK)
                                {
                                    TheApp.app.DeleteGlobalOption = true;
                                }
                            };
                            item_init.DropDownItems.Add(item);
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
                            shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.E) item.PerformClick(); };
                            item.Click += (sender, e) =>
                            {
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
                                gameScreenControl1.SetSfenCommand(sfen);
                            };
                            item_boardedit.DropDownItems.Add(item);
                        }

                        {   // -- 平手の初期局面
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "平手の初期局面配置(&N)"; // No handicaped
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.NoHandicap.ToSfen()); };
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
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapKyo.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "右香落ち(&2)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapRightKyo.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "角落ち(&3)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapKaku.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "飛車落ち(&4)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapHisya.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "飛香落ち(&5)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapHisyaKyo.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "二枚落ち(&6)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap2.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "三枚落ち(&7)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap3.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "四枚落ち(&8)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap4.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "五枚落ち(&9)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap5.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "左五枚落ち(&A)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapLeft5.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "六枚落ち(&B)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap6.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "八枚落ち(&C)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap8.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "十枚落ち(&D)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap10.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }
                            {
                                var item = new ToolStripMenuItem();
                                item.Enabled = inTheBoardEdit;
                                item.Text = "歩三枚(&E)";
                                item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapPawn3.ToSfen()); };
                                item_handicap.DropDownItems.Add(item);
                            }

                        }

                        {   // -- 詰将棋用の配置(駒箱に)
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "詰将棋用に配置(&M)"; // Mate
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Mate1.ToSfen()); };
                            item_boardedit.DropDownItems.Add(item);
                        }

                        {   // -- 双玉詰将棋用の局面
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "双玉詰将棋用に配置(&D)"; // Dual king mate
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Mate2.ToSfen()); };
                            item_boardedit.DropDownItems.Add(item);
                        }

                        {
                            // - 双玉で玉以外すべて駒箱に

                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "双玉で玉以外すべて駒箱に配置(&U)"; // dUal king
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Mate3.ToSfen()); };
                            item_boardedit.DropDownItems.Add(item);
                        }
                    }

                    // -- 「ウインドウ」

                    var item_window = new ToolStripMenuItem();
                    item_window.Text = "ウインドウ(&W)"; // Window
                    menu.Items.Add(item_window);

                    // -- 「ウインドウ」配下のメニュー
                    {
                        { // ×ボタンで消していた検討ウィンドウの復活

                            var item_ = new ToolStripMenuItem();
                            item_.Text = "検討ウィンドウ(&C)"; // Consideration window
                            item_window.DropDownItems.Add(item_);

                            var dock = config.EngineConsiderationWindowDockManager;

                            {
                                var item = new ToolStripMenuItem();
                                item.Text = dock.Visible ? "非表示(&V)" : "再表示(&V)"; // visible // 
                                item.ShortcutKeys = Keys.Control | Keys.R; // EngineConsiderationWindowのR。Eが盤面編集のEditのEで使ってた…。
                                shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.R) item.PerformClick(); };
                                item.Click += (sender, e) => { dock.Visible ^= true; dock.RaisePropertyChanged("DockState", dock.DockState); };
                                item_.DropDownItems.Add(item);
                            }


                            { // フローティングの状態
                                var item = new ToolStripMenuItem();
                                item.Text = "表示位置(&F)"; // Floating window mode
                                item_.DropDownItems.Add(item);

                                {

                                    var item1 = new ToolStripMenuItem();
                                    item1.Text = "メインウインドウに埋め込む(&0)(EmbeddedMode)";
                                    item1.Checked = dock.DockState == DockState.InTheMainWindow;
                                    item1.Click += (sender, e) => { dock.DockState = DockState.InTheMainWindow; };
                                    item.DropDownItems.Add(item1);

                                    var item2 = new ToolStripMenuItem();
                                    item2.Text = "メインウインドウから浮かせ、相対位置を常に保つ(&1)(FollowMode)";
                                    item2.Checked = dock.DockState == DockState.FollowToMainWindow;
                                    item2.Click += (sender, e) => { dock.DockState = DockState.FollowToMainWindow; };
                                    item.DropDownItems.Add(item2);

                                    var item3a = new ToolStripMenuItem();
                                    item3a.Text = "メインウインドウから浮かせ、メインウインドウの上側に配置する(&2)(DockMode)";
                                    item3a.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Top;
                                    item3a.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Top); };
                                    item.DropDownItems.Add(item3a);

                                    var item3b = new ToolStripMenuItem();
                                    item3b.Text = "メインウインドウから浮かせ、メインウインドウの左側に配置する(&3)(DockMode)";
                                    item3b.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Left;
                                    item3b.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Left); };
                                    item.DropDownItems.Add(item3b);

                                    var item3c = new ToolStripMenuItem();
                                    item3c.Text = "メインウインドウから浮かせ、メインウインドウの右側に配置する(&4)(DockMode)";
                                    item3c.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Right;
                                    item3c.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Right); };
                                    item.DropDownItems.Add(item3c);

                                    var item3d = new ToolStripMenuItem();
                                    item3d.Text = "メインウインドウから浮かせ、メインウインドウの下側に配置する(&5)(DockMode)";
                                    item3d.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Bottom;
                                    item3d.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Bottom); };
                                    item.DropDownItems.Add(item3d);

                                    var item4 = new ToolStripMenuItem();
                                    item4.Text = "メインウインドウから浮かせ、自由に配置する(&6)(FloatingMode)";
                                    item4.Checked = dock.DockState == DockState.FloatingMode;
                                    item4.Click += (sender, e) => { dock.DockState = DockState.FloatingMode; };
                                    item.DropDownItems.Add(item4);
                                }
                            }

                            { // 縦幅
                                var item = new ToolStripMenuItem();
                                item.Text = "メインウインドウに埋め込み時の高さ(&H)"; // Height
                                item_.DropDownItems.Add(item);

                                {
                                    var item1 = new ToolStripMenuItem();
                                    item1.Text = "100%(通常)(&1)"; // None
                                    item1.Checked = config.ConsiderationWindowHeightType == 0;
                                    item1.Click += (sender, e) => { config.ConsiderationWindowHeightType = 0; };
                                    item.DropDownItems.Add(item1);

                                    var item2 = new ToolStripMenuItem();
                                    item2.Text = "125%(&2)";
                                    item2.Checked = config.ConsiderationWindowHeightType == 1;
                                    item2.Click += (sender, e) => { config.ConsiderationWindowHeightType = 1; };
                                    item.DropDownItems.Add(item2);

                                    var item3 = new ToolStripMenuItem();
                                    item3.Text = "150%(&3)";
                                    item3.Checked = config.ConsiderationWindowHeightType == 2;
                                    item3.Click += (sender, e) => { config.ConsiderationWindowHeightType = 2; };
                                    item.DropDownItems.Add(item3);

                                    var item4 = new ToolStripMenuItem();
                                    item4.Text = "175%(&4)";
                                    item4.Checked = config.ConsiderationWindowHeightType == 3;
                                    item4.Click += (sender, e) => { config.ConsiderationWindowHeightType = 3; };
                                    item.DropDownItems.Add(item4);

                                    var item5 = new ToolStripMenuItem();
                                    item5.Text = "200%(&5)";
                                    item5.Checked = config.ConsiderationWindowHeightType == 4;
                                    item5.Click += (sender, e) => { config.ConsiderationWindowHeightType = 4; };
                                    item.DropDownItems.Add(item5);
                                }
                            }

                        }

                        { // -- 棋譜ウィンドウ

                            var item_ = new ToolStripMenuItem();
                            item_.Text = "棋譜ウィンドウ(&K)"; // Kifu window

                            item_window.DropDownItems.Add(item_);

                            var dock = config.KifuWindowDockManager;

                            {
                                var item = new ToolStripMenuItem();
                                item.Text = dock.Visible ? "非表示(&V)" : "再表示(&V)"; // visible // 
                                item.ShortcutKeys = Keys.Control | Keys.K; // KifuWindow
                                shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.K) item.PerformClick(); };
                                item.Click += (sender, e) => { dock.Visible ^= true; dock.RaisePropertyChanged("DockState", dock.DockState); };
                                item_.DropDownItems.Add(item);
                            }


                            { // フローティングの状態
                                var item = new ToolStripMenuItem();
                                item.Text = "表示位置(&F)"; // Floating window mode
                                item_.DropDownItems.Add(item);

                                {

                                    var item1 = new ToolStripMenuItem();
                                    item1.Text = "メインウインドウに埋め込む(&0)(EmbeddedMode)";
                                    item1.Checked = dock.DockState == DockState.InTheMainWindow;
                                    item1.Click += (sender, e) => { dock.DockState = DockState.InTheMainWindow; };
                                    item.DropDownItems.Add(item1);

                                    var item2 = new ToolStripMenuItem();
                                    item2.Text = "メインウインドウから浮かせ、相対位置を常に保つ(&1)(FollowMode)";
                                    item2.Checked = dock.DockState == DockState.FollowToMainWindow;
                                    item2.Click += (sender, e) => { dock.DockState = DockState.FollowToMainWindow; };
                                    item.DropDownItems.Add(item2);

                                    var item3a = new ToolStripMenuItem();
                                    item3a.Text = "メインウインドウから浮かせ、メインウインドウの上側に配置する(&2)(DockMode)";
                                    item3a.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Top;
                                    item3a.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Top); };
                                    item.DropDownItems.Add(item3a);

                                    var item3b = new ToolStripMenuItem();
                                    item3b.Text = "メインウインドウから浮かせ、メインウインドウの左側に配置する(&3)(DockMode)";
                                    item3b.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Left;
                                    item3b.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Left); };
                                    item.DropDownItems.Add(item3b);

                                    var item3c = new ToolStripMenuItem();
                                    item3c.Text = "メインウインドウから浮かせ、メインウインドウの右側に配置する(&4)(DockMode)";
                                    item3c.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Right;
                                    item3c.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Right); };
                                    item.DropDownItems.Add(item3c);

                                    var item3d = new ToolStripMenuItem();
                                    item3d.Text = "メインウインドウから浮かせ、メインウインドウの下側に配置する(&5)(DockMode)";
                                    item3d.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Bottom;
                                    item3d.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Bottom); };
                                    item.DropDownItems.Add(item3d);

                                    var item4 = new ToolStripMenuItem();
                                    item4.Text = "メインウインドウから浮かせ、自由に配置する(&6)(FloatingMode)";
                                    item4.Checked = dock.DockState == DockState.FloatingMode;
                                    item4.Click += (sender, e) => { dock.DockState = DockState.FloatingMode; };
                                    item.DropDownItems.Add(item4);
                                }
                            }

                            { // 横幅
                                var item = new ToolStripMenuItem();
                                item.Text = "メインウインドウに埋め込み時の横幅(&W)"; // Width
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

                        { // -- 対局結果一覧ウィンドウ

                            var item_ = new ToolStripMenuItem();
                            item_.Text = "対局結果一覧(&R)"; // game Result
                            item_.Click += (sender, e) =>
                            {
                                using (var dialog = new GameResultDialog())
                                {
                                    FormLocationUtility.CenteringToThisForm(dialog, this);
                                    dialog.ViewModel.AddPropertyChangedHandler("KifuClicked", (args_) =>
                                    {
                                        var filename = (string)args_.value;
                                    // このファイルを読み込む。
                                    var path = Path.Combine(TheApp.app.Config.GameResultSetting.KifuSaveFolder, filename);
                                        try
                                        {
                                            ReadKifuFile(path);
                                        }
                                        catch
                                        {
                                            TheApp.app.MessageShow("棋譜ファイルが読み込めませんでした。", MessageShowType.Error);
                                        }
                                    });
                                    dialog.ShowDialog(this);
                                }
                            };

                            item_window.DropDownItems.Add(item_);
                        }


                        { // -- 対局結果一覧ウィンドウ

                            var item_ = new ToolStripMenuItem();
                            item_.Text = "対局結果の保存設定(&S)"; // アルファベット的にRの次
                            item_.Click += (sender, e) =>
                            {
                                using (var dialog = new GameResultWindowSettingDialog())
                                {
                                    FormLocationUtility.CenteringToThisForm(dialog, this);
                                    dialog.ShowDialog(this);
                                }
                            };

                            item_window.DropDownItems.Add(item_);
                        }

                        item_window.DropDownItems.Add(new ToolStripSeparator());

                        {
                            // デバッグウィンドウ

                            var item_ = new ToolStripMenuItem();
                            item_.Text = "デバッグ用のログ(&D)"; // Debug window

                            item_window.DropDownItems.Add(item_);

                            {
                                // デバッグ

                                {
                                    // デバッグウィンドウ

                                    var item1 = new ToolStripMenuItem();
                                    item1.Text = "デバッグウィンドウの表示(&D)"; // Debug Window
                                    item1.ShortcutKeys = Keys.Control | Keys.D;
                                    shortcut.OnKeyDown += (sender, e) => { if (e.Control && e.KeyCode == Keys.D) item1.PerformClick(); };
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

                    // 「ヘルプ」
                    {
                        var item_others = new ToolStripMenuItem();
                        item_others.Text = "ヘルプ(&H)"; // Help
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
                                using (var dialog = new AboutYaneuraOu())
                                {
                                    FormLocationUtility.CenteringToThisForm(dialog, this);
                                    dialog.ShowDialog(this);
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
                                using (var dialog = new SystemInfo())
                                {
                                    FormLocationUtility.CenteringToThisForm(dialog, this);
                                    dialog.ShowDialog(this);
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

                    // メニューのフォントを設定しなおす。
                    FontUtility.ReplaceFont(menu, config.FontManager.MenuStrip);

                    Controls.Add(menu);

                    // フォームのメインメニューとする
                    MainMenuStrip = menu;
                    old_menu = menu;
                    // 次回このメソッドが呼び出された時にthis.Controls.Remove(old_menu)する必要があるので
                    // 記憶しておかないと駄目。
                }
                // レイアウトロジックを再開する
            }

            // 画面の描画が必要になるときがあるので..
            gameScreenControl1.ForceRedraw();
        }

        /// <summary>
        /// 対局盤面の再描画を促す。
        /// </summary>
        /// <param name="args"></param>
        public void ForceRedraw(PropertyChangedEventArgs args)
        {
            gameScreenControl1.ForceRedraw();
        }

        /// <summary>
        /// 前回のメニュー項目。
        /// </summary>
        private MenuStripEx old_menu { get; set; } = null;

#endregion

    }
}
