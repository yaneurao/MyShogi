using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public static class DockUtility
    {
        /// <summary>
        /// あるControlのDockStyleを変更し、Control自体のSizeも変更する。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="newSize">Controlに設定すべき新しいSize。nullならいまのまま</param>
        /// <param name="newLocationPtr">Controlに設定すべき新しいLocation。nullなら、いまのまま。</param>
        /// <param name="newStyle"></param>
        public static void Change(Control control , DockStyle newStyle , Size? newSizePtr , Point? newLocationPtr = null)
        {
            var newLocation = newLocationPtr == null ? control.Location : newLocationPtr.Value;
            var newSize = newSizePtr == null ? control.Size : newSizePtr.Value;

            // Noneにした瞬間にresizeイベントが発生するのだが、この時まだSizeが設定されていないので
            // 棋譜ウインドウがおかしいところに移動する。そこでDockStyleの設定前にSizeの設定が必要である。
            // しかしこれでも一瞬、ずれて表示される。画面の描画が完了するまで棋譜ウインドウのVisible = false
            // にしたいが、それはわりと難しい。これで我慢する。

#if MONO
            control.Size = Size.Empty;
            control.Dock = newStyle; // ここでLocationが変化するのでこのあと復元する。
            control.Size = newSize;

            // Monoだとなぜかresizeイベントが起きない。
            // DockStyle.Noneにする前のサイズと同じだからのようだ。
            // そこでわざとDockStyle.Noneの前でサイズを(0,0)にして、DockStyleを変更後にSizeを設定することによって
            // 強制的にresizeイベントを生起させる。

#else
            control.Size = newSize;
            control.Dock = newStyle; // ここでLocationが変化するのでこのあと復元する。
#endif

            control.Location = newLocation;
            // LocationをControl.Dockの変更前に設定するとDockへの代入で変化してしまう。
            // DockStyleの変更でLocationが変化することもあるので元に戻す。

        }
    }
}
