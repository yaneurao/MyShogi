using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// プレイヤーを抽象化したインターフェース
    /// 指し手を返す。
    /// </summary>
    public interface Player
    {
        /// <summary>
        /// 対局者の種別
        /// </summary>
        PlayerTypeEnum PlayerType { get; }

        /// <summary>
        /// 対局者名
        /// 
        /// ※　UsiEnginePlayerの場合、エンジンから渡された名前
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 対局者名(これが画面上に表示する名前として使われる)
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// このプレイヤーが指した指し手
        /// </summary>
        Move BestMove { get; set; }

        /// <summary>
        /// このプレイヤーのponderの指し手
        /// </summary>
        Move PonderMove { get; set; }

        /// <summary>
        /// 駒を動かして良いフェーズであるか？
        /// LocalGameServerのほうから設定される。
        /// 人間プレイヤーの場合、UIがこのフラグを見て、UIで操作して良いかどうかを判断する。
        /// </summary>
        bool CanMove { get; set; }

        /// <summary>
        /// 通信の受信などのためにhost側から定期的に呼び出される。
        /// (コールバックが任意のタイミングで起きると制御しにくいので、
        /// このOnIdle()のタイミングで、このOnIdle()を呼び出したスレッドからしか
        /// コールバックが行われないことを保証する。)
        /// </summary>
        void OnIdle();

        /// <summary>
        /// 自分の手番なので思考を開始して、BestMoveとPonderMoveに指し手を返す。
        /// PonderMoveに指し手を返すのは、UsiEnginePlayerのときのみ。
        /// HumanPlayerのときは、usiPositionを生成するコストがもったいないのでnullが渡ってくることになっている。
        /// </summary>
        /// <param name="usiPosition"></param>
        void Think(string usiPosition);
    }
}

