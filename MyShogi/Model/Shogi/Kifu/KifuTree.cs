using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜本体。
    /// 分岐棋譜の管理。
    /// 現在の局面の管理。
    /// </summary>
    public class KifuTree
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KifuTree()
        {
            position = new Position(); // 結構重いのでコンストラクタで1度だけ作成。
            Init();
        }

        /// <summary>
        /// 初期化する。new KifuTree()した状態に戻る。
        /// </summary>
        public void Init()
        {
            position.InitBoard();
            currentNode = rootNode = new KifuNode(null);
        }

        // -------------------------------------------------------------------------
        // public members
        // -------------------------------------------------------------------------

        /// <summary>
        /// 現在の局面を表現している
        /// </summary>
        public Position position;

        /// <summary>
        /// 棋譜の初手の指し手。これを数珠つなぎに、樹形図状に持っている。
        /// </summary>
        public KifuNode rootNode;

        /// <summary>
        /// posの現在の局面に対応するKifuNode
        /// </summary>
        public KifuNode currentNode;

        /// <summary>
        /// rootNodeから数えて何手目であるか。
        /// rootNodeだとply==1となる。
        /// DoMove()で1手加算され、UndoMove()で1手減算される。
        /// </summary>
        public int ply { get { return position.gamePly; } }

        // -------------------------------------------------------------------------
        // 局面に対する操作子
        // -------------------------------------------------------------------------

        // DoMove(),UndoMove()以外はcurrentNode.movesに自分で足すなり引くなりすれば良い

        /// <summary>
        /// posの現在の局面から指し手mで進める。
        /// mは、currentNodeのもつ指し手の一つであるものとする
        /// </summary>
        /// <param name="m"></param>
        public void DoMove(KifuMove m)
        {
            Debug.Assert(m != null);

            position.DoMove(m.nextMove);
            currentNode = m.nextNode;
        }

        /// <summary>
        /// 指し手mで進める。
        /// mは、currentNodeのもつ指し手の一つであるものとする。
        /// </summary>
        /// <param name="m"></param>
        public void DoMove(Move m)
        {
            DoMove(currentNode.moves.Find((x) => x.nextMove == m));
        }

        /// <summary>
        /// posを1手前の局面に移動する
        /// </summary>
        public void UndoMove()
        {
            position.UndoMove();
            currentNode = currentNode.prevNode;
        }

        /// <summary>
        /// 現在の局面(currentMove)に対して、指し手moveが登録されていないなら、その指し手を追加する。
        /// すでに存在しているなら、その指し手は追加しない。
        /// 
        /// thinkingTimeは考慮に要した時間。新たにnodeを追加しないときは、この値は無視される。
        /// ミリ秒まで計測して突っ込んでおいて良い。(棋譜出力時には秒単位で繰り上げられる)
        /// 
        /// totalTimeは総消費時間。nullを指定した場合は、ここまでの総消費時間(TotalConsumptionTime()で取得できる)に
        /// thinkingTimeを秒単位に繰り上げたものが入る。
        /// </summary>
        /// <param name="move"></param>
        /// <param name="thinkingTime"></param>
        public void Add(Move move , TimeSpan thinkingTime , TimeSpan? totalTime = null)
        {
            var m = currentNode.moves.FirstOrDefault((x)=>x.nextMove == move);
            if (m == null)
            {
                // -- 見つからなかったので次のnodeを追加してやる

                KifuNode nextNode = new KifuNode(currentNode);
                currentNode.moves.Add(new KifuMove(move,nextNode,thinkingTime
                    , totalTime ?? TotalConsumptionTime() + RoundTime(thinkingTime) ));
            }
        }

        /// <summary>
        /// currentNode(現在のnode)から、moveの指し手以降の枝を削除する
        /// </summary>
        /// <param name="move"></param>
        public void Remove(Move move)
        {
            currentNode.moves.RemoveAll((x) => x.nextMove == move);
        }

        /// <summary>
        /// ここまでの総消費時間
        /// </summary>
        /// <returns></returns>
        public TimeSpan TotalConsumptionTime()
        {
            // 2手前が自分の手番なので、そこに加算する。
            var prev = currentNode.prevNode;
            if (prev == null)
                return new TimeSpan();
            var prev2 = prev.prevNode;
            if (prev2 == null)
                return new TimeSpan();

            return prev2.moves.Find((x) => x.nextNode == prev).totalTime;
        }
   

        /// <summary>
        /// timeから秒を繰り上げた時間
        /// 表示時や棋譜ファイルへの出力時は、こちらを用いる
        /// </summary>
        /// <param name="t"></param>
        public TimeSpan RoundTime(TimeSpan t)
        {
            // ミリ秒が端数があれば、秒単位で繰り上げる。
            return (t.Milliseconds == 0) ? t : t.Add(new TimeSpan(0, 0, 0, 0, 1000 - t.Milliseconds));
        }

        /// <summary>
        /// rootまで局面を巻き戻す。
        /// そのときのKifuMoveをListにして返す。
        /// このKifuMoveを逆順で適用(DoMove)していくと元の局面になる。
        /// </summary>
        /// <returns></returns>
        public List<KifuMove> RewindToRoot()
        {
            var moves = new List<KifuMove>();

            while (rootNode != currentNode)
            {
                var c = currentNode;
                UndoMove();
                moves.Add(currentNode.moves.Find((x) => x.nextNode == c));
            }

            return moves;
        }
        
        /// <summary>
        /// RewindToRoot()でrootまで巻き戻したものを元の局面に戻す。
        /// RewindToRoot()の返し値を、このメソッドの引数に渡すこと。
        /// </summary>
        /// <param name="moves"></param>
        public void FastForward(List<KifuMove> moves)
        {
            for (int i = moves.Count() - 1; i >= 0; --i)
                DoMove(moves[i]);
        }
    }
}
