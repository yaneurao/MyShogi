using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Data;
using MyShogi.Model.Shogi.Usi;
using MyShogi.Model.Common.Utility;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// エンジンの思考内容。
    /// 片側のエンジン分
    /// </summary>
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
                root_sfen = value;
                listView1.Items.Clear();
                list_item_moves.Clear();

                if (root_sfen != null)
                    position.SetSfen(value);
            }
        }

        /// <summary>
        /// [UI Thread] : エンジン名を設定/取得する。
        /// 
        /// このコントロールの左上のテキストボックスに反映される。
        /// setterでは、ヘッダー情報のクリアも行う。
        /// </summary>
        public string EngineName
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; ClearHeader();  }
        }

        /// <summary>
        /// アイテムがクリックされた時に、そこに表示されている読み筋と、rootSfenがもらえるイベントハンドラのdelegate
        /// </summary>
        /// <param name="rootSfen"></param>
        /// <param name="moves"></param>
        public delegate void ItemClickedEventHandler(MiniShogiBoardData data);

        /// <summary>
        /// アイテムがクリックされた時に、そこに表示されている読み筋と、rootSfenがもらえるイベントハンドラ
        /// </summary>
        /// <param name="rootSfen"></param>
        /// <param name="moves"></param>
        public ItemClickedEventHandler ItemClicked { get; set; }

        /// <summary>
        /// [UI Thread] : 読み筋を1行追加する。
        /// </summary>
        /// <param name="info"></param>
        public void AddThinkReport(UsiThinkReport info)
        {
            if (info.Moves != null || info.InfoString != null)
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

                if (info.Moves != null)
                {
                    var moves = new List<Move>();
                    foreach (var move in info.Moves)
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
                        moves.Add(move);
                        // このあと盤面表示用にmovesを保存するが、
                        // 非合法局面の指し手を渡すことは出来ないので、合法だとわかっている指し手のみを保存しておく。

                        pos.DoMove(move);
                    }
                }
                else
                {
                    kifuString.Append(info.InfoString); // この文字列を読み筋として突っ込む。
                }

                // -- listView.Itemsに追加

                // それぞれの項目、nullである可能性を考慮しながら表示すべし。
                // 経過時間、1/10秒まで表示する。
                var elpasedTimeString = info.ElapsedTime != null ? info.ElapsedTime.ToString("hh':'mm':'ss'.'f") : null;

                var list = new[]{
                    elpasedTimeString,                // 思考時間
                    $"{info.Depth}/{info.SelDepth}" , // 探索深さ
                    info.NodesString ,                // ノード数
                    info.Eval.Eval.Pretty(),          // 評価値
                    kifuString.ToString()             // 読み筋
                };

                var item = new ListViewItem(list);
                listView1.Items.Add(item);
                listView1.TopItem = item; // 自動スクロール

                // 読み筋をここに保存しておく。(ミニ盤面で開く用)
                // なければnullもありうる。
                list_item_moves.Add(info.Moves);
            }

            // -- その他、nullでない項目に関して、ヘッダー情報のところに反映させておく。

            UpdateHeader(info);
        }

        /// <summary>
        /// [UI Thread] : ヘッダー情報をクリアする。
        /// </summary>
        public void ClearHeader()
        {
            UpdateHeader(new UsiThinkReport()
            {
                PonderMove = "",
                NodesString = "",
                NpsString = "",
                HashPercentageString = "",
            });
        }


        // -- handlers

        private void listView1_Resize(object sender, System.EventArgs e)
        {
            UpdatePvWidth();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            // この現在選択されているところにある読み筋の指し手を復元して、イベントハンドラに移譲する。
            var selected = listView1.SelectedIndices;
            if (selected.Count == 0)
                return;// 選択されていない…

            // multi selectではないので1つしか選択されていないはず…。
            int index = selected[0]; // first
            if (index < list_item_moves.Count && list_item_moves[index]!=null /* info stringなどだとnullがありうる。*/)
                ItemClicked?.Invoke(new MiniShogiBoardData()
                {
                    rootSfen = root_sfen,
                    moves = list_item_moves[index]
                });
        }

        private void EngineConsiderationControl_Resize(object sender, System.EventArgs e)
        {
            int h = textBox1.Height + 3;
            listView1.Location = new Point(0, h);
            listView1.Size = new Size(ClientSize.Width, ClientSize.Height - h);
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
            thinking_time.Width = 150;
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

        /// <summary>
        /// 読み筋のところに表示する棋譜文字列の生成器の初期化
        /// </summary>
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
        /// [UI Thread] : ヘッダー情報のところに反映させる。
        /// nullの項目は書き換えない。
        /// </summary>
        /// <param name="info"></param>
        private void UpdateHeader(UsiThinkReport info)
        {
            // .NET FrameworkのTextBox、右端にスペースをpaddingしていて、TextAlignをcenterに設定してもそのスペースを
            // わざわざ除去してからセンタリングするので(余計なお世話)、TextAlignをLeftに設定して、自前でpaddingする。

            //textBox1.Text = info.PlayerName;

            if (info.PonderMove != null)
                textBox2.Text = $" 予想手 : { info.PonderMove.PadLeftUnicode(6)}";

            //textBox3.Text = $"探索手：{info.SearchingMove}";
            // 探索手、エンジン側、まともに出力してると出力だけで時間すごくロスするので
            // 出力してくるエンジン少なめだから、これ不要だと思う。

            //textBox4.Text = $"深さ：{info.Depth}/{info.SelDepth}";
            // 深さも各iterationごとにPVを出力しているわけで、こんなものは不要である。

            if (info.NodesString != null)
                textBox3.Text = $" ノード数 : { info.NodesString.PadLeftUnicode(14) }";

            if (info.NpsString != null)
                textBox4.Text = $" NPS : { info.NpsString.PadLeftUnicode(11) }";

            if (info.HashPercentageString != null)
                textBox5.Text = $" HASH : { info.HashPercentageString.PadLeftUnicode(6) }";
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

        /// <summary>
        /// 表示している読み筋(ListView.Items)に対応する指し手
        /// </summary>
        private List<List<Move>> list_item_moves = new List<List<Move>>();
    }
}
