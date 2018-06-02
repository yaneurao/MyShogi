using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Resource;
using MyShogi.Model.Shogi.Core;
using MyShogi.ViewModel;
using ShogiCore = MyShogi.Model.Shogi.Core;
using SPRITE = MyShogi.Model.Resource.SpriteManager;

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

            ViewInstance = new MainDialogViewInstance();
            ViewInstance.Init(this);

            UpdateMenuItems();

            FitToScreenSize();
            FitToClientSize();

            MinimumSize = new Size(192*2 , 108*2 + menu_height );
        }

        /// <summary>
        /// このViewに対応するViewModel
        /// このクラスをnewした時にViewModelのインスタンスと関連付ける。
        /// </summary>
        public MainDialogViewModel ViewModel
        {
            get { return ViewInstance.ViewModel; }
            set { ViewInstance.ViewModel = value; }
        }

        /// <summary>
        /// 描画のときに必要となる、Viewに属する情報
        /// 1つのViewInstanceと1つのViewModelが対応する。
        /// </summary>
        public MainDialogViewInstance ViewInstance { get; private set; }

        /// <summary>
        /// 対局盤面の描画関係のコード一式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_Paint(object sender, PaintEventArgs e)
        {
            // 将来的には複数viewに対応させる。
            // ここではDrawSprite()とDrawString()だけで描画を完結させてあるので複数Viewへの対応は(描画だけなら)容易。

            var app = TheApp.app;
            var config = app.config;
            this.graphics = e.Graphics; // DrawSprite(),DrawString()のために必要
            var vm = ViewModel;

            // 盤面を反転させて描画するかどうか
            var reverse = config.BoardReverse;

            // -- 盤面の描画
            {
                // 座標系はストレートに指定しておけばaffine変換されて適切に描画される。
                DrawSprite( new Point(0,0) , SPRITE.Board());
            }

            // -- 駒の描画
            {
                // -- 盤上の駒

                // 描画する局面
                var pos = ViewModel.Pos;

                // 最終手(初期盤面などでは存在せず、lastMove == Move.NONEであることに注意)
                var lastMove = pos.State().lastMove;
                // 最終手の移動元の升
                var lastMoveFrom = (lastMove != ShogiCore.Move.NONE && !lastMove.IsDrop()) ? lastMove.From() : Square.NB;
                // 最終手の移動先の升
                var lastMoveTo = (lastMove != ShogiCore.Move.NONE) ? lastMove.To() : Square.NB;

                for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
                {
                    var pc = pos.PieceOn(sq);
                    var dest = PieceLocation((SquareHand)sq);

                    // これが最終手の移動元の升であるなら、エフェクトを描画する必要がある。
                    if (sq == lastMoveFrom)
                        DrawSprite(dest , SPRITE.PieceMove(1));

                    // これが最終手の移動先の升であるなら、エフェクトを描画する必要がある。
                    if (sq == lastMoveTo)
                        DrawSprite(dest , SPRITE.PieceMove(0));
                    
                    // 盤面反転モードなら、駒を先後入れ替えて描画する。
                    DrawSprite(dest , SPRITE.Piece(reverse ? pc.Inverse() : pc));
                }

                // -- 手駒の描画

                for (var c = ShogiCore.Color.ZERO; c < ShogiCore.Color.NB; ++c)
                {
                    Hand h = pos.Hand(c);

                    // 枚数によって位置が自動調整されるの、わりと見づらいので嫌。
                    // 駒種によって位置固定で良いと思う。

                    //同種の駒が3枚以上になったときに、その駒は1枚だけを表示して、
                    //数字を右肩表示しようと考えていたのですが、例えば、金が2枚、
                    //歩が3枚あるときに、歩だけが数字表示になるのはどうもおかしい気が
                    //するのです。2枚以上は全部数字表示のほうが良いだろう。

                    foreach (var pc in hand_piece_list)
                    {
                        int count = h.Count(pc);
                        if (count != 0)
                        {
                            // この駒の描画されるべき位置を求めるためにSquareHand型に変換する。
                            var piece = Util.ToSquareHand(c, pc);
                            var dest = PieceLocation(piece);

                            // 物理画面で後手側の駒台への描画であるか(駒を180度回転さて描画しないといけない)
                            var is_white_in_display = (c == ShogiCore.Color.WHITE) ^ reverse;

                            // 駒の描画
                            DrawSprite(dest, SPRITE.Piece(is_white_in_display ? pc.Inverse() : pc));

                            // 数字の描画(枚数が2枚以上のとき)
                            if (count >= 2)
                                DrawSprite(dest + hand_number_offset, SPRITE.HandNumber(count));
                        }
                    }

                }
            }

            // -- 盤の段・筋を表す数字の表示
            {
                var version = config.BoardNumberImageVersion;
                DrawSprite(board_number_pos[0], SPRITE.BoardNumberFile(reverse));
                DrawSprite(board_number_pos[1], SPRITE.BoardNumberRank(reverse));
            }

            // -- 対局者氏名
            {
                // 氏名の描画は通常状態の駒台表示の場合のみ
                if (config.KomadaiImageVersion == 1)
                {
                    DrawString(name_plate[reverse ? 1 : 0], vm.Player1Name , 28);
                    DrawString(name_plate[reverse ? 0 : 1], vm.Player2Name , 28);
                }
            }

        }

        /// <summary>
        /// sqの描画する場所を得る。
        /// Config.BoardReverseも反映されている。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        private Point PieceLocation(SquareHand sq)
        {
            var reverse = TheApp.app.config.BoardReverse;
            var color = sq.PieceColor();
            Point dest;

            if (color == ShogiCore.Color.NB)
            {
                // 盤面の升
                Square sq2 = reverse ? ((Square)sq).Inv() : (Square)sq;
                int f = 8 - (int)sq2.ToFile();
                int r = (int)sq2.ToRank();

                dest = new Point(board_location.X + piece_img_size.Width * f, board_location.Y + piece_img_size.Height * r);
            } else
            {
                if (reverse)
                    color = color.Not();

                var v = (TheApp.app.config.KomadaiImageVersion == 1) ? 0 : 1;

                var pc = sq.ToPiece();

                if (color == ShogiCore.Color.BLACK)
                    // Point型同士の加算は定義されていないので、第二項をSize型にcastしている。
                    dest = hand_table_pos[v,(int)color] + (Size)hand_piece_pos[v,(int)pc - 1];
                else
                    // 180度回転させた位置を求める
                    // 後手も駒の枚数は右肩に描画するのでそれに合わせて左右のmarginを調整する。
                    dest = new Point(
                        hand_table_pos[v,(int)color].X + (hand_table_size[v].Width  - hand_piece_pos[v,(int)pc - 1].X - piece_img_size.Width - 10),
                        hand_table_pos[v,(int)color].Y + (hand_table_size[v].Height - hand_piece_pos[v,(int)pc - 1].Y - piece_img_size.Height+  0)
                    );
            }

            return dest;
        }

        /// <summary>
        /// 盤面がクリックされたときに呼び出されるハンドラ
        /// 座標系は、affine変換(逆変換)して、盤面座標系(0,0)-(board_img_width,board_image_height)になっている。
        /// </summary>
        /// <param name="p"></param>
        private void BoardClick(Point p)
        {
            SquareHand sq = BoardAxisToSquare(p);
        }

        /// <summary>
        /// 盤面がドラッグされたときに呼び出されるハンドラ
        /// 座標系は、affine変換(逆変換)して、盤面座標系(0,0)-(board_img_width,board_image_height)になっている。
        /// </summary>
        /// <param name="p1">ドラッグ開始点</param>
        /// <param name="p2">ドラッグ終了点</param>
        private void BoardDrag(Point p1,Point p2)
        {
            SquareHand sq1 = BoardAxisToSquare(p1);
            SquareHand sq2 = BoardAxisToSquare(p2);

        }

        /// <summary>
        /// 盤面座標系から、それを表現するSquareに変換する。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        SquareHand BoardAxisToSquare(Point p)
        {
            var config = TheApp.app.config;

            // 盤上の升かどうかの判定
            var board_rect = new Rectangle(board_location.X , board_location.Y  , piece_img_size.Width  * 9 ,  piece_img_size.Height * 9);
            if (board_rect.Contains(p))
            {
                var sq = (Square)(((p.X - board_location.X) / piece_img_size.Width) * 9 + ((p.Y - board_location.Y) / piece_img_size.Height));
                if (config.BoardReverse)
                    sq = sq.Inv();

                return (SquareHand)sq;
            }
            else
            {
                // 手駒かどうかの判定
                // 細長駒台があるのでわりと面倒。何も考えずに描画位置で判定する。

                for (var sq = SquareHand.Hand; sq < SquareHand.HandNB; ++sq)
                    if (new Rectangle(PieceLocation(sq), piece_img_size).Contains(p))
                        return sq;
            }

            // not found
            return SquareHand.NB;
        }

    }
}
