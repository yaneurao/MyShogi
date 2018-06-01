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
                // 描画する盤面
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

                    // 盤面反転モードなら、盤面を180度回した升から取得
                    Square sq2 = reverse ? sq.Inv() : sq;
                    int f = 8 - (int)sq2.ToFile();
                    int r = (int)sq2.ToRank();

                    var dest = new Point(board_left + piece_img_width * f, board_top + piece_img_height * r);

                    // これが最終手の移動元の升であるなら、エフェクトを描画する必要がある。
                    if (sq == lastMoveFrom)
                        DrawSprite(dest , SPRITE.PieceMove(1));

                    // これが最終手の移動先の升であるなら、エフェクトを描画する必要がある。
                    if (sq == lastMoveTo)
                        DrawSprite(dest , SPRITE.PieceMove(0));
                    
                    if (pc != Piece.NO_PIECE)
                    {
                        // 盤面反転モードなら、駒を先後入れ替える
                        if (reverse)
                            pc = pc ^ Piece.WHITE;

                        DrawSprite(dest , SPRITE.Piece(pc));
                    }
                }

                // -- 手駒の描画

                // 駒台の種類によって描画場所が異なる
                var hand_location = config.KomadaiImageVersion == 1 ? hand_location1 : hand_location2;
                var hand_table = config.KomadaiImageVersion == 1 ? hand_table_pos1 : hand_table_pos2;
                var hand_table_width = config.KomadaiImageVersion == 1 ? hand_table_width1 : hand_table_width2;
                var hand_table_height = config.KomadaiImageVersion == 1 ? hand_table_height1 : hand_table_height2;

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

                            Point dest;

                            if (c == ShogiCore.Color.BLACK)
                                dest = new Point(
                                    hand_table[(int)c].X + loc.x,
                                    hand_table[(int)c].Y + loc.y);
                            else
                                // 180度回転させた位置を求める
                                // 後手も駒の枚数は右肩に描画するのでそれに合わせて左右のmarginを調整する。
                                dest = new Point(
                                    hand_table[(int)c].X + (hand_table_width  - loc.x - piece_img_width - 10),
                                    hand_table[(int)c].Y + (hand_table_height - loc.y - piece_img_height)
                                    );

                            // 駒の描画
                            DrawSprite(dest, SPRITE.Piece(pc));

                            // 数字の描画(枚数が2枚以上のとき)
                            if (count >= 2)
                            {
                                var dest2 = new Point(dest.X + 60, dest.Y + 20);
                                DrawSprite(dest2, SPRITE.HandNumber(count));
                            }
                        }
                    }

                }
            }

            // -- 盤の段・筋を表す数字の表示
            {
                var version = config.BoardNumberImageVersion;
                DrawSprite(new Point( 526, 32), SPRITE.BoardNumberFile(reverse));
                DrawSprite(new Point(1397, 49), SPRITE.BoardNumberRank(reverse));
            }

            // -- 対局者氏名
            {
                if (config.KomadaiImageVersion == 1)
                {
                    DrawString(name_plate[reverse ? 1 : 0], vm.Player1Name , 28);
                    DrawString(name_plate[reverse ? 0 : 1], vm.Player2Name , 28);
                }
            }

        }

    }
}
