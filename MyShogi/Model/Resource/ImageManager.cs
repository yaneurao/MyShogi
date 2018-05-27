using MyShogi.App;
using System.IO;
using System.Windows.Forms;

namespace MyShogi.Model.Resource
{
    /// <summary>
    /// 画像を動的に読み込んで管理するためのクラス。
    /// 本GUI専用であって、汎用性はない。
    /// </summary>
    public class ImageManager
    {
        /// <summary>
        /// 画像ファイルの存在するフォルダ
        /// </summary>
        public static readonly string ImageFolder = "image";

        /// <summary>
        /// Configの設定に従い、画像を読み込む。
        /// </summary>
        public void Update()
        {
            var config = TheApp.app.config;

            BoardImg.Release();
            PieceImg.Release();

            BoardImg = Load($"board_v{config.BoardImageVersion}_1920_1080.png");
            // 画像の読み込みに失敗していたら警告ダイアログを表示する。
            if (BoardImg.image == null)
            {
                MessageBox.Show("盤画像の読み込みに失敗しました。");

                // このままApplication.Exit()させてしまうと次回以降も読み込みに失敗してしまい、
                // 永久に起動出来なくなってしまう。
                config.BoardImageVersion = 1;

                Application.Exit();
            }

            PieceImg = Load($"piece_v{config.PieceImageVersion}_768_424.png");
            if (PieceImg.image == null)
            {
                MessageBox.Show("駒画像の読み込みに失敗しました。");

                // このままApplication.Exit()させてしまうと次回以降も読み込みに失敗してしまい、
                // 永久に起動出来なくなってしまう。
                config.PieceImageVersion = 1;

                Application.Exit();
            }
        }

        /// <summary>
        /// ファイル名を与えて、ImgFolderから画像を読み込む
        /// </summary>
        /// <param name="name"></param>
        private ImageLoader Load(string name)
        {
            var img = new ImageLoader();
            img.Load(Path.Combine(ImageFolder,name));
            return img;
        }

        // -- 以下、それぞれの画像

        /// <summary>
        /// 盤面
        /// </summary>
        public ImageLoader BoardImg { get; private set; } = new ImageLoader();

        /// <summary>
        /// 駒画像
        /// </summary>
        public ImageLoader PieceImg { get; private set; } = new ImageLoader();
    }
}
