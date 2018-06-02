using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Math
{
    /// <summary>
    /// Affine変換用の行列(ただし、いまのところ回転は含まない)
    /// </summary>
    public struct AffineMatrix
    {
        public Vector2D<double> Scale;
        public Vector2D<int> Offset;

        /// <summary>
        /// ScaleとOffsetを一括して設定する。
        /// </summary>
        /// <param name="scale_x"></param>
        /// <param name="scale_y"></param>
        /// <param name="offset_x"></param>
        /// <param name="offset_y"></param>
        public void SetMatrix(double scale_x,double scale_y,int offset_x,int offset_y)
        {
            Scale.SetValue(scale_x, scale_y);
            Offset.SetValue(offset_x, offset_y);
        }

        /// <summary>
        /// 与えられたpに対して、affine変換をしたものを返す。
        /// offsetの加算は行わない。Scaleの適用のみ。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Size AffineScale(Size s)
        {
            return new Size(
                (int)(s.Width  * Scale.X),
                (int)(s.Height * Scale.Y)
                );
        }

        /// <summary>
        /// 与えられたpに対して、affine変換をしたものを返す。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point Affine(Point p)
        {
            return new Point(
                (int)(p.X * Scale.X + Offset.X),
                (int)(p.Y * Scale.Y + Offset.Y)
                );
        }

        /// <summary>
        /// pをAffine()して、sにScaleを掛けたRectangleを返す
        /// </summary>
        /// <param name="p"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public Rectangle Affine(Point p , Size s)
        {
            return new Rectangle(Affine(p), AffineScale(s));
        }

        /// <summary>
        /// Affine()の逆変換
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point InverseAffine(Point p)
        {
            return new Point(
                (int)((p.X - Offset.X) / Scale.X),
                (int)((p.Y - Offset.Y) / Scale.Y)
                );
        }
    }

}
