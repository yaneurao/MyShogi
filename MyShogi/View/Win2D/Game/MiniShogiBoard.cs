using System;
using System.Drawing;
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
                gameServer = new LocalGameServer() { NoThread = true, EnableUserMove = false, BoardReverse = boardReverse },
                UpdateMenuItems = null,
                NamePlateVisible = false,
                IgnoreKifuDockState = true,
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

        // -- ボタンのハンドラを外部クラスから呼び出すためのinterface

        /// <summary>
        /// ミニ盤面、一手戻る
        /// </summary>
        public void MiniBoardPerformUp()
        {
            toolStripButton2_Click(null,null);
        }

        /// <summary>
        /// ミニ盤面、一手進む
        /// </summary>
        public void MiniBoardPerformDown()
        {
            toolStripButton3_Click(null, null);
        }

        /// <summary>
        /// ミニ盤面、先頭に戻る
        /// </summary>
        public void MiniBoardPerformHead()
        {
            toolStripButton1_Click(null, null);
        }

        /// <summary>
        /// ミニ盤面、末尾に進む
        /// </summary>
        public void MiniBoardPerformTail()
        {
            toolStripButton4_Click(null, null);
        }

        // -- handlers

        // 以下 ミニ盤面用のボタン

        /// <summary>
        /// 巻き戻しボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var kifu = kifuControl;
            kifu.ViewModel.KifuListSelectedIndex = 1;
        }

        /// <summary>
        /// 1手戻しボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            // 1手目より巻き戻さない。
            var kifu = kifuControl;
            kifu.ViewModel.KifuListSelectedIndex = Math.Max(kifu.ViewModel.KifuListSelectedIndex - 1, 1);
        }

        /// <summary>
        /// 1手進めボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            BoardForward();
        }

        /// <summary>
        /// 早送りボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            BoardGotoLeaf();
        }

        /// <summary>
        /// 盤面反転
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (gameServer != null)
                gameServer.BoardReverse ^= true;
        }

        private void MiniShogiBoard_Resize(object sender, EventArgs e)
        {
            gameScreenControl1.Location = new Point(0, toolStrip1.Height);
            gameScreenControl1.Size = new Size(ClientSize.Width, ClientSize.Height - toolStrip1.Height);
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
