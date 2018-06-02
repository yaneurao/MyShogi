using System;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// MainDialogのユーザーの操作に対する状態を管理する定数
    /// 駒をマウスでクリックしている状態だとか、
    /// 成り・不成のダイアログを出している途中だとかを管理する
    /// </summary>
    public enum GameScreenViewStateEnum
    {
        Normal                , // 通常の状態
        PiecePickedUp         , // マウスで駒をクリックして駒を持ち上げている状態
        PromoteDialog         , // 成り・不成のダイアログを出している最中(駒の移動先の升の選択は完了している)
        CheckAlertDialog      , // 王手を回避していない警告ダイアログ(駒の移動先の升の選択は完了しているがそれは無効)
        RepetitionAlertDialog , // 連続王手の千日手局面に突入する警告ダイアログ(駒の移動先の升の選択は完了しているがそれは無効)
    }

    /// <summary>
    /// 状態とそれに付随する情報
    /// </summary>
    public class GameScreenViewState
    {
        public GameScreenViewState()
        {
            state = GameScreenViewStateEnum.Normal;
            picked_from = picked_to = SquareHand.NB;
        }

        /// <summary>
        /// MainDialogのViewの状態
        /// </summary>
        public GameScreenViewStateEnum state;

        // -- 以下、各状態のときに、それに付随する情報

        /// <summary>
        /// state == PiecePickedUpのときに掴んでいる駒の升(駒台の駒もありうる)
        /// </summary>
        public SquareHand picked_from;

        /// <summary>
        /// state == CheckAlertDialog、RepetitionAlertDialogのときに掴んでいる駒を移動させようとした升
        /// 駒台の駒はありえないが、picked_fromと同じ型にしておく。
        /// </summary>
        public SquareHand picked_to;

        /// <summary>
        /// あと何秒でダイアログが消えるかだとか、状態が遷移するだとか
        /// CheckAlertDialog , RepetitionAlertDialogのときに。
        /// </summary>
        public DateTime state_expired;
    }
}
