using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Resource.Sounds;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.LocalServer;

// とりま、Windows用
// あとで他環境用を用意する(かも)
using MyShogi.View.Win2D;

namespace MyShogi.App
{
    /// <summary>
    /// このアプリケーション
    /// singletonで生成
    /// </summary>
    public partial class TheApp
    {
        #region main
        /// <summary>
        /// ここが本アプリのエントリーポイント
        /// </summary>
        public void Run()
        {
#if true // is beta

            // -- リリース時
            try
            {
                DevTest();
                Main();
            } catch (Exception ex)
            {
                // これを表示するようにしておくと、開発環境以外で実行した時のデバッグが楽ちん。
                MessageShow(ex);
            }
#else
            // -- 開発(デバッグ)時

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

            Config = GlobalConfig.CreateInstance();

            // -- 各インスタンスの生成と、それぞれのbind作業

            // -- 画像の読み込み

            {
                ImageManager = new ImageManager();
                ImageManager.Update(); // ここでconfigに従い、画像が読み込まれる。

                // GlobalConfigのプロパティ変更に対して、このimageManagerが呼び出されるようにbindしておく。

                Config.AddPropertyChangedHandler("BoardImageVersion", ImageManager.UpdateBoardImage);
                Config.AddPropertyChangedHandler("TatamiImageVersion", ImageManager.UpdateBoardImage);
                Config.AddPropertyChangedHandler("PieceTableImageVersion", ImageManager.UpdateBoardImage);

                Config.AddPropertyChangedHandler("PieceImageVersion", ImageManager.UpdatePieceImage);
                Config.AddPropertyChangedHandler("PieceAttackImageVersion", ImageManager.UpdatePieceAttackImage);

                Config.AddPropertyChangedHandler("LastMoveFromColorType", ImageManager.UpdatePieceMoveImage);
                Config.AddPropertyChangedHandler("LastMoveToColorType", ImageManager.UpdatePieceMoveImage);
                Config.AddPropertyChangedHandler("PickedMoveFromColorType", ImageManager.UpdatePieceMoveImage);
                Config.AddPropertyChangedHandler("PickedMoveToColorType", ImageManager.UpdatePieceMoveImage);

                Config.AddPropertyChangedHandler("BoardNumberImageVersion", ImageManager.UpdateBoardNumberImage);
            }

            // -- メインの対局ウィンドゥ

            var mainDialog = new MainDialog();
            mainForm = mainDialog;

            // -- 対局controllerを1つ生成して、メインの対局ウィンドゥのViewModelに加える

            var gameServer = new LocalGameServer();
            mainDialog.Init(gameServer);

            // 盤・駒が変更されたときにMainDialogのメニューの内容を修正しないといけないので更新がかかるようにしておく。

            Config.AddPropertyChangedHandler("BoardImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("TatamiImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("PieceImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("PromotePieceColorType", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("PieceAttackImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("BoardNumberImageVersion", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("LastMoveFromColorType", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("LastMoveToColorType", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("PickedMoveFromColorType", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("PickedMoveToColorType", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("TurnDisplay", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("PieceSoundInTheGame", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("PieceSoundOffTheGame", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("KifuReadOut", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("ReadOutSenteGoteEverytime", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("MemoryLoggingEnable", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("FileLoggingEnable", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("NegateEvalWhenWhite", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("KifuWindowWidthType", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("ConsiderationWindowFollowMainWindow", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("ReadOutCancelWhenGameEnd", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("KifuWindowKifuVersion", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("ConsiderationWindowKifuVersion", mainDialog.UpdateMenuItems, mainDialog);
            Config.AddPropertyChangedHandler("KifuWindowWidthType", mainDialog.ResizeKifuControl, mainDialog);


            // -- ロギング用のハンドラをセット

            // メモリ上でのロギング
            Log.log1 = new MemoryLog();

            var FileLoggingEnable = new PropertyChangedEventHandler((args) =>
            {
                if (Config.FileLoggingEnable)
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

            Config.AddPropertyChangedHandler("FileLoggingEnable", FileLoggingEnable);

            // 上のハンドラを呼び出して、必要ならばロギングを開始しておいてやる。
            FileLoggingEnable(null);


            // 初期化が終わったのでgameServerの起動を行う。
            gameServer.Start();

            // サウンド
            SoundManager = new SoundManager();
            SoundManager.Start();

            // 終了するときに設定ファイルに書き出すコード
            Application.ApplicationExit += new EventHandler((sender, e) =>
            {
                // メインウィンドウのサイズを保存
                SaveMainDialogSize();

                // 設定ファイルの保存
                SaveConfig();

                // サウンドマネージャーの停止
                SoundManager.Dispose();

                // 起動しているGameServerすべてを明示的に終了させる必要がある。(そこにぶら下がっているエンジンを停止させるため)
                if (gameServer != null)
                    gameServer.Dispose();
            });

            // -- メインダイアログを生成して、アプリの開始

            Application.Run(mainDialog);
        }
        #endregion

        #region properties

        // -- それぞれのViewModel
        // 他のViewModelにアクションが必要な場合は、これを経由して通知などを行えば良い。
        // 他のViewに直接アクションを起こすことは出来ない。必ずViewModelに通知などを行い、
        // そのViewModelのpropertyをsubscribeしているViewに間接的に通知が行くという仕組みを取る。

        /// <summary>
        /// 画像の読み込み用。本GUIで用いる画像はすべてここから取得する。
        /// </summary>
        public ImageManager ImageManager { get; private set; }

        /// <summary>
        /// GUIの全体設定
        /// </summary>
        public GlobalConfig Config { get; private set; }

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
        public EngineConfigs EngineConfigs
        {
            get
            {
                lock (this)
                {
                    /// 遅延読み込み。
                    if (engine_configs == null)
                        engine_configs = EngineConfigUtility.GetEngineConfig();
                    return engine_configs;
                }
            }
        }
        private EngineConfigs engine_configs;

        /// <summary>
        /// サウンドマネージャー
        /// </summary>
        public SoundManager SoundManager { get; private set; }

        /// <summary>
        /// Visual Studioのデザインモードであるかの判定。
        /// デザインモードだとconfigが未代入なのでnullのはずであるから…。
        ///
        /// Form.DesignModeは、Formのコンストラクタでは未代入であるので使えない。
        /// こういう方法に頼らざるを得ない。Formクラスの設計ミスであるように思う。
        /// </summary>
        public bool DesignMode { get { return Config == null; } }

        /// <summary>
        /// 終了時にエンジンオプションの設定ファイルを消すフラグ
        /// </summary>
        public bool DeleteEngineOption { get; set; }

        /// <summary>
        /// 終了時にGlobalOptionのファイルを消すフラグ
        /// </summary>
        public bool DeleteGlobalOption { get; set; }

        /// <summary>
        /// singletonなinstance。それぞれのViewModelなどにアクセスしたければ、これ経由でアクセスする。
        /// </summary>
        public static TheApp app = new TheApp();

        #endregion

        #region privates

        /// <summary>
        /// MainDialogのウィンドウサイズをGlobalConfigに代入する。(次回起動時に復元するため)
        /// </summary>
        private void SaveMainDialogSize()
        {
        }

        /// <summary>
        /// 終了時に削除フラグが立っていなければ、このまま設定(GlobalConfigと各エンジンのオプション)を保存する。
        /// 削除フラグが立っていれば設定ファイルを削除する。
        /// </summary>
        private void SaveConfig()
        {
            if (DeleteGlobalOption)
                Config.Delete();
            else
                Config.Save();

            if (DeleteEngineOption)
                EngineConfigUtility.DeleteEngineConfig();
            else
                if (engine_configs != null)
                engine_configs.Save();
        }
        #endregion
    }
}
