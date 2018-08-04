using System.Windows.Forms;
using MyShogi.Model.Shogi.Data;
using MyShogi.Model.Shogi.LocalServer;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 継ぎ盤用のControl
    /// </summary>
    public partial class MiniShogiBoard : UserControl
    {
        public MiniShogiBoard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 盤面表示のために持っているGameScreenControlの初期化を行う。
        /// </summary>
        public void Init(bool boardReverse)
        {
            // GameScreenControlの初期化
            gameScreenControl1.Setting = new GameScreenControlSetting()
            {
                SetButton = null,
                gameServer = new LocalGameServer() { NoThread = true  , EnableUserMove = false , BoardReverse = boardReverse },
                UpdateMenuItems = null,
                NamePlateVisible = false,
            };
            gameScreenControl1.Init();
            gameServer.Start(); // スレッドを別で作らないのでこの時点で開始させて問題ない。
        }

        // -- 以下、局面操作子

        /// <summary>
        /// 棋譜ウィンドウ
        /// </summary>
        public KifuControl kifuControl { get { return gameScreenControl1.kifuControl; } }

        /// <summary>
        /// [UI Thread] : 開始局面に移動
        /// </summary>
        public void BoardGotoRoot()
        {
            gameScreenControl1.kifuControl.ViewModel.KifuListSelectedIndex = 0;
        }

        /// <summary>
        /// [UI Thread] : 局面を1手戻す
        /// </summary>
        public void BoardRewind()
        {
            gameScreenControl1.kifuControl.RewindKifuListIndex();
        }

        /// <summary>
        /// [UI Thread] : 局面を1手進める
        /// </summary>
        public void BoardForward()
        {
            gameScreenControl1.kifuControl.ForwardKifuListIndex();
        }

        /// <summary>
        /// [UI Thread] : 局面を末尾に進める
        /// </summary>
        public void BoardGotoLeaf()
        {
            var kifuVm = gameScreenControl1.kifuControl.ViewModel;
            kifuVm.KifuListSelectedIndex = kifuVm.KifuListCount; // 大きすぎてもclipされて末尾にいくだろうし…。
        }

        // -- properties

        /// <summary>
        /// 描画の設定一式。これに従い、描画がなされている。Init()を呼び出した以降で取得できるようになる。
        /// </summary>
        public GameScreenControlSetting Settings { get { return gameScreenControl1.Setting; } }

        /// <summary>
        /// 外部からSetting.gameServerにアクセスしたい時用。
        /// </summary>
        public LocalGameServer gameServer { get { return Settings==null ? null : Settings.gameServer;} }

        /// <summary>
        /// setterでその情報に基づき、盤面更新がなされる。
        /// </summary>
        public MiniShogiBoardData BoardData {
            get { return boardData; }
            set
            {
                boardData = value;

                // Controlのpropertyなので、VSのデザイナにより、InitializeComponentでnullがセットされるコードが生成されている。
                // そこでnullの時にはUpdateBoard()を呼び出してはならない。
                if (value != null)
                    UpdateBoard();
            }
        }

        // -- privates

        /// <summary>
        /// 盤面の更新
        /// BoardDataに従って盤面を更新する。
        /// </summary>
        private void UpdateBoard()
        {
            /// rootが、思考対象局面になっているので、継ぎ盤の初期状態はrootから1手進めた局面にしたい。
            gameServer.SetBoardDataCommand(BoardData , 1);
        }

        private MiniShogiBoardData boardData;
    }
}
