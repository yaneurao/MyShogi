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

            // textBox5,6は、数値のみのテキストボックスにしたいのだが、
            // キーハンドラ書くの面倒なので、まあいいや…。

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
            var engine_file_names = new[] {
                "YaneuraOu", "Apery","gikou2","usi-engine",
                "YaneuraOu_MATE","SeoTsume","NanohaTsumeUSI","mate-engine"
            };

            // エンジン表示名
            var engine_display_names = new[] {
                "やねうら王系", "Apery系","技巧２","USI対応エンジン",
                "やねうら王詰め","脊尾詰","なのは詰め","USI対応詰将棋エンジン"
            };

            // エンジンに対応するCPU(bit表現で、下位からno_sse,sse2,sse4_1,sse4_2,avx2)
            var engine_cpus = new[] {
                0b11111 , 0b11111 , 0b01000 , 0b11111 ,
                0b11111 , 0b00001 , 0b00111 , 0b11111
            };

            // エンジンの説明1行
            var engine_descriptions_simple = new[]
            {
                "やねうら王系のエンジン",
                "Apery系のエンジン",
                "技巧２",
                "一般的なUSIエンジン",
                "やねうら王詰め",
                "脊尾詰",
                "なのは詰め",
                "一般的な詰将棋エンジン",
            };

            // エンジンの説明3行
            var engine_descriptions = new[]
            {
                "やねうら王をベースとした思考エンジンです。\r\n豊富な機能が自慢です。",
                "Aperyをベースとしたエンジンです。\r\n最新版はRustで書かれています。",
                "美しいソースコードに確かな技術力を感じます。",
                "一般的なUSIプロトコルに対応したエンジンです。",
                "実戦的な詰将棋を超高速に解きます。",
                "超長手数の詰将棋が解ける有名で優秀な詰将棋エンジンです。",
                "長手数の詰将棋が解ける詰将棋エンジンです。",
                "USIプロトコルに対応した詰将棋エンジンです。",
            };

            // 100k nodesのときのレーティング
            var engine_rates = new[]
            {
                "3000","3000","3000","3000",
                "0","0","0","0"
            };

            // 評価関数用のメモリ
            var engine_eval_memory = new[]
            {
                "1000","1000","1000","1000",
                "0","0","0","0"
            };

            textBox1.Text = engine_file_names[checked_radio_number];
            textBox2.Text = engine_display_names[checked_radio_number];
            textBox3.Text = engine_descriptions_simple[checked_radio_number];
            textBox4.Text = engine_descriptions[checked_radio_number];
            textBox5.Text = engine_rates[checked_radio_number];
            textBox6.Text = engine_eval_memory[checked_radio_number];

            var engine_cpu = engine_cpus[checked_radio_number];
            var check_boxs = new[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5 };
            for (int i = 0; i < 5; ++i)
                check_boxs[i].Checked = (engine_cpu & (1 << i)) != 0;

        }

        /// <summary>
        /// ブラウザで説明を書いたテキストを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, System.EventArgs e)
        {
            API.OpenWithBrowser("html/howto-use-external-engine.html");
        }

        /// <summary>
        /// 設定ファイルを書き出す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, System.EventArgs e)
        {

        }
    }
}
