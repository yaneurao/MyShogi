using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using DColor = System.Drawing.Color;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Resource.Images
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
            UpdatePieceAttackImage();
            UpdatePieceMoveImage();
            UpdateBoardNumberImage();
            UpdateHandNumberImage();
            UpdatePromoteDialogImage();
            UpdateTurnImage();

            UpdateLazyLoadImage();
        }

        /// <summary>
        /// TheApp.app.config.BoardImageVersionの設定が変わったときに画像を読み込む。
        /// </summary>
        public void UpdateBoardImage(PropertyChangedEventArgs args = null)
        {
            var config = TheApp.app.Config;

            var board = new ImageLoader();
            var tatami = new ImageLoader();
            var name_plate = new ImageLoader();

            Load(ref board ,$"board_v{config.BoardImageVersion}_1920_1080.png");
            Load(ref tatami, $"tatami_v{config.TatamiImageVersion}_1920_1080.png");
            Load(ref name_plate, "name_plate_v1_1920_1080.png");

            var piece_table = new ImageLoader[2];
            var piece_box = new ImageLoader[2];

            // 駒台
            // piece_table_version == 0 のとき普通の駒台
            // piece_table_version == 1 のとき細長い駒台
            foreach (var piece_table_version in All.Int(2))
            {
                var img = new ImageLoader();
                Load(ref img, $"komadai_v{piece_table_version + 1}_1920_1080.png");
                piece_table[piece_table_version] = img;

                var img2 = new ImageLoader();
                Load(ref img2, $"koma_bako_v{piece_table_version + 1}_1920_1080.png");
                piece_box[piece_table_version] = img2;
            }

            foreach (var piece_table_version in All.Int(2))
            {
                // 駒箱
                // piece_box_exist == false 駒箱なし
                // piece_box_exist ==  true 駒箱あり
                foreach (var piece_box_exist in All.Bools())
                {
                    var img = new ImageLoader();
                    img.CreateBitmap(1920, 1080, PixelFormat.Format24bppRgb);

                    // 畳と盤を合成する。
                    using (var g = Graphics.FromImage(img.image))
                    {
                        var rect = new Rectangle(0, 0, img.image.Width, img.image.Height);
                        // DrawImageで等倍の転送にするためにはrectの指定が必要
                        g.DrawImage(tatami.image, rect, rect, GraphicsUnit.Pixel);
                        g.DrawImage(board.image, rect, rect, GraphicsUnit.Pixel);
                        g.DrawImage(piece_table[piece_table_version].image, rect, rect, GraphicsUnit.Pixel);

                        // 駒台が縦長のとき、ネームプレートは別の素材
                        if (piece_table_version == 0)
                            g.DrawImage(name_plate.image, rect, rect, GraphicsUnit.Pixel);

                        // 駒箱を合成するのは盤面編集モードの時のみ
                        if (piece_box_exist)
                            g.DrawImage(piece_box[piece_table_version].image, rect, rect, GraphicsUnit.Pixel);
                    }

                    var id = piece_table_version + (piece_box_exist ? 2 : 0);

                    // 前回に合成したものは解放しておく。
                    if (BoardImages[id] != null)
                        BoardImages[id].Dispose();
                    BoardImages[id] = img;
                }
            }

            // もう使わないと思うので開放しておく
            foreach (var piece_table_version in All.Int(2))
            {
                piece_table[piece_table_version].Release();
                piece_box[piece_table_version].Release();
            }
            board.Release();
            tatami.Release();
            name_plate.Release();
        }

        /// <summary>
        /// TheApp.app.config.PieceImageVersionが変わったときに呼び出されるハンドラ
        /// </summary>
        public void UpdatePieceImage(PropertyChangedEventArgs args = null)
        {
            var config = TheApp.app.Config;
            Load(ref PieceImage, $"piece_v{config.PieceImageVersion}_776_636.png");
        }

        public void UpdatePieceAttackImage(PropertyChangedEventArgs args = null)
        {
            var config = TheApp.app.Config;
            var version = config.PieceAttackImageVersion;
            if (version == 0)
                PieceAttackImage.SetNullBitmap();
            else
                Load(ref PieceAttackImage, $"piece_atk_v{version}_776_424.png");
        }

        /// <summary>
        /// 着手の移動元、移動先のエフェクト画像の生成。
        /// TheApp.app.config.LastMoveColorTypeが変わったときなどに呼び出されるハンドラ
        /// </summary>
        public void UpdatePieceMoveImage(PropertyChangedEventArgs args = null)
        {
            var config = TheApp.app.Config;

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
            void Fill(PieceMoveEffect n , DColor c)
            {
                for (int j = 0; j < y; ++j)
                    for (int i = 0; i < x; ++i)
                        bmp.SetPixel(i + (int) n * x ,j, c);
                // SetPixel()遅いけど、そんなに大きな領域ではないし、リアルタイムでもないのでまあいいや。
            }

            // 最終手の移動元、移動先で用いるエフェクトの番号に応じた色を返す
            DColor ColorOf(int type)
            {
                DColor c;
                switch (type)
                {
                    case 0: c = DColor.FromArgb(0, 0, 0, 0); break;
                    case 1: c = DColor.FromArgb((int)(255 * 0.40), 0xff, 0x7f, 0x50); break;
                    case 2: c = DColor.FromArgb((int)(255 * 0.40), 0x41, 0x69, 0xe1); break;
                    case 3: c = DColor.FromArgb((int)(255 * 0.40), 0x6b, 0x8e, 0x23); break;
                    default: c = DColor.FromArgb(0, 0, 0, 0); break;
                }
                return c;
            }

            // 駒を掴んだ時の移動元で用いるエフェクトの番号に応じた色を返す
            DColor ColorOf2(int type)
            {
                DColor c;
                switch (type)
                {
                    case 0: c = DColor.FromArgb(0, 0, 0, 0); break;
                    case 1: c = DColor.FromArgb((int)(255 * 0.80), 0xff, 0xef, 0x80); break;
                    case 2: c = DColor.FromArgb((int)(255 * 0.80), 0x81, 0xa9, 0xf1); break;
                    case 3: c = DColor.FromArgb((int)(255 * 0.80), 0xab, 0xde, 0x73); break;
                    default: c = DColor.FromArgb(0, 0, 0, 0); break;
                }
                return c;
            }

            // 駒を掴んだ時の移動先(以外)で用いるエフェクトの番号に応じた色を返す
            DColor ColorOf3(int type)
            {
                DColor c;
                switch (type)
                {
                    case 0: c = DColor.FromArgb(0, 0, 0, 0); break;
                    case 1: c = DColor.FromArgb((int)(255 * 0.30), (int)(243 * 0.7), (int)(230 * 0.7), (int)(187 * 0.7)); break;
                    case 2: c = DColor.FromArgb((int)(255 * 0.30), (int)(243 * 0.5), (int)(230 * 0.5), (int)(187 * 0.5)); break;
                    case 3: c = DColor.FromArgb((int)(255 * 0.30), (int)(243 * 0.2), (int)(230 * 0.2), (int)(187 * 0.2)); break;
                    case 4: c = DColor.FromArgb((int)(255 * 0.3), 255, 255, 255); break;
                    case 5: c = DColor.FromArgb((int)(255 * 0.6), 255, 255, 255); break;

                    default: c = DColor.FromArgb(0, 0, 0, 0); break;
                }
                return c;
            }

            // 最終手の移動先の升の背景
            Fill(PieceMoveEffect.To   , ColorOf(config.LastMoveToColorType));

            // 最終手の移動元の升の背景
            Fill(PieceMoveEffect.From , ColorOf(config.LastMoveFromColorType));

            // 最終手の移動元の升の背景
            Fill(PieceMoveEffect.PickedFrom, ColorOf2(config.PickedMoveFromColorType));

            // 最終手の移動先(以外)の升の背景
            Fill(PieceMoveEffect.PickedTo, ColorOf3(config.PickedMoveToColorType));

            // 確保したBitmapをImageLoaderの管理下に置く。
            PieceMoveImage.SetBitmap(bmp);

#if false
            // 駒の移動元と移動先の候補の影
            if (config.LastMoveFromColorType == 4 || config.PickedMoveToColorType == 6)
                Load(ref PieceShadowImage, "piece_v0_776_636.png");
#endif
        }

        /// <summary>
        /// 駒番号の画像を読み込む
        /// </summary>
        public void UpdateBoardNumberImage(PropertyChangedEventArgs args = null)
        {
            var config = TheApp.app.Config;
            var version = config.BoardNumberImageVersion;

            // 0は非表示の意味
            if (version == 0)
            {
                // 描画したときに効果のない画像を入れておいたほうが、image == nullで場合分けする必要なくて助かる
                BoardNumberImageFile.SetNullBitmap();
                BoardNumberImageRevFile.SetNullBitmap();
                BoardNumberImageRank.SetNullBitmap();
                BoardNumberImageRevRank.SetNullBitmap();
            }
            else
            {
                Load(ref BoardNumberImageFile, $"number_v{version}_873_19.png");
                Load(ref BoardNumberImageRevFile, $"number_v{version}Rev_873_19.png");
                Load(ref BoardNumberImageRank, $"number_v{version}_22_954.png");
                Load(ref BoardNumberImageRevRank, $"number_v{version}Rev_22_954.png");
            }
        }

        /// <summary>
        /// 手駒の右肩に表示する駒の枚数を示す数字画像
        /// </summary>
        private void UpdateHandNumberImage()
        {
            Load(ref HandNumberImage , "hand_number_v1_864_96.png");
            Load(ref HandBoxNumberImage , "hand_box_number_v1_864_96.png");
        }

        /// <summary>
        /// 成り・不成を確認するダイアログ
        /// </summary>
        private void UpdatePromoteDialogImage()
        {
            Load(ref PromoteDialogImage, "promote_dialog.png");
        }

        /// <summary>
        /// 手番を示す画像素材
        /// </summary>
        private void UpdateTurnImage()
        {
            // 小さな画像なので読み直す必要はない。
            Load(ref TurnNormalImage, "turn_v1_106_43.png");
            Load(ref TurnSlimImage, "turn_v1_1057_157.png");
        }

        /// <summary>
        /// 画像の遅延読み込み。
        /// </summary>
        private void UpdateLazyLoadImage()
        {
            // 起動時にすぐに要らない画像は遅延読み込みにしておくことで起動時の高速化を図る。
            // いまは素材が少ないのでさほど問題とはなっていないが、素材が増えてきて、
            // 起動が遅くなるのは嫌なので…。

            // -- エンジンの初期化時の描画

            Load(ref EngineInitImage, "engine_init.png" , true);

            // -- エンジン選択時のNO BANNER

            Load(ref NoBannerImage, "no_banner.png", true);

            // -- 対局に関する画面効果

            Load(ref GameStartImage, "game_effect" + Sep + "game_start_v1.png", true);
            Load(ref GameWinImage,   "game_effect" + Sep + "game_win_v1.png"  , true);
            Load(ref GameLoseImage,  "game_effect" + Sep + "game_lose_v1.png" , true);
            Load(ref GameDrawImage,  "game_effect" + Sep + "game_draw_v1.png" , true);
            Load(ref GameBlackWhiteImage, "game_effect" + Sep + "game_black_white_v1.png", true);
        }

        /// <summary>
        /// ファイル名を与えて、ImgFolderから画像を読み込む
        /// lazy == trueになっていると遅延読み込みが有効になり、
        /// imageプロパティに初めてアクセスされた時に読み込みに行く。
        /// </summary>
        /// <param name="name"></param>
        private void Load(ref ImageLoader img, string name , bool lazy = false)
        {
            // プロパティをrefで渡せないので、プロパティにするのやめる。(´ω｀)

            img.Release();
            img.Load(Path.Combine(ImageFolder,name) , lazy);
        }

        /// <summary>
        /// フォルダの区切り文字列
        ///
        /// Linux環境などで動かすためには、フォルダの区切り文字列の手打ちは避けたほうが無難なので
        /// これを用いる。
        /// </summary>
        private static readonly char Sep = Path.DirectorySeparatorChar;

        /// <summary>
        /// 盤面 + 畳を合成したRGB(RGBAではない)画像
        /// 
        /// { 普通の駒台 , 小さな駒台 } × { 駒箱なし , 駒箱あり }の4通り生成して持っている。
        /// </summary>
        private ImageLoader[] BoardImages = new ImageLoader[4];

        // -- 以下、それぞれの画像

        /// <summary>
        /// 
        /// 盤面 + 畳を合成したRGB画像
        /// 
        /// piece_table_version
        ///   0 : 普通の駒台
        ///   1 : 小さな駒台
        ///   
        /// piece_box
        ///  false : 駒箱なし
        ///   true : 駒箱あり
        /// </summary>
        /// <param name="piece_table_version"></param>
        /// <returns></returns>
        public ImageLoader BoardImage(int piece_table_version , bool piece_box)
        {
            Debug.Assert(0 <= piece_table_version && piece_table_version < 2);

            return BoardImages[piece_table_version + (piece_box ? 2 : 0)];
        }

        /// <summary>
        /// 駒画像
        /// </summary>
        public ImageLoader PieceImage = new ImageLoader();

        /// <summary>
        /// 指し手の移動元や移動先の升の背景色を変更するのに用いる。
        /// </summary>
        public ImageLoader PieceMoveImage = new ImageLoader();

        /// <summary>
        /// 指し手の移動元や移動先の候補の升に描画する駒の影
        /// </summary>
        public ImageLoader PieceShadowImage = new ImageLoader();

        /// <summary>
        /// 駒の移動方向が描いてある画像
        /// </summary>
        public ImageLoader PieceAttackImage = new ImageLoader();

        /// <summary>
        /// 盤面の番号画像、筋・段、盤面反転の筋・段
        /// </summary>
        public ImageLoader BoardNumberImageFile = new ImageLoader();
        public ImageLoader BoardNumberImageRank = new ImageLoader();
        public ImageLoader BoardNumberImageRevFile = new ImageLoader();
        public ImageLoader BoardNumberImageRevRank = new ImageLoader();

        /// <summary>
        /// 手駒の右肩に表示する駒の枚数を示す数字画像
        /// 
        /// HandNumberImage    : 駒台の駒の枚数用の数字
        /// HandBoxNumberImage : 駒箱の駒の枚数用の数字
        /// </summary>
        public ImageLoader HandNumberImage = new ImageLoader();
        public ImageLoader HandBoxNumberImage = new ImageLoader();

        /// <summary>
        /// 成り・不成の選択ダイアログ用の画像
        /// </summary>
        public ImageLoader PromoteDialogImage = new ImageLoader();

        /// <summary>
        /// 手番画像
        /// </summary>
        public ImageLoader TurnNormalImage = new ImageLoader();
        public ImageLoader TurnSlimImage = new ImageLoader();

        /// <summary>
        /// エンジン初期化中の画像
        /// </summary>
        public ImageLoader EngineInitImage = new ImageLoader();

        /// <summary>
        /// エンジンのバナーなし
        /// </summary>
        public ImageLoader NoBannerImage = new ImageLoader();

        #region GameEffects
        /// <summary>
        /// 対局開始/終了/再開
        /// </summary>
        public ImageLoader GameStartImage = new ImageLoader();

        /// <summary>
        /// 対局開始時の「先手」「後手」「上手」「下手」の文字列
        /// </summary>
        public ImageLoader GameBlackWhiteImage = new ImageLoader();

        /// <summary>
        /// 終局時の勝利画像
        /// (プレイヤーの片側のみが人間であるとき)
        /// </summary>
        public ImageLoader GameWinImage = new ImageLoader();

        /// <summary>
        /// 終局時の敗北画像
        /// (プレイヤーの片側のみが人間であるとき)
        /// </summary>
        public ImageLoader GameLoseImage = new ImageLoader();

        /// <summary>
        /// 終局時の引き分け画像
        /// (プレイヤーの片側のみが人間であるとき)
        /// </summary>
        public ImageLoader GameDrawImage = new ImageLoader();

        #endregion
    }
}
