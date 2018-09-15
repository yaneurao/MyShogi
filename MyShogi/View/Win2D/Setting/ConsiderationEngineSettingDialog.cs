using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.View.Win2D;

namespace MyShogi.View.Win2D.Setting
{
    public enum ConsiderationEngineSettingDialogType
    {
        ConsiderationSetting,
        MateSetting,
    }

    public partial class ConsiderationEngineSettingDialog : Form
    {
        public ConsiderationEngineSettingDialog()
        {
            InitializeComponent();

            InitViewModel();
        }

        #region ViewModel

        public class ConsiderationEngineSettingDialogViewModel : NotifyObject
        {
            /// <summary>
            /// このダイアログの種類。検討用/詰検討用のエンジン設定
            /// ウィンドウのcaptionなどに反映される。
            /// </summary>
            public ConsiderationEngineSettingDialogType DialogType
            {
                get { return GetValue<ConsiderationEngineSettingDialogType>("DialogType"); }
                set { SetValue("DialogType", value); }
            }

            /// <summary>
            /// エンジンが選択された時にそのEngineDefineがあるfolder pathが代入される。
            /// </summary>
            public string EngineDefineFolderPath
            {
                get { return GetValue<string>("EngineDefineFolderPath"); }
                set { SetValue("EngineDefineFolderPath", value); }
            }

            /// <summary>
            /// エンジン選択がクリックされた時に発生するイベント
            /// </summary>
            public object EngineSelectionButtonClicked
            {
                get { return GetValue<object>("EngineSelectionButtonClicked"); }
                set { SetValue("EngineSelectionButtonClicked", value); }
            }

            /// <summary>
            /// 「検討開始」ボタンがクリックされた時に発生するイベント
            /// </summary>
            public object StartButtonClicked
            {
                get { return GetValue<object>("StartButtonClicked"); }
                set { SetValue("StartButtonClicked", value); }
            }
        }

        public ConsiderationEngineSettingDialogViewModel ViewModel = new ConsiderationEngineSettingDialogViewModel();

        private void InitViewModel()
        {
            // 検討開始ボタンは、検討エンジンが選択されていないと有効にならない。
            button3.Enabled = false;

            ViewModel.AddPropertyChangedHandler("EngineDefineFolderPath", (args) =>
            {
                SuspendLayout();
                try
                {
                    var folderPath = (string)args.value;
                    var engine_define_ex = TheApp.app.EngineDefines.Find(x => x.FolderPath == folderPath);
                    Setting.EngineDefineFolderPath = folderPath;

                    button2.Enabled = engine_define_ex != null;
                    button3.Enabled = engine_define_ex != null;

                    if (engine_define_ex == null)
                    {
                        // バナー等をクリアしてから返る。
                        pictureBox1.Image = null;
                        textBox1.Text = null;
                        return;
                    }

                    var engine_define = engine_define_ex.EngineDefine;

                    // エンジン名の設定

                    textBox1.Text = engine_define.DisplayName;

                    // -- バナーの設定

                    // (w,h)=(320,100)のつもりだが、dpi scalingのせいで
                    // 環境によって異なるのでここで再取得してそれに合わせる。
                    int w = pictureBox1.Width;
                    int h = pictureBox1.Height;

                    // バナーファイルの設定
                    // ファイルがないならNO BANNERの画像。
                    var banner_file_name = engine_define.BannerFileName;

                    // 一つ前のがあるなら解放
                    banner_mini.Dispose();
                    if (!System.IO.File.Exists(banner_file_name))
                    {
                        var banner = TheApp.app.ImageManager.NoBannerImage;
                        banner_mini = banner.CreateAndCopy(w, h);
                        // これはImageManagerからもらったやつなので解放してはならない。
                    }
                    else
                    {
                        using (var banner = new ImageLoader())
                        {
                            banner.Load(engine_define.BannerFileName);
                            banner_mini = banner.CreateAndCopy(w, h);
                        }
                    }

                    pictureBox1.Image = banner_mini.image;
                }
                finally
                {
                    ResumeLayout();
                }
            });

            ViewModel.AddPropertyChangedHandler("EngineSelectionButtonClicked", _ => CreateEngineSelectionDialog() );

            ViewModel.AddPropertyChangedHandler("DialogType", (args) =>
           {
               var dialogType = ViewModel.DialogType;

               switch (dialogType)
               {
                   case ConsiderationEngineSettingDialogType.ConsiderationSetting:
                       Text = "検討エンジン設定";
                       label3.Text = "検討で使う思考エンジン：";
                       groupBox1.Enabled = true;
                       break;

                   case ConsiderationEngineSettingDialogType.MateSetting:
                       Text = "詰将棋エンジン設定";
                       label3.Text = "詰検討で使う思考エンジン：";
                       groupBox1.Enabled = true; // 詰将棋エンジン側、対応したので有効にしておく。
                       break;
               }
           });
        }

        /// <summary>
        /// 選択ダイアログの生成
        /// </summary>
        private void CreateEngineSelectionDialog()
        {
            // 詳細設定ボタンの無効化と、このエンジン選択ダイアログを閉じる時に詳細設定ボタンの再有効化。
            using (var dialog = new EngineSelectionDialog())
            {

                if (ViewModel.DialogType == ConsiderationEngineSettingDialogType.ConsiderationSetting)
                    dialog.InitEngineDefines(true, false); // 通常のエンジンのみ表示
                else
                    dialog.InitEngineDefines(false, true); // 詰将棋エンジンのみ表示

                // エンジンを選択し、「選択」ボタンが押された時のイベントハンドラ
                dialog.ViewModel.AddPropertyChangedHandler("ButtonClicked", (args) =>
                {
                    var engineDefine = (EngineDefineEx)args.value;
                    ViewModel.EngineDefineFolderPath = engineDefine.FolderPath;
                    dialog.Close(); // 閉じる
                });

                // modal dialogとして出すべき。
                FormLocationUtility.CenteringToThisForm(dialog, this);
                dialog.ShowDialog(Parent);
            }
        }

        /// <summary>
        /// エンジンオプション設定ダイアログを出す。
        /// 
        /// この構築のために思考エンジンに接続してoption文字列を取得しないといけなかったりしてわりと大変。
        /// </summary>
        private void ShowEngineOptionSettingDialog()
        {
            var opt = EngineCommonOptionsSampleOptions.InstanceForConsideration();
            var consideration = ViewModel.DialogType == ConsiderationEngineSettingDialogType.ConsiderationSetting;

            var dialog = EngineOptionSettingDialogBuilder.Build(
                EngineCommonOptionsSample.CreateEngineCommonOptions(opt), // 共通設定のベース(検討、詰検討用)
                consideration ? TheApp.app.EngineConfigs.ConsiderationConfig : TheApp.app.EngineConfigs.MateConfig , // 共通設定の値はこの値で上書き
                ViewModel.EngineDefineFolderPath                          // 個別設定の情報はここにある。
                );

            // 構築に失敗。
            if (dialog == null)
                return;

            try
            {
                FormLocationUtility.CenteringToThisForm(dialog, this);

                // modal dialogとして出す
                dialog.ShowDialog(this);
            }
            finally
            {
                dialog.Dispose();
            }
        }

        #endregion

        #region public members

        /// <summary>
        /// このControlにbindする。
        /// </summary>
        /// <param name="setting"></param>
        public void Bind(ConsiderationEngineSetting setting)
        {
            Setting = setting;

            // radio buttonなので、片側が必ず選択されていなければならない。
            if (!(setting.Limitless ^ setting.TimeLimitEnable))
            {
                setting.Limitless = true;
                setting.TimeLimitEnable = false;
            }

            binder.Bind(setting, "PlayerName" , textBox1);
            binder.Bind(setting, "Limitless"  , radioButton1);
            binder.Bind(setting, "TimeLimitEnable" , radioButton2);
            binder.Bind(setting, "Second", numericUpDown1 );

            ViewModel.EngineDefineFolderPath = setting.EngineDefineFolderPath;
        }

        private void Unbind()
        {
            binder.UnbindAll();
        }

        #endregion

        #region event handler

        /// <summary>
        /// エンジン選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, System.EventArgs e)
        {
            ViewModel.RaisePropertyChanged("EngineSelectionButtonClicked", null);
        }

        /// <summary>
        /// エンジンの詳細設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, System.EventArgs e)
        {
            // 詳細設定ダイアログ
            ShowEngineOptionSettingDialog();
        }

        private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
        {
            numericUpDown1.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, System.EventArgs e)
        {
            numericUpDown1.Enabled = true;
        }

        /// <summary>
        /// 検討開始ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, System.EventArgs e)
        {
            ViewModel.RaisePropertyChanged("StartButtonClicked", null);

            // このダイアログは閉じる。
            Close();
        }

        private void ConsiderationEngineSettingDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            // リソースの解放
            binder.UnbindAll();
            banner_mini.Dispose();
        }

        #endregion

        #region private members

        private ImageLoader banner_mini = new ImageLoader();
        private ControlBinder binder = new ControlBinder();
        private ConsiderationEngineSetting Setting;

        #endregion
    }
}
