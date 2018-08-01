using System;
using System.Windows.Forms;
using MyShogi.Model.Common;

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
                    return MessageBox.Show(mainForm, text, caption, buttons , icon);
                });

                if (mainForm.InvokeRequired)
                {
                    var result = mainForm.BeginInvoke(new Func<DialogResult>(() => { return show(); }));
                    return (DialogResult)mainForm.EndInvoke(result); // これで結果が返るまで待つはず..
                }
                else
                    return show();
            }
            else
                return MessageBox.Show(text, caption, buttons , icon);
        }

        /// <summary>
        /// 例外をダイアログで表示する用。
        /// </summary>
        /// <param name="ex"></param>
        public void MessageShow(Exception ex)
        {
            MessageShow("例外が発生しましたので終了します。\r\n例外内容 : " + ex.Message + "\r\nスタックトレース : \r\n" + ex.StackTrace,
                MessageShowType.Error);
            ApplicationExit();
        }

        public void ApplicationExit()
        {
            // 検討ウィンドウがあると閉じるのを阻害する。(window closingに対してCancelしているので)
            // 検討ウィンドウはメインウインドウにぶら下がっているはずなので、メインウインドウを終了させてしまう。

            Exiting = true; // このフラグを検討ウィンドウから見に来ている。

            Application.Exit(); // 終了させてしまう。
        }

        /// <summary>
        /// App.ApplicationExit()が呼び出された時のフラグ
        /// </summary>
        public bool Exiting;

    }
}
