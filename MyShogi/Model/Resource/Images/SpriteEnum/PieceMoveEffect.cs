namespace MyShogi.Model.Resource.Images
{
    /// <summary>
    /// 駒の移動に関するエフェクトに関する定数
    /// SpriteManagerで用いる。
    /// </summary>
    public enum PieceMoveEffect
    {
        To         = 0 , // 最終手の移動先の升のエフェクト
        From       = 1 , // 最終手の移動元の升のエフェクト
        PickedFrom = 2 , // 掴んだ駒の移動元の升のエフェクト
        PickedTo   = 3 , // 掴んだ駒の移動先(以外)に適用されるエフェクト
    }
}
