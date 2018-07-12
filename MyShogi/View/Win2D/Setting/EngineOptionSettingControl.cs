using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.Usi;

namespace MyShogi.View.Win2D.Setting
{
    public partial class EngineOptionSettingControl : UserControl
    {
        public EngineOptionSettingControl()
        {
            InitializeComponent();

            InitViewModel();
        }

        public class EngineOptionSettingViewModel : NotifyObject
        {
            /// <summary>
            /// この内容に従って描画がなされる。
            /// これのsetterでUsiOptionを生成して、
            /// 表示しているコントロールとdata bindする。
            /// </summary>
            public EngineOptionsForSetting Setting
            {
                get { return GetValue<EngineOptionsForSetting>("Setting"); }
                set { SetValue<EngineOptionsForSetting>("Setting", value); }
            }

            /// <summary>
            /// データバインドしている値が変わった時のイベントハンドラ。
            /// これをtrapして都度保存すると良いような。
            /// </summary>
            public object ValueChanged
            {
                get { return GetValue<object>("ValueChanged"); }
                set { SetValue<object>("ValueChanged", value); }
            }

            /// <summary>
            /// Settingのsetterでこのオブジェクトを生成して、Controlを動的に生成して
            /// データバインドする。
            /// </summary>
            public List<UsiOption> Options;
        }

        public EngineOptionSettingViewModel ViewModel = new EngineOptionSettingViewModel();

        /// <summary>
        /// label1,2,3はマーカー用に配置しているので、そのX座標。
        /// </summary>
        private int[] label_x = new int[3];

        private void InitViewModel()
        {
            // label1～3のX座標を保存しておき、これらは非表示にしておく。
            var labels = new[] { label1, label2, label3 };
            foreach(var i in All.Int(3))
            {
                var label = labels[i];
                label_x[i] = label.Location.X;
                label.Visible = false;
            }

            ViewModel.AddPropertyChangedHandler("Setting", (args) =>
            {
                UpdateUsiOptions((EngineOptionsForSetting)args.value);
            });
        }

        /// <summary>
        /// ViewModel.Settingが更新された時に、ViewModelのOptionsを更新する。
        /// </summary>
        /// <param name="setting"></param>
        private void UpdateUsiOptions(EngineOptionsForSetting setting)
        {
            // -- 順番にControlを生成して表示する。

            int y = 3; // 最初の行のY座標。

            // bindしている値が変化した時にイベントを生起する
            var valueChanged = new Action(() => ViewModel.RaisePropertyChanged("ValueChanged", null));

            if (setting.Descriptions == null)
            {
                // この時は仕方ないので、Optionsの内容そのまま出しておかないと仕方ないのでは…。
                var descriptions = new List<EngineOptionDescription>();

                foreach (var option in setting.Options)
                    descriptions.Add(new EngineOptionDescription(option.Name, option.Name, null, null));

                setting.Descriptions = descriptions;
            }

            foreach (var desc in setting.Descriptions)
            {
                var name = desc.Name;
                if (name == null)
                {
                    // 見出し項目のようであるな…。
                    var description = desc.Description;
                    var displayName = desc.DisplayName.TrimStart();
                    var h = new EventHandler((sender, args) =>
                    {
                        label4.Text = displayName + "とは？";
                        textBox1.Text = description;
                    });

                    var label1 = new Label();
                    label1.Location = new Point(label_x[0], y);
                    label1.AutoSize = true;
                    label1.Text = desc.DisplayName;
                    label1.Font = new Font(label1.Font.FontFamily, label1.Font.Size * 1.5f);
                    label1.MouseHover += h;
                    Controls.Add(label1);

                    y += label1.Height + 6;
                    continue;
                }

                var e = setting.Options.Find(x => x.Name == name);
                if (e == null)
                    continue;

                UsiOption usiOption;
                try
                {
                    // parse出来なければ無視しておく。
                    usiOption = UsiOption.Parse(e.BuildString);

                    // bindする予定の値がないなら、UsiOptionの生成文字列中の"default"の値を
                    // 持ってくる。初回起動時などはこの動作になる。
                    if (e.Value == null)
                        e.Value = usiOption.GetDefault();
                }
                catch
                {
                    continue;
                }

                Control control = null;

                switch (usiOption.OptionType)
                {
                    case UsiOptionType.CheckBox:
                        var checkbox = new CheckBox();

                        binder.BindString(e, "Value", checkbox, valueChanged );

                        control = checkbox;
                        break;

                    case UsiOptionType.SpinBox:
                        var num = new NumericUpDown();
                        num.Minimum = usiOption.MinValue;
                        num.Maximum = usiOption.MaxValue;
                        num.Value = usiOption.DefaultValue;
                        num.TextAlign = HorizontalAlignment.Center;

                        binder.BindString(e , "Value", num , valueChanged);

                        control = num;
                        break;

                    case UsiOptionType.ComboBox:
                        var combobox = new ComboBox();
                        combobox.DropDownStyle = ComboBoxStyle.DropDownList;

                        // comboboxは、readonlyにすると勝手に背景色が変わってしまう..
                        // combobox.BackColor = System.Drawing.Color.White;
                        // →　これでは変更できない…。

                        //combobox.FlatStyle = FlatStyle.Flat;
                        // →　これ、ComboBoxを配置しているところが白いと同化して見にくい。
                        // まあいいや…。

                        foreach (var s in usiOption.ComboList)
                            combobox.Items.Add(s);

                        binder.BindString(e, "Value", combobox, valueChanged);

                        control = combobox;
                        break;

                    case UsiOptionType.TextBox:
                        var textbox = new TextBox();

                        binder.BindString(e, "Value", textbox, valueChanged);

                        control = textbox;
                        break;
                }
                if (control != null)
                {
                    var description = desc.Description;
                    var displayName = desc.DisplayName.TrimStart();
                    var h = new EventHandler( (sender, args) =>
                    {
                        label4.Text = displayName + "の説明";
                        textBox1.Text = description;
                    });

                    var label1 = new Label();
                    label1.Location = new Point(label_x[0], y);
                    label1.AutoSize = true;
                    label1.Text = desc.DisplayName;
                    label1.MouseHover += h;
                    Controls.Add(label1);

                    var label2 = new Label();
                    label2.Location = new Point(label_x[2], y);
                    label2.AutoSize = true;
                    label2.Text = desc.DescriptionSimple;
                    label2.MouseHover += h;
                    Controls.Add(label2);

                    control.Location = new Point(label_x[1], y);
                    control.MouseHover += h;

                    control.Size = new Size(label_x[2] - label_x[1], control.Height);
                    Controls.Add(control);

                    y += control.Height + 4;
                }
            }
        }

        private void EngineOptionSettingControl_Resize(object sender, EventArgs e)
        {
            // button1,2は下にdockしているべきである。
            button1.Location = new Point(button1.Location.X, ClientSize.Height - button1.Height - 3);
            button2.Location = new Point(ClientSize.Width - button1.Location.X - button2.Width,
                ClientSize.Height - button1.Height - 3);

            // textBox1は、「前ページ」「次ページ」ボタンの上に位置しているべきである。
            textBox1.Location = new Point(textBox1.Location.X, button1.Location.Y - textBox1.Height - 3);
            textBox1.Size = new Size(ClientSize.Width, textBox1.Height);

            // label4はtextBox1に表示されているテキストの説明文である。
            // これをtextBox1の上にdockさせる。

            label4.Location = new Point(label4.Location.X, textBox1.Location.Y - label4.Height - 3);

        }

        /// <summary>
        /// 前ページボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 次ページボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

        }

        private ControlBinder binder = new ControlBinder();

    }
}
