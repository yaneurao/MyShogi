using MyShogi.Model.Common.Process;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// USIプロトコルでやりとりする思考エンジンを
    /// Player派生クラスとして実装してある
    /// </summary>
    public class UsiEnginePlayer : Player
    {
        public PlayerTypeEnum PlayerType
        {
            get { return PlayerTypeEnum.Human; }
        }

        /// <summary>
        /// 対局者名(これが画面上に表示する名前として使われる)
        /// </summary>
        public string DisplayName
        {
            get {
                return !string.IsNullOrEmpty(Name) ? Name : AliasName;
            }
        }

        /// <summary>
        /// エンジンからUSIプロトコルによって渡されたエンジン名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ユーザーがエンジンに対して別名をつけるときの名前
        /// これがnullか空の文字列であれば、RawNameがそのままNameになる。
        /// </summary>
        public string AliasName { get; set; }

        /// <summary>
        /// このプレイヤーが指した指し手
        /// </summary>
        public Move BestMove { get; set; }

        /// <summary>
        /// このプレイヤーのponderの指し手
        /// </summary>
        public Move PonderMove { get; set; }

        public ProcessNegotiator negotiator = new ProcessNegotiator();

        public void OnIdle()
        {
            // 思考するように命令が来ていれば、エンジンに対して思考を指示する。

            // 受信処理を行う。
            negotiator.Read();
        }

    }
}
