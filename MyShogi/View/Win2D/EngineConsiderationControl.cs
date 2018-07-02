using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Data;
using System.Text;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class EngineConsiderationControl : UserControl
    {
        public EngineConsiderationControl()
        {
            InitializeComponent();

            InitListView();
            InitKifuFormatter();
        }

        // -- properties

        /// <summary>
        /// 生成する棋譜文字列のフォーマット
        /// </summary>
        public IKifFormatterOptions kifFormatter
        {
            get; set;
        }

        /// <summary>
        /// [UI Thread] : 開始局面のsfen。
        /// これをセットしてからでないとAddInfo()してはならない。
        /// </summary>
        /// <param name=""></param>
        public string RootSfen
        {
            get
            {
                return root_sfen;
            }
            set
            {
                position.SetSfen(value);
                root_sfen = value;
            }
        }

        /// <summary>
        /// [UI Thread] : 読み筋を1行追加する。
        /// </summary>
        /// <param name="info"></param>
        public void AddInfo(EngineConsiderationData info)
        {
            // -- 指し手文字列の構築

            // Positionクラスを用いて指し手文字列を構築しないといけない。
            // UI Threadからしかこのメソッドを呼び出さないことは保証されているので、
            // positionのimmutable性は保つ必要はなく、Position.DoMove()～UndoMove()して良いが、素直にCloneしたほうが速いと思われ..

            var pos = position.Clone();

            var kifuString = new StringBuilder();

            // kifuStringに文字列を追加するlocal method。
            // 文字列を追加するときに句切りのスペースを自動挿入する。
            void append(string s)
            {
                if (kifuString.Length != 0)
                    kifuString.Append(' ');
                kifuString.Append(s);
            }

            foreach(var move in info.Moves)
            {
                if (!pos.IsLegal(move))
                {
                    if (move.IsSpecial())
                        append(move_to_kif_string(pos, move));
                    else
                        // 非合法手に対してKIF2の文字列を生成しないようにする。(それが表示できるとは限らないので..)
                        append($"非合法手({ move.Pretty()})");

                    break;
                }
                append(move_to_kif_string(pos, move));
                pos.DoMove(move);
            }

            // -- listView.Itemsに追加

            var list = new []{
                info.ThinkingTime.ToString() ,    // 思考時間
                $"{info.Depth}/{info.SelDepth}" , // 探索深さ
                info.Nodes.ToString() ,           // ノード数
                info.Eval.Pretty(),               // 評価値
                kifuString.ToString()             // 読み筋
            };
            var item = new ListViewItem(list);
            listView1.Items.Add(item);
            listView1.TopItem = item; // 自動スクロール
        }

        // -- handlers

        private void listView1_Resize(object sender, System.EventArgs e)
        {
            UpdatePvWidth();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            var item = sender as ListViewItem;
            // 思っているところがクリックされたのではなさげ…。
            if (item == null)
                return;

            // TODO : あとでここのハンドラ書く
        }

        // -- privates

        private void InitListView()
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

            var header = new[] { thinking_time, depth, node, eval, pv };

            listView1.Columns.AddRange(header);
        }

        private void InitKifuFormatter()
        {
            kifFormatter = new KifFormatterOptions
            {
                color = ColorFormat.Piece,
                square = SquareFormat.FullWidthMix,
                samepos = SamePosFormat.KI2sp,
                //fromsq = FromSqFormat.Verbose,
                fromsq = FromSqFormat.KI2, // 移動元を入れると棋譜ウィンドウには入り切らないので省略する。
            };
        }

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

        /// <summary>
        /// 指し手を読み筋に表示する棋譜文字列に変換する。
        /// </summary>
        /// <param name="p"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private string move_to_kif_string(Position p, Move m)
        {
            // 特殊な指し手は、KIF2フォーマットではきちんと変換できないので自前で変換する。
            // 例えば、連続王手の千日手による反則勝ちが単に「千日手」となってしまってはまずいので。
            // (『Kifu for Windoiws』ではそうなってしまう..)
            return m.IsOk() ? kifFormatter.format(p, m) : m.SpecialMoveToKif();
        }


        /// <summary>
        /// 開始局面のsfen。
        /// この文字列とpositionの居面は合致している。
        /// RootSfenのsetterでセットされる。
        /// </summary>
        private string root_sfen;

        /// <summary>
        /// 内部に棋譜文字列の構築用に局面クラスを持っている。
        /// RootSfenのsetterでセットされる。
        /// </summary>
        private Position position = new Position();

    }
}
