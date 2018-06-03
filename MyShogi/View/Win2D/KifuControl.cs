using MyShogi.Model.Math;
using MyShogi.Model.Shogi.Core;
using System;
using System.IO;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class KifuControl : UserControl
    {
        public KifuControl()
        {
            InitializeComponent();

            // Visual Studioのデザイナで配置するために、DesignModeでは要らんことをしてはならない。
            // しかしこのタイミングだとDesignModeが正しく判定されない。なんなのこれ…。
            // Controlがネストしているときに起きる例の問題か？
            // cf. https://stackoverflow.com/questions/4498478/designmode-in-subcontrols-is-not-set-correctly

            if (Application.ExecutablePath.IndexOf("devenv.exe", StringComparison.OrdinalIgnoreCase) < 0)
            {
                var file_name = "html/kifu_window.html";
                webBrowser1.Navigate(Path.Combine(Application.StartupPath, file_name).ToString());

#if false
                // これ1回は実行しておかないと、navigateが完了しない
                // しかしこれをするとInitializeComponent()抜ける前にイベントが発生して色々ややこしい..
                Application.DoEvents();

                // テスト用のコード
                AddMoveText("開始局面");
                AddMoveText("1 . ７六歩 (77) 00:00:15");
                // これ、あとで修正する。
#endif
            }
        }

        // -- 以下、棋譜ウインドウに対するオペレーション

        /// <summary>
        /// 現在出力されているテキストの行数
        /// </summary>
        int kifu_line = 0;

        /// <summary>
        /// 棋譜ウインドウに指し手文字列を追加する。(末尾に)
        /// </summary>
        /// <param name="text"></param>
        public void AddMoveText(string text)
        {
            webBrowser1.Document.InvokeScript("add_move_text", new string[] { text });
            ++kifu_line;
        }

        /// <summary>
        /// 棋譜文字列を追加する
        /// </summary>
        /// <param name="gamePly"></param>
        /// <param name="move"></param>
        /// <param name="time"></param>
        public void AddMoveText(int gamePly , string move , string time)
        {
            var text = string.Format("{0,5}.{1,-6}{2}", gamePly, move , time);
            AddMoveText(text);
        }

        /// <summary>
        /// 親ウインドウがリサイズされた時にそれに収まるようにこのコントロール内の文字の大きさを
        /// 調整する。
        /// </summary>
        /// <param name="scale"></param>
        public void OnResize(double scale)
        {
            var scale_text = ((int)(scale * 100 * 0.5)).ToString() + "%";
            webBrowser1.Document.InvokeScript("on_resize", new string[] { scale_text });
        }

    }
}
