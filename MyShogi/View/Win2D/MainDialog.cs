using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Test;
using MyShogi.ViewModel;
using ShogiCore = MyShogi.Model.Shogi.Core;

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

            FitToScreenSize();
            FitToClientSize();

            MinimumSize = new Size(192*2 , 108*2 + menu_height );
        }

        /// <summary>
        /// メニュー高さとToolStripの高さをあわせたもの。
        /// これはClientSize.Heightに含まれてしまうので実際の描画エリアはこれを減算したもの。
        /// </summary>
        public int menu_height
        {
            get
            {
                return System.Windows.Forms.SystemInformation.MenuHeight + toolStrip1.Height;
            }
        }

        /// <summary>
        /// 現在のスクリーンの大きさに合わせたウィンドウサイズにする。(起動時)
        /// </summary>
        public void FitToScreenSize()
        {
            // ディスプレイに収まるサイズのスクリーンにする必要がある。
            // プライマリスクリーンを基準にして良いのかどうかはわからん…。
            int w = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int h = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - menu_height;

            // いっぱいいっぱいだと邪魔なので90%ぐらい使うか。
            w = (int)(w * 0.9);
            h = (int)(h * 0.9);

            // 縦(h)を基準に横方向のクリップ位置を決める
            // 1920 * 1080が望まれる比率
            int w2 = h * 1920 / 1080;

            if (w > w2)
            {
                w = w2;
                // 横幅が余りつつも画面にぴったりなのでこのサイズで生成する。
            }
            else
            {
                int h2 = w * 1080 / 1920;
                h = h2;
            }
            ClientSize = new Size(w, h + menu_height);
        }

        /// <summary>
        /// 現在のウィンドウサイズに収まるようにaffine変換の係数を決定する。
        /// </summary>
        public void FitToClientSize()
        {
            int w = ClientSize.Width;
            int h = ClientSize.Height - menu_height;

            // 縦(h)を基準に横方向のクリップ位置を決める
            // 1920 * 1080が望まれる比率
            int w2 = h * 1920 / 1080;

            // ちらつかずにウインドウのaspect ratioを保つのは.NET Frameworkの範疇では不可能。
            // ClientSizeをResizeイベント中に変更するのは安全ではない。
            // cf. 
            //   https://qiita.com/yu_ka1984/items/b4a3ce9ed7750bd67b86
            // →　あきらめる

#if false
            // このコード、有効にするとハングする。
            double ratio = (double)h / w;
            if (ratio < 0.563)
            {
                w = (int)(h / 0.563);
                ClientSize = new Size(w, h + menu_height);
            }
            else if (ratio > 0.726)
            {
                w = (int)(h / 0.726);
                ClientSize = new Size(w, h + menu_height);
            }
#endif

            offset_x = (w - w2) / 2;
            offset_y = menu_height;
            scale_y = (double)h / board_img_height;
            scale_x = scale_y;
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
            bool CV_GUI = config.CommercialVersion;
            if (CV_GUI)
                Text = "将棋神やねうら王";
            // 商用版とどこで差別化するのか考え中

            // -- メニューの追加。あとで考える。
            {
                var menu = new MenuStrip();

                //レイアウトロジックを停止する
                SuspendLayout();
                menu.SuspendLayout();

                // 前回設定されたメニューを除去する
                if (old_menu != null)
                    Controls.Remove(old_menu);

                var item_file = new ToolStripMenuItem();
                item_file.Text = "ファイル";
                menu.Items.Add(item_file);
                // あとで追加する。

                var item_playgame = new ToolStripMenuItem();
                item_playgame.Text = "対局";
                menu.Items.Add(item_playgame);
                // あとで追加する。

                var item_display = new ToolStripMenuItem();
                item_display.Text = "表示";
                menu.Items.Add(item_display);

                // -- 「表示」配下のメニュー
                {
                    { // -- 盤面反転
                        var item = new ToolStripMenuItem();
                        item.Text = "盤面反転";
                        item.Checked = config.BoardReverse;
                        item.Click += (sender, e) => { config.BoardReverse = !config.BoardReverse; };

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 段・筋の画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "筋・段の表示";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "非表示";
                        item1.Checked = config.BoardNumberImageVersion == 0;
                        item1.Click += (sender, e) => { config.BoardNumberImageVersion = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "標準";
                        item2.Checked = TheApp.app.config.BoardNumberImageVersion == 1;
                        item2.Click += (sender, e) => { config.BoardNumberImageVersion = 1; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "Chess式";
                        item3.Checked = TheApp.app.config.BoardNumberImageVersion == 2;
                        item3.Click += (sender, e) => { config.BoardNumberImageVersion = 2; };
                        item.DropDownItems.Add(item3);
                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 盤画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "盤画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "白色";
                        item1.Checked = config.BoardImageVersion == 1;
                        item1.Click += (sender, e) => { config.BoardImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "黄色";
                        item2.Checked = config.BoardImageVersion == 2;
                        item2.Click += (sender, e) => { config.BoardImageVersion = 2; };
                        item.DropDownItems.Add(item2);
                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 盤画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "畳画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "薄い";
                        item1.Checked = config.TatamiImageVersion == 1;
                        item1.Click += (sender, e) => { config.TatamiImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "濃い";
                        item2.Checked = config.TatamiImageVersion == 2;
                        item2.Click += (sender, e) => { config.TatamiImageVersion = 2; };
                        item.DropDownItems.Add(item2);
                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 駒画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "駒画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "一文字駒";
                        item1.Checked = config.PieceImageVersion == 2;
                        item1.Click += (sender, e) => { config.PieceImageVersion = 2; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "二文字駒";
                        item2.Checked = TheApp.app.config.PieceImageVersion == 1;
                        item2.Click += (sender, e) => { config.PieceImageVersion = 1; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "英文字駒";
                        item3.Checked = TheApp.app.config.PieceImageVersion == 3;
                        item3.Click += (sender, e) => { config.PieceImageVersion = 3; };
                        item.DropDownItems.Add(item3);
                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 駒画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "最終手の升の背景色";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "朱色";
                        item1.Checked = config.LastMoveColorType == 1;
                        item1.Click += (sender, e) => { config.LastMoveColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色";
                        item2.Checked = TheApp.app.config.LastMoveColorType == 2;
                        item2.Click += (sender, e) => { config.LastMoveColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色";
                        item3.Checked = TheApp.app.config.LastMoveColorType == 3;
                        item3.Click += (sender, e) => { config.LastMoveColorType = 3; };
                        item.DropDownItems.Add(item3);
                        item_display.DropDownItems.Add(item);
                    }

                }

                // 「その他」
                {
                    var item_others = new ToolStripMenuItem();
                    item_others.Text = "その他";
                    menu.Items.Add(item_others);

                    // aboutダイアログ

                    var item1 = new ToolStripMenuItem();
                    item1.Text = "about..";
                    item1.Click += (sender, e) =>
                    {
                        if (AboutDialog == null)
                            AboutDialog = new AboutYaneuraOu();

                        AboutDialog.ShowDialog();
                    };
                    item_others.DropDownItems.Add(item1);

                }

#if DEBUG
                // デバッグ用にメニューにテストコードを実行する項目を追加する。
                {
                    var item_debug = new ToolStripMenuItem();
                    item_debug.Text = "デバッグ";

                    var item1 = new ToolStripMenuItem();
                    item1.Text = "DevTest1.Test1()";
                    item1.Click += (sender, e) => { DevTest1.Test1(); };
                    item_debug.DropDownItems.Add(item1);

                    var item2 = new ToolStripMenuItem();
                    item2.Text = "DevTest1.Test2()";
                    item2.Click += (sender, e) => { DevTest1.Test2(); };
                    item_debug.DropDownItems.Add(item2);

                    var item3 = new ToolStripMenuItem();
                    item3.Text = "DevTest1.Test3()";
                    item3.Click += (sender, e) => { DevTest1.Test3(); };
                    item_debug.DropDownItems.Add(item3);

                    var item4 = new ToolStripMenuItem();
                    item4.Text = "DevTest2.Test1()";
                    item4.Click += (sender, e) => { DevTest2.Test1(); };
                    item_debug.DropDownItems.Add(item4);

                    menu.Items.Add(item_debug);
                }
#endif

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

        public MainDialogViewModel ViewModel { get; private set; }

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
            var config = app.config;
            var img = app.imageManager;
            var g = e.Graphics;
            var vm = ViewModel;

            // 盤面を反転させて描画するかどうか
            var reverse = config.BoardReverse;

            // -- 盤面の描画
            {
                // 座標系はストレートに指定しておけばaffine変換されて適切に描画される。
                var board = img.BoardImg.image;
                var srcRect = new Rectangle(0, 0, board_img_width , board_img_height);
                var dstRect = new Rectangle(0, 0, board_img_width , board_img_height);
                Draw(g, board, dstRect, srcRect);
            }

            // -- 駒の描画
            {
                // 描画する盤面
                var pos = ViewModel.Pos;
                var lastMove = pos.State().lastMove;
                var lastMoveTo = (lastMove != ShogiCore.Move.NONE) ? lastMove.To() : Square.NB;

                var piece = img.PieceImg.image;

                for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
                {
                    var pc = pos.PieceOn(sq);
                    if (pc != Piece.NO_PIECE )
                    {
                        // 盤面反転モードなら、盤面を180度回した升から取得
                        Square sq2 = reverse ? sq.Inv() : sq;
                        int f = 8 - (int)sq2.ToFile();
                        int r = (int)sq2.ToRank();

                        var dstRect = new Rectangle(board_left + piece_img_width * f, board_top + piece_img_height * r,
                            piece_img_width, piece_img_height);

                        // 盤面反転モードなら、駒を先後入れ替える
                        if (reverse)
                            pc = pc ^ Piece.WHITE;

                        var srcRect = new Rectangle(
                            ((int)pc % 8) * piece_img_width,
                            ((int)pc / 8) * piece_img_height,
                            piece_img_width, piece_img_height);

                        // これが最終手の移動先の升であるなら、最終手として描画する必要がある。
                        if (sq == lastMoveTo)
                        {
                            var pc2 = Piece.WHITE; // 素材のここに朱色の塗りつぶしがあるはず
                            var srcRect2 = new Rectangle(
                                ((int)pc2 % 8) * piece_img_width,
                                ((int)pc2 / 8) * piece_img_height,
                                piece_img_width, piece_img_height);
                            Draw(g, piece, dstRect, srcRect2);
                        }

                        Draw(g, piece, dstRect, srcRect);
                    }
                }

                // -- 手駒の描画

                var hand_number = img.HandNumberImg.image;

                for (var c = ShogiCore.Color.ZERO; c < ShogiCore.Color.NB; ++c)
                {
                    Hand h = pos.Hand(reverse ? c.Not() : c);

                    // c側の駒台に描画する。

                    // 枚数によって位置が自動調整されるの、わりと見づらいので嫌。
                    // 駒種によって位置固定で良いと思う。


                    //同種の駒が3枚以上になったときに、その駒は1枚だけを表示して、
                    //数字を右肩表示しようと考えていたのですが、例えば、金が2枚、
                    //歩が3枚あるときに、歩だけが数字表示になるのはどうもおかしい気が
                    //するのです。2枚以上は全部数字表示のほうが良いだろう。

                    foreach (var loc in hand_location)
                    {
                        var pc = loc.piece;

                        int count = h.Count(pc);
                        if (count != 0)
                        {
                            // 後手の駒台には後手の駒を描画しなくてはならないのでpcを後手の駒にする。

                            if (c == ShogiCore.Color.WHITE)
                                pc = pc | Piece.WHITE;

                            var srcRect = new Rectangle(
                                ((int)pc % 8) * piece_img_width,
                                ((int)pc / 8) * piece_img_height,
                                piece_img_width, piece_img_height);

                            Rectangle dstRect;
                            
                            if (c == ShogiCore.Color.BLACK)
                                dstRect = new Rectangle(
                                    hand_table_pos[(int)c].X + loc.x,
                                    hand_table_pos[(int)c].Y + loc.y,
                                    piece_img_width, piece_img_height);
                            else
                                // 180度回転させた位置を求める
                                // 後手も駒の枚数は右肩に描画するのでそれに合わせて左右のmarginを調整する。
                                dstRect = new Rectangle(
                                    hand_table_pos[(int)c].X + (hand_table_width  - loc.x - piece_img_width  - 10 ) ,
                                    hand_table_pos[(int)c].Y + (hand_table_height - loc.y - piece_img_height) ,
                                    piece_img_width , piece_img_height);

                            Draw(g, piece, dstRect, srcRect);

                            // 数字の表示
                            if (count > 1)
                            {
                                var srcRect2 = new Rectangle(48 * (count - 1), 0, 48, 48);
                                var dstRect2 = new Rectangle(dstRect.Left + 60, dstRect.Top + 20, 48, 48);
                                Draw(g, hand_number, dstRect2, srcRect2);
                            }
                        }
                    }

                }
            }

            // -- 盤の段・筋を表す数字の表示
            {
                var version = config.BoardNumberImageVersion;
                if (version != 0)
                {
                    var file_img = (!reverse) ? img.BoardNumberImgFile.image : img.BoardNumberImgRevFile.image;
                    if (file_img != null)
                    {
                        var dstRect = new Rectangle(526, 32 , file_img.Width, file_img.Height);
                        var srcRect = new Rectangle(0, 0, file_img.Width, file_img.Height);
                        Draw(g, file_img, dstRect, srcRect);
                    }

                    var rank_img = (!reverse) ? img.BoardNumberImgRank.image : img.BoardNumberImgRevRank.image;
                    if (rank_img != null)
                    {
                        var dstRect = new Rectangle(1397, 49, rank_img.Width, rank_img.Height);
                        var srcRect = new Rectangle(0, 0, rank_img.Width, rank_img.Height);
                        Draw(g, rank_img, dstRect, srcRect);
                    }
                }
            }

            // -- 対局者氏名
            {
                DrawString(g, name_plate[reverse ? 1 : 0] , vm.Player1Name);
                DrawString(g, name_plate[reverse ? 0 : 1] , vm.Player2Name);
            }

        }

        // -- affine変換してのスクリーンへの描画

        /// <summary>
        /// 元画像から画面に描画するときに横・縦方向の縮小率とオフセット値(affine変換の係数)
        /// Draw()で描画するときに用いる。
        /// </summary>
        private double scale_x;
        private double scale_y;
        private int offset_x;
        private int offset_y;

        /// <summary>
        /// scale_x,scale_y、offset_x,offset_yを用いてアフィン変換してから描画する。
        /// </summary>
        /// <param name="g"></param>
        /// <param name="img"></param>
        /// <param name="destRect"></param>
        /// <param name="sourceRect"></param>
        private void Draw(Graphics g, Image img, Rectangle dstRect, Rectangle srcRect)
        {
            var dstRect2 = new Rectangle(
            (int)(dstRect.Left   * scale_x) + offset_x,
            (int)(dstRect.Top    * scale_y) + offset_y,
            (int)(dstRect.Width  * scale_x),
            (int)(dstRect.Height * scale_y)
            );
            g.DrawImage(img, dstRect2, srcRect, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// scale_x,scale_y、offset_x,offset_yを用いてアフィン変換してから描画する。
        /// </summary>
        /// <param name="g"></param>
        /// <param name="dstPoint"></param>
        /// <param name="mes"></param>
        private void DrawString(Graphics g , Point dstPoint , string mes)
        {
            var dstPoint2 = new Point(
            (int)(dstPoint.X * scale_x) + offset_x,
            (int)(dstPoint.Y * scale_y) + offset_y
            );

            // 文字フォントサイズは、scaleの影響を受ける。

            var font_size = (int)(20 * scale_x);
            // こんな小さいものは視認できないので描画しなくて良い。
            if (font_size <= 2)
                return;

            using (var font = new Font("MSPゴシック", font_size))
            {
                g.DrawString(mes, font, Brushes.Black, dstPoint2);
            }
        }

        // -- 以下、このフォームの管理下にあるDialog

        /// <summary>
        /// 「やねうら王について」のダイアログ
        /// </summary>
        public Form AboutDialog;

        // -- 以下、イベントハンドラ

        private void MainDialog_SizeChanged(object sender, EventArgs e)
        {
            // 画面のフルスクリーン化/ウィンドゥ化がなされたので、OnPaintが呼び出されるようにする。
            Invalidate();
        }

        private void MainDialog_Resize(object sender, EventArgs e)
        {
            // 画面がリサイズされたときにそれに収まるように盤面を描画する。
            FitToClientSize();
            Invalidate();
        }

        // -- 以下、ToolStripのハンドラ

        /// <summary>
        /// 待ったボタンが押されたときのハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            var config = TheApp.app.config;
            config.BoardReverse = !config.BoardReverse;
        }

    }
}
