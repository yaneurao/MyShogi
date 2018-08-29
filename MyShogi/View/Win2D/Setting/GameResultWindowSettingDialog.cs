using System;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.View.Win2D.Setting
{
    public partial class GameResultWindowSettingDialog: Form
    {
        /// <summary>
        /// 対局結果の保存設定ダイアログ
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

            // TheApp.app.Config.GameResultSettingを、このFormのControlたちとデータバインドしておく。
            BindSetting();

            Disposed += OnDisposed;
        }


        // comboBox1に格納されているのがサポートしている保存する棋譜の形式。
        // これは、KifuTypeのKifuFileTypeのKIF～JSONまで。SVGとUNKNOWN以外。

        private void BindSetting()
        {
            var setting = TheApp.app.Config.GameResultSetting;

            binder.Bind(setting, "AutomaticSaveKifu", checkBox1);
            binder.Bind(setting, "KifuFileType"     , comboBox1);
            binder.Bind(setting, "KifuFileLimit"    , numericUpDown1);
            binder.Bind(setting, "KifuSaveFolder"   , textBox1);
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            binder.UnbindAll();
        }

        private ControlBinder binder = new ControlBinder();

        /// <summary>
        /// フォルダの選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "棋譜の自動保存先のフォルダを指定してください。";
                //fbd.RootFolder = ...;

                var default_path = textBox1.Text;
                if (string.IsNullOrEmpty(default_path))
                    default_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YaneuraOuKifu");

                fbd.SelectedPath = default_path;

                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    textBox1.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
