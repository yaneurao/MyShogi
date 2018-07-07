using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    public partial class GameSettingDialog : Form
    {
        public GameSettingDialog(MainDialog mainDialog_)
        {
            InitializeComponent();

            mainDialog = mainDialog_;

            // 現在の画面のdpiの影響を受けて、このウィンドウのWidth,Heightが大きくなっているはずなので
            // それをベースに以降の計算を行うため、いくつかの値を保存しておく。

            originalWidth = Width;
            originalGroupBox2 = groupBox2.Location;

            // デモ用にバナーを描画しておく

            // (w,h)=(320,100)のつもりだが、dpi scalingのせいで
            // 環境によって異なるのでここで再取得してそれに合わせる。
            int w = pictureBox1.Width;
            int h = pictureBox1.Height;

            banner1.Load(@"engine/tanuki2018/banner.png");
            banner1mini = banner1.CreateAndCopy(w, h);
            pictureBox1.Image = banner1mini.image;

            banner2.Load(@"engine/yaneuraou2018/banner.png");
            banner2mini = banner2.CreateAndCopy(w, h);
            pictureBox2.Image = banner2mini.image;

            // checkbox5,6がgroupbox4,5に属すると嫌だったのでgroupboxの外に配置しておいてあったので
            // それを移動させる。
            {
                // checkBox3と同じyにしたいが、これはgroupBox5に属するのでgroupBox5相対の座標になっている。
                int y = groupBox5.Location.Y + checkBox3.Location.Y;
                checkBox5.Location = new Point(checkBox5.Location.X, y);
                checkBox6.Location = new Point(checkBox6.Location.X, y);
            }

            // データバインドしておく。
            BindSetting();
        }

        /// <summary>
        /// 親ウィンドウの何かを操作しないといけないことがあるので、
        /// コンストラクタでmainDialogの参照を受け取って、ここに保持しておく。
        /// </summary>
        private MainDialog mainDialog;
        private ControlBinder binder = new ControlBinder();

        /// <summary>
        /// ダイアログ生成時の値、各種。
        /// </summary>
        private int originalWidth;
        private Point originalGroupBox2;

        private ImageLoader banner1 = new ImageLoader();
        private ImageLoader banner2 = new ImageLoader();
        private ImageLoader banner1mini;
        private ImageLoader banner2mini;

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
                binder.Bind(setting.Player(c).PlayerName, playerNames[(int)c], t => setting.Player(c).PlayerName = t);

                // 対局者の種別
                binder.Bind(setting.Player(c).IsHuman, human_radio_buttons[(int)c], v => setting.Player(c).IsHuman = v);
                binder.Bind(setting.Player(c).IsCpu, cpu_radio_buttons[(int)c], v => setting.Player(c).IsCpu = v);
            }

            // -- 開始局面

            // 手合割有効か
            binder.Bind(setting.Board.BoardTypeEnable, radioButton5, v => setting.Board.BoardTypeEnable = v);
            binder.Bind((int)setting.Board.BoardType, comboBox3, v => setting.Board.BoardType = (BoardType)v);

            // 現在の局面から
            binder.Bind(setting.Board.BoardTypeCurrent, radioButton6, v => setting.Board.BoardTypeCurrent = v);

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
                    int c = (int)c_;
                    var n = num[c];
                    var timeSetting = setting.KifuTimeSettings.RawPlayer(c_);
                    binder.Bind(timeSetting.Hour, n[0], v => timeSetting.Hour = v);
                    binder.Bind(timeSetting.Minute, n[1], v => timeSetting.Minute = v);
                    binder.Bind(timeSetting.Second, n[2], v => timeSetting.Second = v);
                    binder.Bind(timeSetting.Byoyomi, n[3], v => timeSetting.Byoyomi = v);
                    binder.Bind(timeSetting.IncTime, n[4], v => timeSetting.IncTime = v);

                    var r = radio[c];
                    // 秒読みのラジオボタンが選択されていれば、IncTimeのほうの設定はグレーアウト。
                    binder.Bind(timeSetting.ByoyomiEnable, r[0], v =>
                    {
                        timeSetting.ByoyomiEnable = v;
                        if (v)
                        {
                            n[3].Enabled = true;
                            n[4].Enabled = false;
                        }
                    });
                    binder.Bind(timeSetting.IncTimeEnable, r[1], v =>
                    {
                        timeSetting.IncTimeEnable = v;
                        if (v)
                        {
                            n[3].Enabled = false;
                            n[4].Enabled = true;
                        }
                    });
                    binder.Bind(timeSetting.IgnoreTime, check[c], v => timeSetting.IgnoreTime = v);
                    binder.Bind(timeSetting.TimeLimitless, check_unlimit_time[c], v =>
                    {
                        timeSetting.TimeLimitless = v;

                        // 時間無制限の時、GroupBox丸ごとDisableに。]
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
            binder.Bind(misc.DetailEnable, button6, v => misc.DetailEnable = v , v => v ? "簡易設定" : "詳細設定");

            // -- 後手の対局時間設定を個別にするのか

            // このチェックボックスが無効だと、それに応じてgroupBox5が無効化されなくてはならない。
            binder.Bind(setting.KifuTimeSettings.WhiteEnable, checkBox1,
                v => {
                    setting.KifuTimeSettings.WhiteEnable = v;
                    groupBox5.Enabled = v && /*時間無制限*/!checkBox6.Checked;
                    checkBox6.Enabled = v; // 時間無制限
                    if (v)
                        groupBox4.Text = "時間設定[先手/上手]";
                    else
                        groupBox4.Text = "時間設定";
                });

            // 指定手数で引き分けにする
            binder.Bind(misc.MaxMovesToDrawEnable, checkBox4, v =>
            {
                misc.MaxMovesToDrawEnable = v;
                numericUpDown11.Enabled = v;

            });
            binder.Bind(misc.MaxMovesToDraw, numericUpDown11, v => misc.MaxMovesToDraw = v);

            ResumeLayout();
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

    }
}
