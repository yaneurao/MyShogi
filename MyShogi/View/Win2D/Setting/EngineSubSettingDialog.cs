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
            binder.Bind(config, "ReadyOkTimeOut", numericUpDown2);

        }

        private void OnDisposed(object sender, EventArgs e)
        {
            // data-bindしていたものすべてを解除する。
            binder.UnbindAll();
        }

        private ControlBinder binder = new ControlBinder();

    }
}
