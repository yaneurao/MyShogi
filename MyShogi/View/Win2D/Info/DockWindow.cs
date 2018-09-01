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

        public void AddControl(Control control)
        {
            ViewModel.Control = control;
            Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        public void RemoveControl()
        {
            var control = ViewModel.Control;
            if (control != null)
            {
                ViewModel.Control = null;
                Controls.Remove(control);
            }
        }

        private void InitViewModel()
        {
            ViewModel.AddPropertyChangedHandler("Caption", (args) => { Text = (string)args.value; });
        }

    }
}
