using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// DoubleBuffer = trueなListView
    /// </summary>
    public class ListViewEx : ListView
    {
        public ListViewEx()
        {
            // このフラグはprotectedなので変更するにはListView派生クラスを作るしかない。
            DoubleBuffered = true;
        }
    }
}
