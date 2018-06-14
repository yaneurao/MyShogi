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
        public UsiEnginePlayer()
        {
            Initializing = true;

            engine = new UsiEngine();
            engine.AddPropertyChangedHandler("State", StateChanged);
            
            var data = new ProcessNegotiatorData("engine/gpsfish/gpsfish.exe");
            engine.Connect(data);
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
        public Move BestMove { get; set; }

        /// <summary>
        /// このプレイヤーのponderの指し手
        /// </summary>
        public Move PonderMove { get; set; }

        /// <summary>
        /// 駒を動かして良いフェーズであるか？
        /// </summary>
        public bool CanMove { get; set; }

        /// <summary>
        /// "readyok"が返ってくるまでtrue
        /// </summary>
        public bool Initializing { get; set; }

        public UsiEngine engine;
        
        public void OnIdle()
        {
            // 思考するように命令が来ていれば、エンジンに対して思考を指示する。

            // 受信処理を行う。
            engine.OnIdle();

            var bestMove = engine.BestMove;
            if (bestMove != Move.NONE)
            { // エンジンから結果が返ってきているので伝播する。
                BestMove = bestMove;
                engine.BestMove = Move.NONE;
            }
        }

        public void Think(string usiPosition)
        {
            engine.Think(usiPosition);
        }

        public void Dispose()
        {
            // エンジンを解体する
            engine.Dispose();
        }


        /// <summary>
        /// いますぐに指させる。
        /// Think()を呼び出してBestMoveはまだ得ていないものとする。
        /// </summary>
        public void MoveNow()
        {
            engine.MoveNow();
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
                case UsiEngineState.ReadyOk:
                case UsiEngineState.InTheGame:
                    Initializing = false; // 少なくとも初期化は終わっている。
                    break;
            }

        }
    }
}
