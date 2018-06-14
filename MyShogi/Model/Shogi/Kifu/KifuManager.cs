using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 1局の対局棋譜を全般的に管理するクラス
    /// ・分岐棋譜をサポート
    /// ・着手時間をサポート
    /// ・対局相手の名前をサポート
    /// ・KIF/KI2/CSA/SFEN/PSN形式での入出力をサポート
    /// ・千日手の管理、検出をサポート
    /// </summary>
    public class KifuManager
    {
        // -------------------------------------------------------------------------
        // public members
        // -------------------------------------------------------------------------

        /// <summary>
        /// デフォルトの先手対局者名。
        /// </summary>
        static public string defaultPlayerNameBlack = "先手";
        /// <summary>
        /// デフォルトの後手対局者名。
        /// </summary>
        static public string defaultPlayerNameWhite = "後手";

        /// <summary>
        /// 対局ヘッダ情報。
        /// キー文字列は柿木形式で使われているヘッダのキー文字列に準ずる。
        /// </summary>
        public Dictionary<string, string> header = new Dictionary<string, string>()
        {
            { "先手", defaultPlayerNameBlack },
            { "後手", defaultPlayerNameWhite }
        };

        /// <summary>
        /// 先手/下手 対局者名。
        ///   playerNameBlack : 先手の名前(駒落ちの場合、下手)
        /// </summary>
        public string playerNameBlack {
            get
            {
                string name;
                return header.TryGetValue("先手", out name) ? name : defaultPlayerNameBlack;
            } set {
                header["先手"] = value;
            }
        }
        /// <summary>
        /// 後手/上手 対局者名。
        ///   playerNameWhite : 後手の名前(駒落ちの場合、上手)
        /// </summary>
        public string playerNameWhite {
            get
            {
                string name;
                return header.TryGetValue("後手", out name) ? name : defaultPlayerNameWhite;
            }
            set
            {
                header["後手"] = value;
            }
        }

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
        /// data bindするならば、これをClone()して用いること。
        ///
        /// また、このクラスが生成された時点では、局面は初期化されていないので、何らかの方法で初期化してから用いること。
        /// </summary>
        public Position Position { get { return Tree.position; } }

        // -------------------------------------------------------------------------
        // public methods
        // -------------------------------------------------------------------------

        /// <summary>
        /// このクラスを初期化する。new KifuManager()とした時の状態になる。
        /// </summary>
        public void Init()
        {
            header = new Dictionary<string, string>()
            {
                { "先手", defaultPlayerNameBlack },
                { "後手", defaultPlayerNameWhite }
            };
            //playerName = new string[2];
            Tree.Init();
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
        /// </summary>
        public bool UndoMoveInTheGame()
        {
            var node = Tree.currentNode;
            if (node.prevNode == null)
                return false;

            // undoできる
            Tree.UndoMove();
            Tree.Remove(node); // この枝を削除しておく。

            return true;
        }

        // -- 以下、棋譜処理

        /// <summary>
        /// 盤面を特定の局面で初期化する。
        /// </summary>
        /// <param name="boardType"></param>
        public void InitBoard(BoardType boardType)
        {
            var sfen = boardType.ToSfen();

            Tree.position.SetSfen(sfen);
            Tree.rootBoardType = boardType;

            // rootSfenを更新したときにイベント通知が起きるので、これを最後にしている。
            Tree.rootSfen = sfen;
        }

        /// <summary>
        /// 棋譜ファイルを読み込む。
        /// this.Treeに反映する。また最終局面までthis.Tree.posを自動的に進める。
        /// フォーマットは自動判別。
        /// CSA/KIF/KI2/PSN/SFEN形式を読み込める。
        ///
        /// ファイル丸ごと読み込んでstring型に入れて引数に渡すこと。
        /// 読み込めたところまでの棋譜を反映させる。読み込めなかった部分やエラーなどは無視する。
        ///
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        /// <param name="filename"></param>
        public string FromString(string content /* , KifuFileType kf */)
        {
            Init();

            var lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (lines.Length == 0)
                return "棋譜が空でした。";
            var line = lines[0];

            // sfen形式なのか？
            if (line.StartsWith("sfen") || line.StartsWith("startpos"))
                return FromSfenString(line);

            // PSN形式なのか？
            if (line.StartsWith("[Sente"))
                return FromPsnString(lines , KifuFileType.PSN);

            // PSN2形式なのか？
            if (line.StartsWith("[BLACK"))
                return FromPsnString(lines , KifuFileType.PSN2);

            // CSA形式なのか？
            if (line.StartsWith("V2")) // 将棋所だと"V2.2"など書いてあるはず。
                return FromCsaString(lines, KifuFileType.CSA);

            // JSON形式なのか？
            if (line.StartsWith("{"))
                return FromJsonString(content, KifuFileType.JSON);

            // KIF/KI2形式なのか？
            if (line.StartsWith("#") || line.IndexOf("：") > 0)
                return FromKifString(lines, KifuFileType.KIF);

            return string.Empty;
        }

        // 読み込み形式手動指定、とりあえず各形式のルーチンを直接テストするため。
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

                // ToDo : 他の形式もサポートする
            }

            // 呼び出された時の局面に戻す
            Tree.RewindToRoot();
            Tree.FastForward(moves);

            return result;
        }

        // -------------------------------------------------------------------------
        // private members
        // -------------------------------------------------------------------------

        /// <summary>
        /// sfen文字列のparser
        /// USIプロトコルの"position"コマンドで使う文字列を読み込む。
        /// 便宜上、ここではsfenと呼んでいるが、本来はsfenには"moves .."は含まれない。
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        /// <param name="sfen"></param>
        private string FromSfenString(string sfen)
        {
            try
            {
                // Position.UsiPositionCmd()からの移植。

                // Scannerを用いた字句解析
                var scanner = new Scanner(sfen);

                //// どうなっとるねん..
                //if (split.Length == 0)
                //    return;

                if (scanner.PeekText() == "sfen")
                {
                    scanner.ParseText();

                    // "sfen ... moves ..."形式かな..
                    // movesの手前までをくっつけてSetSfen()する
                    var sfen_pos = new List<string>();
                    while (scanner.PeekText() != "moves")
                    {
                        if (sfen_pos.Count >= 4)
                            break;

                        sfen_pos.Add(scanner.ParseText());
                    }

                    Tree.rootSfen = string.Join(" ", sfen_pos.ToArray());
                    Tree.position.SetSfen(Tree.rootSfen);
                    Tree.rootBoardType = BoardType.Others;
                }
                else if (scanner.PeekText() == "startpos")
                {
                    scanner.ParseText();

                    Tree.rootSfen = Position.SFEN_HIRATE;
                    Tree.position.SetSfen(Tree.rootSfen);
                    Tree.rootBoardType = BoardType.NoHandicap;
                }

                // "moves"以降の文字列をUSIの指し手と解釈しながら、局面を進める。
                if (scanner.PeekText() == "moves")
                {
                    scanner.ParseText();
                    int ply = 1;
                    while (scanner.PeekText() != null)
                    {
                        // デバッグ用に盤面を出力
                        //Console.WriteLine(Pretty());

                        var move = Util.FromUsiMove(scanner.ParseText());
                        if (!Tree.position.IsLegal(move))
                            // throw new PositionException(string.Format("{0}手目が非合法手です。", i - cur_pos));
                            return string.Format("{0}手目が非合法手です。", ply);

                        // この指し手で局面を進める
                        Tree.AddNode(move, new TimeSpan());
                        Tree.DoMove(move);
                        ++ply;
                    }
                }
            } catch (Exception e)
            {
                return e.Message;
            }

            return string.Empty;
        }

        /// <summary>
        /// PSN形式の棋譜ファイルのparser
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        private string FromPsnString(string[] lines , KifuFileType kf)
        {
            var lineNo = 1;

            var r1 = new Regex(@"\[([^\s]+)\s*""(.*)""\]");

            for (; lineNo <= lines.Length; ++lineNo)
            {
                var line = lines[lineNo - 1];
                var m1 = r1.Match(line);
                if (m1.Success)
                {
                    var token = m1.Groups[1].Value;
                    var body = m1.Groups[2].Value;

                    switch (token)
                    {
                        case "Sente":
                        case "BLACK":
                            playerNameBlack = body;
                            break;

                        case "Gote":
                        case "WHITE":
                            playerNameWhite = body;
                            break;

                        case "SFEN":
                            // 将棋所で出力したPSNファイルはここに必ず"SFEN"が来るはず。平手の局面であっても…。
                            // 互換性のためにも、こうなっているべきだろう。

                            Tree.rootSfen = body;
                            Tree.position.SetSfen(body);
                            Tree.rootBoardType = BoardType.Others;
                            break;
                    }
                }
                else
                    break;
            }

            // PSNフォーマットのサイトを見ると千日手とか宣言勝ちについて規定がない。
            // どう見ても欠陥フォーマットである。
            // http://genedavis.com/articles/2014/05/09/psn/

            // PSN2フォーマットを策定した
            // cf. https://github.com/yaneurao/MyShogi/blob/master/docs/PSN2format.md

            // -- そこ以降は指し手文字列などが来るはず..

            // e.g.
            // 1.P7g-7f          (00:03 / 00:00:03)
            // 2.P5c-5d          (00:02 / 00:00:02)
            // 3.B8hx3c+         (00:03 / 00:00:06)
            // 5.B*4e            (00:01 / 00:00:04)
            // 15.+B4d-3c        (00:01 / 00:00:12)
            // 16.Sennichite     (00:01 / 00:00:10)

            // 9.Resigns         (00:03 / 00:00:08)

            // 駒種(成り駒は先頭に"+")、移動元、移動先をUSI文字列で書いて、captureのときは"x"、非captureは"-"
            // 成りは末尾に"+"が入る。駒打ちは"*"

            // 入玉宣言勝ちは将棋所では次のようになっているようだ。
            // 75.Jishogi        (00:02 / 00:00:44)
            // {
            // 入玉宣言により勝ち。
            // }

            // 変化手順の表現
            // 6手目からの変化手順の場合
            // Variation:6
            // 6.K5a-6b          (00:02 / 00:00:04)

            // 指し手の正規表現
            var r4 = new Regex(@"(\d+)\.([^\s]+)\s*\((.+?)\s*\/\s*(.+)\)");
            // 正規表現のデバッグ難しすぎワロタ
            // 正規表現デバッグ用の神サイトを使う : https://regex101.com/

            // 変化手順
            var r5 = new Regex(@"Variation:(\d+)");

            var moves = new Move[(int)Move.MAX_MOVES];

            for (; lineNo <= lines.Length; ++lineNo)
            {
                var line = lines[lineNo - 1];

                // コメントブロックのスキップ
                // "{"で始まる行に遭遇したら、"}"で終わる行まで読み飛ばす
                if (line.StartsWith("{"))
                {
                    for (++lineNo; lineNo <= lines.Length; ++lineNo)
                    {
                        line = lines[lineNo - 1];
                        if (line.EndsWith("}"))
                            break;
                    }
                    continue;
                }

                // 変化手順
                var m5 = r5.Match(line);
                if (m5.Success)
                {
                    // 正規表現で数値にマッチングしているのでこのparseは100%成功する。
                    int ply = int.Parse(m5.Groups[1].Value);

                    // ply手目まで局面を巻き戻す
                    while (ply < Tree.ply)
                        Tree.UndoMove();

                    continue;
                }

                var m4 = r4.Match(line);
                if (m4.Success)
                {
                    // 正規表現で数値にマッチングしているのでこのparseは100%成功する。
                    var ply2 = int.Parse(m4.Groups[1].Value);

                    // ply1 == ply2のはずなのだが…。
                    // まあいいか…。

                    var move_string = m4.Groups[2].Value;
                    var time_string1 = m4.Groups[3].Value;
                    var time_string2 = m4.Groups[4].Value;

                    Move move = Move.NONE;

                    // move_stringが"Sennichite"などであるか。
                    if (kf == KifuFileType.PSN)
                        switch (move_string)
                        {
                            case "Sennichite":
                                // どちらが勝ちかはわからない千日手
                                move = Move.REPETITION;
                                goto FINISH_MOVE_PARSE;

                            case "Resigns":
                                move = Move.RESIGN;
                                goto FINISH_MOVE_PARSE;

                            case "Interrupt":
                                move = Move.INTERRUPT;
                                goto FINISH_MOVE_PARSE;

                            case "Mate":
                                move = Move.MATED;
                                goto FINISH_MOVE_PARSE;

                            case "Jishogi":
                                // 入玉宣言勝ちなのか最大手数による引き分けなのか不明
                                move = Move.WIN;
                                goto FINISH_MOVE_PARSE;

                            case "Timeup":
                                move = Move.TIME_UP;
                                goto FINISH_MOVE_PARSE;
                        }
                    else if (kf == KifuFileType.PSN2)
                        switch (move_string)
                        {
                            case "Resigns":
                                move = Move.RESIGN;
                                goto FINISH_MOVE_PARSE;

                            case "Interrupt":
                                move = Move.INTERRUPT;
                                goto FINISH_MOVE_PARSE;

                            case "Mate":
                                move = Move.MATED;
                                goto FINISH_MOVE_PARSE;

                            case "Timeup":
                                move = Move.TIME_UP;
                                goto FINISH_MOVE_PARSE;

                            case "MaxMovesDraw":
                                move = Move.MAX_MOVES_DRAW;
                                goto FINISH_MOVE_PARSE;

                            case "DeclarationWin":
                                move = Move.WIN;
                                goto FINISH_MOVE_PARSE;

                            case "RepetitionDraw":
                                move = Move.REPETITION_DRAW;
                                goto FINISH_MOVE_PARSE;

                            case "RepetitionWin":
                                move = Move.REPETITION_WIN;
                                goto FINISH_MOVE_PARSE;
                        }

                    if (kf == KifuFileType.PSN2)
                    {
                        // PSN2ならparse簡単すぎワロタ
                        move = Util.FromUsiMove(move_string);
                        goto FINISH_MOVE_PARSE;
                    }

                    int seek_pos = 0;
                    // 1文字ずつmove_stringから切り出す。終端になると'\0'が返る。
                    char get_char()
                    {
                        return seek_pos < move_string.Length ? move_string[seek_pos++] : '\0';
                    }
                    // 1文字先読みする。終端だと'\0'が返る
                    char peek_char()
                    {
                        return seek_pos < move_string.Length ? move_string[seek_pos] : '\0';
                    }

                    bool promote_piece = false;
                    // 先頭の"+"は成り駒の移動を意味する
                    if (peek_char() == '+')
                    {
                        get_char();
                        promote_piece = true;
                    }
                    char piece_name = get_char();
                    var pc = Util.FromUsiPiece(piece_name);
                    if (pc == Piece.NO_PIECE)
                        return string.Format("PSN形式の{0}行目の指し手文字列の駒名がおかしいです。", lineNo);
                    pc = pc.PieceType();

                    bool drop_move = false;
                    if (peek_char() == '*')
                    {
                        get_char();
                        drop_move = true;
                        if (promote_piece)
                            return string.Format("PSN形式の{0}行目の指し手文字列で成駒を打とうとしました。", lineNo);
                    }

                    // 移動元の升
                    var c1 = get_char();
                    var c2 = get_char();
                    var from = Util.FromUsiSquare(c1,c2);
                    if (from == Square.NB)
                        return string.Format("PSN形式の{0}行目の指し手文字列の移動元の表現がおかしいです。", lineNo);

                    if (drop_move)
                    {
                        // この升に打てばOk.
                        move = Util.MakeMoveDrop(pc, from);
                        goto FINISH_MOVE_PARSE;
                    }

                    //bool is_capture = false;
                    if (peek_char() == '-')
                    {
                        get_char();
                    } else if (peek_char() == 'x')
                    {
                        get_char();
                        //is_capture = true;
                    }

                    // 移動先の升
                    var c3 = get_char();
                    var c4 = get_char();
                    var to = Util.FromUsiSquare(c3, c4);
                    if (to == Square.NB)
                        return string.Format("PSN形式の{0}行目の指し手文字列の移動先の表現がおかしいです。", lineNo);

                    bool is_promote = false;
                    if (peek_char() == '+')
                        is_promote = true;

                    move = !is_promote ? Util.MakeMove(from, to) : Util.MakeMovePromote(from, to);

                    // この指し手でcaptureになるかどうかだとか
                    // 移動元の駒が正しいかを検証する必要があるが、
                    // 非合法手が含まれる可能性はあるので、それは無視する。

                    // ここで指し手のパースは終わり。
                    FINISH_MOVE_PARSE:;

                    // 消費時間、総消費時間のparse。これは失敗しても構わない。
                    // 消費時間のほうはmm:ssなのでhh:mm:ss形式にしてやる。
                    if (time_string1.Length <= 5)
                        time_string1 = "00:" + time_string1;
                    TimeSpan.TryParse(time_string1, out TimeSpan thinking_time);
                    TimeSpan.TryParse(time_string2 , out TimeSpan total_time);

                    // -- 千日手の判定

                    var rep = Tree.position.IsRepetition();
                    switch (rep)
                    {
                        case RepetitionState.NONE:
                            break; // do nothing

                        case RepetitionState.DRAW:
                            move = Move.REPETITION_DRAW;
                            break;

                        case RepetitionState.WIN:
                            move = Move.REPETITION_WIN;
                            break;

                        case RepetitionState.LOSE:
                            move = Move.REPETITION_LOSE;
                            break;
                    }

                    if (move == Move.REPETITION)
                    {
                        // 千日手らしいが、上で千日手判定をしているのに、それに引っかからなかった。
                        // おかしな千日手出力であるので、ここ以降の読み込みに失敗させる。

                        return string.Format("PSN形式の{0}行目が千日手となっているが千日手ではないです。", lineNo);
                    }

                    // -- 詰みの判定

                    if (Tree.position.IsMated(moves))
                    {
                        // move == Move.MATEDでないとおかしいのだが中断もありうるので強制的に詰み扱いにしておく。

                        move = Move.MATED;
                    }

                    // -- 持将棋の判定

                    if (move == Move.WIN)
                    {
                        // 持将棋の条件が異なるかも知れないので、この判定はしないことにする。
                        //if (Tree.position.DeclarationWin(EnteringKingRule.POINT27) == Move.WIN)
                        //    return string.Format("PSN形式の{0}行目が持将棋となっているが持将棋ではないです。", lineNo);
                    }

                    // -- DoMove()

                    Tree.AddNode(move , thinking_time , total_time);

                    // 特殊な指し手、もしくはLegalでないならDoMove()は行わない
                    if (move.IsSpecial() || !Tree.position.IsLegal(move))
                    {
                        // まだ次の分岐棋譜があるかも知れないので読み進める
                        continue;
                    }

                    Tree.DoMove(move);

                    continue;

                } else
                {
                    // 空行など、parseに失敗したものは読み飛ばす
                }
            }


            return string.Empty;
        }

        /// <summary>
        /// CSA形式の棋譜ファイルのparser
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        private string FromCsaString(string[] lines, KifuFileType kf)
        {
            var lineNo = 1;

            /*
             * 例)

                V2.2
                N+人間
                N-人間
                P1-KY-KE-GI-KI-OU-KI-GI-KE-KY
                P2 * -HI *  *  *  *  * -KA *
                P3-FU-FU-FU-FU-FU-FU-FU-FU-FU
                P4 *  *  *  *  *  *  *  *  *
                P5 *  *  *  *  *  *  *  *  *
                P6 *  *  *  *  *  *  *  *  *
                P7+FU+FU+FU+FU+FU+FU+FU+FU+FU
                P8 * +KA *  *  *  *  * +HI *
                P9+KY+KE+GI+KI+OU+KI+GI+KE+KY
                P+
                P-
                +
                +7776FU,T3
                -8384FU,T1
            */

            string line = string.Empty;
            var posLines = new List<string>();
            var headFlag = true;
            var move = Move.NONE;
            for (; lineNo <= lines.Length; ++lineNo)
            {
                line = lines[lineNo - 1];

                // コメント文
                if (line.StartsWith("'"))
                {
                    Tree.currentNode.comment += line.Substring(1).TrimEnd('\r', '\n') + "\n";
                    continue;
                }
                // セパレータ検出
                if (line.StartsWith("/"))
                {
                    // "/"だけの行を挟んで、複数の棋譜・局面を記述することができる。
                    // 現時点ではこのに対応せず、先頭の棋譜のみを読み込む。
                    // そもそも初期局面が異なる可能性もあり、Treeを構成できるとは限らないため。
                    break;
                }
                // マルチステートメント検出
                string[] sublines = line.Split(',');
                foreach (var subline in sublines)
                {
                    if (subline.StartsWith("$"))
                    {
                        // ToDo: 各種棋譜情報
                        continue;
                    }
                    if (subline.StartsWith("N+"))
                    {
                        playerNameBlack = subline.Substring(2);
                        continue;
                    }
                    if (subline.StartsWith("N-"))
                    {
                        playerNameWhite = subline.Substring(2);
                        continue;
                    }
                    if (subline.StartsWith("P"))
                    {
                        posLines.Add(subline);
                        continue;
                    }
                    if (subline.StartsWith("+") || subline.StartsWith("-"))
                    {
                        if (headFlag)
                        {
                            // 1回目は局面の先後とみなす
                            headFlag = false;
                            posLines.Add(subline);
                            Tree.rootSfen = CsaExtensions.CsaToSfen(posLines.ToArray());
                            Tree.position.SetSfen(Tree.rootSfen);
                            continue;
                        }
                        // 2回目以降は指し手とみなす
                        // 消費時間の情報がまだないが、取り敢えず追加
                        move = Tree.position.FromCSA(subline);
                        Tree.AddNode(move, new TimeSpan());
                        // 特殊な指し手や不正な指し手ならDoMove()しない
                        if (move.IsSpecial() || !Tree.position.IsLegal(move))
                        {
                            continue;
                        }
                        Tree.DoMove(move);
                        continue;
                    }
                    if (subline.StartsWith("T"))
                    {
                        // 着手時間
                        var state = Tree.position.State();
                        if (state == null)
                        {
                            return string.Format("line {0}: 初期局面で着手時間が指定されました。", lineNo);
                        }
                        var time = long.Parse(subline.Substring(1));
                        // 1手戻って一旦枝を削除する（時間情報を追加できないので）
                        var lastMove = state.lastMove;
                        if (move == lastMove)
                        {
                            Tree.UndoMove();
                        }
                        Tree.Remove(move);
                        // 改めて指し手を追加
                        Tree.AddNode(move, TimeSpan.FromSeconds(time));
                        // 特殊な指し手や不正な指し手ならDoMove()しない
                        if (move.IsSpecial() || !Tree.position.IsLegal(move))
                        {
                            continue;
                        }
                        Tree.DoMove(move);
                        continue;
                    }
                    if (subline.StartsWith("%"))
                    {
                        var match = new Regex("^%[A-Z_+-]+").Match(subline);
                        if (!match.Success)
                        {
                            continue;
                        }
                        switch (match.Groups[0].Value)
                        {
                            case "%TORYO":
                                move = Move.RESIGN;
                                break;
                            case "%CHUDAN":
                                move = Move.INTERRUPT;
                                break;
                            case "%SENNICHITE":
                                move = Move.REPETITION_DRAW;
                                break;
                            case "%TIME_UP":
                                move = Move.TIME_UP;
                                break;
                            case "%JISHOGI":
                                move = Move.MAX_MOVES_DRAW;
                                break;
                            case "%KACHI":
                                move = Move.WIN;
                                break;
                            case "%TSUMI":
                                move = Move.MATED;
                                break;

                            case "%ILLEGAL_MOVE":
                                move = Move.ILLEGAL_MOVE;
                                break;

                            case "%+ILLEGAL_ACTION":
                                move = Tree.position.sideToMove == Color.BLACK ? Move.ILLEGAL_ACTION_LOSE : Move.ILLEGAL_ACTION_WIN;
                                break;

                            case "%-ILLEGAL_ACTION":
                                move = Tree.position.sideToMove == Color.BLACK ? Move.ILLEGAL_ACTION_WIN : Move.ILLEGAL_ACTION_LOSE;
                                break;

                            case "%HIKIWAKE":
                                move = Move.DRAW;
                                break;

                            // 以下、適切な変換先不明
                            case "%FUZUMI":
                            case "%MATTA":
                            case "%ERROR":
                            default:
                                move = Move.NONE;
                                break;
                        }
                        Tree.AddNode(move, new TimeSpan());
                        continue;
                    }
                }
            }
            if (headFlag) // まだ局面図が終わってない
                return string.Format("CSA形式の{0}行目で局面図が来ずにファイルが終了しました。", lineNo);

            return string.Empty;
        }

        // Kif/KI2形式の読み込み
        private string FromKifString(string[] lines, KifuFileType kf)
        {
            // ToDo: ここに実装する
            return string.Empty;
        }

        // JSON形式棋譜の読み込み
        private string FromJsonString(string content, KifuFileType kf)
        {
            switch (kf)
            {
                case KifuFileType.JKF:
                    return FromJkfString(content, kf);
                case KifuFileType.JSON:
                    return FromLiveJsonString(content, kf);
                default:
                    return string.Empty;
            }
        }

        // JSON Kifu Format
        private string FromJkfString(string content, KifuFileType kf)
        {
            try
            {
                var CSA_PIECE = new string[] {
                    "  ","FU","KY","KE","GI","KA","HI","KI",
                    "OU","TO","NY","NK","NG","UM","RY","QU",
                    "  ","FU","KY","KE","GI","KA","HI","KI",
                    "OU","TO","NY","NK","NG","UM","RY","QU",
                };
                var jsonObj = JkfUtil.FromString(content);
                if (jsonObj == null)
                {
                    return "有効なデータが得られませんでした";
                }
                if (jsonObj.header != null)
                {
                    foreach (var key in jsonObj.header.Keys)
                    {
                        var trimedKey = key.Trim(' ', '\t', '\n', '\r', '　', '\x0b', '\x00');
                        header[trimedKey] = jsonObj.header[key];
                    }
                }
                if (jsonObj.initial != null)
                {
                    switch (jsonObj.initial.preset)
                    {
                        case "HIRATE":
                            Tree.rootSfen = Sfens.HIRATE;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.NoHandicap;
                            break;
                        case "KY":
                            Tree.rootSfen = Sfens.HANDICAP_KYO;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.HandicapKyo;
                            break;
                        case "KY_R":
                            Tree.rootSfen = Sfens.HANDICAP_RIGHT_KYO;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.HandicapRightKyo;
                            break;
                        case "KA":
                            Tree.rootSfen = Sfens.HANDICAP_KAKU;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.HandicapKaku;
                            break;
                        case "HI":
                            Tree.rootSfen = Sfens.HANDICAP_HISYA;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.HandicapHisya;
                            break;
                        case "HIKY":
                            Tree.rootSfen = Sfens.HANDICAP_HISYA_KYO;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.HandicapHisyaKyo;
                            break;
                        case "2":
                            Tree.rootSfen = Sfens.HANDICAP_2;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.Handicap2;
                            break;
                        case "3":
                            Tree.rootSfen = Sfens.HANDICAP_3;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.Handicap3;
                            break;
                        case "4":
                            Tree.rootSfen = Sfens.HANDICAP_4;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.Handicap4;
                            break;
                        case "5":
                            Tree.rootSfen = Sfens.HANDICAP_5;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.Handicap5;
                            break;
                        case "5_L":
                            Tree.rootSfen = Sfens.HANDICAP_LEFT_5;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.HandicapLeft5;
                            break;
                        case "6":
                            Tree.rootSfen = Sfens.HANDICAP_6;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.Handicap6;
                            break;
                        case "8":
                            Tree.rootSfen = Sfens.HANDICAP_8;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.Handicap8;
                            break;
                        case "10":
                            Tree.rootSfen = Sfens.HANDICAP_10;
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootBoardType = BoardType.Handicap10;
                            break;
                        case "OTHER":
                            Tree.rootBoardType = BoardType.Others;
                            if (jsonObj.initial.data == null)
                                return "初期局面が指定されていません";
                            var color = jsonObj.initial.data.color == 0 ? Color.BLACK : Color.WHITE;
                            var board = new Piece[81];
                            for (File f = File.FILE_1; f <= File.FILE_9; ++f)
                            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
                            {
                                var sqi = Util.MakeSquare(f, r).ToInt();
                                var p = jsonObj.initial.data.board[f.ToInt(), r.ToInt()];
                                switch (p.color)
                                {
                                    case 0:
                                        switch (p.kind)
                                        {
                                            case "FU": board[sqi] = Piece.B_PAWN; break;
                                            case "KY": board[sqi] = Piece.B_LANCE; break;
                                            case "KE": board[sqi] = Piece.B_KNIGHT; break;
                                            case "GI": board[sqi] = Piece.B_SILVER; break;
                                            case "KA": board[sqi] = Piece.B_BISHOP; break;
                                            case "HI": board[sqi] = Piece.B_ROOK; break;
                                            case "KI": board[sqi] = Piece.B_GOLD; break;
                                            case "OU": board[sqi] = Piece.B_KING; break;
                                            case "TO": board[sqi] = Piece.B_PRO_PAWN; break;
                                            case "NY": board[sqi] = Piece.B_PRO_LANCE; break;
                                            case "NK": board[sqi] = Piece.B_PRO_KNIGHT; break;
                                            case "NG": board[sqi] = Piece.B_PRO_SILVER; break;
                                            case "UM": board[sqi] = Piece.B_HORSE; break;
                                            case "RY": board[sqi] = Piece.B_DRAGON; break;
                                            default: board[sqi] = Piece.NO_PIECE; break;
                                        }
                                        break;
                                    case 1:
                                        switch (p.kind)
                                        {
                                            case "FU": board[sqi] = Piece.W_PAWN; break;
                                            case "KY": board[sqi] = Piece.W_LANCE; break;
                                            case "KE": board[sqi] = Piece.W_KNIGHT; break;
                                            case "GI": board[sqi] = Piece.W_SILVER; break;
                                            case "KA": board[sqi] = Piece.W_BISHOP; break;
                                            case "HI": board[sqi] = Piece.W_ROOK; break;
                                            case "KI": board[sqi] = Piece.W_GOLD; break;
                                            case "OU": board[sqi] = Piece.W_KING; break;
                                            case "TO": board[sqi] = Piece.W_PRO_PAWN; break;
                                            case "NY": board[sqi] = Piece.W_PRO_LANCE; break;
                                            case "NK": board[sqi] = Piece.W_PRO_KNIGHT; break;
                                            case "NG": board[sqi] = Piece.W_PRO_SILVER; break;
                                            case "UM": board[sqi] = Piece.W_HORSE; break;
                                            case "RY": board[sqi] = Piece.W_DRAGON; break;
                                            default: board[sqi] = Piece.NO_PIECE; break;
                                        }
                                        break;
                                    default:
                                        board[sqi] = Piece.NO_PIECE;
                                        break;
                                }
                            }
                            var hands = new Hand[2] { Hand.ZERO, Hand.ZERO };
                            if (jsonObj.initial.data.hands != null && jsonObj.initial.data.hands.Count >= 2)
                            foreach (var c in new Color[] { Color.BLACK, Color.WHITE })
                            {
                                if (jsonObj.initial.data.hands[c.ToInt()] != null)
                                foreach (var p in new Piece[] { Piece.PAWN, Piece.LANCE, Piece.KNIGHT, Piece.SILVER, Piece.GOLD, Piece.BISHOP, Piece.ROOK })
                                {
                                    int value;
                                    if (jsonObj.initial.data.hands[c.ToInt()].TryGetValue(CSA_PIECE[p.ToInt()], out value))
                                    {
                                        hands[c.ToInt()].Add(p, value);
                                    }
                                }
                            }
                            Tree.rootSfen = Position.SfenFromRawdata(board, hands, color, 1);
                            Tree.position.SetSfen(Tree.rootSfen);
                            break;
                        default:
                            return "初期局面が不明です";
                    }
                }
                if (jsonObj.moves != null)
                {
                    Move m = Move.NONE;
                    foreach (var jkfMove in jsonObj.moves)
                    {
                        TimeSpan spend = (jkfMove.time != null && jkfMove.time.now != null) ?
                            new TimeSpan(jkfMove.time.now.h ?? 0, jkfMove.time.now.m, jkfMove.time.now.s):
                            TimeSpan.Zero;
                        if (!string.IsNullOrWhiteSpace(jkfMove.special))
                        {
                            switch (jkfMove.special)
                            {
                                case "TORYO":           m = Move.RESIGN; break;
                                case "CHUDAN":          m = Move.INTERRUPT; break;
                                case "SENNICHITE":      m = Move.REPETITION_DRAW; break;
                                case "TIME_UP":         m = Move.TIME_UP; break;
                                case "JISHOGI":         m = Move.MAX_MOVES_DRAW; break;
                                case "KACHI"  :         m = Move.WIN; break;
                                // 以下、適切な変換先不明
                                case "HIKIWAKE":        m = Move.DRAW; break;
                                case "TSUMI":           m = Move.MATED; break;
                                case "ILLEGAL_MOVE"   : m = Move.ILLEGAL_MOVE; break;
                                case "+ILLEGAL_ACTION": m = Tree.position.sideToMove == Color.BLACK ? Move.ILLEGAL_ACTION_LOSE : Move.ILLEGAL_ACTION_WIN; break;
                                case "-ILLEGAL_ACTION": m = Tree.position.sideToMove == Color.BLACK ? Move.ILLEGAL_ACTION_WIN : Move.ILLEGAL_ACTION_LOSE; break;
                                case "ERROR":
                                case "FUZUMI":
                                case "MATTA":
                                default: m = Move.NONE; break;
                            }
                        }
                        else if (jkfMove.move == null)
                            m = Move.NONE;
                        else if (jkfMove.move.to == null)
                            m = Move.NONE;
                        else if (jkfMove.move.from == null)
                        {
                            File f = (File)(jkfMove.move.to.x - 1);
                            Rank r = (Rank)(jkfMove.move.to.y - 1);
                            if (f < File.FILE_1 || f > File.FILE_9 || r < Rank.RANK_1 || r > Rank.RANK_9)
                                m = Move.NONE;
                            else
                            {
                                Square sq = Util.MakeSquare(f, r);
                                switch (jkfMove.move.piece)
                                {
                                    case "FU": m = Util.MakeMoveDrop(Piece.PAWN, sq); break;
                                    case "KY": m = Util.MakeMoveDrop(Piece.LANCE, sq); break;
                                    case "KE": m = Util.MakeMoveDrop(Piece.KNIGHT, sq); break;
                                    case "GI": m = Util.MakeMoveDrop(Piece.SILVER, sq); break;
                                    case "KI": m = Util.MakeMoveDrop(Piece.GOLD, sq); break;
                                    case "KA": m = Util.MakeMoveDrop(Piece.BISHOP, sq); break;
                                    case "HI": m = Util.MakeMoveDrop(Piece.ROOK, sq); break;
                                    default: m = Move.NONE; break;
                                }
                            }
                        }
                        else
                        {
                            File frF = (File)(jkfMove.move.from.x - 1);
                            Rank frR = (Rank)(jkfMove.move.from.y - 1);
                            File toF = (File)(jkfMove.move.to.x - 1);
                            Rank toR = (Rank)(jkfMove.move.to.y - 1);
                            if (
                                frF < File.FILE_1 || frF > File.FILE_9 ||
                                frR < Rank.RANK_1 || frR > Rank.RANK_9 ||
                                frF < File.FILE_1 || toF > File.FILE_9 ||
                                frR < Rank.RANK_1 || toR > Rank.RANK_9
                            )
                                m = Move.NONE;
                            else
                            {
                                Square frSq = Util.MakeSquare(frF, frR);
                                Square toSq = Util.MakeSquare(toF, toR);
                                if (jkfMove.move.promote == true)
                                    m = Util.MakeMovePromote(frSq, toSq);
                                else
                                    m = Util.MakeMove(frSq, toSq);
                            }
                        }
                        Tree.AddNode(m, spend);
                        if (m.IsSpecial() || !Tree.position.IsLegal(m))
                            break;
                        Tree.DoMove(m);
                    }
                }
                // ToDo: 分岐棋譜を読み込む
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return string.Empty;

        }

        // とりあえずJSON中継棋譜形式に部分対応
        private string FromLiveJsonString(string content, KifuFileType kf)
        {
            try
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
                var jsonObj = LiveJsonUtil.FromString(content);
                if (jsonObj == null || jsonObj.data == null || jsonObj.data.Count == 0)
                {
                    return "有効なデータが得られませんでした";
                }
                // 先頭のデータのみ読み込む
                var data = jsonObj.data[0];
                {
                    // 対局者名
                    if (data.side != "後手")
                    {
                        playerNameBlack = data.player1;
                        playerNameWhite = data.player2;
                    }
                    else
                    {
                        playerNameBlack = data.player2;
                        playerNameWhite = data.player1;
                    }
                }
                if (Tree.rootKifuLog == null)
                {
                    Tree.rootKifuLog = new KifuLog();
                }
                if (data.realstarttime != null)
                {
                    var starttime = epoch.AddMilliseconds((double)data.realstarttime);
                    Tree.rootKifuLog.moveTime = starttime;
                    Tree.rootNode.comment = starttime.ToString("o");
                }
                else if (!string.IsNullOrWhiteSpace(data.starttime))
                {
                    Tree.rootKifuLog.moveTime = DateTime.ParseExact(data.starttime, "s", null);
                }
                foreach (var kif in data.kif)
                {
                    Move move;
                    DateTime? time = null;
                    if (kif.time != null)
                    {
                        time = epoch.AddMilliseconds((double)kif.time);
                    }
                    // 特殊な着手
                    if (kif.frX == null || kif.frY == null || kif.toX == null || kif.toY == null || kif.prmt == null)
                    {
                        switch (kif.move)
                        {
                            case "投了":
                                move = Move.RESIGN;
                                break;
                            case "中断":
                            case "封じ手":
                                move = Move.INTERRUPT;
                                break;
                            default:
                                return string.Empty;
                        }
                    }
                    // varidation
                    else if (kif.frX < 1 || kif.frX > 9 || kif.toY < 0 || kif.toY > 10)
                    {
                        return "無効な移動元座標を検出しました";
                    }
                    else if (kif.toX < 1 || kif.toX > 9 || kif.toY < 1 || kif.toY > 9)
                    {
                        return "無効な移動先座標を検出しました";
                    }
                    // 先手駒台からの着手
                    else if (kif.frY == 10)
                    {
                        Piece pc;
                        switch (kif.frX)
                        {
                            case 1: pc = Piece.PAWN; break;
                            case 2: pc = Piece.LANCE; break;
                            case 3: pc = Piece.KNIGHT; break;
                            case 4: pc = Piece.SILVER; break;
                            case 5: pc = Piece.GOLD; break;
                            case 6: pc = Piece.BISHOP; break;
                            case 7: pc = Piece.ROOK; break;
                            default:
                                return "先手の無効な駒打ちを検出しました";
                        }
                        var toSq = Util.MakeSquare((File)(kif.toX - 1), (Rank)(kif.toY - 1));
                        move = Util.MakeMoveDrop(pc, toSq);
                    }
                    // 後手駒台からの着手
                    else if (kif.frY == 0)
                    {
                        Piece pc;
                        switch (kif.frX)
                        {
                            case 9: pc = Piece.PAWN; break;
                            case 8: pc = Piece.LANCE; break;
                            case 7: pc = Piece.KNIGHT; break;
                            case 6: pc = Piece.SILVER; break;
                            case 5: pc = Piece.GOLD; break;
                            case 4: pc = Piece.BISHOP; break;
                            case 3: pc = Piece.ROOK; break;
                            default:
                                return "後手の無効な駒打ちを検出しました";
                        }
                        var toSq = Util.MakeSquare((File)(kif.toX - 1), (Rank)(kif.toY - 1));
                        move = Util.MakeMoveDrop(pc, toSq);
                    }
                    // 通常の着手
                    else
                    {
                        var frSq = Util.MakeSquare((File)(kif.frX - 1), (Rank)(kif.frY - 1));
                        var toSq = Util.MakeSquare((File)(kif.toX - 1), (Rank)(kif.toY - 1));
                        if (kif.prmt == 1)
                        {
                            move = Util.MakeMovePromote(frSq, toSq);
                        }
                        else
                        {
                            move = Util.MakeMove(frSq, toSq);
                        }
                    }
                    // 棋譜ツリーへの追加処理
                    Tree.AddNode(move, TimeSpan.FromSeconds((double)(kif.spend ?? 0)));
                    if (time != null)
                    {
                        var kifumove = Tree.currentNode.moves.Find((x) => x.nextMove == move);
                        kifumove.moveTime = (DateTime)time;
                        Tree.currentNode.comment = ((DateTime)time).ToString("o");
                    }
                    if (move.IsSpecial())
                    {
                        return string.Empty;
                    }
                    if (!Tree.position.IsLegal(move))
                    {
                        return String.Format("{0}手目で不正着手を検出しました", Tree.position.gamePly);
                    }
                    Tree.DoMove(move);
                    continue;
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 現在の棋譜をUSIプロトコルの"position"コマンドで使う文字列化する。
        /// 便宜上、ここではsfenと呼んでいるが、本来はsfenには"moves .."は含まれない。
        /// 特殊な指し手は出力されない。
        /// </summary>
        /// <returns></returns>
        private string ToSfenString()
        {
            var sb = new StringBuilder();
            if (Tree.rootBoardType == BoardType.NoHandicap)
            {
                // 平手の初期局面
                sb.Append("startpos");

            } else if (Tree.rootBoardType == BoardType.Others)
            {
                // 任意局面なので"sfen"を出力する
                sb.Append(string.Format("sfen {0}",Tree.rootSfen));
            }

            // 候補の一つ目を選択していく。
            while(Tree.currentNode.moves.Count != 0)
            {
                // 1手目を出力するときには"moves"が必要。(このUSIの仕様、イケてないと思う)
                if (Tree.currentNode == Tree.rootNode)
                    sb.Append(" moves");

                // 複数候補があろうと1つ目の
                var m = Tree.currentNode.moves[0].nextMove;

                // 特殊な指し手文字列であればこれはsfenとしては出力できない。
                if (m.IsSpecial())
                    break;

                // 非合法手も出力せずにそこで中断することにする。
                if (!Tree.position.IsLegal(m))
                    break;

                sb.Append(" " + m.ToUsi());

                Tree.DoMove(m);
            }

            return sb.ToString();
        }

        /// <summary>
        /// PSN形式で書き出す。
        /// </summary>
        /// <returns></returns>
        private string ToPsnString(KifuFileType kt)
        {
            var sb = new StringBuilder();

            // 対局者名
            if (kt == KifuFileType.PSN)
            {
                sb.AppendLine(string.Format(@"[Sente ""{0}""]", playerNameBlack));
                sb.AppendLine(string.Format(@"[Gote ""{0}""]", playerNameWhite));
            } else if (kt == KifuFileType.PSN2)
            {
                sb.AppendLine(string.Format(@"[BLACK ""{0}""]", playerNameBlack));
                sb.AppendLine(string.Format(@"[WHITE ""{0}""]", playerNameWhite));
            }

            // 初期局面
            sb.AppendLine(string.Format(@"[SFEN ""{0}""]", Tree.position.ToSfen()));

            // -- 局面を出力していく

            // Treeのmoves[0]を選択してDoMove()を繰り返したものがPVで、これを最初に出力しなければならないから、
            // わりと難しい。

            // 再帰で書くの難しいので分岐地点をstackに積んでいく実装。

            var stack = new Stack<Node>();

            bool endNode = false;

            while (!endNode || stack.Count != 0)
            {
                int select = 0;
                if (endNode)
                {
                    endNode = false;
                    // 次の分岐まで巻き戻して、また出力していく。

                    var node = stack.Pop();

                    sb.AppendLine();
                    sb.AppendLine(string.Format("Variation:{0}", node.ply));

                    while (node.ply < Tree.ply)
                        Tree.UndoMove();

                    select = node.select;
                    goto SELECT;
                }

                int count = Tree.currentNode.moves.Count;
                if (count == 0)
                {
                    // ここで指し手終わっとる。終端ノードであるな。
                    endNode = true;
                    continue;
                }

                // このnodeの分岐の数
                if (count != 1)
                {
                    // あとで分岐しないといけないので残りをstackに記録しておく
                    for(int i=1;i<count;++i)
                        stack.Push(new Node(Tree.ply, i));
                }

                SELECT:;
                var move = Tree.currentNode.moves[select];

                Move m = move.nextMove;
                string mes;

                if (m.IsSpecial())
                {
                    // 特殊な指し手なら、それを出力して終わり。

                    endNode = true;

                    if (kt == KifuFileType.PSN)
                        switch(m)
                        {
                            case Move.MATED:           mes = "Mate";       break;
                            case Move.INTERRUPT:       mes = "Interrupt";  break;
                            case Move.REPETITION_WIN:  mes = "Sennichite"; break;
                            case Move.REPETITION_DRAW: mes = "Sennichite"; break;
                            case Move.WIN:             mes = "Jishogi";    break;
                            case Move.MAX_MOVES_DRAW:  mes = "Jishogi";    break;
                            case Move.RESIGN:          mes = "Resigns";    break;
                            case Move.TIME_UP:         mes = "Timeup";     break;
                            default:                   mes = "";           break;
                        }
                    else if (kt == KifuFileType.PSN2)
                        switch (m)
                        {
                            case Move.MATED:           mes = "Mate";           break;
                            case Move.INTERRUPT:       mes = "Interrupt";      break;
                            case Move.REPETITION_WIN:  mes = "RepetitionWin";  break;
                            case Move.REPETITION_DRAW: mes = "RepetitionDraw"; break;
                            case Move.WIN:             mes = "DeclarationWin"; break;
                            case Move.MAX_MOVES_DRAW:  mes = "MaxMovesDraw";   break;
                            case Move.RESIGN:          mes = "Resigns";        break;
                            case Move.TIME_UP:         mes = "Timeup"; break;
                            default: mes = ""; break;
                        }
                    else
                        mes = "";

                    mes = Tree.ply + "." + mes;
                }
                else
                {
                    // この指し手を出力する
                    if (kt == KifuFileType.PSN)
                    {
                        var to = m.To();
                        Piece pc;
                        if (m.IsDrop())
                        {
                            pc = m.DroppedPiece().PieceType();
                            mes = string.Format("{0}.{1}*{2}", Tree.ply, pc.ToUsi(), to.ToUsi());
                        }
                        else
                        {
                            var c = Tree.position.IsCapture(m) ? 'x' : '-';
                            var c2 = m.IsPromote() ? "+" : "";
                            var from = m.From();
                            pc = Tree.position.PieceOn(m.From()).PieceType();
                            mes = string.Format("{0}.{1}{2}{3}{4}{5}", Tree.ply, pc.ToUsi(), from.ToUsi(), c, to.ToUsi(), c2);
                        }
                    }
                    else if (kt == KifuFileType.PSN2)
                        // PSN2形式なら指し手表現はUSIの指し手文字列そのまま!!簡単すぎ!!
                        mes = string.Format("{0}.{1}", Tree.ply, m.ToUsi());
                    else
                        mes = "";

                    Tree.DoMove(move);
                }

                var time_string1 = (kt == KifuFileType.PSN) ? move.thinkingTime.ToString("mm\\:ss")
                                                            : move.thinkingTime.ToString("hh\\:mm\\:ss");

                var time_string2 = move.totalTime.ToString("hh\\:mm\\:ss");

                sb.AppendLine(string.Format("{0,-18}({1} / {2})", mes , time_string1 , time_string2));
            }

            return sb.ToString();
        }

        private string ToCsaString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("V2.2");
            sb.AppendFormat("N+", playerNameBlack).AppendLine();
            sb.AppendFormat("N-", playerNameWhite).AppendLine();
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    // 平手の初期局面
                    sb.AppendLine("PI");
                    sb.AppendLine("+");
                    break;
                default:
                    // それ以外は任意局面として出力する
                    sb.AppendLine(Tree.position.ToCsa().TrimEnd('\r', '\n'));
                    break;
            }

            while (Tree.currentNode.moves.Count != 0)
            {
                var m = Tree.currentNode.moves[0].nextMove;

                switch (m)
                {
                    case Move.MATED:
                        sb.AppendLine("%TORYO"); break;
                    case Move.INTERRUPT:
                        sb.AppendLine("%CHUDAN"); break;
                    case Move.REPETITION_WIN:
                        sb.AppendLine("%SENNICHITE"); break;
                    case Move.REPETITION_DRAW:
                        sb.AppendLine("%SENNICHITE"); break;
                    case Move.WIN:
                        sb.AppendLine("%KACHI"); break;
                    case Move.MAX_MOVES_DRAW:
                        sb.AppendLine("%JISHOGI"); break;
                    case Move.RESIGN:
                        sb.AppendLine("%TORYO"); break;
                    case Move.TIME_UP:
                        sb.AppendLine("%TIME_UP"); break;
                }

                if (m.IsSpecial())
                {
                    break;
                }

                if (!Tree.position.IsLegal(m))
                {
                    sb.AppendLine("%ILLEGAL_MOVE");
                    // 現時点の実装としては秒未満切り捨てとして出力。
                    sb.AppendFormat("'{0},T{1}", Tree.position.ToCSA(m), System.Math.Truncate(Tree.currentNode.moves[0].thinkingTime.TotalSeconds)).AppendLine();
                    break;
                }

                // 現時点の実装としては秒未満切り捨てとして出力。
                sb.AppendFormat("'{0},T{1}", Tree.position.ToCSA(m), System.Math.Truncate(Tree.currentNode.moves[0].thinkingTime.TotalSeconds)).AppendLine();

            }
            return sb.ToString();
        }
        private string ToKifString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("先手：", playerNameBlack).AppendLine();
            sb.AppendFormat("後手：", playerNameWhite).AppendLine();
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    sb.AppendLine("手合割：平手");
                    break;
                default:
                    sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));
                    break;
            }
            // ToDo: ここに実装する
            return sb.ToString();
        }

        private string ToKi2String()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("先手：", playerNameBlack).AppendLine();
            sb.AppendFormat("後手：", playerNameWhite).AppendLine();
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    sb.AppendLine("手合割：平手");
                    break;
                default:
                    sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));
                    break;
            }
            // ToDo: ここに実装する
            return sb.ToString();
        }

        private string ToJkfString()
        {
            Jkf jkf = new Jkf();
            jkf.header = new Dictionary<string, string>();
            foreach (var key in header.Keys)
            {
                jkf.header[key] = header[key];
            }
            jkf.initial = new Jkf.Initial();
            var CSA_PIECE = new string[] {
                "  ","FU","KY","KE","GI","KA","HI","KI",
                "OU","TO","NY","NK","NG","UM","RY","QU",
                "  ","FU","KY","KE","GI","KA","HI","KI",
                "OU","TO","NY","NK","NG","UM","RY","QU",
            };
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:       jkf.initial.preset = "HIRATE"; break;
                case BoardType.HandicapKyo:      jkf.initial.preset = "KY"; break;
                case BoardType.HandicapRightKyo: jkf.initial.preset = "KY_R"; break;
                case BoardType.HandicapKaku:     jkf.initial.preset = "KA"; break;
                case BoardType.HandicapHisya:    jkf.initial.preset = "HI"; break;
                case BoardType.HandicapHisyaKyo: jkf.initial.preset = "HIKY"; break;
                case BoardType.Handicap2:        jkf.initial.preset = "2"; break;
                case BoardType.Handicap3:        jkf.initial.preset = "3"; break;
                case BoardType.Handicap4:        jkf.initial.preset = "4"; break;
                case BoardType.Handicap5:        jkf.initial.preset = "5"; break;
                case BoardType.HandicapLeft5:    jkf.initial.preset = "5_L"; break;
                case BoardType.Handicap6:        jkf.initial.preset = "6"; break;
                case BoardType.Handicap8:        jkf.initial.preset = "8"; break;
                case BoardType.Handicap10:       jkf.initial.preset = "10"; break;
                default:
                    jkf.initial.preset = "OTHER";
                    jkf.initial.data = new Jkf.Data()
                    {
                        color = Tree.position.sideToMove == Color.BLACK ? 0 : 1,
                        board = new Jkf.Board[9,9],
                        hands = new List<Dictionary<string, int>> {
                            new Dictionary<string, int>(),
                            new Dictionary<string, int>(),
                        },
                    };
                    for (File f = File.FILE_1; f <= File.FILE_9; ++f)
                    for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
                    {
                        var p = Tree.position.PieceOn(Util.MakeSquare(f, r));
                        jkf.initial.data.board[f.ToInt(), r.ToInt()] = p == Piece.NO_PIECE ?
                            new Jkf.Board():
                            new Jkf.Board()
                            {
                                kind = CSA_PIECE[p.PieceType().ToInt()],
                                color = p.PieceColor() == Color.BLACK ? 0 : 1,
                            };
                    }
                    foreach (var c in new Color[] { Color.BLACK, Color.WHITE })
                    foreach (var p in new Piece[] { Piece.PAWN, Piece.LANCE, Piece.KNIGHT, Piece.SILVER, Piece.GOLD, Piece.BISHOP, Piece.ROOK })
                    {
                        jkf.initial.data.hands[c.ToInt()][CSA_PIECE[p.ToInt()]] = Tree.position.Hand(c).Count(p);
                    }
                    break;
            }
            {
                // 初期局面情報の出力
                var root = new Jkf.MoveFormat()
                {
                    comments = new List<string>(),
                };
                foreach (var line in Tree.rootNode.comment.Split('\n'))
                {
                    root.comments.Add(line);
                }
                foreach (var line in Tree.rootKifuLog.engineComment.Split('\n'))
                {
                    root.comments.Add(line);
                }

                // 棋譜の出力
                var outList = jkf.moves = new List<Jkf.MoveFormat>() { root };
                var inStack = new Stack<Node>();
                var outStack = new Stack<List<List<Jkf.MoveFormat>>>();
                var endNode = false;
                Func<KifMoveInfo, string> relStr = (moveInfo) =>
                {
                    string rel, beh;
                    switch (moveInfo.relative)
                    {
                        case KifMoveInfo.Relative.LEFT:     rel = "L"; break;
                        case KifMoveInfo.Relative.STRAIGHT: rel = "C"; break;
                        case KifMoveInfo.Relative.RIGHT:    rel = "R"; break;
                        default: rel = ""; break;
                    }
                    switch (moveInfo.behavior)
                    {
                        case KifMoveInfo.Behavior.FORWARD:  beh = "U"; break;
                        case KifMoveInfo.Behavior.SLIDE:    beh = "M"; break;
                        case KifMoveInfo.Behavior.BACKWARD: beh = "D"; break;
                        default: beh = ""; break;
                    }
                    // ToDo: 省略可能な駒打ちの場合に"H"と出力すべきか確認する
                    // 省略可能な駒打ちの場合でも"H"と出力
                    string drop = moveInfo.drop == KifMoveInfo.Drop.NONE ? "" : "H";
                    // 省略可能な駒打ちの場合は"H"と出力しない
                    //string drop = moveInfo.drop == KifMoveInfo.Drop.EXPLICIT ? "H" : "";
                    return rel + beh + drop;
                };

                while (!endNode || inStack.Count != 0)
                {
                    int select = 0;
                    var jkfMove = new Jkf.MoveFormat() {
                        forks = new List<List<Jkf.MoveFormat>>(),
                    };

                    if (endNode)
                    {
                        endNode = false;
                        var inNode = inStack.Pop();
                        var outBranches = outStack.Pop();
                        outBranches.Add(outList = new List<Jkf.MoveFormat>());
                        while (inNode.ply < Tree.ply)
                            Tree.UndoMove();
                        select = inNode.select;
                    }
                    else
                    {
                        int count = Tree.currentNode.moves.Count;
                        if (count == 0)
                        {
                            endNode = true;
                            continue;
                        }
                        if (count != 1)
                        {
                            for (int i = 1; i < count; ++i)
                            {
                                inStack.Push(new Node(Tree.ply, i));
                                outStack.Push(jkfMove.forks);
                            }
                        }
                    }

                    var kifMove = Tree.currentNode.moves[select];
                    Move m = kifMove.nextMove;

                    if (m.IsSpecial())
                    {
                        endNode = true;
                        switch (m)
                        {
                            case Move.MATED:           jkfMove.special = "TYORYO"; break;
                            case Move.INTERRUPT:       jkfMove.special = "CHUDAN"; break;
                            case Move.REPETITION_WIN:  jkfMove.special = "SENNICHITE"; break;
                            case Move.REPETITION_DRAW: jkfMove.special = "SENNICHITE"; break;
                            case Move.WIN:             jkfMove.special = "KACHI"; break;
                            case Move.MAX_MOVES_DRAW:  jkfMove.special = "JISHOGI"; break;
                            case Move.RESIGN:          jkfMove.special = "TORYO"; break;
                            case Move.TIME_UP:         jkfMove.special = "TIME_UP"; break;
                            default: continue;
                        }
                    }
                    else
                    {
                        var moveInfo = new KifMoveInfo(Tree.position, kifMove.nextMove);
                        jkfMove.move = new Jkf.Move()
                        {
                            color = Tree.position.sideToMove == Color.BLACK ? 0 : 1,
                            from = m.IsDrop() ? null : new Jkf.PlaceFormat()
                            {
                                x = m.From().ToFile().ToInt() + 1,
                                y = m.From().ToRank().ToInt() + 1,
                            },
                            to = new Jkf.PlaceFormat()
                            {
                                x = m.To().ToFile().ToInt() + 1,
                                y = m.To().ToRank().ToInt() + 1,
                            },
                            piece = CSA_PIECE[moveInfo.fromPc.ToInt()],
                            same = moveInfo.same,
                            promote =
                                (moveInfo.promote == KifMoveInfo.Promote.NONE) ? (bool?)null :
                                (moveInfo.promote == KifMoveInfo.Promote.PROMOTE),
                            capture = moveInfo.capPc == Piece.NO_PIECE ? null : CSA_PIECE[moveInfo.capPc.ToInt()],
                            relative = relStr(moveInfo),
                        };
                    }

                    jkfMove.time = new Jkf.Time()
                    {
                        now = new Jkf.TimeFormat()
                        {
                            h = kifMove.thinkingTime.Days * 24 + kifMove.thinkingTime.Hours,
                            m = kifMove.thinkingTime.Minutes,
                            s = kifMove.thinkingTime.Seconds,
                        },
                        total = new Jkf.TimeFormat()
                        {
                            h = kifMove.totalTime.Days * 24 + kifMove.totalTime.Hours,
                            m = kifMove.totalTime.Minutes,
                            s = kifMove.totalTime.Seconds,
                        },
                    };

                    outList.Add(jkfMove);

                }
            }
            return jkf.ToJson();
        }

        private static KifFormatterImmutableOptions livejsonkifformat = new KifFormatterImmutableOptions(ColorFormat.NONE, SquareFormat.FullWidthMix, SamePosFormat.ZEROsp, FromSqFormat.KI2);
        private string ToJsonString()
        {
            // 平手以外の出力は現状対応しない
            if (Tree.rootBoardType != BoardType.NoHandicap)
            {
                return string.Empty;
            }
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
            var data = new LiveJson.Data();
            data.handicap = "平手";
            data.side = "先手";
            data.player1 = playerNameBlack;
            data.player2 = playerNameWhite;
            if (Tree.rootKifuLog != null && Tree.rootKifuLog.moveTime != null)
            {
                data.realstarttime = (long)Tree.rootKifuLog.moveTime.Subtract(epoch).TotalMilliseconds;
                data.starttime = Tree.rootKifuLog.moveTime.ToString("s");
            }
            var kifList = new List<LiveJson.Kif>();
            while (Tree.currentNode.moves.Count > 0)
            {
                var kifMove = Tree.currentNode.moves[0];
                var kif = new LiveJson.Kif();
                if (kifMove.moveTime != null)
                {
                    kif.time = (long)(kifMove.moveTime.Subtract(epoch).TotalMilliseconds);
                }
                kif.spend = (long)kifMove.thinkingTime.TotalSeconds;
                var nextMove = kifMove.nextMove;
                if (nextMove.IsSpecial())
                {
                    if (kifMove.moveTime != null)
                    {
                        data.endtime = kifMove.moveTime.ToString("s");
                    }
                    switch (nextMove)
                    {
                        case Move.RESIGN:
                            kif.move = "投了";
                            break;
                        case Move.INTERRUPT:
                            kif.move = "中断";
                            break;
                        default:
                            goto jailbreak;
                    }
                }
                else if (nextMove.IsDrop())
                {
                    var droppedPiece = nextMove.DroppedPiece();
                    switch (droppedPiece)
                    {
                        case Piece.PAWN:   kif.type = "FU"; break;
                        case Piece.LANCE:  kif.type = "KYO"; break;
                        case Piece.KNIGHT: kif.type = "KEI"; break;
                        case Piece.SILVER: kif.type = "GIN"; break;
                        case Piece.GOLD:   kif.type = "KIN"; break;
                        case Piece.BISHOP: kif.type = "KAKU"; break;
                        case Piece.ROOK:   kif.type = "HI"; break;
                    }
                    var state = Tree.position.State();
                    kif.move = livejsonkifformat.format(Tree.position, nextMove, state != null ? state.lastMove : Move.NONE);
                    if (Tree.position.sideToMove == Color.BLACK)
                    {
                        kif.frY = 10;
                        switch (droppedPiece)
                        {
                            case Piece.PAWN:   kif.frX = 1; break;
                            case Piece.LANCE:  kif.frX = 2; break;
                            case Piece.KNIGHT: kif.frX = 3; break;
                            case Piece.SILVER: kif.frX = 4; break;
                            case Piece.GOLD:   kif.frX = 5; break;
                            case Piece.BISHOP: kif.frX = 6; break;
                            case Piece.ROOK:   kif.frY = 7; break;
                        }
                    }
                    else
                    {
                        kif.frY = 0;
                        switch (droppedPiece)
                        {
                            case Piece.PAWN:   kif.frX = 9; break;
                            case Piece.LANCE:  kif.frX = 8; break;
                            case Piece.KNIGHT: kif.frX = 7; break;
                            case Piece.SILVER: kif.frX = 6; break;
                            case Piece.GOLD:   kif.frX = 5; break;
                            case Piece.BISHOP: kif.frX = 4; break;
                            case Piece.ROOK:   kif.frY = 3; break;
                        }
                    }
                    kif.toX = nextMove.To().ToFile().ToInt() + 1;
                    kif.toY = nextMove.To().ToFile().ToInt() + 1;
                    kif.prmt = 0;
                }
                else
                {
                    var state = Tree.position.State();
                    kif.move = livejsonkifformat.format(Tree.position, nextMove, state != null ? state.lastMove : Move.NONE);
                    kif.frX = nextMove.From().ToFile().ToInt() + 1;
                    kif.frY = nextMove.From().ToRank().ToInt() + 1;
                    kif.toX = nextMove.To().ToFile().ToInt() + 1;
                    kif.toY = nextMove.To().ToRank().ToInt() + 1;
                    kif.prmt = nextMove.IsPromote() ? 1 : 0;
                    switch (Tree.position.PieceOn(nextMove.From()).PieceType())
                    {
                        case Piece.PAWN:       kif.type = "FU"; break;
                        case Piece.LANCE:      kif.type = "KYO"; break;
                        case Piece.KNIGHT:     kif.type = "KEI"; break;
                        case Piece.SILVER:     kif.type = "GIN"; break;
                        case Piece.GOLD:       kif.type = "KIN"; break;
                        case Piece.BISHOP:     kif.type = "KAKU"; break;
                        case Piece.ROOK:       kif.type = "HI"; break;
                        case Piece.KING:       kif.type = "OU"; break;
                        case Piece.PRO_PAWN:   kif.type = "NFU"; break;
                        case Piece.PRO_LANCE:  kif.type = "NKYO"; break;
                        case Piece.PRO_KNIGHT: kif.type = "NKEI"; break;
                        case Piece.PRO_SILVER: kif.type = "NGIN"; break;
                        case Piece.HORSE:      kif.type = "NKAKU"; break;
                        case Piece.DRAGON:     kif.type = "NHI"; break;
                    }
                }
                kif.num = Tree.position.gamePly;
                kifList.Add(kif);
                if (nextMove.IsSpecial())
                {
                    break;
                }
                Tree.DoMove(kifMove);
            }
            jailbreak:;
            data.kif = kifList;
            data.v = kifList.Count;
            data.breaktime = new List<LiveJson.BreakTime>();
            var jsonObj = new LiveJson();
            jsonObj.data = new List<LiveJson.Data> { data };
            jsonObj.cacheUpdateTime = (long)(DateTime.Now.Subtract(epoch).TotalMilliseconds);
            return jsonObj.ToJson();
        }

        /// <summary>
        /// ply手目のselect個目の指し手分岐をのちほど選択するの意味
        /// </summary>
        private struct Node
        {
            public int ply;
            public int select;
            public Node(int ply_, int select_) { ply = ply_; select = select_; }
        };

    }
}
