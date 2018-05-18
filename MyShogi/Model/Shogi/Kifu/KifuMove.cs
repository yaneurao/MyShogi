using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// KifuTreeで用いる指し手
    /// </summary>
    public class KifuMove
    {
        public KifuMove(Move nextMove_,KifuNode nextNode_)
        {
            nextMove = nextMove_;
            nextNode = nextNode_;
        }

        /// <summary>
        /// 次の指し手
        /// </summary>
        public Move nextMove;

        /// <summary>
        /// nextMoveを指したときの次の局面
        /// </summary>
        public KifuNode nextNode;
    }
}
