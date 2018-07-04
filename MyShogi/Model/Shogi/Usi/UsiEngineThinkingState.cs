using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// エンジンの思考状態を管理する。
    /// 
    /// 二重に"stop"を送信したりしないように、"go"コマンドに対して"bestmove"が返ってきたかを管理する必要がある。
    /// </summary>
    public class UsiEngineThinkingState
    {
        /// <summary>
        /// コマンドはこのメソッドを通じて送るので、このクラスを使う時は設定しておくこと。
        /// </summary>
        /// <param name="s"></param>
        public delegate void SendCommandHandler(string s);
        public SendCommandHandler SendCommand;

        /// <summary>
        /// Thinkで返ってきたbestmoveを取得する。
        /// ただし、Thinkに次の局面が積まれているなら、それに対応するbestMoveではないので、
        /// Move.NONEが返ることが保証されている。
        /// </summary>
        public Move BestMove
        {
            get { return nextPosition != null ? Move.NONE : bestMove; }
        }

        /// <summary>
        /// Thinkで返ってきたponder moveを取得する。
        /// ただし、Thinkに次の局面が積まれているなら、それに対応するponder moveではないので、
        /// Move.NONEが返ることが保証されている。
        /// </summary>
        public Move PonderMove
        {
            get { return nextPosition != null ? Move.NONE : ponderMove; }
        }

        /// <summary>
        /// 思考中であるか。
        /// "go"コマンドで思考を開始した場合、trueになる。
        /// "bestmove"が返ってくるとfalseになる。
        /// </summary>
        public bool Thinking;

        /// <summary>
        /// 思考を開始する。思考中であるなら、queueに積んで、いまの思考が停止してから思考を開始する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="goCommand"></param>
        public void Think(string position,string goCommand)
        {
            // 思考中であれば、いまのを停止させて、queueに積む
            if (Thinking)
            {
                Stop();
                nextPosition = position;
                nextGoCommand = goCommand;
            } else
            {
                SendCommand(position);
                SendCommand(goCommand);
                Thinking = true;
                StopSent = false;
                bestMove = Move.NONE;
                ponderMove = Move.NONE;
            }
        }

        /// <summary>
        /// "stop"コマンドを送信する。思考中でなければ送信しない。すでに送信したあとである場合も送信しない。
        /// </summary>
        public void Stop()
        {
            if (!Thinking)
                return; // すでに停止しているっぽい。

            // すでに送ったのでbestmoveの待ち状態である。2重に送ることは出来ない。
            if (StopSent)
                return;

            SendCommand("stop");
            StopSent = true;
        }

        /// <summary>
        /// "bestmove"を受信した時に呼び出されるハンドラ
        /// </summary>
        public void BestMoveReceived(Move bestMove_,Move ponderMove_)
        {
            Thinking = false;
            bestMove = bestMove_;
            ponderMove = ponderMove_;

            // queueに積まれているのでそのThinkコマンドを叩いてやる。
            if (nextPosition != null)
            {
                Think(nextPosition, nextGoCommand);
                nextPosition = null;
                nextGoCommand = null;
            }
        }

        /// <summary>
        /// "stop"コマンドを送信したか。
        /// 2重にstopを送信することは出来ない。
        /// そのための管理。
        /// </summary>
        private bool StopSent;

        /// <summary>
        /// 次に思考するqueue
        /// </summary>
        private string nextPosition = null;
        private string nextGoCommand = null;

        private Move bestMove;
        private Move ponderMove;

    }
}
