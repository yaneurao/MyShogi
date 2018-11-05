using System.Windows.Forms;
using MyShogi.App;

namespace MyShogi.View.Win2D.Setting
{
    public partial class OperationSettingDialog : Form
    {
        /// <summary>
        /// 
        /// 表示設定ダイアログ
        /// 
        /// </summary>
        public OperationSettingDialog()
        {
            InitializeComponent();

            InitViewModel();

            // フォントの変更。即時反映
            var fontSetter = new FontSetter(this, "SettingDialog");
            Disposed += (sender,args) => fontSetter.Dispose();
        }

        private void InitViewModel()
        {
            var config = TheApp.app.Config;

            // RichSelectorに丸投げできるので、ここではBindの設定をするだけで良い。

            // -- 「駒」のタブ

            // マウスドラッグを許容するか
            richSelector1.Bind(config, "EnableMouseDrag");

            // -- 「棋譜」のタブ

            richSelector2.Bind(config, "KifuWindowPrevNextKey");
            richSelector3.Bind(config, "KifuWindowNextSpecialKey");
            richSelector4.Bind(config, "KifuWindowFirstLastKey");

            // -- 「検討」のタブ

            richSelector5.Bind(config, "ConsiderationWindowPrevNextKey");
            richSelector6.Bind(config, "ConsiderationPvSendKey");

        }

    }
}
