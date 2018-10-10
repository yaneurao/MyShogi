using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.Tool;

namespace MyShogi.View.Win2D
{
    public partial class DebugWindow : Form
    {
        /// <summary>
        /// デバッグ用に思考エンジンとやりとりしているログを画面上に表示するためのダイアログ。
        /// </summary>
        /// <param name="log"></param>
        public DebugWindow(MemoryLog log)
        {
            InitializeComponent();

            var ListAdded_ = new ListAddedEventHandler(args =>
            {
                try
                {
                    // UIスレッドからの呼び出しを保証する。
                    TheApp.app.UIThread(() => ListAdded(args));
                }
                catch { } // 終了間際だとInvoke()で例外が出るかもしれないので握りつぶしておく。
            });

            log.AddHandler(ListAdded_ , ref log_list);
            FormClosed += (sender, args) => { log.RemoveHandler(ListAdded_); };
            memory_log = log;

            UpdateListBox();

            // すぐに入力出来るようにフィルター用のテキストボックスにフォーカスを移動させておく。
            ActiveControl = textBox1;

            FontUtility.ReplaceFont(this, TheApp.app.Config.FontManager.DebugWindow);
        }

        /// <summary>
        /// コンストラクタの引数で渡された出力先のログクラス
        /// </summary>
        private MemoryLog memory_log;

        /// <summary>
        /// 保持しているログの内容。
        /// </summary>
        private Queue<string> log_list;

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

            // TextBoxの高さは変更しない。横幅をボタンにつかない範囲でいっぱいにして縦方向は下にくっつける。
            var t_loc = textBox1.Location;
            var t_height = textBox1.Size.Height;
            var b_size = button1.Size;

            textBox1.Location = new Point(t_loc.X, h - t_height - 3);
            textBox1.Size = new Size(w - t_loc.X - 3 - b_size.Width , t_height);

            button1.Location = new Point(w - b_size.Width, h - t_height - 3);

            // -- label1

            var l_loc = label1.Location;
            label1.Location = new Point(l_loc.X, h - t_height - 3 + 2);

            // -- listBox1

            listBox1.Size = new Size(w - 3, h - t_height - 8);
        }

        /// <summary>
        /// [UI thread] : Logが追加された時の描画ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        private void ListAdded(string lastLine)
        {
            // 最後の行だけ追加されたはずであるから、その部分だけを差分更新してやる。
            log_list.Enqueue(lastLine);

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
            UpdateListBox();
        }

        /// <summary>
        /// [UI thread] : 現在のテキストボックスの内容に従ってListBoxにlogを反映させる。
        /// </summary>
        private void UpdateListBox()
        {
            Debug.Assert(log_list != null);

            // listBox1に表示する内容まとめて生成して、まとめてセットする。
            Queue<string> list;
            var filter = textBox1.Text;
            if (string.IsNullOrEmpty(filter))
                list = log_list;
            else
            {
                list = new Queue<string>();
                try
                {
                    var regex = new Regex(textBox1.Text);
                    foreach (var t in log_list)
                        if (regex.Match(t).Success)
                            list.Enqueue(t);
                } catch
                {
                    list.Enqueue("-- filterで指定されている正規表現の表記に誤りがあります。 -- ");
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
        /// 選択行のコピペを実現する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                var sb = new StringBuilder();
                foreach (string text in listBox1.SelectedItems)
                    sb.AppendLine(text);
                Clipboard.SetText(sb.ToString());
            }
#if false
            // →　デバッグウインドウでこのショートカットキーが利いても嬉しくないか…。
            else
            {
                // メインウインドウのメニューに登録されているキーボードショートカットをハンドルする。
                TheApp.app.KeyShortcut.KeyDown(sender, e);
            }
#endif
        }

        /// <summary>
        /// ログのクリア
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            memory_log.Clear();
            log_list.Clear();
            UpdateListBox();
        }
    }
}
