namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// プレイヤーを抽象化したインターフェース
    /// 指し手を返す。
    /// </summary>
    public interface Player
    {
        /// <summary>
        /// 対局者の種別
        /// </summary>
        PlayerTypeEnum PlayerType { get; }

        /// <summary>
        /// 対局者名
        /// 
        /// ※　UsiEnginePlayerの場合、エンジンから渡された名前
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 対局者名(これが画面上に表示する名前として使われる)
        /// </summary>
        string DisplayName { get; }
    }
}

