using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MyShogi.Model.Common.ObjectModel
{
    /// <summary>
    /// NotifyObjectによるproperty実装と、Windows.Form.Controlと紐付ける。
    /// 
    ///  var binder = new ControlBinder();
    ///  binder.bind(notifyObject , "HogeMessage" , textBox1 , DataBindWay.TwoWay);
    /// のように使う。
    /// </summary>
    public class ControlBinder : IDisposable
    {
        /// <summary>
        /// Bindしたものをすべて解除する。
        /// </summary>
        public void UnbindAll()
        {
            foreach (var e in list)
            {
                if (e.h1 != null)
                    e.notify.RemovePropertyChangedHandler(e.name, e.h1);

                var o = e.control;
                var h2 = e.h2;

                if (h2 != null)
                {
                    if (o is ComboBox)
                        (o as ComboBox).SelectedIndexChanged -= h2;
                    else if (o is NumericUpDown)
                        (o as NumericUpDown).ValueChanged -= h2;
                    else if (o is TextBox)
                        (o as TextBox).TextChanged -= h2;
                    else if (o is Button)
                        (o as Button).TextChanged -= h2;
                    else if (o is CheckBox)
                        (o as CheckBox).CheckedChanged -= h2;
                    else if (o is RadioButton)
                        (o as RadioButton).CheckedChanged -= h2;
                    else
                        throw new Exception("型判定に失敗");
                }
            }
            list.Clear();
        }

        /// <summary>
        /// 内部的にUnbindAll()を呼び出している。
        /// </summary>
        public void Dispose()
        {
            UnbindAll();
        }

        /// <summary>
        /// ComboBoxとbindする。
        /// propertyの型はint。
        /// name : NotifyObjectで実装されたプロパティ名
        /// </summary>
        /// <param name=""></param>
        /// <param name="c"></param>
        /// <param name="setter"></param>
        public void Bind( NotifyObject notify , string name , ComboBox c , Action<int> f = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                // ComboBoxのitemがないこともあるので注意。
                var v = (int)args.value;
                if (control.Items.Count != 0)
                {
                    if (control.Items.Count <= v)
                        v = 0;
                    if (v < 0)
                        v = 0;
                    control.SelectedIndex = v;
                }

                // おまけハンドラがあるなら呼び出す。
                if (f!=null)
                    f(v);
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => {
                    //Console.WriteLine($"{name} = {(int)control.SelectedIndex}");
                    notify.SetValue<int>(name, (int)control.SelectedIndex);
                });
                c.SelectedIndexChanged += h2;
            }

            AddHandler<int>(notify , name, h1 , control, h2);
        }

        public void Bind(NotifyObject notify, string name, NumericUpDown c, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                var v = (int)args.value;
                if (c.Maximum < v)
                    v = (int)c.Maximum;
                if (c.Minimum > v)
                    v = (int)c.Minimum;
                control.Value = v;
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<int>(name, (int)control.Value); });
                c.ValueChanged += h2;
            }

            AddHandler<int>(notify, name, h1, control, h2);
        }

        // Bind()のInt64とBindする版。
        public void Bind64(NotifyObject notify, string name, NumericUpDown c, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                var v = (Int64)args.value;
                if (c.Maximum < v)
                    v = (Int64)c.Maximum;
                if (c.Minimum > v)
                    v = (Int64)c.Minimum;
                control.Value = v;
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<Int64>(name, (Int64)control.Value); });
                c.ValueChanged += h2;
            }

            AddHandler<Int64>(notify, name, h1, control, h2);
        }

        public void Bind(NotifyObject notify, string name, TextBox c, Action<string> f = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                var v = (string)args.value;
                control.Text = v;

                if (f!=null)
                    f(v);
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<string>(name, control.Text); });
                c.TextChanged += h2;
            }

            AddHandler<string>(notify, name, h1, control, h2);
        }

        /// <summary>
        /// convを通してboolをstringに置き換えてからTextBoxに設定する。
        /// </summary>
        public void Bind(NotifyObject notify, string name, TextBox c, Func<bool, string> conv , DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                var v = conv((bool)args.value);
                control.Text = v;
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<bool>(name, control.Text == conv(true)); });
                c.TextChanged += h2;
            }

            AddHandler<bool>(notify, name, h1, control, h2);
        }

        /// <summary>
        /// convを通してboolをstringに置き換えてからButtonに設定する。
        /// </summary>
        public void Bind(NotifyObject notify, string name, Button c , Func<bool, string> conv , DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                var v = conv((bool)args.value);
                control.Text = v;
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<bool>(name, control.Text == conv(true) ); });
                c.TextChanged += h2;
            }

            AddHandler<bool>(notify, name, h1, control, h2);
        }


        public void Bind(NotifyObject notify, string name, CheckBox c, Action<bool> f = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                var v = (bool)args.value;
                control.Checked = v;
                if (f != null)
                    f(v);
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<bool>(name, control.Checked); });
                c.CheckedChanged += h2;
            }

            AddHandler<bool>(notify, name, h1, control, h2);
        }

        /// <summary>
        /// CheckBoxとint型の値をbindする
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="name"></param>
        /// <param name="c"></param>
        /// <param name="f"></param>
        /// <param name="way"></param>
        public void BindToInt(NotifyObject notify, string name, CheckBox c, Action<bool> f = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                var v = ((int)args.value ) != 0;
                control.Checked = v;
                if (f != null)
                    f(v);
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<int>(name, control.Checked ? 1 : 0); });
                c.CheckedChanged += h2;
            }

            AddHandler<int>(notify, name, h1, control, h2);
        }


        public void Bind(NotifyObject notify, string name, RadioButton c, Action<bool> f = null , DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                // 値が範囲外なら補整してからセットする。
                var v = (bool)args.value;
                control.Checked = v;
                if (f != null)
                    f(v);
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<bool>(name, control.Checked); });
                c.CheckedChanged += h2;
            }

            AddHandler<bool>(notify, name, h1, control, h2);
        }

        /// <summary>
        /// nofityのnameがstring型の時にbindする。
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="name"></param>
        /// <param name="c"></param>
        /// <param name="way"></param>
        public void BindString(NotifyObject notify, string name, NumericUpDown c, Action f = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                var vs = (string)args.value;
                int v;
                if (!int.TryParse(vs, out v))
                    return;

                // 値が範囲外なら補整してからセットする。
                if (c.Maximum < v)
                    v = (int)c.Maximum;
                if (c.Minimum > v)
                    v = (int)c.Minimum;
                control.Value = v;

                f();
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<string>(name, control.Value.ToString()); });
                c.ValueChanged += h2;
            }

            AddHandler<string>(notify, name, h1, control, h2);
        }


        public void BindString(NotifyObject notify, string name, TextBox c, Action f = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // これはそのまま移譲しとけば良い。
            Bind(notify, name, c, (string s)=> { f(); }, way);
        }

        public void BindString(NotifyObject notify, string name, CheckBox c, Action f = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                var vs = (string)args.value;
                var v = vs == "true" ? true : false; // USI上、小文字しか許容していない。

                // 値が範囲外なら補整してからセットする。
                control.Checked = v;
                if (f != null)
                    f();
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => { notify.SetValue<string>(name, control.Checked ? "true" : "false"); });
                c.CheckedChanged += h2;
            }

            AddHandler<string>(notify, name, h1, control, h2);
        }

        public void BindString(NotifyObject notify, string name, ComboBox c, Action f = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                var vs = (string)args.value; // この項目を探して、それを選択する。

                if (control.Items.Count != 0)
                {
                    for(int i= 0; i< control.Items.Count;++i)
                    {
                        if (vs == control.Items[i].ToString())
                        {
                            control.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // おまけハンドラがあるなら呼び出す。
                if (f != null)
                    f();
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => {
                    notify.SetValue<string>(name, (string)control.SelectedItem);
                });
                c.SelectedIndexChanged += h2;
            }

            AddHandler<string>(notify, name, h1, control, h2);
        }

        /// <summary>
        /// dicで日本語化する。
        /// dic.Add("no_book","定跡なし");のようになっている。
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="name"></param>
        /// <param name="c"></param>
        /// <param name="f"></param>
        /// <param name="dic"></param>
        /// <param name="way"></param>
        public void BindStringWithDic(NotifyObject notify, string name, ComboBox c, Action f = null, Dictionary<string,string> dic = null, DataBindWay way = DataBindWay.TwoWay)
        {
            // 値が変更になった時にデータバインドしているほうに値を戻す。
            var control = c; // copy for capture

            var h1 = new PropertyChangedEventHandler((args) =>
            {
                var vs = (string)args.value; // この項目を探して、それを選択する。
                if (!dic.ContainsKey(vs))
                    return;
                vs = dic[vs]; // 辞書で変換

                if (control.Items.Count != 0)
                {
                    for (int i = 0; i < control.Items.Count; ++i)
                    {
                        if (vs == control.Items[i].ToString())
                        {
                            control.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // おまけハンドラがあるなら呼び出す。
                if (f != null)
                    f();
            });
            notify.AddPropertyChangedHandler(name, h1);

            EventHandler h2 = null;

            if (way == DataBindWay.TwoWay)
            {
                h2 = new EventHandler((sender, args) => {
                    var selected = control.SelectedItem.ToString();
                    // dicを使って逆変換してから、SetValueする。
                    foreach (var k in dic)
                        if (k.Value == selected)
                        {
                            notify.SetValue<string>(name, (string)k.Key);
                            break;
                        }
                });
                c.SelectedIndexChanged += h2;
            }

            AddHandler<string>(notify, name, h1, control, h2);
        }


        /// <summary>
        /// あとでUnbindAll()出来るように、このクラスのlistに追加しておく。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="h"></param>
        private void AddHandler<T>(NotifyObject notify , string name , PropertyChangedEventHandler h1 , Control control , EventHandler h2)
        {
            // hの中で何か追加でセットしているかも知れないので現在のvの値にしたがって
            // このハンドラを1度呼び出しておく必要がある。

            // 強制的にイベントを発生させたいが、まだpropertyの値が作られていない可能性があるので
            // GetValue()を強制的に呼び出して、値がないならNotifyObjectにそのproprertyを作らせて、
            // それをRaisePropertyChanged()に渡して、通知イベントを強制的に発生させる。
            notify.RaisePropertyChanged(name , notify.GetValue<T>(name));

            // 保存しておき、UnbindAll()で用いる
            list.Add(new BindData(notify , name, h1 , control , h2));
        }

        private class BindData
        {
            public BindData(NotifyObject notify_ , string name_, PropertyChangedEventHandler h1_ , Control control_, EventHandler h2_)
            {
                name = name_;
                control = control_;
                h1 = h1_;
                notify = notify_;
                h2 = h2_;
            }

            /// <summary>
            /// プロパティ名
            /// </summary>
            public string name;

            /// <summary>
            /// NotifyObject側に紐づけられているハンドラ
            /// </summary>
            public PropertyChangedEventHandler h1;

            /// <summary>
            /// h1が関連づけられているNotifyObject
            /// </summary>
            public NotifyObject notify;

            /// <summary>
            /// Control側に紐づけられているハンドラ
            /// </summary>
            public EventHandler h2;

            /// <summary>
            /// h2が紐づけられているControl
            /// </summary>
            public Control control;
        }

        /// <summary>
        /// Bind()したものを保存している。
        /// UnbindAll()で必要になる。
        /// </summary>
        private List<BindData> list = new List<BindData>();
    }
}
