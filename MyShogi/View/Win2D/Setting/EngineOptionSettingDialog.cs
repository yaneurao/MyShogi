using System;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Setting
{
    public partial class EngineOptionSettingDialog : Form
    {
        public EngineOptionSettingDialog()
        {
            InitializeComponent();
        }

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

    }
}
