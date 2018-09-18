using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Tool;

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
            public List<string> KifuList = new List<string>();
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
            if (Width == 0 || Height == 0 || listBox1.ClientSize.Width == 0)
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
            var font_button_enable = !inTheGame && ViewModel.DockState != DockState.InTheMainWindow;

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

            listBox1.Location = new Point(0, 0);
            listBox1.Size = new Size(Width, y);

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
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
        }

        /// <summary>
        /// ViewModel.InTheGameの値が変更になったときに呼び出されるハンドラ
        /// </summary>
        /// <param name="args"></param>
        private void InTheGameChanged(PropertyChangedEventArgs args)
        {
            UpdateButtonLocation();
            UpdateButtonState();
            UpdateButtonState2();
        }

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
            if (Width == 0 || Height == 0 || listBox1.ClientSize.Width == 0)
                return;

            // リサイズしたので連動してボタンの位置が変わる。
            UpdateButtonLocation();

            // 画面を小さくしてもスクロールバーは小さくならないから計算通りのフォントサイズだとまずいのか…。
            var w_rate = 1.0f + TheApp.app.Config.KifuWindowWidthType * 0.25f; // 横幅をどれだけ引き伸ばすのか
            var font_size = (float)(19.5 * scale * w_rate);
            var font_size2 = (float)(16 * scale); // button用のフォントサイズ
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

            var font = new Font("MS Gothic", font_size, FontStyle.Regular, GraphicsUnit.Pixel);
            FontUtility.SetFont( listBox1 , font);

            // buttonのFontSizeあまり変更すると高さが足りなくなるので横幅の比率変更は反映させない。
            var font2 = new Font("MS Gothic", font_size2, FontStyle.Regular, GraphicsUnit.Pixel);
            FontUtility.SetFont(button1 , font2);
            FontUtility.SetFont(button2 , font2);
            FontUtility.SetFont(button3 , font2);
            FontUtility.SetFont(button4 , font2);
            FontUtility.SetFont(button5 , font2);
            FontUtility.SetFont(button6 , font2);
        }

        private float last_font_size = 0;

        // -- handlers

        /// <summary>
        /// [UI thread] : リストが1行追加されたときに呼び出されるハンドラ
        /// </summary>
        private void KifuListAdded(PropertyChangedEventArgs args)
        {
            // 増えた1行がargs.valueに入っているはず。
            var line = args.value as string;

            listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;

            listBox1.Items.Add(line);
            listBox1.SelectedIndex = listBox1.Items.Count-1; // last
            ViewModel.SetKifuListSelectedIndex(listBox1.Items.Count - 1);
            ViewModel.KifuListCount = listBox1.Items.Count;
            ViewModel.KifuList.Add(line); // ここも同期させておく。
            
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;

            // item数が変化したはずなので、「消一手」ボタンを更新する。
            UpdateButtonState2();
        }

        /// <summary>
        /// [UI thread] : リストが1行削除されたときに呼び出されるハンドラ
        /// </summary>
        private void KifuListRemoved(PropertyChangedEventArgs args)
        {
            if (listBox1.Items.Count == 0)
                return; // なんで？

            listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;

            listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
            listBox1.SelectedIndex = listBox1.Items.Count - 1; // last
            ViewModel.SetKifuListSelectedIndex(listBox1.Items.Count - 1);
            ViewModel.KifuListCount = listBox1.Items.Count;
            ViewModel.KifuList.RemoveAt(ViewModel.KifuList.Count - 1); // ここも同期させておく。
            
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;

            // 1手戻った結果、「次分岐」があるかも知れないのでそのボタン状態を更新する。
            UpdateButtonState();

            // item数が変化したはずなので、「消一手」ボタンを更新する。
            UpdateButtonState2();
        }

        /// <summary>
        /// [UI thread] : リストが(丸ごと)変更されたときに呼び出されるハンドラ
        /// </summary>
        private void KifuListChanged(PropertyChangedEventArgs args)
        {
            // ここでListBoxをいじって、listBox1_SelectedIndexChanged()が呼び出されるのは嫌だから抑制する。

            listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;

            var list = args.value as List<string>;

            var listbox = listBox1;
            listbox.BeginUpdate();

            var selectedIndex = listbox.SelectedIndex;

#if false
            int j = -1;
            int start = 0;

            // 値の違う場所のみ書き換える
            // 値の違うところを探す
            // start以降、endまでしか更新されていないことは保証されているものとする。

            // デバッグ用に、startまで要素が足りていなければとりあえず埋めておく。
            for (int i = listbox.Items.Count; i < start; ++i)
                listbox.Items.Add(list[i]);

            // たいていは1行追加されるだけなので、AddRange()を使うより最小差分更新のほうが速いはず。
            for (int i = start; i < list.Count ; ++i)
            {
                if (listbox.Items.Count <= i || list[i] != listbox.Items[i].ToString())
                {
                    // ここ以降を書き換える。
                    while (listbox.Items.Count > i)
                        listbox.Items.RemoveAt(listbox.Items.Count - 1); // RemoveLast

                    j = i; // あとでここにフォーカスを置く
                    for(; i < list.Count; ++i)
                        listbox.Items.Add(list[i]);

                    break;
                }
            }

            // ここまで完全一致なので、末尾にフォーカスがあって良い。
            if (j == -1)
                j = list.Count - 1;

            // そのあとの要素が多すぎるなら削除する。(ユーザーが待ったした時などにそうなる)
            while (listbox.Items.Count > list.Count)
                listbox.Items.RemoveAt(listbox.Items.Count - 1); // RemoveLast

            // カーソルを異なる項目が最初に見つかったところに置いておく。
            // 「本譜」に戻る、「次分岐」などではこの処理がなされているべき。
            // そうしないと棋譜を書き換えた時点で自動的に末尾までスクロールしてしまう。
            listbox.SelectedIndex = j;
            ViewModel.SetKifuListSelectedIndex(j);

            listbox.EndUpdate();

            ViewModel.KifuListCount = listBox1.Items.Count;

            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            // ここでカーソル行の変更通知イベントが発生する？念の為再代入しておく。
            listbox.SelectedIndex = j;
#endif

            // 差分更新だけして、List.Items.Add()とRemove()ではDCが解放されず、リソースリークになるっぽい。
            // これはWindowsのListBoxの出来が悪いからだと思うが…。
            // これだと連続対局においてDCが枯渇してしまう。

#if true // 何も考えずに丸ごと書き換えるコード
            listbox.Items.Clear();
            listbox.Items.AddRange(list.ToArray());
            ViewModel.KifuListCount = listBox1.Items.Count;
            ViewModel.KifuList = list;
            listbox.EndUpdate();
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;

            // 元の選択行、もしくは末尾選択
            // 非選択は良くないので、-1なら0に補正するコードも追加。
            selectedIndex = Math.Max(selectedIndex, 0);
            selectedIndex = Math.Min(selectedIndex, listbox.Items.Count - 1);

            listbox.SelectedIndex = selectedIndex;

            AdjustScrollTop();
#endif

            UpdateButtonState();

            // item数が変化したはずなので、「消一手」ボタンを更新する。
            UpdateButtonState2();
        }

        /// <summary>
        /// [UI thread]
        /// 棋譜の途中の指し手において、選択行が表示範囲の3行ほど手前になるように調整する。
        /// (横スクロール型のアクションゲームとかでよくあるやつ。)
        /// </summary>
        private void AdjustScrollTop()
        {
            var selected = ViewModel.KifuListSelectedIndex;
            var top = listBox1.TopIndex;

            var visibleCount = listBox1.ClientSize.Height / listBox1.ItemHeight;
            var bottom = top + visibleCount; // これListBoxのpropertyにないのだ…。何故なのだ…。

            // スクロール時にこの行数分だけ常に余裕があるように見せる。
            // 縦幅を狭めているときは、marginを縮める必要がある。
            var margin = Math.Min(3 /* デフォルトで3行 */ , (visibleCount - 1) / 2);
            if (top + margin > selected)
                top = selected - margin;
            else if (selected + margin + 1 >= bottom)
                top = selected - (visibleCount - margin - 1);

            listBox1.TopIndex = top;
        }


        /// <summary>
        /// [UI thread] : 棋譜の読み込み時など、LocalServer側の要請により、棋譜ウィンドウを指定行に
        /// フォーカスを当てるためのハンドラ
        /// </summary>
        private void setKifuListIndex(PropertyChangedEventArgs args)
        {
            // 選べる要素が存在しない。
            if (listBox1.Items.Count == 0)
                return;

            var selectedIndex = (int)args.value;

            // 範囲外なら押し戻す。
            if (selectedIndex < 0)
                selectedIndex = 0;
            else if (listBox1.Items.Count <= selectedIndex)
                selectedIndex = listBox1.Items.Count - 1;

            // 押し戻された可能性があるので、"ViewModel.KifuListSelectedIndex"に書き戻しておく。値が同じであれば変更イベントは発生しない。
            ViewModel.KifuListSelectedIndex = listBox1.SelectedIndex = selectedIndex;

            AdjustScrollTop();
        }

        /// <summary>
        /// 選択行が変更されたので、ViewModelにコマンドを送信してみる。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewModel.SetValueAndRaisePropertyChanged("KifuListSelectedIndex", listBox1.SelectedIndex);
            UpdateButtonState();
        }

        /// <summary>
        /// 分岐棋譜の時だけ、「消分岐」「次分岐」ボタンを有効にする。
        /// </summary>
        private void UpdateButtonState()
        {
            var s = listBox1.SelectedItem;
            if (s!=null)
            {
                var item = (string)s;

                // 本譜ボタン
                var e = item.StartsWith(">");
                button1.Enabled = e;

                if (e)
                    item = item.Substring(1,1); // 1文字目をskipして2文字目を取得 

                var e2 = item.StartsWith("+") || item.StartsWith("*");
                button2.Enabled = e2;
                button3.Enabled = e2;
            }
        }

        /// <summary>
        /// 「消一手」ボタンの有効/無効を切り替える。
        /// </summary>
        private void UpdateButtonState2()
        {
            // Items[0] == "開始局面"なので、そこ以降に指し手があればundo出来るのではないかと。(special moveであろうと)
            button4.Enabled = listBox1.Items.Count > 1;
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
            if (ViewModel.DockState != DockState.InTheMainWindow)
            {
                var fontsize = TheApp.app.Config.KifuWindowFontSize;
                if (fontsize == 0)
                    fontsize = 11.25f;
                else if (fontsize < MIN_FONT_SIZE)
                    fontsize = 5f;
                else if (fontsize > MAX_FONT_SIZE)
                    fontsize = 30f;
                TheApp.app.Config.KifuWindowFontSize = fontsize; // writeback

                var font = new Font("MS Gothic", fontsize , FontStyle.Regular, GraphicsUnit.Point);
                FontUtility.SetFont(listBox1, font);

                var font2 = new Font("MS Gothic", 11.25F, FontStyle.Regular, GraphicsUnit.Point);

                FontUtility.SetFont(button1, font2);
                FontUtility.SetFont(button2, font2);
                FontUtility.SetFont(button3, font2);
                FontUtility.SetFont(button4, font2);
                FontUtility.SetFont(button5, font2);
                FontUtility.SetFont(button6, font2);

                UpdateButtonLocation();
            }
        }

        /// <summary>
        /// ListBoxの文字フォントサイズの最大、最小。
        /// </summary>
        private const float MAX_FONT_SIZE = 30f;
        private const float MIN_FONT_SIZE = 5f;

        /// <summary>
        /// 「+」「-」ボタンのEnableを更新する。
        /// </summary>
        private void UpdateButtonEnable()
        {
            button5.Enabled = TheApp.app.Config.KifuWindowFontSize < MAX_FONT_SIZE;
            button6.Enabled = TheApp.app.Config.KifuWindowFontSize > MIN_FONT_SIZE;
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
            if (TheApp.app.Config.KifuWindowFontSize < MAX_FONT_SIZE)
            {
                TheApp.app.Config.KifuWindowFontSize++;
                KifuControl_SizeChanged(sender, e);
                UpdateButtonEnable();
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
            if (TheApp.app.Config.KifuWindowFontSize > MIN_FONT_SIZE)
            {
                TheApp.app.Config.KifuWindowFontSize--;
                KifuControl_SizeChanged(sender, e);
                UpdateButtonEnable();
            }
        }


#if false


        private void InitOwnerDraw()
        {
            // 文字色を変えたいのでowner drawにする。
            // →　ちらつくし、また、遅かった…これはやめよう。
            //listBox1.DrawMode = DrawMode.OwnerDrawFixed;

            // 棋譜ウィンドウがちらつくの嫌なのでダブルバッファリングにする。
            // →　うまくいかなかった。なんで？
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        // 棋譜ウィンドウの各行の色をカスタムに変更したいので、描画ハンドラを自前で書く。
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index > -1)
            {
                Brush wBrush = null;
                try
                {
                    if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
                    {
                        /*                        
                                                //選択されていない行
                                                if (e.Index < 2)
                                                {
                                                    //指定された行より小さければ青
                                                    wBrush = new SolidBrush(Color.Blue);
                                                }
                                                else
                                                {
                                                    //指定された行より大きければ通常色
                                                    wBrush = new SolidBrush(e.ForeColor);
                                                }
                        */
                        // あとで考える。

                        wBrush = new SolidBrush(e.ForeColor);
                    }
                    else
                    {
                        //選択されている行なら通常色
                        wBrush = new SolidBrush(e.ForeColor);
                    }
                    //文字を設定
                    e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, wBrush, e.Bounds);
                }
                finally
                {
                    if (wBrush != null)
                    {
                        wBrush.Dispose();
                    }
                }
            }
            e.DrawFocusRectangle();
        }
#endif

    }
}
