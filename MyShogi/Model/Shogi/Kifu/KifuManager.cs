using MyShogi.Model.Shogi.Core;
using System;
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
            switch(kt)
            {
                case KifuFileType.SFEN:
                    return ToSfenString();

                // ToDo : 他の形式もサポートする
            }

            return "";
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
            if (lines.Length < 3)
                return "PSN形式の棋譜が短すぎます。";

            var r1 = new Regex(@"\[Sente ""(.*)""\]");
            var m1 = r1.Match(lines[0]);
            if (!m1.Success)
                return "PSN形式で先手の対局者名の部分がおかしいです。";
            playerName[(int)Color.BLACK] = m1.Groups[1].Value;

            var r2 = new Regex(@"\[Gote ""(.*)""\]");
            var m2 = r2.Match(lines[1]);
            if (!m2.Success)
                return "PSN形式で後手の対局者名の部分がおかしいです。";
            playerName[(int)Color.WHITE] = m2.Groups[1].Value;

            var r3 = new Regex(@"\[SFEN ""(.*)""\]");
            var m3 = r3.Match(lines[2]);
            // 将棋所で出力したPSNファイルはここに必ず"SFEN"が来るはず。平手の局面であっても…。
            // 互換性のためにも、こうなっているべきだろう。

            if (!m3.Success)
                return "PSN形式で開始局面が解釈出来ませんでした。";

            var rootSfen = m3.Groups[1].Value;
            Tree.position.SetSfen(rootSfen);
            rootBoardType = BoardType.Others;

            // -- 4行目以降は指し手文字列などが来るはず..

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

            var r4 = new Regex(@"(\d+)\.([^\s]+)\s*\((.+?)\s*\/\s*(.+)\)");
            // 正規表現のデバッグ難しすぎワロタ
            // 正規表現デバッグ用の神サイトを使う : https://regex101.com/

            for (var lineNo = 4; lineNo <= lines.Length; ++lineNo)
            {
                var line = lines[lineNo - 1];
                var m4 = r4.Match(line);
                if (m4.Success)
                {
                    var ply_string = m4.Groups[1].Value;
                    int ply;
                    if (!int.TryParse(ply_string, out ply))
                        return string.Format("PSN形式の{0}行目の手数文字列がおかしいです。", lineNo);
                    var move_string = m4.Groups[2].Value;
                    var time_string1 = m4.Groups[3].Value;
                    var time_string2 = m4.Groups[4].Value;

                    Move move = Move.NONE;

                    // move_stringが"Sennichite"などであるか。
                    if (move_string == "Sennichite")
                    {
                        // どちらが勝ちかはわからない千日手
                        move = Move.REPETITION;
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

                    Tree.Add(move , thinking_time , total_time);

                    // 特殊な指し手、もしくはLegalでないならDoMove()は行わない
                    if (move.IsSpecial() || !Tree.position.IsLegal(move))
                        break;

                    Tree.DoMove(move);

                    continue;
                }

                // 分岐棋譜、もしくは負けなど
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
            // 現在の局面をrootまで巻き戻す
            var moves = Tree.RewindToRoot();

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

            // 元の局面に戻す
            Tree.RewindToRoot();
            Tree.FastForward(moves);

            return sb.ToString();
        }

    }
}
