using System.Collections.Generic;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜の表現
    /// ある局面での指し手、本譜の手順、etc..
    /// </summary>
    public class KifuNode
    {
        public KifuNode(KifuNode prevNode_)
        {
            prevNode = prevNode_;
        }

        /// <summary>
        /// この局面での指し手(分岐があるので複数ある)
        /// moves[0]が本譜の手順。
        /// movesの指し手は重複していないものとする。
        /// </summary>
        public List<KifuMove> moves = new List<KifuMove>();

        /// <summary>
        /// 本譜の手順のindex
        /// moves[selectedKifuMoveIndex]が本譜(現在棋譜ウィンドウに表示されている)の手順
        /// moves.Count!=0のときにおいて、moves[selectedKifuMoveIndex]が合法であることは保証されているものとする。
        /// </summary>
        public int selectedKifuMoveIndex;

        /// <summary>
        /// 本譜の手順
        /// </summary>
        public KifuMove selectedKifuMove
        {
            get { return (moves.Count == 0) ? null : moves[selectedKifuMoveIndex]; }
        }

        /// <summary>
        /// 一手前のnode
        /// </summary>
        public KifuNode prevNode;

        /// <summary>
        /// この局面に対する棋譜コメント
        /// rootNode(開始局面)に対するコメントもここに。
        /// </summary>
        public string comment;

    }
}
