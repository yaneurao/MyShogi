using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.LocalServer;
using System;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Setting
{
    /// <summary>
    /// プレイヤーが「人間」か「コンピュータ」かを選択し、
    /// 「コンピュータ」である場合は、ソフト名を選択でき、
    /// 選択したソフトに対してそのバナーを表示するControl。
    /// 
    /// 片側のプレイヤー分。
    /// </summary>
    public partial class PlayerSettingControl : UserControl
    {
        public PlayerSettingControl()
        {
            InitializeComponent();

            InitViewModel();
        }

        public class PlayerSettingViewModel : NotifyObject
        {
            /// <summary>
            /// エンジン設定
            /// </summary>
            public EngineDefineEx EngineDefine;
            
            /// <summary>
            /// ↑のEngineDefinesの要素が変更された時に発生するイベント
            /// 変更されたほうのColorがargs.Valueに入ってくる。
            /// </summary>
            //public int EngineDefineChanged;

            /// <summary>
            /// エンジン選択ボタンが押された時に飛んでくるイベント
            /// </summary>
            public bool EngineSelectionButtonClicked;

            /// <summary>
            /// 手番
            /// </summary>
            public Color Color
            {
                get { return GetValue<Color>("Color"); }
                set { SetValue("Color", value); }
            }
        }

        public PlayerSettingViewModel ViewModel = new PlayerSettingViewModel();

        /// <summary>
        /// このControl上のcontrolにbindする。
        /// </summary>
        /// <param name="player"></param>
        public void Bind(PlayerSetting player)
        {
            // -- プレイヤーごとの設定
            foreach (var c in All.Colors())
            {
                // 対局者氏名
                binder.Bind(player ,"PlayerName", textBox1);

                // 対局者の種別
                binder.Bind(player , "IsHuman" , radioButton1);
                binder.Bind(player , "IsCpu"  , radioButton2);

                // プレセット
                binder.Bind(player, "SelectedEnginePreset", comboBox1);
            }
        }
        
        /// <summary>
        /// Bind()したものをすべて解除する。
        /// </summary>
        public void Unbind()
        {
            binder.UnbindAll();
        }

        // -- ViewModelの初期化

        private void InitViewModel()
        {
            var vm = ViewModel;

            vm.AddPropertyChangedHandler("Color", (args) =>
             {
                 var color = (Color)args.value;
                 groupBox1.Text = color == Color.BLACK ? "先手/下手" : "後手/上手";
             });

            vm.AddPropertyChangedHandler("EngineDefineChanged", (args) =>
            {
                var player = TheApp.app.config.GameSetting.Player(vm.Color);

                var engine_define_ex = ViewModel.EngineDefine;
                var engine_define = engine_define_ex.EngineDefine;

                // (w,h)=(320,100)のつもりだが、dpi scalingのせいで
                // 環境によって異なるのでここで再取得してそれに合わせる。
                int w = pictureBox1.Width;
                int h = pictureBox1.Height;

                // バナーファイルの設定
                // ファイルがないならNO BANNERの画像。
                var banner_file_name = engine_define.BannerFileName;
                ImageLoader banner;
                if (!System.IO.File.Exists(banner_file_name))
                    banner = TheApp.app.imageManager.NoBannerImage;
                else
                {
                    banner = new ImageLoader();
                    banner.Load(engine_define.BannerFileName);
                }
                if (banner_mini != null)
                    banner_mini.Dispose();
                banner_mini = banner.CreateAndCopy(w, h);

                pictureBox1.Image = banner_mini.image;

                var PlayerSetting = TheApp.app.config.GameSetting.Player(vm.Color);
                int preset = PlayerSetting.SelectedEnginePreset;

                // プリセットをコンボボックスに反映
                comboBox1.Items.Clear();
                foreach (var e in engine_define.Presets)
                    comboBox1.Items.Add(e.Name);

                // ComboBoxとデータバインドされているのでこれで変更されるはず。
                player.EngineDefineFolderPath = engine_define_ex.FolderPath;

                // 前回と同じものを選んでおいたほうが使いやすいかも。
                // →　「やねうら王」初段　のあと「tanuki」を選んだ時も初段であって欲しい気がするので。

                // 復元する。データバインドされているのであとは何とかなるはず。
                PlayerSetting.SelectedEnginePreset = preset;
                PlayerSetting.RaisePropertyChanged("SelectedEnginePreset", preset);
            });

        }

        // -- handlers

        /// <summary>
        /// エンジン選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ViewModel.RaisePropertyChanged("EngineSelectionButtonClicked", null);
        }

        /// <summary>
        /// エンジンの詳細設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

        }

        // -- privates

        private ImageLoader banner_mini;

        private ControlBinder binder = new ControlBinder();
    }
}
