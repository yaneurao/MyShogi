using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using MyShogi.Model.Shogi;

namespace MyShogi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Model.Test.DevTest1.Test1();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Model.Test.DevTest1.Test2();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Model.Test.DevTest1.Test3();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Model.Test.DevTest2.Test1();
        }
    }
}
