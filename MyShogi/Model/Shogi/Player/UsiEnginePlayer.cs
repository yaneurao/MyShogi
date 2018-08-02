using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Usi;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Process;

namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// USIプロトコルでやりとりする思考エンジンを
    /// Player派生クラスとして実装してある
    /// </summary>
    public class UsiEnginePlayer : Player
    {
        // このあとStart()を呼び出すと開始する。
        public UsiEnginePlayer()
        {
            Initializing = true;
            Engine = new UsiEngine(); // 生成だけしておく。まだ開始はしていない。
        }

        public void Start(string exePath)
        {
            Engine.AddPropertyChangedHandler("State", StateChanged);

            var data = new ProcessNegotiatorData(exePath)
            {
                IsLowPriority = true
            };

            Engine.Connect(data);
            // 接続できているものとする。
        }

        public PlayerTypeEnum PlayerType
        {
            get { return PlayerTypeEnum.UsiEngine; }
        }

        /// <summary>
        /// エンジンからUSIプロトコルによって渡されたエンジン名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 対局者名(これが画面上に表示する名前として使われる)
        /// あとでちゃんと書く。
        /// </summary>
        public string DisplayName { get { return Name; } }

        /// <summary>
        /// このプレイヤーが指した指し手
        /// </summary>
        public Move BestMove
        {
            get { return Engine.BestMove; }
            set { /*実は実装してない*/ }
        }

        /// <summary>
        /// TIME_UPなどが積まれる。BestMoveより優先して解釈される。
        /// </summary>
        public Move SpecialMove { get; set; }

        /// <summary>
        /// このプレイヤーのponderの指し手
        /// </summary>
        public Move PonderMove { get { return Engine.PonderMove; } }

        /// <summary>
        /// 駒を動かして良いフェーズであるか？
        /// </summary>
        public bool CanMove { get; set; }

        /// <summary>
        /// "readyok"が返ってくるまでtrue
        /// </summary>
        public bool Initializing { get; set; }

        /// <summary>
        /// Engine本体
        /// </summary>
        public UsiEngine Engine;

        public void OnIdle()
        {
            // 思考するように命令が来ていれば、エンジンに対して思考を指示する。

            // 受信処理を行う。
            Engine.OnIdle();
        }

        public void Think(string usiPosition, UsiThinkLimit limit , Color sideToMove)
        {
            Engine.Think(usiPosition,limit,sideToMove);
        }

        public void Dispose()
        {
            // エンジンを解体する
            Engine.Dispose();
        }


        /// <summary>
        /// いますぐに指させる。
        /// Think()を呼び出してBestMoveはまだ得ていないものとする。
        /// </summary>
        public void MoveNow()
        {
            Engine.MoveNow();
        }

        // -- private member

        /// <summary>
        /// EngineのStateが変化したときに呼び出される。
        /// IsInitなど、必要なフラグを変更するのに用いる。
        /// </summary>
        /// <param name="args"></param>
        private void StateChanged(PropertyChangedEventArgs args)
        {
            var state = (UsiEngineState)args.value;
            switch(state)
            {
                case UsiEngineState.UsiOk:
                    // エンジンの設定を取得したいだけの時はこのタイミングで初期化は終わっていると判定すべき。
                    if (Engine.EngineSetting)
                        Initializing = false;
                    break;

                case UsiEngineState.ReadyOk:
                case UsiEngineState.InTheGame:
                    Initializing = false; // 少なくとも初期化は終わっている。
                    break;
            }

        }
    }
}
