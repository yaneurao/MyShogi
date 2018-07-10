using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
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
            /// Settingのsetterでこのオブジェクトを生成して、Controlを動的に生成して
            /// データバインドする。
            /// </summary>
            public List<UsiOption> Options;
        }

        public EngineOptionSettingViewModel ViewModel = new EngineOptionSettingViewModel();

        private void InitViewModel()
        {
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

            foreach (var desc in setting.Descriptions)
            {
                var name = desc.Name;
                var e = setting.Options.Find(x => x.Name == name);
                if (e == null)
                    continue;

                UsiOption usiOption;
                try
                {
                    // parse出来なければ無視しておく。
                    usiOption = UsiOption.Parse(e.BuildString);
                }
                catch
                {
                    continue;
                }

                Control control = null;
                switch (usiOption.OptionType)
                {
                    case UsiOptionType.CheckBox:
                        control = new CheckBox();
                        break;

                    case UsiOptionType.SpinBox:
                        control = new NumericUpDown();
                        break;
                }
                if (control != null)
                {
                    var label1 = new Label();
                    label1.Location = new Point(3, y);
                    label1.AutoSize = true;
                    label1.Text = desc.DisplayName;
                    Controls.Add(label1);

                    var label2 = new Label();
                    label2.Location = new Point(400, y);
                    label2.AutoSize = true;
                    label2.Text = desc.Description;
                    Controls.Add(label2);

                    control.Location = new Point(200, y);
                    Controls.Add(control);

                    y += control.Height + 3;
                }
            }

        }

    }
}
