using MyShogi.Model.Shogi.LocalServer;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局画面を表現するクラス
    /// 
    /// 描画が完全に抽象化されているので、
    /// 一つのMainDialogが複数のGameScreenを持つことが出来る。
    /// </summary>
    public partial class GameScreenControl
    {
        /// <summary>
        /// 描画設定など一式
        /// 親クラスから初期化の時に設定される。
        /// </summary>
        public GameScreenControlSetting Setting { get; set; }

        /// <summary>
        /// 駒台のバージョン
        /// 
        /// このGameScreenのアスペクト比により、(横幅を狭めると)自動的に2が選ばれる。
        /// 
        /// 0 : 通常の駒台
        /// 1 : 細長い駒台
        /// 
        /// </summary>
        public int PieceTableVersion = 0;

        /// <summary>
        /// この対局盤面の描画のために必要となるViewModel
        /// 別クラスになってはいるが、GameScreenと1:1で対応するので、GameScreenの生成と同時に生成している。
        /// </summary>
        public GameScreenControlViewModel ViewModel { get; private set; } = new GameScreenControlViewModel();

        /// <summary>
        /// 関連付けられているLocalGameServerのインスタンスを返す。
        /// これは外部からSettingにセットされている。
        /// </summary>
        public LocalGameServer gameServer { get {
                if (Setting == null)
                    return null;// まだセットされていない。
                return Setting.gameServer; }
        }

        /// <summary>
        /// 関連付けられているKifuControlのインスタンスを返す。
        /// </summary>
        public KifuControl kifuControl {  get { return kifuControl1; } }

    }
}
