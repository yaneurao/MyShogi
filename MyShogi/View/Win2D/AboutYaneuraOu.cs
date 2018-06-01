using MyShogi.App;
using System.IO;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class AboutYaneuraOu : Form
    {
        public AboutYaneuraOu()
        {
            InitializeComponent();

            var file_name = 
                TheApp.app.config.CommercialVersion ?
                "html/about_dialog_commercial_version.html":
                "html/about_dialog_opensource_version.html";

            webBrowser1.Navigate(Path.Combine(Application.StartupPath,file_name).ToString());

            // -- バージョン文字列をJavaScript経由でインジェクションする。

            // 1回メッセージループを処理しないとNavigate()が終わらない。
            Application.DoEvents();

            var version_string = "MyShogi Version " + GlobalConfig.MYSHOGI_VERSION_STRING;
            webBrowser1.Document.InvokeScript("add_version_string", new string[] { version_string });
        }
    }
}
