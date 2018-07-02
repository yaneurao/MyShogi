using System.Windows.Forms;
using MyShogi.Model.Shogi.Data;
using MyShogi.Model.Shogi.LocalServer;


namespace MyShogi.View.Win2D
{
    public partial class MiniShogiBoard : UserControl
    {
        /// <summary>
        /// 継ぎ盤用のControl
        /// </summary>
        public MiniShogiBoard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 盤面表示のために持っているGameScreenControlの初期化を行う。
        /// </summary>
        public void Init()
        {
            // GameScreenControlの初期化
            gameScreenControl1.Setting = new GameScreenControlSetting()
            {
                SetButton = null,
                gameServer = new LocalGameServer() { NoThread = true },
                UpdateMenuItems = null,
                NamePlateVisible = false,
            };
            gameScreenControl1.Init();
            gameServer.Start(); // スレッドを別で作らないのでこの時点で開始させて問題ない。
        }

        /// <summary>
        /// 強制的にredrawする。
        /// LocalGameServerをNoThreadで動かしているので、
        /// LocalGameServer.BoardReverseを変更するなどしても画面の再描画が
        /// 行われないので、再描画は自前で行う必要がある。
        /// </summary>
        public void ForceRedraw()
        {
            gameScreenControl1.ForceRedraw();
        }

        // -- 以下、局面操作子

        /// <summary>
        /// [UI Thread] : 開始局面に移動
        /// </summary>
        public void BoardGotoRoot()
        {
            gameScreenControl1.kifuControl.SetKifuListIndex(0);
            ForceRedraw();
        }

        /// <summary>
        /// [UI Thread] : 局面を1手戻す
        /// </summary>
        public void BoardRewind()
        {
            gameScreenControl1.kifuControl.RewindKifuListIndex();
            ForceRedraw();
        }

        /// <summary>
        /// [UI Thread] : 局面を1手進める
        /// </summary>
        public void BoardForward()
        {
            gameScreenControl1.kifuControl.ForwardKifuListIndex();
            ForceRedraw();
        }

        /// <summary>
        /// [UI Thread] : 局面を末尾に進める
        /// </summary>
        public void BoardGotoLeaf()
        {
            gameScreenControl1.kifuControl.SetKifuListIndex(int.MaxValue /* clipされて末尾に行く */);
            ForceRedraw();
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
            set { boardData = value;
                // Controlのpropertyなので、VSのデザイナにより、InitializeComponentでnullがセットされるコードが生成されている。
                // そこでnullの時にはUpdateBoard()を呼び出してはならない。
                if (value != null) UpdateBoard();
            }
        }

        /// <summary>
        /// 盤面を反転させるかどうか。
        /// Init()を呼び出したあとにしか設定/取得できない。
        /// </summary>
        public bool BoardReverse { get { return gameServer.BoardReverse; } set { gameServer.BoardReverse = value; } }

        // -- privates

        /// <summary>
        /// 盤面の更新
        /// BoardDataに従って盤面を更新する。
        /// </summary>
        private void UpdateBoard()
        {
            gameServer.SetBoardDataCommand(BoardData , 1);
            ForceRedraw();
        }

        private MiniShogiBoardData boardData;
    }
}
