using System.Windows.Forms;

namespace MyShogiUpdater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

#if false
            // 例) V100とV108との差分を生成する。
            PatchMaker.MakePatch("V100", "V108","V100toV108");
#endif

        }
    }
}
