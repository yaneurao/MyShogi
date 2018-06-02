using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Shogi.Core;
using ShogiCore = MyShogi.Model.Shogi.Core;
using SPRITE = MyShogi.Model.Resource.SpriteManager;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局画面を表現するクラス
    /// 
    /// 描画が完全に抽象化されているので、
    /// 一つのMainDialogが複数のGameScreenを持つことが出来る。
    /// </summary>
    public partial class GameScreen
    {
        /// <summary>
        /// 初期化
        /// 親フォームにこのインスタンスの持つControlを関連付けておく。
        /// </summary>
        /// <param name="parent"></param>
        public void Init(Form parent)
        {
            ViewModel.kifuControl = new KifuControl();
            parent.Controls.Add(ViewModel.kifuControl);
        }

        /// <summary>
        /// この対局盤面の描画のために必要となるViewModel
        /// 別クラスになってはいるが、GameScreenと1:1で対応するので、GameScreenの生成と同時に生成している。
        /// </summary>
        public GameScreenViewModel ViewModel { get; private set; } = new GameScreenViewModel();

        /// <summary>
        /// 描画時に呼び出される。
        /// 対局盤面を描画する。
        /// </summary>
        public void OnDraw(Graphics g_)
        {
            graphics = g_;
            /// 以降、このクラスのDrawSprite(),DrawString()は正常にaffine変換されて描画されるものとする。

            // ここではDrawSprite()とDrawString()だけで描画を完結させてあるので複数Viewへの対応は(描画だけなら)容易。

            var app = TheApp.app;
            var config = app.config;
            var vm = ViewModel.ViewModel; // MainDialogViewModel

            // 盤面を反転させて描画するかどうか
            var reverse = config.BoardReverse;

            // -- 盤面の描画
            {
                // 座標系はストレートに指定しておけばaffine変換されて適切に描画される。
                DrawSprite(new Point(0, 0), SPRITE.Board());
            }

            // -- 駒の描画
            {
                // -- 盤上の駒

                // 描画する局面
                var pos = ViewModel.ViewModel.Pos;

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
                        DrawSprite(dest, SPRITE.PieceMove(1));

                    // これが最終手の移動先の升であるなら、エフェクトを描画する必要がある。
                    if (sq == lastMoveTo)
                        DrawSprite(dest, SPRITE.PieceMove(0));

                    // 盤面反転モードなら、駒を先後入れ替えて描画する。
                    DrawSprite(dest, SPRITE.Piece(reverse ? pc.Inverse() : pc));
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
                    DrawString(name_plate[reverse ? 1 : 0], vm.Player1Name, 28);
                    DrawString(name_plate[reverse ? 0 : 1], vm.Player2Name, 28);
                }
            }

        }

    }

}
