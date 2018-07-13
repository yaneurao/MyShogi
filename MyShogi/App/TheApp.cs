using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Resource.Sounds;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.LocalServer;

// とりま、Windows用
// あとで他環境用を用意する
using MyShogi.View.Win2D;

namespace MyShogi.App
{
    /// <summary>
    /// このアプリケーション
    /// singletonで生成
    /// </summary>
    public class TheApp
    {
        /// <summary>
        /// ここが本アプリのエントリーポイント
        /// </summary>
        public void Run()
        {
#if false
            try
            {
                DevTest();
                Main();
            } catch (Exception ex)
            {
                // これを表示するようにしておくと、開発環境以外で実行した時のデバッグが楽ちん。
                MessageBox.Show("例外が発生しましたので終了します。\r\n例外内容 : " + ex.Message + "\r\nスタックトレース : \r\n" + ex.StackTrace);
            }
#else
            // 開発時に例外がここでcatchされてしまうとデバッグがしにくいので
            // 開発時にはこちらを使う。(といいかも)
            DevTest();
            Main();
#endif
        }

        /// <summary>
        /// 開発時のテストコード
        /// </summary>
        private void DevTest()
        {
            // -- 駒素材画像の変換

            //ImageConverter.ConvertPieceImage();
            //ImageConverter.ConvertBoardNumberImage();

            // -- 各エンジン用の設定ファィルを書き出す。

            //EngineDefineSample.WriteEngineDefineFiles2018();
        }

        /// <summary>
        /// メインの処理。
        /// 
        /// 各インスタンスを生成して、イベントのbindを行い、メインダイアログの実行を開始する。
        /// </summary>
        private void Main()
        {
            // -- global configの読み込み

            config = GlobalConfig.CreateInstance();

            // -- 各インスタンスの生成と、それぞれのbind作業

            // -- 画像の読み込み

            {
                imageManager = new ImageManager();
                imageManager.Update(); // ここでconfigに従い、画像が読み込まれる。

                // GlobalConfigのプロパティ変更に対して、このimageManagerが呼び出されるようにbindしておく。

                config.AddPropertyChangedHandler("BoardImageVersion", imageManager.UpdateBoardImage);
                config.AddPropertyChangedHandler("TatamiImageVersion", imageManager.UpdateBoardImage);
                config.AddPropertyChangedHandler("PieceTableImageVersion", imageManager.UpdateBoardImage);

                config.AddPropertyChangedHandler("PieceImageVersion", imageManager.UpdatePieceImage);
                config.AddPropertyChangedHandler("PieceAttackImageVersion", imageManager.UpdatePieceAttackImage);

                config.AddPropertyChangedHandler("LastMoveFromColorType", imageManager.UpdatePieceMoveImage);
                config.AddPropertyChangedHandler("LastMoveToColorType", imageManager.UpdatePieceMoveImage);
                config.AddPropertyChangedHandler("PickedMoveFromColorType", imageManager.UpdatePieceMoveImage);
                config.AddPropertyChangedHandler("PickedMoveToColorType", imageManager.UpdatePieceMoveImage);

                config.AddPropertyChangedHandler("BoardNumberImageVersion", imageManager.UpdateBoardNumberImage);
            }

            // -- メインの対局ウィンドゥ

            var mainDialog = new MainDialog();
            mainForm = mainDialog;

            // -- 対局controllerを1つ生成して、メインの対局ウィンドゥのViewModelに加える

            var gameServer = new LocalGameServer();
            mainDialog.Init(gameServer);

            // 盤・駒が変更されたときにMainDialogのメニューの内容を修正しないといけないので更新がかかるようにしておく。

            config.AddPropertyChangedHandler("BoardImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("TatamiImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("PieceImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("PromotePieceColorType", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("PieceAttackImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("BoardNumberImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("LastMoveFromColorType", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("LastMoveToColorType", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("PickedMoveFromColorType", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("PickedMoveToColorType", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("TurnDisplay", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("PieceSoundInTheGame", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("KifuReadOut", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("ReadOutSenteGoteEverytime", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("MemoryLoggingEnable", mainDialog.UpdateMenuItems, mainDialog);
            config.AddPropertyChangedHandler("FileLoggingEnable", mainDialog.UpdateMenuItems, mainDialog);

            // -- ロギング用のハンドラをセット

            var MemoryLoggingEnableHandler = new PropertyChangedEventHandler((args) =>
            {
                if (config.MemoryLoggingEnable)
                    Log.log1 = new MemoryLog();
                else
                {
                    if (Log.log1 != null)
                        Log.log1.Dispose();
                    Log.log1 = null;
                }
            });
            var FileLoggingEnable = new PropertyChangedEventHandler((args) =>
            {
                if (config.FileLoggingEnable)
                {
                    var now = DateTime.Now;
                    Log.log2 = new FileLog($"log{now.ToString("yyyyMMddHHmm")}.txt");
                }
                else
                {
                    if (Log.log2 != null)
                        Log.log2.Dispose();
                    Log.log2 = null;
                }
            });

            config.AddPropertyChangedHandler("MemoryLoggingEnable", MemoryLoggingEnableHandler);
            config.AddPropertyChangedHandler("FileLoggingEnable", FileLoggingEnable);

            // 上のハンドラを呼び出して、必要ならばロギングを開始しておいてやる。
            MemoryLoggingEnableHandler(null);
            FileLoggingEnable(null);

            // 初期化が終わったのでgameServerの起動を行う。
            gameServer.Start();

            // サウンド
            soundManager = new SoundManager();
            soundManager.Start();

            // 終了するときに設定ファイルに書き出すコード
            Application.ApplicationExit += new EventHandler((sender, e) =>
            {
                // メインウィンドウと検討ウィンドウに関して、
                // 終了時のウィンドウサイズを記憶しておき、次回起動時にこのサイズでウィンドウを生成する。
                if (mainDialog.ClientSize.Width >= 100 && mainDialog.ClientSize.Height >= 100)
                    config.MainDialogClientSize = mainDialog.ClientSize;
                if (mainDialog.engineConsiderationDialog != null &&
                    mainDialog.engineConsiderationDialog.Width >= 100 && mainDialog.engineConsiderationDialog.Height >= 100)
                {
                    config.ConsiderationDialogClientSize = mainDialog.engineConsiderationDialog.ClientSize;
                    config.ConsiderationDialogClientLocation =
                        new Point(
                            mainDialog.engineConsiderationDialog.Location.X - mainDialog.Location.X,
                            mainDialog.engineConsiderationDialog.Location.Y - mainDialog.Location.Y
                        );
                }

                config.Save();

                if (engine_config != null)
                    engine_config.Save();

                soundManager.Dispose();

                // 起動しているGameServerすべてを終了させる必要がある。(エンジンを停止させるため)
                if (gameServer != null)
                    gameServer.Dispose();
            });

            // -- メインダイアログを生成して、アプリの開始

            Application.Run(mainDialog);
        }

        /// <summary>
        /// 最前面に来るようにしてMessageBox.Show(text)を呼び出す。
        /// </summary>
        /// <param name="text"></param>
        public void MessageShow(string text)
        {
            if (mainForm != null && mainForm.IsHandleCreated && !mainForm.IsDisposed)
            {
                if (mainForm.InvokeRequired)
                    mainForm.Invoke(new Action(() => { MessageBox.Show(mainForm, text); }));
                else
                    MessageBox.Show(mainForm, text);
            }
            else
                MessageBox.Show(text);
        }

        /// <summary>
        /// 最前面に来るようにしてMessageBox.Show(text,caption)を呼び出す。
        /// </summary>
        public void MessageShow(string text, string caption)
        {
            if (mainForm != null)
                MessageBox.Show(mainForm, text, caption);
            else
                MessageBox.Show(text, caption);
        }

        // -- それぞれのViewModel
        // 他のViewModelにアクションが必要な場合は、これを経由して通知などを行えば良い。
        // 他のViewに直接アクションを起こすことは出来ない。必ずViewModelに通知などを行い、
        // そのViewModelのpropertyをsubscribeしているViewに間接的に通知が行くという仕組みを取る。

        /// <summary>
        /// 画像の読み込み用。本GUIで用いる画像はすべてここから取得する。
        /// </summary>
        public ImageManager imageManager { get; private set; }

        /// <summary>
        /// GUIの全体設定
        /// </summary>
        public GlobalConfig config { get; private set; }

        /// <summary>
        /// エンジン設定(最初のアクセスの時に読み込む。遅延読み込み。)
        /// </summary>
        public List<EngineDefineEx> EngineDefines
        {
            get {
                lock (this)
                {
                    if (engine_defines == null)
                        engine_defines = EngineDefineUtility.GetEngineDefines();
                    return engine_defines;
                }
            }
        }
        private List<EngineDefineEx> engine_defines;

        /// <summary>
        /// [UI Thread] : EngineConfigを返す。
        /// (エンジンのオプションの共通設定、個別設定が格納されている。)
        /// </summary>
        public EngineConfig EngineConfig
        {
            get
            {
                lock (this)
                {
                    /// 遅延読み込み。
                    if (engine_config == null)
                        engine_config = EngineConfigUtility.GetEngineConfig();
                    return engine_config;
                }
            }
        }
        private EngineConfig engine_config;

    /// <summary>
    /// サウンドマネージャー
    /// </summary>
    public SoundManager soundManager { get; private set; }

        /// <summary>
        /// メインのForm
        /// これがないとMessageBox.Show()などで親を指定できなくて困る。
        /// </summary>
        public Form mainForm { get; private set; }

        /// <summary>
        /// singletonなinstance。それぞれのViewModelなどにアクセスしたければ、これ経由でアクセスする。
        /// </summary>
        public static TheApp app = new TheApp();
    }
}
