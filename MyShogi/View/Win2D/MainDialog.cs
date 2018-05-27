using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.ViewModel;

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

            FindScreenSize();

            if (TheApp.app.YaneuraOu2018_GUI_MODE)
                Text = "将棋神やねうら王";
        }

        /// <summary>
        /// スクリーンサイズにぴったり収まるぐらいのウィンドゥサイズを決定する。
        /// </summary>
        public void FindScreenSize()
        {
            // ディスプレイに収まるサイズのスクリーンにする必要がある。
            // プライマリスクリーンを基準にして良いのかどうかはわからん…。
            int w = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int h = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            float[] scale = new float[]
            {
                // ぴったり収まりそうな画面サイズを探す
                4.0f , 3.0f , 2.5f , 2.0f , 1.75f , 1.5f , 1.25f , 1.0f , 0.75f , 0.5f , 0.25f
            };

            bool is_suitable(int cx /* window width*/ , float s /* scale */)
            {
                // window size = 1440 : 1080 = 4:3
                int clip = (1920 - cx) / 2;

                int w2 = (int)((1920 - clip * 2) * s);
                int h2 = (int)(1080 * s);

                // 10%ほど余裕を持って画面に収まるサイズを探す。
                if (w >= (int)(w2 * 1.1f) && h >= (int)(h2 * 1.1f))
                {
                    ClientSize = new Size(w2, h2);
                    clip_x = clip;
                    return true;
                }
                return false;
            }

            foreach (var s in scale)
            {
                // window size = 1620 : 1080 = 3:2
                if (is_suitable(1620, s))
                    break;

                // window size = 1440 : 1080 = 4:3
                if (is_suitable(1440, s))
                    break;

            }
        }

        public MainDialogViewModel ViewModel { get; private set;}

        /// <summary>
        /// 盤面画像を描画するときに左側と右側とをclipする量。
        /// </summary>
        public int clip_x = 0;

        /// <summary>
        /// ViewModelを設定する。
        /// このクラスのインスタンスとは1:1で対応する。
        /// </summary>
        /// <param name="vm"></param>
        public void Bind(MainDialogViewModel vm)
        {
            ViewModel = vm;
        }

        private void MainDialog_Paint(object sender, PaintEventArgs e)
        {
            // ここに盤面を描画するコードを色々書く。(あとで)

            var app = TheApp.app;
            var img = app.imageManager;

            // -- 盤面の描画
            {
                var board = img.BoardImg.image;
                var destRect = new Rectangle(0, 0, ClientSize.Width , ClientSize.Height);
                var sourceRect = new Rectangle(clip_x, 0, board.Width - clip_x * 2, board.Height);
                e.Graphics.DrawImage(board, destRect, sourceRect, GraphicsUnit.Pixel);
            }

            // -- 駒の描画
            {
                // 書きかけ。あとでちゃんと書く。

                var piece = img.PieceImg.image;
                var scale_x = (float)ClientSize.Width / ( 1920 - clip_x*2 );
                var scale_y = (float)ClientSize.Height / 1080;

                var destRect = new Rectangle((int)(scale_x * (526-clip_x )), (int)(scale_y * 53),
                    (int)(96 * 8 * scale_x) , (int)(106*4*scale_y));
                var sourceRect = new Rectangle(0, 0, piece.Width, piece.Height);
                e.Graphics.DrawImage(piece, destRect, sourceRect, GraphicsUnit.Pixel);
            }

        }

        private void MainDialog_SizeChanged(object sender, EventArgs e)
        {
            // 画面のフルスクリーン化/ウィンドゥ化がなされたので、OnPaintが呼び出されるようにする。
            Invalidate();
        }

    }
}
