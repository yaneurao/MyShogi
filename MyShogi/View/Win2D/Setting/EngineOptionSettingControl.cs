using System.Collections.Generic;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Usi;

namespace MyShogi.View.Win2D.Setting
{
    public partial class EngineOptionSettingControl : UserControl
    {
        public EngineOptionSettingControl()
        {
            InitializeComponent();
        }

        public class EngineOptionSettingViewModel : NotifyObject
        {
            /// <summary>
            /// この内容に従って描画がなされる。
            /// 表示しているコントロールとdata bindされている。
            /// </summary>
            public List<UsiOption> UsiOptions
            {
                get { return GetValue<List<UsiOption>>("UsiOptions"); }
                set { SetValue<List<UsiOption>>("UsiOptions",value); }
            }
        }

        public EngineOptionSettingViewModel ViewModel = new EngineOptionSettingViewModel();
    }
}
