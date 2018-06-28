namespace MyShogi.View.Win2D
{
    /// <summary>
    /// メインダイアログのToolStripについているボタンに対応する定数
    /// ボタン番号で呼ぶのはわかりにくいので名前をつける。
    /// </summary>
    public enum MainDialogButtonEnum
    {
        RESIGN    , // 投了ボタン
        UNDO_MOVE , // 待った
        MOVE_NOW  , // 急いで指させる
        INTERRUPT , // 中断

        REWIND    , // ◁ボタン
        FORWARD   , // ▷ボタン
        MAIN_BRANCH , // 本譜
    }
}
