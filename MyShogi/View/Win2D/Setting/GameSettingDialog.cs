using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using SCore = MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局設定ダイアログ
    /// 
    /// 注意)
    /// Visual StudioのデザイナでこのDialogを編集するときは
    ///   AutoScale = Size(96F,96F)
    /// で編集しなければならない。
    /// 
    /// high dpi環境で編集して(192F,192F)とかになっていると、
    /// 解像度の低い実行環境でダイアログの大きさが小さくなってしまう。
    /// (.NET Frameworkのhigh dpiのバグ)
    /// </summary>
    public partial class GameSettingDialog : Form
    {
        public GameSettingDialog(MainDialog mainDialog_)
        {
            InitializeComponent();

            mainDialog = mainDialog_;

            // 画面初期化
            InitScreen();

            // ViewModelのハンドラの設定
            SetHandlers();

            // TheApp.app.config.GameSettingを、このFormのControlたちとデータバインドしておく。
            BindSetting();
        }

        // -- properties

        public class GameSettingViewModel : NotifyObject
        {
            /// <summary>
            /// エンジン設定。二人分。
            /// </summary>
            public EngineDefine[] EngineDefines = new EngineDefine[(int)SCore.Color.NB];

            /// <summary>
            /// ↑のEngineDefinesの要素が変更された時に発生するイベント
            /// </summary>
            public int EngineDefineChanged
            {
                get { return GetValue<int>("EngineDefineChanged"); }
                set { SetValue<int>("EngineDefineChanged",value); }
            }
        }

        public GameSettingViewModel ViewModel = new GameSettingViewModel();

        // -- handlers

        /// <summary>
        /// エンジン詳細設定(先手)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// エンジン詳細設定(後手)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 対局開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            var gameSetting = TheApp.app.config.GameSetting;
            var gameServer = mainDialog.gameServer;

            // 設定をClone()してから渡す。(immutableにしたいため)
            gameServer.GameStartCommand(gameSetting.Clone());

            // 対局が開始するのでこのダイアログを閉じる
            this.Close();
        }

        /// <summary>
        /// 先手側エンジン選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            CreateEngineSelectionDialog(SCore.Color.BLACK);
        }

        /// <summary>
        /// 後手側エンジン選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            CreateEngineSelectionDialog(SCore.Color.WHITE);
        }

        /// <summary>
        /// 「詳細設定」ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            if (button6.Text == "詳細設定")
            {
                button6.Text = "簡易設定";
                ChangeToWideDialog();
            }
            else
            {
                button6.Text = "詳細設定";
                ChangeToNarrowDialog();
            }
            ResumeLayout();
        }

        /// <summary>
        /// 「先後入替」ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            // 対局者氏名、エンジン、持ち時間設定を入れ替える。
            // データバインドされているはずなので、DataSourceのほうで入替えて、
            // rebindすればいいような..

            binder.UnbindAll();
            TheApp.app.config.GameSetting.SwapPlayer();
            BindSetting();
        }

        // -- screen settings

        private void InitScreen()
        {
            // 現在の画面のdpiの影響を受けて、このウィンドウのWidth,Heightが大きくなっているはずなので
            // それをベースに以降の計算を行うため、いくつかの値を保存しておく。

            originalWidth = Width;
            originalGroupBox2 = groupBox2.Location;

            // checkbox5,6がgroupbox4,5に属すると嫌だったのでgroupboxの外に配置しておいてあったので
            // それを移動させる。
            {
                // checkBox3と同じyにしたいが、これはgroupBox5に属するのでgroupBox5相対の座標になっている。
                int y = groupBox5.Location.Y + checkBox3.Location.Y;
                checkBox5.Location = new Point(checkBox5.Location.X, y);
                checkBox6.Location = new Point(checkBox6.Location.X, y);
            }
        }

        /// <summary>
        /// ViewModelのハンドラの設定
        /// </summary>
        private void SetHandlers()
        {
            ViewModel.AddPropertyChangedHandler("EngineDefineChanged", (args) =>
            {
                int c = (int)args.value;
                var engine_define = ViewModel.EngineDefines[c];

                var pictureBox = c == 0 ? pictureBox1 : pictureBox2;

                // (w,h)=(320,100)のつもりだが、dpi scalingのせいで
                // 環境によって異なるのでここで再取得してそれに合わせる。
                int w = pictureBox.Width;
                int h = pictureBox.Height;

                // バナーファイルの設定
                // ファイルがないならNO BANNERの画像。
                var banner_file_name = engine_define.BannerFileName;
                ImageLoader banner;
                if (!System.IO.File.Exists(banner_file_name))
                    banner = TheApp.app.imageManager.NoBannerImage;
                else
                {
                    banner = new ImageLoader();
                    banner.Load(engine_define.BannerFileName);
                }
                banner_mini[c] = banner.CreateAndCopy(w, h);
                pictureBox.Image = banner_mini[c].image;
            });
        }


        /// <summary>
        /// このダイアログのControlとGlobalConfig.Settingの一部をbindしておく。
        /// </summary>
        private void BindSetting()
        {
            SuspendLayout();

            var setting = TheApp.app.config.GameSetting;

            // 対局者氏名のテキストボックス
            var playerNames = new[] { textBox1, textBox2 };

            // 対局相手のラジオボタン
            var human_radio_buttons = new[] { radioButton1, radioButton3 };
            var cpu_radio_buttons = new[] { radioButton2, radioButton4 };

            // -- プレイヤーごとの設定
            foreach (var c in All.Colors())
            {
                // 対局者氏名
                binder.Bind(setting.Player(c),"PlayerName", playerNames[(int)c]);

                // 対局者の種別
                binder.Bind(setting.Player(c) , "IsHuman" , human_radio_buttons[(int)c]);
                binder.Bind(setting.Player(c) , "IsCpu"  , cpu_radio_buttons[(int)c]);
            }

            // -- 開始局面

            // 手合割有効か
            binder.Bind(setting.Board , "BoardTypeEnable" , radioButton5 );
            binder.Bind(setting.Board , "BoardType" , comboBox3 );

            // 現在の局面から
            binder.Bind(setting.Board , "BoardTypeCurrent" , radioButton6);

            // -- 対局時間設定をbindする

            {
                var num = new[]
                {
                    new []{ numericUpDown1, numericUpDown2 , numericUpDown3 , numericUpDown4 , numericUpDown5},
                    new []{ numericUpDown6, numericUpDown7 , numericUpDown8 , numericUpDown9 , numericUpDown10},
                };
                var radio = new[]
                {
                    new [] { radioButton7  , radioButton8},
                    new [] { radioButton9  , radioButton10},
                };
                var check = new[] { checkBox2, checkBox3 };
                var check_unlimit_time = new[] { checkBox5, checkBox6 };
                var group = new[] { groupBox4, groupBox5 };

                foreach (var c_ in All.Colors())
                {
                    int c = (int)c_; // copy for capture
                    var n = num[c];
                    var timeSetting = setting.KifuTimeSettings.RawPlayer(c_);
                    binder.Bind(timeSetting , "Hour"   , n[0] );
                    binder.Bind(timeSetting , "Minute" , n[1] );
                    binder.Bind(timeSetting , "Second" , n[2] );
                    binder.Bind(timeSetting , "Byoyomi", n[3] );
                    binder.Bind(timeSetting , "IncTime", n[4] );

                    var r = radio[c];
                    // 秒読みのラジオボタンが選択されていれば、IncTimeのほうの設定はグレーアウト。
                    binder.Bind(timeSetting, "ByoyomiEnable", r[0] , (v)=>
                    {
                        if (v)
                        {
                            n[3].Enabled = true;
                            n[4].Enabled = false;
                        }
                    });
                    binder.Bind(timeSetting, "IncTimeEnable", r[1] , (v)=>
                    {
                        if (v)
                        {
                            n[3].Enabled = false;
                            n[4].Enabled = true;
                        }
                    });
                    binder.Bind(timeSetting , "IgnoreTime" , check[c] );
                    binder.Bind(timeSetting , "TimeLimitless", check_unlimit_time[c] , (v)=>
                    {
                        // 時間無制限の時、GroupBox丸ごとDisableに。
                        // ただし、自分のチェックボックスは除外。この除外は、コンストラクタでGroupから除外している。
                        group[c].Enabled = !v;
                    });
                }
            }

            // -- 詳細設定であるか

            var misc = setting.MiscSettings;

            if (misc.DetailEnable)
                ChangeToWideDialog();
            else
                ChangeToNarrowDialog();

            // 「詳細設定」ボタンのテキスト
            binder.Bind(misc , "DetailEnable" , button6, v => v ? "簡易設定" : "詳細設定");

            // -- 後手の対局時間設定を個別にするのか

            // このチェックボックスが無効だと、それに応じてgroupBox5が無効化されなくてはならない。
            binder.Bind(setting.KifuTimeSettings, "WhiteEnable", checkBox1 , (v)=>
            {
                groupBox5.Enabled = v && /*時間無制限*/!checkBox6.Checked;
                checkBox6.Enabled = v; // 時間無制限
                if (v)
                    groupBox4.Text = "時間設定[先手/上手]";
                else
                    groupBox4.Text = "時間設定";
            });

            // 指定手数で引き分けにする
            binder.Bind(misc, "MaxMovesToDrawEnable", checkBox4 , (v)=>
            {
                misc.MaxMovesToDrawEnable = v;
                numericUpDown11.Enabled = v;
            });

            binder.Bind(misc , "MaxMovesToDraw" , numericUpDown11);

            ResumeLayout();
        }


        /// <summary>
        /// 幅広いダイアログに変更
        /// </summary>
        private void ChangeToWideDialog()
        {
            Width = originalWidth;

            // 後手の名前など
            groupBox2.Location = new Point(groupBox5.Location.X , groupBox1.Location.Y);
        }

        /// <summary>
        /// 幅の狭いダイアログに変更(デフォルト)
        /// </summary>
        private void ChangeToNarrowDialog()
        {
            Width = originalWidth/2;

            // 後手の名前など
            groupBox2.Location = originalGroupBox2;
        }

        /// <summary>
        /// 選択ダイアログの生成
        /// </summary>
        private void CreateEngineSelectionDialog(SCore.Color c)
        {
            ReleaseEngineSelectionDialog();
            engineSelectionDialog = new EngineSelectionDialog();

            engineSelectionDialog.ViewModel.AddPropertyChangedHandler("ButtonClicked", (args) =>
            {
                // これが選択された。
                var selectedEngine = (int)args.value;
                var defines = TheApp.app.engine_defines;
                if (selectedEngine < defines.Count)
                {
                    var engineDefine = TheApp.app.engine_defines[selectedEngine];
                    // 先手か後手かは知らんが、そこにこのEngineDefineを設定

                    ViewModel.EngineDefines[(int)c] = engineDefine;
                    ViewModel.RaisePropertyChanged("EngineDefineChanged",(int)c);
                }
                ReleaseEngineSelectionDialog();
            });
            engineSelectionDialog.Show();
        }

        /// <summary>
        /// エンジン選択ダイアログの解体
        /// </summary>
        private void ReleaseEngineSelectionDialog()
        {
            if (engineSelectionDialog != null)
            {
                engineSelectionDialog.Dispose();
                engineSelectionDialog = null;
            }
        }

        // -- privates

        /// <summary>
        /// 親ウィンドウの何かを操作しないといけないことがあるので、
        /// コンストラクタでmainDialogの参照を受け取って、ここに保持しておく。
        /// </summary>
        private MainDialog mainDialog;

        /// <summary>
        /// エンジン選択ボタンが押された時にエンジンを選ぶダイアログ。
        /// </summary>
        public EngineSelectionDialog engineSelectionDialog;

        private ControlBinder binder = new ControlBinder();

        /// <summary>
        /// ダイアログ生成時の値、各種。
        /// </summary>
        private int originalWidth;
        private Point originalGroupBox2;

        private ImageLoader[] banner_mini = new ImageLoader[2];

    }
}
