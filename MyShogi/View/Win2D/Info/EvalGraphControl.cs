using System;
using System.Collections.Generic;
using System.Drawing;
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
        string fontFamilyName;
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

            {
                FontFamily[] ff = FontFamily.Families;
                fontFamilyName = FontFamily.GenericMonospace.Name;
                foreach (var n in new[] { "Consolas", "Inconsolata" })
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
            public float scaleLineLen;
            public float scaleTextPad;
            public float lPad;
            public float rPad;
            public float uPad;
            public float bPad;
            public float graphBodyWidth;
            public float plyWidth;
            public float plyFontSize;
            public float scoreRound;
            public float scoreLineWidth;
            public int boxWidth;
            public float xlen;
            public float ymul;
            public float yadd;

            public ViewGeo(EvalGraphControl con)
            {
                plyMax = Math.Max((Math.Max(con.evaldata.maxIndex, con.evaldata.selectedIndex) + 9) / 10 * 10, 50);
                hScrollValue = con.HorizontalScroll.Value;
                scoreFontSize = Math.Min(Math.Max(con.Width / 16f, 12f), Math.Max(con.Height / 16f, 8f));
                scaleLineLen = 6;
                scaleTextPad = 8;
                {
                    var path = new System.Drawing.Drawing2D.GraphicsPath();
                    path.AddString("+9999", new FontFamily(con.fontFamilyName), (int)FontStyle.Regular, scoreFontSize, new PointF(0, 0), new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near });
                    var bounds = path.GetBounds();
                    lPad = 4 + (int)(-1.0f * bounds.Left + scaleTextPad);
                    rPad = 4 + (int)(-0.3f * bounds.Left);
                    uPad = 4 + (int)(+0.5f * bounds.Bottom);
                    bPad = 4 + (int)(+1.0f * bounds.Bottom + scaleTextPad);
                }
                xlen = con.evalGraphPictureBox.Width - lPad - rPad;
                ymul = (con.evalGraphPictureBox.Height - uPad - bPad) * 0.5f;
                yadd = uPad + ymul;
                graphBodyWidth = con.Width - lPad - rPad;
                plyWidth = Math.Max(graphBodyWidth / plyMax, 4f);
                plyFontSize = Math.Max(Math.Min(plyWidth * 4f, scoreFontSize), 8f);
                scoreRound = Math.Max(Math.Min(con.Height * 0.01f, plyWidth * 0.5f), 3f);
                scoreLineWidth = Math.Max(scoreRound * 0.25f, 1f);
                boxWidth = Math.Max((int)(lPad + rPad + plyMax * plyWidth), con.Width);
            }
        }

        /// <summary>
        /// グラフの描画
        /// </summary>
        public void Render(object sender, PaintEventArgs e)
        {
            // ToDo: MainDialogで局面が変更された時（ToolStrip, KifuControlの操作など）にEvalGraphの更新をトリガする

            Graphics g = e.Graphics;
            g.PageUnit = GraphicsUnit.Pixel;
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
                g.FillRectangle(brush, new Rectangle(0, 0, evalGraphPictureBox.Width, evalGraphPictureBox.Height));
            }

            var vertFunc = evaldata.eval2VertFunc;

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
            using (var font = new Font(fontFamilyName, geo.plyFontSize, GraphicsUnit.Pixel))
            using (var bpen = new Pen(DColor.Black, 2))
            using (var bbrush = new SolidBrush(DColor.Black))
            for (var i = 0; ; i += 10)
            using (var pen = new Pen(DColor.Silver, (i % 50) == 0 ? 2 : 1))
            {
                float x = geo.lPad + geo.plyWidth * i;
                if (x < geo.lPad + geo.hScrollValue) continue;
                if (x + geo.rPad > evalGraphPictureBox.Width) break;
                g.DrawLine(pen, x, geo.yadd - geo.ymul, x, geo.yadd + geo.ymul);
                if (i % 20 != 0) continue;
                g.DrawLine(bpen, x, geo.yadd + geo.ymul, x, geo.yadd + geo.ymul + geo.scaleLineLen);
                g.DrawString(i.ToString(), font, bbrush, x, geo.yadd + geo.ymul + geo.scaleTextPad, strformatPly);
            }

            // 評価値目盛りの描画
            using (var font = new Font(fontFamilyName, geo.scoreFontSize, GraphicsUnit.Pixel))
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
                        using (var pen = new Pen(color, ent.width))
                        {
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y0, geo.lPad + geo.xlen, y0);
                            if (ent.score != 0)
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y1, geo.lPad + geo.xlen, y1);
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
                        using (var pen = new Pen(color, 2))
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue - geo.scaleLineLen, y, geo.lPad + geo.hScrollValue, y);
                        using (var brush = new SolidBrush(color))
                            g.DrawString(ent.text, font, brush, geo.lPad + geo.hScrollValue - geo.scaleTextPad, y, strformatScore);
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
                        var vert = vertFunc((EvalValue)ent.score);
                        var color = DColor.Silver;
                        var y0 = -vert * geo.ymul + geo.yadd;
                        var y1 = +vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, ent.width))
                        {
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y0, geo.lPad + geo.xlen, y0);
                            if (ent.score != 0)
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y1, geo.lPad + geo.xlen, y1);
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
                        new { text = "+∞", score = (int)EvalValue.MatePlus },
                        new { text = "-200", score = -200 },
                        new { text = "-500", score = -500 },
                        new { text = "-1000", score = -1000 },
                        new { text = "-2000", score = -2000 },
                        new { text = "-9999", score = -9999 },
                        new { text = "-∞", score = (int)EvalValue.MatedMinus },
                    })
                    {
                        var vert = vertFunc((EvalValue)ent.score);
                        var color = Vert2Color(vert, evaldata.reverse);
                        var y = -vert * geo.ymul + geo.yadd;
                        using (var pen = new Pen(color, 2))
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue - geo.scaleLineLen, y, geo.lPad + geo.hScrollValue, y);
                        using (var brush = new SolidBrush(color))
                            g.DrawString(ent.text, font, brush, geo.lPad + geo.hScrollValue - geo.scaleTextPad, y, strformatScore);
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
                        using (var pen = new Pen(color, ent.width))
                        {
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y0, geo.lPad + geo.xlen, y0);
                            if (ent.vert != 0)
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue, y1, geo.lPad + geo.xlen, y1);
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
                        using (var pen = new Pen(color, 2))
                            g.DrawLine(pen, geo.lPad + geo.hScrollValue - geo.scaleLineLen, y, geo.lPad + geo.hScrollValue, y);
                        using (var brush = new SolidBrush(color))
                            g.DrawString(ent.text, font, brush, geo.lPad + geo.hScrollValue - geo.scaleTextPad, y, strformatScore);
                    }
                    break;
            }

            // 外枠線の描画
            using (var pen = new Pen(DColor.Silver, 3f))
            {
                g.DrawRectangle(pen, geo.lPad + geo.hScrollValue, geo.yadd - geo.ymul, geo.xlen - geo.hScrollValue, geo.ymul + geo.ymul);
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
                    var yp = vertFunc(data.values[ip]);
                    g.DrawLine(pen, ip * geo.plyWidth + geo.lPad, -yp * geo.ymul + geo.yadd, i * geo.plyWidth + geo.lPad, -y * geo.ymul + geo.yadd);
                }
            }

            if (evaldata.selectedIndex * geo.plyWidth >= geo.hScrollValue)
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
            Console.WriteLine($"EvalGraph Click: Xl{xl} Xr{xr} X{p.X} Y{p.Y} ply{ply}");

            // ToDo: 入力を受けた手数の局面に移動する
        }

        // マウス左ボタンドラッグイベント
        private void OnDrag(Point p0, Point p1)
        {
            var xl = geo.hScrollValue + geo.lPad;
            var xr = xl + geo.xlen;
            var ply0 = (p0.X - geo.lPad) / geo.plyWidth;
            var ply1 = (p1.X - geo.lPad) / geo.plyWidth;
            Console.WriteLine($"EvalGraph Drag: Xl{xl} Xr{xr} X0{p0.X} Y0{p0.Y} ply0{ply0} X1{p1.X} Y1{p1.Y} ply1{ply1}");

            // ToDo: 用途があれば…？ (グラフのスクロール？)
        }

        // マウス右クリックイベント
        private void OnRightClick(Point p)
        {
            var xl = geo.hScrollValue + geo.lPad;
            var xr = xl + geo.xlen;
            var ply = (p.X - geo.lPad) / geo.plyWidth;
            Console.WriteLine($"EvalGraph RightClick: Xl{xl} Xr{xr} X{p.X} Y{p.Y} ply{ply}");

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
