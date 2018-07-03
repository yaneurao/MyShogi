namespace MyShogi.Model.Shogi.LocalServer
{
    // 検討ダイアログにLocalGameServerが思考エンジンの読み筋などを出力する時の
    // メッセージングに必要なenumなど。

    /// <summary>
    /// エンジンが返したい情報
    /// </summary>
    public enum EngineInfoType
    {
        /// <summary>
        /// 思考エンジンのインスタンス数。
        /// このとき number にインスタンス数が入れる。
        /// number = 0 or 1 or 2。0だと検討ウィンドウを非表示にする。
        /// これを設定した時に、検討ウィンドウの出力内容が初期化される。
        /// </summary>
        InstanceNumber,

        /// <summary>
        /// rootのsfenを設定する。
        /// number = 出力するインスタンス番号
        /// data = sfen文字列(string)
        /// </summary>
        SetRootSfen,

        /// <summary>
        /// NPSなどを出力する。
        /// number = 出力するインスタンス番号
        /// data = EngineConsiderationInfoData
        /// </summary>
        EngineConsiderationInfoData,

        /// <summary>
        /// PVを出力する。
        /// number = 出力するインスタンス番号
        /// data = EngineConsiderationPvData
        /// </summary>
        EngineConsiderationPvData,
    }

    /// <summary>
    /// エンジンが読み筋などを返す時の構造体
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public class EngineInfo
    {
        /// <summary>
        /// メッセージの種類
        /// </summary>
        public EngineInfoType type;

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
