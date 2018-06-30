using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class GameScreenControl : UserControl
    {
        public GameScreenControl()
        {
            InitializeComponent();

            // 棋譜コントロールのハンドラを設定する。
            SetKifuControlHandler();
        }
    }
}
