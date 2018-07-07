using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.EngineDefine;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// エンジン選択ダイアログ用のControl。
    /// 1つのエンジン分の情報を表示する。
    /// </summary>
    public partial class EngineSelectionControl : UserControl
    {
        public EngineSelectionControl()
        {
            InitializeComponent();

            ViewModel.AddPropertyChangedHandler("EngineDefine", EngineDefineChanged , this);
        }

        public class EngineSelectionViewModel : NotifyObject
        {
            /// <summary>
            /// Engine設定。これが画面に反映される。
            /// </summary>
            public EngineDefine EngineDefine
            {
                get { return GetValue<EngineDefine>("EngineDefine"); }
                set { SetValue<EngineDefine>("EngineDefine", value); }
            }
        }

        public EngineSelectionViewModel ViewModel = new EngineSelectionViewModel();

        private void EngineDefineChanged(PropertyChangedEventArgs args)
        {
            var engineDefine = args.value;
        }

    }
}
