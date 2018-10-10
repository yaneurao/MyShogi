using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.EngineDefine;

namespace MyShogi.View.Win2D
{
    public partial class EngineOptionSettingDialog : Form
    {
        /// <summary>
        /// 
        /// 注意)
        /// Visual StudioのデザイナでこのDialogを編集するときは
        ///   AutoScale = Size(96F,96F)
        /// で編集しなければならない。
        /// 
        /// high dpi環境で編集して(192F,192F)とかになっていると、
        /// 解像度の低い実行環境でダイアログの大きさが小さくなってしまう。
        /// (.NET Frameworkのhigh dpiのバグ)
        /// 
        /// </summary>
        public EngineOptionSettingDialog()
        {
            InitializeComponent();

            InitViewModel();

            InitFont();
        }

        public class EngineOptionSettingDialogViewModel : NotifyObject
        {
            /// <summary>
            /// いま個別設定を行っているエンジン名。
            /// これを設定するとTabPageのTextに表示される。
            /// </summary>
            public string EngineDisplayName
            {
                get { return GetValue<string>("EngineDisplayName"); }
                set { SetValue<string>("EngineDisplayName", value); }
            }

            /// <summary>
            /// いま共通設定・個別設定を行っている設定種別。
            /// (通常対局用か、検討用か、詰将棋用か)
            /// 
            /// これを設定すると、ダイアログのCaptionに反映される。
            /// </summary>
            public EngineConfigType EngineConfigType
            {
                get { return GetValue<EngineConfigType>("EngineConfigType"); }
                set { SetValue<EngineConfigType>("EngineConfigType", value); }
            }
        }

        public EngineOptionSettingDialogViewModel ViewModel = new EngineOptionSettingDialogViewModel();


        /// <summary>
        /// このダイアログが、TabControl内に持っているEngineOptionSettingControlを返す。
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

            ViewModel.AddPropertyChangedHandler("EngineConfigType", (args) =>
            {
                var configType = (EngineConfigType)args.value;
                this.Text = $"エンジンオプション設定({configType.Pretty()}用)";
            });
        }

        /// <summary>
        /// フォントの初期化
        /// </summary>
        private void InitFont()
        {
            FontUtility.ReplaceFont(this, TheApp.app.Config.FontManager.SettingDialog);
        }

    }
}
