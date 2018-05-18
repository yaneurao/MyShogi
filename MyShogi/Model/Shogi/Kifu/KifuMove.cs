using System;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// KifuTreeで用いる指し手
    /// </summary>
    public class KifuMove
    {
        public KifuMove(Move nextMove_,KifuNode nextNode_ , TimeSpan thinkingTime_)
        {
            nextMove = nextMove_;
            nextNode = nextNode_;
            thinkingTime = thinkingTime_;
        }

        /// <summary>
        /// 次の指し手
        /// </summary>
        public Move nextMove;

        /// <summary>
        /// 着手に要した時間(計測)
        /// ミリ単位まで保持している。
        /// 表示時や出力時に秒単位で繰り上げる。
        /// </summary>
        public TimeSpan thinkingTime;

        /// <summary>
        /// timeから秒を繰り上げた時間
        /// 表示時や棋譜ファイルへの出力時は、こちらを用いる
        /// </summary>
        public TimeSpan RoundTime
        {
            get
            {
                // ミリ秒が端数があれば、秒単位で繰り上げる。
                return (thinkingTime.Milliseconds == 0) ? thinkingTime
                                                        : thinkingTime.Add(new TimeSpan(0,0,0,0,1000 - thinkingTime.Milliseconds));
            }
        }

        /// <summary>
        /// nextMoveを指したときの次の局面
        /// </summary>
        public KifuNode nextNode;
    }
}
