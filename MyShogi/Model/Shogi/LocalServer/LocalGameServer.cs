using System.Threading;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.Player;
using MyShogi.Model.Common.ObjectModel;
using System;

namespace MyShogi.Model.LocalServer
{
    /// <summary>
    /// 対局を管理するクラス
    /// 
    /// 内部に局面(Position)を持つ
    /// 内部に棋譜管理クラス(KifuManager)を持つ
    /// 
    /// 思考エンジンへの参照を持つ
    /// プレイヤー入力へのインターフェースを持つ
    /// 時間を管理している。
    /// 
    /// </summary>
    public class LocalGameServer : NotifyObject
    {
        public LocalGameServer()
        {
            kifuManager = new KifuManager();
            position = new Position();
            
            kifuManager.Bind(position);

            // 起動時に平手の初期局面が表示されるようにしておく。
            kifuManager.Init();

            KifuList = new List<string>();

            var usiEngine = new UsiEnginePlayer();


            Players = new Player[2]
            {
                new HumanPlayer(),
//                new HumanPlayer(),
                usiEngine,
            };

            // 対局監視スレッドを起動して回しておく。
            thread = new Thread(thread_worker);
            thread.Start();
        }

        public void Dispose()
        {
            // スレッドを停止させる。
            stop = true;
        }

        // -- public members

        public Position position { get; private set;}

        public KifuManager kifuManager { get; private set; }

        /// <summary>
        /// 対局しているプレイヤー
        /// </summary>
        public Player[] Players;

        /// <summary>
        /// 棋譜。これをメインウィンドウの棋譜ウィンドウとdata bindする。
        /// </summary>
        public List<string> KifuList
        {
            get { return GetValue<List<string>>("KifuList"); }
            set { SetValue<List<string>>("KifuList", value); }
        }

        /// <summary>
        /// これがtrueになれば、これを購読しているViewの更新が起きるものとする。
        /// </summary>
        public bool dirty;

        // -- public methods

        /// <summary>
        /// 対局スタート
        /// </summary>
        /// <param name="player1">先手プレイヤー(駒落ちのときは下手)</param>
        /// <param name="player2">後手プレイヤー(駒落ちのときは上手)</param>
        public void GameStart(Player player1 , Player player2 /* 引数、あとで考える */)
        {
            Players[0] = player1;
            Players[1] = player2;

            KifuList = new List<string>(kifuManager.KifuList);
        }

        /// <summary>
        /// ユーザーから指し手が指された
        /// </summary>
        /// <param name="m"></param>
        public void DoMoveFromUI(Move m)
        {
            var stm = position.sideToMove;
            var stmPlayer = Players[(int)stm];

            if (stmPlayer is HumanPlayer)
            {
                // これを積んでおけばworker_threadのほうでいずれ処理される。
                stmPlayer.BestMove = m;
            }

        }

        // -- private members

        /// <summary>
        /// スレッドの終了フラグ。
        /// 終了するときにtrueになる。
        /// </summary>
        private bool stop = false;

        /// <summary>
        /// 対局中であるかを示すフラグ。
        /// これがfalseであれば対局中ではないので自由に駒を動かせる。
        /// </summary>
        private bool inTheGame = false;

        /// <summary>
        /// 対局の監視スレッド。
        /// </summary>
        private Thread thread;

        /// <summary>
        /// スレッドによって実行されていて、対局を管理している。
        /// </summary>
        private void thread_worker()
        {
            while (!stop)
            {
                foreach (var player in Players)
                {
                    player.OnIdle();
                }

                // 指し手が指されたかのチェック
                CheckMove();

                // 10msごとに各種処理を行う。
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 指し手が指されたかのチェックを行う
        /// </summary>
        private void CheckMove()
        {
            // 現状の局面の手番側
            var stm = position.sideToMove;
            var stmPlayer = Players[(int)stm];
            var bestMove = stmPlayer.BestMove; // 指し手
            if (bestMove != Move.NONE)
            {
                stmPlayer.BestMove = Move.NONE; // クリア

                if (position.IsLegal(bestMove))
                {
                    // 受理する

                    var thinkingTime = new TimeSpan(0, 0, 1);
                    kifuManager.Tree.Add(bestMove, thinkingTime);
                    kifuManager.Tree.DoMove(bestMove);

                    // -- 局面が変更になったことを通知する。

                    dirty = true;

                    // 棋譜が1行追加になったはずなので、それを反映させる。

                    KifuList = new List<string>(kifuManager.KifuList);
                    SetValue("KifuList", KifuList, KifuList.Count - 1); // 末尾が変更になったことを通知
                }
            }
        }

    }
}
