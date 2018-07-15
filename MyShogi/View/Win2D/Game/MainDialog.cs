using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.Model.Shogi.Usi;
using MyShogi.Model.Test;
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

        /// <summary>
        /// 「やねうら王について」のダイアログ
        /// </summary>
        public Form aboutDialog;

        /// <summary>
        /// 「通常対局」の設定ダイアログ
        /// </summary>
        public Form gameSettingDialog;

        /// <summary>
        /// CPU infoを表示するダイアログ
        /// </summary>
        public Form cpuInfoDialog;

        /// <summary>
        /// デバッグウィンドウ
        /// </summary>
        public Form debugDialog;

        /// <summary>
        /// エンジンの思考出力用
        /// </summary>
        public EngineConsiderationDialog engineConsiderationDialog;

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
        }

        private void MainDialog_Resize(object sender, System.EventArgs e)
        {
            UpdateEngineConsiderationDialogLocation();
        }

        /// <summary>
        /// ウィンドウを移動させたときなどに、そこの左下に検討ウィンドウを追随させる。
        /// </summary>
        private void UpdateEngineConsiderationDialogLocation()
        {
            if (engineConsiderationDialog != null)
            {
                var loc = Location;
                engineConsiderationDialog.Location =
                    new Point(loc.X, loc.Y + Height);
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
            var size = TheApp.app.config.MainDialogClientSize;
            if (size.Width < 192 || size.Height < 108)
                size = Size.Empty;

            if (size.IsEmpty)
            {
                // ディスプレイに収まるサイズのスクリーンにする必要がある。
                // プライマリスクリーンを基準にして良いのかどうかはわからん…。
                int w = Screen.PrimaryScreen.Bounds.Width;
                int h = Screen.PrimaryScreen.Bounds.Height - menu_height;

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
            } else
            {
                ClientSize = size;
            }

            MinimumSize = new Size(192 * 2, 108 * 2 + menu_height);
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

            if (engineConsiderationDialog == null)
            {
                var dialog = new EngineConsiderationDialog();
                dialog.Init(gameServer.BoardReverse /* これ引き継ぐ。以降は知らん。*/);
                // ウィンドウ幅を合わせておく。

                // 前回起動時のサイズが記録されているならそれを復元してやる。
                var size = TheApp.app.config.ConsiderationDialogClientSize;
                if (size.Width < 192 || size.Height < 108)
                    size = Size.Empty;
                if (size.IsEmpty)
                    size = new Size(Width, (int)(Width * 0.2)); /* メインウィンドウの20%ぐらいの高さ */
                dialog.Size = size;
                dialog.Show(/*this*/);
                // 検討ウィンドウはClosingイベントをキャンセルして非表示にしているのでメインウインドウにぶら下げると
                // アプリを終了できなくなってしまう。また、メインウインドウを動かした時に検討ウィンドウは自動追随するので
                // 現状、普通に使用していてメインウインドウで検討ウィンドウが隠れることはないため、これで良しとする。

                var offset = TheApp.app.config.ConsiderationDialogClientLocation;
                if (offset.IsEmpty)
                    dialog.Location = new Point(Location.X, Location.Y + Height);
                else
                    dialog.Location = new Point(Location.X + offset.X, Location.Y + offset.Y);

                dialog.Visible = false;

                dialog.ConsiderationInstance(0).Notify.AddPropertyChangedHandler("MultiPV", (h) =>
                 { gameServer.ChangeMultiPvCommand((int)h.value); });

                engineConsiderationDialog = dialog;
                // 何にせよ、インスタンスがなくては話にならないので生成だけしておく。
            } else
            {
                // 検討ウィンドウが非表示になっていたら、PVのメッセージ無視していいや…。
                // (処理に時間かかるし…)
                if (!engineConsiderationDialog.Visible && message.type == UsiEngineReportMessageType.UsiThinkReport)
                        return;
            }

            engineConsiderationDialog.DispatchThinkReportMessage(message);
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
            var consideration = gameServer.GameMode == GameModeEnum.ConsiderationWithEngine;
            gameServer.ChangeGameModeCommand(
                consideration ?
                GameModeEnum.ConsiderationWithoutEngine :
                GameModeEnum.ConsiderationWithEngine
            );
        }

        /// <summary>
        /// 「解」ボタン。棋譜解析。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, System.EventArgs e)
        {

        }

        /// <summary>
        /// 「詰」ボタン。詰みがあるかどうかを調べる。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton7_Click(object sender, System.EventArgs e)
        {
            var mate_consideration = gameServer.GameMode == GameModeEnum.ConsiderationWithMateEngine;
            gameServer.ChangeGameModeCommand(
                mate_consideration ?
                GameModeEnum.ConsiderationWithoutEngine :
                GameModeEnum.ConsiderationWithMateEngine
            );
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

        #endregion

        #region update menu

        /// <summary>
        /// 棋譜の上書き保存のために、前回保存したときの名前を保持しておく。
        /// </summary>
        private string lastFileName;

        /// <summary>
        /// [UI thread] : メニューのitemを動的に追加する。
        /// 商用版とフリーウェア版とでメニューが異なるのでここで動的に追加する必要がある。
        /// </summary>
        public void UpdateMenuItems(ObjectModel.PropertyChangedEventArgs args = null)
        {
            // 頑張れば高速化出来るが、対局中はこのメソッド呼び出されていないし、
            // ToolStripも、CPU×CPUの対局中は更新は発生していないし、
            // CPU×人間のときは多少遅くても誤差だし、まあいいか…。

            var config = TheApp.app.config;

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

                // 検討モード(通常エンジン)
                var consideration = gameServer == null ? false : gameServer.GameMode == GameModeEnum.ConsiderationWithEngine;
                // 検討モード(詰将棋用)
                var mate_consideration = gameServer == null ? false : gameServer.GameMode == GameModeEnum.ConsiderationWithMateEngine;
                // 対局中
                var inTheGame = gameServer == null ? false : gameServer.InTheGame;
                // 盤面編集中
                var inTheBoardEdit = gameServer == null ? false : gameServer.GameMode == GameModeEnum.InTheBoardEdit;
                // 盤面反転
                var boardReverse = gameServer == null ? false : gameServer.BoardReverse;


                var item_file = new ToolStripMenuItem();
                item_file.Text = "ファイル";
                menu.Items.Add(item_file);

                // 対局中は、ファイルメニュー項目は丸ごと無効化
                item_file.Enabled = !inTheGame;

                // -- 「ファイル」配下のメニュー
                {
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜を開く";
                        item.Click += (sender, e) =>
                        {
                            var fd = new OpenFileDialog();

                                //[ファイルの種類]に表示される選択肢を指定する
                                //指定しないとすべてのファイルが表示される
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
                                //ダイアログを表示する
                                if (fd.ShowDialog() == DialogResult.OK)
                            {
                                var filename = fd.FileName;
                                try
                                {
                                    var kifu_text = FileIO.ReadText(filename);
                                    gameServer.KifuReadCommand(kifu_text);
                                    lastFileName = filename; // 最後に開いたファイルを記録しておく。
                                    }
                                catch
                                {
                                    TheApp.app.MessageShow("ファイル読み込みエラー");
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜の上書き保存";
                        item.Click += (sender, e) =>
                        {
                            try
                            {
                                    // 「開く」もしくは「名前をつけて保存無したファイルに上書きする。
                                    // 「局面の保存」は棋譜ではないのでこれは無視する。
                                    // ファイル形式は、拡張子から自動判別する。
                                    gameServer.KifuWriteCommand(lastFileName,
                                    KifuFileTypeExtensions.StringToKifuFileType(lastFileName));
                            }
                            catch
                            {
                                TheApp.app.MessageShow("ファイル書き出しエラー");
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜の名前をつけて保存";
                        item.Click += (sender, e) =>
                        {
                            var fd = new SaveFileDialog();

                                //[ファイルの種類]に表示される選択肢を指定する
                                //指定しないとすべてのファイルが表示される
                                fd.Filter = "KIF形式(*.KIF)|*.KIF|KIF2形式(*.KI2)|*.KI2|CSA形式(*.CSA)|*.CSA"
                                + "|PSN形式(*.PSN)|*.PSN|PSN2形式(*.PSN2)|*.PSN2"
                                + "|SFEN形式(*.SFEN)|*.SFEN|すべてのファイル(*.*)|*.*";
                            fd.FilterIndex = 1;
                            fd.Title = "棋譜を保存するファイル形式を選択してください";
                                //ダイアログを表示する
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
                                    lastFileName = filename; // 最後に保存したファイルを記録しておく。
                                    }
                                catch
                                {
                                    TheApp.app.MessageShow("ファイル書き出しエラー");
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "局面の保存";
                        item.Click += (sender, e) =>
                        {
                            var fd = new SaveFileDialog();

                                //[ファイルの種類]に表示される選択肢を指定する
                                //指定しないとすべてのファイルが表示される
                                fd.Filter = "KIF形式(*.KIF)|*.KIF|KIF2形式(*.KI2)|*.KI2|CSA形式(*.CSA)|*.CSA"
                                + "|PSN形式(*.PSN)|*.PSN|PSN2形式(*.PSN2)|*.PSN2"
                                + "|SFEN形式(*.SFEN)|*.SFEN|すべてのファイル(*.*)|*.*";
                            fd.FilterIndex = 1;
                            fd.Title = "局面を保存するファイル形式を選択してください";
                                //ダイアログを表示する
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

                                    gameServer.PositionWriteCommand(filename, kifuType);
                                }
                                catch
                                {
                                    TheApp.app.MessageShow("ファイル書き出しエラー");
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    item_file.DropDownItems.Add(new ToolStripSeparator());

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "終了";
                        item.Click += (sender, e) => { TheApp.app.ApplicationExit(); };
                        item_file.DropDownItems.Add(item);
                    }

                }

                var item_playgame = new ToolStripMenuItem();
                item_playgame.Text = "対局";
                menu.Items.Add(item_playgame);

                // -- 「対局」配下のメニュー
                {
                    { // -- 通常対局
                        var item = new ToolStripMenuItem();
                        item.Text = "通常対局";
                        item.Click += (sender, e) =>
                        {
                            // ShowDialog()はリソースが開放されないので、都度生成して、Form.Show()で表示する。
                            if (gameSettingDialog != null)
                            gameSettingDialog.Dispose();

                            gameSettingDialog = new GameSettingDialog(this);
                            gameSettingDialog.Show(this);
                        };

                        item_playgame.DropDownItems.Add(item);
                    }

                    item_playgame.DropDownItems.Add(new ToolStripSeparator());

                    { // -- 検討モード

                        var item = new ToolStripMenuItem();
                        item.Text = consideration ? "検討モードを終了する" : "検討モード";

                        // toolStripのボタンのテキストを検討モードであるかどうかにより変更する。
                        toolStripButton5.Text = consideration ? "終" : "検";
                        toolStripButton5.ToolTipText = consideration ? "検討モードを終了します。" : "検討モードに入ります。";
                        toolStripButton5.Enabled = !inTheGame;
                        item.Click += (sender, e) => { toolStripButton5_Click(null, null); };

                        item_playgame.DropDownItems.Add(item);
                    }


                    // 「解」ボタン : 棋譜解析
                    toolStripButton6.Enabled = !inTheGame;

                    { // -- 検討モード

                        var item = new ToolStripMenuItem();
                        item.Text = mate_consideration ? "詰検討モードを終了する" : "詰検討モード";

                        // toolStripのボタンのテキストを検討モードであるかどうかにより変更する。
                        toolStripButton7.Text = mate_consideration ? "終" : "詰";
                        toolStripButton7.ToolTipText = mate_consideration ? "詰検討モードを終了します。" : "詰検討モードに入ります。";
                        // 「詰」ボタン : 詰将棋ボタン
                        toolStripButton7.Enabled = !inTheGame;
                        item.Click += (sender, e) => { toolStripButton7_Click(null, null); };

                        item_playgame.DropDownItems.Add(item);
                    }
                }


                var item_display = new ToolStripMenuItem();
                item_display.Text = "表示";
                menu.Items.Add(item_display);

                // -- 「表示」配下のメニュー
                {
                    { // -- 盤面反転
                        var item = new ToolStripMenuItem();
                        item.Text = "盤面反転";
                        item.Checked = boardReverse;
                        item.Click += (sender, e) => { gameServer.BoardReverse ^= true; };

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 段・筋の画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "筋・段の表示";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "非表示";
                        item1.Checked = config.BoardNumberImageVersion == 0;
                        item1.Click += (sender, e) => { config.BoardNumberImageVersion = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "標準";
                        item2.Checked = TheApp.app.config.BoardNumberImageVersion == 1;
                        item2.Click += (sender, e) => { config.BoardNumberImageVersion = 1; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "Chess式";
                        item3.Checked = TheApp.app.config.BoardNumberImageVersion == 2;
                        item3.Click += (sender, e) => { config.BoardNumberImageVersion = 2; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 盤画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "盤画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "白色";
                        item1.Checked = config.BoardImageVersion == 1;
                        item1.Click += (sender, e) => { config.BoardImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "黄色";
                        item2.Checked = config.BoardImageVersion == 2;
                        item2.Click += (sender, e) => { config.BoardImageVersion = 2; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 盤画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "畳画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "薄い";
                        item1.Checked = config.TatamiImageVersion == 1;
                        item1.Click += (sender, e) => { config.TatamiImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "濃い";
                        item2.Checked = config.TatamiImageVersion == 2;
                        item2.Click += (sender, e) => { config.TatamiImageVersion = 2; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 駒画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "駒画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "一文字駒";
                        item1.Checked = config.PieceImageVersion == 2;
                        item1.Click += (sender, e) => { config.PieceImageVersion = 2; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "二文字駒";
                        item2.Checked = TheApp.app.config.PieceImageVersion == 1;
                        item2.Click += (sender, e) => { config.PieceImageVersion = 1; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "英文字駒";
                        item3.Checked = TheApp.app.config.PieceImageVersion == 3;
                        item3.Click += (sender, e) => { config.PieceImageVersion = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 成駒の画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "成駒の色";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "黒";
                        item1.Checked = config.PromotePieceColorType == 0;
                        item1.Click += (sender, e) => { config.PromotePieceColorType = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "赤";
                        item2.Checked = TheApp.app.config.PromotePieceColorType == 1;
                        item2.Click += (sender, e) => { config.PromotePieceColorType = 1; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    // -- 最終手のエフェクト
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "最終手の移動元";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし";
                        item0.Checked = config.LastMoveFromColorType == 0;
                        item0.Click += (sender, e) => { config.LastMoveFromColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "朱色";
                        item1.Checked = config.LastMoveFromColorType == 1;
                        item1.Click += (sender, e) => { config.LastMoveFromColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色";
                        item2.Checked = TheApp.app.config.LastMoveFromColorType == 2;
                        item2.Click += (sender, e) => { config.LastMoveFromColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色";
                        item3.Checked = TheApp.app.config.LastMoveFromColorType == 3;
                        item3.Click += (sender, e) => { config.LastMoveFromColorType = 3; };
                        item.DropDownItems.Add(item3);

#if false
                        var item4 = new ToolStripMenuItem();
                        item4.Text = "駒の影のみ";
                        item4.Checked = TheApp.app.config.LastMoveFromColorType == 4;
                        item4.Click += (sender, e) => { config.LastMoveFromColorType = 4; };
                        item.DropDownItems.Add(item4);
#endif

                        item_display.DropDownItems.Add(item);
                    }
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "最終手の移動先";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし";
                        item0.Checked = config.LastMoveToColorType == 0;
                        item0.Click += (sender, e) => { config.LastMoveToColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "朱色";
                        item1.Checked = config.LastMoveToColorType == 1;
                        item1.Click += (sender, e) => { config.LastMoveToColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色";
                        item2.Checked = TheApp.app.config.LastMoveToColorType == 2;
                        item2.Click += (sender, e) => { config.LastMoveToColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色";
                        item3.Checked = TheApp.app.config.LastMoveToColorType == 3;
                        item3.Click += (sender, e) => { config.LastMoveToColorType = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "駒を掴んだ時の移動元";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし";
                        item0.Checked = config.PickedMoveFromColorType == 0;
                        item0.Click += (sender, e) => { config.PickedMoveFromColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "黄色";
                        item1.Checked = config.PickedMoveFromColorType == 1;
                        item1.Click += (sender, e) => { config.PickedMoveFromColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色";
                        item2.Checked = TheApp.app.config.PickedMoveFromColorType == 2;
                        item2.Click += (sender, e) => { config.PickedMoveFromColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色";
                        item3.Checked = TheApp.app.config.PickedMoveFromColorType == 3;
                        item3.Click += (sender, e) => { config.PickedMoveFromColorType = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "駒を掴んだ時の移動候補";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "エフェクトなし";
                        item0.Checked = config.PickedMoveToColorType == 0;
                        item0.Click += (sender, e) => { config.PickedMoveToColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "移動できない升を少し暗くする";
                        item1.Checked = config.PickedMoveToColorType == 1;
                        item1.Click += (sender, e) => { config.PickedMoveToColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "移動できない升を暗くする";
                        item2.Checked = TheApp.app.config.PickedMoveToColorType == 2;
                        item2.Click += (sender, e) => { config.PickedMoveToColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "移動できない升をかなり暗くする";
                        item3.Checked = TheApp.app.config.PickedMoveToColorType == 3;
                        item3.Click += (sender, e) => { config.PickedMoveToColorType = 3; };
                        item.DropDownItems.Add(item3);

                        var item4 = new ToolStripMenuItem();
                        item4.Text = "移動できる升を少し明るくする";
                        item4.Checked = TheApp.app.config.PickedMoveToColorType == 4;
                        item4.Click += (sender, e) => { config.PickedMoveToColorType = 4; };
                        item.DropDownItems.Add(item4);

                        var item5 = new ToolStripMenuItem();
                        item5.Text = "移動できる升を明るくする";
                        item5.Checked = TheApp.app.config.PickedMoveToColorType == 5;
                        item5.Click += (sender, e) => { config.PickedMoveToColorType = 5; };
                        item.DropDownItems.Add(item5);

#if false
                        var item6 = new ToolStripMenuItem();
                        item6.Text = "駒の影のみ";
                        item6.Checked = TheApp.app.config.PickedMoveToColorType == 6;
                        item6.Click += (sender, e) => { config.PickedMoveToColorType = 6; };
                        item.DropDownItems.Add(item6);
#endif

                        item_display.DropDownItems.Add(item);
                    }

                    // 駒の移動方向
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "移動方角マーカー";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし";
                        item0.Checked = config.PieceAttackImageVersion == 0;
                        item0.Click += (sender, e) => { config.PieceAttackImageVersion = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "あり";
                        item1.Checked = config.PieceAttackImageVersion == 1;
                        item1.Click += (sender, e) => { config.PieceAttackImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 手番プレートの表示

                        var item = new ToolStripMenuItem();
                        item.Text = "手番表示";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "なし";
                        item1.Checked = config.TurnDisplay == 0;
                        item1.Click += (sender, e) => { config.TurnDisplay = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "あり";
                        item2.Checked = TheApp.app.config.TurnDisplay == 1;
                        item2.Click += (sender, e) => { config.TurnDisplay = 1; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                }

                // 「音声」
                {
                    var item_sounds = new ToolStripMenuItem();
                    item_sounds.Text = "音声";
                    menu.Items.Add(item_sounds);

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "対局時の駒音";
                        item1.Checked = TheApp.app.config.PieceSoundInTheGame == 1;
                        item1.Click += (sender, e) => { TheApp.app.config.PieceSoundInTheGame ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

#if false
                        // あまりいい効果音作れなかったのでコメントアウトしとく。
                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "王手などの駒音を衝撃音に";
                        item1.Checked = TheApp.app.config.CrashPieceSoundInTheGame == 1;
                        item1.Click += (sender, e) => { TheApp.app.config.CrashPieceSoundInTheGame ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }
#endif

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "棋譜読み上げ";
                        item1.Checked = TheApp.app.config.KifuReadOut == 1;
                        item1.Enabled = TheApp.app.config.CommercialVersion != 0; // 商用版のみ選択可
                        item1.Click += (sender, e) => { TheApp.app.config.KifuReadOut ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "「先手」「後手」を毎回読み上げる";
                        item1.Checked = TheApp.app.config.ReadOutSenteGoteEverytime == 1;
                        item1.Enabled = TheApp.app.config.CommercialVersion != 0; // 商用版のみ選択可
                        item1.Click += (sender, e) => { TheApp.app.config.ReadOutSenteGoteEverytime ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

                }

                var item_boardedit = new ToolStripMenuItem();
                item_boardedit.Text = "盤面編集";
                item_boardedit.Enabled = !inTheGame;
                menu.Items.Add(item_boardedit);

                // 盤面編集の追加
                {
                    {   // -- 盤面編集の開始
                        var item = new ToolStripMenuItem();
                        item.Text = inTheBoardEdit ? "盤面編集の終了" : "盤面編集の開始";
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
                        item.Text = "手番の変更";
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
                        item.Text = "平手の初期局面配置";
                        item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.NoHandicap.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 駒落ちの局面
                        var item_handicap = new ToolStripMenuItem();
                        item_handicap.Enabled = inTheBoardEdit;
                        item_handicap.Text = "駒落ち初期局面配置";
                        item_boardedit.DropDownItems.Add(item_handicap);

                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "香落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "右香落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapRightKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "角落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapKaku.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "飛車落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapHisya.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "飛香落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapHisyaKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "二枚落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap2.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "三枚落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap3.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "四枚落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap4.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "五枚落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap5.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "左五枚落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HandicapLeft5.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "六枚落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap6.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "八枚落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap8.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "十枚落ち";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Handicap10.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "歩三枚";
                            item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.HANDICAP_PAWN3.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }

                    }

                    {   // -- 詰将棋用の配置(駒箱に)
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "詰将棋用に配置";
                        item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Mate1.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 双玉詰将棋用の局面
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "双玉詰将棋用に配置";
                        item.Click += (sender, e) => { gameServer.SetSfenCommand(BoardType.Mate2.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                }

                var item_window = new ToolStripMenuItem();
                item_window.Text = "ウインドウ";
                menu.Items.Add(item_window);

                // -- 「ウインドウ」配下のメニュー
                {
                    { // ×ボタンで消していた検討ウィンドウの復活

                        var item = new ToolStripMenuItem();
                        item.Text = "検討ウィンドウの表示";
                        item.Click += (sender, e) =>
                        {
                            if (engineConsiderationDialog != null)
                            {
                                if (!engineConsiderationDialog.Visible)
                                    engineConsiderationDialog.Visible = true;
                            }
                        };
                        item_window.DropDownItems.Add(item);
                    }
                }

                // 「情報」
                {
                    var item_others = new ToolStripMenuItem();
                item_others.Text = "情報";
                menu.Items.Add(item_others);

                    {
                        // メモリへのロギング

                        var item1 = new ToolStripMenuItem();
                        item1.Text = TheApp.app.config.MemoryLoggingEnable ? "デバッグ終了" : "デバッグ開始";
                        item1.Checked = TheApp.app.config.MemoryLoggingEnable;
                        item1.Click += (sender, e) =>
                        {
                            TheApp.app.config.MemoryLoggingEnable ^= true;
                            if (!TheApp.app.config.MemoryLoggingEnable && debugDialog != null)
                            {
                                debugDialog.Dispose(); // 終了させておく。
                                debugDialog = null;
                            }
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    {
                        // デバッグウィンドウ

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "デバッグウィンドウ";
                        item1.Enabled = TheApp.app.config.MemoryLoggingEnable;
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
                                debugDialog.Show();
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    {
                        // ファイルへのロギング

                        var item1 = new ToolStripMenuItem();
                        item1.Text = TheApp.app.config.FileLoggingEnable ? "ロギング終了" : "ロギング開始";
                        item1.Checked = TheApp.app.config.FileLoggingEnable;
                        item1.Click += (sender, e) => { TheApp.app.config.FileLoggingEnable ^= true; };
                        item_others.DropDownItems.Add(item1);
                    }

                    item_others.DropDownItems.Add(new ToolStripSeparator());

                    {
                        // システム情報ダイアログ

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "システム情報";
                        item1.Click += (sender, e) =>
                        {
                            if (cpuInfoDialog != null)
                                cpuInfoDialog.Dispose();

                            cpuInfoDialog = new SystemInfo();
                            cpuInfoDialog.Show(this);
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    item_others.DropDownItems.Add(new ToolStripSeparator());

                    {
                        // aboutダイアログ

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "バージョン情報";
                        item1.Click += (sender, e) =>
                        {
                            if (aboutDialog != null)
                                aboutDialog.Dispose();

                            aboutDialog = new AboutYaneuraOu();
                            aboutDialog.Show(this);
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "アップデートの確認";
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

#if DEBUG

                // デバッグ用にメニューにテストコードを実行する項目を追加する。
                {
                    var item_debug = new ToolStripMenuItem();
                    item_debug.Text = "デバッグ";

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
                //フォームのメインメニューとする
                MainMenuStrip = menu;
                old_menu = menu;

                //レイアウトロジックを再開する
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
