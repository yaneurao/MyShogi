using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D.Common
{
    /// <summary>
    /// Visual Studioのデザイナをhigh dpi環境で使うと、本来のAutoScale = (96F,96F)から、 (192F,192F) のように
    /// その環境に合わせた値を設定するようになるが、こうなっていると座標系が2倍される。(ClientSizeなども2倍)
    ///
    /// ところが、ダイアログの生成時にWindows APIを用いて、この座標系のサイズでいったんウインドウを生成しようとするため、
    /// 現在の画面解像度以上のウインドウを生成できないというWindows APIの制約に引っかかるようで、現在の画面解像度にリサイズされてしまい、
    /// 想定しているサイズで生成されない。
    ///
    /// これを修正するため、AutoScaleの値および、子コントロールの値すべてを一つ一つ書き換えるのがこのヘルパークラスの役割である。
    /// この問題は、.NET Framework側のバグと言える気がするのだが…。
    /// </summary>
    public static class AutoScaleFixer
    {
        /// <summary>
        /// FormのInitializeComponet()のあとにこの関数を呼び出すこと。
        /// </summary>
        /// <param name="form"></param>
        public static void Init(Form form)
        {
            form.SuspendLayout();

            // 元のscalingと変更後のscaling
            var org_scale = form.AutoScaleDimensions;
            var new_scale = new SizeF(96F, 96F);

            // scale_x == scale_yのはずではあるが、一応分けておく。
            var scale_x = new_scale.Width / org_scale.Width;
            var scale_y = new_scale.Height / org_scale.Height;

            // 全体的に(すべての子Controlに対して)このScale変換を適用してやる。

            void ResizeControls(Control.ControlCollection controls)
            {
                if (controls != null)
                {
                    foreach (Control c in controls)
                    {
                        c.Location = new Point((int)(c.Location.X * scale_x), (int)(c.Location.Y * scale_y));
                        c.Size = new Size((int)(c.Size.Width * scale_x), (int)(c.Size.Height * scale_y));
                        c.Margin = new Padding((int)(c.Margin.Left * scale_x), (int)(c.Margin.Top * scale_y), (int)(c.Margin.Right * scale_x), (int)(c.Margin.Bottom * scale_y));

                        // 子コントロールの子コントロールにもこれを適用するために再帰的に処理しておく。
                        // →　コントロールには、AutoScaleDimensionsというプロパティがないため、ここに書いてある問題は起きないっぽい。ほんまかいな…。

                        //ResizeControls(c.Controls);
                    }
                }
            }
            ResizeControls(form.Controls);

            form.AutoScaleDimensions = new_scale;

            form.ResumeLayout();
        }
    }
}
