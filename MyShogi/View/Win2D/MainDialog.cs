using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Shogi.Core;
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

            UpdateMenuItems();
            FindScreenSize();
        }

        // -- 各種定数

        // 盤面素材の画像サイズ
        public static readonly int board_img_width = 1920;
        public static readonly int board_img_height = 1080;

        // 盤面素材における、駒を配置する升の左上。
        public static readonly int board_top = 53;
        public static readonly int board_left = 526;

        // 駒素材の画像サイズ(駒1つ分)
        // これが横に8つ、縦に4つ、計32個並んでいる。
        public static readonly int piece_img_width = 96;
        public static readonly int piece_img_height = 106;

        // メニュー高さ。これはClientSize.Heightに含まれてしまうのでこれを加算した分だけ確保しないといけない。
        public static int menu_height = SystemInformation.MenuHeight;

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
                int clip = (board_img_width - cx) / 2;

                int w2 = (int)((board_img_width - clip * 2) * s);
                int h2 = (int)(board_img_height * s + menu_height);

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

        /// <summary>
        /// メニューのitemを動的に追加する。
        /// 商用版とフリーウェア版とでメニューが異なるのでここで動的に追加する必要がある。
        /// </summary>
        public void UpdateMenuItems()
        {
            var app = TheApp.app;
            var config = app.config;

            // Commercial Version GUI
            bool CV_GUI = config.YaneuraOu2018_GUI_MODE;
            if (CV_GUI)
                Text = "将棋神やねうら王";

            // -- メニューの追加。あとで考える。
            {
                var menu = new MenuStrip();

                //レイアウトロジックを停止する
                SuspendLayout();
                menu.SuspendLayout();

                // 前回設定されたメニューを除去する
                if (old_menu != null)
                    Controls.Remove(old_menu);

                var file_display = new ToolStripMenuItem();
                file_display.Text = "ファイル";
                menu.Items.Add(file_display);
                // あとで追加する。

                var item_display = new ToolStripMenuItem();
                item_display.Text = "表示";
                menu.Items.Add(item_display);

                // 駒種
                {
                    if (CV_GUI)
                    {
                        { // -- 盤画像の選択メニュー

                            var item = new ToolStripMenuItem();
                            item.Text = "盤画像";

                            var item1 = new ToolStripMenuItem();
                            item1.Text = "Type1";
                            item1.Checked = config.BoardImageVersion == 1;
                            item1.Click += (sender, e) =>
                            {
                                config.BoardImageVersion = 1;
                                UpdateMenuItems();
                            };
                            item.DropDownItems.Add(item1);

                            var item2 = new ToolStripMenuItem();
                            item2.Text = "Type2";
                            item2.Checked = config.BoardImageVersion == 2;
                            item2.Click += (sender, e) =>
                            {
                                config.BoardImageVersion = 2;
                                UpdateMenuItems();
                            };
                            item.DropDownItems.Add(item2);
                            item_display.DropDownItems.Add(item);

                        }

                        { // -- 駒画像の選択メニュー

                            var item = new ToolStripMenuItem();
                            item.Text = "駒画像";

                            var item1 = new ToolStripMenuItem();
                            item1.Text = "一文字駒";
                            item1.Checked = config.PieceImageVersion == 2;
                            item1.Click += (sender, e) =>
                            {
                                config.PieceImageVersion = 2;
                                // このときに画像の読み直しが発生する(かも)なので
                                // メニューの更新もこのへんでしとかないといけない。
                                UpdateMenuItems();
                            };
                            item.DropDownItems.Add(item1);

                            var item2 = new ToolStripMenuItem();
                            item2.Text = "二文字駒";
                            item2.Checked = TheApp.app.config.PieceImageVersion == 1;
                            item2.Click += (sender, e) =>
                            {
                                config.PieceImageVersion = 1;
                                UpdateMenuItems();
                            };
                            item.DropDownItems.Add(item2);

                            var item3 = new ToolStripMenuItem();
                            item3.Text = "英文字駒";
                            item3.Checked = TheApp.app.config.PieceImageVersion == 3;
                            item3.Click += (sender, e) =>
                            {
                                config.PieceImageVersion = 3;
                                UpdateMenuItems();
                            };
                            item.DropDownItems.Add(item3);
                            item_display.DropDownItems.Add(item);
                        }
                    }
                }

                Controls.Add(menu);
                //フォームのメインメニューとする
                MainMenuStrip = menu;
                old_menu = menu;

                //レイアウトロジックを再開する
                menu.ResumeLayout(false);
                menu.PerformLayout();
                ResumeLayout(false);
                PerformLayout();
            }

            Invalidate();
        }

        private MenuStrip old_menu { get; set; } = null;

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
                var destRect = new Rectangle(0, menu_height , ClientSize.Width , ClientSize.Height - menu_height);
                var sourceRect = new Rectangle(clip_x, 0, board_img_width - clip_x * 2, board_img_height);
                e.Graphics.DrawImage(board, destRect, sourceRect, GraphicsUnit.Pixel);
            }

            // -- 駒の描画
            {
                // 描画する盤面
                var pos = ViewModel.Pos;

                var piece = img.PieceImg.image;
                var scale_x = (float)ClientSize.Width / (board_img_width - clip_x*2 );
                var scale_y = (float)(ClientSize.Height - menu_height) / board_img_height;

                for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
                {
                    var pc = pos.PieceOn(sq);
                    if (pc != Piece.NO_PIECE)
                    {
                        File f = sq.ToFile();
                        Rank r = sq.ToRank();

                        var destRect = new Rectangle(
                            (int)(scale_x * (board_left - clip_x +(piece_img_width + 0.5f )* (8 - (int)f) - 4)    ),
                            (int)(scale_y * (board_top  +          piece_img_height    * (int)r) + menu_height),
                            (int)(piece_img_width  * 1 * scale_x),
                            (int)(piece_img_height * 1 * scale_y));

                        var sourceRect = new Rectangle(
                            ((int)pc % 8) * piece_img_width , 
                            ((int)pc / 8) * piece_img_height,
                            piece_img_width , piece_img_height);

                        e.Graphics.DrawImage(piece, destRect, sourceRect, GraphicsUnit.Pixel);
                    }
                }

            }

        }

        private void MainDialog_SizeChanged(object sender, EventArgs e)
        {
            // 画面のフルスクリーン化/ウィンドゥ化がなされたので、OnPaintが呼び出されるようにする。
            Invalidate();
        }

    }
}
