using MyShogi.Model.Shogi.Core;
using System.Linq;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜本体。
    /// 分岐棋譜の管理。
    /// 現在の局面の管理。
    /// </summary>
    public class KifuTree
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KifuTree()
        {
            currentNode = rootNode;
        }

        /// <summary>
        /// 現在の局面を表現している
        /// </summary>
        public Position pos = new Position();

        /// <summary>
        /// 棋譜の初手の指し手。これを数珠つなぎに、樹形図状に持っている。
        /// </summary>
        public KifuNode rootNode = new KifuNode(null);

        /// <summary>
        /// posの現在の局面に対応するKifuNode
        /// </summary>
        public KifuNode currentNode;

        // -------------------------------------------------------------------------
        // 局面に対する操作子
        // -------------------------------------------------------------------------

        // DoMove(),UndoMove()以外はcurrentNode.movesに自分で足すなり引くなりすれば良い

        /// <summary>
        /// posの現在の局面から指し手mで進める。
        /// mは、currentNodeのもつ指し手の一つであるものとする
        /// </summary>
        /// <param name="m"></param>
        public void DoMove(KifuMove m)
        {
            pos.DoMove(m.nextMove);
            currentNode = m.nextNode;
        }

        /// <summary>
        /// posを1手前の局面に移動する
        /// </summary>
        public void UndoMove()
        {
            pos.UndoMove();
            currentNode = currentNode.prevNode;
        }

        /// <summary>
        /// 現在の局面(currentMove)に対して、指し手moveが登録されていないなら、その指し手を追加して
        /// posをその指し手で進める。すでに存在しているなら、その指し手は追加しない。
        /// </summary>
        /// <param name="move"></param>
        public void DoMove(Move move)
        {
            var m = currentNode.moves.FirstOrDefault((x)=>x.nextMove == move);
            if (m == null)
            {
                // -- 見つからなかったので次のnodeを追加してやる

                KifuNode nextNode = new KifuNode(currentNode);
                currentNode.moves.Add(new KifuMove(move,nextNode));
                currentNode = nextNode;
            } else
            {
                currentNode = m.nextNode;
            }

            pos.DoMove(move);
        }

    }
}
