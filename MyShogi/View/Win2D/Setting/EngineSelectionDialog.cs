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

        private EngineSelectionControl[] SelectionControls;

        /// <summary>
        /// EngineSelectionControlを5つ生成して貼り付けておく。
        /// </summary>
        private void InitSelectionControls()
        {
            // 空き物理メモリ[MB]
            var free_memory = Enviroment.GetFreePhysicalMemory() / 1024;

            var defines = TheApp.app.engine_defines;

            // エンジン5個表示できる。
            SelectionControls = new EngineSelectionControl[5];
            var pos = new Point(0, 3);
            foreach (var i in All.Int(5))
            {
                var control = new EngineSelectionControl();
                control.Location = pos;
                pos = new Point(pos.X , pos.Y + control.Height);
                SelectionControls[i] = control;

                control.ViewModel.FreePhysicalMemory = (int)free_memory;
                control.ViewModel.EngineDefine = defines[i];

                // このFormの子として追加する。
                this.Controls.Add(control);
            }
            ClientSize = new Size(SelectionControls[4].Width + 3, pos.Y + 3);
        }

    }
}
