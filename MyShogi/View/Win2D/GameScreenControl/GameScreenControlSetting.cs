using MyShogi.Model.Shogi.LocalServer;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// GameScreenControlの描画設定一式。
    /// 
    /// これを初期化時に渡して、この設定に従って描画される。
    /// </summary>
    public class GameScreenControlSetting
    {
        /// <summary>
        /// メインウィンドウに付随している上部のToolStripの
        /// ボタンのEnable/Disableを切り替えたい時のcallback用のデリゲート
        /// 
        /// nullのままにしておくと呼び出されない。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="enable"></param>
        public delegate void SetButtonHandler(ToolStripButtonEnum name, bool enable);

        public SetButtonHandler SetButton { get; set; }

        /// <summary>
        /// ゲームサーバー本体。外部で生成して渡す。
        /// </summary>
        public LocalGameServer gameServer { get; set; }
    }
}
