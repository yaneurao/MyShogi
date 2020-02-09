using MyShogi.App;
using MyShogi.Model.Shogi.EngineDefine;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Setting
{
    public partial class EngineDefineEditDialog : Form
    {
        public EngineDefineEditDialog()
        {
            InitializeComponent();

            InitEngineDefine();

            // フォント変更。
            FontUtility.ReplaceFont(this, TheApp.app.Config.FontManager.SettingDialog);
        }

        #region property

        /// <summary>
        /// 編集中のエンジン定義ファイル
        /// </summary>
        public EngineDefine engineDefine;

        #endregion

        #region private method

        /// <summary>
        /// engineDefineを初期化する。
        /// デフォルト値を突っ込む＆データバインドする。
        /// </summary>
        private void InitEngineDefine()
        {
            InitEngineDefault();
        }

        /// <summary>
        /// エンジン設定のデフォルト値を突っ込む。
        /// </summary>
        private void InitEngineDefault()
        {
            engineDefine = new EngineDefine();
        }

        #endregion

    }
}
