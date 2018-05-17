using System;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 千日手であるかを表現する
    /// Position.IsRepetition()の返し値
    /// </summary>
    public enum RepetitionState : Int32
    {
        NONE = 0, // 千日手ではない
        DRAW = 1, // 千日手
        WIN  = 2, // 連続王手の千日手を相手が行った(ので手番側の勝ちの局面)
        LOSE = 3, // 連続王手の千日手を自分が行った(ので手番側の負けの局面)
    };
}
