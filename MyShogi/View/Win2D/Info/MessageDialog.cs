using MyShogi.App;
using MyShogi.Model.Common.Collections;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Resource.Images;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class MessageDialog : Form
    {
        public MessageDialog()
        {
            InitializeComponent();

            FontUtility.ReplaceFont(this, TheApp.app.Config.FontManager.MessageDialog);
        }

        /// <summary>
        /// [UI Thread] テキストボックスに表示するメッセージを設定する。
        /// </summary>
        /// <param name="text"></param>
        public void SetMessage(string caption , string text , MessageShowType type)
        {
            Text = caption;

            // アイコンの設定

            var icon = type.ToIcon();
            var image_number = - 1;
            switch (icon)
            {
                case MessageBoxIcon.Error       : image_number = 0; break;
                case MessageBoxIcon.Question    : image_number = 1; break;
                case MessageBoxIcon.Exclamation : image_number = 2; break;
                case MessageBoxIcon.Asterisk    : image_number = 3; break;
            }

            if (image_number >= 0)
            {
                var image = TheApp.app.ImageManager.MessageBoxIconImage;
                var p = pictureBox1;
                var rect = new Rectangle(image_number * 128, 0, 128, 128);
                picture_mini = image.CreateAndCopy(p.Width , p.Height , rect ,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                p.Image = picture_mini.image;
            }

            // テキストの設定

            if (image_number == 0)
            {
                // 例外のときは、テキストボックスに書く。

                textBox2.Text = "エラーが発生しました。詳細は以下の通りです。";

                textBox1.Text = text;
                textBox1.SelectionStart = 0;
                textBox1.SelectionLength = 0;

            } else
            {
                // 例外以外なら、べた書き。
                textBox2.Text = text;
                
                textBox1.Visible = false;

                Size = new Size(Width, Height - textBox1.Height + button1.Height/2);
            }

            ShowType = type;

            Init();

            // とりまOKのほうをアクティブにしとく。
            button1.Focus(); 
        }

        /// <summary>
        /// SetMessage()の引数で渡された値
        /// </summary>
        private MessageShowType ShowType;
        private bool initialized; // SetMessage()が完了しているのか。

        /// <summary>
        /// レイアウトの初期化
        /// </summary>
        private void Init()
        {
            initialized = true;
            MessageDialog_SizeChanged(null, null);
        }

        /// <summary>
        /// ウインドウのリサイズ時に各Controlのレイアウトを調整する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageDialog_SizeChanged(object sender, System.EventArgs e)
        {
            // SetMessage()での初期化が終わってからしか処理しない。
            if (!initialized)
                return;

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            int bh = button1.Height;

            //if (textBox1.Visible)
            // →　Show()する前は、Controls.Add(textBox1)しているので、親側のVisibleを反映して常にfalse

            if (!textBox1.Text.Empty())
                textBox1.Size = new Size(w, h - bh - bh / 2 - textBox1.Location.Y);

            // ボタンを表示すべきY座標
            int by = h - bh - bh / 4;

            var buttons = ShowType.ToButtons();
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    button1.Location = new Point(Width / 2 - button1.Width /2 , by);
                    button2.Visible = false;
                    break;

                case MessageBoxButtons.OKCancel:
                    button1.Location = new Point(Width / 2 - button1.Width - DefaultMargin.Left, by);
                    button2.Location = new Point(Width / 2                 + DefaultMargin.Left, by);
                    break;
            }
        }

        /// <summary>
        /// Button1,2の両方のクリックハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, System.EventArgs e)
        {
            DialogResult = sender == button1 ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        private void MessageDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            ReleaseImage();
        }


        private void ReleaseImage()
        {
            if (picture_mini != null)
            {
                picture_mini.Dispose();
                picture_mini = null;
            }
        }

        private ImageLoader picture_mini;
    }
}
