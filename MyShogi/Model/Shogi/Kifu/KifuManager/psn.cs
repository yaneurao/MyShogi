using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// psnの読み書き
    /// </summary>
    public partial class KifuManager
    {
        /// <summary>
        /// PSN形式の棋譜ファイルのparser
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければnullが返る。
        /// </summary>
        private string FromPsnString(string[] lines, KifuFileType kf)
        {
            // 消費時間、残り時間、消費時間を管理する。
            var timeSettings = KifuTimeSettings.TimeLimitless;

            var lineNo = 1;

            try
            {

                var r1 = new Regex(@"\[([^\s]+)\s*""(.*)""\]");

                for (; lineNo <= lines.Length; ++lineNo)
                {
                    var line = lines[lineNo - 1];
                    var m1 = r1.Match(line);
                    if (m1.Success)
                    {
                        var token = m1.Groups[1].Value.ToLower();
                        var body = m1.Groups[2].Value;

                        switch (token)
                        {
                            case "sente":
                            case "black":
                                KifuHeader.PlayerNameBlack = body;
                                break;

                            case "gote":
                            case "white":
                                KifuHeader.PlayerNameWhite = body;
                                break;

                            case "sfen":
                                // 将棋所で出力したPSNファイルはここに必ず"SFEN"が来るはず。平手の局面であっても…。
                                // 互換性のためにも、こうなっているべきだろう。

                                Tree.rootSfen = body;
                                Tree.position.SetSfen(body);
                                Tree.rootBoardType = BoardType.Others;
                                break;

                            case "blacktimesetting":
                                var black_setting = KifuTimeSetting.FromKifuString(body);
                                if (black_setting != null)
                                    timeSettings.Players[(int)Color.BLACK] = black_setting;
                                break;

                            case "whitetimesetting":
                                var white_setting = KifuTimeSetting.FromKifuString(body);
                                if (white_setting != null)
                                {
                                    timeSettings.Players[(int)Color.WHITE] = white_setting;
                                    timeSettings.WhiteEnable = true; // 後手は個別設定
                                }
                                break;
                        }
                    }
                    else
                        break;
                }

                // 残り時間の計算用。
                var times = timeSettings.GetInitialKifuMoveTimes();
                Tree.SetKifuMoveTimes(times.Clone()); // root局面での残り時間の設定
                Tree.KifuTimeSettings = timeSettings.Clone();

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
                        while (ply < Tree.gamePly)
                            Tree.UndoMove();

                        // このnodeでの残り時間に戻す
                        times = Tree.GetKifuMoveTimes();

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
                            throw new KifuException("指し手文字列の駒名がおかしいです。", line);
                        pc = pc.PieceType();

                        bool drop_move = false;
                        if (peek_char() == '*')
                        {
                            get_char();
                            drop_move = true;
                            if (promote_piece)
                                throw new KifuException("指し手文字列で成駒を打とうとしました。", line);
                        }

                        // 移動元の升
                        var c1 = get_char();
                        var c2 = get_char();
                        var from = Util.FromUsiSquare(c1, c2);
                        if (from == Square.NB)
                            throw new KifuException("指し手文字列の移動元の表現がおかしいです。", line);

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
                        }
                        else if (peek_char() == 'x')
                        {
                            get_char();
                            //is_capture = true;
                        }

                        // 移動先の升
                        var c3 = get_char();
                        var c4 = get_char();
                        var to = Util.FromUsiSquare(c3, c4);
                        if (to == Square.NB)
                            throw new KifuException("指し手文字列の移動先の表現がおかしいです。" , line);

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
                        TimeSpan.TryParse(time_string2, out TimeSpan total_time);

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

                            throw new KifuException("千日手となっていますが千日手ではないです。" , line);
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

                        var turn = Tree.position.sideToMove;
                        times.Players[(int)turn] = times.Players[(int)turn].Create(
                            timeSettings.Player(turn),
                            thinking_time,thinking_time,
                            total_time /*消費時間は棋譜に記録されているものをそのまま使用する*/
                            /*残り時間は棋譜上に記録されていない*/
                            );

                        // -- DoMove()

                        Tree.AddNode(move, times.Clone());

                        // 特殊な指し手、もしくはLegalでないならDoMove()は行わない
                        if (!move.IsSpecial() && !Tree.position.IsLegal(move))
                        {
                            // まだ次の分岐棋譜があるかも知れないので読み進める
                            continue;
                        }

                        // special moveであってもTree.DoMove()は出来る
                        Tree.DoMove(move);
                        continue;

                    }
                    else
                    {
                        // 空行など、parseに失敗したものは読み飛ばす
                    }
                }

            } catch (Exception e)
            {
                return $"棋譜読み込みエラー : {lineNo}行目\n{e.Message}";
            }

            return null;
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
                sb.AppendLine(string.Format(@"[Sente ""{0}""]", KifuHeader.PlayerNameBlack));
                sb.AppendLine(string.Format(@"[Gote ""{0}""]", KifuHeader.PlayerNameWhite));
            }
            else if (kt == KifuFileType.PSN2)
            {
                sb.AppendLine(string.Format(@"[Black ""{0}""]", KifuHeader.PlayerNameBlack));
                sb.AppendLine(string.Format(@"[White ""{0}""]", KifuHeader.PlayerNameWhite));

                // 持ち時間設定も合わせて書き出す
                sb.AppendLine($"[BlackTimeSetting \"{Tree.KifuTimeSettings.Player(Color.BLACK).ToKifuString()}\"]");
                sb.AppendLine($"[WhiteTimeSetting \"{Tree.KifuTimeSettings.Player(Color.WHITE).ToKifuString()}\"]");
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

                    while (node.ply < Tree.gamePly)
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
                    for (int i = 1; i < count; ++i)
                        stack.Push(new Node(Tree.gamePly, i));
                }

                SELECT:;
                var move = Tree.currentNode.moves[select];
                var m = move.nextMove;

                // DoMove()する前の現局面の手番
                var turn = Tree.position.sideToMove;

                string mes;

                if (m.IsSpecial())
                {
                    // 特殊な指し手なら、それを出力して終わり。

                    endNode = true;

                    if (kt == KifuFileType.PSN)
                        switch (m)
                        {
                            case Move.MATED: mes = "Mate"; break;
                            case Move.INTERRUPT: mes = "Interrupt"; break;
                            case Move.REPETITION_WIN: mes = "Sennichite"; break;
                            case Move.REPETITION_DRAW: mes = "Sennichite"; break;
                            case Move.WIN: mes = "Jishogi"; break;
                            case Move.MAX_MOVES_DRAW: mes = "Jishogi"; break;
                            case Move.RESIGN: mes = "Resigns"; break;
                            case Move.TIME_UP: mes = "Timeup"; break;
                            default: mes = ""; break;
                        }
                    else if (kt == KifuFileType.PSN2)
                        switch (m)
                        {
                            case Move.MATED: mes = "Mate"; break;
                            case Move.INTERRUPT: mes = "Interrupt"; break;
                            case Move.REPETITION_WIN: mes = "RepetitionWin"; break;
                            case Move.REPETITION_DRAW: mes = "RepetitionDraw"; break;
                            case Move.WIN: mes = "DeclarationWin"; break;
                            case Move.MAX_MOVES_DRAW: mes = "MaxMovesDraw"; break;
                            case Move.RESIGN: mes = "Resigns"; break;
                            case Move.TIME_UP: mes = "Timeup"; break;
                            default: mes = ""; break;
                        }
                    else
                        mes = "";

                    mes = Tree.gamePly + "." + mes;
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
                            mes = string.Format("{0}.{1}*{2}", Tree.gamePly, pc.ToUsi(), to.ToUsi());
                        }
                        else
                        {
                            var c = Tree.position.IsCapture(m) ? 'x' : '-';
                            var c2 = m.IsPromote() ? "+" : "";
                            var from = m.From();
                            pc = Tree.position.PieceOn(m.From()).PieceType();
                            mes = string.Format("{0}.{1}{2}{3}{4}{5}", Tree.gamePly, pc.ToUsi(), from.ToUsi(), c, to.ToUsi(), c2);
                        }
                    }
                    else if (kt == KifuFileType.PSN2)
                        // PSN2形式なら指し手表現はUSIの指し手文字列そのまま!!簡単すぎ!!
                        mes = string.Format("{0}.{1}", Tree.gamePly, m.ToUsi());
                    else
                        mes = "";

                    Tree.DoMove(move);
                }

                var time_string1 = (kt == KifuFileType.PSN) ? move.kifuMoveTimes.Player(turn).ThinkingTime.ToString("mm\\:ss")
                                                            : move.kifuMoveTimes.Player(turn).ThinkingTime.ToString("hh\\:mm\\:ss");

                var time_string2 = move.kifuMoveTimes.Player(turn).TotalTime.ToString("hh\\:mm\\:ss");

                sb.AppendLine(string.Format("{0,-18}({1} / {2})", mes, time_string1, time_string2));
            }

            return sb.ToString();
        }
    }
}
