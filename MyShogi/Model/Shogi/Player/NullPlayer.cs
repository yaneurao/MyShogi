using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// 何もしないプレイヤー
    /// </summary>
    public class NullPlayer : Player
    {
        public PlayerTypeEnum PlayerType
        {
            get { return PlayerTypeEnum.Null; }
        }

        public string Name
        {
            get { return "NullPlayer"; }
        }

        public string DisplayName
        {
            get { return Name; }
        }

        /// <summary>
        /// このプレイヤーが指した指し手
        /// </summary>
        public Move BestMove { get; set; }

        /// <summary>
        /// このプレイヤーのponderの指し手
        /// </summary>
        public Move PonderMove { get; set; }

        /// <summary>
        /// プレイヤーの手番であるか。
        /// これはLocalGameServerのほうから設定される。
        /// </summary>
        public bool CanMove { get; set; }

        public bool IsInit { get; } = false;

        public void OnIdle()
        {
        }

        public void Think(string usiPosition)
        {
            // 次の指し手。投了する
            BestMove = Move.RESIGN;
        }

    }
}
