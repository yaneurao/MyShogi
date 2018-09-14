using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class ExceptionDialog : Form
    {
        public ExceptionDialog()
        {
            InitializeComponent();

            Init();
        }

        /// <summary>
        /// [UI Thread] テキストボックスに表示するメッセージを設定する。
        /// </summary>
        /// <param name="text"></param>
        public void SetMessage(string text)
        {
            textBox1.Text = text;

            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = 0;
        }

        /// <summary>
        /// レイアウトの初期化
        /// </summary>
        private void Init()
        {
            ExceptionDialog_SizeChanged(null, null);
        }

        /// <summary>
        /// ウインドウのリサイズ時に各Controlのレイアウトを調整する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExceptionDialog_SizeChanged(object sender, System.EventArgs e)
        {
            int w = ClientSize.Width;
            int h = ClientSize.Height;
            int bh = button1.Height;
            textBox1.Size = new Size(w, h - bh - bh / 4 - textBox1.Location.Y);
            button1.Location = new Point(w / 2 - button1.Width / 2, h - bh - bh/8);
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
