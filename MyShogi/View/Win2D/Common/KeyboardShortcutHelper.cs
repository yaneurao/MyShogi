using System;
using System.Collections.Generic;
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
            try
            {
                var list = new[] { OnKeyDown1, OnKeyDown2 };
                foreach (var onKeyList in list)
                    foreach (var onKey in onKeyList)
                    {
                        onKey.Invoke(sender, e);

                        // キーイベントが処理されたなら、そこで終了
                        if (e.Handled)
                            return;
                    }

                // 上記以外のキーは処理しない。
                e.Handled = true;
            } finally
            {
                // これをしておかないとキーがこのあとListViewなどによって処理されてしまう。
                // 特にSpaceキーだとListViewはFocusのある場所(これは現在の選択行とは異なる)に移動してしまう。
                // この動作をキャンセルさせる必要がある。
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// イベントリストその1(メインメニュー)を初期化
        /// </summary>
        public void InitEvent1()
        {
            OnKeyDown1.Clear();
        }

        /// <summary>
        /// イベントリストその2(ToolStrip)を初期化
        /// </summary>
        public void InitEvent2()
        {
            OnKeyDown2.Clear();
        }

        /// <summary>
        /// イベントリストその1にキーイベントを追加
        /// </summary>
        /// <param name="action"></param>
        public void AddEvent1(Action<object, KeyEventArgs> action)
        {
            OnKeyDown1.Add(action);
        }

        /// <summary>
        /// イベントリストその2にキーイベントを追加
        /// </summary>
        /// <param name="action"></param>
        public void AddEvent2(Action<object, KeyEventArgs> action)
        {
            OnKeyDown2.Add(action);
        }

        /// <summary>
        /// イベントリストその1
        /// メニューのアイテムのclickイベントを生起するためのショートカットキーハンドラ
        /// </summary>
        private List<Action<object, KeyEventArgs>> OnKeyDown1 = new List<Action<object, KeyEventArgs>>();

        /// <summary>
        /// イベントリストその2
        /// メインウインドウにぶら下がっているToolStripのボタンのclickイベントを生起するためのショートカットハンドラ
        /// </summary>
        private List<Action<object, KeyEventArgs>> OnKeyDown2 = new List<Action<object, KeyEventArgs>>();

    }
}
