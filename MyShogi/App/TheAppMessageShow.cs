using System;
using System.Windows.Forms;
using MyShogi.Model.Common.Utility;

namespace MyShogi.App
{
    /// <summary>
    /// TheAppの、MessageShow()まわりだけを切り離した。
    /// </summary>
    public partial class TheApp
    {
        /// <summary>
        /// 最前面に来るようにしてMessageBox.Show(text)を呼び出す。
        /// </summary>
        /// <param name="text"></param>
        public DialogResult MessageShow(string text, MessageShowType type)
        {
            var caption = type.Pretty();
            var icon = type.ToIcon();
            var buttons = type.ToButtons();
            
            if (mainForm != null && mainForm.IsHandleCreated && !mainForm.IsDisposed)
            {
                var show = new Func<DialogResult>(() =>
                {
                    return MessageBox.Show(mainForm, text, caption, buttons , icon );
                });

                if (mainForm.InvokeRequired)
                {
                    try
                    {
                        var result = mainForm.BeginInvoke(new Func<DialogResult>(() => { return show(); }));
                        return (DialogResult)mainForm.EndInvoke(result); // これで結果が返るまで待つはず..
                        // ここでウィンドウが破棄される可能性があるのでEndInvoke()が成功するとは限らない。
                    } catch
                    {
                        return DialogResult.OK;
                    }
                }
                else
                    return show();
            }
            else
                return MessageBox.Show(text, caption, buttons , icon );
        }

        /// <summary>
        /// 例外をダイアログで表示する用。
        /// </summary>
        /// <param name="ex"></param>
        public void MessageShow(Exception ex , bool exit = true)
        {
            if (exit)
            {
                MessageShow("例外が発生しましたので終了します。\r\n例外内容 : " + ex.Message + "\r\nスタックトレース : \r\n" + ex.StackTrace,
                    MessageShowType.Error);
                ApplicationExit();
            } else
            {
                MessageShow("例外が発生しました。\r\n例外内容 : " + ex.Message + "\r\nスタックトレース : \r\n" + ex.StackTrace,
                    MessageShowType.Error);
            }
        }

        public void ApplicationExit()
        {
            // 検討ウィンドウがあると閉じるのを阻害する。(window closingに対してCancelしているので)
            // 検討ウィンドウはメインウインドウにぶら下がっているはずなので、メインウインドウを終了させてしまう。

            Exiting = true; // このフラグを検討ウィンドウから見に来ている。

            Application.Exit(); // 終了させてしまう。
        }

        /// <summary>
        /// App.ApplicationExit()が呼び出されたあとであるかのフラグ
        /// </summary>
        public bool Exiting { get; private set; }

        /// <summary>
        /// メインのForm
        /// これがないとMessageBox.Show()などで親を指定できなくて困る。
        /// </summary>
        private Form mainForm { get; set; }

        /// <summary>
        /// UI threadで実行したい時にこれを用いる。
        /// </summary>
        /// <param name="action"></param>
        public void UIThread(Action action)
        {
            var a = new Action(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    MessageShow(ex);
                }
            });
            if (mainForm == null)
                a();
            else
                mainForm.BeginInvoke(a);
        }

    }
}
