using MyShogi.Model.Shogi.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
        /// 対局者名。
        ///   playerName[(int)Color.BLACK] : 先手の名前(駒落ちの場合、下手)
        ///   playerName[(int)Color.WHITE] : 後手の名前(駒落ちの場合、上手)
        /// </summary>
        public string[] playerName;
        
        /// <summary>
        /// 棋譜本体。分岐棋譜。
        /// </summary>
        public KifuTree Tree = new KifuTree();

        /// <summary>
        /// rootの局面の局面タイプ
        /// 任意局面の場合は、BoardType.Others
        /// </summary>
        public BoardType rootBoardType;

        /// <summary>
        /// rootの局面図。sfen形式で。
        /// </summary>
        public string rootSfen;

        // -------------------------------------------------------------------------
        // public methods
        // -------------------------------------------------------------------------

        public KifuManager()
        {
            Init();
        }

        /// <summary>
        /// このクラスを初期化する。new KifuManager()とした時の状態になる。
        /// </summary>
        public void Init()
        {
            playerName = new string[2];
            Tree.Init();
            rootBoardType = BoardType.NoHandicap;
            rootSfen = Position.SFEN_HIRATE;
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
                return FromPsnString(lines);

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
                    result = ToPsnString();
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
            // Position.UsiPositionCmd()からの移植。

            // スペースをセパレータとして分離する
            var split = sfen.Split(
                new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            //// どうなっとるねん..
            //if (split.Length == 0)
            //    return;

            // 現在の指し手が書かれている場所 split[cur_pos]
            var cur_pos = 1;
            if (split[0] == "sfen")
            {
                // "sfen ... moves ..."形式かな..
                // movesの手前までをくっつけてSetSfen()する
                while (cur_pos < split.Length && split[cur_pos] != "moves")
                {
                    ++cur_pos;
                }

                if (!(cur_pos == 4 || cur_pos == 5))
                    throw new PositionException("Position.UsiPositionCmd()に渡された文字列にmovesが出てこない");

                if (cur_pos == 4)
                    rootSfen = string.Format("{0} {1} {2}", split[1], split[2], split[3]);
                else // if (cur_pos == 5)
                    rootSfen = string.Format("{0} {1} {2} {3}", split[1], split[2], split[3], split[4]);

                Tree.position.SetSfen(rootSfen);
                rootBoardType = BoardType.Others;
            }
            else if (split[0] == "startpos")
            {
                rootSfen = Position.SFEN_HIRATE;
                Tree.position.SetSfen(rootSfen);
                rootBoardType = BoardType.NoHandicap;
            }

            // "moves"以降の文字列をUSIの指し手と解釈しながら、局面を進める。
            if (cur_pos < split.Length && split[cur_pos] == "moves")
                for (int i = cur_pos + 1; i < split.Length; ++i)
                {
                    // デバッグ用に盤面を出力
                    //Console.WriteLine(Pretty());

                    var move = Util.FromUsiMove(split[i]);
                    if (!Tree.position.IsLegal(move))
                        // throw new PositionException(string.Format("{0}手目が非合法手です。", i - cur_pos));
                        return string.Format("{0}手目が非合法手です。", i - cur_pos);

                    // この指し手で局面を進める
                    Tree.Add(move , new TimeSpan());
                    Tree.DoMove(move);
                }

            return string.Empty;
        }

        /// <summary>
        /// PSN形式の棋譜ファイルのparser
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        private string FromPsnString(string[] lines)
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
                            playerName[(int)Color.BLACK] = body;
                            break;

                        case "Gote":
                            playerName[(int)Color.WHITE] = body;
                            break;

                        case "SFEN":
                            // 将棋所で出力したPSNファイルはここに必ず"SFEN"が来るはず。平手の局面であっても…。
                            // 互換性のためにも、こうなっているべきだろう。

                            rootSfen = body;
                            Tree.position.SetSfen(body);
                            rootBoardType = BoardType.Others;
                            break;
                    }
                }
                else
                    break;
            }

            // PSNフォーマットのサイトを見ると千日手とか宣言勝ちについて規定がない。
            // どう見ても欠陥フォーマットである。
            // http://genedavis.com/articles/2014/05/09/psn/


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
                            move = Move.WIN;
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
        /// 現在の棋譜をUSIプロトコルの"position"コマンドで使う文字列化する。
        /// 便宜上、ここではsfenと呼んでいるが、本来はsfenには"moves .."は含まれない。
        /// 特殊な指し手は出力されない。
        /// </summary>
        /// <returns></returns>
        private string ToSfenString()
        {
            var sb = new StringBuilder();
            if (rootBoardType == BoardType.NoHandicap)
            {
                // 平手の初期局面
                sb.Append("startpos");

            } else if (rootBoardType == BoardType.Others)
            {
                // 任意局面なので"sfen"を出力する
                sb.Append(string.Format("sfen {0}",rootSfen));
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
        private string ToPsnString()
        {
            var sb = new StringBuilder();

            // 対局者名
            sb.AppendLine(string.Format(@"[Sente ""{0}""]", playerName[(int)Color.BLACK]));
            sb.AppendLine(string.Format(@"[Gote ""{0}""]", playerName[(int)Color.WHITE]));

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

                    switch(m)
                    {
                        case Move.MATED:           mes = "Mate";       break;
                        case Move.INTERRUPT:       mes = "Interrupt";  break;
                        case Move.REPETITION_WIN:  mes = "Sennichite"; break;
                        case Move.REPETITION_DRAW: mes = "Sennichite"; break;
                        case Move.WIN:             mes = "Jishogi";    break;
                        case Move.RESIGN:          mes = "Resigns";    break;
                        default:                   mes = "";           break;
                    }

                    mes = Tree.ply + "." + mes;
                }
                else
                {
                    // この指し手を出力する
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


                    Tree.DoMove(move);
                }

                var time_string1 = move.thinkingTime.ToString("mm\\:ss");
                var time_string2 = move.totalTime.ToString("hh\\:mm\\:ss");

                sb.AppendLine(string.Format("{0,-18}({1} / {2})", mes , time_string1 , time_string2));
            }

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
