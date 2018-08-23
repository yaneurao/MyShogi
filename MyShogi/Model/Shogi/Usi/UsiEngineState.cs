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
        /// (子プロセスの起動後)
        /// </summary>
        Connected,

        /// <summary>
        /// "usi"を送信して"usiok"がエンジンから送られて来た状態
        /// </summary>
        UsiOk,

        /// <summary>
        /// "isready"を送信して"readyok"がエンジンから送られて来た状態
        /// </summary>
        ReadyOk,

        /// <summary>
        /// "usinewgame"を送信して思考ができる状態
        /// ゲーム中なので局面のコマンドを送って思考させることが出来る。
        /// </summary>
        InTheGame,

        /// <summary>
        /// ゲーム終了。
        /// </summary>
        GameOver,

        // -- 以下、エラー状態

        /// <summary>
        /// 子プロセスの起動に失敗した。
        /// </summary>
        ConnectionFailed,

        /// <summary>
        /// Connectのあと"usi"を送信したが30秒経っても"usiok"が返ってこなかった。
        /// </summary>
        ConnectionTimeout ,

    }

    public static class UsiEngineStateExtensions
    {
        public static string ToString(this UsiEngineState state)
        {
            switch (state)
            {
                case UsiEngineState.Init:              return "Init";
                case UsiEngineState.Connected:         return "Connected";
                case UsiEngineState.UsiOk:             return "UsiOk";
                case UsiEngineState.ReadyOk:           return "ReadyOk";
                case UsiEngineState.InTheGame:         return "InTheGame";
                case UsiEngineState.ConnectionFailed:  return "ConnectionFailed";
                case UsiEngineState.ConnectionTimeout: return "ConnectionTimeout";
                default: return "";
            }
        }

    }

}
