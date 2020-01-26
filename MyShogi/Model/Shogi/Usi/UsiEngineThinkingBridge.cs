using System;
using System.Diagnostics;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// エンジンの思考状態を管理する。
    /// 
    /// 二重に"stop"を送信したりしないように、"go"コマンドに対して"bestmove"が返ってきたかを
    /// 管理する必要があるので("bestmove"が返ってきてからしか次のposition～goが送れないので、そのためのクラス。
    /// </summary>
    public class UsiEngineThinkingBridge
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
            get { return IsStopping ? Move.NONE : bestMove; }
        }

        /// <summary>
        /// Thinkで返ってきたponder moveを取得する。
        /// ただし、Thinkに次の局面が積まれているなら、それに対応するponder moveではないので、
        /// Move.NONEが返ることが保証されている。
        /// </summary>
        public Move PonderMove
        {
            get { return IsStopping ? Move.NONE : ponderMove; }
        }

        /// <summary>
        /// 思考中であるか。
        /// "go"コマンドで思考を開始した場合、trueになる。
        /// "bestmove"が返ってくるとfalseになる。
        /// </summary>
        public bool Thinking { get; private set; }

        /// <summary>
        /// 次のThink()が予約されていて、前回のThink()は停止途中にある。
        /// (このとき、BestMove,PonderMove,ThinkReportなどが無効化されるべき。)
        /// </summary>
        public bool IsStopping { get { return nextPosition != null; } }

        /// <summary>
        /// "MultiPV"の値。検討モードで用いる。
        /// </summary>
        public int MultiPV
        {
            get { return multiPv; }
            set {
                if (multiPv != value)
                    UpdateMultiPv(value);
                multiPv = value;
            }
        }
        private int multiPv = 1;
        private int nextMultiPv = 0;

        /// <summary>
        /// "MultiPV"が変更になった時に呼び出される。
        /// 思考中なら、stopさせて、再度探索する。
        /// </summary>
        /// <param name="multiPv_"></param>
        private void UpdateMultiPv(int multiPv_)
        {
            nextMultiPv = multiPv_;
        }

        /// <summary>
        /// 思考を開始する。思考中であるなら、queueに積んで、いまの思考が停止してから思考を開始する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="goCommand"></param>
        public void Think(Kifu.KifuNode node, string position, string goCommand)
        {
            // 思考中であれば、いまのを停止させて、queueに積む
            if (Thinking)
            {
                Stop();
                nextPosition = position;
                nextGoCommand = goCommand;
                nextNode = node;
            } else
            {
                if (nextMultiPv != 0)
                {
                    // このタイミングでMultiPVを設定してしまう。
                    SendCommand($"setoption name MultiPV value {nextMultiPv}");
                    nextMultiPv = 0;
                }

                SendCommand(position);
                SendCommand(goCommand);
                Thinking = true;
                StopSent = false;
                bestMove = Move.NONE;
                ponderMove = Move.NONE;
                CurrentNode = node;

                // Stopwatchも回しておく。
                think_timer.Reset();
                think_timer.Start();
            }
        }

        /// <summary>
        /// Think()を呼び出して思考が開始されてからの経過時間を返す。
        /// </summary>
        /// <returns></returns>
        public TimeSpan ElapsedTime
        {
            get { return think_timer.Elapsed; }
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
        /// <returns>queueに次のが積まれておらず、これで受け取ったbestMoveが、this.bestMoveに反映されたならばtrueを返す。</returns>
        public bool BestMoveReceived(Move bestMove_,Move ponderMove_)
        {
            Thinking = false;
            CurrentNode = null;

            // queueに積まれているのでそのThinkコマンドを叩いてやる。
            if (nextPosition != null)
            {
                Think(nextNode, nextPosition, nextGoCommand);
                nextPosition = null;
                nextGoCommand = null;
                nextNode = null;
                return false;
            } else
            {
                bestMove = bestMove_;
                ponderMove = ponderMove_;
                StopSent = false;
                return true;
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
        private Kifu.KifuNode nextNode = null;

        private Move bestMove;
        private Move ponderMove;
        public Kifu.KifuNode CurrentNode { get; private set; }

        private Stopwatch think_timer = new Stopwatch();
    }
}
