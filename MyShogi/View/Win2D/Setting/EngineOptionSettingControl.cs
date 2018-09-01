using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.Usi;
using MyShogi.View.Win2D.Common;

namespace MyShogi.View.Win2D.Setting
{
    public partial class EngineOptionSettingControl : UserControl
    {
        public EngineOptionSettingControl()
        {
            InitializeComponent();

            InitViewModel();

            Disposed += OnDisposed;
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
        /// 動的に配置したコントロールをすべて削除
        /// </summary>
        private void ClearPages()
        {
            binder.UnbindAll();

            foreach (var page in pages)
                foreach (var c in page)
                    Controls.Remove(c);
            pages.Clear();
        }

        /// <summary>
        /// ViewModel.Settingが更新された時に、ViewModelのOptionsを更新する。
        /// </summary>
        /// <param name="setting"></param>
        private void UpdateUsiOptions(EngineOptionsForSetting setting)
        {
            Debug.Assert(setting != null && setting.Options != null && setting.Descriptions != null);

            var followCommonSettingDescription =
                "左側のチェックボックスの説明\r\n" +
                "このチェックボックスをオンにすると、この項目は共通設定に従います。\r\n" +
                "このチェックボックスをオフにすると、このエンジン用にこの項目の値を個別に設定できます。";

            // -- 順番にControlを生成して表示する。
            SuspendLayout();

            ClearPages();

            var page = new List<Control>();
            pages.Add(page);

            // これを基準に高さを調整していく。
            var hh = label1.Height;

            int y = hh/4; // 最初の行のY座標。

            // エンジン個別設定なら「共通設定に従う」チェックボックスを左端に追加するので全体的に少し右にずらして表示する。
            var indivisual = setting.IndivisualSetting;
            var x_offset = indivisual ? hh*2 : 0;

            // bindしている値が変化した時にイベントを生起する
            var valueChanged = new Action(() => ViewModel.RaisePropertyChanged("ValueChanged", null));

            for (var k = 0; k < setting.Descriptions.Count; ++k)
            {
                var desc = setting.Descriptions[k];

                if (desc.Hide)
                    continue;

                var name = desc.Name;
                if (name == null)
                {
                    // 見出し項目のようであるな…。

                    // 先頭要素でなければ、水平線必要だわ。
                    if (pages[pages.Count-1].Count != 0)
                    {
                        var label = new Label();
                        // 水平線なのでフォントは問題ではない
                        label.Font = new Font("ＭＳ ゴシック", 9);
                        label.Location = new Point(label_x[0] + x_offset, y);
                        label.Size = new Size(ClientSize.Width, 1);
                        label.BorderStyle = BorderStyle.FixedSingle;
                        y += hh/2;
                        Controls.Add(label);
                        page.Add(label);
                    }

                    var displayName = desc.DisplayName.TrimStart();

                    var description =
                        displayName + "の説明\r\n" +
                        (desc.Description == null ? "説明文がありません。" : desc.Description);

                    var label1 = new Label();
                    label1.Location = new Point(label_x[0] + x_offset, y);
                    label1.AutoSize = true;
                    label1.Text = desc.DisplayName;
                    label1.Font = new Font("ＭＳ ゴシック", 14 );
                    // Font()の第三引数に " GraphicsUnit.Pixel "をつけるとdpi設定の影響を受けない。
                    // ここでは、dpi設定の影響を受けないといけないので、指定しない。

                    toolTip1.SetToolTip(label1, description);

                    Controls.Add(label1);
                    page.Add(label1);

                    y += label1.Height + hh/2;
                    continue;
                }

                var e = setting.Options.Find(x => x.Name == name);
                if (e == null)
                    continue;

                UsiOption usiOption;
                try
                {
                    // parse出来なければ無視しておく。
                    usiOption = UsiOption.Parse(e.UsiBuildString);

                    // bindする予定の値がないなら、UsiOptionの生成文字列中の"default"の値を
                    // 持ってくる。初回起動時などはこの動作になる。
                    if (e.Value == null)
                        e.Value = usiOption.GetDefault();
                }
                catch
                {
                    // デバッグ用に、Parse()に失敗した文字列を出力してみる。
                    //Console.WriteLine(e.UsiBuildString);

                    continue;
                }

                Control control = null;
                var defaultText = usiOption.GetDefault();

                switch (usiOption.OptionType)
                {
                    case UsiOptionType.CheckBox:
                        var checkbox = new CheckBox();

                        binder.BindString(e, "Value", checkbox, valueChanged );

                        control = checkbox;
                        defaultText = usiOption.DefaultBool ? "オン(true)" : "オフ(false)";
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

                        if (desc.ComboboxDisplayName == null)
                        {
                            foreach (var s in usiOption.ComboList)
                                combobox.Items.Add(s);

                            binder.BindString(e, "Value", combobox, valueChanged);
                        } else
                        {
                            // ComboBoxの日本語化
                            var split = desc.ComboboxDisplayName.Split(',');
                            var dic = new Dictionary<string, string>();
                            for (int i = 0; i < split.Length / 2; ++i)
                                dic.Add(split[i * 2 + 0], split[i * 2 + 1]);

                            foreach (var s in usiOption.ComboList)
                            {
                                if (dic.ContainsKey(s))
                                    combobox.Items.Add(dic[s]);

                                // デフォルト値もdicを経由して表示名に変換しておいてやる。
                                if (dic.ContainsKey(defaultText))
                                    defaultText = dic[defaultText];
                            }

                            binder.BindStringWithDic(e, "Value", combobox, valueChanged , dic);
                        }

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
                    var displayName = desc.DisplayName == null ? desc.Name : desc.DisplayName;
                    var description =
                        displayName + "の説明\r\n"+
                        (desc.Description == null ? "説明文がありません。" : desc.Description) +
                        $"\r\nデフォルト値 = {defaultText}";

                    var label1 = new Label();
                    label1.Font = new Font("ＭＳ ゴシック", 10);
                    label1.Location = new Point(label_x[0] + x_offset, y);
                    label1.AutoSize = true;
                    label1.Text = displayName.LeftUnicode(18); // GPS将棋とか長すぎるオプション名がある。
                    toolTip1.SetToolTip(label1, description);

                    Controls.Add(label1);
                    page.Add(label1);

                    var label2 = new Label();
                    label2.Font = new Font("ＭＳ ゴシック", 10);
                    label2.Location = new Point(label_x[2] + x_offset  + hh /* 配置したcontrolから少し右に配置 */ , y);
                    label2.AutoSize = true;
                    label2.Text = desc.DescriptionSimple;
                    toolTip1.SetToolTip(label2, description);

                    Controls.Add(label2);
                    page.Add(label2);

                    control.Location = new Point(label_x[1] + x_offset , y);
                    toolTip1.SetToolTip(control, description);

                    control.Font = new Font("ＭＳ ゴシック", 9);

                    control.Size = new Size(label_x[2] - label_x[1] , control.Height);
                    Controls.Add(control);
                    page.Add(control);

                    if (indivisual && e.EnableFollowCommonSetting)
                    {
                        // エンジン個別設定なので左端に「共通設定に従う」のチェックボックスを配置する。

                        var checkbox = new CheckBox();
                        checkbox.Location = new Point(label_x[0], y);
                        checkbox.Size = new Size(x_offset, control.Height);
                        //checkbox.MouseHover += followCheckboxHover;
                        toolTip1.SetToolTip(checkbox, followCommonSettingDescription);

                        Controls.Add(checkbox);
                        page.Add(checkbox);

                        binder.Bind(e, "FollowCommonSetting", checkbox, v => {
                            valueChanged();
                            // 連動してlabel1,2,controlを無効化 
                            label1.Enabled = !v;
                            label2.Enabled = !v;
                            control.Enabled = !v;
                        });

                        // ラベルをクリックしても左端のチェックボックスのオンオフが出来る。
                        //label1.Click += (sender,args)=> { checkbox.Checked ^= true; };
                        // …ようにしようと思ったら、label1がEnable == falseになってしまうので
                        // このクリックイベントが発生しなくなるのか…。うーむ、、そうか…。
                    }

                    y += control.Height + hh/3;
                }

                // 次の項目が見出し項目であるか
                var nextIsHeader = k+1 < setting.Descriptions.Count && !setting.Descriptions[k+1].Hide && setting.Descriptions[k+1].Name == null;
                if ((y >= button1.Location.Y - button1.Height * 2.5)
                    || (nextIsHeader && y >= button1.Location.Y - button1.Height * 4.5) /* 次が見出し項目なら早めに次ページに改ページする */
                    )
                {
                    // 次ページに

                    page = new List<Control>();
                    pages.Add(page);
                    y = hh/4;
                }
            }

            // 最後のページから空のページでありうるので、その場合、最後のページを削除する。
            if (pages.Count != 0 && pages[pages.Count - 1].Count == 0)
                pages.RemoveAt(pages.Count - 1);

            ResumeLayout();

            UpdatePage(0);
        }

        private void EngineOptionSettingControl_Resize(object sender, EventArgs e)
        {
            // button1,2は下にdockしているべきである。
            button1.Location = new Point(button1.Location.X, ClientSize.Height - button1.Height - 3);
            button2.Location = new Point(ClientSize.Width - button1.Location.X - button2.Width,
                ClientSize.Height - button1.Height - 3);

            // textBox1は、「前ページ」「次ページ」ボタンの上に位置しているべきである。
            //textBox1.Location = new Point(textBox1.Location.X, button1.Location.Y - textBox1.Height - 3);
            //textBox1.Size = new Size(ClientSize.Width, textBox1.Height);

            // label4はtextBox1に表示されているテキストの説明文である。
            // これをtextBox1の上にdockさせる。

            //label4.Location = new Point(label4.Location.X, textBox1.Location.Y - label4.Height - 3);

            // -- タブ内に隠れているほうは、Resizeイベントが発生していなくて、label4が移動していない。
            // Resize()に対して、このメソッドが呼び出されるべき。

            if (ViewModel.Setting != null)
                UpdateUsiOptions(ViewModel.Setting);
        }

        /// <summary>
        /// ページめくり
        /// 
        /// page_noのページ以外に属するControlはすべて非表示にする。
        /// </summary>
        /// <param name="page_no"></param>
        private void UpdatePage(int page_no)
        {
            // 範囲外。この条件で呼び出されることはないはずなのだが…。
            if (page_no < 0 || page_no >= pages.Count)
                return;

            SuspendLayout();

            foreach(var page in All.Int(pages.Count))
            {
                var visible = page == page_no;
                foreach (var c in pages[page])
                    c.Visible = visible;
            }

            ResumeLayout();

            // 前ページ、次ページに行けるかどうか。

            // 行けないときはボタン自体を非表示にしておく。
            // 途中で使っているComboBoxがReadOnlyなので背景色がグレーであり、
            // ボタンがグレーであっても押せるように見えてしまうため。

            button1.Visible = page_no - 1 >= 0;
            button2.Visible = page_no + 1 < pages.Count;

            currentPage = page_no;
        }

        /// <summary>
        /// 前ページボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            UpdatePage(currentPage - 1);
        }

        /// <summary>
        /// 次ページボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            UpdatePage(currentPage + 1);
        }

        /// <summary>
        /// マウスの移動イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EngineOptionSettingControl_MouseMove(object sender, MouseEventArgs e)
        {
            toolTipHelper.OnMouseMove(this, this.toolTip1, e.Location);
        }
        private ToolTipHelper toolTipHelper = new ToolTipHelper();

        private void OnDisposed(object sender, EventArgs e)
        {
            // data-bindしていたものすべてを解除する。
            binder.UnbindAll();
        }

        private ControlBinder binder = new ControlBinder();

        /// <summary>
        /// 各ページに表示すべきControl
        /// </summary>
        private List<List<Control>> pages = new List<List<Control>>();

        /// <summary>
        /// 現在表示しているページ番号
        /// </summary>
        private int currentPage = 0;

    }
}
