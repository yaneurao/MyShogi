using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Converter
{
    /// <summary>
    /// ToDo : なんやかや書くかも
    /// </summary>
    public class KifConverter
    {
    }

    public enum ColorFormat
    {
        NONE,
        CSA,
        KIF,
        KIFTurn,
        Piece,
        PieceTurn,
        NB
    }
    public enum SquareFormat
    {
        ASCII,
        FullWidthArabic,
        FullWidthMix,
        NB
    }
    public enum SamePosFormat
    {
        NONE,
        SHORT,
        KIFsp,
        KI2sp,
        Verbose,
        NB
    }

    public enum FromSqFormat
    {
        NONE,
        KIF,
        KI2,
        NB
    }

    public class KifFormatter
    {
        public ColorFormat colorFmt;
        public SquareFormat squareFmt;
        public SamePosFormat sameposFmt;
        public FromSqFormat fromsqFmt;

        public KifFormatter(
            ColorFormat colorFmt,
            SquareFormat squareFmt,
            SamePosFormat sameposFmt,
            FromSqFormat fromsqFmt
        )
        {
            this.colorFmt = colorFmt;
            this.squareFmt = squareFmt;
            this.sameposFmt = sameposFmt;
            this.fromsqFmt = fromsqFmt;
        }
        public static KifFormatter Kif() => new KifFormatter(
            ColorFormat.NONE,
            SquareFormat.FullWidthMix,
            SamePosFormat.KIFsp,
            FromSqFormat.KIF
        );
        public static KifFormatter Ki2() => new KifFormatter(
            ColorFormat.NONE,
            SquareFormat.FullWidthMix,
            SamePosFormat.KI2sp,
            FromSqFormat.KI2
        );

        private static readonly string[] PIECE_KIF = {
            "口",
            "歩","香","桂","銀","角","飛","金","玉",
            "と","成香","成桂","成銀","馬","龍","成金","王",
            "歩","香","桂","銀","角","飛","金","玉",
            "と","成香","成桂","成銀","馬","龍","成金","王"
        };
        private static readonly string[] CN_NUMBER = {
            "一","二","三","四","五","六","七","八","九",
        };
        private static readonly string[] FW_NUMBER = {
            "１","２","３","４","５","６","７","８","９",
        };
        private static readonly string[] HW_NUMBER = {
            "1","2","3","4","5","6","7","8","9",
        };

        public string format(Color c)
        {
            switch (colorFmt)
            {
                case ColorFormat.NONE: return "";
                case ColorFormat.CSA: switch (c)
                {
                    case Color.BLACK: return "+";
                    case Color.WHITE: return "-";
                    default: return "";
                }
                case ColorFormat.KIF: switch (c)
                {
                    case Color.BLACK: return "▲";
                    case Color.WHITE: return "△";
                    default: return "";
                }
                case ColorFormat.KIFTurn: switch (c)
                {
                    case Color.BLACK: return "▲";
                    case Color.WHITE: return "▽";
                    default: return "";
                }
                case ColorFormat.Piece: switch (c)
                {
                    case Color.BLACK: return "☗";
                    case Color.WHITE: return "☖";
                    default: return "";
                }
                case ColorFormat.PieceTurn: switch (c)
                {
                    case Color.BLACK: return "☗";
                    case Color.WHITE: return "⛉";
                    default: return "";
                }
                default: return "";
            }
        }
        public string format(Square sq)
        {
            File f = sq.ToFile();
            Rank r = sq.ToRank();
            switch (squareFmt)
            {
                case SquareFormat.ASCII:
                    return HW_NUMBER[(int)f] + HW_NUMBER[(int)r];
                case SquareFormat.FullWidthArabic:
                    return FW_NUMBER[(int)f] + FW_NUMBER[(int)r];
                case SquareFormat.FullWidthMix:
                    return FW_NUMBER[(int)f] + CN_NUMBER[(int)r];
                default:
                    return "";
            }
        }
        public string format(Position pos, Move move, Move lastMove)
        {
            string kif = format(pos.SideToMove);
            switch (move)
            {
                case Move.NONE:
                    return kif + "エラー";
                case Move.NULL:
                    return kif + "パス";
                case Move.RESIGN:
                    return kif + "投了";
                case Move.WIN:
                    return kif + "勝ち宣言";
            }
            // 普通の指し手
            // 着手元の駒種
            Piece fromPiece = pos.PieceOn(move.From());
            Piece fromPieceType = fromPiece.PieceType();
            if (lastMove.IsOk() && lastMove.To() == move.To())
            {
                // 一つ前の指し手の移動先と、今回の移動先が同じ場合、"同"金のように表示する。
                switch (sameposFmt)
                {
                    case SamePosFormat.SHORT:
                        kif += "同";
                        break;
                    // KIF形式では"同"の後に全角空白
                    case SamePosFormat.KIFsp:
                        kif += "同　";
                        break;
                    // KI2形式では成香・成桂・成銀・成り動作での空白は入らない
                    case SamePosFormat.KI2sp:
                        if (
                            move.IsPromote() ||
                            fromPieceType == Piece.PRO_LANCE ||
                            fromPieceType == Piece.PRO_KNIGHT ||
                            fromPieceType == Piece.PRO_SILVER
                        )
                        {
                            kif += "同";
                            break;
                        }
                        else
                        {
                            kif += "同　";
                            break;
                        }
                    // 座標 + "同"
                    case SamePosFormat.Verbose:
                        kif += format(move.To()) + "同";
                        break;
                }
            }
            else
            {
                kif += format(move.To());
            }
            kif += PIECE_KIF[(int)move.From()];
            if (move.IsDrop())
            {
                kif += "打";
            }
            else
            {
                switch (fromsqFmt) {
                    case FromSqFormat.NONE:
                        break;
                    case FromSqFormat.KIF:
                    {
                        if (move.IsPromote())
                        {
                            kif += "成";
                        }
                        else if (
                            !fromPiece.IsPromote() &&
                            (Core.Util.CanPromote(pos.SideToMove, move.To()) ||
                            Core.Util.CanPromote(pos.SideToMove, move.From()))
                        )
                        {
                            kif += "不成";
                        }
                        Square fromSquare = move.From();
                        kif += "(" +
                            HW_NUMBER[(int)move.From().ToFile()] +
                            HW_NUMBER[(int)move.From().ToRank()] +
                            ")";
                        break;
                    }
                    case FromSqFormat.KI2:
                    {
                        kif += fromSqFormat_KI2(pos, move);
                        if (move.IsPromote())
                        {
                            kif += "成";
                        }
                        else if (
                            !fromPiece.IsPromote() &&
                            (Core.Util.CanPromote(pos.SideToMove, move.To()) ||
                            Core.Util.CanPromote(pos.SideToMove, move.From()))
                        )
                        {
                            kif += "不成";
                        }
                        break;
                    }
                }
            }
            return kif;
        }
        static string fromSqFormat_KI2(Position pos, Move move)
        {
            Color c = pos.SideToMove;
            Square fromSq = move.From(), toSq = move.To();
            Piece p = pos.PieceOn(fromSq), pt = p.PieceType();
            Bitboard sameBB = pos.AttackersTo(toSq) & pos.Pieces(p);
            if (sameBB.IsOne()) return "";
            if (sameBB.IsZero()) throw new System.ArgumentException("maybe illegal move");
            if (bb_check(sameBB, Bitboard.RankBB(toSq.ToRank()), fromSq)) return "寄";
            if (bb_check(sameBB, dir_bb(c, toSq, Direct.D), fromSq)) return "上";
            if (bb_check(sameBB, dir_bb(c, toSq, Direct.U), fromSq)) return "引";
            if (bb_check(sameBB, line_dir_bb(c, toSq, Direct.D), fromSq) && (
                pt == Piece.SILVER || pt == Piece.GOLD ||
                pt == Piece.PRO_PAWN || pt == Piece.PRO_LANCE || pt == Piece.PRO_KNIGHT || pt == Piece.PRO_SILVER
            )) return "直";
            if (bb_check(sameBB, dir_or_bb(c, fromSq, Direct.L), fromSq)) return "左";
            if (bb_check(sameBB, dir_or_bb(c, fromSq, Direct.R), fromSq)) return "右";
            if (bb_check(sameBB, line_dir_bb(c, fromSq, Direct.L), fromSq)) return "左寄";
            if (bb_check(sameBB, line_dir_bb(c, fromSq, Direct.R), fromSq)) return "右寄";
            if (bb_check(sameBB, dir_bb(c, toSq, Direct.D) & dir_or_bb(c, fromSq, Direct.L), fromSq)) return "左上";
            if (bb_check(sameBB, dir_bb(c, toSq, Direct.D) & dir_or_bb(c, fromSq, Direct.R), fromSq)) return "右上";
            if (bb_check(sameBB, dir_bb(c, toSq, Direct.U) & dir_or_bb(c, fromSq, Direct.L), fromSq)) return "左引";
            if (bb_check(sameBB, dir_bb(c, toSq, Direct.U) & dir_or_bb(c, fromSq, Direct.R), fromSq)) return "右引";
            throw new System.ArgumentException("maybe illegal position/move");
        }
        static bool bb_check(Bitboard samebb, Bitboard dirbb, Square sq) => (dirbb.IsSet(sq) && (samebb & dirbb).IsOne());
        static Bitboard dir_bb_(Color color, Square sq, Direct dir, bool or_flag)
        {
            // color == Color.White ならば逆の方角にする
            dir = (color != Color.WHITE) ? dir : (7 - dir);

            int offset = or_flag ? 0 : 1;

            Bitboard bb = Bitboard.ZeroBB();
            switch (dir)
            {
                case Direct.R: for (File f = sq.ToFile() - offset; f >= File.FILE_1; f--) bb |= Bitboard.FileBB(f); break;
                case Direct.L: for (File f = sq.ToFile() + offset; f <= File.FILE_9; f++) bb |= Bitboard.FileBB(f); break;
                case Direct.U: for (Rank r = sq.ToRank() - offset; r >= Rank.RANK_1; r--) bb |= Bitboard.RankBB(r); break;
                case Direct.D: for (Rank r = sq.ToRank() + offset; r <= Rank.RANK_9; r++) bb |= Bitboard.RankBB(r); break;
                default: throw new System.ArgumentException("不適切な方向が指定されました");
            }

            return bb;
        }
        static Bitboard dir_bb(Color color, Square sq, Direct dir) => dir_bb_(color, sq, dir, false);
        static Bitboard dir_or_bb(Color color, Square sq, Direct dir) => dir_bb_(color, sq, dir, true);
        static Bitboard line_dir_bb(Color color, Square sq, Direct dir)
        {
            dir = (color != Color.WHITE) ? dir : (7 - dir);

            File f = sq.ToFile();
            Rank r = sq.ToRank();

            Bitboard bb = Bitboard.ZeroBB();
            switch (dir)
            {
                case Direct.R: for (File fi = f - 1; fi >= File.FILE_1; fi--) bb |= Bitboard.SquareBB(Core.Util.MakeSquare(fi, r)); break;
                case Direct.L: for (File fi = f + 1; fi <= File.FILE_9; fi++) bb |= Bitboard.SquareBB(Core.Util.MakeSquare(fi, r)); break;
                case Direct.U: for (Rank ri = r - 1; ri >= Rank.RANK_1; ri--) bb |= Bitboard.SquareBB(Core.Util.MakeSquare(f, ri)); break;
                case Direct.D: for (Rank ri = r + 1; ri <= Rank.RANK_9; ri++) bb |= Bitboard.SquareBB(Core.Util.MakeSquare(f, ri)); break;
                default: throw new System.ArgumentException("不適切な方向が指定されました");
            }
            return bb;
        }
    }

    /// <summary>
    /// kif形式の入出力
    /// </summary>
    public static class KifExtensions
    {
        private static readonly string[] PIECE_KIF = {
            "口",
            "歩","香","桂","銀","角","飛","金","玉",
            "と","成香","成桂","成銀","馬","龍","成金","王",
            "歩","香","桂","銀","角","飛","金","玉",
            "と","成香","成桂","成銀","馬","龍","成金","王"
        };
        private static readonly string[] PIECE_BOD = {
            " ・"," 歩"," 香"," 桂"," 銀"," 角"," 飛"," 金",
            " 玉"," と"," 杏"," 圭"," 全"," 馬"," 龍"," 菌",
            " ・","v歩","v香","v桂","v銀","v角","v飛","v金",
            "v玉","vと","v杏","v圭","v全","v馬","v龍","v菌",
        };
        private static readonly string[] CN_NUMBER = {
            "一","二","三","四","五","六","七","八","九",
        };
        private static readonly string[] HAND_NUM = {
            "", "", "二", "三", "四", "五", "六", "七", "八", "九",
            "十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八",
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
        /// 現在の局面図をBOD形式で出力する
        /// Position.ToSfen()のBOD版
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string ToBod(this Position pos)
        {
            string bod = "";
            // TODO: 後手対局者名
            // bod += "後手：" + whitePlayerName + "\n";
            // 後手の持駒
            Hand whiteHand = pos.Hand(Color.WHITE);
            if (whiteHand == Hand.ZERO)
            {
                bod += "後手の持駒：なし\n";
            }
            else
            {
                bod += "後手の持駒：";
                foreach (Piece pc in HAND_ORDER)
                {
                    int cnt = whiteHand.Count(pc);
                    if (cnt > 0)
                    {
                        bod += PIECE_KIF[(int)pc] + HAND_NUM[cnt] + " ";
                    }
                }
                bod += "\n";
            }
            // 盤面
            bod += "  ９ ８ ７ ６ ５ ４ ３ ２ １\n";
            bod += "+---------------------------+\n";
            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; r++)
            {
                bod += "|";
                for (File f = File.FILE_9; f >= File.FILE_1; f--)
                {
                    bod += PIECE_BOD[(int)pos.PieceOn(Shogi.Core.Util.MakeSquare(f, r))];
                }
                bod += "|" + CN_NUMBER[(int)r] + "\n";
            }
            bod += "+---------------------------+\n";
            // TODO: 先手対局者名
            // bod += "先手：" + blackPlayerName + "\n";
            // 先手の持駒
            Hand blackHand = pos.Hand(Color.BLACK);
            if (blackHand == Hand.ZERO)
            {
                bod += "先手の持駒：なし\n";
            }
            else
            {
                bod += "先手の持駒：";
                foreach (Piece pc in HAND_ORDER)
                {
                    int cnt = blackHand.Count(pc);
                    if (cnt > 0)
                    {
                        bod += PIECE_KIF[(int)pc] + HAND_NUM[cnt] + " ";
                    }
                }
                bod += "\n";
            }
            // TODO: 現在手数、直前の指し手出力
            // bod += "手数＝" + (string)pos.gamePly + "  " + lastMoveWithSideColor + "  まで\n";
            // 後手番のみ追加行
            if (pos.SideToMove == Color.WHITE)
            {
                bod += "後手番\n";
            }

            return bod;
        }

        /// <summary>
        /// ある指し手をKIF形式で出力する
        /// Move.ToSfen()のKIF版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static string ToKif(this Position pos, Move move, Move lastMove = Move.NONE) =>
            KifFormatter.Kif().format(pos, move, lastMove);

        /// <summary>
        /// ある指し手をKI2形式で出力する
        /// Move.ToSfen()のKI2版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static string ToKi2(this Position pos, Move move, Move lastMove = Move.NONE) =>
            KifFormatter.Ki2().format(pos, move, lastMove);

        /// <summary>
        /// KIF/KI2形式の指し手を与えて、Moveに変換する。指し手の合法性のチェックはここでは行わない。
        /// 変換に失敗したときはMove.NONEが返る。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="kifMove"></param>
        /// <returns></returns>
        public static Move FromKif(this Position pos, string kifMove)
        {
            // ToDo : あとで実装する
            return Move.NONE;
        }

        /// <summary>
        /// BOD形式の局面図をsfen形式にする
        /// </summary>
        /// <param name="bod"></param>
        /// <returns></returns>
        public static string BodToSfen(string bod)
        {
            return "";
        }
    }
}
