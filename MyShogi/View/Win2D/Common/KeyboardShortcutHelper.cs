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
            KeyDownPrivate(sender, e);

            // これをしておかないとキーがこのあとListViewなどによって処理されてしまう。
            // 特にSpaceキーだとListViewはFocusのある場所(これは現在の選択行とは異なる)に移動してしまう。
            // この動作をキャンセルさせる必要がある。
            e.SuppressKeyPress = true;

            // ハンドルされなかった場合も、これ以上キーイベントを下位のControlでされると困るので
            // 処理済みとして扱う。
            e.Handled = true;
        }

        /// <summary>
        /// KeyDown()の下請け
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyDownPrivate(object sender, KeyEventArgs e)
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
        }

        /// <summary>
        /// カーソルキー入力がControlに食われてしまうのでその回避策
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        public bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
//         if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN) ...


            var e = new KeyEventArgs(keyData);
            KeyDownPrivate(null, e);

            return e.Handled;
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
