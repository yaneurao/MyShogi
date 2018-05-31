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
            UpdatePieceMoveImage();
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
        /// 着手の移動元、移動先のエフェクト画像の生成。
        /// TheApp.app.config.LastMoveColorTypeが変わったときなどに呼び出されるハンドラ
        /// </summary>
        public void UpdatePieceMoveImage()
        {
            var config = TheApp.app.config;

            // 駒の横・縦のサイズ[px]
            int x = 97;
            int y = 106;

            // 1. 着手の移動先の升のエフェクト
            // 2. 着手の移動元の升のエフェクト
            // 3. 駒を持ち上げたときの移動元のエフェクト
            // 4. 駒を持ち上げたときに移動できない升のエフェクト
            // の4つを左から並べる
            var bmp = new Bitmap(x * 4, y, PixelFormat.Format32bppArgb);

            // 左からn番目をcで塗りつぶす
            // Graphics.DrawImageだとαが合成されてしまい、思っている色で塗りつぶされない
            void Fill(int n , Color c)
            {
                for (int j = 0; j < y; ++j)
                    for (int i = 0; i < x; ++i)
                        bmp.SetPixel(i + n * x ,j, c);
                // SetPixel()遅いけど、そんなに大きな領域ではないし、リアルタイムでもないのでまあいいや。
            }

            // 番号に応じた色を返す
            Color ColorOf(int type)
            {
                Color c;
                switch (type)
                {
                    case 0: c = Color.FromArgb(0, 0, 0, 0); break;
                    case 1: c = Color.FromArgb((int)(255 * 0.40), 0xff, 0x7f, 0x50); break;
                    case 2: c = Color.FromArgb((int)(255 * 0.40), 0x41, 0x69, 0xe1); break;
                    case 3: c = Color.FromArgb((int)(255 * 0.40), 0x6b, 0x8e, 0x23); break;
                    default: c = Color.FromArgb(0, 0, 0, 0); break;
                }
                return c;
            }


            // 最終手の移動先の升の背景
            Fill(0, ColorOf(config.LastMoveToColorType));

            // 最終手の移動元の升の背景
            Fill(1, ColorOf(config.LastMoveFromColorType));

            // 確保したBitmapをImageLoaderの管理下に置く。
            PieceMoveImg.SetBitmap(bmp);
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
        /// 指し手の移動元や移動先の升の背景色を変更するのに用いる。
        /// </summary>
        public ImageLoader PieceMoveImg = new ImageLoader();

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
