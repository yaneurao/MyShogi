using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class EngineConsiderationControl : UserControl
    {
        public EngineConsiderationControl()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            listView1.FullRowSelect = true;
            //listView1.GridLines = true;
            listView1.Sorting = SortOrder.None;
            listView1.View = System.Windows.Forms.View.Details;

            var depth = new ColumnHeader();
            depth.Text = "深さ";
            depth.Width = 50;

            var node = new ColumnHeader();
            node.Text = "node";
            node.Width = 100;

            var pv = new ColumnHeader();
            pv.Text = "読み筋";
            pv.Width = 150;

            var header = new[] { depth, node, pv };

            listView1.Columns.AddRange(header);
        }
    }
}
