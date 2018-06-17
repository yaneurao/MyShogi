using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜本体。
    /// 分岐棋譜の管理。
    /// 現在の局面の管理。
    /// </summary>
    public class KifuTree : NotifyObject
    {
        /// <summary>
        /// コンストラクタ
        ///
        /// このクラスが内部的なPositionのインスタンスも保持している。
        /// </summary>
        public KifuTree()
        {
            EnableKifuList = true;
            kifFormatter = KifFormatter.Ki2C;
            EnableUsiMoveList = true;

            position = new Position();
            Init();
        }

        /// <summary>
        /// 初期化する。new KifuTree()した状態に戻る。
        /// </summary>
        public void Init()
        {
            position.InitBoard();
            
            currentNode = rootNode = new KifuNode(null);
            //    UsiMoveStringList.Clear();
            rootBoardType = BoardType.NoHandicap;
            rootSfen = Position.SFEN_HIRATE;

            // 対局情報などを保存するためにここを確保する。
            rootKifuMove = new KifuMove(Move.NONE, rootNode, KifuMoveTimes.Zero);

            RaisePropertyChanged("Position", position);
        }

        // -------------------------------------------------------------------------
        // public members
        // -------------------------------------------------------------------------

        /// <summary>
        /// 現在の局面を表現している。
        /// immutableではないので(DoMove()/UndoMove()によって変化するので)、
        /// data bindするときはClone()してからにすること。
        /// 
        /// また、"Position"というNotifyObjectの仮想プロパティがあり、このクラスのDoMove()/UndoMove()に対して
        /// この"Position"のプロパティ変更通知が来る。
        /// </summary>
        public Position position { get; private set; }

        /// <summary>
        /// 棋譜の初期局面を示すnode。これを数珠つなぎに、樹形図状に持っている。
        /// </summary>
        public KifuNode rootNode;

        /// <summary>
        /// posの現在の局面に対応するKifuNode
        /// </summary>
        public KifuNode currentNode;

        /// <summary>
        /// rootの局面に至るための仮想的なKifuMove
        /// ここに対局日の情報、対局開始局面の棋譜コメント、対局開始時の持ち時間などが記載される。
        /// </summary>
        public KifuMove rootKifuMove;

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

                // rootSfenは局面を設定して、最後に呼び出すはずなので、ここで通知しても大丈夫。
                RaisePropertyChanged("Position", position);

                if (EnableKifuList)
                {
                    KifuList = new List<string>();
                    KifuList.Add("   === 開始局面 ===");
                    RaisePropertyChanged("KifuList", KifuList);
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

        // -- 以下、文字列化された棋譜絡み

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
        /// 生成する棋譜文字列のフォーマット
        /// </summary>
        public IKifFormatterOptions kifFormatter
        {
            get; set;
        }

        /// <summary>
        /// 現局面までの棋譜。
        /// EnableKifuListがtrueのとき、DoMove()/UndoMove()するごとにリアルタイムに更新される。
        /// 
        /// また、"KifuList"というNotifyObjectの仮想プロパティがあり、このクラスのDoMove()/UndoMove()などに対して
        /// この"KifuList"のプロパティ変更通知が来る。
        /// immutableではないので、data-bindなどで用いるなら、Clone()してから用いること。
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

        // --- 以下、rootの局面に対するproperty

        /// <summary>
        /// ここに開始局面での残り時間などが格納されている。
        /// </summary>
        public KifuMoveTimes RootKifuMoveTimes
        {
            get { return rootKifuMove.kifuMoveTimes; }
            set { rootKifuMove.kifuMoveTimes = value; }
        }

        /// <summary>
        /// ここに対局日、開始局面に対するコメント等の情報が格納されている。
        /// </summary>
        public KifuLog RootKifuLog
        {
            get { return rootKifuMove as KifuLog; }
        }

        // -------------------------------------------------------------------------
        // 局面に対する操作子
        // -------------------------------------------------------------------------

        // DoMove(),UndoMove()以外はcurrentNode.movesに自分で足すなり引くなりすれば良い
        // CurrentNodeが設定されていないと局面を進められない。

        /// <summary>
        /// posの現在の局面から指し手mで進める。
        /// mは、currentNodeのもつ指し手の一つであるものとする
        /// </summary>
        /// <param name="m"></param>
        public void DoMove(KifuMove m)
        {
            Debug.Assert(m != null);

            // 棋譜の更新
            var stm = position.sideToMove;
            var thinkingTime = m.kifuMoveTimes.Player(stm).ThinkingTime;
            AddKifu(m.nextMove , thinkingTime);
            
            position.DoMove(m.nextMove);
            RaisePropertyChanged("Position", position);

            currentNode = m.nextNode;

            // もし次がSpecialMoveの局面に到達したのであれば、棋譜に積む。
            if (currentNode.moves.Count != 0 && currentNode.moves[0].nextMove.IsSpecial())
                AddKifuSpecialMove(currentNode.moves[0].nextMove , TimeSpan.Zero);
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
            // 棋譜の末端にspecial moveを積んでいたのであればそれを取り除く。
            if (currentNode.moves.Count != 0 && currentNode.moves[0].nextMove.IsSpecial())
                RemoveKifu(true);

            position.UndoMove();
            RaisePropertyChanged("Position", position);

            currentNode = currentNode.prevNode;

            // 棋譜の更新
            RemoveKifu();
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
        public void AddNode(Move move, KifuMoveTimes kifuMoveTimes)
        {
            var m = currentNode.moves.FirstOrDefault((x) => x.nextMove == move);
            if (m == null)
            {
                // -- 見つからなかったので次のnodeを追加してやる

                KifuNode nextNode = new KifuNode(currentNode);
                currentNode.moves.Add(new KifuMove(move, nextNode, kifuMoveTimes));

                // 特殊な指し手であるなら、この時点で棋譜の終端にその旨を記録しておく。
                if (move.IsSpecial())
                    AddKifuSpecialMove(move , kifuMoveTimes.Player(position.sideToMove).ThinkingTime);
            }
        }

        /// <summary>
        /// ある指し手に対するノードコメントを追加する。
        /// </summary>
        /// <param name="move"></param>
        /// <param name="comment"></param>
        public void AddNodeComment(Move move,string comment)
        {
            var m = currentNode.moves.FirstOrDefault((x) => x.nextMove == move);
            m.nextNode.comment = comment;
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
        /// currentNode(現在のnode)から、次のnodeがnextNodeである枝を削除する。
        /// 対局時の待ったの処理用。
        /// </summary>
        /// <param name="nextNode"></param>
        public void Remove(KifuNode nextNode)
        {
            currentNode.moves.RemoveAll((x) => x.nextNode == nextNode);
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
        /// 現局面以降の棋譜データを削除する。(中断局の再開など用)
        /// 末尾にspecial moveが積んである場合、その部分の棋譜も削除する。
        /// </summary>
        public void ClearForward()
        {
            // 末尾にspecial moveが積んであるなら、棋譜を1行削除する必要がある。
            if (currentNode.moves.Count!=0 && currentNode.moves[0].nextMove.IsSpecial())
                RemoveKifu(true);

            // この枝の持つ指し手をすべて削除
            currentNode.moves.Clear();
        }

        /// <summary>
        /// 現在の局面のKifuMoveTimesを返す。
        /// </summary>
        /// <returns></returns>
        public KifuMoveTimes GetKifuMoveTime()
        {
            // 1つ前の局面の指し手を指した直後に記録されている。

            var prevNode = currentNode.prevNode;
            if (prevNode != null)
            {
                // prevNodeから、この局面に至る指し手を探して、そこに記録されているKifuMoveTimeを返せば良い。
                return prevNode.moves.Find((x) => x.nextNode == currentNode).kifuMoveTimes;
            }
            else
                // rootの局面では、rootKifuMoveTimesのほうに格納されている。
                return RootKifuMoveTimes;
        }

        // -- 以下private

        /// <summary>
        /// メインウインドウの棋譜ウィンドウに表示する棋譜文字列に変換する。
        /// </summary>
        /// <param name="p"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private string move_to_kif_string(Position p , Move m)
        {
            // 特殊な指し手は、KIF2フォーマットではきちんと変換できないので自前で変換する。
            // 例えば、連続王手の千日手による反則勝ちが単に「千日手」となってしまってはまずいので。
            // (『Kifu for Windoiws』ではそうなってしまう..)
            return m.IsOk() ? kifFormatter.format(p, m) : m.SpecialMoveToKif().Left(6);
        }

        /// <summary>
        /// DoMove()のときに棋譜に追加する
        /// </summary>
        /// <param name="m"></param>
        private void AddKifu(Move m , TimeSpan t)
        {
            if (EnableKifuList)
            {
                // 棋譜をappendする

                var move_text = move_to_kif_string(position, m);
                var move_text_game_ply = position.gamePly;

                move_text = string.Format("{0,-4}", move_text);
                move_text = move_text.Replace(' ', '　'); // 半角スペースから全角スペースへの置換

                var text = string.Format("{0,3}.{1} {2}", move_text_game_ply, move_text,
                    $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}");

                KifuList.Add(text);
                RaisePropertyChanged("KifuList", KifuList, KifuList.Count-1 /*末尾が変更になった*/);
            }

            if (EnableUsiMoveList)
            {
                // special moveは、USIとしては規定されていない指し手になるのでここでは出力しない。
                // ("position"コマンドで送信してしまいかねないので)
                if (!m.IsSpecial())
                    UsiMoveList.Add(m.ToUsi());
            }
        }

        /// <summary>
        /// UndoMove()のときに棋譜を1行取り除く。
        /// </summary>
        private void RemoveKifu(bool isSpecialMove = false)
        {
            if (EnableKifuList)
            {
                KifuList.RemoveAt(KifuList.Count - 1); // RemoveLast()
                RaisePropertyChanged("KifuList", KifuList, KifuList.Count /*末尾が削除になった*/);
            }

            // UsiMoveListにはspecial moveは含まれていないので、取り除くことは出来ない。
            if (EnableUsiMoveList && !isSpecialMove)
                UsiMoveList.RemoveAt(UsiMoveList.Count - 1); // RemoveLast()
        }

        /// <summary>
        /// KifuListの末尾にspecial moveを積む。
        /// </summary>
        /// <param name="m"></param>
        private void AddKifuSpecialMove(Move m , TimeSpan t)
        {
            Debug.Assert(m.IsSpecial());
            AddKifu(m , t);
        }

    }
}
