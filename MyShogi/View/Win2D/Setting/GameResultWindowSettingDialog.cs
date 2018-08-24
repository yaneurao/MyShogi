using System;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.View.Win2D.Setting
{
    public partial class GameResultWindowSettingDialog: Form
    {
        /// <summary>
        /// 対局結果ウィンドウ設定ダイアログ
        ///
        ///   自動保存するか
        ///   保存先のフォルダ
        ///   保存件数上限
        ///   
        /// を設定できるようにする。
        ///
        /// </summary>
        public GameResultWindowSettingDialog()
        {
            InitializeComponent();

            // TheApp.app.config.GameResultSettingを、このFormのControlたちとデータバインドしておく。
            BindSetting();

            Disposed += OnDisposed;
        }


        // comboBox1に格納されているのがサポートしている保存する棋譜の形式。
        // これは、KifuTypeのKifuFileTypeのKIF～JSONまで。SVGとUNKNOWN以外。

        private void BindSetting()
        {

        }

        private void OnDisposed(object sender, EventArgs e)
        {
            binder.UnbindAll();
        }

        private ControlBinder binder = new ControlBinder();
    }
}
