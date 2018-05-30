using MyShogi.App;
using System.Drawing;
using System.Drawing.Imaging;
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
            var board = new ImageLoader();
            var tatami = new ImageLoader();
            Load(ref board ,$"board_v{config.BoardImageVersion}_1920_1080.png");
            Load(ref tatami, $"tatami_v{config.TatamiImageVersion}_1920_1080.png");

            BoardImg.CreateBitmap(1920, 1080, PixelFormat.Format24bppRgb);

            // 畳と盤を合成する。
            using (var g = Graphics.FromImage(BoardImg.image))
            {
                var rect = new Rectangle(0, 0, BoardImg.image.Width, BoardImg.image.Height);
                // DrawImageで等倍の転送にするためにはrectの指定が必要
                g.DrawImage(tatami.image , rect , rect , GraphicsUnit.Pixel);
                g.DrawImage(board.image, rect , rect , GraphicsUnit.Pixel);
            }

            // しばらく使わないと思うので開放しておく
            board.Release();
            tatami.Release();
        }

        /// <summary>
        /// TheApp.app.config.PieceImageVersionが変わったときに呼び出されるハンドラ
        /// </summary>
        public void UpdatePieceImage()
        {
            var config = TheApp.app.config;
            Load(ref PieceImg , $"piece_v{config.PieceImageVersion}_776_424.png");
        }

        /// <summary>
        /// 駒番号の画像を読み込む
        /// </summary>
        public void UpdateBoardNumberImage()
        {
            var config = TheApp.app.config;
            var version = config.BoardNumberImageVersion;

            // 0は非表示の意味
            if (version == 0)
                return;

            Load(ref BoardNumberImgFile    , $"number_v{version}_873_19.png");
            Load(ref BoardNumberImgRevFile , $"number_v{version}Rev_873_19.png");
            Load(ref BoardNumberImgRank    , $"number_v{version}_22_954.png");
            Load(ref BoardNumberImgRevRank , $"number_v{version}Rev_22_954.png");
        }

        /// <summary>
        /// 手駒の右肩に表示する駒の枚数を示す数字画像
        /// </summary>
        private void UpdateHandNumberImage()
        {
            Load(ref HandNumberImg , "hand_number_v1_864_96.png");
        }


        /// <summary>
        /// ファイル名を与えて、ImgFolderから画像を読み込む
        /// </summary>
        /// <param name="name"></param>
        private void Load(ref ImageLoader img, string name)
        {
            // プロパティをrefで渡せないので、プロパティにするのやめる。(´ω｀)

            img.Release();
            img.Load(Path.Combine(ImageFolder,name));
        }

        // -- 以下、それぞれの画像

        /// <summary>
        /// 盤面 + 畳を合成したRGB画像
        /// </summary>
        public ImageLoader BoardImg = new ImageLoader();

        /// <summary>
        /// 駒画像
        /// </summary>
        public ImageLoader PieceImg = new ImageLoader();

        /// <summary>
        /// 盤面の番号画像、筋・段、盤面反転の筋・段
        /// </summary>
        public ImageLoader BoardNumberImgFile  = new ImageLoader();
        public ImageLoader BoardNumberImgRank  = new ImageLoader();
        public ImageLoader BoardNumberImgRevFile  = new ImageLoader();
        public ImageLoader BoardNumberImgRevRank  = new ImageLoader();

        /// <summary>
        /// 手駒の右肩に表示する駒の枚数を示す数字画像
        /// </summary>
        public ImageLoader HandNumberImg = new ImageLoader();
    }
}
