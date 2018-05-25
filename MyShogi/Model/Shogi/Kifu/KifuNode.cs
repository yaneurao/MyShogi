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
        /// 一手前のnode
        /// </summary>
        public KifuNode prevNode;
        
    }
}
