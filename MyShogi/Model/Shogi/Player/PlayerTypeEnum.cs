namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// プレイヤーの種類を表す定数。
    /// 人間も思考エンジンも同一のインターフェースで取り扱う。
    /// </summary>
    public enum PlayerTypeEnum
    {
        /// <summary>
        /// ダミーエンジンです。
        /// </summary>
        Null,

        /// <summary>
        /// 人間が代わりに操作します。
        /// </summary>
        Human,
        
        /// <summary>
        /// USIプロトコルでやりとりするエンジン。
        /// </summary>
        UsiEngine,
    }
}
