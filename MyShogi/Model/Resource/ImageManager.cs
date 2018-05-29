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
        /// TheApp.app.configの内容に従い、画像を読み込む。
        /// 
        /// これらの画像を描画するのはUIスレッドのみ。
        /// また、読み直しもUIスレッドが行うので、スレッド競合の問題はない。
        /// </summary>
        public void Update()
        {
            UpdateBoardImage();
            UpdatePieceImage();
            UpdateBoardNumberImage();
            UpdateHandNumberImage();
        }

        /// <summary>
        /// TheApp.app.config.BoardImageVersionの設定が変わったときに画像を読み込む。
        /// </summary>
        public void UpdateBoardImage()
        {
            var config = TheApp.app.config;

            BoardImg.Release();

            BoardImg = Load($"board_v{config.BoardImageVersion}_1920_1080.png",true);
            // 画像の読み込みに失敗していたら警告ダイアログを表示する。
            if (BoardImg.image == null)
            {
                MessageBox.Show("盤画像の読み込みに失敗しました。");

                // このままApplication.Exit()させてしまうと次回以降も読み込みに失敗してしまい、
                // 永久に起動出来なくなってしまう。
                config.BoardImageVersion = 1;

                Application.Exit();
            }

        }

        /// <summary>
        /// TheApp.app.config.PieceImageVersionが変わったときに呼び出されるハンドラ
        /// </summary>
        public void UpdatePieceImage()
        {
            var config = TheApp.app.config;

            PieceImg.Release();

            PieceImg = Load($"piece_v{config.PieceImageVersion}_776_424.png");
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
        /// 駒番号の画像を読み込む
        /// </summary>
        public void UpdateBoardNumberImage()
        {
            var config = TheApp.app.config;
            var version = config.BoardNumberImageVersion;

            BoardNumberImgFile.Release();
            BoardNumberImgRevFile.Release();
            BoardNumberImgRank.Release();
            BoardNumberImgRevRank.Release();

            // 0は非表示の意味
            if (version == 0)
                return;

            BoardNumberImgFile = Load($"number_v{version}_873_19.png");
            BoardNumberImgRevFile = Load($"number_v{version}Rev_873_19.png");
            BoardNumberImgRank = Load($"number_v{version}_22_954.png");
            BoardNumberImgRevRank = Load($"number_v{version}Rev_22_954.png");

            if (BoardNumberImgFile.image == null || BoardNumberImgRevFile.image == null
                || BoardNumberImgRank.image == null || BoardNumberImgRevRank.image == null)
            {
                MessageBox.Show("駒番号画像の読み込みに失敗しました。");

                // このままApplication.Exit()させてしまうと次回以降も読み込みに失敗してしまい、
                // 永久に起動出来なくなってしまう。
                config.BoardNumberImageVersion = 0;

                Application.Exit();
            }
        }

        /// <summary>
        /// 手駒の右肩に表示する駒の枚数を示す数字画像
        /// </summary>
        private void UpdateHandNumberImage()
        {
            HandNumberImg = Load("hand_number_v1_864_96.png");
        }


        /// <summary>
        /// ファイル名を与えて、ImgFolderから画像を読み込む
        /// 
        /// noAlpha == trueなら、ARGBではなくRGBのBitmapを作成してそこに読み込む。
        /// </summary>
        /// <param name="name"></param>
        private ImageLoader Load(string name , bool noAlpha = false)
        {
            var img = new ImageLoader();
            img.Load(Path.Combine(ImageFolder,name) , noAlpha);
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

        /// <summary>
        /// 盤面の番号画像、筋・段、盤面反転の筋・段
        /// </summary>
        public ImageLoader BoardNumberImgFile { get; private set; } = new ImageLoader();
        public ImageLoader BoardNumberImgRank { get; private set; } = new ImageLoader();
        public ImageLoader BoardNumberImgRevFile { get; private set; } = new ImageLoader();
        public ImageLoader BoardNumberImgRevRank { get; private set; } = new ImageLoader();

        /// <summary>
        /// 手駒の右肩に表示する駒の枚数を示す数字画像
        /// </summary>
        public ImageLoader HandNumberImg { get; private set; } = new ImageLoader();
    }
}
