using System;

namespace MyShogi.Model.Shogi.Usi
{
    // 検討ダイアログにLocalGameServerが思考エンジンの読み筋などを出力する時の
    // メッセージングに必要なenumなど。

    /// <summary>
    /// エンジンが返したい情報
    /// </summary>
    public enum UsiEngineReportMessageType
    {
        /// <summary>
        /// 思考エンジンのインスタンス数。
        /// このとき number にインスタンス数が入れる。
        /// number = 0 or 1 or 2。0だと検討ウィンドウを非表示にする。
        /// これを設定した時に、検討ウィンドウの出力内容が初期化される。
        /// </summary>
        NumberOfInstance,

        /// <summary>
        /// 思考エンジンの名前を設定する。
        /// これが検討ウィンドウの左上のエンジン名のところに反映される。
        /// 
        /// number = 出力するインスタンス番号
        /// data = エンジンの名前(string)
        /// </summary>
        SetEngineName,

        /// <summary>
        /// 現在の思考モードを設定する。
        /// 
        /// data = ゲームモード(GameModeEnum)
        /// (検討モードなら、「候補手」のComboBoxを出すなどする)
        /// </summary>
        SetGameMode,

        /// <summary>
        /// rootのsfenを設定する。
        /// number = 出力するインスタンス番号
        /// data = sfen文字列(string)
        /// </summary>
        SetRootSfen,

        /// <summary>
        /// PV,NPSなどを出力する。
        /// number = 出力するインスタンス番号
        /// data = UsiThinkReport
        /// </summary>
        UsiThinkReport,

    }

    /// <summary>
    /// エンジンが読み筋などを返す時の構造体
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public class UsiThinkReportMessage
    {
        /// <summary>
        /// このUsiThinkReportMessageの通しナンバー。
        /// </summary>
        //public UInt64 Id;

        /// <summary>
        /// メッセージの種類
        /// </summary>
        public UsiEngineReportMessageType type;

        /// <summary>
        /// instance numberなど用の変数
        /// </summary>
        public int number;

        /// <summary>
        /// データ本体。
        /// </summary>
        public object data;
    }
}
