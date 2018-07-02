using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class EngineConsiderationDialog : Form
    {
        public EngineConsiderationDialog()
        {
            InitializeComponent();

            InitSpliter();
            SetEngineInstanceNumber(1);
        }

        /// <summary>
        /// 基本的なレイアウト設定にする。
        /// </summary>
        private void InitSpliter()
        {
            var h = splitContainer1.Height;
            var sh = splitContainer1.SplitterWidth;
            splitContainer1.SplitterDistance = (h - sh) / 2; // ちょうど真ん中に

            InitSpliter2Position();
        }

        /// <summary>
        /// ミニ盤面の縦横比
        /// </summary>
        float aspect_ratio = 1.05f;

        /// <summary>
        /// splitContainer2のsplitterの位置を調整する。
        /// </summary>
        private void InitSpliter2Position()
        {
            var board_height = Math.Max(ClientSize.Height - toolStrip1.Height,1);

            // 継ぎ盤があるなら、その領域は最大でも横幅の1/4まで。
            var board_width = Math.Max((int)(board_height * aspect_ratio) , 1);
            var max_board_width = Math.Max(ClientSize.Width * 1 / 4 , 1);

            if (board_width > max_board_width)
            {
                board_width = max_board_width;
                // 制限した結果、画面に収まらなくなっている可能性がある。
                board_height = Math.Max((int)(board_width / aspect_ratio) , 1);
            }

            int dist = ClientSize.Width - splitContainer2.SplitterWidth - board_width;
            splitContainer2.SplitterDistance = Math.Max(dist,1);

            DockMiniBoard(board_width,board_height);
        }

        /// <summary>
        /// ユーザーのSplitterの操作に対して、MiniBoardの高さを調整する。
        /// splitterAdjest : splitterの位置の調整も行うのか？
        /// </summary>
        private void UpdateBoardHeight(bool splitterAdjest)
        {
            var board_width = Math.Max(ClientSize.Width -  splitContainer2.SplitterWidth - splitContainer2.SplitterDistance , 1);
            var max_board_height = Math.Max(ClientSize.Height - toolStrip1.Height,1);
            var board_height = Math.Max((int)(board_width / aspect_ratio),1);

            if (board_height > max_board_height)
            {
                board_height = max_board_height;
                board_width = Math.Max((int)(board_height * aspect_ratio) ,1);

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
        private void DockMiniBoard(int board_width , int board_height)
        {
            // miniShogiBoardをToolStripのすぐ上に配置する。
            int y = ClientSize.Height - board_height - toolStrip1.Height;
            miniShogiBoard1.Size = new Size(board_width, board_height);
            miniShogiBoard1.Location = new Point(0, y);
        }

        /// <summary>
        /// MiniBoardの表示、非表示を切り替えます。
        /// </summary>
        public bool MiniBoardVisiblity
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
        public EngineConsiderationControl GetConsiderationInstance(int n)
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

        /// <summary>
        /// ミニ盤面の初期化
        /// </summary>
        public void Init()
        {
            miniShogiBoard1.Init();
        }

        /// <summary>
        /// ミニ盤面に試しに局面をセットしてみるテスト用のコード
        /// Init()のなかではまだminiShogiBoard1のhandleが初期化されてないのでInit()のなかから呼び出すのは不可。
        /// </summary>
        public void BoardSetTest()
        {
            miniShogiBoard1.BoardData = new MiniShogiBoardData()
            {
                BoardReverse = false,
                rootSfen = BoardType.NoHandicap.ToSfen(),
                moves = new List<Move>() { Util.MakeMove(Square.SQ_77, Square.SQ_76), Util.MakeMove(Square.SQ_33, Square.SQ_34) }
            };
        }

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

        /// <summary>
        /// 巻き戻しボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            miniShogiBoard1.BoardGotoRoot();
        }

        /// <summary>
        /// 1手戻しボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            miniShogiBoard1.BoardRewind();
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
            //MiniBoardVisiblity = false;

            //BoardSetTest();

            engineConsiderationControl1.SetRootSfen(BoardType.NoHandicap.ToSfen());
            engineConsiderationControl1.AddInfo(new EngineConsiderationData()
            {
                ThinkingTime = new TimeSpan(0, 0, 3),
                Depth = 15,
                SelDepth = 20,
                Nodes = 123456789,
                Eval = EvalValue.Mate - 1 /*(EvalValue)1234*/,
                Moves = new List<Move>() { Util.MakeMove(Square.SQ_77, Square.SQ_76), Util.MakeMove(Square.SQ_33, Square.SQ_34) }
            });

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
            {
                gameServer.BoardReverse ^= true;
                miniShogiBoard1.ForceRedraw();
            }
        }
    }
}
