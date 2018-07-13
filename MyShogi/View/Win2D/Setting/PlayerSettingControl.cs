using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.Model.Shogi.Player;

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

            // プレセット
            binder.Bind(player, "SelectedEnginePreset", comboBox1);
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
                var indivisualEngine = TheApp.app.EngineConfig.Find(engine_define_ex.FolderPath);
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
            var dialog = new EngineOptionSettingDialog();

            // -- エンジン共通設定

            var setting = EngineCommonOptionsSample.CreateEngineCommonOptions();
            setting.IndivisualSetting = false; // エンジン共通設定
            setting.BuildOptionsFromDescriptions(); // OptionsをDescriptionsから構築する。

            // エンジン共通設定の、ユーザーの選択をシリアライズしたものがあるなら、そのValueを上書きする。
            var options = TheApp.app.EngineConfig.CommonOptions;
            if (options != null)
                setting.OverwriteEngineOptions(options);

            dialog.SettingControls(0).ViewModel.Setting = setting;
            dialog.SettingControls(0).ViewModel.AddPropertyChangedHandler("ValueChanged", (args) =>
            {
                TheApp.app.EngineConfig.CommonOptions = setting.ToEngineOptions();
                // 値が変わるごとに保存しておく。
            });

            // -- エンジン個別設定

            // 思考エンジンの個別ダイアログのための項目を、実際に思考エンジンを起動して取得。
            // 一瞬で起動～終了するはずなので、UIスレッドでやっちゃっていいや…。
            var engineDefine = ViewModel.EngineDefine.EngineDefine;
            var exefilename = engineDefine.EngineExeFileName();

            var engine = new UsiEnginePlayer();
            try
            {
                engine.Engine.EngineSetting = true;
                engine.Start(exefilename);

                while (engine.Initializing)
                {
                    engine.OnIdle();
                    Thread.Sleep(100);
                    var ex = engine.Engine.Exception;
                    if (ex != null)
                    {
                        // time outも例外が飛んでくる…ようにすべき…。
                        // 現状の思考エンジンでここでタイムアウトにならないから、まあいいや…。
                        TheApp.app.MessageShow(ex.ToString());
                        return;
                    }
                }
            }
            finally
            {
                engine.Dispose();
            }

            // エンジンからこれが取得出来ているはずなのだが。
            Debug.Assert(engine.Engine.OptionList != null);

            // エンジンからUsiOption文字列を取得

            var useHashCommand = engineDefine.IsSupported(ExtendedProtocol.UseHashCommandExtension);

            var ind_options = new List<EngineOptionForSetting>();
            foreach (var option in engine.Engine.OptionList)
            {
                //Console.WriteLine(option.CreateOptionCommandString());

                // "USI_Ponder"は無視する。
                if (option.Name == "USI_Ponder")
                    continue;

                // "USI_Hash","Hash"は統合する。
                else if (option.Name == "USI_Hash")
                {
                    // USI_Hash使わないエンジンなので無視する。
                    if (useHashCommand)
                        continue;

                    option.SetName("Hash_"); // これにしておけばあとで置換される。
                }
                else if (option.Name == "Hash")
                {
                    //Debug.Assert(useHashCommand);

                    option.SetName("Hash_"); // これにしておけばあとで置換される。
                }

                var opt = new EngineOptionForSetting(option.Name, option.CreateOptionCommandString());
                opt.Value = option.GetDefault();
                ind_options.Add(opt);
            }

            var ind_descriptions = engineDefine.EngineOptionDescriptions; // nullありうる
            if (ind_descriptions == null)
            {
                // この時は仕方ないので、Optionsの内容そのまま出しておかないと仕方ないのでは…。
                ind_descriptions = new List<EngineOptionDescription>();

                foreach (var option in ind_options)
                    ind_descriptions.Add(new EngineOptionDescription(option.Name, option.Name, null, null, option.UsiBuildString));
            } else
            {
                // Descriptionsに欠落している項目を追加する。(Optionsにだけある項目を追加する。)
                foreach (var option in ind_options)
                {
                    if (ind_descriptions.Find(x => x.Name == option.Name) == null)
                        ind_descriptions.Add(new EngineOptionDescription(option.Name, option.Name, null, null, option.UsiBuildString));
                }

                // DescriptionにあってOptionsにない項目をOptionsに追加する。
                foreach(var desc in ind_descriptions)
                {
                    if (ind_options.Find(x => x.Name == desc.Name) == null)
                        ind_options.Add(new EngineOptionForSetting(desc.Name, desc.UsiBuildString));
                }
            }

            // エンジン個別設定でシリアライズしていた値で上書きしてやる。
            // TODO:

            var ind_setting = new EngineOptionsForSetting()
            {
                Options = ind_options,
                Descriptions = ind_descriptions,
                IndivisualSetting = true, // エンジン個別設定
            };

            dialog.SettingControls(1).ViewModel.Setting = ind_setting;
            dialog.SettingControls(1).ViewModel.AddPropertyChangedHandler("ValueChanged", (args) =>
            {
                // TODO:
                //TheApp.app.EngineConfig.CommonOptions = setting.ToEngineOptions();
                // 値が変わるごとに保存しておく。
            });


            dialog.Show();
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
            var indivisualEngine = TheApp.app.EngineConfig.Find(ViewModel.EngineDefine.FolderPath);
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
