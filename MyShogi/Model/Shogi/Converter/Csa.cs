using System;
using System.Text;
using System.Text.RegularExpressions;
using SysMath = System.Math;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Converter
{
    // CSA形式の文字列を取り扱うクラス群

    /// <summary>
    /// csa形式の入出力
    /// </summary>
    public static class CsaExtensions
    {
        private static string[] csa_pos_rank = {
            "P1","P2","P3","P4","P5","P6","P7","P8","P9",
        };
        private static string[] csa_pos_piece = {
            " * ","+FU","+KY","+KE","+GI","+KA","+HI","+KI",
            "+OU","+TO","+NY","+NK","+NG","+UM","+RY","+QU",
            " * ","-FU","-KY","-KE","-GI","-KA","-HI","-KI",
            "-OU","-TO","-NY","-NK","-NG","-UM","-RY","-QU",
        };
        private static string[] csa_piece = {
            "  ","FU","KY","KE","GI","KA","HI","KI",
            "OU","TO","NY","NK","NG","UM","RY","QU",
            "  ","FU","KY","KE","GI","KA","HI","KI",
            "OU","TO","NY","NK","NG","UM","RY","QU",
        };
        private static readonly string[] HW_NUMBER = {
            "1","2","3","4","5","6","7","8","9",
        };
        private static readonly Piece[] HAND_ORDER = {
            Piece.ROOK,
            Piece.BISHOP,
            Piece.GOLD,
            Piece.SILVER,
            Piece.KNIGHT,
            Piece.LANCE,
            Piece.PAWN
        };

        /// <summary>
        /// 現在の局面図をCSA形式で出力する
        /// Position.ToSfen()のCSA版
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string ToCsa(this Position pos)
        {
            StringBuilder csa = new StringBuilder();
            // 盤面
            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
            {
                csa.Append(csa_pos_rank[r.ToInt()]);
                for (File f = File.FILE_9; f >= File.FILE_1; --f)
                {
                    csa.Append(csa_pos_piece[pos.PieceOn(Core.Util.MakeSquare(f, r)).ToInt()]);
                }
                csa.AppendLine();
            }
            // 持駒
            foreach (Color c in new Color[] { Color.BLACK, Color.WHITE })
            {
                if (pos.Hand(c) == Hand.ZERO)
                    continue;
                csa.Append((c == Color.BLACK) ? "P+" : "P-");
                foreach (Piece pc in HAND_ORDER)
                {
                    int cnt = pos.Hand(c).Count(pc);
                    for (int i = 0; i < cnt; ++i)
                        csa.Append(" ").Append(csa_piece[pc.ToInt()]);
                }
                csa.AppendLine();
            }
            // 手番
            csa.AppendLine(pos.sideToMove == Color.BLACK ? "+" : "-");
            return csa.ToString();
        }

        /// <summary>
        /// ある指し手をCSA形式で出力する
        /// Move.ToSfen()のCSA版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <param name="turnOut"></param>
        /// <returns></returns>
        public static string ToCSA(this Position pos, Move move, bool turnOut = true)
        {
            switch (move)
            {
                case Move.NONE:
                case Move.NULL:
                    return "%ERROR";
                case Move.RESIGN:
                    return "%TORYO";
                case Move.WIN:
                    return "%WIN";
                case Move.INTERRUPT:
                    return "%CHUDAN";
                case Move.REPETITION_DRAW:
                    return "%SENNICHITE";
                case Move.MAX_MOVES_DRAW:
                    return "%JISHOGI";
                case Move.TIME_UP:
                    return "%TIME_UP";
                // ToDo: 他にも対応を確認
            }
            if (move.IsSpecial())
            {
                return "";
            }

            StringBuilder csa = new StringBuilder();

            // 手番
            if (turnOut)
                csa.Append(pos.sideToMove != Color.WHITE ? "+" : "-");

            // 打つ指し手のときは移動元の升は"00"と表現する。
            // さもなくば、"77"のように移動元の升目をアラビア数字で表現。
            if (move.IsDrop())
                csa.Append("00");
            else
                csa.AppendFormat("{0}{1}", HW_NUMBER[move.From().ToFile().ToInt()], HW_NUMBER[move.From().ToRank().ToInt()]);

            csa.AppendFormat("{0}{1}", HW_NUMBER[move.To().ToFile().ToInt()], HW_NUMBER[move.To().ToRank().ToInt()]);

            Piece p = move.IsDrop() ? move.DroppedPiece() : pos.PieceOn(move.From());
            if (move.IsPromote()) p |= Piece.PROMOTE;
            csa.Append(csa_piece[p.ToInt()]);

            return csa.ToString();
        }

        /// <summary>
        /// CSA形式の指し手を与えて、Moveに変換する。指し手の合法性のチェックはここでは行わない。
        /// 変換に失敗したときはMove.NONEが返る。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="csaMove"></param>
        /// <returns></returns>
        public static Move FromCSA(this Position pos, string csaMove)
        {
            Match match = new Regex(@"^([+-]?)([0-9][0-9][1-9][1-9])(FU|KY|KE|GI|KI|KA|HI|OU|TO|NY|NK|NG|UM|RY)").Match(csaMove);
            if (!match.Success) return Move.NONE;
            Color c = pos.sideToMove;
            string mg1 = match.Groups[1].Value;
            if (mg1 == "+" && c != Color.BLACK) return Move.NONE;
            if (mg1 == "-" && c != Color.WHITE) return Move.NONE;
            string mg2 = match.Groups[2].Value;
            File f0 = (File)(mg2[0] - '1');
            Rank r0 = (Rank)(mg2[1] - '1');
            File f1 = (File)(mg2[2] - '1');
            Rank r1 = (Rank)(mg2[3] - '1');
            Square sq1 = Util.MakeSquare(f1, r1);
            Piece pt1 = FromCsaPieceType(match.Groups[3].Value);
            Piece pc1 = Util.MakePiece(c, pt1);
            if (f0 >= File.FILE_1 && r0 >= Rank.RANK_1)
            {
                Square sq0 = Util.MakeSquare(f0, r0);
                Piece pc0 = pos.PieceOn(sq0);
                if (pc0 == pc1) return Util.MakeMove(sq0, sq1);
                if (pc0.PieceColor() == c && pc0.RawPieceType() == pc1.RawPieceType() && pc1.IsPromote()) return Util.MakeMovePromote(sq0, sq1);
                return Move.NONE;
            }
            if (pt1.IsPromote()) return Move.NONE;
            return Util.MakeMoveDrop(pt1, sq1);
        }

        /// <summary>
        /// CSA形式の局面図をsfen形式にする
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="kif"></param>
        /// <returns></returns>
        public static string CsaToSfen(string[] csa)
        {
            var board = new Piece[81];
            for (int i = 0; i < 81; ++i) board[i] = Piece.NO_PIECE;
            var hand = new Hand[2];
            for (int i = 0; i < 2; ++i) hand[i] = Hand.ZERO;
            Color turn = Color.BLACK;
            int ply = 1;

            // 盤面一括
            Regex bRegex = new Regex(@"^P[1-9](?: \* |[+-](?:FU|KY|KE|GI|KI|KA|HI|OU|TO|NY|NK|NG|UM|RY)){9}");
            // 駒個別
            Regex hRegex = new Regex(@"^P[+-]((?:[0-9][0-9](?:FU|KY|KE|GI|KI|KA|HI|OU|TO|NY|NK|NG|UM|RY))+)");

            foreach (var line in csa)
            {
                if (line.StartsWith("+"))
                {
                    turn = Color.BLACK;
                    break;
                }
                if (line.StartsWith("-"))
                {
                    turn = Color.WHITE;
                    break;
                }
                if (!line.StartsWith("P"))
                {
                    continue;
                }
                if (line.StartsWith("PI"))
                {
                    // 平手初期配置
                    for (int i = 0; i < 81; ++i) board[i] = Piece.NO_PIECE;
                    board[Square.SQ_11.ToInt()] = Piece.W_LANCE;
                    board[Square.SQ_21.ToInt()] = Piece.W_KNIGHT;
                    board[Square.SQ_31.ToInt()] = Piece.W_SILVER;
                    board[Square.SQ_41.ToInt()] = Piece.W_GOLD;
                    board[Square.SQ_51.ToInt()] = Piece.W_KING;
                    board[Square.SQ_61.ToInt()] = Piece.W_GOLD;
                    board[Square.SQ_71.ToInt()] = Piece.W_SILVER;
                    board[Square.SQ_81.ToInt()] = Piece.W_KNIGHT;
                    board[Square.SQ_91.ToInt()] = Piece.W_LANCE;
                    board[Square.SQ_22.ToInt()] = Piece.W_BISHOP;
                    board[Square.SQ_82.ToInt()] = Piece.W_ROOK;
                    board[Square.SQ_13.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_23.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_33.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_43.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_53.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_63.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_73.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_83.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_93.ToInt()] = Piece.W_PAWN;
                    board[Square.SQ_17.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_27.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_37.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_47.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_57.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_67.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_77.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_87.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_97.ToInt()] = Piece.B_PAWN;
                    board[Square.SQ_28.ToInt()] = Piece.B_ROOK;
                    board[Square.SQ_88.ToInt()] = Piece.B_BISHOP;
                    board[Square.SQ_19.ToInt()] = Piece.B_LANCE;
                    board[Square.SQ_29.ToInt()] = Piece.B_KNIGHT;
                    board[Square.SQ_39.ToInt()] = Piece.B_SILVER;
                    board[Square.SQ_49.ToInt()] = Piece.B_GOLD;
                    board[Square.SQ_59.ToInt()] = Piece.B_KING;
                    board[Square.SQ_69.ToInt()] = Piece.B_GOLD;
                    board[Square.SQ_79.ToInt()] = Piece.B_SILVER;
                    board[Square.SQ_89.ToInt()] = Piece.B_KNIGHT;
                    board[Square.SQ_99.ToInt()] = Piece.B_LANCE;
                    // 駒落ちの検出
                    foreach (Match match in new Regex(@"^([1-9][1-9])(FU|KY|KE|GI|KI|KA|HI|OU)").Matches(line)
                    )
                    {
                        if (!match.Success) continue;
                        File f = (File)(match.Groups[1].Value[0] - '1');
                        Rank r = (Rank)(match.Groups[1].Value[1] - '1');
                        Square sq = Util.MakeSquare(f, r);
                        Piece pc = FromCsaPieceType(match.Groups[2].Value);
                        // そのマスに指定された駒が無かったらおかしい
                        if (board[sq.ToInt()].PieceType() != pc) return "";
                        board[sq.ToInt()] = Piece.NO_PIECE;
                    }
                    continue;
                }
                Match bMatch = bRegex.Match(line);
                if (bMatch.Success)
                {
                    Rank r = (Rank)(bMatch.Groups[0].Value[1] - '1');
                    for (File f = File.FILE_9; f >= File.FILE_1; --f)
                    {
                        Piece pc = FromCsaPiece(bMatch.Groups[0].Value.Substring((int)(File.FILE_9 - f) * 3 + 2, 3));
                        board[Util.MakeSquare(f, r).ToInt()] = pc;
                    }
                    continue;
                }
                if (line.StartsWith("P+00AL"))
                {
                    int[] restCount = { 2, 18, 4, 4, 4, 2, 2, 4 };
                    for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
                    {
                        var p = board[sq.ToInt()];
                        if (p == Piece.NO_PIECE) continue;
                        restCount[p.RawPieceType().ToInt()] -= 1;
                    }
                    for (Piece p = Piece.PAWN; p < Piece.KING; ++p)
                    {
                        restCount[p.ToInt()] -= hand[Color.BLACK.ToInt()].Count(p);
                        restCount[p.ToInt()] -= hand[Color.WHITE.ToInt()].Count(p);
                        hand[Color.BLACK.ToInt()].Add(p, SysMath.Max(restCount[p.ToInt()], 0));
                    }
                    continue;
                }
                if (line.StartsWith("P-00AL"))
                {
                    int[] restCount = { 2, 18, 4, 4, 4, 2, 2, 4 };
                    for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
                    {
                        var p = board[sq.ToInt()];
                        if (p == Piece.NO_PIECE) continue;
                        restCount[p.RawPieceType().ToInt()] -= 1;
                    }
                    for (Piece p = Piece.PAWN; p < Piece.KING; ++p)
                    {
                        restCount[p.ToInt()] -= hand[Color.BLACK.ToInt()].Count(p);
                        restCount[p.ToInt()] -= hand[Color.WHITE.ToInt()].Count(p);
                        hand[Color.WHITE.ToInt()].Add(p, SysMath.Max(restCount[p.ToInt()], 0));
                    }
                    continue;
                }
                Match hMatch = hRegex.Match(line);
                if (hMatch.Success)
                {
                    // 駒別単独表現
                    Color c = (hMatch.Groups[0].Value[1] != '-') ? Color.BLACK : Color.WHITE;
                    string resGroup = hMatch.Groups[1].Value;
                    for (int i = 0; i < resGroup.Length; i += 4)
                    {
                        File f = (File)(resGroup[i + 0] - '1');
                        Rank r = (Rank)(resGroup[i + 1] - '1');
                        Piece pc = FromCsaPieceType(resGroup.Substring(i + 2, 2));
                        if (f >= File.FILE_1 && r >= Rank.RANK_1)
                        {
                            board[Util.MakeSquare(f, r).ToInt()] = Util.MakePiece(c, pc);
                            continue;
                        }
                        if (pc.IsPromote()) return "";
                        hand[c.ToInt()].Add(pc);
                    }
                    continue;
                }
            }
            return Position.SfenFromRawdata(board, hand, turn, ply);
        }
        private static Piece FromCsaPiece(string s)
        {
            switch (s)
            {
                case "FU": return Piece.PAWN;
                case "KY": return Piece.LANCE;
                case "KE": return Piece.KNIGHT;
                case "GI": return Piece.SILVER;
                case "KI": return Piece.GOLD;
                case "KA": return Piece.BISHOP;
                case "HI": return Piece.ROOK;
                case "OU": return Piece.KING;
                case "TO": return Piece.PRO_PAWN;
                case "NY": return Piece.PRO_LANCE;
                case "NK": return Piece.PRO_KNIGHT;
                case "NG": return Piece.PRO_SILVER;
                case "UM": return Piece.HORSE;
                case "RY": return Piece.DRAGON;
                case " * ": return Piece.NO_PIECE;
                case "+FU": return Piece.B_PAWN;
                case "+KY": return Piece.B_LANCE;
                case "+KE": return Piece.B_KNIGHT;
                case "+GI": return Piece.B_SILVER;
                case "+KI": return Piece.B_GOLD;
                case "+KA": return Piece.B_BISHOP;
                case "+HI": return Piece.B_ROOK;
                case "+OU": return Piece.B_KING;
                case "+TO": return Piece.B_PRO_PAWN;
                case "+NY": return Piece.B_PRO_LANCE;
                case "+NK": return Piece.B_PRO_KNIGHT;
                case "+NG": return Piece.B_PRO_SILVER;
                case "+UM": return Piece.B_HORSE;
                case "+RY": return Piece.B_DRAGON;
                case "-FU": return Piece.W_PAWN;
                case "-KY": return Piece.W_LANCE;
                case "-KE": return Piece.W_KNIGHT;
                case "-GI": return Piece.W_SILVER;
                case "-KI": return Piece.W_GOLD;
                case "-KA": return Piece.W_BISHOP;
                case "-HI": return Piece.W_ROOK;
                case "-OU": return Piece.W_KING;
                case "-TO": return Piece.W_PRO_PAWN;
                case "-NY": return Piece.W_PRO_LANCE;
                case "-NK": return Piece.W_PRO_KNIGHT;
                case "-NG": return Piece.W_PRO_SILVER;
                case "-UM": return Piece.W_HORSE;
                case "-RY": return Piece.W_DRAGON;
                default: return Piece.NO_PIECE;
            }
        }
        private static Piece FromCsaPieceType(string s)
        {
            switch (s)
            {
                case "FU": return Piece.PAWN;
                case "KY": return Piece.LANCE;
                case "KE": return Piece.KNIGHT;
                case "GI": return Piece.SILVER;
                case "KI": return Piece.GOLD;
                case "KA": return Piece.BISHOP;
                case "HI": return Piece.ROOK;
                case "OU": return Piece.KING;
                case "TO": return Piece.PRO_PAWN;
                case "NY": return Piece.PRO_LANCE;
                case "NK": return Piece.PRO_KNIGHT;
                case "NG": return Piece.PRO_SILVER;
                case "UM": return Piece.HORSE;
                case "RY": return Piece.DRAGON;
                default: return Piece.NO_PIECE;
            }
        }
        private static string ToCsaPieceType(Piece pc)
        {
            return "* FUKYKEGIKAHIKIOUTONYNKNGUMRY".Substring(pc.PieceType().ToInt() * 2, 2);
        }
        private static string ToCsaPiece(Piece pc)
        {
            if (pc == Piece.NO_PIECE) return " * ";
            return (pc.PieceColor() == Color.BLACK ? "+" : "-") + ToCsaPieceType(pc);
        }
    }
}
