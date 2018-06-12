namespace MyShogi.View.Win2D
{
    /// <summary>
    /// メインダイアログのToolTipについているボタンに対応する定数
    /// ボタン番号で呼ぶのはわかりにくいので名前をつける。
    /// </summary>
    public enum MainDialogButtonEnum
    {
        RESIGN    , // 投了ボタン
        UNDO_MOVE , // 待った
        MOVE_NOW  , // 急いで指させる
        INTERRUPT , // 中断
    }
}
