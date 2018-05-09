using System.Text;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 盤面を表現するクラス
    /// </summary>
    public class Position
    {
        // -------------------------------------------------------------------------

            /// <summary>
        /// 盤面、81升分の駒 + 1
        /// </summary>
        private Piece[] board = new Piece[Square.NB_PLUS1.ToInt()];

        /// <summary>
        /// 手駒
        /// </summary>
        private Hand[] hand = new Hand[Color.NB.ToInt()];

        /// <summary>
        /// 手番
        /// </summary>
        private Color sideToMove = Color.BLACK;

        /// <summary>
        /// 玉の位置
        /// </summary>
        private Square[] kingSquare = new Square[Color.NB.ToInt()];

        // 初期局面からの手数(初期局面 == 1)
        private int gamePly = 1;

        // -------------------------------------------------------------------------

        /// <summary>
        /// 盤面を初期化する
        /// </summary>
        public void init()
        {
            //sideToMove = Color.BLACK;
            //gamePly = 1;
        }

        /// <summary>
        /// 盤面を日本語形式で出力する。
        /// </summary>
        /// <returns></returns>
        public string Pretty()
        {
            // あとで実装する
            return "";
        }

        /// <summary>
        /// USI形式で盤面を出力する。
        /// </summary>
        /// <returns></returns>
        public string ToUSI()
        {
            // 書きかけ。あとで書く。

            var sb = new StringBuilder();
            sb.Append(sideToMove.ToUSI());
            sb.Append(gamePly.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// 指し手で盤面を1手進める
        /// </summary>
        /// <param name="move"></param>
        public void DoMove(Move move)
        {

        }

        /// <summary>
        /// 指し手で盤面を1手戻す
        /// </summary>
        /// <param name="move"></param>
        public void UndoMove(Move move)
        {

        }
    }
}
