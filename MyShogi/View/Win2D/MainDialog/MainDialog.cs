using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Shogi.Core;
using MyShogi.ViewModel;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局盤面などがあるメインウィンドゥ
    /// </summary>
    public partial class MainDialog : Form
    {
        public MainDialog()
        {
            InitializeComponent();

            // あとで複数インスタンスに対応させる
            gameScreen = new GameScreen();
            gameScreen.Init(this);

            UpdateMenuItems();

            FitToScreenSize();
            FitToClientSize();

            MinimumSize = new Size(192 * 2, 108 * 2 + menu_height);
        }

        /// <summary>
        /// このViewに対応するViewModel
        /// このクラスをnewした時にViewModelのインスタンスと関連付ける。
        /// </summary>
        public MainDialogViewModel ViewModel
        {
            get { return gameScreen.ViewModel.ViewModel; }
            set { gameScreen.ViewModel.ViewModel = value; }
        }

        /// <summary>
        /// 描画のときに必要となる、Viewに属する情報
        /// 1つのViewInstanceと1つのViewModelが対応する。
        /// </summary>
        public GameScreen gameScreen { get; private set; }

        /// <summary>
        /// 対局盤面の描画関係のコード一式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDialog_Paint(object sender, PaintEventArgs e)
        {
            // 描画は丸ごと、GameScreenに移譲してある。
            // マルチスクリーン対応にするときは、GameScreenのインスタンスを作成して、
            // それぞれに移譲すれば良い。
            gameScreen.OnDraw(e.Graphics);
        }
    }
}

