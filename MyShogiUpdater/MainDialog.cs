using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MyShogiUpdater
{
    public partial class MainDialog : Form
    {
        public MainDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 設定関係
        /// </summary>
        public class MainDialogViewModel
        {
            /// <summary>
            /// 製品名
            /// </summary>
            public string ProductName;

            /// <summary>
            /// Updateに関する説明文が書いてあるテキスト
            /// </summary>
            public string UpdateTextFile;

            /// <summary>
            /// インストール先のフォルダ
            /// </summary>
            public string InstallFolder;

            /// <summary>
            /// インストール元のフォルダ
            /// (ここにpatchが入っている)
            /// </summary>
            public string SourceFolder;

            /// <summary>
            /// インストールが予約されているのでこのときは自動的にインストール。
            /// </summary>
            public bool AutoInstall;
        }

        public MainDialogViewModel ViewModel = new MainDialogViewModel();

        /// <summary>
        /// ViewModelに従って初期化する。
        /// </summary>
        public void Init()
        {
            var model = ViewModel;
            try
            {
                Text = model.ProductName + "アップデーター";

                var text = File.ReadAllText(model.UpdateTextFile);
                richTextBox1.Text = text;

                textBox1.Text = model.InstallFolder;

            } catch (Exception ex)
            {
                MessageBox.Show("例外が発生しました。\r\n"+ex.Message);
            }
        }

        private Thread workerThread;

        private void worker()
        {
            var model = ViewModel;

            Invoke(new Action(() => { richTextBox2.Text += $"\r\nCopy \"{ViewModel.SourceFolder}\" to \"{ViewModel.InstallFolder}\""; }));

            PatchMaker.FolderCopy(ViewModel.SourceFolder, ViewModel.InstallFolder, ViewModel.UpdateTextFile /*このファイル除外*/ , (filename) => {
                OutputProgressMessage(filename);
            });

            Invoke(new Action(() => { button1.Enabled = true; }));
        }

        /// <summary>
        /// 進捗表示用のテキストボックスにメッセージを表示する。
        /// </summary>
        /// <param name="message"></param>
        private void OutputProgressMessage(string message)
        {
            Invoke(new Action(() => {
                richTextBox2.AppendText( "\r\n" + message);
                try
                {
                    var start = richTextBox2.Text.Length; // 末尾を選択しておく。
                    richTextBox2.SelectionStart = start;
                    richTextBox2.ScrollToCaret();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));
        }

        /// <summary>
        /// インストール先フォルダの選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "インストール先のフォルダを指定してください。";
                //fbd.RootFolder = ...;
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                if (fbd.ShowDialog(this) == DialogResult.OK)
                    textBox1.Text = fbd.SelectedPath;
            }
        }

        /// <summary>
        /// インストール開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var targetFolder = textBox1.Text;
            var myShogiPath = Path.Combine(targetFolder, "myshogi.exe");
            if (!File.Exists(myShogiPath))
            {
                MessageBox.Show($"選択されたインストール先にmyshogi.exeがありません。\r\nこのフォルダには、{ViewModel.ProductName}がインストールされていません。");
                return;
            }

            // インストール先のフォルダは、このtargetFolderに変更。
            ViewModel.InstallFolder = targetFolder;

            // インストール開始
            // 権限昇格しないといけない…。
            try
            {
                button1.Enabled = false;

                // 管理者として実行されているのか？
                var isAdmin = PatchMaker.IsAdministrator();

                // AutoInstallが予約されている。
                if (ViewModel.AutoInstall)
                {
                    if (isAdmin)
                    {
                        workerThread = new Thread(worker);
                        workerThread.Start();
                    } else
                    {
                        //「ユーザーアカウント制御」ダイアログでキャンセルされたなどによって起動できなかった時
                        MessageBox.Show("管理者になれませんでした。UACの設定を確認してください。");
                    }

                } else if (isAdmin)
                {
                    // このまま実行できる。
                    workerThread = new Thread(worker);
                    workerThread.Start();
                }
                else {

                    //管理者として自分自身を起動する
                    var psi = new System.Diagnostics.ProcessStartInfo();
                    psi.UseShellExecute = true;
                    psi.FileName = Application.ExecutablePath;
                    psi.Verb = "runas"; // 管理者
                    psi.Arguments = $"FolderCopy \"{targetFolder}\"";
                    // このコマンドを実行してもらう。
                    try
                    {
                        //起動する
                        var process = System.Diagnostics.Process.Start(psi);
                        Application.Exit(); // このプロセスは終了させる。
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        //「ユーザーアカウント制御」ダイアログでキャンセルされたなどによって起動できなかった時
                        MessageBox.Show("起動しませんでした: " + ex.Message);
                    }
                }

                //PatchMaker.FolderCopy(ViewModel.SourceFolder, targetFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show("例外が発生しました。\r\n" + ex.Message);
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        private void MainDialog_Load(object sender, EventArgs e)
        {
            // UACで起動したので起動する。
            if (ViewModel.AutoInstall)
                button1_Click(null, null);
        }
    }
}
