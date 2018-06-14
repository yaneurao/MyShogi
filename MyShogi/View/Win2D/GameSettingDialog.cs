using System;
using System.Windows.Forms;
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
            comboBox4.SelectedIndex = 0;
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
            // 人間かエンジンかを見てそれを反映させてゲームを開始させる。
            var gameServer = mainDialog.ViewModel.gameServer;
            var players = new Player[2];

            // 対局相手のラジオボタン
            var human_radio_buttons = new []{ radioButton1 , radioButton3 };
            var cpu_radio_buttons = new[] { radioButton2, radioButton4 };

            var gameSetting = new GameSetting();

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
                gameSetting.BoardType = BoardType.NoHandicap;
            }
            else // if (radioButton6.Checked)
            {
                // 現在の局面から開始
                gameSetting.BoardType = BoardType.Current;
            }

            gameServer.GameStartCommand(gameSetting);

            // 対局が開始するのでこのダイアログを隠す
            this.Hide();
        }
    }
}
