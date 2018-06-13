using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// ユーザーの操作を受け付けるクラスを
    /// Player派生クラスとして実装してある。
    /// </summary>
    public class HumanPlayer : Player
    {
        public PlayerTypeEnum PlayerType
        {
            get { return PlayerTypeEnum.Human; }
        }

        public string Name { get; set; }

        public string DisplayName { get { return Name; } }

        /// <summary>
        /// このプレイヤーが指した指し手
        /// </summary>
        public Move BestMove { get; set; }

        /// <summary>
        /// このプレイヤーのponderの指し手
        /// </summary>
        public Move PonderMove { get; set; }

        /// <summary>
        /// 駒を動かして良いフェーズであるか？
        /// 人間プレイヤーの場合、UIがこのフラグを見て判断する。
        /// </summary>
        public bool CanMove { get; set; }

        /// <summary>
        /// 人間プレイヤーの場合、初期化処理は不要なのでこのフラグは常にfalse
        /// </summary>
        public bool Initializing { get; } = false;

        public void OnIdle() { }

        public void Think(string usiPosition){ }

        public void Dispose() { }

    }
}
