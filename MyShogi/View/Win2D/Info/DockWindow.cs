using System.Windows.Forms;
using MyShogi.Model.Common.ObjectModel;

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
        }

        public DockWindowViewModel ViewModel = new DockWindowViewModel();


        /// <summary>
        /// このWindowの上に乗っけるControlを設定する。
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(Control control)
        {
            ViewModel.Control = control;
            Controls.Add(control);
            oldDockStyle = control.Dock;
            control.Dock = DockStyle.Fill;
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

    }
}
