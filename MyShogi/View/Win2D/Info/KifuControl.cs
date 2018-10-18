using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Tool;
using MyShogi.Model.Shogi.Kifu;

namespace MyShogi.View.Win2D
{
    public partial class KifuControl : UserControl
    {
        /// <summary>
        /// 棋譜表示用のコントロール
        ///
        /// InitViewModel(Form)を初期化のために必ず呼び出すこと。
        /// </summary>
        public KifuControl()
        {
            InitializeComponent();

            InitListView();

            var fm = new FontSetter(this, "KifuWindow");
            Disposed += (sender,args) => { fm.Dispose(); }; 
        }

        // -- properties

        public class KifuControlViewModel : NotifyObject
        {
            /// <summary>
            /// 棋譜リスト上の現在選択されている行
            /// 
            /// 双方向databindによって、LocalGameServerの同名のプロパティと紐付けられている。
            /// </summary>
            public int KifuListSelectedIndex
            {
                get { return GetValue<int>("KifuListSelectedIndex"); }
                set { SetValue<int>("KifuListSelectedIndex",value); }
            }

            /// <summary>
            /// KifuListSelectedIndexの値を変更して、イベントを発生させない。
            /// </summary>
            /// <param name="i"></param>
            public void SetKifuListSelectedIndex(int i)
            {
                SetValueAndNotRaise("KifuListSelectedIndex", i);
            }

            /// <summary>
            /// 棋譜リストの項目の数。KifuListSelectedIndexをこれより進めるべきではない。
            /// </summary>
            public int KifuListCount;

            /// <summary>
            /// KifuListを表現する仮想プロパティ
            /// LocalGameServerの同名のプロパティとDataBindによって接続されていて、
            /// あちらで更新があると、これらのプロパティの更新通知が来るのでそれをハンドルする。
            /// </summary>
            public List<KifuListRow> KifuList = new List<KifuListRow>();
            public string KifuListAdded;
            public object KifuListRemoved;

            /// <summary>
            /// 本譜ボタンがクリックされた。
            /// </summary>
            public object MainBranchButtonClicked;

            /// <summary>
            /// 次分岐ボタンがクリックされた。
            /// </summary>
            public object NextBranchButtonClicked;

            /// <summary>
            /// 分岐削除ボタンがクリックされた。
            /// </summary>
            public object EraseBranchButtonClicked;

            /// <summary>
            /// 最後の指し手を削除する。
            /// </summary>
            public object RemoveLastMoveClicked;

            /// <summary>
            /// フローティングモードなのかなどを表す。
            /// </summary>
            public DockState DockState
            {
                get { return GetValue<DockState>("DockState"); }
                set { SetValue<DockState>("DockState", value); }
            }

            /// <summary>
            /// LocalGameServer.InTheGameModeが変更されたときに呼び出されるハンドラ
            /// 「本譜」ボタン、「次分岐」ボタンなどを非表示/表示を切り替える。
            /// </summary>
            public bool InTheGame
            {
                get { return GetValue<bool>("InTheGame"); }
                set { SetValue<bool>("InTheGame", value); }
            }
        }

        public KifuControlViewModel ViewModel = new KifuControlViewModel();

        /// <summary>
        /// 外部から初期化して欲しい。
        /// </summary>
        /// <param name="parent"></param>
        public void InitViewModel(Form parent)
        {
            ViewModel.AddPropertyChangedHandler("KifuListSelectedIndex", setKifuListIndex, parent);
            ViewModel.AddPropertyChangedHandler("KifuList", KifuListChanged, parent);
            ViewModel.AddPropertyChangedHandler("KifuListAdded", KifuListAdded, parent);
            ViewModel.AddPropertyChangedHandler("KifuListRemoved", KifuListRemoved, parent);
            ViewModel.AddPropertyChangedHandler("InTheGame", InTheGameChanged , parent);
        }

        ///// <summary>
        ///// [UI Thread] : 表示している棋譜の行数
        ///// </summary>
        //public int KifuListCount
        //{
        //    get { return listBox1.Items.Count; }
        //}

        // -- 以下、棋譜ウインドウに対するオペレーション

        /// <summary>
        /// [UI Thread] : 棋譜ウィンドウ上、一手戻るボタン
        /// 局面が一手戻るとは限らない。
        /// </summary>
        public void RewindKifuListIndex()
        {
            ViewModel.KifuListSelectedIndex = Math.Max( 0 , ViewModel.KifuListSelectedIndex - 1);
        }

        /// <summary>
        /// [UI Thread] : 棋譜ウィンドウ上、一手進むボタン
        /// 局面が一手進むとは限らない。
        /// </summary>
        public void ForwardKifuListIndex()
        {
            ViewModel.KifuListSelectedIndex = Math.Min(ViewModel.KifuListCount - 1 , ViewModel.KifuListSelectedIndex + 1);
        }

        /// <summary>
        /// [UI thread] : 内部状態が変わったのでボタンの有効、無効を更新するためのハンドラ。
        /// 
        /// ViewModel.InTheGameが変更になった時に呼び出される。
        /// </summary>
        /// <param name="inTheGame"></param>
        private void UpdateButtonLocation()
        {
            // 最小化したのかな？
            if (Width == 0 || Height == 0 || listView1.ClientSize.Width == 0)
                return;

            var inTheGame = ViewModel.InTheGame;

            // 非表示だったものを表示したのであれば、これによって棋譜が隠れてしまう可能性があるので注意。
            var needScroll = !button1.Visible && !inTheGame;

            // ボタンの表示は対局外のときのみ
            button1.Visible = !inTheGame;
            button2.Visible = !inTheGame;
            button3.Visible = !inTheGame;
            button4.Visible = !inTheGame;

            // フォントサイズ変更ボタンが有効か
            // 「+」「-」ボタンは、メインウインドウに埋め込んでいないときのみ
            // → やめよう　メインウインドウ埋め込み時も有効にしよう。
            var font_button_enable = !inTheGame; // && ViewModel.DockState != DockState.InTheMainWindow;

            button5.Visible = font_button_enable;
            button6.Visible = font_button_enable;

            // -- ボタンなどのリサイズ

            // ボタン高さ

            // メインウインドウに埋め込んでいるなら 全体の8%
            // フローティングモードなら23固定。
            // 対局中は非表示。
            int bh = inTheGame ? 0 :
                (ViewModel.DockState == DockState.InTheMainWindow) ? Height * 8 / 100 :
                23;

            int x = font_button_enable ? Width / 5 : Width / 4;
            int y = Height - bh;

            listView1.Location = new Point(0, 0);
            listView1.Size = new Size(Width, y);

            if (!inTheGame)
            {
                button1.Location = new Point(x * 0, y);
                button1.Size = new Size(x, bh);
                button2.Location = new Point(x * 1, y);
                button2.Size = new Size(x, bh);
                button3.Location = new Point(x * 2, y);
                button3.Size = new Size(x, bh);
                button4.Location = new Point(x * 3, y);
                button4.Size = new Size(x, bh);

                button5.Location = new Point(x * 4, y);
                button5.Size = new Size(x/2, bh);
                button6.Location = new Point((int)(x * 4.5), y);
                button6.Size = new Size(x/2, bh);
            }

            if (needScroll)
            {
                // 選択行が隠れていないことを確認しなければ..。
                // SelectedIndexを変更すると、SelectedIndexChangedイベントが発生してしまうので良くない。
                // 現在は、対局が終了した瞬間であるから、棋譜の末尾に移動して良い。
                SetListViewSelectedIndex(listView1.Items.Count - 1);
            }
        }

        /// <summary>
        /// ViewModel.InTheGameの値が変更になったときに呼び出されるハンドラ
        /// </summary>
        /// <param name="args"></param>
        private void InTheGameChanged(PropertyChangedEventArgs args)
        {
            using (var s = new SuspendLayoutBlock(this))
            {
                UpdateButtonLocation();
                UpdateButtonState();
            }
        }

#if false
        /// <summary>
        /// [UI thread] : 親ウインドウがリサイズされた時にそれに収まるようにこのコントロール内の文字の大きさを調整する。
        /// (MainWindowに埋め込んでいるときに、リサイズに対して呼び出される)
        /// 
        /// inTheGame == trueのときはゲーム中なので「本譜」ボタンと「次分岐」ボタンを表示しない。
        /// </summary>
        /// <param name="scale"></param>
        public void OnResize(double scale)
        {
            // MainWindowに埋め込んでいないなら呼び出してはならない。
            Debug.Assert(ViewModel.DockState == DockState.InTheMainWindow);

            // 最小化したのかな？
            if (Width == 0 || Height == 0 || listView1.ClientSize.Width == 0)
                return;

            using (var s = new SuspendLayoutBlock(this))
            {

                // リサイズしたので連動してボタンの位置が変わる。
                UpdateButtonLocation();

                // 画面を小さくしてもスクロールバーは小さくならないから計算通りのフォントサイズだとまずいのか…。
                var w_rate = 1.0f + TheApp.app.Config.KifuWindowWidthType * 0.25f; // 横幅をどれだけ引き伸ばすのか
                var font_size = (float)(19.5 * scale * w_rate);
                var font_size2 = (float)(16 * scale);
                // button用のフォントサイズ

                /*
                    // ClientSizeはスクロールバーを除いた幅なので、controlのwidthとの比の分だけ
                    // fontを小さく表示してやる。
                    font_size *= (float)listBox1.ClientSize.Width / this.Width;

                    Console.WriteLine(listBox1.ClientSize.Width + "/" + this.Width);
                */

                // スクロールバーが出たために文字の横幅が収まらない可能性を考慮してItems.Add()ごとに
                // OnResize()を都度呼び出してやりたい…が、スクロールバーが出た結果、文字を縮小して、
                // その結果、スクロールバーが消えるという現象が…。

                // →　結論的には、スクロールバーの有無によって文字フォントサイズを変更するのは
                // 筋が良くないということに。最初からスクロールバーの分だけ控えて描画すべき。

                // ところがスクロールバーの横幅不明。実測34だったが、環境によって異なる可能性が..
                // AutoScaleの値とか反映されてると困るのだが…。
                font_size *= ((float)Width - 34 /* scroll bar */) / Width;
                font_size2 *= ((float)Width - 34 /* scroll bar */) / Width;

                // 幅を縮めるとfont_sizeが負になるが、負のサイズのFontは生成できないので1にしておく。
                if (font_size <= 0)
                    font_size = 1;
                if (font_size2 <= 0)
                    font_size2 = 1;

                // 前回のフォントサイズと異なるときだけ再設定する
                //if (last_font_size == font_size)
                //    return;

                last_font_size = font_size;

                var config = TheApp.app.Config;
                var fontname = config.FontManager.KifuWindow.FontName;
                var fontstyle = config.FontManager.KifuWindow.FontStyle;
                var font = new Font(fontname, font_size, fontstyle, GraphicsUnit.Pixel);
                listView1.Font = font;

                // buttonのFontSizeあまり変更すると高さが足りなくなるので横幅の比率変更は反映させない。
                var font2 = new Font(fontname, font_size2, fontstyle, GraphicsUnit.Pixel);
                var buttons = new[] { button1, button2, button3, button4, button5, button6 };
                foreach (var b in buttons)
                    b.Font = font2;
            }
        }
        private float last_font_size = 0;
#endif

        // -- initialize design

        /// <summary>
        /// LisTviewのheader、列の幅などの初期化。
        /// </summary>
        private void InitListView()
        {
            listView1.FullRowSelect = true;
            //listView1.GridLines = true;
            listView1.Sorting = SortOrder.None;
            listView1.View = System.Windows.Forms.View.Details;

            var ply_string = new ColumnHeader();
            ply_string.Text = "手数";
            ply_string.Width = 50;
            ply_string.TextAlign = HorizontalAlignment.Center;

            var move_string = new ColumnHeader();
            move_string.Text = "指し手";
            move_string.Width = 100;
            move_string.TextAlign = HorizontalAlignment.Left;

            var time_string = new ColumnHeader();
            time_string.Text = "時間";
            time_string.Width = 60;
            time_string.TextAlign = HorizontalAlignment.Right;

            var header = new[] { ply_string, move_string, time_string };
            
            listView1.Columns.AddRange(header);

            // TODO : あとで

            //listView1.AutoResizeColumns( ColumnHeaderAutoResizeStyle.ColumnContent);
            // headerとcontentの文字長さを考慮して、横幅が自動調整されて水平スクロールバーで移動してくれるといいのだが、うまくいかない。よくわからない。
#if false
            foreach (var index in All.Int(5))
            {
                int w1 = listView1.Columns[index].Width;
                int w2 = TheApp.app.Config.ConsiderationColumnWidth[index];
                listView1.Columns[index].Width = w2 == 0 ? w1 : w2; // w2が初期化直後の値なら、採用しない。
                                                                    // これだと幅を0にすると保存されなくなってしまうのか…。そうか…。保存するときに1にしておくべきなのか…。
            }
#endif
        }

        // -- handlers

        /// <summary>
        /// [UI thread] : リストが1行追加されたときに呼び出されるハンドラ
        /// </summary>
        private void KifuListAdded(PropertyChangedEventArgs args)
        {
            using (var slb = new SuspendLayoutBlock(this))
            {
                // 増えた1行がargs.valueに入っているはず。
                var row = args.value as KifuListRow;

                listView1.SelectedIndexChanged -= listView1_SelectedIndexChanged;

                listView1.Items.Add(KifuListRowToListItem(row));

                ViewModel.KifuListCount = listView1.Items.Count;
                ViewModel.KifuList.Add(row); // ここも同期させておく。

                var lastIndex = listView1.Items.Count - 1;
                ViewModel.SetKifuListSelectedIndex(lastIndex);

                // 末尾の項目を選択しておく。
                SetListViewSelectedIndex(lastIndex);

                listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;

                // item数が変化したはずなので、「消一手」ボタンなどを更新する。
                UpdateButtonState();
            }
        }

        /// <summary>
        /// KifuListRowを、棋譜ウインドウのListViewで表示するListViewItemの形式に変換する。
        /// </summary>
        /// <param name="row"></param>
        private ListViewItem KifuListRowToListItem(KifuListRow row)
        {
            var list = new[] { row.PlyString, row.MoveString, row.ConsumeTime };
            return new ListViewItem( list );
        }

        /// <summary>
        /// [UI thread] : リストが1行削除されたときに呼び出されるハンドラ
        /// </summary>
        private void KifuListRemoved(PropertyChangedEventArgs args)
        {
            if (listView1.Items.Count == 0)
                return; // なんで？

            listView1.SelectedIndexChanged -= listView1_SelectedIndexChanged;

            listView1.Items.RemoveAt(listView1.Items.Count - 1);

            //listView1.SelectedIndex = listView1.Items.Count - 1; // last
            listView1.EnsureVisible(listView1.Items.Count - 1);
//            ViewModel.SetKifuListSelectedIndex(listBox1.Items.Count - 1);

            // → 「待った」によるUndoと「消一手」による末尾の指し手の削除でしかこのメソッドが
            // 呼び出されないので、ここで選択行が変更になったイベントを生起させるべき。
            // 対局中の「待った」に対しては、そのハンドラでは、対局中は何も処理をしないし、
            // 検討中ならば、残り時間の巻き戻しとNotifyTurnChanged()を行うのでこの書き方で問題ない。
            ViewModel.KifuListSelectedIndex = listView1.Items.Count - 1;

            ViewModel.KifuListCount = listView1.Items.Count;
            ViewModel.KifuList.RemoveAt(ViewModel.KifuList.Count - 1); // ここも同期させておく。

            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;

            // 1手戻った結果、「次分岐」があるかも知れないのでそのボタン状態を更新する。
            UpdateButtonState();
        }

        /// <summary>
        /// [UI thread] : リストが(丸ごと)変更されたときに呼び出されるハンドラ
        /// 　　UI上で駒を動かした時は、分岐が発生する場合があるので、毎回丸ごと渡ってくる。(ちょっと無駄な気も..)
        /// </summary>
        private void KifuListChanged(PropertyChangedEventArgs args)
        {
            // ここでListBoxをいじって、listBox1_SelectedIndexChanged()が呼び出されるのは嫌だから抑制する。

            listView1.SelectedIndexChanged -= listView1_SelectedIndexChanged;

            // 現在の選択行を復元する。
            //var selected = GetListViewSelectedIndex();

            var list = args.value as List<KifuListRow>;
            listView1.BeginUpdate();

            // 差分更新だけして、List.Items.Add()とRemove()ではDCが解放されず、リソースリークになるっぽい。
            // これはWindowsのListBoxの出来が悪いからだと思うが…。
            // これだと連続対局においてDCが枯渇してしまう。

            // 何も考えずに丸ごと書き換えるコード
            listView1.Items.Clear();
            // AddRange()で書けない。ごめん。
            foreach(var e in list)
                listView1.Items.Add(KifuListRowToListItem(e));

            ViewModel.KifuListCount = listView1.Items.Count;
            ViewModel.KifuList = list;

            listView1.EndUpdate();
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;

            // 現在の選択行を復元する。
            //if (0 <= selected && selected < listView1.Items.Count)
            //    SetListViewSelectedIndex(selected);

            // →　勝手に選択行を作るのはよろしくない。
            // 項目数が1個(開始局面)のみのときだけ、そこを選択するようにする。
            if (listView1.Items.Count == 1)
                SetListViewSelectedIndex(0);

            // 再度、Selectedのイベントが来るはずなのでその時にボタン状態を更新する。
            //UpdateButtonState();
        }

        /// <summary>
        /// [UI thread]
        /// 棋譜の途中の指し手において、選択行が表示範囲の3行ほど手前になるように調整する。
        /// (横スクロール型のアクションゲームとかでよくあるやつ。)
        /// </summary>
        private void AdjustScrollTop()
        {
            // TODO あとで
#if false
            var selected = ViewModel.KifuListSelectedIndex;
            var top = listView1.TopItem.Index;

            var visibleCount = listView1.ClientSize.Height / listView1.ItemHeight;
            var bottom = top + visibleCount; // これListBoxのpropertyにないのだ…。何故なのだ…。

            // スクロール時にこの行数分だけ常に余裕があるように見せる。
            // 縦幅を狭めているときは、marginを縮める必要がある。
            var margin = Math.Min(3 /* デフォルトで3行 */ , (visibleCount - 1) / 2);
            if (top + margin > selected)
                top = selected - margin;
            else if (selected + margin + 1 >= bottom)
                top = selected - (visibleCount - margin - 1);

            listView1.TopItem.Index = top;
#endif
    }


        /// <summary>
        /// [UI thread] : 棋譜の読み込み時など、LocalServer側の要請により、棋譜ウィンドウを指定行に
        /// フォーカスを当てるためのハンドラ
        /// </summary>
        private void setKifuListIndex(PropertyChangedEventArgs args)
        {
            // 選べる要素が存在しない。
            if (listView1.Items.Count == 0)
                return;

            var selectedIndex = (int)args.value;

            // 範囲外なら押し戻す。
            if (selectedIndex < 0)
                selectedIndex = 0;
            else if (listView1.Items.Count <= selectedIndex)
                selectedIndex = listView1.Items.Count - 1;

            // 押し戻された可能性があるので、"ViewModel.KifuListSelectedIndex"に書き戻しておく。値が同じであれば変更イベントは発生しない。
            ViewModel.KifuListSelectedIndex = selectedIndex;
            SetListViewSelectedIndex(selectedIndex);

            AdjustScrollTop();
        }

        /// <summary>
        /// ListBoxのSelectedIndexのgetter相当のメソッド
        /// </summary>
        /// <returns></returns>
        private int GetListViewSelectedIndex()
        {
            var items = listView1.SelectedItems;
            if (items.Count == 0)
                return -1;

            var index = items[0].Index;
            return index;
        }

        /// <summary>
        /// ListBoxのSelectedIndexのsetter相当のメソッド
        /// </summary>
        /// <param name="index"></param>
        private void SetListViewSelectedIndex(int index)
        {
            listView1.SelectedIndexChanged -= listView1_SelectedIndexChanged;
            if (0 <= index && index < listView1.Items.Count)
            {
                listView1.EnsureVisible(index);

                // ListBox相当のSelectedIndexをemulationする。

                listView1.Items[index].Selected = true;
            }

            UpdateButtonState();

            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
        }

        /// <summary>
        /// 選択行が変更されたので、ViewModelにコマンドを送信してみる。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

#if false
            var items = listView1.SelectedItems;
            Console.WriteLine(items.Count);
            foreach (ListViewItem c in items)
                Console.WriteLine(">" + c.Index);
#endif
            // UIにより選択が移り変わるときに一度非選択の状態になり、このときindex == -1なので 0番目のselectをしてしまう。
            // index == -1ならこれを無視してイベントを生起しない。
            var index = GetListViewSelectedIndex();
            if (index == -1)
                return;

            ViewModel.SetValueAndRaisePropertyChanged("KifuListSelectedIndex", index);
            UpdateButtonState();
        }

        /// <summary>
        /// 分岐棋譜の時だけ、「消分岐」「次分岐」ボタンを有効にする。
        /// </summary>
        private void UpdateButtonState()
        {
            using (var slb = new SuspendLayoutBlock(this))
            {
                var index = GetListViewSelectedIndex();
                var s = index < 0 ? null : listView1.Items[index];
                if (s != null)
                {
                    var item = (s as ListViewItem).SubItems[0].Text;

                    // 本譜ボタン
                    var e = item.StartsWith(">");
                    button1.Enabled = e;

                    if (e)
                        item = item.Substring(1, 1); // 1文字目をskipして2文字目を取得 

                    var e2 = item.StartsWith("+") || item.StartsWith("*");
                    button2.Enabled = e2;
                    button3.Enabled = e2;
                }
                // Items[0] == "開始局面"なので、そこ以降に指し手があればundo出来るのではないかと。(special moveであろうと)
                button4.Enabled = listView1.Items.Count > 1;
            }
        }

        /// <summary>
        /// 本譜ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ViewModel.RaisePropertyChanged("MainBranchButtonClicked");
        }

        /// <summary>
        /// 次分岐ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            ViewModel.RaisePropertyChanged("NextBranchButtonClicked");
        }

        /// <summary>
        /// 分岐消去ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            ViewModel.RaisePropertyChanged("EraseBranchButtonClicked");
        }

        /// <summary>
        /// 消す1手ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            ViewModel.RaisePropertyChanged("RemoveLastMoveClicked");
        }

        /// <summary>
        /// リサイズ。
        /// 非フローティングモードなら、このメッセージは無視する。
        /// フローティングモードなら自律的にサイズを変更。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KifuControl_SizeChanged(object sender, EventArgs e)
        {
            if (TheApp.app.DesignMode)
                return;

            // メインウインドウに埋め込み時もフォント固定する。
            //if (ViewModel.DockState != DockState.InTheMainWindow)
            {
                using (var s = new SuspendLayoutBlock(this))
                {
                    // このフォント、ここで設定しなければ親に従うのでは…。
                    /*
                    var f = TheApp.app.Config.FontManager.KifuWindow;

                    var font = f.CreateFont();
                    listView1.Font = font;

                    var buttons = new[] { button1, button2, button3, button4, button5, button6 };
                    foreach (var button in buttons)
                        button.Font = font;
                    */

                    UpdateButtonLocation();
                }
            }
        }

        /// <summary>
        /// 「+」「-」ボタンのEnableを更新する。
        /// </summary>
        private void UpdateButtonEnable()
        {
            var fontsize = TheApp.app.Config.FontManager.KifuWindow.FontSize;
            button5.Enabled = fontsize < FontManager.MAX_FONT_SIZE;
            button6.Enabled = fontsize > FontManager.MIN_FONT_SIZE;
        }

        /// <summary>
        /// 文字を大きくする「+」ボタン
        ///
        /// ウインドウ時のみ有効。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            var fm = TheApp.app.Config.FontManager;
            if (fm.KifuWindow.FontSize < FontManager.MAX_FONT_SIZE)
            {
                fm.KifuWindow.FontSize++;
                fm.RaisePropertyChanged("FontChanged", "KifuWindow");
            }
        }

        /// <summary>
        /// 文字を小さくする「-」ボタン
        /// 
        /// ウインドウ時のみ有効。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            var fm = TheApp.app.Config.FontManager;
            if (fm.KifuWindow.FontSize > FontManager.MIN_FONT_SIZE)
            {
                fm.KifuWindow.FontSize--;
                fm.RaisePropertyChanged("FontChanged", "KifuWindow");
            }
        }

    }
}
