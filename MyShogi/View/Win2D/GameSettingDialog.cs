using MyShogi.Model.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class GameSettingDialog : Form
    {
        public GameSettingDialog()
        {
            InitializeComponent();

            // デモ用にバナーを描画しておく

            banner1.Load(@"engine/tanuki2018/banner.png");
            banner1mini = banner1.CreateAndCopy(320, 100);
            pictureBox1.Image = banner1mini.image;

            banner2.Load(@"engine/yaneuraou2018/banner.png");
            banner2mini = banner2.CreateAndCopy(320, 100);
            pictureBox2.Image = banner2mini.image;

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 5;
            comboBox4.SelectedIndex = 0;
        }

        private ImageLoader banner1 = new ImageLoader();
        private ImageLoader banner2 = new ImageLoader();
        private ImageLoader banner1mini;
        private ImageLoader banner2mini;

    }
}
