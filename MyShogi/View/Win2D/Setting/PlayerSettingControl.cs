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

            // ponder設定、マスターアップに間に合わなさそうなのでいったん非表示に。
            checkBox1.Visible = false;

            Disposed += OnDisposed;
        }

        public class PlayerSettingViewModel : NotifyObject
        {
            // Bind()の時に参照を持ってる。
            public PlayerSetting PlayerSetting;

            /// <summary>
            /// エンジン設定
            /// </summary>
            //public EngineDefineEx EngineDefine;
            public string EngineDefineFolderPath
            {
                get { return GetValue<string>("EngineDefineFolderPath"); }
                set {
                    // 手動bind
                    if (PlayerSetting != null)
                        PlayerSetting.EngineDefineFolderPath = value;
                    SetValue<string>("EngineDefineFolderPath", value);
                }
            }

            /// <summary>
            /// これは、EngineDefineFolderPathを設定した時に設定される。
            /// 直接設定してはならない。
            /// </summary>
            public EngineDefineEx EngineDefineEx
            {
                get; set;
            }


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

#if false
            /// <summary>
            /// 「エンジン選択」「詳細設定」ボタンのEnableが変化して欲しい時に発生するイベント。
            /// 詳細設定ダイアログを出している時にもう片側のプレイヤーのこのボタンも無効化しないといけないので
            /// このイベントを介して行う。
            /// </summary>
            public bool EngineSelectButton
            {
                get { return GetValue<bool>("EngineSelectButton"); }
                set { SetValue("EngineSelectButton", value); }
            }
            public bool EngineOptionButton
            {
                get { return GetValue<bool>("EngineOptionButton"); }
                set { SetValue("EngineOptionButton", value); }
            }
#endif

            /// <summary>
            /// エンジンが選択された時に、CPUのほうが選択されて欲しいのでそのためのイベント。
            /// </summary>
            public object EngineSelected
            {
                set { SetValue("EngineSelected",value); }
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
            binder.Bind(player, "PlayerName", textBox1);

            // 対局者の種別
            binder.Bind(player, "IsHuman", radioButton1);
            binder.Bind(player, "IsCpu", radioButton2);

            // プリセット
            // この値を変更した時に、エンジンのプリセット選択のほうにも反映しないといけないが、それはcomboBoxの変更イベントで行っている。
            
            binder.Bind(player, "SelectedEnginePreset", comboBox1 );

            // ponder(相手の手番でも思考するか)
            binder.Bind(player, "Ponder", checkBox1);

            // ここに保存しておく。(ViewModel.EngineDefineFolderPathを変更した時に、ここに反映させる必要があるため。)
            
            ViewModel.PlayerSetting = player;
        }

        /// <summary>
        /// Bind()したものをすべて解除する。
        /// </summary>
        public void Unbind()
        {
            binder.UnbindAll();
            ViewModel.PlayerSetting = null;
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

#if false
            vm.AddPropertyChangedHandler("EngineSelectButton", (args) =>
            {
                var b = (bool)args.value;
                button1.Enabled = b; // エンジン選択ボタン
            });
            vm.AddPropertyChangedHandler("EngineOptionButton", (args) =>
            {
                var b = (bool)args.value;
                button2.Enabled = b; // 詳細設定ボタン
            });
#endif

            vm.AddPropertyChangedHandler("EngineDefineFolderPath", (args) =>
            {
                SuspendLayout();

                var player = TheApp.app.config.GameSetting.PlayerSetting(vm.Color);

                var folderPath = (string)args.value;
                var engine_define_ex = TheApp.app.EngineDefines.Find(x => x.FolderPath == folderPath);
                ViewModel.EngineDefineEx = engine_define_ex; // ついでなので保存しておく。

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

                // 人間が選択されている時は、Radioボタンのハンドラでプレイヤー名が「先手」「後手」になるのでここでは設定しない。
                if (player.IsCpu)
                    textBox1.Text = engine_define.DisplayName;

                // -- プリセットのコンボボックス

                var PlayerSetting = TheApp.app.config.GameSetting.PlayerSetting(vm.Color);
                
                // プリセットをコンボボックスに反映
                // SuspendLayout()～ResumeLayout()中にやらないとイベントハンドラが呼び出されておかしいことになる。
                // このプリセットのどこを選ぶかは、この外側でこのcomboBoxとdata bindしている変数を書き換えることで示す。

                comboBox1.Items.Clear();
                foreach (var e in engine_define.Presets)
                    comboBox1.Items.Add(e.Name);

                // 前回と同じものを選んでおいたほうが使いやすいかも。
                // →　「やねうら王」初段　のあと「tanuki」を選んだ時も初段であって欲しい気がするが…。
                // 先後入替えなどもあるので、やはりそれは良くない気がする。

                // エンジンを選択したのだから、対局相手はコンピュータになっていて欲しいはず。
                // →　エンジンを選択したとは限らない。先後入替えでもここが呼び出される。
                //radioButton2.Checked = true;

                ResumeLayout();
            });

            vm.AddPropertyChangedHandler("EngineSelected", args =>
            {
                // -- CPUの選択
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
            var engineDefine = ViewModel.EngineDefineEx;
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
                ViewModel.EngineDefineEx                               // 個別設定の情報はここにある。
                );

            // 構築に失敗。
            if (dialog == null)
                return;

            // 「詳細設定」ボタンをDisableにする。
            //ViewModel.SettingButton = false;
            // →　この処理やめる。modal dialogとして出せばOk.

            engineSettingDialog = dialog;
            //engineSettingDialog.Disposed += (sender, args) => { ViewModel.SettingButton = true; };

            // modal dialogとして出す
            engineSettingDialog.ShowDialog(this.Parent);
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
                var engineDefineEx = ViewModel.EngineDefineEx;
                if (engineDefineEx == null)
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

                textBox1.Text = engineDefineEx.EngineDefine.DisplayName;
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

            var folderPath = ViewModel.EngineDefineFolderPath;
            if (folderPath == null)
                return;

            // プリセットは前回のエンジンの選択時のSelectedPresetIndexを持って来て選ぶ。
            var indivisualEngine = TheApp.app.EngineConfigs.NormalConfig.Find(folderPath);
            if (indivisualEngine == null)
                return;

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
