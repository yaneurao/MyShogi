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

            // ディスプレイに収まるサイズのスクリーンにする必要がある。
            // プライマリスクリーンを基準にして良いのかどうかはわからん…。
            int w = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int h = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            float[] scale = new float[]
            {
                // ぴったり収まりそうな画面サイズを探す
                4.0f , 3.0f , 2.5f , 2.0f , 1.75f , 1.5f , 1.25f , 1.0f , 0.75f , 0.5f , 0.25f
            };

            foreach (var s in scale)
            {
                int w2 = (int)(1920 * s);
                int h2 = (int)(1080 * s);

                // 10%ほど余裕を持って画面に収まるサイズを探す。
                if (w >= (int)(w2 * 1.1f) && h >= (int)(h2 * 1.1f))
                {
                    ClientSize = new Size(w2, h2);
                    break;
                }
            }

            //Text = "将棋神やねうら王";
        }

        public MainDialogViewModel ViewModel { get; private set;}

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
                var destRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
                var sourceRect = new Rectangle(0, 0, board.Width, board.Height);
                e.Graphics.DrawImage(board, destRect, sourceRect, GraphicsUnit.Pixel);
            }

            // -- 駒の描画
            {
                // 書きかけ。あとでちゃんと書く。

                var piece = img.PieceImg.image;
                var scale_x = (float)ClientSize.Width / 1920;
                var scale_y = (float)ClientSize.Height / 1080;

                var destRect = new Rectangle((int)(scale_x * 526), (int)(scale_y * 53),
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
