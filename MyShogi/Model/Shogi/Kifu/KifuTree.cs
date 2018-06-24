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
            kifFormatter = new KifFormatterOptions
            {
                color = ColorFormat.Piece,
                square = SquareFormat.FullWidthMix,
                samepos = SamePosFormat.KI2sp,
                fromsq = FromSqFormat.Verbose,
            };
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

            // root nodeを作る
            currentNode = rootNode = new KifuNode(null);
            pliesFromRoot = 0;

            //    kifFormatter = KifFormatter.Ki2C;
            //    UsiMoveStringList.Clear();
            rootBoardType = BoardType.NoHandicap;
            rootSfen = Position.SFEN_HIRATE;

            // 対局情報などを保存するためにここを確保する。
            rootKifuMove = new KifuMove(Move.NONE, rootNode, KifuMoveTimes.Zero);

            KifuList = new List<string>();
            kifuWindowMoves = new List<KifuMove>();
            UsiMoveList = new List<string>();
            KifuTimeSettings = KifuTimeSettings.TimeLimitless;

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
        /// DoMove()で1手加算され、UndoMove()で1手減算される。
        /// 何手目の局面であるか。sfenを渡して初期化した場合、rootNodeが1以外の数から始まるので注意。
        /// </summary>
        public int gamePly { get { return position.gamePly; } }

        /// <summary>
        /// rootの局面からの手数。
        /// currentNode == rootNodeにおいては0。
        /// </summary>
        public int pliesFromRoot { get; private set; }

        /// <summary>
        /// rootの局面の局面タイプ
        /// 任意局面の場合は、BoardType.Others
        /// </summary>
        public BoardType rootBoardType
        {
            get { return rootBoardType_; }
            set {
                // KifuHeaderもrootBoardTypeを持っているので、そちらにも反映させなくてはならない。
                rootBoardType_ = KifuHeader.rootBoardType = value;
            }
        }
        private BoardType rootBoardType_;

        /// <summary>
        /// 棋譜ファイルからの入出力絡み
        /// 対局者氏名などもここから取り出す。
        /// </summary>
        public KifuHeader KifuHeader { get; set; } = new KifuHeader();

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
        /// 棋譜に記録されていた持ち時間設定
        ///
        /// 棋譜を読み書きする時、この設定を読み書きできるフォーマットであるならこの設定を書き出すこと。
        /// </summary>
        public KifuTimeSettings KifuTimeSettings
        {
            get; set;
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
        ///
        /// speical moveの時は、Position.DoMove()は呼び出さないが次のnodeに到達できることは保証される。
        /// (そうしないとcurrentNodeが更新されないため)
        /// </summary>
        /// <param name="m"></param>
        public void DoMove(KifuMove m)
        {
            Debug.Assert(m != null);

            // 棋譜の更新
            var stm = position.sideToMove;
            var thinkingTime = m.kifuMoveTimes.Player(stm).ThinkingTime;
            AddKifu(m.nextMove, thinkingTime);

            // special moveに対してはDoMove()を行わない。
            if (!m.nextMove.IsSpecial())
            {
                position.DoMove(m.nextMove);
                RaisePropertyChanged("Position", position);
            }

            // rootNodeからの指し手。これは棋譜リストと同期させている。
            if (EnableKifuList)
                kifuWindowMoves.Add(m);

            ++pliesFromRoot;

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
            // speical moveでこの局面に来たのであればPosition.UndoMove()は呼び出さない。
            if (!IsLastMoveSpecialMove())
            {
                position.UndoMove();
                RaisePropertyChanged("Position", position);
            }

            // rootNodeからの指し手。これは棋譜リストと同期させている。
            if (EnableKifuList)
                kifuWindowMoves.RemoveAt(kifuWindowMoves.Count-1); // RemoveLast()

            --pliesFromRoot;

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
        ///
        /// moveがspecial moveもありうる。
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
            }
        }

        /// <summary>
        /// ある指し手に対するノードコメントを追加する。
        /// </summary>
        /// <param name="move"></param>
        /// <param name="comment"></param>
        public void AddNodeComment(Move move, string comment)
        {
            var m = currentNode.moves.FirstOrDefault((x) => x.nextMove == move);
            m.nextNode.comment = comment;
        }

        /// <summary>
        /// ある指し手に対する着手時刻を追加する。
        /// </summary>
        /// <param name="move"></param>
        /// <param name="comment"></param>
        public void AddNodeMoveTime(Move move, DateTime movetime)
        {
            var m = currentNode.moves.FirstOrDefault((x) => x.nextMove == move);
            m.moveTime = movetime;
        }

        /// <summary>
        /// ある指し手に対する着手時刻を追加する。
        /// </summary>
        /// <param name="move"></param>
        /// <param name="comment"></param>
        public void AddNodeMoveTime(Move move) => AddNodeMoveTime(move, DateTime.Now);

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
        /// </summary>
        public void ClearForward()
        {
            // この枝の持つ指し手をすべて削除
            currentNode.moves.Clear();

            // 現在の局面(currentNode)までの内容と棋譜ウィンドウを同期させる。
            // 検討モードなどで棋譜ウィンドウと同期させていなかった時のための処理
            if (EnableKifuList && pliesFromRoot + 1 != KifuList.Count())
            {
                // 棋譜が同期していないので現在行以降を削除

                KifuList.RemoveRange(pliesFromRoot + 1, KifuList.Count - (pliesFromRoot + 1));
                RaisePropertyChanged("KifuList", KifuList);
            }
        }

        /// <summary>
        /// 直前の指し手がspecial moveであるか
        /// </summary>
        /// <returns></returns>
        public bool IsLastMoveSpecialMove()
        {
            // rootNodeでは遡れない。
            if (currentNode == rootNode)
                return false;

            var move = currentNode.prevNode.moves.Find(m => m.nextNode == currentNode);
            return move.nextMove.IsSpecial();
        }

        /// <summary>
        /// rootから数えてselectedIndex目にある棋譜の局面に移動する。
        /// 棋譜は消去はしない。
        /// </summary>
        /// <param name="index"></param>
        public void GotoSelectedIndex(int selectedIndex)
        {
            // rootまで何nodeあるか数えて、selectedIndexと合致するなら、局面を変更する必要性がない
            int n = 0;
            for (var c = currentNode; c.prevNode != null ; ++n)
                c = c.prevNode;
            if (n == selectedIndex)
                return;

            var e = EnableKifuList;
            EnableKifuList = false; // いま棋譜リストが更新されると困る

            RewindToRoot();
            for(int i=0;i<selectedIndex && i < kifuWindowMoves.Count; ++i)
            {
                // 棋譜ウィンドウに表示していたものを選んだのだからこれは合法。
                var move = kifuWindowMoves[i];
                // この指し手のlegalityは担保されている。(special moveであってもDoMove()は出来る)
                DoMove(move);
            }

            EnableKifuList = e; // 元の値
        }

        /// <summary>
        /// 現在の局面のKifuMoveTimesを返す。
        /// </summary>
        /// <returns></returns>
        public KifuMoveTimes GetKifuMoveTimes()
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

        /// <summary>
        /// KifuMoveTimesを現在の局面に対して設定する。
        /// </summary>
        /// <param name="kifuMoveTimes"></param>
        public void SetKifuMoveTimes(KifuMoveTimes kifuMoveTimes)
        {
            // 1つ前の局面の指し手を指した直後に記録されている。

            var prevNode = currentNode.prevNode;
            if (prevNode != null)
            {
                // prevNodeから、この局面に至る指し手を探して、そこに記録されているKifuMoveTimeに設定する
                prevNode.moves.Find((x) => x.nextNode == currentNode).kifuMoveTimes = kifuMoveTimes;
            }
            else
                // rootの局面では、rootKifuMoveTimesのほうに格納されている。
                RootKifuMoveTimes = kifuMoveTimes;
        }

        // -- 以下 private members

        /// <summary>
        /// 棋譜ウィンドウに表示されている指し手の集合
        /// 棋譜ウィンドウの指し手をクリックしてcurrentNodeから先に進む時に必要となる。
        ///
        /// EnableKifuListがtrueのときに、DoMove()/UndoMove()に応じて自動更新される。
        /// </summary>
        private List<KifuMove> kifuWindowMoves;

        /// <summary>
        /// メインウインドウの棋譜ウィンドウに表示する棋譜文字列に変換する。
        /// </summary>
        /// <param name="p"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private string move_to_kif_string(Position p, Move m)
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
        private void AddKifu(Move m, TimeSpan t)
        {
            if (EnableKifuList)
            {
                // -- 棋譜をappendする

                // 分岐棋譜であれば、先頭に"+"を付与。
                // ただし、本譜の手順でなければ"*"を付与。

                char plus;
                if (currentNode.moves.Count != 1)
                {
                    // 本譜の手順なら'+',本譜以外の手順であれば'*'
                    // ちなみに「本譜」ボタンを押した時、
                    // 棋譜の先頭から最初の'*'のあるところまで戻って、そこを'+'にする。
                    plus = currentNode.moves[0].nextMove == m ? '+' : '*';
                } else
                    plus = ' ';

                var move_text = move_to_kif_string(position, m);
                var move_text_game_ply = position.gamePly;

                move_text = string.Format("{0,-4}", move_text);
                move_text = move_text.Replace(' ', '　'); // 半角スペースから全角スペースへの置換

                var text = $"{plus}{move_text_game_ply, 3}.{move_text} {t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";

                KifuList.Add(text);
                RaisePropertyChanged("KifuList", KifuList, KifuList.Count-1 /*末尾が変更になった*/);
            }

            if (EnableUsiMoveList)
            {
                // special moveは、USIとしては規定されていない指し手になるのでここでは出力しない。
                // ("position"コマンドで送信してしまいかねないので)
                // ただし削除処理がややこしくなるのは嫌なので要素は一つ増やしておく。
                UsiMoveList.Add(!m.IsSpecial() ? m.ToUsi() : string.Empty);
            }
        }

        /// <summary>
        /// UndoMove()のときに棋譜を1行取り除く。
        /// </summary>
        private void RemoveKifu()
        {
            if (EnableKifuList)
            {
                KifuList.RemoveAt(KifuList.Count - 1); // RemoveLast()
                RaisePropertyChanged("KifuList", KifuList, KifuList.Count /*末尾が削除になった*/);
            }

            if (EnableUsiMoveList)
                UsiMoveList.RemoveAt(UsiMoveList.Count - 1); // RemoveLast()
        }

    }
}
