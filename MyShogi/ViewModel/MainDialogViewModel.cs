using System;
using System.Collections.Generic;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.LocalServer;

namespace MyShogi.ViewModel
{
    /// <summary>
    /// MainDialogを表示するときに必要な情報を保持しているクラス
    /// </summary>
    public class MainDialogViewModel : NotifyObject
    {
        public MainDialogViewModel()
        {
            // デバッグ中。あとで削除する。
            //Pos.InitBoard(BoardType.NoHandicap);

            // 指し手生成祭りの局面
            //            Pos.SetSfen("l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w RGgsn5p 1");

            // 手駒が1種ずつあある局面
            //Pos.SetSfen("l6n1/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GSG/R8/LN5KL w rbgsnl5p 1");

            // 入玉局面
            //Pos.SetSfen("ln6+R/1+P2GKGBR/p1ppp+P+PP+P/1k7/1p7/9/PPPPP4/1B7/LNSG1GSNL b 2SNL3P 75");

            //  成駒がいっぱいある局面
            //Pos.SetSfen("ln6+R/1+P2GKGBR/p1ppp+P+PP+P/1k7/1p7/9/PPPPP4/1B7/+L+N+SG1GSNL b 2SNL3P 75");

#if false
            // 王手結構かかる局面 王手になる指し手の数 = 67
            Pos.SetSfen("9/R1S1k1S2/2+P3+P2/2+P3+P2/2N3N2/B2L1L3/9/4+B4/K3L4 b R4G2S2NL14P 1");

            var moves = new Move[(int)Move.MAX_MOVES];
            int n = MoveGen.LegalAll(Pos, moves, 0);
            Console.WriteLine(n);
            int j = 0;
            for (int i = 0; i < n; ++i)
            {
                Pos.DoMove(moves[i]);
                if (Pos.InCheck())
                {
                    ++j;
                    Console.WriteLine(j.ToString() + ":" + moves[i].Pretty());
                }
                Pos.UndoMove();
            }
#endif

#if false
            // psnの読み込み
            var manager = new KifuManager();
            var pos = new Position();
            manager.Bind(pos);

            var psn = System.IO.File.ReadAllText("kif/4.psn", Encoding.GetEncoding("Shift_JIS"));
            var error = manager.FromString(psn);

            Pos = pos;
#endif

            // デバッグ中
            // あとで書き直す。
            //CanMove = true;

            //var list = new List<string>();
            //list.Add("   === 開始局面 ===");
            //KifuList = list;
        }

        public void Dispose()
        {
            gameServer.Dispose();
        }

        /// <summary>
        /// 対局はN個、並列対局できるので、GameControllerのインスタンスをN個保持している
        /// あとで修正する。
        /// </summary>
        public LocalGameServer gameServer { get; set; }
        
        /// <summary>
        /// 盤面。あとで書き直す。デバッグ用。
        /// </summary>
        public Position Pos { get { return gameServer.position; } }

        /// <summary>
        /// 対局者氏名。
        /// 
        /// あとで書き直す。
        /// </summary>
        public string Player1Name { get; private set; } = "わたし";
        public string Player2Name { get; private set; } = "あなた";

        /// <summary>
        /// 対局者氏名
        /// player == 0 : 先手
        /// player == 1 : 後手
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public string PlayerName(int player)
        {
            return player == 0 ? Player1Name : Player2Name;
        }

        /// <summary>
        /// いまユーザーの手番で、ユーザーはマウス操作によって駒を動かせるのか？
        /// 
        /// デバッグ用。あとで書き直す。
        /// </summary>
        /// <returns></returns>
        public bool CanMove { get; private set;}

        /// <summary>
        /// ユーザーがマウス操作によってmの指し手を入力した。
        /// ユーザーはこれを合法手だと思っているが、これが受理されるかどうかは別の話。
        /// (時間切れなどがあるので)
        /// 
        /// これを受理するのは、UIスレッドではない。
        /// </summary>
        /// <param name="m"></param>
        public void DoMoveCmd(Move m)
        {
            // UIから指し手が指された
            gameServer.DoMoveFromUI(m);
        }

    }
}
