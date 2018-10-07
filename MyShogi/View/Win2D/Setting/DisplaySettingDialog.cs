using MyShogi.App;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Setting
{
    public partial class DisplaySettingDialog : Form
    {
        public DisplaySettingDialog()
        {
            InitializeComponent();

            InitViewModel();
        }

        private void InitViewModel()
        {
            var config = TheApp.app.Config;
            richSelector1.Bind(config, "BoardNumberImageVersion");

        }

    }
}
