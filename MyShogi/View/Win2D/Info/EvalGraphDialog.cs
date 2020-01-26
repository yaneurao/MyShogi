using MyShogi.App;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Info
{
    /// <summary>
    /// 形勢グラフ
    /// </summary>
    public partial class EvalGraphDialog : System.Windows.Forms.Form
    {
        private Model.Shogi.Data.EvaluationGraphData graphData;
        public Model.Shogi.Data.EvaluationGraphType graphType { get; set; }
        public bool reverse { get; set; }
        public EvalGraphDialog()
        {
            graphData = null;
            graphType = Model.Shogi.Data.EvaluationGraphType.TrigonometricSigmoid;
            reverse = false;
            InitializeComponent();
            FontUtility.ReplaceFont(this, TheApp.app.Config.FontManager.SettingDialog);
        }

        public DockWindow.DockWindowViewModel ViewModel = new DockWindow.DockWindowViewModel();
        private EvalGraphControl evalGraphControl;

        /// <summary>
        /// このWindowの上に乗っけるControlを設定する。
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(EvalGraphControl control, Form mainForm, Model.Common.Tool.DockManager dockManager)
        {
            ViewModel.Control = control;
            panel1.Controls.Add(control);
            control.Size = new System.Drawing.Size(panel1.Size.Width, panel1.Size.Height);
            evalGraphControl = control;
            ViewModel.DockManager = dockManager;
            ViewModel.MainForm = mainForm;
        }

        /// <summary>
        /// このWindowに乗っけていたControlを解除する。
        /// </summary>
        public void RemoveControl()
        {
            var control = ViewModel.Control;
            if (control != null)
            {
                ViewModel.Control = null;
                panel1.Controls.Remove(control);
                evalGraphControl = null;
                ViewModel.DockManager = null;
                ViewModel.MainForm = null;
            }
        }

        public void DispatchEvalGraphUpdate(Model.Shogi.LocalServer.LocalGameServer gameServer)
        {
            if (evalGraphControl == null) return;
            graphData = gameServer.GetEvaluationGraphDataCommand(graphType, reverse);
            evalGraphControl.OnEvalDataChanged(new Model.Common.ObjectModel.PropertyChangedEventArgs("EvalData", graphData));
        }

        private void LinearToolStripMenuItem_Click(object sender, System.EventArgs e)
        { 
            graphType = Model.Shogi.Data.EvaluationGraphType.Normal;
            if (graphData != null)
            {
                graphData.type = graphType;
                graphData.reverse = reverse;
                evalGraphControl.OnEvalDataChanged(new Model.Common.ObjectModel.PropertyChangedEventArgs("EvalData", graphData));
            }
        }

        private void NonlinearToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            graphType = Model.Shogi.Data.EvaluationGraphType.TrigonometricSigmoid;
            if (graphData != null)
            {
                graphData.type = graphType;
                graphData.reverse = reverse;
                evalGraphControl.OnEvalDataChanged(new Model.Common.ObjectModel.PropertyChangedEventArgs("EvalData", graphData));
            }
        }

        private void WinrateToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            graphType = Model.Shogi.Data.EvaluationGraphType.WinningRate;
            if (graphData != null)
            {
                graphData.type = graphType;
                graphData.reverse = reverse;
                evalGraphControl.OnEvalDataChanged(new Model.Common.ObjectModel.PropertyChangedEventArgs("EvalData", graphData));
            }
        }

        private void ReverseToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            reverse = !reverse;
            if (graphData != null)
            {
                graphData.type = graphType;
                graphData.reverse = reverse;
                evalGraphControl.OnEvalDataChanged(new Model.Common.ObjectModel.PropertyChangedEventArgs("EvalData", graphData));
            }
        }

        /// <summary>
        /// ウインドウの位置・サイズが変更になったので、DockManagerで管理しているLocationなどを更新する。
        /// </summary>
        private void SaveWindowLocation()
        {
            var dockManager = ViewModel.DockManager;
            if (dockManager != null)
                dockManager.SaveWindowLocation(ViewModel.MainForm, this);
        }

        private void EvalGraphDialog_Move(object sender, System.EventArgs e)
        {
            SaveWindowLocation();
        }

        private void EvalGraphDialog_Resize(object sender, System.EventArgs e)
        {
            SaveWindowLocation();
            if (evalGraphControl != null)
                evalGraphControl.Size = new System.Drawing.Size(panel1.Size.Width, panel1.Size.Height);
        }
    }
}
