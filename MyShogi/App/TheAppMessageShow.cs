using System;
using System.Windows.Forms;

namespace MyShogi.App
{
    /// <summary>
    /// TheApp.MessageShow()の引数で使う通知のタイプ。
    /// </summary>
    public enum MessageShowType
    {
        Information,  // 単なる通知
        Warning,      // 警告
        Error ,       // エラー
        Confirmation, // 確認
    }

    public static class MessageShowTypeExtension
    {
        /// <summary>
        /// MessageShowType型を綺麗な日本語文字列に変換
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Pretty(this MessageShowType type)
        {
            switch (type)
            {
                case MessageShowType.Information: return "通知";
                case MessageShowType.Warning: return "警告";
                case MessageShowType.Error: return "エラー";
                case MessageShowType.Confirmation: return "確認";
                default: return "";
            }
        }

        /// <summary>
        /// MessageShowType型を綺麗な日本語文字列に変換
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MessageBoxIcon ToIcon(this MessageShowType type)
        {
            switch (type)
            {
                case MessageShowType.Information: return MessageBoxIcon.Asterisk;
                case MessageShowType.Warning: return MessageBoxIcon.Exclamation;
                case MessageShowType.Error: return MessageBoxIcon.Hand;
                case MessageShowType.Confirmation: return MessageBoxIcon.Question; // このIcon、現在、非推奨なのか…。まあいいや。
                default: return MessageBoxIcon.None;
            }
        }
    }

    /// <summary>
    /// TheAppの、MessageShow()まわりだけを切り離した。
    /// </summary>
    public partial class TheApp
    {
        /// <summary>
        /// 最前面に来るようにしてMessageBox.Show(text)を呼び出す。
        /// </summary>
        /// <param name="text"></param>
        public DialogResult MessageShow(string text, MessageShowType type , MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            var caption = type.Pretty();
            var icon = type.ToIcon();

            if (mainForm != null && mainForm.IsHandleCreated && !mainForm.IsDisposed)
            {
                var show = new Func<DialogResult>(() =>
                {
                    return MessageBox.Show(mainForm, text, caption, buttons , icon);
                });

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() => { show(); }));
                    return DialogResult.OK; // すまん。futureで返すべきかも知れん。面倒見きれん。
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
