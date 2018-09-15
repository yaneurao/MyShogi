using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Data;
using MyShogi.View.Win2D.Setting;
using SCore = MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    public partial class GameResultDialog : Form
    {
        public GameResultDialog()
        {
            InitializeComponent();

            InitListViewHeader();
            InitListViewContents();
        }

        public class GameResultDialogViewModel : NotifyObject
        {
            /// <summary>
            /// 棋譜を読み込むときのイベント。読み込みボタンが押されたときに発生する。
            /// </summary>
            public string KifuClicked
            {
                get { return GetValue<string>("KifuClicked"); }
            }
        };

        public GameResultDialogViewModel ViewModel = new GameResultDialogViewModel();
        
        public void InitListViewHeader()
        {
            listView1.FullRowSelect = true;
            //listView1.GridLines = true;
            listView1.Sorting = SortOrder.None;
            listView1.View = System.Windows.Forms.View.Details;

            // ヘッダーのテキストだけセンタリング、実項目は右寄せしたいのだが、これをするには
            // オーナードローにする必要がある。面倒くさいので、ヘッダーのテキストにpaddingしておく。
            // またヘッダーの1列目のTextAlignは無視される。これは.NET FrameworkのListViewの昔からあるバグ。(仕様？)

            var startTime = new ColumnHeader();
            startTime.Text = "対局日時";
            startTime.Width = 150;
            startTime.TextAlign = HorizontalAlignment.Center;

            var player_name_black = new ColumnHeader();
            player_name_black.Text = "先手/下手";
            player_name_black.Width = 150;
            player_name_black.TextAlign = HorizontalAlignment.Left;

            var player_name_white = new ColumnHeader();
            player_name_white.Text = "後手/上手";
            player_name_white.Width = 150;
            player_name_white.TextAlign = HorizontalAlignment.Left;

            var board_type = new ColumnHeader();
            board_type.Text = "手合割";
            board_type.Width = 80;
            board_type.TextAlign = HorizontalAlignment.Center;

            var game_ply = new ColumnHeader();
            game_ply.Text = "手数";
            game_ply.Width = 60;
            game_ply.TextAlign = HorizontalAlignment.Right;

            var game_result = new ColumnHeader();
            game_result.Text = "勝敗";
            game_result.Width = 80;
            game_result.TextAlign = HorizontalAlignment.Center;

            var special_move = new ColumnHeader();
            special_move.Text = "結果";
            special_move.Width = 100;
            special_move.TextAlign = HorizontalAlignment.Center;

            var time_setting = new ColumnHeader();
            time_setting.Text = "持ち時間";
            time_setting.Width = 150;
            time_setting.TextAlign = HorizontalAlignment.Center;

            var kifu_filename = new ColumnHeader();
            kifu_filename.Text = "ファイル名";
            kifu_filename.Width = 400;
            kifu_filename.TextAlign = HorizontalAlignment.Left;

            var header = new[] { startTime, player_name_black, player_name_white , board_type ,
                game_ply , game_result , special_move , time_setting , kifu_filename };

            listView1.Columns.AddRange(header);
        }

        private void InitListViewContents()
        {
            var setting = TheApp.app.Config.GameResultSetting;
            var csv = new GameResultTable();
            var lines = csv.ReadOrCreate(setting.CsvFilePath());
            var items = new List<ListViewItem>(); // 逆順にしないといけないのでいったんここに入れる。
            foreach(var result in lines)
            {
                // 解析できなかった行はnullになってしまうので、この判定必要。
                if (result == null)
                    continue;

                string[] list;
                try
                {
                    if (!string.IsNullOrEmpty(result.Comment))
                    {
                        // コメント行なら、とりあえず対局日時のところに表示しておく。
                        list = new[] { result.Comment };
                    }
                    else
                    {
                        var game_result = result.LastMove.GameResult();
                        if (result.LastColor == SCore.Color.WHITE) // 後手から見た結果なので反転させる。
                            game_result = game_result.Not();
                        var game_result_string = game_result.Pretty(result.Handicapped);
                        var special_move_string = result.LastMove.SpecialMoveToKif();

                        list = new[] { result.StartTime.ToString(), result.PlayerNames[0], result.PlayerNames[1] ,
                            result.BoardType.Pretty() , result.GamePly.ToString() , game_result_string ,
                            special_move_string  , result.TimeSettingString , result.KifuFileName ,
                        };
                    }
                } catch (Exception ex)
                {
                    list = new[] { $"行の解析に失敗しました。{ex.Message}" };
                }
                var item = new ListViewItem(list);
                items.Add(item);
            }
            // 逆順にする。
            items.Reverse();
            listView1.Items.AddRange(items.ToArray());
        }

        /// <summary>
        // focusが失われていたのでHideSelection==falseにしているにも関わらず
        // 選択行の背景色が元に戻ってしまっている。(ListViewが糞仕様である)
        // そこでボタンハンドラなどで終了時に再度focusを戻してやる必要がある。
        /// </summary>
        private void SelectListView()
        {
            listView1.Select();
        }

        /// <summary>
        /// 対局結果の保存設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new GameResultWindowSettingDialog())
            {
                FormLocationUtility.CenteringToThisForm(dialog, this);
                dialog.ShowDialog(this);
            }
            SelectListView();
        }

        /// <summary>
        /// 棋譜保存フォルダを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            var path = TheApp.app.Config.GameResultSetting.KifuSaveFolder;
            if (!Directory.Exists(path))
            {
                TheApp.app.MessageShow("棋譜保存フォルダが存在しません。\r\n棋譜の保存設定を確認してください。", MessageShowType.Warning);
                return;
            }

            var indices = listView1.SelectedIndices;
            if (indices.Count == 0)
            {
                // どの行も選択されていないのでフォルダを開く
                Process.Start(path);

            }
            else
            {

                try
                {
                    // このファイルが選択されている状態でexplorerを開く
                    var sub_items = listView1.Items[indices[0]].SubItems;
                    var filename = sub_items.Count >= 9 ? sub_items[8].Text : null;
                    if (string.IsNullOrEmpty(filename))
                    {
                        Process.Start(path);
                    }
                    else
                    {
                        var filepath = Path.Combine(path, filename);
                        if (!System.IO.File.Exists(filepath))
                            throw new Exception("ファイルが見つかりません。" + filepath);

                        Process.Start("explorer.exe", $"/select,\"{filepath}\"");
                    }
                }
                catch
                {
                    TheApp.app.MessageShow("この棋譜ファイルが見当たりません。", MessageShowType.Warning);
                }
            }

            SelectListView();
        }

        /// <summary>
        /// 対局結果一覧のクリア
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (TheApp.app.MessageShow("対局結果一覧をクリアしますか？(棋譜ファイルは保存先フォルダから削除されません)",
                MessageShowType.ConfirmationOkCancel) == DialogResult.Cancel)
                return;

            try
            {
                var setting = TheApp.app.Config.GameResultSetting;
                System.IO.File.Delete(setting.CsvFilePath());

                listView1.Items.Clear();

            }
            catch (Exception ex)
            {
                TheApp.app.MessageShow("対局結果一覧のファイル削除に失敗しました。\r\n" + ex.Message,
                    MessageShowType.Confirmation);
                return;
            }
        }

        /// <summary>
        /// 対局棋譜の読み込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, System.EventArgs e)
        {
            var indices = listView1.SelectedIndices;
            if (indices.Count == 0)
                return;

            try
            {
                var filename = listView1.Items[indices[0]].SubItems[8].Text;
                ViewModel.RaisePropertyChanged("KifuClicked", filename);
                //Console.WriteLine(filename);
            }
            catch { }

            SelectListView();
        }

        private void GameResultDialog_Resize(object sender, System.EventArgs e)
        {
            var h = button1.Height;
            var y = ClientSize.Height - h;
            button1.Location = new Point(button1.Location.X , y );
            button2.Location = new Point(button2.Location.X, y);
            button3.Location = new Point(button3.Location.X, y);

            var w4 = button4.Width;
            button4.Location = new Point(ClientSize.Width - w4 - 3, y);

            listView1.Size = new Size(ClientSize.Width, y - 3);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var indices = listView1.SelectedIndices;
            // 1つでも選択されていれば棋譜読み込みボタンを有効に。
            button4.Enabled = indices.Count > 0
                && listView1.Items[indices[0]].SubItems.Count >= 9;
        }

        private void GameResultDialog_Load(object sender, EventArgs e)
        {
            // ボタン基準でリサイズ
            var w1 = button1.Width;
            ClientSize = new Size(w1 * 6, w1 * 4);
        }
    }
}
