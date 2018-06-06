namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// プレイヤーのインスタンスの生成
    /// </summary>
    public static class PlayerBuilder
    {
        /// <summary>
        /// Playerの生成
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Player Create(PlayerTypeEnum type)
        {
            switch (type)
            {
                case PlayerTypeEnum.Null:
                    return new NullPlayer();

                case PlayerTypeEnum.Human:
                    return new HumanPlayer();

                case PlayerTypeEnum.UsiEngine:
                    return new UsiEnginePlayer();

            }
            return null;
        }
    }
}
