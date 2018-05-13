using System;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 駒番号
    /// 盤上のどの駒がどこに移動したかを追跡するために用いる
    /// 1 ～ 40までの番号がついている。
    /// </summary>
    public enum PieceNo : Int32
    {
        // 駒がない場合
        NONE = 0,

        ZERO = 1, // これややこしいかな…。
        NB = 41,

        // 歩の枚数の最大
        PAWN_MAX = 18,
    }
}
