using System.Windows.Forms;
using MyShogi.Model.Shogi.LocalServer;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局盤面などがあるメインウィンドゥ
    ///
    /// propertyだけここに集めてある。
    /// </summary>
    public partial class MainDialog : Form
    {

        /// <summary>
        /// activeなGameScreenControlに関連付けられてるLocalGameServerのインスタンスを返す。
        /// 現状、GameScreenControlは一つしかインスタンスを生成していないので、それがactiveである。
        /// </summary>
        public LocalGameServer gameServer { get { return gameScreenControl1.gameServer; } }

        /// <summary>
        /// activeなGameScreenControlに関連付けられているKifuControlのインスタンスを返す。
        /// 現状、GameScreenControlは一つしかインスタンスを生成していないので、それがactiveである。
        /// </summary>
        public KifuControl kifuControl { get { return gameScreenControl1.kifuControl; } }

        /// <summary>
        /// 棋譜ウインドウをフローティングモードで使っているとき用。
        /// </summary>
        public DockWindow kifuDockWindow { get; set; }

        // -- メニューが生成しうるダイアログ

        /// modal dialogとして表示するするものはコメントアウトした。

        /// <summary>
        /// 「やねうら王について」のダイアログ
        /// </summary>
        //public Form aboutDialog;

        /// <summary>
        /// 「通常対局」の設定ダイアログ
        /// </summary>
        //public Form gameSettingDialog;

        /// <summary>
        /// CPU infoを表示するダイアログ
        /// </summary>
        //public Form cpuInfoDialog;

        /// <summary>
        /// デバッグウィンドウ
        /// </summary>
        public Form debugDialog;

        /// <summary>
        /// ・検討エンジン設定ダイアログ
        /// ・詰将棋エンジン設定ダイアログ
        /// 共通。
        /// </summary>
        //public Form ConsiderationEngineSettingDialog;

        /// <summary>
        /// 検討ウインドウを埋めて使うための入れ物。
        /// エンジンの思考出力用。
        /// </summary>
        public DockWindow engineConsiderationDockWindow;

        /// <summary>
        /// これが検討ウインドウ本体。これを↑のに埋めて使う。
        /// </summary>
        public EngineConsiderationMainControl engineConsiderationMainControl;

#if false
        /// <summary>
        /// 評価値グラフの出力用
        /// </summary>
        public Info.EvalGraphDialog evalGraphDialog;
#endif

    }
}
