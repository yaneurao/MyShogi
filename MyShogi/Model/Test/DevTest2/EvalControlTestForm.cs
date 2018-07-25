using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MyShogi.Model.Shogi.Data;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Test
{
    public partial class EvalControlTestForm : Form
    {
        int seed;
        EvaluationGraphType type;
        bool reverse;
        EvaluationGraphData evaldata;
        public EvalControlTestForm()
        {
            InitializeComponent();
            seed = 75698453; //Environment.TickCount;
            type = EvaluationGraphType.TrigonometricSigmoid;
            reverse = false;
            evaldata = new EvaluationGraphData
            {
                data_array = new[]
                {
                    new GameEvaluationData() { values = new List<EvalValue>() },
                    new GameEvaluationData() { values = new List<EvalValue>() },
                },
                selectedIndex = -1,
                maxIndex = 0,
                type = type,
                reverse = reverse,
            };
        }

        public void EvalUpdate(EvaluationGraphData evaldata)
        {
            evalGraphControl.OnEvalDataChanged(new PropertyChangedEventArgs("EvalData", evaldata));
        }

        static GameEvaluationData randomEval(Random rand, int player, int length)
        {
            var list = new List<EvalValue>();
            EvalValue score = (EvalValue)70;
            for (var i = 0; i < length; ++i)
            {
                if (1 - (i & 1) == player)
                {
                    list.Add(EvalValue.NoValue);
                }
                else if ((Int32)score < -100000000)
                {
                    if (score >= EvalValue.Mated)
                    {
                        list.Add(score = score - 1);
                    }
                    else
                    {
                        list.Add(score = EvalValue.Mated);
                    }
                }
                else if ((Int32)score > +1000000000)
                {
                    if (score <= EvalValue.Mate)
                    {
                        list.Add(score = score + 1);
                    }
                    else
                    {
                        list.Add(score = EvalValue.Mate);
                    }
                }
                else
                {
                    if (rand.Next(0, 500000) < Math.Abs((Int32)score))
                    {
                        if (score < 0)
                        {
                            list.Add(score = EvalValue.Mated + rand.Next(0, 32));
                        }
                        else
                        {
                            list.Add(score = EvalValue.Mate - rand.Next(0, 32));
                        }
                    }
                    else
                    {
                        float nextScore = (float)score * 1.02f + (player < 2 ? 0.00004f : 0.00002f) * rand.Next(-100, +100) * rand.Next(-100, +100) * rand.Next(-1000, +1000);
                        score = (EvalValue)Math.Max(Math.Min(nextScore, +10000000), -10000000);
                        list.Add(score);
                    }
                }

            }
            return new GameEvaluationData() { values = list };
        }

        private void UpdateData_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"seed: {seed}");
            var rand = new Random(seed++);
            var maxIndex = rand.Next(10, 300);
            evaldata = new EvaluationGraphData
            {
                data_array = new[]
                {
                    randomEval(rand, 0, maxIndex),
                    randomEval(rand, 1, maxIndex),
                    randomEval(rand, 2, maxIndex),
                    randomEval(rand, 3, maxIndex),
                    randomEval(rand, 4, maxIndex),
                    randomEval(rand, 5, maxIndex),
                    randomEval(rand, 6, maxIndex),
                },
                selectedIndex = rand.Next(-1, maxIndex),
                maxIndex = maxIndex,
                type = type,
                reverse = reverse,
            };
            EvalUpdate(evaldata);
        }

        private void LinearUpdate_Click(object sender, EventArgs e)
        {
            evaldata.type = type = EvaluationGraphType.Normal;
            EvalUpdate(evaldata);
        }

        private void NonlinearUpdate_Click(object sender, EventArgs e)
        {
            evaldata.type = type = EvaluationGraphType.TrigonometricSigmoid;
            EvalUpdate(evaldata);
        }

        private void WinRateUpdate_Click(object sender, EventArgs e)
        {
            evaldata.type = type = EvaluationGraphType.WinningRate;
            EvalUpdate(evaldata);
        }

        private void GraphReverse_Click(object sender, EventArgs e)
        {
            evaldata.reverse = !(evaldata.reverse);
            EvalUpdate(evaldata);
        }

        private void backwardPly_Click(object sender, EventArgs e)
        {
            evaldata.selectedIndex = Math.Max(evaldata.selectedIndex - 10, -1);
            EvalUpdate(evaldata);
        }

        private void forwardPly_Click(object sender, EventArgs e)
        {
            evaldata.selectedIndex = Math.Min(evaldata.selectedIndex + 10, evaldata.maxIndex - 1);
            EvalUpdate(evaldata);
        }
    }
}
