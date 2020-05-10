using MyShogi.App;
using MyShogi.Model.Dependency;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Setting
{
    public partial class EngineDefineEditDialog : Form
    {
        public EngineDefineEditDialog()
        {
            InitializeComponent();

            // フォント変更。
            FontUtility.ReplaceFont(this, TheApp.app.Config.FontManager.SettingDialog);
        }

        #region property

        #endregion

        /// <summary>
        /// ラジオボタン変更イベントはすべてここに送られてくる。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            // 何番目のラジオボタンにチェックが入っているのかを確定させる。
            var radio_buttons = new[]{
                radioButton1, radioButton2, radioButton3, radioButton4,
                radioButton5, radioButton6, radioButton7, radioButton8
            };

            // 選択されているラジオボタンが何番目であるか
            int checked_radio_number = -1;
            for(int i = 0; i < radio_buttons.Length; ++i)
            {
                var radio = radio_buttons[i];
                if (radio.Checked)
                {
                    checked_radio_number = i;
                    break;
                }
            }

            // エンジンファイル名
            var engine_file_names = new[] { "YaneuraOu", "Apery","gikou2","usi-engine",
                "YaneuraOu_MATE","SeoTsume","NanohaTsumeUSI","mate-engine" };

            // エンジン表示名
            var engine_display_names = new[] { "やねうら王", "Apery","技巧２","usi-engine",
                " やねうら王詰め","脊尾詰","なのは詰め","mate-engine" };

            // エンジンに対応するCPU(bit表現で、下位からno_sse,sse2,sse4_1,sse4_2,avx2)
            var engine_cpus = new[] {
                0b11111 , 0b11111 , 0b01000 , 0b11111 ,
                0b11111 , 0b00001 , 0b00111 , 0b11111
            }; 

            textBox1.Text = engine_file_names[checked_radio_number];
            textBox2.Text = engine_display_names[checked_radio_number];
            var engine_cpu = engine_cpus[checked_radio_number];
            var check_boxs = new[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5 };
            for (int i = 0; i < 5; ++i)
                check_boxs[i].Checked = (engine_cpu & (1 << i)) != 0;


        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            // ブラウザで説明書を開く
            API.OpenWithBrowser("html/howto-use-external-engine.html");
        }
    }
}
