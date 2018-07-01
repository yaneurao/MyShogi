using System.Windows.Forms;
using MyShogi.Model.Shogi.LocalServer;

namespace MyShogi.View.Win2D
{
    public partial class MiniShogiBoard : UserControl
    {
        /// <summary>
        /// 継ぎ盤用のControl
        /// </summary>
        public MiniShogiBoard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 盤面表示のために持っているGameScreenControlの初期化を行う。
        /// </summary>
        public void Init()
        {
            // GameScreenControlの初期化
            var setting = new GameScreenControlSetting()
            {
                SetButton = null,
                gameServer = new LocalGameServer(),
                UpdateMenuItems = null,
            };
            gameScreenControl1.Setting = setting;
            gameScreenControl1.Init();
        }

#if false
        public new void Dispose()
        {
            var gameServer = gameScreenControl1.gameServer;
            if (gameServer != null)
            {
                gameServer.Dispose();
            }
        }
#endif

        public GameScreenControlSetting Settings { get { return gameScreenControl1.Setting; } }
    }
}
