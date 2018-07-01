using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.EvaluationGraph;

namespace MyShogi.View.Win2D
{
    public partial class EvalGraphControl : UserControl
    {
        EvaluationGraphData evaldata;
        Color[] playerColor;
        string fontFamilyName;
        bool scrollCheck;
        public EvalGraphControl()
        {
            evaldata = new EvaluationGraphData()
            {
                data_array = new[]
                {
                    new GameEvaluationData() { values = new List<int>() },
                    new GameEvaluationData() { values = new List<int>() },
                },
                selectedIndex = -1,
                maxIndex = 0,
                type = EvaluationGraphType.TrigonometricSigmoid,
            };

            playerColor = new[]
            {
                Color.Red,
                Color.Blue,
                Color.Black,
            };

            {
                FontFamily[] ff = FontFamily.Families;
                fontFamilyName = FontFamily.GenericMonospace.Name;
                foreach (var n in new[] { "Consolas" })
                foreach (var f in ff)
                if (f.Name == n)
                {
                    fontFamilyName = f.Name;
                    goto fontLoopOut;
                }
                fontLoopOut:;
            }

            InitializeComponent();

            evalGraphPictureBox.Paint += Render;
        }

        /// <summary>
        /// サイズ変更時にウィンドウ全体を再描画する
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            scrollCheck = true;
            evalGraphPictureBox.Invalidate();
        }

        /// <summary>
        /// [UI thread] : リストが変更されたときに呼び出されるハンドラ
        /// </summary>
        public void OnEvalDataChanged(PropertyChangedEventArgs args)
        {
            evaldata = args.value as EvaluationGraphData;
            scrollCheck = true;
            evalGraphPictureBox.Invalidate();
        }

        public void ScrollUpdate(int newValue)
        {
            evalGraphPictureBox.Invalidate();
        }

        Func<int, float> Score2VertFunc(EvaluationGraphType evaltype)
        {
            switch (evaltype)
            {
                case EvaluationGraphType.Normal:
                    return (int score) => score == int.MinValue ? float.NaN :
                        Math.Min(Math.Max(score / 3000f, -1f), +1f);
                case EvaluationGraphType.TrigonometricSigmoid:
                    return (int score) => score == int.MinValue ? float.NaN :
                        (float)(Math.Asin(Math.Atan(score * 0.00201798867190979486291580478906) * 2 / Math.PI) * 2 / Math.PI);
                case EvaluationGraphType.WinningRate:
                    return (int score) => score == int.MinValue ? float.NaN :
                        (float)(Math.Tanh(score / 1200.0));
                default:
                    return (int score) => score == int.MinValue ? float.NaN : 0f;
            }
        }

        Color Vert2Color(float vert)
        {
            Color c0 = playerColor[2];
            if (float.IsNaN(vert)) { return c0; }
            Color c1 = vert < 0f ? playerColor[1] : playerColor[0];
            float absvert = Math.Abs(vert);
            float absvertr = 1 - absvert;
            return Color.FromArgb(
                (int)Math.Round(absvertr * c0.A + absvert * c1.A),
                (int)Math.Round(absvertr * c0.R + absvert * c1.R),
                (int)Math.Round(absvertr * c0.G + absvert * c1.G),
                (int)Math.Round(absvertr * c0.B + absvert * c1.B)
            );
        }

        public void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int plyMax = Math.Max((evaldata.maxIndex + 9) / 10 * 10, 50);
            int hScrollValue = this.HorizontalScroll.Value;
            float scoreFontSize = Math.Max(Math.Min(Width, Height) / 64f, 6f);
            float scaleLineLen = 6;
            float scaleTextPad = 8;
            int lPad = (int)(8.0f * scoreFontSize + scaleTextPad);
            int rPad = (int)(2.7f * scoreFontSize);
            int uPad = (int)(2.7f * scoreFontSize);
            int bPad = (int)(2.7f * scoreFontSize + scaleTextPad);
            float plyWidth = Math.Max((0f + Width - lPad - rPad) / plyMax, 4f);
            float plyFontSize = Math.Max(Math.Min(plyWidth * 2f, scoreFontSize), 6f);
            float scoreRound = Math.Max(Math.Min(Height * 0.01f, plyWidth * 0.5f), 3f);
            float scoreLineWidth = Math.Max(scoreRound * 0.25f, 1f);
            int boxWidth = Math.Max((int)(lPad + rPad + plyMax * plyWidth), Width);

            if (scrollCheck && evaldata.selectedIndex >= 0)
            {
                var x0 = (int)(evaldata.selectedIndex * plyWidth - hScrollValue);
                var x1 = (int)(x0 + lPad + rPad - Width);
                if (x0 < 0)
                {
                    hScrollValue = Math.Max(HorizontalScroll.Value + x0, HorizontalScroll.Minimum);
                    AutoScrollPosition = new Point(hScrollValue, 0);
                }
                else if (x1 > 0)
                {
                    hScrollValue = Math.Min(HorizontalScroll.Value + x1, HorizontalScroll.Maximum);
                    AutoScrollPosition = new Point(hScrollValue, 0);
                }
                evalGraphPictureBox.Invalidate();
            }
            scrollCheck = false;

            if (evalGraphPictureBox.Width != boxWidth)
            {
                evalGraphPictureBox.Width = boxWidth;
                scrollCheck = true;
                evalGraphPictureBox.Invalidate();
            }

            int xlen = evalGraphPictureBox.Width - lPad - rPad;
            float ymul = (evalGraphPictureBox.Height - uPad - bPad) * 0.5f;
            float yadd = uPad + ymul;

            using (var brush = new SolidBrush(Color.White))
            {
                g.FillRectangle(brush, new Rectangle(0, 0, evalGraphPictureBox.Width, evalGraphPictureBox.Height));
            }

            var vertFunc = Score2VertFunc(evaldata.type);

            var strformatScore = new StringFormat()
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
            };
            var strformatPly = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
            };

            // 手数目盛りの描画
            using (var font = new Font(fontFamilyName, plyFontSize, FontStyle.Regular))
            using (var bpen = new Pen(Color.Black, 2))
            using (var bbrush = new SolidBrush(Color.Black))
            for (var i = 0; ; i += 10)
            using (var pen = new Pen(Color.Silver, (i % 50) == 0 ? 2 : 1))
            {
                float x = lPad + plyWidth * i;
                if (x < lPad + hScrollValue) continue;
                if (x + rPad > evalGraphPictureBox.Width) break;
                g.DrawLine(pen, x, yadd - ymul, x, yadd + ymul);
                if (i % 20 != 0) continue;
                g.DrawLine(bpen, x, yadd + ymul, x, yadd + ymul + scaleLineLen);
                g.DrawString(i.ToString(), font, bbrush, x, yadd + ymul + scaleTextPad, strformatPly);
            }

            // 評価値目盛りの描画
            using (var font = new Font(fontFamilyName, scoreFontSize, FontStyle.Regular))
            switch (evaldata.type)
            {
                case EvaluationGraphType.Normal:
                    foreach (var ent in evalGraphPictureBox.Height >= 256 ?
                    new[]
                    {
                        new { score = 0, width = 3 },
                        new { score = 100, width = 1 },
                        new { score = 200, width = 1 },
                        new { score = 300, width = 1 },
                        new { score = 400, width = 1 },
                        new { score = 500, width = 2 },
                        new { score = 600, width = 1 },
                        new { score = 700, width = 1 },
                        new { score = 800, width = 1 },
                        new { score = 900, width = 1 },
                        new { score = 1000, width = 3 },
                        new { score = 1100, width = 1 },
                        new { score = 1200, width = 1 },
                        new { score = 1300, width = 1 },
                        new { score = 1400, width = 1 },
                        new { score = 1500, width = 2 },
                        new { score = 1600, width = 1 },
                        new { score = 1700, width = 1 },
                        new { score = 1800, width = 1 },
                        new { score = 1900, width = 1 },
                        new { score = 2000, width = 3 },
                        new { score = 2100, width = 1 },
                        new { score = 2200, width = 1 },
                        new { score = 2300, width = 1 },
                        new { score = 2400, width = 1 },
                        new { score = 2500, width = 2 },
                        new { score = 2600, width = 1 },
                        new { score = 2700, width = 1 },
                        new { score = 2800, width = 1 },
                        new { score = 2900, width = 1 },
                        new { score = 3000, width = 3 },
                    }:
                    new[]
                    {
                        new { score = 0, width = 3 },
                        new { score = 500, width = 2 },
                        new { score = 1000, width = 3 },
                        new { score = 1500, width = 2 },
                        new { score = 2000, width = 3 },
                        new { score = 2500, width = 2 },
                        new { score = 3000, width = 3 },
                    })
                    {
                        var vert = vertFunc(ent.score);
                        var color = Color.Silver;
                        var y0 = -vert * ymul + yadd;
                        var y1 = +vert * ymul + yadd;
                        using (var pen = new Pen(color, ent.width))
                        {
                            g.DrawLine(pen, lPad + hScrollValue, y0, lPad + xlen, y0);
                            if (ent.score != 0)
                            g.DrawLine(pen, lPad + hScrollValue, y1, lPad + xlen, y1);
                        }
                    }
                    foreach (var ent in new[]
                    {
                        new { text = "±0", score = 0 },
                        new { text = "+500", score = +500 },
                        new { text = "+1000", score = +1000 },
                        new { text = "+1500", score = +1500 },
                        new { text = "+2000", score = +2000 },
                        new { text = "+2500", score = +2500 },
                        new { text = "+3000", score = +3000 },
                        new { text = "-500", score = -500 },
                        new { text = "-1000", score = -1000 },
                        new { text = "-1500", score = -1500 },
                        new { text = "-2000", score = -2000 },
                        new { text = "-2500", score = -2500 },
                        new { text = "-3000", score = -3000 },
                    })
                    {
                        var vert = vertFunc(ent.score);
                        var color = Vert2Color(vert);
                        var y = -vert * ymul + yadd;
                        using (var pen = new Pen(color, 2))
                            g.DrawLine(pen, lPad + hScrollValue - scaleLineLen, y, lPad + hScrollValue, y);
                        using (var brush = new SolidBrush(color))
                            g.DrawString(ent.text, font, brush, lPad + hScrollValue - scaleTextPad, y, strformatScore);
                    }
                    break;
                case EvaluationGraphType.TrigonometricSigmoid:
                    foreach (var ent in new[]
                    {
                        new { score = 0, width = 3 },
                        new { score = 100, width = 1 },
                        new { score = 200, width = 1 },
                        new { score = 300, width = 1 },
                        new { score = 400, width = 1 },
                        new { score = 500, width = 1 },
                        new { score = 600, width = 1 },
                        new { score = 700, width = 1 },
                        new { score = 800, width = 1 },
                        new { score = 900, width = 1 },
                        new { score = 1000, width = 2 },
                        new { score = 2000, width = 1 },
                        new { score = 3000, width = 1 },
                        new { score = 4000, width = 1 },
                        new { score = 5000, width = 1 },
                        new { score = 6000, width = 1 },
                        new { score = 7000, width = 1 },
                        new { score = 8000, width = 1 },
                        new { score = 9000, width = 1 },
                        new { score = 9999, width = 2 },
                        new { score = 99999, width = 1 },
                    })
                    {
                        var vert = vertFunc(ent.score);
                        var color = Color.Silver;
                        var y0 = -vert * ymul + yadd;
                        var y1 = +vert * ymul + yadd;
                        using (var pen = new Pen(color, ent.width))
                        {
                            g.DrawLine(pen, lPad + hScrollValue, y0, lPad + xlen, y0);
                            if (ent.score != 0)
                            g.DrawLine(pen, lPad + hScrollValue, y1, lPad + xlen, y1);
                        }
                    }
                    foreach (var ent in new[]
                    {
                        new { text = "±0", score = 0 },
                        new { text = "+200", score = +200 },
                        new { text = "+500", score = +500 },
                        new { text = "+1000", score = +1000 },
                        new { text = "+2000", score = +2000 },
                        new { text = "+9999", score = +9999 },
                        new { text = "+∞", score = +int.MaxValue },
                        new { text = "-200", score = -200 },
                        new { text = "-500", score = -500 },
                        new { text = "-1000", score = -1000 },
                        new { text = "-2000", score = -2000 },
                        new { text = "-9999", score = -9999 },
                        new { text = "-∞", score = -int.MaxValue },
                    })
                    {
                        var vert = vertFunc(ent.score);
                        var color = Vert2Color(vert);
                        var y = -vert * ymul + yadd;
                        using (var pen = new Pen(color, 2))
                            g.DrawLine(pen, lPad + hScrollValue - scaleLineLen, y, lPad + hScrollValue, y);
                        using (var brush = new SolidBrush(color))
                            g.DrawString(ent.text, font, brush, lPad + hScrollValue - scaleTextPad, y, strformatScore);
                    }
                    break;
                case EvaluationGraphType.WinningRate:
                    foreach (var ent in new[]
                    {
                        new { vert = -1.0f, width = 3 },
                        new { vert = -0.9f, width = 1 },
                        new { vert = -0.8f, width = 2 },
                        new { vert = -0.7f, width = 1 },
                        new { vert = -0.6f, width = 2 },
                        new { vert = -0.5f, width = 1 },
                        new { vert = -0.4f, width = 2 },
                        new { vert = -0.3f, width = 1 },
                        new { vert = -0.2f, width = 2 },
                        new { vert = -0.1f, width = 1 },
                        new { vert = +0.0f, width = 3 },
                        new { vert = +0.1f, width = 1 },
                        new { vert = +0.2f, width = 2 },
                        new { vert = +0.3f, width = 1 },
                        new { vert = +0.4f, width = 2 },
                        new { vert = +0.5f, width = 1 },
                        new { vert = +0.6f, width = 2 },
                        new { vert = +0.7f, width = 1 },
                        new { vert = +0.8f, width = 2 },
                        new { vert = +0.9f, width = 1 },
                        new { vert = +1.0f, width = 3 },
                    })
                    {
                        var vert = ent.vert;
                        var color = Color.Silver;
                        var y0 = -vert * ymul + yadd;
                        var y1 = +vert * ymul + yadd;
                        using (var pen = new Pen(color, ent.width))
                        {
                            g.DrawLine(pen, lPad + hScrollValue, y0, lPad + xlen, y0);
                            if (ent.vert != 0)
                            g.DrawLine(pen, lPad + hScrollValue, y1, lPad + xlen, y1);
                        }
                    }
                    foreach (var ent in new[]
                    {
                        new { text = "0%", vert = -1f },
                        new { text = "10%", vert = -0.8f },
                        new { text = "20%", vert = -0.6f },
                        new { text = "30%", vert = -0.4f },
                        new { text = "40%", vert = -0.2f },
                        new { text = "50%", vert = +0.0f },
                        new { text = "60%", vert = +0.2f },
                        new { text = "70%", vert = +0.4f },
                        new { text = "80%", vert = +0.6f },
                        new { text = "90%", vert = +0.8f },
                        new { text = "100%", vert = +1.0f },
                    })
                    {
                        var vert = ent.vert;
                        var color = Vert2Color(vert);
                        var y = -vert * ymul + yadd;
                        using (var pen = new Pen(color, 2))
                            g.DrawLine(pen, lPad + hScrollValue - scaleLineLen, y, lPad + hScrollValue, y);
                        using (var brush = new SolidBrush(color))
                            g.DrawString(ent.text, font, brush, lPad + hScrollValue - scaleTextPad, y, strformatScore);
                    }
                    break;
            }

            // 外枠線の描画
            using (var pen = new Pen(Color.Silver, 3f))
            {
                g.DrawRectangle(pen, lPad + hScrollValue, yadd - ymul, xlen - hScrollValue, ymul + ymul);
            }

            // 評価値推移線の描画
            for (var p = 0; p < evaldata.data_array.Count(); ++p)
            {
                var data = evaldata.data_array[p];
                var color = p >= playerColor.Count() ? playerColor.Last() : playerColor[p];
                using (var pen = new Pen(color, scoreLineWidth))
                for (var i = 1; i < data.values.Count; ++i)
                {
                    if (i * plyWidth < hScrollValue) continue;
                    var y = vertFunc(data.values[i]);
                    if (float.IsNaN(y)) continue;
                    int ip = -1;
                    if (i >= 1 && data.values[i - 1] != int.MinValue)
                        ip = i - 1;
                    else
                    if (i >= 2 && data.values[i - 2] != int.MinValue)
                        ip = i - 2;
                    else
                        continue;
                    if (ip * plyWidth < hScrollValue) continue;
                    var yp = vertFunc(data.values[ip]);
                    g.DrawLine(pen, ip * plyWidth + lPad, -yp * ymul + yadd, i * plyWidth + lPad, -y * ymul + yadd);
                }
            }

            if (evaldata.selectedIndex * plyWidth >= hScrollValue)
            {
                var x = evaldata.selectedIndex * plyWidth + lPad;
                using (var pen = new Pen(Color.DarkTurquoise, 3f))
                {
                    g.DrawLine(pen, x, uPad, x, uPad + ymul * 2);
                }
            }

            // 評価値点の描画
            for (var i = 0; i < evaldata.maxIndex; ++i)
            {
                if (i * plyWidth < hScrollValue) continue;
                for (var p = 0; p < evaldata.data_array.Count(); ++p)
                {
                    if (i >= evaldata.data_array[p].values.Count()) continue;
                    var color = p >= playerColor.Count() ? playerColor.Last() : playerColor[p];
                    var y = vertFunc(evaldata.data_array[p].values[i]);
                    if (float.IsNaN(y)) continue;
                    using (var brush = new SolidBrush(color))
                        g.FillEllipse(brush, i * plyWidth + lPad - scoreRound, -y * ymul + yadd - scoreRound, scoreRound * 2, scoreRound * 2);
                }
            }

        }

        private void EvalGraphControl_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
                ScrollUpdate(e.NewValue);
        }
    }
}
