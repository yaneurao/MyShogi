using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Tool;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.View.Win2D.Setting;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局盤面などがあるメインウィンドゥ
    ///
    /// DockWindowに関する処理だけここに集めてある。
    /// </summary>
    public partial class MainDialog : Form
    {
        /// <summary>
        /// DockWindow関係の初期化
        /// </summary>
        private void InitDocks()
        {
            var config = TheApp.app.Config;

            config.KifuWindowDockManager.AddPropertyChangedHandler("DockState", UpdateKifuWindowDockState);
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

            config.MiniShogiBoardDockManager.AddPropertyChangedHandler("DockState", UpdateMiniShogiBoardDockState);
            config.MiniShogiBoardDockManager.AddPropertyChangedHandler("DockPosition", UpdateMiniShogiBoardDockState);
        }

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
                    dockManager.Size = new Size(Width, Height / 4);

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
        public void ResizeConsiderationControl(PropertyChangedEventArgs args = null)
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

                    DockUtility.Change(gameScreenControl1, DockStyle.None, new Size(w, h - ch), null /*いまの場所*/ );
                    DockUtility.Change(engineConsiderationMainControl, DockStyle.None, new Size(w, ch), new Point(0, ClientSize.Height - ch));
                }
                else
                {
                    DockUtility.Change(gameScreenControl1, DockStyle.Fill, new Size(w, h), null);
                    DockUtility.Change(engineConsiderationMainControl, DockStyle.Fill, null, null);
                }
            }
        }

        /// <summary>
        /// UpdateKifuWindowDockState()のミニ盤面用。
        /// だいたい同じ感じの処理。
        /// </summary>
        private void UpdateMiniShogiBoardDockState(PropertyChangedEventArgs args)
        {
            var dockState = (DockState)args.value;

            var dockManager = TheApp.app.Config.MiniShogiBoardDockManager;
            dockManager.DockState = dockState; // 次回起動時のためにここに保存しておく。

            if (miniShogiBoardDockWindow != null)
            {
                miniShogiBoardDockWindow.RemoveControl();
                miniShogiBoardDockWindow.Dispose();
                miniShogiBoardDockWindow = null;
            }
            engineConsiderationMainControl.RemoveMiniShogiBoard();

            // dockManager.Visibleは反映させないと駄目。
            if (!dockManager.Visible)
            {
                // フォーカス移動されてると困るので戻す。
                this.Focus();
                return;
            }

            if (dockState == DockState.InTheMainWindow)
            {
                engineConsiderationMainControl.AddMiniShogiBoard();
            }
            else
            {
                miniShogiBoardDockWindow = new DockWindow();
                miniShogiBoardDockWindow.ViewModel.AddPropertyChangedHandler("MenuUpdated", _ => UpdateMenuItems());
                miniShogiBoardDockWindow.Owner = this;

                miniShogiBoardDockWindow.ViewModel.Caption = "ミニ盤面";

                // デフォルト位置とサイズにする。
                if (dockManager.Size.IsEmpty)
                {
                    // デフォルトでは、このウインドウサイズに従う
                    dockManager.Size = new Size(Width/4 , Height / 4);

                    //var pos = miniShogi.CalcKifuWindowLocation();
                    //// これクライアント座標なので、スクリーン座標にいったん変換する。
                    //pos = gameScreenControl1.PointToScreen(pos);

                    var pos = new Point(0, 0);
                    dockManager.LocationOnDocked = new Point(pos.X - this.Location.X, pos.Y - this.Location.Y);
                    dockManager.LocationOnFloating = pos;
                }

                // Showで表示とサイズが確定してからdockManagerを設定しないと、
                // Showのときの位置とサイズがdockManagerに記録されてしまう。
                miniShogiBoardDockWindow.Visible = true;

                miniShogiBoardDockWindow.AddControl(miniShogiBoard, this, dockManager);
                dockManager.InitDockWindowLocation(this, miniShogiBoardDockWindow);

                miniShogiBoardDockWindow.Show();
            }
        }

    }
}
