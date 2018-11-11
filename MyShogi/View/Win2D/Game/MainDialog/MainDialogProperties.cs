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

        // -- DockWindow

        // 棋譜Control

        /// <summary>
        /// activeなGameScreenControlに関連付けられているKifuControlのインスタンスを返す。
        /// 現状、GameScreenControlは一つしかインスタンスを生成していないので、それがactiveである。
        /// </summary>
        public KifuControl kifuControl { get { return gameScreenControl1.kifuControl; } }

        /// <summary>
        /// 棋譜ウインドウをフローティングモードで使っているとき用。
        /// </summary>
        public DockWindow kifuDockWindow { get; set; }


        // 検討Control

        /// <summary>
        /// これが検討ウインドウ本体。
        /// この生成はMainDialogが行う。
        /// 
        /// これを↓のに埋めて使う。
        /// </summary>
        public EngineConsiderationMainControl engineConsiderationMainControl;

        /// <summary>
        /// 検討ウインドウを埋めて使うための入れ物。
        /// エンジンの思考出力用。
        /// </summary>
        public DockWindow engineConsiderationDockWindow;


        // ミニ盤面

        /// <summary>
        /// 検討ウインドウに埋まっているミニ盤面のControl。
        /// 検討ウインドウから外して、Dockして使うときもこのinstanceは有効。
        /// </summary>
        public MiniShogiBoard miniShogiBoard { get { return engineConsiderationMainControl.MiniShogiBoard; } }

        /// <summary>
        /// ミニ盤面を埋めて使うための入れ物。
        /// </summary>
        public DockWindow miniShogiBoardDockWindow;


#if false
        /// <summary>
        /// 評価値グラフの出力用
        /// </summary>
        public Info.EvalGraphDialog evalGraphDialog;
#endif

        // -- 単独ウインドウ

        /// <summary>
        /// デバッグウィンドウ
        /// </summary>
        public Form debugDialog;

    }
}
