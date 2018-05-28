using System;
using System.IO;
using System.Windows.Forms;
using MyShogi.Controller;
using MyShogi.Model.ObjectModel;
using MyShogi.Model.Resource;

// とりま、Windows用
// あとで他環境用を用意する
using MyShogi.View.Win2D;

using MyShogi.ViewModel;

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
            // -- 開発時のテストコード

            // 駒素材画像の変換
            //ImageConverter.ConvertPieceImage();

            // -- global configの読み込み

            config = GlobalConfig.CreateInstance();

            // -- 各インスタンスの生成と、それぞれのbind作業

            // -- 画像の読み込み

            {
                imageManager = new ImageManager();
                imageManager.Update(); // ここでconfigに従い、画像が読み込まれる。

                // GlobalConfigのプロパティ変更に対して、このimageManagerが呼び出されるようにbindしておく。

                config.AddPropertyChangedHandler("BoardImageVersion", imageManager.UpdateBoardImage);
                config.AddPropertyChangedHandler("PieceImageVersion", imageManager.UpdatePieceImage);
            }

            // -- メインの対局ウィンドゥ

            var mainDialog = new MainDialog();
            mainDialogViewModel = new MainDialogViewModel();
            mainDialog.Bind(mainDialogViewModel);

            // -- 対局controllerを1つ生成して、メインの対局ウィンドゥのViewModelに加える
            {
                var game = new GameController();
                mainDialogViewModel.Add(game);
            }

            // 盤・駒が変更されたときにMainDialogのメニューの内容を修正しないといけないので更新がかかるようにしておく。
            config.AddPropertyChangedHandler("BoardImageVersion", mainDialog.UpdateMenuItems );
            config.AddPropertyChangedHandler("PieceImageVersion", mainDialog.UpdateMenuItems );


            // Notifyクラスのテスト(あとで消す)
            //NotifyTest.Test();

            // 終了するときに設定ファイルに書き出すコード
            Application.ApplicationExit += new EventHandler((sender,e) =>
            {
                config.Save();
            });

            Application.Run(mainDialog);
        }

        // -- それぞれのViewModel
        // 他のViewModelにアクションが必要な場合は、これを経由して通知などを行えば良い。
        // 他のViewに直接アクションを起こすことは出来ない。必ずViewModelに通知などを行い、
        // そのViewModelのpropertyをsubscribeしているViewに間接的に通知が行くという仕組みを取る。

        /// <summary>
        /// MainDialogのViewModel
        /// </summary>
        public MainDialogViewModel mainDialogViewModel { get; private set; }

        /// <summary>
        /// 画像の読み込み用。本GUIで用いる画像はすべてここから取得する。
        /// </summary>
        public ImageManager imageManager { get; private set; }

        /// <summary>
        /// GUIの全体設定
        /// </summary>
        public GlobalConfig config { get; private set; }

        /// <summary>
        /// singletonなinstance。それぞれのViewModelなどにアクセスしたければ、これ経由でアクセスする。
        /// </summary>
        public static TheApp app = new TheApp();
    }
}
