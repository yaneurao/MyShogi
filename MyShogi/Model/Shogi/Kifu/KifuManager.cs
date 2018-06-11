using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Common.Utility;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 1局の対局棋譜を全般的に管理するクラス
    /// ・分岐棋譜をサポート
    /// ・着手時間をサポート
    /// ・対局相手の名前をサポート
    /// ・KIF/KI2/CSA/SFEN/PSN形式での入出力をサポート
    /// ・千日手の管理、検出をサポート
    ///
    /// 使用上の注意)
    /// ・bind()で、Positionのインスタンスを関連付けてから使うこと。
    /// ・また、必要ならば、そのあとにInit()を呼び出すこと。
    /// </summary>
    public class KifuManager
    {
        // -------------------------------------------------------------------------
        // public members
        // -------------------------------------------------------------------------

        /// <summary>
        /// 対局者名。
        ///   playerName[(int)Color.BLACK] : 先手の名前(駒落ちの場合、下手)
        ///   playerName[(int)Color.WHITE] : 後手の名前(駒落ちの場合、上手)
        /// </summary>
        public string[] playerName;

        /// <summary>
        /// 棋譜本体。分岐棋譜。
        /// </summary>
        public KifuTree Tree = new KifuTree();

        // -------------------------------------------------------------------------
        // public methods
        // -------------------------------------------------------------------------

        /// <summary>
        /// このメソッドを用いて、必ず外部からPositionのインスタンスを関連付けてから
        /// このクラスのメソッドを呼び出すこと。
        /// </summary>
        /// <param name="pos"></param>
        public void Bind(Position pos)
        {
            Tree.Bind(pos);
        }

        /// <summary>
        /// このクラスを初期化する。new KifuManager()とした時の状態になる。
        /// </summary>
        public void Init()
        {
            playerName = new string[2];
            Tree.Init();
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
        public string FromString(string content /* , KifFileType kf */)
        {
            Init();

            var lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (lines.Length == 0)
                return "棋譜が空でした。";
            var line = lines[0];

            // sfen形式なのか？
            if ( line.StartsWith("sfen") || line.StartsWith("startpos"))
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

            return string.Empty;
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

                // ToDo : 他の形式もサポートする
            }

            // 呼び出された時の局面に戻す
            Tree.RewindToRoot();
            Tree.FastForward(moves);

            return result;
        }

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
                    scanner.ParseWord();
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
                        Tree.Add(move, new TimeSpan());
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
                            playerName[(int)Color.BLACK] = body;
                            break;

                        case "Gote":
                        case "WHITE":
                            playerName[(int)Color.WHITE] = body;
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

                    Tree.Add(move , thinking_time , total_time);

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
                        playerName[(int)Color.BLACK] = subline.Substring(2);
                        continue;
                    }
                    if (subline.StartsWith("N-"))
                    {
                        playerName[(int)Color.WHITE] = subline.Substring(2);
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
                        Tree.Add(move, new TimeSpan());
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
                        Tree.Add(move, TimeSpan.FromSeconds(time));
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
                            // 以下、適切な変換先不明
                            case "%HIKIWAKE":
                            case "%TSUMI":
                            case "%FUZUMI":
                            case "%MATTA":
                            case "%ILLEGAL_MOVE":
                            case "%+ILLEGAL_ACTION":
                            case "%-ILLEGAL_ACTION":
                            case "%ERROR":
                            default:
                                move = Move.NONE;
                                break;
                        }
                        Tree.Add(move, new TimeSpan());
                        continue;
                    }
                }
            }
            if (headFlag) // まだ局面図が終わってない
                return string.Format("CSA形式の{0}行目で局面図が来ずにファイルが終了しました。", lineNo);

            return string.Empty;
        }

        private string FromKifString(string[] lines)
        {
            // ToDo: ここに実装する
            return string.Empty;
        }

        private string FromKi2String(string[] lines)
        {
            // ToDo: ここに実装する
            return string.Empty;
        }
        private string FromJsonString(string[] lines)
        {
            // ToDo: ここに実装する
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
                sb.AppendLine(string.Format(@"[Sente ""{0}""]", playerName[(int)Color.BLACK]));
                sb.AppendLine(string.Format(@"[Gote ""{0}""]", playerName[(int)Color.WHITE]));
            } else if (kt == KifuFileType.PSN2)
            {
                sb.AppendLine(string.Format(@"[BLACK ""{0}""]", playerName[(int)Color.BLACK]));
                sb.AppendLine(string.Format(@"[WHITE ""{0}""]", playerName[(int)Color.WHITE]));
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
            sb.AppendFormat("N+", playerName[Color.BLACK.ToInt()]).AppendLine();
            sb.AppendFormat("N-", playerName[Color.WHITE.ToInt()]).AppendLine();
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
            sb.AppendFormat("先手：", playerName[Color.BLACK.ToInt()]).AppendLine();
            sb.AppendFormat("後手：", playerName[Color.WHITE.ToInt()]).AppendLine();
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
            sb.AppendFormat("先手：", playerName[Color.BLACK.ToInt()]).AppendLine();
            sb.AppendFormat("後手：", playerName[Color.WHITE.ToInt()]).AppendLine();
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
        private string ToJsonString()
        {
            var sb = new StringBuilder();
            // ToDo: ここに実装する
            return sb.ToString();
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
