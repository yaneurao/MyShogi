using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.View.Win2D.Common;

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
            /// このダイアログの種類
            /// </summary>
            public ConsiderationEngineSettingDialogType DialogType
            {
                get { return GetValue<ConsiderationEngineSettingDialogType>("DialogType"); }
                set { SetValue("DialogType", value); }
            }

            public EngineDefineEx EngineDefineEx { get; set; }
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
                    ViewModel.EngineDefineEx = engine_define_ex; // ついでなので保存しておく。
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
                       break;

                   case ConsiderationEngineSettingDialogType.MateSetting:
                       Text = "詰将棋エンジン設定";
                       label3.Text = "詰検討で使う思考エンジン：";
                       break;
               }
           });
        }

        /// <summary>
        /// 選択ダイアログの生成
        /// </summary>
        private void CreateEngineSelectionDialog()
        {
            ReleaseEngineSelectionDialog();
            // 詳細設定ボタンの無効化と、このエンジン選択ダイアログを閉じる時に詳細設定ボタンの再有効化。
            engineSelectionDialog = new EngineSelectionDialog();

            // エンジンを選択し、「選択」ボタンが押された時のイベントハンドラ
            engineSelectionDialog.ViewModel.AddPropertyChangedHandler("ButtonClicked", (args) =>
            {
                // これが選択された。
                var selectedEngine = (int)args.value;
                var defines = TheApp.app.EngineDefines;
                if (selectedEngine < defines.Count)
                {
                    var engineDefine = defines[selectedEngine];
                    // 先手か後手かは知らんが、そこにこのEngineDefineを設定

                    ViewModel.EngineDefineFolderPath = engineDefine.FolderPath;
                }
                ReleaseEngineSelectionDialog();
            });

            // modal dialogとして出すべき。
            FormLocationUtility.CenteringToThisForm(engineSelectionDialog, this);
            engineSelectionDialog.ShowDialog(Parent);
        }

        /// <summary>
        /// エンジン選択ダイアログの解体
        /// </summary>
        private void ReleaseEngineSelectionDialog()
        {
            if (engineSelectionDialog != null)
            {
                engineSelectionDialog.Dispose();
                engineSelectionDialog = null;
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

        }

        private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
        {
            numericUpDown1.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, System.EventArgs e)
        {
            numericUpDown1.Enabled = true;
        }

        #endregion

        #region private members

        private EngineSelectionDialog engineSelectionDialog;
        private ImageLoader banner_mini;
        private ControlBinder binder = new ControlBinder();
        private ConsiderationEngineSetting Setting;

        #endregion

    }
}
