using MyShogi.Model.Common.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class DebugWindow : Form
    {
        public DebugWindow(MemoryLog log)
        {
            InitializeComponent();

            var ListAdded_ = new ListAddedEventHandler(sender =>
            {
                // UIスレッドからの呼び出しを保証する。
                if (InvokeRequired)
                    Invoke(new Action(() => ListAdded(sender)));
                else
                    ListAdded(sender);
            });

            log.ListAdded += ListAdded_;
            FormClosed += (sender, args) => { log.ListAdded -= ListAdded_; };

            memoryLog = log;

            UpdateListBox(log.LogList);
        }

        private MemoryLog memoryLog;

        /// <summary>
        /// [UI thread] : Form.ClientSizeChanged , Load イベントに対するハンドラ。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugWindow_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateControlsPosition();
        }

        /// <summary>
        /// [UI thread] : Controlの再配置を行う。
        /// </summary>
        private void UpdateControlsPosition()
        {
            // dockしてないのでTextBoxなどを自前で再配置する。

            if (Height == 0)
                return;

            int w = ClientSize.Width;
            int h = ClientSize.Height;

            // -- textBox1

            // TextBoxの高さは変更しない。横幅をいっぱいにして縦方向は下にくっつける。
            var t_loc = textBox1.Location;
            var t_height = textBox1.Size.Height;

            textBox1.Location = new Point(t_loc.X, h - t_height - 3);
            textBox1.Size = new Size(w - t_loc.X - 3, t_height);

            // -- label1

            var l_loc = label1.Location;
            label1.Location = new Point(l_loc.X, h - t_height - 3);

            // -- listBox1

            listBox1.Size = new Size(w - 3, h - t_height - 8);
        }

        /// <summary>
        /// [UI thread] : Logが追加された時の描画ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        private void ListAdded(object sender)
        {
            lastLogList = sender as List<string>;
            var lastLine = lastLogList[lastLogList.Count - 1]; // 今回追加になった行

            // 最後の行だけ追加されたはずであるから、その部分だけを差分更新してやる。

            var filter = textBox1.Text;
            string appendLine = null;

            if (string.IsNullOrEmpty(filter))
                appendLine = lastLine;
            else
            {
                // filter条件にマッチする時だけ追加してやる。
                try
                {
                    var regex = new Regex(filter);
                    if (regex.Match(lastLine).Success)
                        appendLine = lastLine;
                } catch {
                    listBox1.Items.Clear();
                    listBox1.Items.Add("-- filterで指定されている正規表現の表記に誤りがあります。 -- ");
                    return;
                }
            }
            if (appendLine != null)
            {
                listBox1.Items.Add(appendLine);
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }

        }

        /// <summary>
        /// [UI thread] : filter条件が変更になった時のハンドラ
        /// </summary>
        /// <param name="sender"></param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // 条件が変わったので再描画する。
            UpdateListBox(lastLogList);
        }

        /// <summary>
        /// [UI thread] : 現在のテキストボックスの内容に従ってListBoxにlogを反映させる。
        /// </summary>
        private void UpdateListBox(List<string> log_list)
        {
            if (log_list == null)
                return;
            lastLogList = log_list;

            // listBox1に表示する内容まとめて生成して、まとめてセットする。
            List<string> list;
            var filter = textBox1.Text;
            if (string.IsNullOrEmpty(filter))
                list = log_list;
            else
            {
                list = new List<string>();
                try
                {
                    var regex = new Regex(textBox1.Text);
                    foreach (var t in log_list)
                        if (regex.Match(t).Success)
                            list.Add(t);
                } catch
                {
                    list.Add("-- filterで指定されている正規表現の表記に誤りがあります。 -- ");
                }
            }

            // 丸ごと更新してみる。
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            listBox1.Items.AddRange(list.ToArray());
            listBox1.TopIndex = listBox1.Items.Count - 1;
            listBox1.EndUpdate();
        }

        /// <summary>
        /// 最後に渡されたUpdateListBoxの引数
        /// </summary>
        private List<string> lastLogList;

    }
}
