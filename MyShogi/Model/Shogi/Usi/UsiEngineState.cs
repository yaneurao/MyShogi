namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// USIエンジンの状態を示します。(大雑把に)
    /// </summary>
    public enum UsiEngineState
    {
        /// <summary>
        /// 初期状態(初期化はまだ)
        /// </summary>
        Init,

        /// <summary>
        /// 接続済
        /// (子プロセスの起動に成功)
        /// </summary>
        Connected,

        /// <summary>
        /// usiok後の状態
        /// </summary>
        UsiOk,

        /// <summary>
        /// readyok後の状態
        /// </summary>
        ReadyOk,

        /// <summary>
        /// usinewgame後の状態
        /// ゲーム中なので局面のコマンドを送って思考させることが出来る。
        /// </summary>
        InTheGame,
    }
}
