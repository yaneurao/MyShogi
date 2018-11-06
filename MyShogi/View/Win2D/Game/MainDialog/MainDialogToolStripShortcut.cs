using System;
using System.Diagnostics;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局盤面などがあるメインウインドウ
    ///
    /// メインメニューにぶら下がっているToolStripのボタンのショートカットハンドラの初期化
    /// </summary>
    public partial class MainDialog : Form
    {
        /// <summary>
        /// メインウインドウにぶら下がっているToolTipのShortcutKeyを設定する。
        /// </summary>
        public void UpdateToolStripShortcut(PropertyChangedEventArgs args = null)
        {
            var config = TheApp.app.Config;
            var shortcut = TheApp.app.KeyShortcut;
            var addEvent = (Action<Action<KeyEventArgs>>)(TheApp.app.KeyShortcut.AddEvent2);
            shortcut.InitEvent2(); ; // このdelegateにShortcutキーのハンドラを登録していく。

            // 棋譜の1手戻る/進むキー
            switch (config.KifuWindowPrevNextKey)
            {
                case 0: // なし
                    break;
                case 1: // ←と→
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Left) { toolStripButton9.PerformClick(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Right) { toolStripButton10.PerformClick(); e.Handled = true; } });
                    break;
                case 2: // ↑と↓
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Up) { toolStripButton9.PerformClick(); e.Handled = true; } });
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Down) { toolStripButton10.PerformClick(); e.Handled = true; } });
                    break;

            }

            // 次の1手に移動する特殊キー
            // 使わないキー、殺しておかないとListViewが反応しかねない。
            switch (config.KifuWindowNextSpecialKey)
            {
                case 0: // なし
                    break;

                case 1: // スペースキー (EnterとSpaceキーは同時押しは使わないので単独で判定する)
                    addEvent( e => { if (/*e.Modifiers == Keys.None &&*/ e.KeyCode == Keys.Space) { toolStripButton10.PerformClick(); e.Handled = true; } });
                    break;

                case 2: // Enterキー
                    addEvent( e => { if (/*e.Modifiers == Keys.None &&*/ e.KeyCode == Keys.Enter) { toolStripButton10.PerformClick(); e.Handled = true; } });
                    break;
            }

            // 最初に戻る/最後に進むキー
            switch (config.KifuWindowFirstLastKey)
            {
                case 0: // なし
                    break;

                case 1: // ←と→
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Left) { toolStripButton12.PerformClick(); e.Handled = true; } });
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Right) { toolStripButton13.PerformClick(); e.Handled = true; } });
                    break;

                case 2: // ↑と↓ : デフォルト
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Up) { toolStripButton12.PerformClick(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Down) { toolStripButton13.PerformClick(); e.Handled = true; } });
                    break;

                case 3: // PageUpとPageDown
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp) { toolStripButton12.PerformClick(); e.Handled = true; } });
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown) { toolStripButton13.PerformClick(); e.Handled = true; } });
                    break;
            }

            // --- 検討ウインドウ

            var cons = engineConsiderationMainControl;
            Debug.Assert(cons != null);

            // 選択行の上下
            switch (config.ConsiderationWindowPrevNextKey)
            {
                case 0: // なし
                    break;

                case 1: // Shift←→ : デフォルト
                    addEvent( e => { if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Left  ) { cons.PerformUp(); e.Handled = true; } });
                    addEvent( e => { if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Right ) { cons.PerformDown(); e.Handled = true; } });
                    break;

                case 2: // Shift↑↓
                    addEvent(e => { if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Up   ) { cons.PerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Down ) { cons.PerformDown(); e.Handled = true; } });
                    break;

                case 3: // ←と→
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Left) { cons.PerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Right) { cons.PerformDown(); e.Handled = true; } });
                    break;

                case 4: // ↑と↓ 
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Up) { cons.PerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Down) { cons.PerformDown(); e.Handled = true; } });
                    break;

                case 5: // ，(カンマ)と ．(ピリオド)
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Oemcomma) { cons.PerformUp(); e.Handled = true; } });
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.OemPeriod) { cons.PerformDown(); e.Handled = true; } });
                    break;

                case 6: // PageUpとPageDown
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp) { cons.PerformUp(); e.Handled = true; } });
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown) { cons.PerformDown(); e.Handled = true; } });
                    break;
            }

            // 選択行の先頭/末尾へ
            switch (config.ConsiderationWindowHeadTailKey)
            {
                case 0: // なし
                    break;

                case 1: // Shift←→
                    addEvent(e => { if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Left ) { cons.PerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Right) { cons.PerformTail(); e.Handled = true; } });
                    break;

                case 2: // Shift↑↓ : デフォルト
                    addEvent(e => { if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Up   ) { cons.PerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Down ) { cons.PerformTail(); e.Handled = true; } });
                    break;

                case 3: // ←と→
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Left) { cons.PerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Right) { cons.PerformTail(); e.Handled = true; } });
                    break;

                case 4: // ↑と↓ 
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Up) { cons.PerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Down) { cons.PerformTail(); e.Handled = true; } });
                    break;

                case 5: // ，(カンマ)と ．(ピリオド)
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Oemcomma) { cons.PerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.OemPeriod) { cons.PerformTail(); e.Handled = true; } });
                    break;

                case 6: // PageUpとPageDown
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp) { cons.PerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown) { cons.PerformTail(); e.Handled = true; } });
                    break;
            }

            // ミニ盤面にPVを転送
            switch (config.ConsiderationPvSendKey)
            {
                case 0:
                    break;

                case 1: // Space
                    addEvent( e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Space) { cons.SendCurrentPvToMiniBoard(); e.Handled = true; } });
                    break;

                case 2: // Enter : デフォルト
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Enter) { cons.SendCurrentPvToMiniBoard(); e.Handled = true; } });
                    break;
            }

            // --- ミニ盤面
            
            // 選択行の上下
            switch (config.MiniBoardPrevNextKey)
            {
                case 0: // なし
                    break;

                case 1: // Ctrl←→ : デフォルト
                    addEvent(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Left) { cons.MiniBoardPerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Right  ) { cons.MiniBoardPerformDown(); e.Handled = true; } });
                    break;

                case 2: // Ctrl↑↓
                    addEvent(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Up   ) { cons.MiniBoardPerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Down ) { cons.MiniBoardPerformDown(); e.Handled = true; } });
                    break;

                case 3: // ←と→
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Left) { cons.MiniBoardPerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Right) { cons.MiniBoardPerformDown(); e.Handled = true; } });
                    break;

                case 4: // ↑と↓ 
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Up) { cons.MiniBoardPerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Down) { cons.MiniBoardPerformDown(); e.Handled = true; } });
                    break;

                case 5: // ，(カンマ)と ．(ピリオド)
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Oemcomma) { cons.MiniBoardPerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.OemPeriod) { cons.MiniBoardPerformDown(); e.Handled = true; } });
                    break;

                case 6: // PageUpとPageDown
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp) { cons.MiniBoardPerformUp(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown) { cons.MiniBoardPerformDown(); e.Handled = true; } });
                    break;
            }

            // 選択行の先頭/末尾へ
            switch (config.MiniBoardHeadTailKey)
            {
                case 0: // なし
                    break;

                case 1: // Ctrl←→
                    addEvent(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Left ) { cons.MiniBoardPerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Right) { cons.MiniBoardPerformTail(); e.Handled = true; } });
                    break;

                case 2: // Ctrl↑↓ : デフォルト
                    addEvent(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Up   ) { cons.MiniBoardPerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Down ) { cons.MiniBoardPerformTail(); e.Handled = true; } });
                    break;

                case 3: // ←と→
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Left) { cons.MiniBoardPerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Right) { cons.MiniBoardPerformTail(); e.Handled = true; } });
                    break;

                case 4: // ↑と↓ 
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Up) { cons.MiniBoardPerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Down) { cons.MiniBoardPerformTail(); e.Handled = true; } });
                    break;

                case 5: // ，(カンマ)と ．(ピリオド)
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.Oemcomma) { cons.MiniBoardPerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.OemPeriod) { cons.MiniBoardPerformTail(); e.Handled = true; } });
                    break;

                case 6: // PageUpとPageDown
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp) { cons.MiniBoardPerformHead(); e.Handled = true; } });
                    addEvent(e => { if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown) { cons.MiniBoardPerformTail(); e.Handled = true; } });
                    break;
            }

        }


    }
}
