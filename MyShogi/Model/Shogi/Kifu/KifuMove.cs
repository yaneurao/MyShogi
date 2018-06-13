using System;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// KifuTreeで用いる指し手
    /// </summary>
    public class KifuMove : KifuLog
    {
        public KifuMove(Move nextMove_,KifuNode nextNode_ , TimeSpan thinkingTime_ , TimeSpan totalTime_)
        {
            nextMove = nextMove_;
            nextNode = nextNode_;
            thinkingTime = thinkingTime_;
            totalTime = totalTime_;
        }

        /// <summary>
        /// 次の指し手
        /// 1. SpecialMove(Moveの定義を見よ)である可能性がある。
        /// 2. 非合法手である可能性がある。(その指し手を指して、負けたことを示すために)
        /// DoMove()する前に、Move.IsSpecial()とPosition.IsLegal()で判定すること。
        /// </summary>
        public Move nextMove;

        /// <summary>
        /// 着手に要した時間(計測)
        /// ミリ単位まで保持している。
        /// 表示時や出力時に秒単位で繰り上げる。
        /// </summary>
        public TimeSpan thinkingTime;

        /// <summary>
        /// この局面までの合計消費時間
        /// </summary>
        public TimeSpan totalTime;

        /// <summary>
        /// nextMoveを指したときの次の局面
        /// </summary>
        public KifuNode nextNode;

    }
}
