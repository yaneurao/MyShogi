using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.View.Win2D
{
    public partial class KifuControl : UserControl
    {
        public KifuControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// リストが変更されたときに呼び出されるハンドラ
        /// </summary>
        public void OnListChanged(PropertyChangedEventArgs args)
        {
            Invoke(new Action(() =>
            {
                List<string> list = args.value as List<string>;

                int start;
                if (args.start == -1)
                    start = 0; // 丸ごと更新された
                else
                    start = args.start; // 部分更新された

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

                for (int i = start; i < list.Count ; ++i)
                {
                    if (listbox.Items.Count <= i || list[i] != listbox.Items[i].ToString())
                    {
                        // ここ以降を書き換える。
                        while (listbox.Items.Count > i)
                            listbox.Items.RemoveAt(listbox.Items.Count -1); // RemoveLast

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
            }));
        }

        // -- 以下、棋譜ウインドウに対するオペレーション

        /// <summary>
        /// 親ウインドウがリサイズされた時にそれに収まるようにこのコントロール内の文字の大きさを
        /// 調整する。
        /// </summary>
        /// <param name="scale"></param>
        public void OnResize(double scale)
        {
            // 最小化したのかな？
            if (this.Width == 0 || listBox1.ClientSize.Width == 0)
                return;

            // 画面を小さくしてもスクロールバーは小さくならないから計算通りのフォントサイズだとまずいのか…。
            var font_size = (float)(22 * scale);

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
            font_size *= ((float)Width - 34 /* scroll bar */) / Width;

            // 前回のフォントサイズと異なるときだけ再設定する
            if (last_font_size == font_size)
                return;

            last_font_size = font_size;
            last_scale = scale;

            listBox1.Font = new Font("MS Gothic", font_size, FontStyle.Regular , GraphicsUnit.Pixel);

            // font変更の結果、選択しているところがlistboxの表示範囲外になってしまうことがある。
            // これ、あとで修正を考える。
        }

        private double last_scale = 0;
        private float last_font_size = 0;
    }
}
