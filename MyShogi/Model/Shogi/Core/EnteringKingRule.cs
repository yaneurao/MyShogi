namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 入玉ルール
    /// </summary>
    public enum EnteringKingRule
    {
        NONE        , // 入玉ルールなし
        POINT24     , // 24点法(31点以上で宣言勝ち)
        POINT27     , // 27点法 == CSAルール
        TRY_RULE    , // トライルール(敵陣の(先手から見て)51の升に自玉が到達して、王手がかかっていなければ勝ち)
    }
}
