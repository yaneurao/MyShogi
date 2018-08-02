using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
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

            /// <summary>
            /// 「選択」ボタンが押された時にsetterが呼び出される仮想プロパティ
            /// </summary>
            public bool ButtonClicked
            {
                get { return GetValue<bool>("ButtonClicked"); }
                set { SetValue<bool>("ButtonClicked", value); }
            }
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
                var label_list = new[] { label1, label2, label3, label4, label5 };
                foreach (var label in label_list)
                    label.Text = null;

                textBox1.Text = null;
                banner_mini.Dispose();
                pictureBox1.Image = null;
                button1.Enabled = false;
                return;
            }

            // 「選択」ボタン有効化
            button1.Enabled = true;

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
            var engine_define = ViewModel.EngineDefine;
            var work_memory = engine_define.EvalMemory + engine_define.WorkingMemory + engine_define.StackPerThread * 4;
            var required_memory = work_memory + engine_define.MinimumHashMemory;
            var is_enough = required_memory <= free_memory;

            label2.Text = "必要メモリ : ";
            label3.Text = $"本体 {work_memory}[MB] + HASH {engineDefine.MinimumHashMemory}[MB]以上 ＝ ";
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

        // -- handlers

        private void button1_Click(object sender, System.EventArgs e)
        {
            // メモリ足りないならこの時点で警告ダイアログを出す。

            var free_memory = ViewModel.FreePhysicalMemory;
            var engine_define = ViewModel.EngineDefine;
            var required_memory = engine_define.EvalMemory + engine_define.WorkingMemory + engine_define.MinimumHashMemory;
            var is_enough = required_memory <= free_memory;

            if (!is_enough)
            {
                var result = TheApp.app.MessageShow("空き物理メモリが足りないので思考エンジンの動作が不安定になる可能性があります。"
                    , MessageShowType.WarningOkCancel);

                // 選択をキャンセル
                if (result == DialogResult.Cancel)
                    return;
            }

            ViewModel.RaisePropertyChanged("ButtonClicked");
        }

        /// <summary>
        /// banner用のImageLoader
        /// </summary>
        private ImageLoader banner_mini = new ImageLoader();
    }
}
