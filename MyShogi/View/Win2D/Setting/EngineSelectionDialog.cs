using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// エンジン選択ダイアログ
    /// </summary>
    public partial class EngineSelectionDialog : Form
    {
        /// <summary>
        /// 
        /// 注意)
        /// Visual StudioのデザイナでこのDialogを編集するときは
        ///   AutoScale = Size(96F,96F)
        /// で編集しなければならない。
        /// 
        /// high dpi環境で編集して(192F,192F)とかになっていると、
        /// 解像度の低い実行環境でダイアログの大きさが小さくなってしまう。
        /// (.NET Frameworkのhigh dpiのバグ)
        /// 
        /// </summary>
        public EngineSelectionDialog()
        {
            InitializeComponent();
        }

        public class EngineSelectionViewModel : NotifyObject
        {
            /// <summary>
            /// エンジンの選択ボタンが押された時に
            /// 変更通知イベントを捕捉して使うと良い。
            /// </summary>
            public EngineDefineEx ButtonClicked
            {
                get { return GetValue<EngineDefineEx>("ButtonClicked"); }
                set { SetValue("ButtonClicked", value); }
            }
        }

        public EngineSelectionViewModel ViewModel = new EngineSelectionViewModel();

        private List<EngineDefineEx> engineDefines;

        /// <summary>
        /// 条件にマッチするエンジンだけを選択肢として表示する。
        /// コンストラクタのあと呼び出すこと。
        /// 
        /// NormalSearchSupportEngine : 通常探索に対応しているエンジンを表示するのか。
        /// MateSupportEngine         : 詰将棋探索に対応しているエンジンを表示するのか。
        /// </summary>
        public void InitEngineDefines(bool NormalSearchSupportEngine , bool MateSupportEngine )
        {
            engineDefines = new List<EngineDefineEx>();
            foreach(var e in TheApp.app.EngineDefines)
            {
                var type = e.EngineDefine.EngineType;
                if ((NormalSearchSupportEngine && (type == 0 || type == 2)) ||
                    (MateSupportEngine         && (type == 1 || type == 2))
                    )
                        engineDefines.Add(e);
            }

            InitSelectionControls();

            // フォント変更。コンストラクタのタイミング゛は子Controlが生成されておらず、間に合わなかったので
            // このタイミングで初期化を行う。
            FontUtility.ReplaceFont(this, TheApp.app.Config.FontManager.SettingDialog);
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

                // 子コントロールの「決定」ボタンが押された時のハンドラ
                control.ViewModel.AddPropertyChangedHandler("ButtonClicked", (args) =>
                 {
                     var engine_defines = engineDefines;

                     foreach (int j in All.Int(5))
                     {
                         if (SelectionControls[j].ViewModel == args.sender)
                         {
                             // このイベントを生起してやる。
                             var selectedEngineIndex = SelectionControlTopIndex + j;
                             // 範囲内であることを確認する。
                             if (selectedEngineIndex < engine_defines.Count)
                                 ViewModel.RaisePropertyChanged("ButtonClicked", engineDefines[selectedEngineIndex]);
                             break;
                         }
                     }
                 });
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
            var defines = engineDefines;

            foreach (var i in All.Int(5))
            {
                var j = SelectionControlTopIndex + i;
                if (j < defines.Count)
                {
                    SelectionControls[i].ViewModel.FreePhysicalMemory = (int)free_memory;
                    SelectionControls[i].ViewModel.SetValueAndRaisePropertyChanged("EngineDefine", defines[j].EngineDefine);
                } else
                {
                    SelectionControls[i].ViewModel.FreePhysicalMemory = (int)free_memory;
                    SelectionControls[i].ViewModel.SetValueAndRaisePropertyChanged<EngineDefineEx>("EngineDefine", null); // 無効化しとく
                    // 最初nullで、そのあとnullをセットしてもイベントが発生しないのでこのようにセットしてやる必要がある。
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
