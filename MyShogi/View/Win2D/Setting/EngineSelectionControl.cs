using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.EngineDefine;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// エンジン選択ダイアログ用のControl。
    /// 1つのエンジン分の情報を表示する。
    /// </summary>
    public partial class EngineSelectionControl : UserControl
    {
        public EngineSelectionControl()
        {
            InitializeComponent();

            ViewModel.AddPropertyChangedHandler("EngineDefine", EngineDefineChanged , Parent);
        }

        public class EngineSelectionViewModel : NotifyObject
        {
            /// <summary>
            /// Engine設定。これが画面に反映される。
            /// </summary>
            public EngineDefine EngineDefine
            {
                get { return GetValue<EngineDefine>("EngineDefine"); }
                set { SetValue<EngineDefine>("EngineDefine", value); }
            }

            /// <summary>
            /// 現環境の現在の空き物理メモリ。
            /// EngineDefineのsetterを呼び出す前に設定しておくこと。
            /// </summary>
            public int FreePhysicalMemory
            {   get; set; }
        }

        public EngineSelectionViewModel ViewModel = new EngineSelectionViewModel();

        /// <summary>
        /// [UI Thread] : EngineDefineが変更になった時のハンドラ。これが画面に反映される。
        /// </summary>
        /// <param name="args"></param>
        private void EngineDefineChanged(PropertyChangedEventArgs args)
        {
            var engineDefine = args.value as EngineDefine;

            if (engineDefine == null)
            {
                // 全部クリア
                var label_list = new[] { label1 , label2, label3,label4,label5 };
                foreach(var label in label_list)
                    label.Text = null;

                textBox1.Text = null;
                banner_mini.Dispose();
                pictureBox1.Image = null;
                return;
            }

            // ソフト名
            label1.Text = engineDefine.DescriptionSimple;

            // ソフトの説明文
            textBox1.Text = engineDefine.Description;

            // バナー

            // ImageLoader経由での読み込みなのでなければ「×」画像になるだけ。
            int w = pictureBox1.Width;
            int h = pictureBox1.Height;

            if (!File.Exists(engineDefine.BannerFileName))
            {
                // ファイルがないのでNO BANNERのbannerにする。
                var banner = TheApp.app.imageManager.NoBannerImage;
                banner_mini.Dispose();
                banner_mini = banner.CreateAndCopy(w, h);
            }
            else
            {
                var banner = new ImageLoader();
                banner.Load(engineDefine.BannerFileName);
                banner_mini.Dispose();
                banner_mini = banner.CreateAndCopy(w, h);
            }
            pictureBox1.Image = banner_mini.image;

            // ソフトの使用メモリ

            var free_memory = ViewModel.FreePhysicalMemory;
            var required_memory = engineDefine.RequiredMemory + engineDefine.MinimumHashMemory;
            var is_enough = required_memory <= free_memory;

            label2.Text = "必要メモリ : ";
            label3.Text = $"本体 {engineDefine.RequiredMemory}[MB] + HASH {engineDefine.MinimumHashMemory}[MB]以上 ＝ ";
            label4.Text = required_memory.ToString();
            label4.ForeColor = is_enough ? Color.Black : Color.Red;
            label5.Text = "[MB]以上" + (is_enough ? "≦" : "＞") + $" 現在の空き物理メモリ {free_memory}[MB]";

            // labelの位置を調整する。
            var labels = new []{ label2 , label3, label4,label5};
            for(int i = 1;i<labels.Length;++i)
            {
                labels[i].Location = new Point(labels[i - 1].Location.X + labels[i - 1].Width , labels[i - 1].Location.Y);
            }
        }

        /// <summary>
        /// banner用のImageLoader
        /// </summary>
        private ImageLoader banner_mini = new ImageLoader();

        private void button1_Click(object sender, System.EventArgs e)
        {

        }
    }
}
