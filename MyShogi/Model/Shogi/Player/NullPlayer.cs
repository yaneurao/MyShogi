using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Usi;

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
        public Move BestMove { get { return Move.NONE; } }

        /// <summary>
        /// TIME_UPなどが積まれる。BestMoveより優先して解釈される。
        /// </summary>
        public Move SpecialMove { get; set; }

        /// <summary>
        /// このプレイヤーのponderの指し手
        /// </summary>
        public Move PonderMove { get { return Move.NONE; } }

        /// <summary>
        /// プレイヤーの手番であるか。
        /// これはLocalGameServerのほうから設定される。
        /// </summary>
        public bool CanMove { get; set; }

        public bool Initializing { get; } = false;

        public void OnIdle(){}

        public void Think(string usiPosition , UsiThinkLimit limit , Color sideToMove) {}

        public void Dispose() { }

    }
}
