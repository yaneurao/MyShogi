using System.Windows.Forms;
using MyShogi.App;

namespace MyShogi.View.Win2D.Setting
{
    public partial class DisplaySettingDialog : Form
    {
        /// <summary>
        /// 
        /// 表示設定ダイアログ
        /// 
        /// </summary>
        public DisplaySettingDialog()
        {
            InitializeComponent();

            InitViewModel();

            // フォントの変更。即時反映
            var fontSetter = new FontSetter(this, "SettingDialog");
            Disposed += (sender,args) => fontSetter.Dispose();
        }

        /*
        - 表示設定ダイアログのタブ切り替えたときにちらつくの何故？
          - DoubleBuffer trueになっているのに…。
          - RichSelectorとFontSelectorのこのプロパティもtrueに変更。
            - 少しだけマシになった。
          - TabControlのDoubleBufferは利かないらしい。
            - cf. TabControl（Page）上のコントロール再描画ちらつきを抑制したい : http://www.atmarkit.co.jp/bbs/phpBB/viewtopic.php?topic=39187&forum=7

            // 以下のコードでDoubleBufferが利くようになるようだが、他の部分に支障が出かねない。やめとく。
            // WindowsAPIの実装に不具合があるので、.NETではDoubleBufferを反映しない実装になっているのかな…。うむむ…。
        */

        private void InitViewModel()
        {
            var config = TheApp.app.Config;

            // RichSelectorに丸投げできるので、ここではBindの設定をするだけで良い。

            // -- 「盤面」のタブ

            // 段・筋の表示
            richSelector1.Bind(config, "BoardNumberImageVersion");

            // 盤面画像
            richSelector4.ViewModel.SelectionOffset = 1;
            richSelector4.Bind(config, "BoardImageVersion");

            // 盤の色味
            richSelector22.Bind(config, "BoardImageColorVersion");

            // 畳画像
            richSelector5.ViewModel.SelectionOffset = 1;
            richSelector5.Bind(config, "TatamiImageVersion");

            // 畳の色味
            richSelector23.Bind(config, "TatamiImageColorVersion");

            // -- 「駒」のタブ

            // 駒画像
            richSelector6.ViewModel.SelectionOffset = 1;
            richSelector6.Bind(config, "PieceImageVersion");

            // 成駒の色
            richSelector21.Bind(config, "PieceImageColorVersion");

            // 成駒の色
            richSelector7.Bind(config, "PromotePieceColorType");

            // 移動方角マーカー
            richSelector8.Bind(config, "PieceAttackImageVersion");

            // 駒を掴む表現
            richSelector18.Bind(config, "PickedMoveDisplayStyle");

            // -- 「升」のタブ

            // 最終手の移動元の升
            richSelector9.Bind(config, "LastMoveFromColorType");

            // 最終手の移動先の升
            richSelector10.Bind(config, "LastMoveToColorType");

            // 駒を掴んだ時の移動元の升
            richSelector11.Bind(config, "PickedMoveFromColorType");

            // 駒を掴んだ時の移動候補の升
            richSelector12.Bind(config, "PickedMoveToColorType");

            // -- 「ダイアログ」のタブ

            // 相手側の成り・不成のダイアログを反転させるか。
            richSelector24.Bind(config, "FlipWhitePromoteDialog");

            // -- 「手番」のタブ

            // 駒を掴んだ時の移動候補の升
            richSelector13.Bind(config, "TurnDisplay");

            // 対局者名の先頭の手番記号
            richSelector14.Bind(config, "DisplayNameTurnVersion");

            // -- 「棋譜」のタブ

            // 棋譜ウインドウの棋譜の記法
            richSelector2.ViewModel.WarningRestart = true;
            richSelector2.Bind(config, "KifuWindowKifuVersion");

            // 検討ウインドウの棋譜の記法
            richSelector3.ViewModel.WarningRestart = true;
            richSelector3.Bind(config, "ConsiderationWindowKifuVersion");

            // 棋譜ウインドウに総消費時間を表示するのか
            richSelector25.ViewModel.WarningRestart = true;
            richSelector25.Bind(config, "KifuWindowDisplayTotalTime");

            // 棋譜を開いた時に何手目の局面にするか
            richSelector19.Bind(config, "MovesWhenKifuOpen");

            // -- 「エフェクト」のタブ

            // 対局開始・終了エフェクト
            richSelector16.Bind(config, "EnableGameGreetingEffect");

            // 振り駒のエフェクト
            richSelector17.Bind(config, "EnablePieceTossEffect");

            // -- 「評価値」のタブ

            // 後手番のCPUの評価値をどちらから見た値にするか
            richSelector15.ViewModel.SelectionTypeIsBool = true;
            richSelector15.Bind(config, "NegateEvalWhenWhite");

            // 形勢を評価値のところに表示するか
            richSelector20.Bind(config, "DisplayEvalJudgement");

            // -- 「フォント」のタブ

            var font = TheApp.app.Config.FontManager;
            fontSelectionConrol1.Bind(font.MenuStrip            , "MenuStrip");
            fontSelectionConrol2.Bind(font.MainToolStrip        , "MainToolStrip");
            fontSelectionConrol3.Bind(font.MiniBoardTab         , "MiniBoardTab");
            fontSelectionConrol4.Bind(font.SubToolStrip         , "SubToolStrip");
            fontSelectionConrol5.Bind(font.MainWindow           , "MainWindow");
            fontSelectionConrol6.Bind(font.SettingDialog        , "SettingDialog");
            fontSelectionConrol7.Bind(font.MessageDialog        , "MessageDialog");
            fontSelectionConrol8.Bind(font.KifuWindow           , "KifuWindow");
            fontSelectionConrol9.Bind(font.ConsiderationWindow  , "ConsiderationWindow");
            fontSelectionConrol10.Bind(font.ToolTip             , "ToolTip");
            fontSelectionConrol11.Bind(font.DebugWindow         , "DebugWindow");
            fontSelectionConrol12.Bind(font.EvalGraphControl    , "EvalGraphControl");

        }

    }
}
