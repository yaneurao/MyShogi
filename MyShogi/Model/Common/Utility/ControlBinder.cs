using System;
using System.Windows.Forms;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// WinFormsのControlに対して双方向でデータバインドするためのもの。
    /// 人力でbindしておく。
    /// 
    /// binder.Bind(ref value,control);みたいな感じで使いたいが、refで渡した引数をlambda内でcapture出来ない。
    /// cf. https://stackoverflow.com/questions/1365689/cannot-use-ref-or-out-parameter-in-lambda-expressions
    /// 
    /// 仕方ないのでBind()の3つ目の引数でsetterを渡すことにする。
    /// binder.Bind(value,control,v=>value=v );
    /// として呼ぶ。
    /// </summary>
    public class ControlBinder
    {
        public void Bind(int v, ComboBox c, Action<int> setter)
        {
            // 最初、値をControlに設定しておく。

            // 値が範囲外なら補整してからセットする。
            if (c.Items.Count <= v)
                v = 0;
            if (v < 0)
                v = 0;
            setter(v);

            c.SelectedIndex = v;
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            c.SelectedIndexChanged += (sender, args) => { setter((int)(sender as ComboBox).SelectedIndex); };
        }

        public void Bind(int v , NumericUpDown c , Action<int> setter)
        {
            // -- 最初、値をControlに設定しておく。

            // 値が範囲外なら補整してからセットする。
            if (c.Maximum < v)
                v = (int)c.Maximum;
            if (c.Minimum > v)
                v = (int)c.Minimum;
            setter(v);

            c.Value = v;
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            c.ValueChanged += (sender,args) => { setter((int) c.Value); };
        }

        public void Bind(string v, TextBox c, Action<string> setter)
        {
            // -- 最初、値をControlに設定しておく。

            c.Text = v;
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            c.TextChanged += (sender, args) => { setter(c.Text); };
        }

        /// <summary>
        /// convを通してboolをstringに置き換えてからTextBoxに設定する。
        /// </summary>
        /// <param name="v"></param>
        /// <param name="c"></param>
        /// <param name="setter"></param>
        /// <param name="conv"></param>
        public void Bind(bool v, TextBox c, Action<bool> setter , Func<bool,string> conv)
        {
            // -- 最初、値をControlに設定しておく。

            c.Text = conv(v);
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            c.TextChanged += (sender, args) => { setter(c.Text == conv(true)); };
        }

        /// <summary>
        /// convを通してboolをstringに置き換えてからButtonのTextに設定する。
        /// </summary>
        /// <param name="v"></param>
        /// <param name="c"></param>
        /// <param name="setter"></param>
        /// <param name="conv"></param>
        public void Bind(bool v, Button c, Action<bool> setter, Func<bool, string> conv)
        {
            // -- 最初、値をControlに設定しておく。

            c.Text = conv(v);
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            c.TextChanged += (sender, args) => { setter(c.Text == conv(true)); };
        }

        public void Bind(bool v, CheckBox c, Action<bool> setter)
        {
            // -- 最初、値をControlに設定しておく。

            c.Checked = v;
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            c.CheckedChanged += (sender, args) => { setter(c.Checked); };
        }

        public void Bind(bool v, RadioButton c, Action<bool> setter)
        {
            // -- 最初、値をControlに設定しておく。

            c.Checked = v;
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            c.CheckedChanged += (sender, args) => { setter(c.Checked); };
        }

    }
}
