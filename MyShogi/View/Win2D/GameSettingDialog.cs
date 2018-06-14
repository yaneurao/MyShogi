using System;
using System.Windows.Forms;
using MyShogi.Model.Resource;
using MyShogi.Model.Shogi.Player;
using SCore = MyShogi.Model.Shogi.Core;

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

            for (SCore.Color c = SCore.Color.ZERO; c < SCore.Color.NB; ++c)
            {
                if (human_radio_buttons[(int)c].Checked)
                    players[(int)c] = new HumanPlayer();
                else if (cpu_radio_buttons[(int)c].Checked)
                    players[(int)c] = new UsiEnginePlayer();
                else
                    // このラジオボタンどうなっとるんや…。
                    throw new Exception("どちらのプレイヤーも選択されていません");

                // その他、設定を調べて受け継ぐ。
            }

            gameServer.GameStartCommand(players[0], players[1]);

            // 対局が開始するのでこのダイアログを隠す
            this.Hide();
        }
    }
}
