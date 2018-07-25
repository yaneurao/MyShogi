using System;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.LocalServer;

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

            Disposed += OnDisposed;
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

            /// <summary>
            /// 「詳細設定」ボタンのEnableが変化して欲しい時に発生するイベント。
            /// 詳細設定ダイアログを出している時にもう片側のプレイヤーのこのボタンも無効化しないといけないので
            /// このイベントを介して行う。
            /// </summary>
            public bool SettingButton
            {
                get { return GetValue<bool>("SettingButton"); }
                set { SetValue("SettingButton", value); }
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

            // 対局者氏名
            binder.Bind(player ,"PlayerName", textBox1);

            // 対局者の種別
            binder.Bind(player , "IsHuman" , radioButton1);
            binder.Bind(player , "IsCpu"  , radioButton2);

            // プリセット
            binder.Bind(player, "SelectedEnginePreset", comboBox1);

            // ponder(相手の手番でも思考するか)
            binder.Bind(player, "Ponder", checkBox1);
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

            vm.AddPropertyChangedHandler("SettingButton", (args) =>
            {
                var b = (bool)args.value;
                button2.Enabled = b;
            });

            vm.AddPropertyChangedHandler("EngineDefineChanged", (args) =>
            {
                var player = TheApp.app.config.GameSetting.Player(vm.Color);

                var engine_define_ex = ViewModel.EngineDefine;

                button2.Enabled = engine_define_ex != null;
                if (engine_define_ex == null)
                    return;

                var engine_define = engine_define_ex.EngineDefine;

                // -- バナーの設定

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

                // -- 対局者名の設定

                textBox1.Text = engine_define.DisplayName;

                // -- プリセットのコンボボックス

                var PlayerSetting = TheApp.app.config.GameSetting.Player(vm.Color);
                //int preset = PlayerSetting.SelectedEnginePreset;

                // プリセットは前回のエンジンの選択時のSelectedPresetIndexを持って来て選ぶ。
                var indivisualEngine = TheApp.app.EngineConfigs.NormalConfig.Find(engine_define_ex.FolderPath);
                var preset = indivisualEngine.SelectedPresetIndex;

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

                UpdatePresetText();

                // エンジンを選択したのだから、対局相手はコンピュータになっていて欲しいはず。
                radioButton2.Checked = true;
            });

        }

        // -- screen update

        /// <summary>
        /// 選択しているプリセットが変更されたか、もしくはViewModelにセットされているEngineDefineが
        /// 変更になったかした時に、プリセットの説明文を更新するためのハンドラ
        /// </summary>
        private void UpdatePresetText()
        {
            var engineDefine = ViewModel.EngineDefine;
            if (engineDefine == null)
                return;

            var presets = engineDefine.EngineDefine.Presets;
            var selectedIndex = comboBox1.SelectedIndex;
            // 未選択か？
            if (selectedIndex < 0)
                return;

            if (presets.Count <= selectedIndex)
                selectedIndex = comboBox1.SelectedIndex = 0;

            textBox2.Text = presets[selectedIndex].Description;
        }

        /// <summary>
        /// エンジンオプション設定ダイアログを出す。
        /// 
        /// この構築のために思考エンジンに接続してoption文字列を取得しないといけなかったりしてわりと大変。
        /// </summary>
        private void ShowEngineOptionSettingDialog()
        {
            // 前回ダイアログを出しているなら消しておく。
            // (ボタンを無効化しているので出せないはずなのだが…)
            if (engineSettingDialog != null)
            {
                engineSettingDialog.Dispose();
                engineSettingDialog = null;
            }

            var dialog = EngineOptionSettingDialogBuilder.Build(
                EngineCommonOptionsSample.CreateEngineCommonOptions(), // 共通設定のベース
                TheApp.app.EngineConfigs.NormalConfig,                 // 共通設定の値はこの値で上書き
                ViewModel.EngineDefine                                 // 個別設定の情報はここにある。
                );

            // 構築に失敗。
            if (dialog == null)
                return;

            // 「詳細設定」ボタンをDisableにする。

            ViewModel.SettingButton = false;

            engineSettingDialog = dialog;
            engineSettingDialog.Disposed += (sender, args) => { ViewModel.SettingButton = true; };

            engineSettingDialog.Show();
        }

        /// <summary>
        /// 詳細設定ダイアログ
        /// </summary>
        private EngineOptionSettingDialog engineSettingDialog;

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
            // 詳細設定ダイアログ
            ShowEngineOptionSettingDialog();
        }

        /// <summary>
        /// 「人間」に切り替えた時に..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                textBox1.Text = ViewModel.Color.Pretty();
            }
        }

        /// <summary>
        /// 「コンピュータ」に切り替えた時に..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                // コンピュータの名前を持ってくる。
                var engineDefine = ViewModel.EngineDefine;
                if (engineDefine == null)
                {
#if false
                    if (TheApp.app.EngineDefines.Count == 0)
                        return;

                    // 一つ目のエンジンを選択しておく。
                    // これはEngineOrder順に並んでいるはずなので0番目がいいエンジンなはず…。
                    engineDefine = ViewModel.EngineDefine = TheApp.app.EngineDefines[0];
                    ViewModel.RaisePropertyChanged("EngineDefineChanged", engineDefine);
#endif
                    // 自分でエンジンの説明を見てから選ばないと空き物理メモリが足りない時に
                    // そのエンジンを選択してしまっていることがある。

                    //textBox2.Text = "エンジン未選択です。「エンジン選択」ボタンを押してエンジンを選択してください。";

                    // エンジン選択ダイアログを出したほうが親切か？

                    ViewModel.RaisePropertyChanged("EngineSelectionButtonClicked");
                    return;
                } 

                textBox1.Text = engineDefine.EngineDefine.DisplayName;
            }
        }

        /// <summary>
        /// 選択しているプリセットが変わった時に..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePresetText();

            // プリセットは前回のエンジンの選択時のSelectedPresetIndexを持って来て選ぶ。
            var indivisualEngine = TheApp.app.EngineConfigs.NormalConfig.Find(ViewModel.EngineDefine.FolderPath);
            indivisualEngine.SelectedPresetIndex = comboBox1.SelectedIndex;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            // data-bindしていたものすべてを解除する。
            binder.UnbindAll();
        }

        // -- privates

        private ImageLoader banner_mini;

        private ControlBinder binder = new ControlBinder();

    }
}
