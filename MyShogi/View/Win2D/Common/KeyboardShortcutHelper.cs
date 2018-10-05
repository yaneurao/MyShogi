using System;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// メニューのショートカットキーは、サブウインドウに送られると効かない。(そちらがアクティブなので)
    /// サブウインドウをアクティブにしないようにするにはSetWindowPos APIの呼び出しが必要で移植性が下がるからやりたくない。
    /// (またサブウインドウでCtrl+Cなどが利かなくなる)
    ///
    /// ゆえに独自にサブウインドウ側でキーイベントをハンドルする必要がある。
    /// このクラスはそのための補助クラスである。
    /// </summary>
    public class KeyboardShortcutHelper
    {
        /// <summary>
        /// サブウインドウ側のKeyDownで、ショートカットキーを処理しない場合に、
        /// このハンドラを呼び出すと事前に登録されていたdelegateが呼び出される。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown?.Invoke(sender, e);
        }

        public Action<object,KeyEventArgs> OnKeyDown;

    }
}
