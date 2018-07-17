using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.View.Win2D.Setting
{
    public partial class EngineOptionSettingDialog : Form
    {
        public EngineOptionSettingDialog()
        {
            InitializeComponent();

            InitViewModel();
        }

        public class EngineOptionSettingDialogViewModel : NotifyObject
        {
            /// <summary>
            /// いま個別設定を行っているエンジン名。これがCaptionかどこかに表示される。
            /// </summary>
            public string EngineDisplayName
            {
                get { return GetValue<string>("EngineDisplayName"); }
                set { SetValue<string>("EngineDisplayName", value); }
            }
        }

        public EngineOptionSettingDialogViewModel ViewModel = new EngineOptionSettingDialogViewModel();


        /// <summary>
        /// Tab内に持っているEngineOptionSettingControlを返す。
        /// 
        /// index == 0 : 共通設定
        /// index == 1 : 個別設定
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public EngineOptionSettingControl SettingControls(int index)
        {
            return index == 0 ? engineOptionSettingControl1 : engineOptionSettingControl2;
        }


        private void InitViewModel()
        {

            ViewModel.AddPropertyChangedHandler("EngineDisplayName", (args) =>
            {
                //this.Text = (string)args.value;
                tabPage2.Text = $"詳細設定({(string)args.value})";
            });
        }

    }
}
