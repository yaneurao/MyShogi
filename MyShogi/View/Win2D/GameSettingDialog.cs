using System;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Resource;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.LocalServer;
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

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 5;

            LoadGameSetting();
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
            // 現在のダイアログの状態をGlobalConfigに保存する。
            SaveGameSetting();

            var gameSetting = TheApp.app.config.GameSetting;
            var gameServer = mainDialog.ViewModel.gameServer;
            gameServer.GameStartCommand(gameSetting);

            // 対局が開始するのでこのダイアログを閉じる
            this.Close();
        }

        /// <summary>
        /// GlobalConfigのGameSettingから情報を復元する。
        /// </summary>
        private void LoadGameSetting()
        {
            // -- 対局設定をGlobalConfigから復元する。

            // 手動データバインディングみたいなことをしている。

            var gameSetting = TheApp.app.config.GameSetting;

            var boardType = gameSetting.BoardType;
            if (boardType != BoardType.Current)
            {
                comboBox3.SelectedIndex = (int)boardType;
                radioButton5.Checked = true;
            }
            else
            {
                comboBox3.SelectedIndex = 0;
                radioButton6.Checked = true;
            }

            // 対局相手のラジオボタン
            var human_radio_buttons = new[] { radioButton1, radioButton3 };
            var cpu_radio_buttons = new[] { radioButton2, radioButton4 };

            // 対局者氏名のテキストボックス
            var playerNames = new [] { textBox1, textBox2 };

            foreach (var c in All.Colors())
            {
                PlayerTypeEnum playerType = gameSetting.Player(c).PlayerType;

                human_radio_buttons[(int)c].Checked = playerType == PlayerTypeEnum.Human;
                cpu_radio_buttons[(int)c].Checked = playerType == PlayerTypeEnum.UsiEngine;

                // 対局者氏名
                playerNames[(int)c].Text = gameSetting.Player(c).PlayerName;
            }
        }

        private void SaveGameSetting()
        {
            var gameSetting = TheApp.app.config.GameSetting;

            // 対局相手のラジオボタン
            var human_radio_buttons = new[] { radioButton1, radioButton3 };
            var cpu_radio_buttons = new[] { radioButton2, radioButton4 };

            foreach (var c in All.Colors())
            {
                PlayerTypeEnum playerType;
                if (human_radio_buttons[(int)c].Checked)
                    playerType = PlayerTypeEnum.Human;
                else if (cpu_radio_buttons[(int)c].Checked)
                    playerType = PlayerTypeEnum.UsiEngine;
                else
                    // このラジオボタンどうなっとるんや…。
                    throw new Exception("どちらのプレイヤーも選択されていません");

                gameSetting.Player(c).PlayerType = playerType;
                // その他、設定を調べて受け継ぐ。
            }

            // 開始局面の選択
            if (radioButton5.Checked)
            {
                // 手合割の取得
                var index = comboBox3.SelectedIndex;
                var boardType = (BoardType)index;
                if (!boardType.IsSfenOk())
                    boardType = BoardType.NoHandicap; // なぜなのか..どこも選択されていないのか？

                gameSetting.BoardType = boardType;
            }
            else // if (radioButton6.Checked)
            {
                // 現在の局面から開始
                gameSetting.BoardType = BoardType.Current;
            }

            // 対局者氏名
            var playerNames = new string[2] { textBox1.Text, textBox2.Text };
            foreach (var c in All.Colors())
            {
                var playerName = playerNames[(int)c];
                // 入力されていなければ、 "先手"とか"後手"とかにする。
                if (string.IsNullOrEmpty(playerName))
                    playerName = c.Pretty();

                gameSetting.Player(c).PlayerName = playerName;
            }
        }

        /// <summary>
        /// このフォームが閉じられる時に、設定をGlobalConfigのほうに移動させておかないと次回開いたときに
        /// 設定が保存されていなくて気持ち悪い。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameSettingDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveGameSetting();
        }
    }
}
