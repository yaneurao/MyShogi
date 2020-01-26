using System;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.View.Win2D
{
    public partial class EngineSubSettingDialog : Form
    {
        public EngineSubSettingDialog()
        {
            InitializeComponent();

            BindControl();
            Disposed += OnDisposed;

            FontUtility.ReplaceFont(this, TheApp.app.Config.FontManager.SettingDialog);
        }

        private void BindControl()
        {
            var config = TheApp.app.Config;

            // --- タイムアウト時間

            binder.Bind(config, "UsiOkTimeOut", numericUpDown1);

        //  binder.Bind(config, "ReadyOkTimeOut", numericUpDown2);
        // →　これ、よくないアイデアであった。
        // cf. USIプロトコルでisready後の初期化に時間がかかる時にどうすれば良いのか？
        //     http://yaneuraou.yaneu.com/2020/01/05/usi%e3%83%97%e3%83%ad%e3%83%88%e3%82%b3%e3%83%ab%e3%81%a7isready%e5%be%8c%e3%81%ae%e5%88%9d%e6%9c%9f%e5%8c%96%e3%81%ab%e6%99%82%e9%96%93%e3%81%8c%e3%81%8b%e3%81%8b%e3%82%8b%e6%99%82%e3%81%ab%e3%81%a9/


        }

        private void OnDisposed(object sender, EventArgs e)
        {
            // data-bindしていたものすべてを解除する。
            binder.UnbindAll();
        }

        private ControlBinder binder = new ControlBinder();

    }
}
