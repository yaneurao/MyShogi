using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.String;
using MyShogi.Model.Common.Tool;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.View.Win2D.Setting;

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

    }
}
