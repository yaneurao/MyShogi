using System.Drawing;
using System.Windows.Forms;
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
            gameScreen.SetButton = SetButton;
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

    }
}

