using System;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 思考ログなどのプロパティ集合
    /// </summary>
    public class KifuLog
    {
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
