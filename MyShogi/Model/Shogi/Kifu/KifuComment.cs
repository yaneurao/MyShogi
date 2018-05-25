using System;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜につけるコメント、思考ログなどのプロパティ集合
    /// 指し手のみならず、rootの局面についてもこれがつけられる。
    /// </summary>
    public class KifuComment
    {
        /// <summary>
        /// この指し手に対する棋譜コメント
        /// 局面に対するコメントはKifuNodeに別に用意すべきだが、どう表示するかだとか、
        /// どう編集するかだとか色々難しい問題がある。
        /// 現実的には、指し手に対するコメントとrootNode(開始局面)に対するコメントぐらいがあれば
        /// 十分なのではないかと…。
        /// </summary>
        public string comment;

        /// <summary>
        /// この指し手に対する思考ログ(エンジンによって出力される)
        /// この指し手のための"go"コマンドによって最後に出力された PV , 評価値など
        /// </summary>
        public string engineComment;

        /// <summary>
        /// この指し手を指した時刻(書きたければ)
        /// rootの局面では対局開始日時が入る。
        /// </summary>
        public DateTime moveTime;
    }
}
