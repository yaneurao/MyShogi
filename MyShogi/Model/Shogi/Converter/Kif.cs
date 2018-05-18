using MyShogi.Model.Shogi.Core;
using System.Text;

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
        private static bool DEBUG = true;
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
                    default: throw new ConverterException();
                }
                case ColorFormat.KIF: switch (c)
                {
                    case Color.BLACK: return "▲";
                    case Color.WHITE: return "△";
                    default: throw new ConverterException();
                }
                case ColorFormat.KIFTurn: switch (c)
                {
                    case Color.BLACK: return "▲";
                    case Color.WHITE: return "▽";
                    default: throw new ConverterException();
                }
                case ColorFormat.Piece: switch (c)
                {
                    case Color.BLACK: return "☗";
                    case Color.WHITE: return "☖";
                    default: throw new ConverterException();
                }
                case ColorFormat.PieceTurn: switch (c)
                {
                    case Color.BLACK: return "☗";
                    case Color.WHITE: return "⛉";
                    default: throw new ConverterException();
                }
                default: throw new ConverterException();
            }
        }
        public string format(Square sq)
        {
            File f = sq.ToFile();
            Rank r = sq.ToRank();
            switch (squareFmt)
            {
                case SquareFormat.ASCII:
                    return HW_NUMBER[f.ToInt()] + HW_NUMBER[r.ToInt()];
                case SquareFormat.FullWidthArabic:
                    return FW_NUMBER[f.ToInt()] + FW_NUMBER[r.ToInt()];
                case SquareFormat.FullWidthMix:
                    return FW_NUMBER[f.ToInt()] + CN_NUMBER[r.ToInt()];
                default:
                    throw new ConverterException();
            }
        }
        public string format(Position pos, Move move, Move lastMove)
        {
            StringBuilder kif = new StringBuilder();
            kif.Append(format(pos.SideToMove));
            switch (move)
            {
                case Move.NONE:
                    return kif.Append("エラー").ToString();
                case Move.NULL:
                    return kif.Append("パス").ToString();
                case Move.RESIGN:
                    return kif.Append("投了").ToString();
                case Move.WIN:
                    return kif.Append("勝ち宣言").ToString();
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
                    case SamePosFormat.NONE:
                        break;
                    case SamePosFormat.SHORT:
                        kif.Append("同");
                        break;
                    // KIF形式では"同"の後に全角空白
                    case SamePosFormat.KIFsp:
                        kif.Append("同　");
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
                            kif.Append("同");
                            break;
                        }
                        else
                        {
                            kif.Append("同　");
                            break;
                        }
                    // 座標 + "同"
                    case SamePosFormat.Verbose:
                        kif.Append(format(move.To())).Append("同");
                        break;
                    default:
                        throw new ConverterException();
                }
            }
            else
            {
                kif.Append(format(move.To()));
            }
            kif.Append(PIECE_KIF[pos.PieceOn(move.From()).ToInt()]);
            switch (fromsqFmt) {
                case FromSqFormat.NONE:
                    break;
                case FromSqFormat.KIF:
                {
                    if (move.IsDrop())
                    {
                        // KIF形式では持駒からの着手は必ず"打"と表記する
                        kif.Append("打");
                    } else {
                        if (move.IsPromote())
                        {
                            kif.Append("成");
                        }
                        else if (
                            !fromPiece.IsPromote() &&
                            (Core.Util.CanPromote(pos.SideToMove, move.To()) ||
                            Core.Util.CanPromote(pos.SideToMove, move.From()))
                        )
                        {
                            kif.Append("不成");
                        }
                        Square fromSquare = move.From();
                        kif.AppendFormat("({0}{1})",
                            HW_NUMBER[move.From().ToFile().ToInt()],
                            HW_NUMBER[move.From().ToRank().ToInt()]
                        );
                    }
                    break;
                }
                case FromSqFormat.KI2:
                {
                    if (move.IsDrop())
                    {
                        // KI2では紛らわしくない場合、"打"と表記しない。
                        Bitboard sameBB = pos.AttackersTo(pos.SideToMove, move.To()) & pos.Pieces(pos.SideToMove, move.DroppedPiece());
                        if (!sameBB.IsZero()) kif.Append("打");
                        break;
                    }
                    kif.Append(fromSqFormat_KI2(pos, move));
                    if (move.IsPromote())
                    {
                        kif.Append("成");
                        break;
                    }
                    if (
                        !fromPiece.IsPromote() &&
                        (Core.Util.CanPromote(pos.SideToMove, move.To()) ||
                        Core.Util.CanPromote(pos.SideToMove, move.From()))
                    )
                    {
                        kif.Append("不成");
                    }
                    break;
                }
                default:
                    throw new ConverterException();
            }
            return kif.ToString();
        }
        static string fromSqFormat_KI2(Position pos, Move move)
        {
            Color c = pos.SideToMove;
            Square fromSq = move.From(), toSq = move.To();
            Piece p = pos.PieceOn(fromSq), pt = p.PieceType();
            Bitboard sameBB = pos.AttackersTo(c, toSq) & pos.Pieces(c, pt);
            if (!sameBB.IsSet(fromSq))
            {
                // 異常な局面・指し手をわざと食わせる必要がある場合はどうするべき？
                if (DEBUG)
                    throw new ConverterException("Position(" + pos.Pretty() + ") で Move(" + move.Pretty() + ") は不正な着手のようです");
                else
                    return "";
            }
            if (sameBB.IsOne()) return "";
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
            // 正常な局面・指し手でここに到達する筈は無い
            // 異常な局面・指し手をわざと食わせる必要がある場合はどうするべき？
            if (DEBUG)
                throw new ConverterException("Position(" + pos.Pretty() + ") で Move(" + move.Pretty() + ") の表記に失敗しました");
            else
                return "";
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
                default: throw new ConverterException("不適切な方向が指定されました");
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
                default: throw new ConverterException("不適切な方向が指定されました");
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
            "と","成香","成桂","成銀","馬","龍","成金","王",
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
            StringBuilder bod = new StringBuilder();
            // TODO: 後手対局者名
            // 対局者名はここに書かずにヘッダ部に書くべき？
            // bod.AppendFormat("後手：{0}", whitePlayerName).appendLine();
            // 後手の持駒
            Hand whiteHand = pos.Hand(Color.WHITE);
            if (whiteHand == Hand.ZERO)
            {
                bod.AppendLine("後手の持駒：なし");
            }
            else
            {
                bod.Append("後手の持駒：");
                foreach (Piece pc in HAND_ORDER)
                {
                    int cnt = whiteHand.Count(pc);
                    if (cnt > 0)
                    {
                        bod.AppendFormat("{0}{1} ", PIECE_KIF[pc.ToInt()], HAND_NUM[cnt]);
                    }
                }
                bod.AppendLine();
            }
            // 盤面
            bod.AppendLine("  ９ ８ ７ ６ ５ ４ ３ ２ １");
            bod.AppendLine("+---------------------------+");
            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; r++)
            {
                bod.Append("|");
                for (File f = File.FILE_9; f >= File.FILE_1; f--)
                {
                    bod.Append(PIECE_BOD[pos.PieceOn(Core.Util.MakeSquare(f, r)).ToInt()]);
                }
                bod.AppendFormat("|{0}", CN_NUMBER[r.ToInt()]).AppendLine();
            }
            bod.AppendLine("+---------------------------+");
            // TODO: 先手対局者名
            // 対局者名はここに書かずにヘッダ部に書くべき？
            // bod.AppendFormat("先手：{0}", blackPlayerName).appendLine();
            // 先手の持駒
            Hand blackHand = pos.Hand(Color.BLACK);
            if (blackHand == Hand.ZERO)
            {
                bod.AppendLine("先手の持駒：なし");
            }
            else
            {
                bod.Append("先手の持駒：");
                foreach (Piece pc in HAND_ORDER)
                {
                    int cnt = blackHand.Count(pc);
                    if (cnt > 0)
                    {
                        bod.AppendFormat("{0}{1} ", PIECE_KIF[pc.ToInt()], HAND_NUM[cnt]);
                    }
                }
                bod.AppendLine();
            }
            // TODO: 現在手数、直前の指し手出力
            // pos.gamePly が private。直前の指し手を出力するには一旦undoMove()して出力すべきか？
            //
            // bod.AppendFormat("手数＝{0}  {1}  まで", pos.gamePly - 1, pos.State().lastMove).appendLine();
            // 後手番のみ追加行
            if (pos.SideToMove == Color.WHITE)
            {
                bod.AppendLine("後手番");
            }

            return bod.ToString();
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
