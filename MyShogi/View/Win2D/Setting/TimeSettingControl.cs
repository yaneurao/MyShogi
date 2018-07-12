using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Kifu;
using SCore = MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D.Setting
{
    public partial class TimeSettingControl : UserControl
    {
        public TimeSettingControl()
        {
            InitializeComponent();

            InitScreen();
            InitViewModel();

            Disposed += OnDisposed;
        }

        public class PlayerSettingViewModel : NotifyObject
        {
            /// <summary>
            /// 手番
            /// </summary>
            public SCore.Color Color
            {
                get { return GetValue<SCore.Color>("Color"); }
                set { SetValue("Color", value); }
            }

            /// <summary>
            /// 後手の個別設定を有効にするかのフラグ
            /// </summary>
            public bool WhiteEnable
            {
                get { return GetValue<bool>("WhiteEnable"); }
                set { SetValue("WhiteEnable", value); }
            }
        }
        public PlayerSettingViewModel ViewModel = new PlayerSettingViewModel();

        /// <summary>
        /// このControl上にあるcontrolとbindする。
        /// </summary>
        public void Bind(KifuTimeSetting player)
        {
            var n = new[] { numericUpDown1, numericUpDown2, numericUpDown3, numericUpDown4, numericUpDown5 };
            var r = new[] { radioButton1, radioButton2 };
            var check = checkBox1;
            var check_unlimit_time = checkBox2;
            var group =  groupBox1;

            binder.Bind(player, "Hour", n[0]);
            binder.Bind(player, "Minute", n[1]);
            binder.Bind(player, "Second", n[2]);
            binder.Bind(player, "Byoyomi", n[3]);
            binder.Bind(player, "IncTime", n[4]);

            // 秒読みのラジオボタンが選択されていれば、IncTimeのほうの設定はグレーアウト。
            binder.Bind(player, "ByoyomiEnable", r[0], (v) =>
            {
                if (v)
                {
                    n[3].Enabled = true;
                    n[4].Enabled = false;
                }
            });
            binder.Bind(player, "IncTimeEnable", r[1], (v) =>
            {
                if (v)
                {
                    n[3].Enabled = false;
                    n[4].Enabled = true;
                }
            });
            binder.Bind(player, "IgnoreTime", check);
            binder.Bind(player, "TimeLimitless", check_unlimit_time, (v) =>
            {
                // 時間無制限の時、GroupBox丸ごとDisableに。
                // ただし、自分のチェックボックスは除外。この除外は、コンストラクタでGroupから除外している。
                group.Enabled = !v;
            });
        }

        /// <summary>
        /// Bind()したものをすべて解除する。
        /// </summary>
        public void Unbind()
        {
            binder.UnbindAll();
        }

        // -- screen setting

        private void InitScreen()
        {
            // checkbox2がgroupbox1に属すると嫌だったのでgroupboxの外に配置しておいてあったので
            // それを移動させる。

            // checkBox1と同じyにしたいが、これはgroupBox1に属するのでgroupBox1相対の座標になっている。
                int y = groupBox1.Location.Y + checkBox1.Location.Y;
                checkBox2.Location = new Point(checkBox2.Location.X, y);
        }

        /// <summary>
        /// ViewModelのpropertyのhandlerの設定
        /// </summary>
        private void InitViewModel()
        {
            ViewModel.AddPropertyChangedHandler("WhiteEnable", (args) =>
             {
                 var whiteEnable = (bool)args.value;
                 var color = ViewModel.Color;

                 if (color == SCore.Color.BLACK)
                 {
                     groupBox1.Text = whiteEnable ? "時間設定[先手/下手]" : "時間設定";
                 }
                 else
                 {
                     groupBox1.Text = whiteEnable ? "時間設定[後手/上手]" : "時間設定";
                     // 後手用のboxは、WhiteEnable == falseのときは丸ごと無効。
                     groupBox1.Enabled = whiteEnable;
                 }

             });
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            // data-bindしていたものすべてを解除する。
            binder.UnbindAll();
        }

        // -- privates

        private ControlBinder binder = new ControlBinder();

    }
}
