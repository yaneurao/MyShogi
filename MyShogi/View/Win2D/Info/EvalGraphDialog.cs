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
        }

        public void DispatchEvalGraphUpdate(Model.Shogi.LocalServer.LocalGameServer gameServer)
        {
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
    }
}
