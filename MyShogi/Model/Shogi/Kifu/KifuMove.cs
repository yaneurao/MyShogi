using System;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// KifuTreeで用いる指し手
    /// </summary>
    public class KifuMove : KifuLog
    {
        public KifuMove(Move nextMove_,KifuNode nextNode_ , KifuMoveTimes kifuMoveTimes_)
        {
            nextMove = nextMove_;
            nextNode = nextNode_;
            kifuMoveTimes = kifuMoveTimes_;
        }

        /// <summary>
        /// 次の指し手
        /// 1. SpecialMove(Moveの定義を見よ)である可能性がある。
        /// 2. 非合法手である可能性がある。(その指し手を指して、負けたことを示すために)
        /// DoMove()する前に、Move.IsSpecial()とPosition.IsLegal()で判定すること。
        /// </summary>
        public Move nextMove;

        /// <summary>
        /// 1手の消費時間、この時点での残り持ち時間、総消費時間など(両プレイヤー分)
        /// </summary>
        public KifuMoveTimes kifuMoveTimes;

        /// <summary>
        /// nextMoveを指したときの次の局面
        /// </summary>
        public KifuNode nextNode;

    }
}
