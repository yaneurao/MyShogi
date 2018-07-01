using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MyShogi.Model.Shogi.EvaluationGraph;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Test
{
    public partial class EvalControlTestForm : Form
    {
        int seed;
        public EvalControlTestForm()
        {
            InitializeComponent();
            seed = Environment.TickCount;
        }

        public void EvalUpdate(EvaluationGraphData evaldata)
        {
            evalGraphControl1.OnEvalDataChanged(new PropertyChangedEventArgs("EvalData", evaldata));
        }

        static GameEvaluationData randomEval(Random rand, int player, int length)
        {
            var list = new List<int>();
            int score = 70;
            for (var i = 0; i < length; ++i)
            {
                if (1 - (i & 1) == player)
                {
                    list.Add(int.MinValue);
                }
                else
                {
                    float nextScore = score * 1.02f + (player < 2 ? 0.00004f : 0.00002f) * rand.Next(-100, +100) * rand.Next(-100, +100) * rand.Next(-1000, +1000);
                    score = (int)Math.Max(Math.Min(nextScore, +10000000), -10000000);
                    list.Add(score);
                }
            }
            return new GameEvaluationData() { values = list };
        }

        private void EvalRandUpdate(EvaluationGraphType type)
        {
            var rand = new Random(seed++);
            var maxIndex = rand.Next(10, 300);
            EvalUpdate(new EvaluationGraphData
            {
                data_array = new[]
                {
                    randomEval(rand, 0, maxIndex),
                    randomEval(rand, 1, maxIndex),
                    randomEval(rand, 2, maxIndex),
                },
                selectedIndex = rand.Next(-1, maxIndex),
                maxIndex = maxIndex,
                type = type,
            });
        }

        private void LinearUpdate_Click(object sender, EventArgs e)
        {
            EvalRandUpdate(EvaluationGraphType.Normal);
        }

        private void NonlinearUpdate_Click(object sender, EventArgs e)
        {
            EvalRandUpdate(EvaluationGraphType.TrigonometricSigmoid);
        }

        private void WinRateUpdate_Click(object sender, EventArgs e)
        {
            EvalRandUpdate(EvaluationGraphType.WinningRate);
        }
    }
}
