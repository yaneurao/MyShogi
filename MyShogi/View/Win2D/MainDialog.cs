using MyShogi.App;
using MyShogi.Model.Shogi.Core;
using MyShogi.ViewModel;
using System;
using System.Drawing;
using System.Windows.Forms;
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
        /// このViewに対応するViewModel
        /// このクラスをnewした時にViewModelのインスタンスと関連付ける。
        /// </summary>
        public MainDialogViewModel ViewModel { get; set; }

        /// <summary>
        /// 対局盤面の描画関係のコード一式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                var lastMoveFrom = (lastMove != ShogiCore.Move.NONE && !lastMove.IsDrop()) ? lastMove.From() : Square.NB;
                var lastMoveTo = (lastMove != ShogiCore.Move.NONE) ? lastMove.To() : Square.NB;

                var piece = img.PieceImg.image;
                var piece_move_img = img.PieceMoveImg.image;

                for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
                {
                    var pc = pos.PieceOn(sq);

                    // 盤面反転モードなら、盤面を180度回した升から取得
                    Square sq2 = reverse ? sq.Inv() : sq;
                    int f = 8 - (int)sq2.ToFile();
                    int r = (int)sq2.ToRank();

                    var dstRect = new Rectangle(
                        board_left + piece_img_width * f, board_top + piece_img_height * r,
                        piece_img_width, piece_img_height);

                    // これが最終手の移動元の升であるなら、最終手として描画する必要がある。
                    if (sq == lastMoveFrom)
                        Draw(g, piece_move_img, dstRect, PieceRect((Piece)1));

                    // これが最終手の移動先の升であるなら、最終手として描画する必要がある。
                    if (sq == lastMoveTo)
                        Draw(g, piece_move_img, dstRect, PieceRect((Piece)0));

                    if (pc != Piece.NO_PIECE)
                    {
                        // 盤面反転モードなら、駒を先後入れ替える
                        if (reverse)
                            pc = pc ^ Piece.WHITE;

                        Draw(g, piece, dstRect, PieceRect(pc));
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

                            Draw(g, piece, dstRect, PieceRect(pc));

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

        /// <summary>
        /// 駒画像に対して、pcを指定して、その駒の転送元矩形を返す
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public Rectangle PieceRect(Piece pc)
        {
            return new Rectangle(
                ((int)pc % 8) * piece_img_width,
                ((int)pc / 8) * piece_img_height,
                piece_img_width, piece_img_height);
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

    }
}
