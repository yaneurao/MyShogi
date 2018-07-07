using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局盤面を描画してユーザーの入力を受け付けるUserControl。
    /// 
    /// ソースコード、結構あるので、残りは
    /// 　View/Win2D/GameScreenControl/
    /// に入れてある。
    /// </summary>
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
