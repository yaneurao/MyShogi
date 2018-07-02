using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Data;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class EngineConsiderationControl : UserControl
    {
        public EngineConsiderationControl()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            listView1.FullRowSelect = true;
            //listView1.GridLines = true;
            listView1.Sorting = SortOrder.None;
            listView1.View = System.Windows.Forms.View.Details;

            // ヘッダーのテキストだけセンタリング、実項目は右寄せしたいのだが、これをするには
            // オーナードローにする必要がある。面倒くさいので、ヘッダーのテキストにpaddingしておく。
            // またヘッダーの1列目のTextAlignは無視される。これは.NET FrameworkのListViewの昔からあるバグ。(仕様？)

            var thinking_time = new ColumnHeader();
            thinking_time.Text = "思考時間";
            thinking_time.Width = 140;
            thinking_time.TextAlign = HorizontalAlignment.Right;

            var depth = new ColumnHeader();
            depth.Text = "深さ ";
            depth.Width = 100;
            depth.TextAlign = HorizontalAlignment.Right;

            var node = new ColumnHeader();
            node.Text = "ノード数  ";
            node.Width = 180;
            node.TextAlign = HorizontalAlignment.Right;

            var eval = new ColumnHeader();
            eval.Text = "評価値  ";
            eval.Width = 150;
            eval.TextAlign = HorizontalAlignment.Right;

            var pv = new ColumnHeader();
            pv.Text = "読み筋";
            pv.Width = 0;
            pv.TextAlign = HorizontalAlignment.Left;
            // 読み筋の幅は残り全部。UpdatePvWidth()で調整される。

            var header = new[] { thinking_time , depth, node, eval , pv };

            listView1.Columns.AddRange(header);
        }

        // -- properties

        /// <summary>
        /// 開始局面のsfen。
        /// これをセットしてからでないとAddInfo()してはならない。
        /// </summary>
        /// <param name=""></param>
        public void SetRootSfen(string root_sfen)
        {
            position.SetSfen(root_sfen);
        }

        /// <summary>
        /// 読み筋を1行追加する。
        /// </summary>
        /// <param name="info"></param>
        public void AddInfo(EngineConsiderationData info)
        {
            var kifuString = "なんか";
            var list = new []{ info.ThinkingTime.ToString() , $"{info.Depth}/{info.SelDepth}" , info.Nodes.ToString() ,
                info.Eval.Pretty(), kifuString };
            listView1.Items.Add(new ListViewItem(list));
        }

        // -- privates

        /// <summary>
        /// 「読み筋」の列の幅を調整する。
        /// </summary>
        private void UpdatePvWidth()
        {
            int sum_width = 0;
            int i = 0;
            for (; i < listView1.Columns.Count - 1; ++i)
                sum_width += listView1.Columns[i].Width;

            // Columnsの末尾が「読み筋」の表示であるから、この部分は、残りの幅全部にしてやる。
            listView1.Columns[i /* is the last index*/].Width = ClientSize.Width - sum_width;
        }

        private void listView1_Resize(object sender, System.EventArgs e)
        {
            UpdatePvWidth();
        }

        /// <summary>
        /// 内部に棋譜文字列の構築用に局面クラスを持っている。
        /// </summary>
        private Position position = new Position();
    }
}
