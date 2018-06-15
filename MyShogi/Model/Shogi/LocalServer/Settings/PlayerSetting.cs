namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 各プレイヤーごとの対局設定
    /// LocalGameServer.GameStart()の引数に渡す、対局条件などを一式書いた設定データの片側のプレイヤー分。
    /// </summary>
    public class PlayerSetting
    {
        public PlayerSetting()
        {
            IsHuman = true;
            IsCpu = false;
        }

        /// <summary>
        /// このインスタンスのClone()
        /// </summary>
        /// <returns></returns>
        public PlayerSetting Clone()
        {
            return (PlayerSetting)this.MemberwiseClone();
        }

        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string PlayerName;

        /// <summary>
        /// 対局相手は人間か？
        /// </summary>
        public bool IsHuman;

        /// <summary>
        /// 対局相手はコンピュータか？
        /// </summary>
        public bool IsCpu;

    }
}
