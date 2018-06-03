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
            var text = string.Format("{0,5}.{1,-6}{2}", gamePly, move, time);
            AddMoveText(text);
        }

        /// <summary>
        /// 親ウインドウがリサイズされた時にそれに収まるようにこのコントロール内の文字の大きさを
        /// 調整する。
        /// </summary>
        /// <param name="scale"></param>
        public void OnResize(double scale)
        {
            listBox1.Font = new Font("MS Gothic", (int)(20 * scale), FontStyle.Regular , GraphicsUnit.Pixel);
        }

    }
}
