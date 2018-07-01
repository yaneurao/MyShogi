using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class EngineConsiderationDialog : Form
    {
        public EngineConsiderationDialog()
        {
            InitializeComponent();

            InitSpliter();
            SetEngineInstanceNumber(1);
        }

        /// <summary>
        /// 基本的なレイアウト設定にする。
        /// </summary>
        private void InitSpliter()
        {
            var h = splitContainer1.Height;
            var sh = splitContainer1.SplitterWidth;
            splitContainer1.SplitterDistance = (h - sh) / 2; // ちょうど真ん中に
        }

        /// <summary>
        /// 読み筋を表示するコントロールのinstanceを返す。
        /// </summary>
        /// <param name="n">
        /// 
        /// n = 0 : 先手用
        /// n = 1 : 後手用
        /// 
        /// ただし、SetEngineInstanceNumber(1)と設定されていれば、
        /// 表示されているのは1つのみであり、先手用のほうしかない。
        /// 
        /// </param>
        /// <returns></returns>
        public EngineConsiderationControl GetConsiderationInstance(int n)
        {
            switch (n)
            {
                case 0: return engineConsiderationControl1;
                case 1: return engineConsiderationControl2;
            }
            return null;
        }

        /// <summary>
        /// エンジンのインスタンス数を設定する。
        /// この数だけエンジンの読み筋を表示する。
        /// </summary>
        /// <param name="n"></param>
        public void SetEngineInstanceNumber(int n)
        {
            if (n == 1)
            {
                splitContainer1.Panel2.Visible = false;
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.IsSplitterFixed = true;
            }
            else if (n == 2)
            {
                splitContainer1.Panel2.Visible = true;
                splitContainer1.Panel2Collapsed = false;
                splitContainer1.IsSplitterFixed = false;
            }
        }

        /// <summary>
        /// ミニ盤面の初期化
        /// </summary>
        public void Init()
        {
            miniShogiBoard1.Init();
            miniShogiBoard1.Settings.gameServer.Start();
        }
    }
}
