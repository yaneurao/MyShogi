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
        InTheGame ,                 // 対局中
        InTheBoardEdit ,            // 盤面編集中
        ConsiderationWithoutEngine, // 検討中(エンジンが動作していない)
        ConsiderationWithEngine,    // 検討中(エンジンが動作している)
    }

    public static class GameModeExtensions
    {
        /// <summary>
        /// 検討中であるか
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static bool IsConsideration(this GameModeEnum mode)
        {
            return mode == GameModeEnum.ConsiderationWithoutEngine || mode == GameModeEnum.ConsiderationWithEngine;
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
