using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;

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
            /// KifuListを表現する仮想プロパティ
            /// LocalGameServerの同名のプロパティとDataBindによって接続されていて、
            /// あちらで更新があると、これらのプロパティの更新通知が来るのでそれをハンドルする。
            /// </summary>
            public List<string> KifuList;
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
        }

        /// <summary>
        /// [UI Thread] : 表示している棋譜の行数
        /// </summary>
        public int KifuListCount
        {
            get { return listBox1.Items.Count; }
        }

        // -- 以下、棋譜ウインドウに対するオペレーション

        /// <summary>
        /// [UI Thread] : 棋譜ウィンドウ上、一手戻るボタン
        /// 局面が一手戻るとは限らない。
        /// </summary>
        public void RewindKifuListIndex()
        {
            ViewModel.KifuListSelectedIndex = listBox1.SelectedIndex - 1;
        }

        /// <summary>
        /// [UI Thread] : 棋譜ウィンドウ上、一手進むボタン
        /// 局面が一手進むとは限らない。
        /// </summary>
        public void ForwardKifuListIndex()
        {
            ViewModel.KifuListSelectedIndex = listBox1.SelectedIndex + 1;
        }

        /// <summary>
        /// [UI thread] : 内部状態が変わったのでボタンの有効、無効を更新するためのハンドラ。
        /// </summary>
        /// <param name="inTheGame"></param>
        public void UpdateButtonState(bool inTheGame)
        {
            // 最小化したのかな？
            if (Width == 0 || Height == 0 || listBox1.ClientSize.Width == 0)
                return;

            // 非表示だったものを表示したのであれば、これによって棋譜が隠れてしまう可能性があるので注意。
            var needScroll = !button1.Visible && !inTheGame;

            // ボタンの表示は対局外のときのみ
            button1.Visible = !inTheGame;
            button2.Visible = !inTheGame;
            button3.Visible = !inTheGame;

            // -- ボタンなどのリサイズ

            // 全体の8%の高さのボタンを用意。
            int bh = inTheGame ? 0 : Height * 8 / 100;
            int x = Width / 3;
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
        /// [UI thread] : 親ウインドウがリサイズされた時にそれに収まるようにこのコントロール内の文字の大きさを調整する。
        /// 
        /// inTheGame == trueのときはゲーム中なので「本譜」ボタンと「次分岐」ボタンを表示しない。
        /// </summary>
        /// <param name="scale"></param>
        public void OnResize(double scale, bool inTheGame)
        {
            // 最小化したのかな？
            if (Width == 0 || Height == 0 || listBox1.ClientSize.Width == 0)
                return;

            UpdateButtonState(inTheGame);

            // 画面を小さくしてもスクロールバーは小さくならないから計算通りのフォントサイズだとまずいのか…。
            var w_rate = 1.0f + TheApp.app.config.KifuWindowWidthType * 0.25f; // 横幅をどれだけ引き伸ばすのか
            var font_size = (float)(19.5 * scale * w_rate);
            var font_size2 = (float)(19.5 * scale); // button用のフォントサイズ
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
            listBox1.Font = font;

            // buttonのFontSizeあまり変更すると高さが足りなくなるので横幅の比率変更は反映させない。
            var font2 = new Font("MS Gothic", font_size2, FontStyle.Regular, GraphicsUnit.Pixel);
            button1.Font = font2;
            button2.Font = font2;
            button3.Font = font2;
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

            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
        }

        /// <summary>
        /// [UI thread] : リストが1行削除されたときに呼び出されるハンドラ
        /// </summary>
        private void KifuListRemoved(PropertyChangedEventArgs args)
        {
            if (listBox1.Items.Count == 0)
                return; // なんで？

            listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;

            listBox1.Items.Remove(listBox1.Items.Count - 1);
            listBox1.SelectedIndex = listBox1.Items.Count - 1; // last

            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
        }

        /// <summary>
        /// [UI thread] : リストが変更されたときに呼び出されるハンドラ
        /// </summary>
        private void KifuListChanged(PropertyChangedEventArgs args)
        {
            // ここでListBoxをいじって、listBox1_SelectedIndexChanged()が呼び出されるのは嫌だから抑制する。

            listBox1.SelectedIndexChanged -= listBox1_SelectedIndexChanged;

            var list = args.value as List<string>;
            if (list == null)
                return; // まだ初期化終わってない。

            int start = 0;
            /*
            int start;
            if (args.start == -1)
                start = 0; // 丸ごと更新された
            else
                start = args.start; // 部分更新された
            */

            // endの指定は無視される。

            var listbox = listBox1;
            listbox.BeginUpdate();

            int j = -1;

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
            listbox.SelectedIndex = j;

            listbox.EndUpdate();

            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            UpdateButtonState();
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

            // 押し戻された可能性があるので、"ViewModel.KifuListSelectedIndex"に書き戻しておく。
            ViewModel.KifuListSelectedIndex = listBox1.SelectedIndex = selectedIndex;
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
