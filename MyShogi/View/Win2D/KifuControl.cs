using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class KifuControl : UserControl
    {
        public KifuControl()
        {
            InitializeComponent();

            // とりま初期メッセージを入れておく。
            // これはあとで修正する。
            listBox1.Items.Add("   === 開始局面 ===");
            listBox1.SelectedIndex = 0;
        }

        // -- 以下、棋譜ウインドウに対するオペレーション

        /// <summary>
        /// 棋譜ウインドウに指し手文字列を追加する。(末尾に)
        /// </summary>
        /// <param name="text"></param>
        public void AddMoveText(string text)
        {
            listBox1.Items.Add(text);

            // カーソルを末尾に移動させておく。
            listBox1.SelectedIndex = listBox1.Items.Count-1;
        }

        /// <summary>
        /// 棋譜文字列を追加する
        /// </summary>
        /// <param name="gamePly"></param>
        /// <param name="move"></param>
        /// <param name="time"></param>
        public void AddMoveText(int gamePly, string move, string time)
        {
            // moveの最大文字数は5か？
            // 34銀引成みたいな?? レアケースなのでそこだけ表示が崩れてもまあいいだろう。

            // 4文字になるようにpaddingしても、半角文字だと、表示が崩れるので
            // 全角スペースでpaddingがなされなくてはならない。
            move = string.Format("{0,-4}", move);
            move = move.Replace(' ', '　'); // 半角スペースから全角スペースへの置換

            var text = string.Format("{0,3}.{1} {2}", gamePly, move, time);
            AddMoveText(text);

            //Console.WriteLine(text);
        }

        /// <summary>
        /// 親ウインドウがリサイズされた時にそれに収まるようにこのコントロール内の文字の大きさを
        /// 調整する。
        /// </summary>
        /// <param name="scale"></param>
        public void OnResize(double scale)
        {
            // 画面を小さくしてもスクロールバーは小さくならないから計算通りのフォントサイズだとまずいのか…。
            double font_size = 18 * scale;

            // ClientSizeはスクロールバーを除いた幅なので、controlのwidthとの比の分だけ
            // fontを小さく表示してやる。
            if (this.Width != 0)
                font_size *= ClientSize.Width / this.Width;

            listBox1.Font = new Font("MS Gothic", (int)font_size, FontStyle.Regular , GraphicsUnit.Pixel);
        }

    }
}
