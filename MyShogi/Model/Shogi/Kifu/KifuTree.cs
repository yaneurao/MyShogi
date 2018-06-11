using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyShogi.Model.Shogi.Converter;
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
        ///
        /// このクラスにはPositionのインスタンスは持っていない。
        /// Bind(Position)で、外部からPositionのインスタンスを設定して使うこと。
        /// </summary>
        public KifuTree()
        {
            //    position = new Position(); // 結構重いのでコンストラクタで1度だけ作成。
            //    UsiMoveStringList = new List<string>();
            //    Init();

            EnableKifuList = true;
            EnableUsiMoveList = true;
        }

        /// <summary>
        /// 初期化する。new KifuTree()した状態に戻る。
        /// </summary>
        public void Init()
        {
            Debug.Assert(position != null, "Bind(Position)を呼び出してからInit()を呼ぶようにしてください。");

            position.InitBoard();

            currentNode = rootNode = new KifuNode(null);
            //    UsiMoveStringList.Clear();
            rootBoardType = BoardType.NoHandicap;
            rootSfen = Position.SFEN_HIRATE;
        }

        // -------------------------------------------------------------------------
        // public members
        // -------------------------------------------------------------------------

        /// <summary>
        /// 現在の局面を表現している
        /// これは、Bind()で渡すことになっている。
        /// </summary>
        public Position position { get; private set; }

        public void Bind(Position pos) { position = pos; }

        /// <summary>
        /// 棋譜の初期局面を示すnode。これを数珠つなぎに、樹形図状に持っている。
        /// </summary>
        public KifuNode rootNode;

        /// <summary>
        /// 棋譜の開始局面に対してつけられる思考ログ、対局を開始した時刻などの情報
        /// </summary>
        public KifuLog rootKifuLog;

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

        /// <summary>
        /// rootの局面の局面タイプ
        /// 任意局面の場合は、BoardType.Others
        /// </summary>
        public BoardType rootBoardType;

        /// <summary>
        /// rootの局面図。sfen形式で。
        /// </summary>
        public string rootSfen
        {
            get { return rootSfen_; }
            set
            {
                // これが設定されたときに局面リストを更新しなければならない。
                rootSfen_ = value;

                if (EnableKifuList)
                {
                    KifuList = new List<string>();
                    KifuList.Add("   === 開始局面 ===");
                }

                if (EnableUsiMoveList)
                {
                    // 初期局面のsfenであれば簡略できるので簡略して記録する。
                    // (USIエンジンに対して"position"コマンドで送信する必要があるので、
                    // そのときに素のsfen文字列で初期局面を送ると長すぎるため。)

                    UsiMoveList = new List<string>();
                    if (rootSfen_ == Position.SFEN_HIRATE)
                        UsiMoveList.Add("startpos moves");
                    else
                        UsiMoveList.Add($"sfen {rootSfen_} moves");

                    // 2つ目以降の要素があるなら"moves"の文字列が必要だからここで付与しておく。
                    // 要素が1つしかないときは末尾の"moves"を取って渡す。
                    // cf. UsiPositionString
                }
            }
        }
        private string rootSfen_;

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

            if (EnableKifuList)
                KifuList.Add(position.ToKi2(m.nextMove));

            if (EnableUsiMoveList)
                UsiMoveList.Add(m.nextMove.ToUsi());

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

            if (EnableKifuList)
                KifuList.RemoveAt(KifuList.Count - 1); // RemoveLast()

            if (EnableUsiMoveList)
                UsiMoveList.RemoveAt(UsiMoveList.Count - 1); // RemoveLast()
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
        public void Add(Move move, TimeSpan thinkingTime, TimeSpan? totalTime = null)
        {
            var m = currentNode.moves.FirstOrDefault((x) => x.nextMove == move);
            if (m == null)
            {
                // -- 見つからなかったので次のnodeを追加してやる

                KifuNode nextNode = new KifuNode(currentNode);
                currentNode.moves.Add(new KifuMove(move, nextNode, thinkingTime
                    , totalTime ?? TotalConsumptionTime() + RoundTime(thinkingTime)));
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
            // ToDo: t.Thiks != 0 だった場合を考慮しなくて良いか確認
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

        /// <summary>
        /// KIF2形式の棋譜リストを常に生成する。
        /// これをtrueにする KifuList というpropertyが有効になる。
        /// 
        /// デフォルト : true
        /// </summary>
        public bool EnableKifuList
        {
            get; set;
        }

        /// <summary>
        /// 現局面までの棋譜。
        /// EnableKifuListがtrueのとき、DoMove()/UndoMove()するごとにリアルタイムに更新される。
        /// </summary>
        public List<string> KifuList
        {
            get; set;
        }

        /// <summary>
        /// USIの指し手文字列の形式の棋譜リストを常に生成する。
        /// これをtrueにする EnableUsiMoveList というpropertyが有効になる。
        /// 
        /// デフォルト : true
        /// </summary>
        public bool EnableUsiMoveList
        {
            get; set;
        }

        /// <summary>
        /// 現局面までの棋譜。USIの指し手文字列
        /// EnableUsiMoveListがtrueのとき、DoMove()/UndoMove()するごとにリアルタイムに更新される。
        /// 
        /// cf. UsiPositionString()
        /// </summary>
        public List<string> UsiMoveList
        {
            get; set;
        }

        /// <summary>
        /// USIの"position"コマンドで用いる局面図
        /// </summary>
        public string UsiPositionString
        {
            get
            {
                if (UsiMoveList.Count == 1)
                    return UsiMoveList[0].Replace(" moves", ""); /* movesの部分を除去して返す*/

                return string.Join(" ", UsiMoveList);
            }
        }

    }
}
