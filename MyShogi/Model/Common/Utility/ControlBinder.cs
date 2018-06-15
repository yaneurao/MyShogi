using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Bindしたものをすべて解除する。
        /// </summary>
        public void UnbindAll()
        {
            for (int i = 0; i < list.Count; ++i)
            {
                var o = list[i].Item1;
                var h = list[i].Item2;

                if (o is ComboBox)
                    (o as ComboBox).SelectedIndexChanged -= h;
                else if (o is NumericUpDown)
                    (o as NumericUpDown).ValueChanged -= h;
                else if (o is TextBox)
                    (o as TextBox).TextChanged -= h;
                else if (o is Button)
                    (o as Button).TextChanged -= h;
                else if (o is CheckBox)
                    (o as CheckBox).CheckedChanged -= h;
                else if (o is RadioButton)
                    (o as RadioButton).CheckedChanged -= h;
                else
                    throw new Exception("型判定に失敗");
            }
            list.Clear();
        }

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
            var h = new EventHandler ((sender, args) => { setter((int)(sender as ComboBox).SelectedIndex); });
            c.SelectedIndexChanged += h;
            AddHandler(c, h);
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
            var h = new EventHandler((sender, args) => { setter((int)c.Value); });
            c.ValueChanged += h;
            AddHandler(c, h);
        }

        public void Bind(string v, TextBox c, Action<string> setter)
        {
            // -- 最初、値をControlに設定しておく。

            c.Text = v;
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var h = new EventHandler((sender, args) => { setter(c.Text); });
            c.TextChanged += h;
            AddHandler(c, h);
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
            var h = new EventHandler((sender, args) => { setter(c.Text == conv(true)); });
            c.TextChanged += h;
            AddHandler(c, h);
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
            var h = new EventHandler((sender, args) => { setter(c.Text == conv(true)); });
            c.TextChanged += h;
            AddHandler(c, h);
        }

        public void Bind(bool v, CheckBox c, Action<bool> setter)
        {
            // -- 最初、値をControlに設定しておく。

            c.Checked = v;
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var h = new EventHandler((sender, args) => { setter(c.Checked); });
            c.CheckedChanged += h;
            AddHandler(c, h);
        }

        public void Bind(bool v, RadioButton c, Action<bool> setter)
        {
            // -- 最初、値をControlに設定しておく。

            c.Checked = v;
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var h = new EventHandler((sender, args) => { setter(c.Checked); });
            c.CheckedChanged += h;
            AddHandler(c, h);
        }

        /// <summary>
        /// あとでUnbindAll()出来るように、このクラスのlistに追加しておく。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="h"></param>
        private void AddHandler(Control c,EventHandler h)
        {
            // hの中で何か追加でセットしているかも知れないので現在のvの値にしたがって
            // このハンドラを1度呼び出しておく必要がある。
            h(c, null);

            // 保存しておき、UnbindAll()で用いる
            list.Add(new Tuple<Control, EventHandler>(c, h));
        }

        /// <summary>
        /// Bind()したものを保存している。
        /// UnbindAll()で必要になる。
        /// </summary>
        private List<Tuple<Control , EventHandler>> list = new List<Tuple<Control , EventHandler>>();
    }
}
