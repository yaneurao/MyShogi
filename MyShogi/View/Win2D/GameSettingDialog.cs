using System;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Resource;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Player;

namespace MyShogi.View.Win2D
{
    public partial class GameSettingDialog : Form
    {
        public GameSettingDialog(MainDialog mainDialog_)
        {
            InitializeComponent();

            mainDialog = mainDialog_;

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

            // データバインドしておく。
            BindSetting();
        }

        /// <summary>
        /// 親ウィンドウの何かを操作しないといけないことがあるので、
        /// コンストラクタでmainDialogの参照を受け取って、ここに保持しておく。
        /// </summary>
        private MainDialog mainDialog;

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
            var gameServer = mainDialog.ViewModel.gameServer;

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
            var setting = TheApp.app.config.GameSetting;
            var binder = new ControlBinder();

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

            var timeSetting = setting.TimeSetting(Color.BLACK);
            binder.Bind(timeSetting.Hour, numericUpDown1, v => timeSetting.Hour = v );
            binder.Bind(timeSetting.Minute, numericUpDown2, v => timeSetting.Minute = v);
            binder.Bind(timeSetting.Byoyomi, numericUpDown3, v => timeSetting.Byoyomi = v);
            binder.Bind(timeSetting.IncTime, numericUpDown4, v => timeSetting.IncTime = v);
            binder.Bind(timeSetting.ByoyomiEnable, radioButton7, v => timeSetting.ByoyomiEnable = v);
            binder.Bind(timeSetting.IncTimeEnable, radioButton8, v => timeSetting.IncTimeEnable = v);

            // -- 詳細設定であるか
            
        }


        /// <summary>
        /// 「詳細設定」ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
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
        }

        /// <summary>
        /// 幅広いダイアログに変更
        /// </summary>
        private void ChangeToWideDialog()
        {

        }

        /// <summary>
        /// 幅の狭いダイアログに変更(デフォルト)
        /// </summary>
        private void ChangeToNarrowDialog()
        {

        }
    }
}
