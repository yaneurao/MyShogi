using System;
using System.Text;
using System.Text.RegularExpressions;
using SysMath = System.Math;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Converter
{

    // KIF/KI2形式の文字列を取り扱うクラス群

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

        // singleton object
        public static KifFormatter Kif { get; private set; } = new KifFormatter(
            ColorFormat.NONE,
            SquareFormat.FullWidthMix,
            SamePosFormat.KIFsp,
            FromSqFormat.KIF
        );

        public static KifFormatter Ki2 { get; private set; } = new KifFormatter(
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
            kif.Append(format(pos.sideToMove));
            if (move == Move.NONE)
                return kif.Append("NONE").ToString();

            if (move.IsSpecial())
            {
                switch (move)
                {
                    case Move.NULL:
                        return kif.Append("パス").ToString();
                    case Move.RESIGN:
                        return kif.Append("投了").ToString();
                    case Move.WIN:
                        return kif.Append("勝ち宣言").ToString();

                        // 以下、棋譜ウィンドウへの出力で必要なので追加
                    case Move.MATED:
                        return kif.Append("詰み").ToString();
                    case Move.REPETITION_DRAW:
                        return kif.Append("千日手").ToString();
                    case Move.REPETITION_WIN:
                        return kif.Append("連続王手千日手反則勝ち").ToString();
                    case Move.REPETITION_LOSE:
                        return kif.Append("連続王手千日手反則負け").ToString();
                }
            }
            Piece fromPieceType = move.IsDrop() ? move.DroppedPiece() : pos.PieceOn(move.From()).PieceType();
            // 普通の指し手
            if (!move.IsDrop() && lastMove.IsOk() && lastMove.To() == move.To())
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
                    case SamePosFormat.KI2sp: {
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
            kif.Append(PIECE_KIF[fromPieceType.ToInt()]);
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
                            !fromPieceType.IsPromote() &&
                            fromPieceType != Piece.GOLD &&
                            fromPieceType != Piece.KING &&
                            (Util.CanPromote(pos.sideToMove, move.To()) ||
                            Util.CanPromote(pos.sideToMove, move.From()))
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
                        Bitboard sameBB = pos.AttackersTo(pos.sideToMove, move.To()) & pos.Pieces(pos.sideToMove, move.DroppedPiece());
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
                        !fromPieceType.IsPromote() &&
                        fromPieceType != Piece.GOLD &&
                        fromPieceType != Piece.KING &&
                        (Util.CanPromote(pos.sideToMove, move.To()) ||
                        Util.CanPromote(pos.sideToMove, move.From()))
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
            Color c = pos.sideToMove;
            Square fromSq = move.From(), toSq = move.To();
            Piece p = pos.PieceOn(fromSq), pt = p.PieceType();
            Bitboard sameBB = pos.AttackersTo(c, toSq) & pos.Pieces(c, pt);
            if (!sameBB.IsSet(fromSq)) return "";
            if (sameBB.IsOne()) return "";
            if (KifUtil.checkBB(sameBB, Bitboard.RankBB(toSq.ToRank()), fromSq)) return "寄";
            if (KifUtil.checkBB(sameBB, KifUtil.dirBB(c, toSq, Direct.D), fromSq)) return "上";
            if (KifUtil.checkBB(sameBB, KifUtil.dirBB(c, toSq, Direct.U), fromSq)) return "引";
            if (KifUtil.checkBB(sameBB, KifUtil.lineDirBB(c, toSq, Direct.D), fromSq) && (
                pt == Piece.SILVER || pt == Piece.GOLD ||
                pt == Piece.PRO_PAWN || pt == Piece.PRO_LANCE || pt == Piece.PRO_KNIGHT || pt == Piece.PRO_SILVER
            )) return "直";
            if (KifUtil.checkBB(sameBB, KifUtil.dirOrBB(c, fromSq, Direct.L), fromSq)) return "左";
            if (KifUtil.checkBB(sameBB, KifUtil.dirOrBB(c, fromSq, Direct.R), fromSq)) return "右";
            if (KifUtil.checkBB(sameBB, KifUtil.lineDirOrBB(c, fromSq, Direct.L) & Bitboard.RankBB(toSq.ToRank()), fromSq)) return "左寄";
            if (KifUtil.checkBB(sameBB, KifUtil.lineDirOrBB(c, fromSq, Direct.R) & Bitboard.RankBB(toSq.ToRank()), fromSq)) return "右寄";
            if (KifUtil.checkBB(sameBB, KifUtil.dirBB(c, toSq, Direct.D) & KifUtil.dirOrBB(c, fromSq, Direct.L), fromSq)) return "左上";
            if (KifUtil.checkBB(sameBB, KifUtil.dirBB(c, toSq, Direct.D) & KifUtil.dirOrBB(c, fromSq, Direct.R), fromSq)) return "右上";
            if (KifUtil.checkBB(sameBB, KifUtil.dirBB(c, toSq, Direct.U) & KifUtil.dirOrBB(c, fromSq, Direct.L), fromSq)) return "左引";
            if (KifUtil.checkBB(sameBB, KifUtil.dirBB(c, toSq, Direct.U) & KifUtil.dirOrBB(c, fromSq, Direct.R), fromSq)) return "右引";
            // 正常な局面・指し手でここに到達する筈は無い
            return "";
        }
    }

    static class KifUtil
    {
        public static bool checkBB(Bitboard samebb, Bitboard dirbb, Square sq) => (dirbb.IsSet(sq) && (samebb & dirbb).IsOne());
        static Bitboard dirBB_(Color color, Square sq, Direct dir, bool or_flag)
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
        public static Bitboard dirBB(Color color, Square sq, Direct dir) => dirBB_(color, sq, dir, false);
        public static Bitboard dirOrBB(Color color, Square sq, Direct dir) => dirBB_(color, sq, dir, true);
        public static Bitboard lineDirBB_(Color color, Square sq, Direct dir, bool or_flag)
        {
            dir = (color != Color.WHITE) ? dir : (7 - dir);
            int offset = or_flag ? 0 : 1;
            File f = sq.ToFile();
            Rank r = sq.ToRank();
            Bitboard bb = Bitboard.ZeroBB();
            switch (dir)
            {
                case Direct.R: for (File fi = f - offset; fi >= File.FILE_1; fi--) bb |= Bitboard.SquareBB(Util.MakeSquare(fi, r)); break;
                case Direct.L: for (File fi = f + offset; fi <= File.FILE_9; fi++) bb |= Bitboard.SquareBB(Util.MakeSquare(fi, r)); break;
                case Direct.U: for (Rank ri = r - offset; ri >= Rank.RANK_1; ri--) bb |= Bitboard.SquareBB(Util.MakeSquare(f, ri)); break;
                case Direct.D: for (Rank ri = r + offset; ri <= Rank.RANK_9; ri++) bb |= Bitboard.SquareBB(Util.MakeSquare(f, ri)); break;
                default: throw new ConverterException("不適切な方向が指定されました");
            }
            return bb;
        }
        public static Bitboard lineDirBB(Color color, Square sq, Direct dir) => lineDirBB_(color, sq, dir, false);
        public static Bitboard lineDirOrBB(Color color, Square sq, Direct dir) => lineDirBB_(color, sq, dir, true);
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
                    bod.Append(PIECE_BOD[pos.PieceOn(Util.MakeSquare(f, r)).ToInt()]);
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
            if (pos.sideToMove == Color.WHITE)
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
            KifFormatter.Kif.format(pos, move, lastMove);

        /// <summary>
        /// ある指し手をKI2形式で出力する
        /// Move.ToSfen()のKI2版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static string ToKi2(this Position pos, Move move, Move lastMove = Move.NONE) =>
            KifFormatter.Ki2.format(pos, move, lastMove);

        /// <summary>
        /// KIF/KI2形式の指し手を与えて、Moveに変換する。指し手の合法性のチェックはここでは行わない。
        /// 変換に失敗したときはMove.NONEが返る。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="kifMove"></param>
        /// <returns></returns>
        public static Move FromKif(this Position pos, string kifMove)
        {
            // match.Group[1]: ([▲△▼▽☗☖⛊⛉]?)
            // match.Group[2]: ([1-9１２３４５６７８９一二三四五六七八九]{2}同?)
            // match.Group[3]: (同)
            // match.Group[4]: ([歩香桂銀金角飛玉王と杏圭全馬竜龍]|成[香桂銀])
            // match.Group[5]: ([右左]?[上寄引]?|直)
            // match.Group[6]: (成|不成|打)?
            // match.Group[7]: (\([1-9]{2}\))?
            Match match = new Regex(@"([▲△▼▽☗☖⛊⛉]?)(?:([1-9１２３４５６７８９一二三四五六七八九]{2}同?)|(同))[　 ]*([歩香桂銀金角飛玉王と杏圭全馬竜龍]|成[香桂銀])(直|[右左]?[上寄引]?)(成|不成|打)?(\([1-9]{2}\))?").Match(kifMove);
            if (!match.Success) return Move.NONE;
            Color c0 = pos.sideToMove;
            Color c1 = FromColor(match.Groups[1].Value);
            Square sq0;
            Square sq1 = match.Groups[3].Value == "同" ? pos.State().lastMove.To() : FromSquare(match.Groups[2].Value);
            if (sq1 == Square.NB) return Move.NONE;
            Piece pt1 = FromKifPieceType(match.Groups[4].Value);
            // 駒打ち
            if (match.Groups[6].Value == "打") return Util.MakeMoveDrop(pt1, sq1);
            // 成チェック
            bool isPromote = (match.Groups[6].Value == "成");
            // KIF形式で元座標が示されている場合
            if (match.Groups[7].Value.Length == 4)
            {
                sq0 = FromSquare(match.Groups[7].Value.Substring(1, 2));
                return isPromote ? Util.MakeMovePromote(sq0, sq1) : Util.MakeMove(sq0, sq1);
            }
            // KI2棋譜用処理
            // 駒の移動範囲を調べるため、着手前の駒を求める
            Piece pt0 = isPromote ? pt1.RawPieceType() : pt1;
            Bitboard sameBB = pos.AttackersTo(c0, sq1) & pos.Pieces(c0, pt0);
            if (sameBB.IsZero()) return Util.MakeMoveDrop(pt1, sq1);
            if (sameBB.IsOne())
            {
                sq0 = sameBB.Pop();
                return isPromote ? Util.MakeMovePromote(sq0, sq1) : Util.MakeMove(sq0, sq1);
            }
            Bitboard bb;
            switch (match.Groups[5].Value)
            {
                case "右":
                    sq0 = rightMost(c0, sameBB);
                    break;
                case "左":
                    sq0 = leftMost(c0, sameBB);
                    break;
                case "直":
                    bb = (KifUtil.lineDirBB(c0, sq1, Direct.D) & sameBB);
                    if (!bb.IsOne()) return Move.NONE;
                    sq0 = bb.Pop();
                    break;
                case "上":
                    bb = (KifUtil.dirBB(c0, sq1, Direct.D) & sameBB);
                    if (!bb.IsOne()) return Move.NONE;
                    sq0 = bb.Pop();
                    break;
                case "引":
                    bb = (KifUtil.dirBB(c0, sq1, Direct.U) & sameBB);
                    if (!bb.IsOne()) return Move.NONE;
                    sq0 = bb.Pop();
                    break;
                case "寄":
                    bb = (Bitboard.RankBB(sq1.ToRank()) & sameBB);
                    if (!bb.IsOne()) return Move.NONE;
                    sq0 = bb.Pop();
                    break;
                case "右上":
                    bb = (KifUtil.dirBB(c0, sq1, Direct.D) & sameBB);
                    sq0 = rightMost(c0, bb);
                    break;
                case "左上":
                    bb = (KifUtil.dirBB(c0, sq1, Direct.D) & sameBB);
                    sq0 = leftMost(c0, bb);
                    break;
                case "右引":
                    bb = (KifUtil.dirBB(c0, sq1, Direct.U) & sameBB);
                    sq0 = rightMost(c0, bb);
                    break;
                case "左引":
                    bb = (KifUtil.dirBB(c0, sq1, Direct.U) & sameBB);
                    sq0 = leftMost(c0, bb);
                    break;
                case "右寄":
                    bb = (Bitboard.RankBB(sq1.ToRank()) & sameBB);
                    sq0 = rightMost(c0, bb);
                    break;
                case "左寄":
                    bb = (Bitboard.RankBB(sq1.ToRank()) & sameBB);
                    sq0 = leftMost(c0, bb);
                    break;
                default: return Move.NONE;
            }
            if (sq0 == Square.NB) return Move.NONE;
            return isPromote ? Util.MakeMovePromote(sq0, sq1) : Util.MakeMove(sq0, sq1);
        }
        // bbの中で最も右にあるSquareを検出する。全く無かったり右に複数あったりすればSquare.NBを返す。
        private static Square rightMost(Color c, Bitboard bb)
        {
            switch (c)
            {
                case Color.BLACK: return rightMost(bb);
                case Color.WHITE: return leftMost(bb);
                default: return Square.NB;
            }
        }
        // bbの中で最も左にあるSquareを検出する。全く無かったり左に複数あったりすればSquare.NBを返す。
        private static Square leftMost(Color c, Bitboard bb)
        {
            switch (c)
            {
                case Color.BLACK: return leftMost(bb);
                case Color.WHITE: return rightMost(bb);
                default: return Square.NB;
            }
        }
        // bbの中で最も右にあるSquareを検出する。全く無かったり右に複数あったりすればSquare.NBを返す。
        private static Square rightMost(Bitboard bb)
        {
            for (File f = File.FILE_1; f <= File.FILE_9; ++f)
            {
                Bitboard bbF = (bb & Bitboard.FileBB(f));
                if (bbF.IsZero()) continue;
                if (bbF.IsOne()) return bbF.Pop();
                return Square.NB;
            }
            return Square.NB;
        }
        // bbの中で最も左にあるSquareを検出する。全く無かったり左に複数あったりすればSquare.NBを返す。
        private static Square leftMost(Bitboard bb)
        {
            for (File f = File.FILE_9; f >= File.FILE_1; --f)
            {
                Bitboard bbF = (bb & Bitboard.FileBB(f));
                if (bbF.IsZero()) continue;
                if (bbF.IsOne()) return bbF.Pop();
                return Square.NB;
            }
            return Square.NB;
        }

        /// <summary>
        /// BOD形式の局面図をsfen形式にする
        /// </summary>
        /// <param name="bod"></param>
        /// <returns></returns>
        public static string BodToSfen(string[] bod)
        {
            var board = new Piece[81];
            for (int i = 0; i < 81; ++i) board[i] = Piece.NO_PIECE;
            var hand = new Hand[2];
            for (int i = 0; i < 2; ++i) hand[i] = Hand.ZERO;
            Color turn = Color.BLACK;
            int ply = 1;

            // 盤上の駒
            var bRegex = new Regex(@"^\|((?:[ v][・歩香桂銀金角飛玉王と杏圭全馬龍竜]){9})\|([一二三四五六七八九])");
            // 持駒
            var hRegex = new Regex(@"(歩香桂銀金角飛)(十[一二三四五六七八]|[一二三四五六七八九])?");

            foreach (var line in bod)
            {
                if (line.StartsWith("先手の持駒：") || line.StartsWith("下手の持駒："))
                {
                    hand[Color.BLACK.ToInt()] = Hand.ZERO;
                    var hMatches = hRegex.Matches(line);
                    foreach (Match hMatch in hMatches)
                    {
                        hand[Color.BLACK.ToInt()].Add(FromKifPieceType(hMatch.Groups[1].Value), SysMath.Max(FromNum(hMatch.Groups[2].Value), 1));
                    }
                    continue;
                }
                if (line.StartsWith("後手の持駒：") || line.StartsWith("上手の持駒："))
                {
                    hand[Color.WHITE.ToInt()] = Hand.ZERO;
                    var hMatches = hRegex.Matches(line);
                    foreach (Match hMatch in hMatches)
                    {
                        hand[Color.WHITE.ToInt()].Add(FromKifPieceType(hMatch.Groups[1].Value), SysMath.Max(FromNum(hMatch.Groups[2].Value), 1));
                    }
                    continue;
                }
                if (line.StartsWith("先手番") || line.StartsWith("下手番"))
                {
                    turn = Color.BLACK;
                }
                if (line.StartsWith("後手番") || line.StartsWith("上手番"))
                {
                    turn = Color.WHITE;
                }
                if (line.StartsWith("手数＝"))
                {
                    Match plyMatch = new Regex(@"^手数＝([0-9]+)").Match(line);
                    if (plyMatch.Success)
                    {
                        ply = int.Parse(plyMatch.Groups[1].Value) + 1;
                    }
                    // 手数と同じ行にある最終着手の情報はここでは取り込まない
                    continue;
                }
                if (line.StartsWith("|"))
                {
                    Match bMatch = bRegex.Match(line);
                    if (bMatch.Success)
                    {
                        Rank r = (Rank)(FromNum(bMatch.Groups[2].Value) - 1);
                        for (File f = File.FILE_9; f >= File.FILE_1; --f)
                        {
                            board[Util.MakeSquare(f, r).ToInt()] = FromKifPiece(bMatch.Groups[1].Value.Substring((int)(File.FILE_9 - f) * 2, 2));
                        }
                    }
                    continue;
                }
            }
            return Position.SfenFromRawdata(board, hand, turn, ply);
        }
        private static Piece FromKifPiece(string str)
        {
            switch (str)
            {
                case " ・": return Piece.NO_PIECE;
                case " 歩": return Piece.B_PAWN;
                case " 香": return Piece.B_LANCE;
                case " 桂": return Piece.B_KNIGHT;
                case " 銀": return Piece.B_SILVER;
                case " 角": return Piece.B_BISHOP;
                case " 飛": return Piece.B_ROOK;
                case " 金": return Piece.B_GOLD;
                case " 玉": return Piece.B_KING;
                case " 王": return Piece.B_KING;
                case " と": return Piece.B_PRO_PAWN;
                case " 杏": return Piece.B_PRO_LANCE;
                case " 圭": return Piece.B_PRO_KNIGHT;
                case " 全": return Piece.B_PRO_SILVER;
                case " 馬": return Piece.B_HORSE;
                case " 龍": return Piece.B_DRAGON;
                case " 竜": return Piece.B_DRAGON;
                case "v歩": return Piece.W_PAWN;
                case "v香": return Piece.W_LANCE;
                case "v桂": return Piece.W_KNIGHT;
                case "v銀": return Piece.W_SILVER;
                case "v角": return Piece.W_BISHOP;
                case "v飛": return Piece.W_ROOK;
                case "v金": return Piece.W_GOLD;
                case "v玉": return Piece.W_KING;
                case "v王": return Piece.W_KING;
                case "vと": return Piece.W_PRO_PAWN;
                case "v杏": return Piece.W_PRO_LANCE;
                case "v圭": return Piece.W_PRO_KNIGHT;
                case "v全": return Piece.W_PRO_SILVER;
                case "v馬": return Piece.W_HORSE;
                case "v龍": return Piece.W_DRAGON;
                case "v竜": return Piece.W_DRAGON;
                case "歩": return Piece.PAWN;
                case "歩兵": return Piece.PAWN;
                case "香": return Piece.LANCE;
                case "香車": return Piece.LANCE;
                case "桂": return Piece.KNIGHT;
                case "桂馬": return Piece.KNIGHT;
                case "銀": return Piece.SILVER;
                case "銀将": return Piece.SILVER;
                case "角": return Piece.BISHOP;
                case "角行": return Piece.BISHOP;
                case "飛": return Piece.ROOK;
                case "飛車": return Piece.ROOK;
                case "金": return Piece.GOLD;
                case "金将": return Piece.GOLD;
                case "玉": return Piece.KING;
                case "玉将": return Piece.KING;
                case "王": return Piece.KING;
                case "王将": return Piece.KING;
                case "と": return Piece.PRO_PAWN;
                case "と金": return Piece.PRO_PAWN;
                case "杏": return Piece.PRO_LANCE;
                case "成香": return Piece.PRO_LANCE;
                case "圭": return Piece.PRO_KNIGHT;
                case "成桂": return Piece.PRO_KNIGHT;
                case "全": return Piece.PRO_SILVER;
                case "成銀": return Piece.PRO_SILVER;
                case "馬": return Piece.HORSE;
                case "竜馬": return Piece.HORSE;
                case "龍馬": return Piece.HORSE;
                case "龍": return Piece.DRAGON;
                case "龍王": return Piece.DRAGON;
                case "竜": return Piece.DRAGON;
                case "竜王": return Piece.DRAGON;
                default: return Piece.NO_PIECE;
            }
        }
        private static Piece FromKifPieceType(string str)
        {
            // 解釈できない時はPiece.NO_PIECEを返す
            switch (str)
            {
                case "歩": return Piece.PAWN;
                case "歩兵": return Piece.PAWN;
                case "香": return Piece.LANCE;
                case "香車": return Piece.LANCE;
                case "桂": return Piece.KNIGHT;
                case "桂馬": return Piece.KNIGHT;
                case "銀": return Piece.SILVER;
                case "銀将": return Piece.SILVER;
                case "角": return Piece.BISHOP;
                case "角行": return Piece.BISHOP;
                case "飛": return Piece.ROOK;
                case "飛車": return Piece.ROOK;
                case "金": return Piece.GOLD;
                case "金将": return Piece.GOLD;
                case "玉": return Piece.KING;
                case "玉将": return Piece.KING;
                case "王": return Piece.KING;
                case "王将": return Piece.KING;
                case "と": return Piece.PRO_PAWN;
                case "と金": return Piece.PRO_PAWN;
                case "杏": return Piece.PRO_LANCE;
                case "成香": return Piece.PRO_LANCE;
                case "圭": return Piece.PRO_KNIGHT;
                case "成桂": return Piece.PRO_KNIGHT;
                case "全": return Piece.PRO_SILVER;
                case "成銀": return Piece.PRO_SILVER;
                case "馬": return Piece.HORSE;
                case "竜馬": return Piece.HORSE;
                case "龍馬": return Piece.HORSE;
                case "龍": return Piece.DRAGON;
                case "龍王": return Piece.DRAGON;
                case "竜": return Piece.DRAGON;
                case "竜王": return Piece.DRAGON;
                default: return Piece.NO_PIECE;
            }
        }
        private static Color FromColor(string colorStr)
        {
            // 解釈できない時はColor.NBを返す
            switch (colorStr)
            {
                case "▲": return Color.BLACK;
                case "△": return Color.WHITE;
                case "▼": return Color.BLACK;
                case "▽": return Color.WHITE;
                case "☗": return Color.BLACK;
                case "☖": return Color.WHITE;
                case "⛊": return Color.BLACK;
                case "⛉": return Color.WHITE;
                case "先手": return Color.BLACK;
                case "後手": return Color.WHITE;
                case "下手": return Color.BLACK;
                case "上手": return Color.WHITE;
                default: return Color.NB;
            }
        }
        private static Square FromSquare(string sqStr)
        {
            // 座標文字列からSquareに（解釈出来ない時はSquare.NBを返す）
            if (sqStr.Length < 2) return Square.NB;
            int FileNum = FromNum(sqStr.Substring(0, 1));
            int RankNum = FromNum(sqStr.Substring(1, 1));
            if (FileNum < 1 || FileNum > 9) return Square.NB;
            if (RankNum < 1 || RankNum > 9) return Square.NB;
            return Util.MakeSquare((File)(FileNum - 1), (Rank)(RankNum - 1));
        }
        private static int FromNum(string numStr)
        {
            // 解釈できない時は-1を返す
            switch (numStr)
            {
                case "0": return 0;
                case "1": return 1;
                case "2": return 2;
                case "3": return 3;
                case "4": return 4;
                case "5": return 5;
                case "6": return 6;
                case "7": return 7;
                case "8": return 8;
                case "9": return 9;
                case "０": return 0;
                case "１": return 1;
                case "２": return 2;
                case "３": return 3;
                case "４": return 4;
                case "５": return 5;
                case "６": return 6;
                case "７": return 7;
                case "８": return 8;
                case "９": return 9;
                case "〇": return 0;
                case "零": return 0;
                case "一": return 1;
                case "二": return 2;
                case "三": return 3;
                case "四": return 4;
                case "五": return 5;
                case "六": return 6;
                case "七": return 7;
                case "八": return 8;
                case "九": return 9;
                case "一〇": return 10;
                case "一一": return 11;
                case "一二": return 12;
                case "一三": return 13;
                case "一四": return 14;
                case "一五": return 15;
                case "一六": return 16;
                case "一七": return 17;
                case "一八": return 18;
                case "一九": return 19;
                case "十": return 10;
                case "十一": return 11;
                case "十二": return 12;
                case "十三": return 13;
                case "十四": return 14;
                case "十五": return 15;
                case "十六": return 16;
                case "十七": return 17;
                case "十八": return 18;
                case "十九": return 19;
                default: return -1;
            }
        }
    }
}
