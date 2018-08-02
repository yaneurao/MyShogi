namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// ゲームモードを持って状態を管理する。
    /// 
    /// これらは互いに排他されるべき。
    /// 
    /// </summary>
    public enum GameModeEnum
    {
        NotInit,                        // 初期状態。LocalGameServer.Start()で値を代入した時にGameModeの変更通知が飛ぶように。
        InTheGame ,                     // 対局中
        InTheBoardEdit ,                // 盤面編集中
        ConsiderationWithoutEngine,     // 検討中(エンジンが動作していない) : ユーザーが何もしていないならこの状態。
        ConsiderationWithEngine,        // 検討中(検討用エンジンが動作している)
        ConsiderationWithMateEngine,    // 検討中(詰将棋エンジンが動作している)
        ConsiderationWithMultiEngine,   // 検討中(複数の検討エンジン・詰将棋エンジンが動作している)
    }

    public static class GameModeExtensions
    {
        /// <summary>
        /// 検討中であるか(エンジンが動いていないケースも含めて)
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool IsConsideration(this GameModeEnum mode)
        {
            return 
                mode == GameModeEnum.ConsiderationWithoutEngine  ||
                mode == GameModeEnum.ConsiderationWithEngine     ||
                mode == GameModeEnum.ConsiderationWithMateEngine ||
                mode == GameModeEnum.ConsiderationWithMultiEngine;
        }

        /// <summary>
        /// 思考エンジンを用いた検討モードであるか
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool IsWithEngine(this GameModeEnum mode)
        {
            return
                mode == GameModeEnum.ConsiderationWithEngine ||
                mode == GameModeEnum.ConsiderationWithMateEngine ||
                mode == GameModeEnum.ConsiderationWithMultiEngine;
        }

        /// <summary>
        /// 人間が駒をUIで動かせるモードなのか
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool CanUserMove(this GameModeEnum mode)
        {
            return mode != GameModeEnum.InTheGame;
        }
    }
}
