using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Data;
using DColor = System.Drawing.Color; // DColorはDrawing.Colorの意味。

namespace MyShogi.View.Win2D
{
    public partial class EvalGraphControl : UserControl
    {
        EvaluationGraphData evaldata;
        DColor[] playerColor;
        bool scrollCheck;
        ViewGeo geo;
        public EvalGraphControl()
        {
            evaldata = new EvaluationGraphData()
            {
                data_array = new[]
                {
                    new GameEvaluationData() { values = new List<EvalValue>() },
                    new GameEvaluationData() { values = new List<EvalValue>() },
                },
                selectedIndex = -1,
                maxIndex = 0,
                type = EvaluationGraphType.TrigonometricSigmoid,
                reverse = false,
            };

            playerColor = new[]
            {
                DColor.Red,
                DColor.Blue,
                DColor.Black,
                DColor.Green,
                DColor.Purple,
                DColor.Orange,
                DColor.YellowGreen,
            };

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
        /// グラフのスクロール検出時に再描画する
        /// </summary>
        private void EvalGraphControl_Scroll(object sender, ScrollEventArgs e)
        {
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

        /// <summary>
        /// [-1,+1]のy軸値から色の算出
        /// </summary>
        DColor Vert2Color(float vert, bool reverse)
        {
            var c0 = playerColor[2];
            if (float.IsNaN(vert)) { return c0; }
            var c1 = ((vert < 0f) ^ reverse) ? playerColor[1] : playerColor[0];
            float absvert = Math.Abs(vert);
            float absvertr = 1 - absvert;
            return DColor.FromArgb(
                (int)Math.Round(absvertr * c0.A + absvert * c1.A),
                (int)Math.Round(absvertr * c0.R + absvert * c1.R),
                (int)Math.Round(absvertr * c0.G + absvert * c1.G),
                (int)Math.Round(absvertr * c0.B + absvert * c1.B)
            );
        }

        private class ViewGeo
        {
            public int plyMax;
            public int hScrollValue;
            public float scoreFontSize;
            public float scoreFontSizeXmul;
            public float parFontSize;
            public float scaleLineLen;
            public float scaleTextPad;
            public float lPad;
            public float rPad;
            public float uPad;
            public float bPad;
            public float xlen;
            public float ymul;
            public float yadd;
            public float graphBodyWidth;
            public float plyWidth;
            public float plyFontSize;
            public float scoreRound;
            public float scoreLineWidth;
            public float lineWidthMul;
            public int boxWidth;

            public ViewGeo(EvalGraphControl con)
            {
                plyMax = Math.Max((Math.Max(con.evaldata.maxIndex, con.evaldata.selectedIndex) + 9) / 10 * 10, 50);
                var plyMaxLen = $"{plyMax}".Length;
                hScrollValue = con.HorizontalScroll.Value;
                scoreFontSize = Math.Max(Math.Min(con.Width, con.Height) / 16f, 1f);
                parFontSize = Math.Max(Math.Min(con.Width, con.Height) / 20f, 1f);
                // 評価値の文字縦長度
                scoreFontSizeXmul = con.Font.Size / 16f;
                lineWidthMul = Math.Max(Math.Min(con.Width, con.Height) / 512f, 1f);
                scaleLineLen = 4 * lineWidthMul;
                scaleTextPad = 5 * lineWidthMul;
                RectangleF bounds;
                using (var path = new GraphicsPath())
                {
                    path.AddString(
                        "+9999",
                        con.Font.FontFamily,
                        (int)con.Font.Style,
                        scoreFontSize,
                        new PointF(0f, 0f),
                        new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near }
                        );
                    using (var matrix = new Matrix(scoreFontSizeXmul, 0f, 0f, 1f, 0f, 0f))
                    path.Transform(matrix);
                    bounds = path.GetBounds();
                    lPad = 4 + (-1.0f * bounds.Left + scaleTextPad);
                    rPad = 4 + (con.evaldata.type == EvaluationGraphType.TrigonometricSigmoid ?
                        (-0.8f * bounds.Left * parFontSize / scoreFontSize + scaleTextPad) :
                        (-0.1f * bounds.Left * plyMaxLen));
                    graphBodyWidth = con.Width - lPad - rPad;
                    plyWidth = Math.Max(graphBodyWidth / plyMax, 4f);
                    plyFontSize = Math.Max(Math.Min(plyWidth * -80f / (plyMaxLen * bounds.Left), 1f) * scoreFontSize, 1f);
                    uPad = 4 + (int)(+0.5f * bounds.Bottom);
                    bPad = 4 + (int)(+1.0f * bounds.Bottom * plyFontSize / scoreFontSize + scaleTextPad);
                }
                xlen = con.evalGraphPictureBox.Width - lPad - rPad;
                ymul = (con.evalGraphPictureBox.Height - uPad - bPad) * 0.5f;
                yadd = uPad + ymul;
                scoreRound = Math.Max(Math.Min(con.Height * 0.01f, plyWidth * 0.5f), 3f);
                scoreLineWidth = Math.Max(scoreRound * 0.5f, 1f);
                boxWidth = Math.Max((int)(lPad + rPad + plyMax * plyWidth), con.Width);
            }
        }

        /// <summary>
        /// グラフの描画
        /// </summary>
        public void Render(object sender, PaintEventArgs e)
        {
            // ToDo: MainDialogで局面が変更された時（ToolStrip, KifuControlの操作など）にEvalGraphの更新をトリガする

            // フォントの変更。即時反映
            var fontSetter = new FontSetter(this, "EvalGraphControl");
            Disposed += (_sender, args) => fontSetter.Dispose();

            Graphics g = e.Graphics;
            g.PageUnit = GraphicsUnit.Pixel;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            geo = new ViewGeo(this);

            float graphPlyHPad = Math.Min(10f * geo.plyWidth, 0.5f * geo.graphBodyWidth);

            if (evalGraphPictureBox.Width != geo.boxWidth)
            {
                evalGraphPictureBox.Width = geo.boxWidth;
                scrollCheck = true;
                evalGraphPictureBox.Invalidate();
            }

            if (scrollCheck && evaldata.selectedIndex >= 0)
            {
                var x0 = (int)(evaldata.selectedIndex * geo.plyWidth - geo.hScrollValue - graphPlyHPad);
                var x1 = (int)(x0 + geo.lPad + geo.rPad - Width + graphPlyHPad);
                if (x0 < 0)
                {
                    geo.hScrollValue = Math.Max(HorizontalScroll.Value + x0, HorizontalScroll.Minimum);
                    AutoScrollPosition = new Point(geo.hScrollValue, 0);
                }
                else if (x1 > 0)
                {
                    geo.hScrollValue = Math.Min(HorizontalScroll.Value + x1, HorizontalScroll.Maximum);
                    AutoScrollPosition = new Point(geo.hScrollValue, 0);
                }
                evalGraphPictureBox.Invalidate();
            }
            scrollCheck = false;

            using (var brush = new SolidBrush(DColor.White))
            {
                g.FillRectangle(brush, new Rectangle(0, 0, geo.boxWidth, evalGraphPictureBox.Height));
            }

            var vertFunc = evaldata.eval2VertFunc;

            var strformatScore = new StringFormat()
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
            };
            var strformatPar = new StringFormat()
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
            };
            var strformatPly = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
            };

            // 手数目盛りの描画
            using (var bpen = new Pen(DColor.Black, 2f * geo.lineWidthMul))
            using (var bbrush = new SolidBrush(DColor.Black))
            for (var i = 0; ; i += 10)
            using (var pen = new Pen(DColor.Silver, ((i % 50) == 0 ? 2f : 1f) * geo.lineWidthMul))
            {
                float x = geo.lPad + geo.plyWidth * i;
                if (x < geo.lPad + geo.hScrollValue) continue;
                if (x > geo.lPad + geo.graphBodyWidth + geo.hScrollValue) break;
                g.DrawLine(pen, x, geo.yadd - geo.ymul, x, geo.yadd + geo.ymul);
                if (i % 20 != 0) continue;
                g.DrawLine(bpen, x, geo.yadd + geo.ymul, x, geo.yadd + geo.ymul + geo.scaleLineLen);
                using (var path = new GraphicsPath())
                {
                    path.AddString(
                        i.ToString(),
                        Font.FontFamily,
                        (int)Font.Style,
                        geo.plyFontSize,
                        new PointF(0f, 0f),
                        strformatPly
                    );
                    using (var matrix = new Matrix(geo.scoreFontSizeXmul, 0f, 0f, 1f, x, geo.yadd + geo.ymul + geo.scaleTextPad))
                        path.Transform(matrix);
                    g.FillPath(bbrush, path);
                }
            }

            // 評価値目盛りの描画
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
                        var vert = vertFunc((EvalValue)ent.score);
                        var color = DColor.Silver;
                        var y0 = -vert * geo.ymul + geo.yadd;
                        var y1 = +vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, ent.width * geo.lineWidthMul))
                        {
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y0, geo.lPad + geo.hScrollValue + geo.graphBodyWidth, y0);
                            if (ent.score != 0)
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y1, geo.lPad + geo.hScrollValue + geo.graphBodyWidth, y1);
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
                        var vert = vertFunc((EvalValue)ent.score);
                        var color = Vert2Color(vert, evaldata.reverse);
                        var y = -vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, 2f * geo.lineWidthMul))
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue - geo.scaleLineLen, y, geo.lPad + geo.hScrollValue, y);
                        using (var path = new GraphicsPath())
                        {
                            path.AddString(
                                ent.text,
                                Font.FontFamily,
                                (int)Font.Style,
                                geo.scoreFontSize,
                                new PointF(0f, 0f),
                                strformatScore
                            );
                            using (var matrix = new Matrix(geo.scoreFontSizeXmul, 0f, 0f, 1f, geo.lPad + geo.hScrollValue - geo.scaleTextPad, y))
                                path.Transform(matrix);
                            using (var brush = new SolidBrush(color))
                                g.FillPath(brush, path);
                        }
                    }
                    break;
                case EvaluationGraphType.TrigonometricSigmoid:
                    foreach (var ent in evalGraphPictureBox.Height >= 192 ?
                    new[]
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
                    }:
                    new[]
                    {
                        new { score = 0, width = 3 },
                        new { score = 200, width = 1 },
                        new { score = 500, width = 1 },
                        new { score = 1000, width = 2 },
                        new { score = 2500, width = 1 },
                        new { score = 9999, width = 2 },
                        new { score = 99999, width = 1 },
                    })
                    {
                        var vert = vertFunc((EvalValue)ent.score);
                        var color = DColor.Silver;
                        var y0 = -vert * geo.ymul + geo.yadd;
                        var y1 = +vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, ent.width * geo.lineWidthMul))
                        {
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y0, geo.lPad + geo.hScrollValue + geo.graphBodyWidth, y0);
                            if (ent.score != 0)
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y1, geo.lPad + geo.hScrollValue + geo.graphBodyWidth, y1);
                        }
                    }
                    foreach (var ent in new[]
                    {
                        new { text = "±0", score = 0 },
                        new { text = "+200", score = +200 },
                        new { text = "+500", score = +500 },
                        new { text = "+1000", score = +1000 },
                        new { text = "+2500", score = +2500 },
                        new { text = "+9999", score = +9999 },
                        new { text = "+∞", score = (int)EvalValue.MatePlus },
                        new { text = "-200", score = -200 },
                        new { text = "-500", score = -500 },
                        new { text = "-1000", score = -1000 },
                        new { text = "-2500", score = -2500 },
                        new { text = "-9999", score = -9999 },
                        new { text = "-∞", score = (int)EvalValue.MatedMinus },
                    })
                    {
                        var vert = vertFunc((EvalValue)ent.score);
                        var color = Vert2Color(vert, evaldata.reverse);
                        var y = -vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, 2f * geo.lineWidthMul))
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue - geo.scaleLineLen, y, geo.lPad + geo.hScrollValue, y);
                        using (var path = new GraphicsPath())
                        {
                            path.AddString(
                                ent.text,
                                Font.FontFamily,
                                (int)Font.Style,
                                geo.scoreFontSize,
                                new PointF(0f, 0f),
                                strformatScore
                            );
                            using (var matrix = new Matrix(geo.scoreFontSizeXmul, 0f, 0f, 1f, geo.lPad + geo.hScrollValue - geo.scaleTextPad, y))
                                path.Transform(matrix);
                            using (var brush = new SolidBrush(color))
                                g.FillPath(brush, path);
                        }
                    }
                    foreach (var ent in new[]
                    {
                        new { text = "100%", score = (int)EvalValue.MatePlus },
                        new { text = "99%", score = (int)(600f * Math.Log(0.99f / 0.01f)) },
                        new { text = "90%", score = (int)(600f * Math.Log(0.9f / 0.1f)) },
                        new { text = "80%", score = (int)(600f * Math.Log(0.8f / 0.2f)) },
                        new { text = "70%", score = (int)(600f * Math.Log(0.7f / 0.3f)) },
                        new { text = "60%", score = (int)(600f * Math.Log(0.6f / 0.4f)) },
                        new { text = "50%", score = (int)(600f * Math.Log(0.5f / 0.5f)) },
                        new { text = "40%", score = (int)(600f * Math.Log(0.4f / 0.6f)) },
                        new { text = "30%", score = (int)(600f * Math.Log(0.3f / 0.7f)) },
                        new { text = "20%", score = (int)(600f * Math.Log(0.2f / 0.8f)) },
                        new { text = "10%", score = (int)(600f * Math.Log(0.1f / 0.9f)) },
                        new { text = "1%", score = (int)(600f * Math.Log(0.01f / 0.99f)) },
                        new { text = "0%", score = (int)EvalValue.MatedMinus },
                    })
                    {
                        var vert = vertFunc((EvalValue)ent.score);
                        var color = Vert2Color(vert, evaldata.reverse);
                        var y = -vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, 2f * geo.lineWidthMul))
                            g.DrawLine(pen, geo.hScrollValue + geo.lPad + geo.graphBodyWidth, y, geo.hScrollValue + geo.lPad + geo.graphBodyWidth + geo.scaleLineLen, y);
                        using (var path = new GraphicsPath())
                        {
                            path.AddString(
                                ent.text,
                                Font.FontFamily,
                                (int)Font.Style,
                                geo.parFontSize,
                                new PointF(0f, 0f),
                                strformatPar
                            );
                            using (var matrix = new Matrix(geo.scoreFontSizeXmul, 0f, 0f, 1f, geo.hScrollValue + geo.lPad + geo.graphBodyWidth + geo.scaleTextPad, y))
                                path.Transform(matrix);
                            using (var brush = new SolidBrush(color))
                                g.FillPath(brush, path);
                        }
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
                        var color = DColor.Silver;
                        var y0 = -vert * geo.ymul + geo.yadd;
                        var y1 = +vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, ent.width * geo.lineWidthMul))
                        {
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y0, geo.lPad + geo.hScrollValue + geo.graphBodyWidth, y0);
                            if (ent.vert != 0)
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y1, geo.lPad + geo.hScrollValue + geo.graphBodyWidth, y1);
                        }
                    }
                    foreach (var ent in new[]
                    {
                        new { text = "0%", vert = -1.0f },
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
                        var vert = (evaldata.reverse ? -1 : +1) * ent.vert;
                        var color = Vert2Color(vert, evaldata.reverse);
                        var y = -vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, 2f * geo.lineWidthMul))
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue - geo.scaleLineLen, y, geo.lPad + geo.hScrollValue, y);
                        using (var path = new GraphicsPath())
                        {
                            path.AddString(
                                ent.text,
                                Font.FontFamily,
                                (int)Font.Style,
                                geo.scoreFontSize,
                                new PointF(0f, 0f),
                                strformatScore
                            );
                            using (var matrix = new Matrix(geo.scoreFontSizeXmul, 0f, 0f, 1f, geo.lPad + geo.hScrollValue - geo.scaleTextPad, y))
                                path.Transform(matrix);
                            using (var brush = new SolidBrush(color))
                                g.FillPath(brush, path);
                        }
                    }
                    break;
            }

            // 外枠線の描画
            using (var pen = new Pen(DColor.Silver, 3f * geo.lineWidthMul))
            {
                g.DrawRectangle(pen, geo.lPad + geo.hScrollValue, geo.yadd - geo.ymul, geo.graphBodyWidth, geo.ymul + geo.ymul);
            }

            // 評価値推移線の描画
            for (var p = evaldata.data_array.Count() - 1; p >= 0; --p)
            {
                var data = evaldata.data_array[p];
                var color = p >= playerColor.Count() ? playerColor.Last() : playerColor[p];
                using (var pen = new Pen(color, geo.scoreLineWidth))
                for (var i = 1; i < data.values.Count; ++i)
                {
                    if (i * geo.plyWidth < geo.hScrollValue) continue;
                    var y = vertFunc(data.values[i]);
                    if (float.IsNaN(y)) continue;
                    int ip = -1;
                    if (i >= 1 && data.values[i - 1] != EvalValue.NoValue)
                        ip = i - 1;
                    else
                    if (i >= 2 && data.values[i - 2] != EvalValue.NoValue)
                        ip = i - 2;
                    else
                        continue;
                    if (ip * geo.plyWidth < geo.hScrollValue) continue;
                    if (i * geo.plyWidth > geo.hScrollValue + geo.graphBodyWidth) break;
                    var yp = vertFunc(data.values[ip]);
                    g.DrawLine(pen, ip * geo.plyWidth + geo.lPad, -yp * geo.ymul + geo.yadd, i * geo.plyWidth + geo.lPad, -y * geo.ymul + geo.yadd);
                }
            }

            if (evaldata.selectedIndex * geo.plyWidth >= geo.hScrollValue && evaldata.selectedIndex * geo.plyWidth <= geo.hScrollValue + geo.graphBodyWidth)
            {
                var x = evaldata.selectedIndex * geo.plyWidth + geo.lPad;
                using (var pen = new Pen(DColor.DarkTurquoise, 3f))
                {
                    g.DrawLine(pen, x, geo.uPad, x, geo.uPad + geo.ymul * 2);
                }
            }

            // 評価値点の描画
            for (var i = 0; i < evaldata.maxIndex; ++i)
            {
                if (i * geo.plyWidth < geo.hScrollValue) continue;
                if (i * geo.plyWidth > geo.hScrollValue + geo.graphBodyWidth) break;
                // 逆順に描画して番号の若い方を表になるようにする
                for (var p = evaldata.data_array.Count() - 1; p >= 0; --p)
                {
                    if (i >= evaldata.data_array[p].values.Count()) continue;
                    var color = p >= playerColor.Count() ? playerColor.Last() : playerColor[p];
                    var y = vertFunc(evaldata.data_array[p].values[i]);
                    if (float.IsNaN(y)) continue;
                    using (var brush = new SolidBrush(color))
                        g.FillEllipse(brush, i * geo.plyWidth + geo.lPad - geo.scoreRound, -y * geo.ymul + geo.yadd - geo.scoreRound, geo.scoreRound * 2, geo.scoreRound * 2);
                }
            }

        }

        // マウス移動イベント
        private void OnMouseMove(Point p)
        {
            // noop
        }

        // マウス左クリックイベント
        private void OnClick(Point p)
        {
            var xl = geo.hScrollValue + geo.lPad;
            var xr = xl + geo.xlen;
            var ply = (p.X - geo.lPad) / geo.plyWidth;
#if DEBUG
            Console.WriteLine($"EvalGraph Click: Xl{xl} Xr{xr} X{p.X} Y{p.Y} ply{ply}");
#endif
            // ToDo: 入力を受けた手数の局面に移動する
        }

        // マウス左ボタンドラッグイベント
        private void OnDrag(Point p0, Point p1)
        {
            var xl = geo.hScrollValue + geo.lPad;
            var xr = xl + geo.xlen;
            var ply0 = (p0.X - geo.lPad) / geo.plyWidth;
            var ply1 = (p1.X - geo.lPad) / geo.plyWidth;
#if DEBUG
            Console.WriteLine($"EvalGraph Drag: Xl{xl} Xr{xr} X0{p0.X} Y0{p0.Y} ply0{ply0} X1{p1.X} Y1{p1.Y} ply1{ply1}");
#endif
            // ToDo: 用途があれば…？ (グラフのスクロール？)
        }

        // マウス右クリックイベント
        private void OnRightClick(Point p)
        {
            var xl = geo.hScrollValue + geo.lPad;
            var xr = xl + geo.xlen;
            var ply = (p.X - geo.lPad) / geo.plyWidth;
#if DEBUG
            Console.WriteLine($"EvalGraph RightClick: Xl{xl} Xr{xr} X{p.X} Y{p.Y} ply{ply}");
#endif
            // ToDo: 用途があれば…？ (右クリックメニューを開く？)
        }

        private void evalGraphPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseLastDown = e.Location;
            }
            else if (e.Button == MouseButtons.Right)
            {
                OnRightClick(e.Location);
            }
        }

        private void evalGraphPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            var p = e.Location;
            if (e.Button == MouseButtons.Left)
            {
                if (mouseLastDown == p)
                {
                    OnClick(p);
                }
                else
                {
                    OnDrag(mouseLastDown, p);
                }
            }
        }

        private void evalGraphPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e.Location);
        }

        /// <summary>
        /// MouseDownが最後に発生した場所
        /// </summary>
        private Point mouseLastDown = new Point(-1, -1); // 意味のない地点

    }
}
