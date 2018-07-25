using System.Collections.Generic;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜の表現
    /// ある局面での指し手、本譜の手順、etc..
    ///
    /// 本譜の手順はmoves[0]である。
    /// 実際の対局で指された指し手は、moves[0]とswapして、moves[0]に持ってくるようにすること。
    ///
    /// 分岐棋譜の時は、そこに本譜の手順が格納されることが保証されているものとする。
    /// また、書き出しの時も同様で、棋譜ファイルには本譜の手順を1番目に書き出すこと。
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
        /// movesの指し手は重複を許さない。
        /// (対局後、途中の局面から再度対局を再開させた場合も同じ指し手ならば、そこを上書きしていく)
        /// </summary>
        public List<KifuMove> moves = new List<KifuMove>();

        /// <summary>
        /// 一手前のnode
        /// </summary>
        public KifuNode prevNode;

        /// <summary>
        /// この局面に対する棋譜コメント
        /// rootNode(開始局面)に対するコメントもここに。
        /// </summary>
        public string comment;

        public List<Usi.UsiThinkReportMessage> thinkmsgs = new List<Usi.UsiThinkReportMessage>();

        public List<Core.EvalValueEx> evalList = new List<Core.EvalValueEx>();

    }
}
