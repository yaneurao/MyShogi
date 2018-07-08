using MyShogi.App;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// エンジン選択ダイアログ
    /// </summary>
    public partial class EngineSelectionDialog : Form
    {
        public EngineSelectionDialog()
        {
            InitializeComponent();

            InitSelectionControls();
        }

        /// <summary>
        /// このFormにぶら下がっているエンジン選択(1個)のControl×5個
        /// </summary>
        private EngineSelectionControl[] SelectionControls;

        /// <summary>
        /// SelectionControlsの画面に表示する一番上のindex
        /// </summary>
        private int SelectionControlTopIndex = 0;

        /// <summary>
        /// EngineSelectionControlを5つ生成して貼り付けておく。
        /// </summary>
        private void InitSelectionControls()
        {
            // エンジン5個表示できる。
            SelectionControls = new EngineSelectionControl[5];
            var pos = new Point(0, 3);
            foreach (var i in All.Int(5))
            {
                var control = new EngineSelectionControl();
                control.Location = pos;
                pos = new Point(pos.X , pos.Y + control.Height);
                SelectionControls[i] = control;

                // このFormの子として追加する。
                this.Controls.Add(control);
            }

            // ボタンも移動させる。

            int width = SelectionControls[4].Width + 3;
            pos = new Point(pos.X, pos.Y + 3);

            button1.Location = new Point(10, pos.Y);
            button2.Location = new Point(width - 10 - button2.Width , pos.Y);
            pos = new Point(pos.X, pos.Y + button1.Height);

            ClientSize = new Size(width , pos.Y + 3);

            UpdateSelectionControls();
        }

        /// <summary>
        /// 5つのエンジン選択ダイアログを初期化する。
        /// </summary>
        private void UpdateSelectionControls()
        {
            // 空き物理メモリ[MB]
            var free_memory = Enviroment.GetFreePhysicalMemory() / 1024;
            var defines = TheApp.app.engine_defines;

            foreach (var i in All.Int(5))
            {
                var j = SelectionControlTopIndex + i;
                if (j < defines.Count)
                {
                    SelectionControls[i].ViewModel.FreePhysicalMemory = (int)free_memory;
                    SelectionControls[i].ViewModel.EngineDefine = defines[j];
                } else
                {
                    SelectionControls[i].ViewModel.FreePhysicalMemory = (int)free_memory;
                    SelectionControls[i].ViewModel.EngineDefine = null; // 無効化しとく
                }
            }

            // pager、これ以上めくられないなら押せないようにしておく。

            button1.Enabled = SelectionControlTopIndex - 5 >= 0;
            button2.Enabled = SelectionControlTopIndex + 5 < defines.Count;
        }

        /// <summary>
        /// 前ページボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, System.EventArgs e)
        {
            SelectionControlTopIndex -= 5;

            // 押せないようにしといたはずなんだけど(´ω｀)
            if (SelectionControlTopIndex < 0)
                SelectionControlTopIndex = 0;

            UpdateSelectionControls();
        }

        /// <summary>
        /// 次ページボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, System.EventArgs e)
        {
            SelectionControlTopIndex += 5;

            // 押せないようにしといたはずなんだけど(´ω｀)
            if (SelectionControlTopIndex > SelectionControls.Length)
                SelectionControlTopIndex -= 5;

            UpdateSelectionControls();
        }
    }
}
