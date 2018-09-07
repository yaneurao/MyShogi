using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyShogi.App;
using MyShogi.Model.Common.Extensions;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.LocalServer;

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
            EnableUsiMoveList = true;

            InitKifuFormatter();

            position = new Position();
            Init();
        }

        /// <summary>
        /// 棋譜の表示形式。現在のGlobalOptionの値を反映させる。
        /// </summary>
        private void InitKifuFormatter()
        {
            /*
            kifFormatter = new KifFormatterOptions
            {
                color = ColorFormat.Piece,
                square = SquareFormat.FullWidthMix,
                samepos = SamePosFormat.KI2sp,
                //fromsq = FromSqFormat.Verbose,
                fromsq = FromSqFormat.KI2, // 移動元を入れると棋譜ウィンドウには入り切らないので省略する。
            };
            */

            var kifu_version = TheApp.app.Config.KifuWindowKifuVersion;
            switch(kifu_version)
            {
                case 0: kifFormatter = KifFormatter.Ki2CP; break;
                case 1: kifFormatter = KifFormatter.KifCP; break;
                case 2: kifFormatter = KifFormatter.CsaCP; break;
                case 3: kifFormatter = KifFormatter.SfenCP; break;
                default: Debug.Assert(false);  break;
            }
        }

        /// <summary>
        /// 初期化する。new KifuTree()した状態に戻る。
        /// ただし、EnableKifuList == falseだと棋譜リストの初期化されないので、リセットしたいならResetKifuList()を明示的に呼び出す必要がある。
        /// </summary>
        public void Init()
        {
            // root nodeを作る
            currentNode = rootNode = new KifuNode(null);
            pliesFromRoot = 0;

            SetRootBoardType(BoardType.NoHandicap);

            // rootSfenのsetterで初期化されているのでここではKifuList、UsiMoveListの初期化はしない
            //KifuList = new List<string>();
            //UsiMoveList = new List<string>();

            // 対局情報などを保存するためにここを確保する。
            rootKifuMove = new KifuMove(Move.NONE, rootNode, KifuMoveTimes.Zero);

            kifuWindowMoves = new List<KifuMove>();
            KifuTimeSettings = KifuTimeSettings.TimeLimitless;
            KifuBranch = -1;
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
        /// 棋譜ファイルからの入出力絡み
        /// 対局者氏名などもここから取り出す。
        /// </summary>
        public KifuHeader KifuHeader { get; set; } = new KifuHeader();

        /// <summary>
        /// rootの局面図。sfen形式で。
        ///
        /// 注意 : 
        /// 　このsetterでposition.Clone()してイベントが生起するので、
        /// 　rootSfenに代入する前にPosition.SetSfen()をするなど、Clone()されても大丈夫な状態になっていることを考慮すること。
        /// </summary>
        public string rootSfen
        {
            get { return rootSfen_; }
            private set
            {
                // これが設定されたときに局面リストを更新しなければならない。
                rootSfen_ = value;

                // rootSfenは局面を設定して、最後に呼び出すはずなので、ここで通知しても大丈夫。
                RaisePropertyChanged("Position", position.Clone());

                if (EnableKifuList)
                {
                    ResetKifuList();
                    RaisePropertyChanged("KifuList", new List<string>(KifuList));
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

                KifuDirty = true;
            }
        }
        private string rootSfen_;

        /// <summary>
        /// rootの局面の局面タイプ
        /// 任意局面の場合は、BoardType.OthersかBoardType.OthersHandicap(駒落ちのとき)
        /// </summary>
        public BoardType rootBoardType
        {
            get { return rootBoardType_; }
            private set
            {
                // KifuHeaderもrootBoardTypeを持っているので、そちらにも反映させなくてはならない。
                rootBoardType_ = KifuHeader.rootBoardType = value;
            }
        }
        private BoardType rootBoardType_;

        /// <summary>
        /// rootのsfenを設定する。root nodeの初期化には必ずこのメソッドかSetRootBoardTypeを用いること。
        ///
        /// rootBoardTypeは自動的に設定される。
        /// position.SetSfen()も自動的に呼び出される。
        /// 
        /// BoardTypeがわかっているならSetRootBoardType()を呼び出したほうが
        /// (sfen文字列からBoardTypeを判定するコストがかからなくて)良い。
        /// </summary>
        /// <param name="sfen"></param>
        public void SetRootSfen(string sfen)
        {
            position.SetSfen(sfen);
            rootBoardType = Util.BoardTypeFromSfen(sfen);

            // このsetterでイベントが生起するので、position.SetSfen()のあとで代入しないといけない。
            rootSfen = sfen;
        }

        /// <summary>
        /// rootのsfenを設定する。
        /// 
        /// position.SetSfen()したときは、
        /// このメソッドがSetRootSfen()かのどちらかを呼び出すこと。
        /// </summary>
        /// <param name="boardType"></param>
        public void SetRootBoardType(BoardType boardType)
        {
            Debug.Assert(boardType < BoardType.Others);

            var sfen = boardType.ToSfen();
            position.SetSfen(sfen);
            rootBoardType = boardType;

            // このsetterでイベントが生起するので、position.SetSfen()のあとで代入しないといけない。
            rootSfen = sfen;
        }

        /// <summary>
        /// 棋譜リストの初期化。(イベントは発生しない)
        /// </summary>
        public void ResetKifuList()
        {
            KifuList = new List<string>();
            KifuList.Add("   === 開始局面 ===");
        }

        /// <summary>
        /// 棋譜が汚れたか。
        /// 棋譜を保存したときにこれをfalseにしておけば、そのあと何らかの操作で棋譜が汚れた(更新があった)時に
        /// 自動的にtrueになるので、棋譜が未保存であるか判定できて便利である。
        /// </summary>
        public bool KifuDirty { get; set; }


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
        /// 最後に棋譜リストに追加された文字列をKI2形式で取得する。
        /// special moveは対象外。
        ///
        /// </summary>
        public string LastKifuString
        {
            get; private set;
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

        /// <summary>
        /// 棋譜上で、本譜の手順から最初に分岐した行
        /// (この行以降は、インデントを入れたり、棋譜の文字の色を変えたりして表現すると良いと思う)
        /// ※ この値は0を取らない。(開始局面の手前で分岐することはないため)
        ///
        /// -1 : 分岐した行がない
        /// </summary>
        public int KifuBranch { get; set; }

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
            AddKifu(m, thinkingTime);

            // special moveに対してはDoMove()を行わない。(行っても良いが盤面が変わらないのでやるだけ無駄である)
            if (!m.nextMove.IsSpecial())
            {
                position.DoMove(m.nextMove);
                RaisePropertyChanged("Position", position.Clone());
            }

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
            if (!IsSpecialNode())
            {
                position.UndoMove();
                RaisePropertyChanged("Position", position.Clone());
            }

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

#if false
        // これ使わないのでコメントアウトしておく。
        // RemoveNextNode()のように、KifuBranchの更新が必要だと思う。

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
#endif

        /// <summary>
        /// currentNodeから、次のnodeをすべて削除する。
        /// 対局時の待ったや、検討時に一手削除する用。
        /// </summary>
        public void RemoveNextNode()
        {
            currentNode.moves.Clear();

            // この結果、KifuBranchより遡るなら、本譜の手順に戻っているわけで、分岐表現をしてはならない。

            if (pliesFromRoot < KifuBranch)
                KifuBranch = -1;
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
            if (EnableKifuList)
            {
                // 棋譜が同期していない可能性があるので現在行以降を削除
                ClearKifuForward();

                RaisePropertyChanged("KifuList", new List<string>(KifuList));
            }
        }

        /// <summary>
        /// rootから数えてselectedIndex目にある棋譜の局面に移動する。
        /// 棋譜は消去はしない。棋譜の選択行の移動もしない。
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

            // 棋譜ウィンドウに表示されている行数を超えている場合も移動できないと思われ…。(移動に失敗するので何もしない)
            // kifuWindowMovesは指し手の集合であり、ウィンドウ上の棋譜の先頭は「開始局面」のnodeであるから、
            // ウィンドウ上の棋譜の行数 - 1 == kifuWindowMoves.Countのはずである。
            // よって条件式は、 ">=" ではなく  ">" になる。
            if (selectedIndex > kifuWindowMoves.Count)
                return;

            var e1 = EnableKifuList;
            var e2 = PropertyChangedEventEnable;

            EnableKifuList = false; // いま棋譜リストが更新されると困る
            PropertyChangedEventEnable = false; // いまイベントが発生すると困る(DoMove()で発生させてしまう)

            RewindToRoot();
            for (int i = 0; i < selectedIndex && i < kifuWindowMoves.Count; ++i)
            {
                // 棋譜ウィンドウに表示していたものを選んだのだからこれは合法。
                var move = kifuWindowMoves[i];
                // この指し手のlegalityは担保されている。(special moveであってもDoMove()は出来る)
                DoMove(move);
            }

            EnableKifuList = e1; // 元の値
            PropertyChangedEventEnable = e2;

            // 移動が完了したので局面の更新通知を送る。
            RaisePropertyChanged("Position", position.Clone());
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

        /// <summary>
        /// 本譜の分岐を選ぶボタン
        /// </summary>
        public void MainBranch()
        {
            if (KifuBranch == -1)
                return;
            if (pliesFromRoot < KifuBranch)
                return;

            // KifuBranchの手まで戻る。

            PropertyChangedEventEnable = false;
            int branch = KifuBranch;

            // pliesFromRoot == branch -1 のnode(ここに分岐がある)に
            // さかのぼって本譜の手順を選択してKifuListを更新する。

            while (pliesFromRoot >= branch)
                UndoMove();

            // ただし、画面に表示すべきはpliesFromRoot == branchのnodeであるから、このあと、
            // DoMove()して、 (その回数 - 1 )回だけ UndoMove()を行う。

            // 現在行以降のKifuList、KifuMovesを削除
            ClearKifuForward();

            var e = EnableKifuList;
            EnableKifuList = true;

            int ply = 0;
            for (; currentNode.moves.Count != 0; ++ply)
            {
                var move = currentNode.moves[0]; // 0を選んでいく。

                // この指し手のlegalityは担保されている。(special moveであってもDoMove()は出来る)
                DoMove(move);
            }
            EnableKifuList = false; // 棋譜リストの更新が終わったので棋譜Windowをフリーズ
            for (; ply > 1; --ply) // DoMoveした回数 - 1回だけ回る。
                UndoMove();

            PropertyChangedEventEnable = true;

            RaisePropertyChanged("KifuList", new List<string>(KifuList));
            RaisePropertyChanged("Position", position.Clone());

            EnableKifuList = e; // 元の値

        }

        /// <summary>
        /// 次の分岐を選ぶボタン
        /// </summary>
        public void NextBranch()
        {
            // 1つ前のnodeがなければNG
            var prev = currentNode.prevNode;
            if (prev == null)
                return;

            // 分岐がなければNG
            if (prev.moves.Count <= 1)
                return;

            // 分岐があるので次の分岐を選択して、棋譜ウィンドウを更新する。
            int n = prev.moves.FindIndex((x) => x.nextNode == currentNode);
            int n2 = (n + 1) % prev.moves.Count; /* 次分岐 */

            PropertyChangedEventEnable = false;

            // このnodeに来て、n2を選択してKifuListを更新する。
            UndoMove();

            // 現在行以降のKifuList、KifuMovesを削除
            ClearKifuForward();

            var e = EnableKifuList;
            EnableKifuList = true;

            DoMove(prev.moves[n2]);

            int ply = 0;
            for (; currentNode.moves.Count != 0 ; ++ply)
            {
                var move = currentNode.moves[0]; // 0を選んでいく。

                // この指し手のlegalityは担保されている。(special moveであってもDoMove()は出来る)
                DoMove(move);
            }
            EnableKifuList = false; // 棋譜リストの更新が終わったので棋譜Windowをフリーズ
            for (; ply > 0; --ply)
                UndoMove();

            PropertyChangedEventEnable = true;

            RaisePropertyChanged("KifuList", new List<string>(KifuList));
            RaisePropertyChanged("Position", position.Clone());

            EnableKifuList = e; // 元の値
        }

        /// <summary>
        /// 分岐を削除するボタン
        /// </summary>
        public void EraseBranch()
        {
            // 1つ前のnodeがなければNG
            var prev = currentNode.prevNode;
            if (prev == null)
                return;

            // 分岐がなければNG
            if (prev.moves.Count <= 1)
                return;

            // 分岐があるので現在の分岐を削除して、棋譜ウィンドウを更新する。
            int n = prev.moves.FindIndex((x) => x.nextNode == currentNode);

            PropertyChangedEventEnable = false;

            // このnodeに来て、n2を選択してKifuListを更新する。
            UndoMove();

            // 現在行以降のKifuList、KifuMovesを削除
            ClearKifuForward();

            var e = EnableKifuList;
            EnableKifuList = true;

            prev.moves.RemoveAt(n);
            // nの要素に後続のものがあれば、それを辿る。なければmoves[0]を辿る。
            n = n % prev.moves.Count;

            // 棋譜ウィンドウ上の分岐の起点で、ここに他の分岐がなくなってしまったのであれば、KifuBranchの値を更新する。
            if (prev.moves.Count == 1 && KifuBranch == pliesFromRoot + 1)
                KifuBranch = -1;

            DoMove(prev.moves[n]);

            int ply = 0;
            for (; currentNode.moves.Count != 0; ++ply)
            {
                var move = currentNode.moves[0]; // 0を選んでいく。

                // この指し手のlegalityは担保されている。(special moveであってもDoMove()は出来る)
                DoMove(move);
            }
            EnableKifuList = false; // 棋譜リストの更新が終わったので棋譜Windowをフリーズ
            for (; ply > 0; --ply)
                UndoMove();

            PropertyChangedEventEnable = true;

            RaisePropertyChanged("KifuList", new List<string>(KifuList));
            RaisePropertyChanged("Position", position.Clone());

            EnableKifuList = e; // 元の値

            KifuDirty = true; // 分岐の削除に成功して棋譜が変更になった
        }

        /// <summary>
        /// 現在のnodeを本譜の手順に変更する。
        ///
        /// 棋譜ウィンドウのカーソル位置は変更しないので呼び出し元で面倒見るべき。
        /// </summary>
        public void MakeCurrentNodeMainBranch()
        {
            PropertyChangedEventEnable = false;
            var e = EnableKifuList;
            EnableKifuList = true;

            var kifuMove = new List<KifuMove> (kifuWindowMoves); // Clone()

            int ply = pliesFromRoot;
            while (pliesFromRoot > 0)
                UndoMove();
            for (int i = 0; i<ply;++i)
            {
                int n = currentNode.moves.FindIndex((x) => x == kifuMove[i]);

                // 本譜の手順に変更する。
                // ※　swap()を呼びたいのだが、Listの[]は、indexerであるからrefで渡せない。
                var t = currentNode.moves[0];
                currentNode.moves[0] = currentNode.moves[n];
                currentNode.moves[n] = t;

                DoMove(currentNode.moves[0]);
            }

            EnableKifuList = e;
            PropertyChangedEventEnable = true;
            RaisePropertyChanged("KifuList", new List<string>(KifuList)); // 棋譜ウィンドウのカーソル位置変わりうる。
        }

        /// <summary>
        /// このnodeに至るための直前の指し手を返す。
        /// root nodeであればnullが返る。
        /// </summary>
        /// <returns></returns>
        public KifuMove LastMove()
        {
            // 直前がspecial moveであるなら、ここはspecial move後のnodeであるから、動かせない可能性がある。
            var prev = currentNode.prevNode;
            if (prev == null)
                return null;

            // 必ずあるはず..
            return prev.moves.Find((x) => x.nextNode == currentNode);
        }

        /// <summary>
        /// 現在、special nodeにいるのか。
        /// </summary>
        /// <returns></returns>
        public bool IsSpecialNode()
        {
            var last = LastMove();
            return last == null ? false : last.nextMove.IsSpecial();
        }

        /// <summary>
        /// special nodeに突入するnodeなのか。
        /// special nodeに到達するのであれば、その指し手(specail move)を返す。
        ///
        /// isHuman == trueの場合、人間プレイヤーなので入玉宣言が出来る状況ならば自動的に入玉宣言を行う。
        /// </summary>
        /// <returns></returns>
        public Move IsNextNodeSpecialNode(bool isHuman, MiscSettings misc)
        {
            // 現在、special nodeでないことを前提とする。
            // すでにspecial nodeに突入していたら、何もしない。
            if (IsSpecialNode())
                return Move.NONE;

            // -- このDoMoveの結果、千日手や詰み、持将棋など特殊な局面に至ったか？
            Move m = Move.NONE;

            var rep = position.IsRepetition();

            // 入玉ルール
            var rule = (EnteringKingRule)misc.EnteringKingRule;

            // トライルールによる勝ち
            if (rule == EnteringKingRule.TRY_RULE && position.DeclarationWin(rule) == Move.WIN_THEM)
            {
                m = Move.WIN_THEM; // 一つ前の手番側の入玉勝ち
            }
            // 手数による引き分けの局面であるか
            else if (misc.MaxMovesToDrawEnable && misc.MaxMovesToDraw < position.gamePly)
            {
                m = Move.MAX_MOVES_DRAW;
            }
            // この指し手の結果、詰みの局面に至ったか
            else if (position.IsMated(moves))
            {
                m = Move.MATED;
            }
            // 千日手絡みの局面であるか？
            else if (rep != RepetitionState.NONE)
            {
                // 千日手関係の局面に至ったか
                switch (rep)
                {
                    case RepetitionState.DRAW: m = Move.REPETITION_DRAW; break;
                    case RepetitionState.LOSE: m = Move.REPETITION_LOSE; break; // 実際にはこれは起こりえない。
                    case RepetitionState.WIN: m = Move.REPETITION_WIN; break;
                    default: break;
                }
            }
            // 人間が入玉局面の条件を満たしているなら自動的に入玉局面して勝ちとする。
            // コンピューターの時は、これをやってしまうとコンピューターが入玉宣言の指し手(Move.WIN)をちゃんと指せているかの
            // チェックが出来なくなってしまうので、コンピューターの時はこの処理を行わない。
            else if (isHuman && rule != EnteringKingRule.TRY_RULE && position.DeclarationWin(rule) == Move.WIN)
            {
                m = Move.WIN;
            }

            return m;
        }

        /// <summary>
        /// 対局していないときにUI上の操作で駒を動かす。
        ///
        /// DoMove()に成功したならtrue。駒を動かせなかったならfalseが返る。
        /// </summary>
        /// <param name="m"></param>
        public bool DoMoveUI(Move m , MiscSettings misc)
        {
            if (!position.IsLegal(m))
                return false;

            if (IsSpecialNode())
            {
                // current nodeは、special moveによって到達したnodeであった。
                // このことからlastMove() != nullが言える。
                var last = LastMove();
                var sm = last.nextMove;
                if (!(sm == Move.RESIGN || sm == Move.ILLEGAL_MOVE || sm == Move.INTERRUPT || sm == Move.TIME_UP))
                    return false;

                // Move.DRAWとかMAX_MOVES_DRAWとかは、削除しても再度このnodeに到達してしまうのでこのspecial moveの
                // nodeで動かすわけにはいかない。

                // 削除して問題なさげなので、このnodeを削除して、前の局面に戻れば良い。
                UndoMove();

                // この枝は、上記のRESIGNなどの枝なので、削除しておく。
                currentNode.moves.Remove(last);
            }
            else if (IsNextNodeSpecialNode(true, misc) != Move.NONE)
                return false;
                // このnodeはspecial nodeではないが、すでに千日手成立局面などに到達しているのであれば、
                // この局面では何も出来ない。

            var node_existed = currentNode.moves.Exists((x) => x.nextMove == m);
            // 現在の棋譜上の指し手なので棋譜ウィンドウの更新は不要である。
            if (node_existed && kifuWindowMoves[pliesFromRoot].nextMove == m)
            {
                DoMove(m);
                return true;
            }

            PropertyChangedEventEnable = false;

            // 新規nodeなので棋譜クリア
            ClearKifuForward();

            var e = EnableKifuList;
            EnableKifuList = true;

            // 合法っぽいので受理して次の局面に行く
            if (node_existed)
            {
                // すでにあるのでそこを辿るだけにする。

                // 現在棋譜ウィンドウの局面でないことは保証されている。
                DoMove(m);

                // このあと、本譜の手順を選んでいき、このnodeに戻る
                int ply = 0;
                for (; currentNode.moves.Count != 0; ++ply)
                    DoMove(currentNode.moves[0]);
                EnableKifuList = false;
                while (ply-- != 0)
                    UndoMove();
            }
            else
            {
                // -- 次のnodeとして存在しなかったのでnodeを生成して局面を移動する
                AddNode(m, KifuMoveTimes.Zero);
                DoMove(m);

            }

            // special nodeに到達してしまった。
            m = IsNextNodeSpecialNode(true, misc);
            if (m != Move.NONE)
            {
                AddNode(m, KifuMoveTimes.Zero);
                DoMove(m);
            }

            // この結果、special nodeに到達する可能性があるが…。

            EnableKifuList = false;
            PropertyChangedEventEnable = true;

            RaisePropertyChanged("KifuList", new List<string>(KifuList));
            RaisePropertyChanged("Position", position.Clone());

            KifuDirty = true; // 新しいnodeに到達したので棋譜は汚れた扱い。

            return true;
        }

        /// <summary>
        /// 現在のnode以降のKifuList,kifuWindowMovesを削除
        /// </summary>
        public void ClearKifuForward()
        {
            if (KifuList.Count - 1 > pliesFromRoot)
            {
                KifuList.RemoveRange(pliesFromRoot + 1, KifuList.Count - (pliesFromRoot + 1));
                kifuWindowMoves.RemoveRange(pliesFromRoot, kifuWindowMoves.Count - pliesFromRoot);
            }
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
            return m.IsOk() ? kifFormatter.format(p, m) :
                kifFormatter.format(p.sideToMove) + m.SpecialMoveToKif().Left(6);
        }

        /// <summary>
        /// DoMove()のときに棋譜に追加する
        /// </summary>
        /// <param name="m"></param>
        private void AddKifu(KifuMove move, TimeSpan t)
        {
            var m = move.nextMove;
            if (EnableKifuList)
            {
                // rootNodeからの指し手。これは棋譜リストと同期させている。
                kifuWindowMoves.Add(move);

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

                    if (plus == '*') // 本譜ではないところを選んでいる
                    {
                        // 最初の分岐であるか。
                        if (KifuBranch == -1 || (KifuList.Count < KifuBranch))
                            KifuBranch = KifuList.Count;
                    } else if (plus == '+')
                    {
                        // 本譜の手順に変わったので分岐ではない。
                        if (KifuBranch == KifuList.Count)
                            KifuBranch = -1;
                    }
                } else
                    plus = ' ';

                // 分岐棋譜の場合、分岐以降はインデントを入れる。
                var indent = "";
                if (KifuBranch != -1 && KifuBranch <= KifuList.Count)
                    indent = ">";

                var move_text = move_to_kif_string(position, m);
                var move_text_game_ply = position.gamePly;

                // 消費時間の文字列「1秒」のように短めの文字列で表現。
                // 一番上の桁は、そのあとのPadMidUnicode()でpaddingされるので、PadLeft的なpaddingはしないでおく。
                string time_string;
                if (t.TotalSeconds < 60)
                    time_string = $"{t.Seconds}秒";
                else if (t.TotalMinutes < 60)
                    time_string = $"{t.Minutes}分{t.Seconds,2}秒";
                else if (t.TotalHours < 24)
                    time_string = $"{t.Hours}時間{t.Minutes,2}分{t.Seconds,2}秒";
                else
                    time_string = $"{t.Days}日{t.Hours,2}時間{t.Minutes,2}分{t.Seconds,2}秒";

                var move_time = move_text.PadMidUnicode( time_string , 12 /*指し手=最大全角6文字=半角12文字*/ + 1 /* space */+ 7 /*時間文字列、1分00秒で半角7文字*/);

                var text = $"{indent}{plus}{move_text_game_ply, 3}.{move_time}";

                KifuList.Add(text);
                RaisePropertyChanged("KifuListAdded", text /*末尾が変更になった。変更になった行を送る。*/);

                // -- この内容をLastKifuStringに反映させる。

                // special moveは除外
                if (m.IsOk())
                {
                    var kifu_version = TheApp.app.Config.KifuWindowKifuVersion;
                    switch (kifu_version)
                    {
                        // 同じ形式なので棋譜ウィンドウに表示しているものをそのまま代入しておけば良い。
                        case 0: LastKifuString = move_text; break;

                        case 1:
                        case 2:
                        case 3:
                            LastKifuString = KifFormatter.Ki2C.format(position, m);
                            break;

                        default:
                            Debug.Assert(false);break;
                    }
                }

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
                // rootNodeからの指し手。これは棋譜リストと同期させている。
                kifuWindowMoves.RemoveAt(kifuWindowMoves.Count - 1);

                // 棋譜ウィンドウに表示する用の文字列
                KifuList.RemoveAt(KifuList.Count - 1);
                RaisePropertyChanged("KifuListRemoved", null /*末尾が削除になった*/);
            }

            if (EnableUsiMoveList)
                UsiMoveList.RemoveAt(UsiMoveList.Count - 1); // RemoveLast()
        }

        /// <summary>
        /// 詰み判定のための指し手生成バッファ
        /// </summary>
        private Move[] moves = new Move[(int)Move.MAX_MOVES];
    }
}
