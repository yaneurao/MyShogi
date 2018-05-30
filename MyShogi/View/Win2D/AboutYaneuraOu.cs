using MyShogi.App;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class AboutYaneuraOu : Form
    {
        public AboutYaneuraOu()
        {
            InitializeComponent();

            // ファイルから読み込む
            string text;
            try
            {
                text = File.ReadAllText("text/about_myshogi.txt", Encoding.UTF8);
            }catch
            {
                text = "";
            }

            text = "MyShogi Version " + GlobalConfig.MYSHOGI_VERSION_STRING + "\r\n\r\n" + text;

            textBox1.Text = text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
