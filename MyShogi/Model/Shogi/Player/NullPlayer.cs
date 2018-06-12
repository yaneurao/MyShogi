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

        public void OnIdle()
        {
        }
    }
}
