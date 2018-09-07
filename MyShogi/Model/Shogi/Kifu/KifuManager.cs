using System;
using System.Collections.Generic;
using System.Diagnostics;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 1局の対局棋譜を全般的に管理するクラス
    /// ・分岐棋譜をサポート
    /// ・着手時間をサポート
    /// ・対局相手の名前をサポート
    /// ・KIF/KI2/CSA/SFEN/PSN/JSON形式での入出力をサポート
    /// ・千日手の管理、検出をサポート
    /// </summary>
    public partial class KifuManager
    {
        // -------------------------------------------------------------------------
        // public members
        // -------------------------------------------------------------------------

        /// <summary>
        /// 棋譜本体。分岐棋譜。
        /// </summary>
        public KifuTree Tree = new KifuTree();

        // -- 以下、棋譜絡み。

        /// <summary>
        /// KIF2形式の棋譜リストを常に生成する。
        /// これをtrueにする KifuList というpropertyが有効になる。
        ///
        /// デフォルト : true
        ///
        /// 棋譜の読み込み前に設定を行うこと。
        /// </summary>
        public bool EnableKifuList
        {
            get { return Tree.EnableKifuList; }
            set { Tree.EnableKifuList = value; }
        }

        /// <summary>
        /// 現局面までの棋譜。
        /// 対局ウィンドウの棋譜ウィンドウにdata bindでそのまま表示できる形式。
        ///
        /// EnableKifuListがtrueのとき、DoMove()/UndoMove()するごとにリアルタイムに更新される。
        /// </summary>
        public List<string> KifuList
        {
            get { return Tree.KifuList; }
            set { Tree.KifuList = value; }
        }

        /// <summary>
        /// USIの指し手文字列の形式の棋譜リストを常に生成する。
        /// これをtrueにする EnableUsiMoveList というpropertyが有効になる。
        ///
        /// デフォルト : true
        ///
        /// 棋譜の読み込み前に設定を行うこと。
        /// </summary>
        public bool EnableUsiMoveList
        {
            get { return Tree.EnableUsiMoveList; }
            set { Tree.EnableUsiMoveList = value; }
        }

        /// <summary>
        /// 現局面までの棋譜。USIの指し手文字列
        /// EnableUsiMoveListがtrueのとき、DoMove()/UndoMove()するごとにリアルタイムに更新される。
        /// これをstring.Join(" ",UsiMoveList)すると"position"に渡す文字列が出来上がる。
        /// cf. UsiPositionString()
        /// </summary>
        public List<string> UsiMoveList
        {
            get { return Tree.UsiMoveList; }
            set { Tree.UsiMoveList = value; }
        }

        /// <summary>
        /// USIの"position"コマンドで用いる局面図
        /// </summary>
        public string UsiPositionString
        {
            get { return Tree.UsiPositionString; }
        }

        /// <summary>
        /// このクラスが保持しているPosition。これはDoMove()/UndoMove()に対して変化するのでimmutableではない。
        /// data bindするならば、Tree.Positionにbindして用いること。
        ///
        /// また、このクラスが生成された時点では、局面は初期化されていないので、何らかの方法で初期化してから用いること。
        /// </summary>
        public Position Position { get { return Tree.position; } }

        /// <summary>
        /// 棋譜のヘッダにある対局者氏名などを格納するクラス
        /// </summary>
        public KifuHeader KifuHeader { get { return Tree.KifuHeader; } }

        // -------------------------------------------------------------------------
        // public methods
        // -------------------------------------------------------------------------

        /// <summary>
        /// このクラスを初期化する。new KifuManager()とした時の状態になる。
        /// </summary>
        public void Init()
        {
            KifuHeader.Init();
            Tree.Init();

            // Tree.EnableKifuList == falseだと棋譜リストは初期化されない。
            // 強制的に初期化したいので明示的にリセットする。
            if (!EnableKifuList)
                Tree.ResetKifuList();
        }

        /// <summary>
        /// 指し手で局面を進める。
        /// </summary>
        /// <param name="m"></param>
        public void DoMove(Move m)
        {
            Tree.DoMove(m);
        }

        /// <summary>
        /// 指し手で局面を戻す。
        /// </summary>
        public void UndoMove()
        {
            Tree.UndoMove();
        }

        /// <summary>
        /// 対局中の「待った」用のUndo
        /// 棋譜から、現局面への指し手を削除してのUndo
        ///
        /// 削除したならtrueが返る。
        /// </summary>
        public bool UndoMoveInTheGame()
        {
            var node = Tree.currentNode;
            if (node.prevNode == null)
                return false;

            // undoできる

            // Tree.UndoMove()で棋譜1行削除するが、このときにEnableKifuList == trueでないと削除されない。
            Debug.Assert(EnableKifuList);

            Tree.UndoMove();

            //Tree.Remove(node); // この枝を削除しておく。
            // → 全部の枝を削除すべきでは..。枝が残ったままで棋譜の表示だけ消えているのは不整合なのでは。
            Tree.RemoveNextNode();

            return true;
        }

        /// <summary>
        /// 末尾の指し手に移動する
        /// </summary>
        public void GotoLastMove()
        {
            // 本譜の手順を選んでいけば末尾まで行けることが保証されている。
            while (Tree.currentNode.moves.Count != 0)
                Tree.DoMove(Tree.currentNode.moves[0].nextMove);
        }

        // -- 以下、棋譜処理

        /// <summary>
        /// 盤面を特定の局面で初期化する。
        /// </summary>
        /// <param name="boardType"></param>
        public void InitBoard(BoardType boardType)
        {
            Tree.SetRootBoardType(boardType);
        }

        /// <summary>
        /// string中に現れる文字をカウントする
        /// </summary>
        private static int CountKeywrod(string str, char ch)
        {
            return str.Length - str.Replace(ch.ToString(), "").Length;
        }

        /// <summary>
        /// 棋譜ファイルを読み込む。
        /// this.Treeに反映する。また本譜の手順の最終局面までthis.Tree.posを自動的に進める。
        /// フォーマットは自動判別。
        /// CSA/KIF/KI2/PSN/SFEN形式を読み込める。
        ///
        /// ファイル丸ごと読み込んでstring型に入れて引数に渡すこと。
        /// 読み込めたところまでの棋譜を反映させる。読み込めなかった部分やエラーなどは無視する。
        ///
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければnullが返る。
        /// </summary>
        /// <param name="filename"></param>
        public string FromString(string content /* , KifuFileType kf */)
        {
            // なるべくファジーに判定する

            /// <summary>
            /// 文字列がsfenっぽいならtrue
            /// (position ){,1}((sfen ){,1}(.*\/){8})|(startpos)
            /// 大文字小文字の差異も許容しておく
            /// </summary>
            bool isSfen(string line)
            {
                string[] token = line.Split(' ');
                if (token.Length <= 3) { return false; }
                int i = 0;
                if (token[i].ToLower() == "position") { ++i; } // 先頭のpositionは任意
                if (token[i].ToLower() == "startpos") { return true; } // 平手の開始局面を表すsfenだった
                if (token[i].ToLower() == "sfen") { ++i; } // sfenも必須ではないようにする
                if (CountKeywrod(token[i], '/') == 8) { return true; } // 最初のトークンで/が8個ならおそらくsfenだろう
                return false;
            }

            var e = Tree.PropertyChangedEventEnable;

            try
            {
                // イベントの一時抑制
                Tree.PropertyChangedEventEnable = false;

                Init();

                // 棋譜リストの更新をしないモードだと初期化されないので棋譜リストを明示的に初期化する。
                if (!EnableKifuList)
                    Tree.ResetKifuList();

                var lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                if (lines.Length == 0)
                    return "棋譜が空でした。";

                var line = lines[0];

                // sfen形式なのか？
                if (isSfen(line))
                    return FromSfenString(line);

                // PSN形式なのか？
                if (line.StartsWith("[Sente"))
                    return FromPsnString(lines, KifuFileType.PSN);

                // PSN2形式なのか？
                if (line.StartsWith("[Black"))
                    return FromPsnString(lines, KifuFileType.PSN2);

                // CSA形式なのか？
                if (line.StartsWith("V2") || line.StartsWith("N+")) // 将棋所だと"V2.2"など書いてあるはず。バージョンなしでも許容する。
                    return FromCsaString(lines, KifuFileType.CSA);

                // JSON形式なのか？
                if (line.StartsWith("{"))
                    return FromJsonString(content, KifuFileType.JSON);

                // KIF/KI2形式なのか？
                if (line.StartsWith("#") || line.IndexOf("：") > 0 || line.StartsWith("後手の持駒") || line.IndexOf("▲") > 0 || line.IndexOf("△") > 0)
                    return FromKifString(lines, KifuFileType.KIF);

                return "棋譜の形式が判別できませんでした。";
            }
            finally
            {
                // -- 本譜の手順の末尾に移動。

                Tree.RewindToRoot();

                GotoLastMove();

                // イベントの一時抑制を解除して、更新通知を送る。
                Tree.PropertyChangedEventEnable = e;
                Tree.RaisePropertyChanged("KifuList",new List<string>(Tree.KifuList));
                Tree.RaisePropertyChanged("Position",Tree.position.Clone());

            }
        }

        /// <summary>
        /// 読み込み形式手動指定、とりあえず各形式のルーチンを直接テストするため。
        /// </summary>
        /// <param name="content"></param>
        /// <param name="kf"></param>
        /// <returns></returns>
        public string FromString(string content, KifuFileType kf)
        {
            Init();

            var lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            switch(kf)
            {
                case KifuFileType.SFEN:
                    return FromSfenString(content);
                case KifuFileType.CSA:
                    return FromCsaString(lines, kf);
                case KifuFileType.KIF:
                case KifuFileType.KI2:
                    return FromKifString(lines, kf);
                case KifuFileType.PSN:
                case KifuFileType.PSN2:
                    return FromPsnString(lines, kf);
                case KifuFileType.JKF:
                    return FromJkfString(content, kf);
                case KifuFileType.JSON:
                    return FromLiveJsonString(content, kf);
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 棋譜ファイルを書き出す
        /// フォーマットは引数のkfで指定する。
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="kf"></param>
        public string ToString(KifuFileType kt)
        {
            // -- イベントの一時抑制

            // KifuListの更新通知がいっぱい発生すると棋譜ウィンドウが乱れるため。
            var e1 = Tree.PropertyChangedEventEnable;
            Tree.PropertyChangedEventEnable = false;

            // 棋譜ウィンドウを操作してはならないので棋譜ウィンドウとのsyncを停止させておく。
            var e2 = Tree.EnableKifuList;
            Tree.EnableKifuList = false;

            // 局面をrootに移動(あとで戻す)
            var moves = Tree.RewindToRoot();

            string result = string.Empty;
            switch(kt)
            {
                case KifuFileType.SFEN:
                    result = ToSfenString();
                    break;

                case KifuFileType.PSN:
                case KifuFileType.PSN2:
                    result = ToPsnString(kt);
                    break;

                case KifuFileType.CSA:
                    result = ToCsaString();
                    break;

                case KifuFileType.KIF:
                    result = ToKifString();
                    break;

                case KifuFileType.KI2:
                    result = ToKi2String();
                    break;

                case KifuFileType.JKF:
                    result = ToJkfString();
                    break;

                case KifuFileType.JSON:
                    result = ToJsonString();
                    break;

                case KifuFileType.SVG:
                    // SVG形式は局面出力のみで棋譜には対応しないが、一応書いておく
                    result = ToSvgPositionString();
                    break;

                // ToDo : 他の形式もサポートする
            }

            // 呼び出された時の局面に戻す
            Tree.RewindToRoot();
            Tree.FastForward(moves);

            Tree.EnableKifuList = e2;
            Tree.PropertyChangedEventEnable = e1;

            return result;
        }

        /// <summary>
        /// 局面文字列を書き出す
        /// フォーマットは引数のkfで指定する。
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="kf"></param>
        public string ToPositionString(KifuFileType kt)
        {
            string result = string.Empty;
            switch(kt)
            {
                // 局面出力の専用ルーチンを使用
                case KifuFileType.KIF:
                case KifuFileType.KI2:
                    result = ToKifPositionString(kt);
                    break;
                case KifuFileType.SVG:
                    result = ToSvgPositionString();
                    break;

                // 局面出力を用意していないものは、棋譜書き出しルーチンを流用して出力する
                default:
                    var kifu = new KifuManager();
                    // 経路を消すためにsfen化して代入しなおして書き出す
                    kifu.FromString($"sfen {Position.ToSfen()}");
                    kifu.KifuHeader.PlayerNameBlack = KifuHeader.PlayerNameBlack;
                    kifu.KifuHeader.PlayerNameWhite = KifuHeader.PlayerNameWhite;
                    result = kifu.ToString(kt);
                    break;

                // ToDo : 他の形式もサポートする
            }

            return result;
        }

        /// <summary>
        /// ply手目のselect個目の指し手分岐をのちほど選択するの意味。
        /// 分岐棋譜を辿るときに分岐しているnodeをあとで訪問するために必要となる。
        /// </summary>
        private struct Node
        {
            public int ply;
            public int select;
            public Node(int ply_, int select_) { ply = ply_; select = select_; }
        };
    }
}
