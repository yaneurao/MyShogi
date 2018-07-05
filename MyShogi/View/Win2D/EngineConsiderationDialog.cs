using System;
using System.Drawing;
using System.Windows.Forms;
//using MyShogi.App;
//using MyShogi.Model.Common.Win32API;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Data;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 思考エンジンの出力(最大 2CPU分) + ミニ盤面
    /// </summary>
    public partial class EngineConsiderationDialog : Form
    {
        public EngineConsiderationDialog()
        {
            InitializeComponent();

            // これ、どうするか考え中。
            //Win32API.HideCloseButton(this.Handle);

            InitSpliter();
            InitEngineConsiderationControl();
        }

        /// <summary>
        /// ミニ盤面の初期化
        /// 必ず呼び出すべし。
        /// </summary>
        public void Init(bool boardReverse)
        {
            miniShogiBoard1.Init(boardReverse);
        }

        // -- properties

        /// <summary>
        /// MiniBoardの表示、非表示を切り替えます。
        /// </summary>
        public bool MiniBoardVisible
        {
            set {
                splitContainer2.Panel2.Visible = value;
                splitContainer2.Panel2Collapsed = !value;
                splitContainer2.IsSplitterFixed = !value;

                // MiniBoard、スレッドが回っているわけでもないし、
                // 画面が消えていれば更新通知等、来ないのでは？
            }
        }

        /// <summary>
        /// 読み筋を表示するコントロールのinstanceを返す。
        /// </summary>
        /// <param name="n">
        /// 
        /// n = 0 : 先手用
        /// n = 1 : 後手用
        /// 
        /// ただし、SetEngineInstanceNumber(1)と設定されていれば、
        /// 表示されているのは1つのみであり、先手用のほうしかない。
        /// 
        /// </param>
        /// <returns></returns>
        public EngineConsiderationControl ConsiderationInstance(int n)
        {
            switch (n)
            {
                case 0: return engineConsiderationControl1;
                case 1: return engineConsiderationControl2;
            }
            return null;
        }

        /// <summary>
        /// エンジンのインスタンス数を設定する。
        /// この数だけエンジンの読み筋を表示する。
        /// </summary>
        /// <param name="n"></param>
        public void SetEngineInstanceNumber(int n)
        {
            if (n == 1)
            {
                splitContainer1.Panel2.Visible = false;
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.IsSplitterFixed = true;
            }
            else if (n == 2)
            {
                splitContainer1.Panel2.Visible = true;
                splitContainer1.Panel2Collapsed = false;
                splitContainer1.IsSplitterFixed = false;
            }
        }


        // -- handlers

        private void splitContainer2_Resize(object sender, EventArgs e)
        {
            UpdateBoardHeight(true);
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            // splitterの位置調整を行ってしまうと無限再帰になってしまうので、
            // この時はsplitterの位置調整は行わない。
            UpdateBoardHeight(false);
        }

        private void EngineConsiderationDialog_Resize(object sender, EventArgs e)
        {
            InitSpliter2Position();
        }

        private void EngineConsiderationDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

            //TheApp.app.MessageShow("この検討ウィンドウの表示/非表示は、メニューの「ウィンドウ」の設定に依存します。×ボタンで閉じることは出来ません。");
            // MessageShow()は、モーダルなので、ここに制御が戻ってこれずhang upしてしまう。

            // cancelして非表示にして隠しておく。
            e.Cancel = true;
            Visible = false;

            // この瞬間に内容をクリアしておかないと再表示した時に前のものが残っていて紛らわしい。
            foreach (var i in All.Int(2))
            {
                ConsiderationInstance(i).ClearHeader();
                ConsiderationInstance(i).ClearItems();
            }
        }

        // 以下 ミニ盤面用のボタン

        /// <summary>
        /// 巻き戻しボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var kifu = miniShogiBoard1.kifuControl;
            kifu.KifuListSelectedIndex = 1;
        }

        /// <summary>
        /// 1手戻しボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            // 1手目より巻き戻さない。
            var kifu = miniShogiBoard1.kifuControl;
            kifu.KifuListSelectedIndex = Math.Max(kifu.KifuListSelectedIndex - 1, 1);
        }

        /// <summary>
        /// 1手進めボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            miniShogiBoard1.BoardForward();
        }

        /// <summary>
        /// 早送りボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            miniShogiBoard1.BoardGotoLeaf();
        }

        /// <summary>
        /// 閉じるボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            MiniBoardVisible = false;

            //AddInfoTest();
            //BoardSetTest();
        }

        /// <summary>
        /// 盤面反転
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            var gameServer = miniShogiBoard1.gameServer;
            if (gameServer != null)
                gameServer.BoardReverse ^= true;
        }

        // -- design adjustment

        /// <summary>
        /// spliterに関して、基本的なレイアウト設定にする。
        /// </summary>
        private void InitSpliter()
        {
            var h = splitContainer1.Height;
            var sh = splitContainer1.SplitterWidth;
            splitContainer1.SplitterDistance = (h - sh) / 2; // ちょうど真ん中に

            InitSpliter2Position();
        }

        /// <summary>
        /// エンジンの思考表示controlの初期化
        /// </summary>
        private void InitEngineConsiderationControl()
        {
            SetEngineInstanceNumber(1);

            foreach (var i in All.Int(2))
                ConsiderationInstance(i).Notify.AddPropertyChangedHandler( "PvClicked" , (h) =>
                {
                    var data = h.value as MiniShogiBoardData;

                    MiniBoardVisible = true;
                    miniShogiBoard1.BoardData = data;
                });
        }

        /// <summary>
        /// ミニ盤面の縦横比
        /// </summary>
        float aspect_ratio = 1.05f;
        //float aspect_ratio = 1.5f; // debug用に幅広くしておくと、棋譜ウィンドウが表示されるのでデバッグが捗る。

        /// <summary>
        /// splitContainer2のsplitterの位置を調整する。
        /// </summary>
        private void InitSpliter2Position()
        {
            var board_height = Math.Max(ClientSize.Height - toolStrip1.Height, 1);

            // 継ぎ盤があるなら、その領域は最大でも横幅の1/4まで。
            var board_width = Math.Max((int)(board_height * aspect_ratio), 1);
            var max_board_width = Math.Max(ClientSize.Width * 1 / 4, 1);

            if (board_width > max_board_width)
            {
                board_width = max_board_width;
                // 制限した結果、画面に収まらなくなっている可能性がある。
                board_height = Math.Max((int)(board_width / aspect_ratio), 1);
            }

            int dist = ClientSize.Width - splitContainer2.SplitterWidth - board_width;
            splitContainer2.SplitterDistance = Math.Max(dist, 1);

            DockMiniBoard(board_width, board_height);
        }

        /// <summary>
        /// ユーザーのSplitterの操作に対して、MiniBoardの高さを調整する。
        /// splitterAdjest : splitterの位置の調整も行うのか？
        /// </summary>
        private void UpdateBoardHeight(bool splitterAdjest)
        {
            var board_width = Math.Max(ClientSize.Width - splitContainer2.SplitterWidth - splitContainer2.SplitterDistance, 1);
            var max_board_height = Math.Max(ClientSize.Height - toolStrip1.Height, 1);
            var board_height = Math.Max((int)(board_width / aspect_ratio), 1);

            if (board_height > max_board_height)
            {
                board_height = max_board_height;
                board_width = Math.Max((int)(board_height * aspect_ratio), 1);

                if (splitterAdjest)
                {
                    // 横幅減ったはず。spliterの右側、無駄であるから、詰める。
                    int dist = ClientSize.Width - splitContainer2.SplitterWidth - board_width;
                    splitContainer2.SplitterDistance = Math.Max(dist, 1);
                }
            }

            DockMiniBoard(board_width, board_height);
        }

        /// <summary>
        /// miniShogiBoardをToolStripのすぐ上に配置する。
        /// </summary>
        private void DockMiniBoard(int board_width, int board_height)
        {
            // miniShogiBoardをToolStripのすぐ上に配置する。
            int y = ClientSize.Height - board_height - toolStrip1.Height;
            miniShogiBoard1.Size = new Size(board_width, board_height);
            miniShogiBoard1.Location = new Point(0, y);
        }

        // -- test code


#if false
        /// <summary>
        /// AddInfoTest()で使う、カウンター
        /// </summary>
        private int add_info_test_number = 0;

        /// <summary>
        /// 適当に読み筋をAddInfoしてやるテスト。
        /// </summary>
        private void AddInfoTest()
        {
            if (add_info_test_number == 0)
                engineConsiderationControl1.RootSfen = BoardType.NoHandicap.ToSfen();

            List<Move> moves;
            switch (add_info_test_number++ % 6)
            {
                case 0: moves = new List<Move>() { Util.MakeMove(Square.SQ_77, Square.SQ_76), Util.MakeMove(Square.SQ_33, Square.SQ_34) }; break;
                case 1: moves = new List<Move>() { Util.MakeMove(Square.SQ_27, Square.SQ_26), Util.MakeMove(Square.SQ_33, Square.SQ_34), Util.MakeMove(Square.SQ_55, Square.SQ_34) }; break;
                case 2: moves = new List<Move>() { Util.MakeMove(Square.SQ_57, Square.SQ_56), Util.MakeMove(Square.SQ_83, Square.SQ_84) }; break;
                case 3: moves = new List<Move>() { Util.MakeMove(Square.SQ_37, Square.SQ_36), Util.MakeMove(Square.SQ_51, Square.SQ_52) ,
                    Util.MakeMove(Square.SQ_57, Square.SQ_56), Util.MakeMove(Square.SQ_83, Square.SQ_84)}; break;
                case 4:
                    moves = new List<Move>() { Util.MakeMove(Square.SQ_37, Square.SQ_36), Util.MakeMove(Square.SQ_51, Square.SQ_52) ,
                    Util.MakeMove(Square.SQ_57, Square.SQ_56), Util.MakeMove(Square.SQ_83, Square.SQ_84)}; break;
                case 5:
                    moves = new List<Move>() { Util.MakeMove(Square.SQ_47, Square.SQ_46), Util.MakeMove(Square.SQ_51, Square.SQ_52) ,
                    Util.MakeMove(Square.SQ_57, Square.SQ_56), Util.MakeMove(Square.SQ_83, Square.SQ_84)}; break;
                default: moves = null; break;
            }

            engineConsiderationControl1.AddThinkReport(new UsiThinkReport()
            {
                ThinkingTime = new TimeSpan(0, 0, 3),
                Depth = 15,
                SelDepth = 20,
                Nodes = 123456789,
                Eval = EvalValue.Mate - 1 /*(EvalValue)1234*/,
                Moves = moves
            });
        }

        /// <summary>
        /// ミニ盤面に試しに局面をセットしてみるテスト用のコード
        /// Init()のなかではまだminiShogiBoard1のhandleが初期化されてないのでInit()のなかから呼び出すのは不可。
        /// </summary>
        public void BoardSetTest()
        {
            miniShogiBoard1.BoardData = new MiniShogiBoardData()
            {
                rootSfen = BoardType.NoHandicap.ToSfen(),
                moves = new List<Move>() { Util.MakeMove(Square.SQ_77, Square.SQ_76), Util.MakeMove(Square.SQ_33, Square.SQ_34) }
            };
        }
#endif

    }
}
