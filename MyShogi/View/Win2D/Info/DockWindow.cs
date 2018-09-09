using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Tool;

namespace MyShogi.View.Win2D
{
    public partial class DockWindow : Form
    {
        /// <summary>
        /// 他のControlを埋め込んでフローティングモードで使うためのForm
        /// </summary>
        public DockWindow()
        {
            InitializeComponent();

            InitViewModel();

            // タスクバーでは非表示。
            ShowInTaskbar = false;
        }

        public class DockWindowViewModel : NotifyObject
        {
            /// <summary>
            /// これをdockして、fillにして使う。
            /// </summary>
            public Control Control { get; set; }

            /// <summary>
            /// このウインドウのCaption
            /// </summary>
            public string Caption
            {
                get { return GetValue<string>("Caption"); }
                set { SetValue("Caption", value); }
            }

            /// <summary>
            /// DockManager。
            /// このFormのresizeとmoveに対して、位置とサイズをここに記録する。
            /// これは、AddControl()のときに渡された引数の値がセットされる。
            /// </summary>
            public DockManager DockManager { get; set; }

            /// <summary>
            /// このDockWindowを移動させたときにMainForm相対で位置を計算しないといけないことがあるため、
            /// 追随するMainFormがセットされていなくてはならない。
            /// これは、AddControl()のときに渡された引数の値がセットされる。
            /// </summary>
            public Form MainForm { get; set; }

            /// <summary>
            /// Menuを更新して欲しいときに発生する仮想イベント。
            /// </summary>
            public object MenuUpdated { get; set; }
        }

        public DockWindowViewModel ViewModel = new DockWindowViewModel();


        /// <summary>
        /// このWindowの上に乗っけるControlを設定する。
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(Control control, Form mainForm, DockManager dockManager)
        {
            if (ViewModel.Control != control)
            {
                ViewModel.Control = control;
                Controls.Add(control);
                oldDockStyle = control.Dock;
                control.Dock = DockStyle.Fill;
                ViewModel.DockManager = dockManager;
                ViewModel.MainForm = mainForm;
            }
        }

        /// <summary>
        /// このWindowに乗っけていたControlを解除する。
        /// </summary>
        public void RemoveControl()
        {
            var control = ViewModel.Control;
            if (control != null)
            {
                ViewModel.Control = null;
                control.Dock = oldDockStyle;
                Controls.Remove(control);
                ViewModel.DockManager = null;
                ViewModel.MainForm = null;
            }
        }

        private void InitViewModel()
        {
            ViewModel.AddPropertyChangedHandler("Caption", (args) => { Text = (string)args.value; });
        }

        /// <summary>
        /// AddControl()したcontrolの元のDockStyle。
        /// これは、RemoveControl()で必要。
        /// </summary>
        private DockStyle oldDockStyle;

        /// <summary>
        /// ウインドウの位置・サイズが変更になったので、DockManagerで管理しているLocationなどを更新する。
        /// </summary>
        private void SaveWindowLocation()
        {
            var dockManager = ViewModel.DockManager;
            if (dockManager != null)
                dockManager.SaveWindowLocation(ViewModel.MainForm, this);
        }

        private void DockWindow_Resize(object sender, System.EventArgs e)
        {
            SaveWindowLocation();
        }

        private void DockWindow_Move(object sender, System.EventArgs e)
        {
            SaveWindowLocation();
        }

        private void DockWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
#if false
            if (!TheApp.app.Exiting)
            {
                // cancelして非表示にして隠しておく。
                e.Cancel = true;
                Visible = false;

                // 「棋譜ウインドウ」の再表示がメニュー上で選べるようになっていて欲しいので、
                // メニューの再描画を要求する。

                ViewModel.RaisePropertyChanged("MenuUpdated");
            }
#endif
            // →　Ownerを設定する場合において、closeをCancelするのは筋が良くない。

            if (ViewModel.Control != null)
            {
                // Dispose()が呼ばれるとたまらないのでremoveしておく。(親側で解体すべき)
                Controls.Remove(ViewModel.Control);
                ViewModel.Control = null;

                Visible = false; // これにしてから、Menuの更新をすれば、メニューの棋譜ウインドウの「再表示」が有効になる。
                ViewModel.RaisePropertyChanged("MenuUpdated");
            }
        }
    }
}
