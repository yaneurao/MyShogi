using System;
using System.Text;
using System.Text.RegularExpressions;
using SysMath = System.Math;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Converter
{

    // KIF/KI2形式の文字列を取り扱うクラス群

    /// <summary>
    /// 手番を示す符号
    /// </summary>
    public enum ColorFormat
    {
        /// <summary>
        /// 手番符号なし
        /// </summary>
        NONE,
        /// <summary>
        /// +, -
        /// </summary>
        CSA,
        /// <summary>
        /// ▲, △
        /// </summary>
        KIF,
        /// <summary>
        /// ▲, ▽
        /// </summary>
        KIFTurn,
        /// <summary>
        /// ☗, ☖
        /// </summary>
        Piece,
        /// <summary>
        /// ☗, ⛉
        /// </summary>
        PieceTurn,
        NB
    }

    /// <summary>
    /// 到達地点の筋・段を示す数字の文字種
    /// </summary>
    public enum SquareFormat
    {
        /// <summary>
        /// 23銀
        /// </summary>
        ASCII,
        /// <summary>
        /// ２３銀
        /// </summary>
        FullWidthArabic,
        /// <summary>
        /// ２三銀
        /// </summary>
        FullWidthMix,
        NB
    }

    /// <summary>
    /// 1手前と同じ地点に駒を動かした場合の表記
    /// </summary>
    public enum SamePosFormat
    {
        /// <summary>
        /// ２三銀成, ２三銀
        /// </summary>
        NONE,
        /// <summary>
        /// 同銀成, 同銀
        /// </summary>
        ZEROsp,
        /// <summary>
        /// 同　銀成, 同　銀
        /// </summary>
        KIFsp,
        /// <summary>
        /// 同銀成, 同　銀
        /// </summary>
        KI2sp,
        /// <summary>
        /// ２三同銀成, ２三同銀
        /// </summary>
        Verbose,
        NB
    }

    /// <summary>
    /// 移動元の駒を判別するための表記
    /// </summary>
    public enum FromSqFormat
    {
        /// <summary>
        /// ２三銀
        /// </summary>
        NONE,
        /// <summary>
        /// ２三銀(14)
        /// </summary>
        KIF,
        /// <summary>
        /// ２三銀右
        /// </summary>
        KI2,
        /// <summary>
        /// ２三銀右(14)
        /// </summary>
        Verbose,
        NB
    }

    /// <summary>
    /// 棋譜書式設定オプション
    /// </summary>
    public interface IKifFormatterOptions
    {
        ColorFormat color { get; }
        SquareFormat square { get; }
        SamePosFormat samepos { get; }
        FromSqFormat fromsq { get; }
    }

    /// <summary>
    /// 変更可能な棋譜書式設定オプション
    /// </summary>
    public class KifFormatterOptions : IKifFormatterOptions
    {
        public ColorFormat color { get; set; } = ColorFormat.NONE;
        public SquareFormat square { get; set; } = SquareFormat.FullWidthMix;
        public SamePosFormat samepos { get; set; } = SamePosFormat.KI2sp;
        public FromSqFormat fromsq { get; set; } = FromSqFormat.KI2;

        public KifFormatterOptions() {}
        public KifFormatterOptions(IKifFormatterOptions opt)
        {
            color = opt.color;
            square = opt.square;
            samepos = opt.samepos;
            fromsq = opt.fromsq;
        }
        public KifFormatterOptions(
            ColorFormat _color,
            SquareFormat _square,
            SamePosFormat _samepos,
            FromSqFormat _fromsq
        )
        {
            color = _color;
            square = _square;
            samepos = _samepos;
            fromsq = _fromsq;
        }
    }

    /// <summary>
    /// 変更不可能な棋譜書式設定オプション
    /// </summary>
    public class KifFormatterImmutableOptions : IKifFormatterOptions
    {
        public ColorFormat color { get; }
        public SquareFormat square { get; }
        public SamePosFormat samepos { get; }
        public FromSqFormat fromsq { get; }

        public KifFormatterImmutableOptions(IKifFormatterOptions opt)
        {
            color = opt.color;
            square = opt.square;
            samepos = opt.samepos;
            fromsq = opt.fromsq;
        }
        public KifFormatterImmutableOptions(
            ColorFormat _color,
            SquareFormat _square,
            SamePosFormat _samepos,
            FromSqFormat _fromsq
        )
        {
            color = _color;
            square = _square;
            samepos = _samepos;
            fromsq = _fromsq;
        }
    }

    public static class KifFormatter
    {

        // singleton object

        /// <summary>
        /// KIF(手番文字なし)
        /// </summary>
        public static KifFormatterImmutableOptions Kif { get; } = new KifFormatterImmutableOptions(
            ColorFormat.NONE,
            SquareFormat.FullWidthMix,
            SamePosFormat.KIFsp,
            FromSqFormat.KIF
        );

        /// <summary>
        /// KIF(▲△手番文字あり)
        /// </summary>
        public static KifFormatterImmutableOptions KifC { get; } = new KifFormatterImmutableOptions(
            ColorFormat.KIF,
            SquareFormat.FullWidthMix,
            SamePosFormat.KIFsp,
            FromSqFormat.KIF
        );

        /// <summary>
        /// KI2(手番文字なし)
        /// </summary>
        public static KifFormatterImmutableOptions Ki2 { get; } = new KifFormatterImmutableOptions(
            ColorFormat.NONE,
            SquareFormat.FullWidthMix,
            SamePosFormat.KI2sp,
            FromSqFormat.KI2
        );

        /// <summary>
        /// KI2(▲△手番文字あり)
        /// </summary>
        public static KifFormatterImmutableOptions Ki2C { get; } = new KifFormatterImmutableOptions(
            ColorFormat.KIF,
            SquareFormat.FullWidthMix,
            SamePosFormat.KI2sp,
            FromSqFormat.KI2
        );

        /// <summary>
        /// KI2(手番文字なし,最終着手/RootPV表示向け)
        /// 例: 同２三銀右
        /// </summary>
        public static KifFormatterImmutableOptions Ki2Root { get; } = new KifFormatterImmutableOptions(
            ColorFormat.NONE,
            SquareFormat.FullWidthMix,
            SamePosFormat.Verbose,
            FromSqFormat.KI2
        );

        /// <summary>
        /// KI2(▲△手番文字あり,最終着手/RootPV表示向け)
        /// 例: ▲同２三銀右
        /// </summary>
        public static KifFormatterImmutableOptions Ki2CRoot { get; } = new KifFormatterImmutableOptions(
            ColorFormat.KIF,
            SquareFormat.FullWidthMix,
            SamePosFormat.Verbose,
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

        public static string format(this IKifFormatterOptions opt, Color c)
        {
            switch (opt.color)
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
        public static string format(this IKifFormatterOptions opt, Square sq)
        {
            File f = sq.ToFile();
            Rank r = sq.ToRank();
            switch (opt.square)
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
        public static string format(this IKifFormatterOptions opt, Position pos, Move move)
        {
            var state = pos.State();
            return opt.format(pos, move, state == null ? Move.NONE : state.lastMove);
        }
        public static string format(this IKifFormatterOptions opt, Position pos, Move move, Move lastMove)
        {
            StringBuilder kif = new StringBuilder();
            kif.Append(opt.format(pos.sideToMove));

            if (!move.IsOk())
            {
                switch (move)
                {
                    case Move.NONE:
                        return kif.Append("NONE").ToString();
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
            var moveInfo = new KifMoveInfo(pos, move);
            var fromPieceType = moveInfo.fromPt;
            // 普通の指し手
            if (moveInfo.same)
            {
                // 一つ前の指し手の移動先と、今回の移動先が同じ場合、"同"金のように表示する。
                switch (opt.samepos)
                {
                    case SamePosFormat.NONE:
                        break;
                    case SamePosFormat.ZEROsp:
                        kif.Append("同");
                        break;
                    // KIF形式では"同"の後に全角空白
                    case SamePosFormat.KIFsp:
                        kif.Append("同　");
                        break;
                    // KI2形式では成香・成桂・成銀・成り・不成・相対位置・動作での空白は入らない
                    case SamePosFormat.KI2sp: {
                        if (
                            move.IsPromote() ||
                            !fromPieceType.IsPromote() &&
                            fromPieceType != Piece.GOLD &&
                            fromPieceType != Piece.KING &&
                            (
                                Util.CanPromote(pos.sideToMove, move.From()) ||
                                Util.CanPromote(pos.sideToMove, move.To())
                            ) ||
                            moveInfo.relative != KifMoveInfo.Relative.NONE ||
                            moveInfo.behavior != KifMoveInfo.Behavior.NONE ||
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
                        kif.Append(opt.format(move.To())).Append("同");
                        break;
                    default:
                        throw new ConverterException();
                }
            }
            else
            {
                kif.Append(opt.format(move.To()));
            }
            kif.Append(PIECE_KIF[fromPieceType.ToInt()]);
            switch (opt.fromsq) {
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
                        else if (moveInfo.promote == KifMoveInfo.Promote.NOPROMOTE)
                        {
                            kif.Append("不成");
                        }
                        kif.AppendFormat("({0}{1})",
                            HW_NUMBER[move.From().ToFile().ToInt()],
                            HW_NUMBER[move.From().ToRank().ToInt()]
                        );
                    }
                    break;
                }
                case FromSqFormat.KI2:
                case FromSqFormat.Verbose:
                {
                    if (moveInfo.drop == KifMoveInfo.Drop.EXPLICIT)
                    {
                        // KI2では紛らわしくない場合、"打"と表記しない。
                        kif.Append("打");
                        break;
                    }
                    switch (moveInfo.relative)
                    {
                        case KifMoveInfo.Relative.LEFT:     kif.Append("左"); break;
                        case KifMoveInfo.Relative.STRAIGHT: kif.Append("直"); break;
                        case KifMoveInfo.Relative.RIGHT:    kif.Append("右"); break;
                    }
                    switch (moveInfo.behavior)
                    {
                        case KifMoveInfo.Behavior.FORWARD:  kif.Append("上"); break;
                        case KifMoveInfo.Behavior.SLIDE:    kif.Append("寄"); break;
                        case KifMoveInfo.Behavior.BACKWARD: kif.Append("引"); break;
                    }
                    if (move.IsPromote())
                    {
                        kif.Append("成");
                    }
                    else if (moveInfo.promote == KifMoveInfo.Promote.NOPROMOTE)
                    {
                        kif.Append("不成");
                    }
                    if (opt.fromsq == FromSqFormat.Verbose)
                    {
                        kif.AppendFormat("({0}{1})",
                            HW_NUMBER[move.From().ToFile().ToInt()],
                            HW_NUMBER[move.From().ToRank().ToInt()]
                        );
                    }
                    break;
                }
                default:
                    throw new ConverterException();
            }
            return kif.ToString();
        }
    }

    /// <summary>
    /// 指し手情報の中間形式
    /// 棋譜読み上げ、棋譜ファイル出力等に使えるかも？
    /// (JKF形式への出力の際に使用)
    /// </summary>
    public struct KifMoveInfo
    {
        public int ply { get; }
        public Color turn { get; }
        public Move nextMove { get; }
        public Move lastMove { get; }
        public Piece fromPc { get; }
        public Piece toPc { get; }
        public Piece capPc { get; }
        public Piece fromPt { get => fromPc.PieceType(); }
        public Piece toPt { get => toPc.PieceType(); }
        public Piece capPt { get => capPc.PieceType(); }
        public Piece fromPr { get => fromPc.RawPieceType(); }
        public Piece toPr { get => toPc.RawPieceType(); }
        public Piece capPr { get => capPc.RawPieceType(); }
        public Bitboard sameBB { get; }
        public bool same { get; }
        public Relative relative { get; }
        public Behavior behavior { get; }
        public Promote promote { get; }
        public Drop drop { get; }
        public bool legal { get; }
        public bool special { get => nextMove.IsSpecial(); }
        /// <summary>
        /// 相対位置
        /// </summary>
        public enum Relative {
            /// <summary>
            /// 相対表記不要
            /// </summary>
            NONE,
            /// <summary>
            /// "左"
            /// </summary>
            LEFT,
            /// <summary>
            /// "直"
            /// </summary>
            STRAIGHT,
            /// <summary>
            /// "右"
            /// </summary>
            RIGHT
        }
        /// <summary>
        /// 動作
        /// </summary>
        public enum Behavior {
            /// <summary>
            /// 動作表記不要
            /// </summary>
            NONE,
            /// <summary>
            /// "上"
            /// </summary>
            FORWARD,
            /// <summary>
            /// "寄"
            /// </summary>
            SLIDE,
            /// <summary>
            /// "引"
            /// </summary>
            BACKWARD
        }
        /// <summary>
        /// 成/不成
        /// </summary>
        public enum Promote {
            /// <summary>
            /// 成/不成に関係ない着手（敵陣への駒打ちもこちら）
            /// </summary>
            NONE,
            /// <summary>
            /// "不成"
            /// </summary>
            NOPROMOTE,
            /// <summary>
            /// "成"
            /// </summary>
            PROMOTE
        }
        /// <summary>
        /// 駒打ち
        /// </summary>
        public enum Drop {
            /// <summary>
            /// 駒打ちではない
            /// </summary>
            NONE,
            /// <summary>
            /// 暗黙の駒打ち（省略可）
            /// </summary>
            IMPLICIT,
            /// <summary>
            /// 明示的な駒打ち（省略不可）
            /// </summary>
            EXPLICIT
        }
        public KifMoveInfo(Position pos, Move move)
        {
            var state = pos.State();
            ply = pos.gamePly;
            turn = pos.sideToMove;
            nextMove = move;
            lastMove = state != null ? state.lastMove : Move.NONE;
            if (move.IsSpecial())
            {
                fromPc = toPc = capPc = Piece.NO_PIECE;
                sameBB = Bitboard.ZeroBB();
                same = false;
                relative = Relative.NONE;
                behavior = Behavior.NONE;
                promote = Promote.NONE;
                drop = Drop.NONE;
                legal = pos.IsLegal(move);
            }
            else if (move.IsDrop())
            {
                fromPc = toPc = Util.MakePiece(turn, move.DroppedPiece());
                capPc = pos.PieceOn(move.To());
                sameBB = pos.AttackersTo(turn, move.To()) & pos.Pieces(turn, move.DroppedPiece());
                same = lastMove.IsSpecial() ? false : (lastMove.To() == move.To());
                relative = Relative.NONE;
                behavior = Behavior.NONE;
                promote = move.IsPromote() ? Promote.PROMOTE : Promote.NONE;
                drop = sameBB.IsZero() ? Drop.IMPLICIT : Drop.EXPLICIT;
                legal = pos.IsLegal(move);
            }
            else
            {
                var fromSq = move.From();
                var toSq = move.To();
                fromPc = pos.PieceOn(move.From());
                var fromPt = fromPc.PieceType();
                toPc = move.IsPromote() ? Util.MakePiecePromote(turn, fromPt) : fromPc;
                capPc = pos.PieceOn(move.To());
                sameBB = pos.AttackersTo(turn, move.To()) & pos.Pieces(turn, fromPc.PieceType());
                same = lastMove.IsSpecial() ? false : (lastMove.To() == move.To());

                if (!sameBB.IsSet(fromSq) || sameBB.IsOne()) {
                    // ""
                    relative = Relative.NONE;
                    behavior = Behavior.NONE;
                }
                else if (KifUtil.checkBB(sameBB, Bitboard.RankBB(toSq.ToRank()), fromSq))
                {
                    // "寄"
                    relative = Relative.NONE;
                    behavior = Behavior.SLIDE;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.dirBB(turn, move.To(), Direct.D), fromSq))
                {
                    // "上"
                    relative = Relative.NONE;
                    behavior = Behavior.FORWARD;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.dirBB(turn, toSq, Direct.U), fromSq))
                {
                    // "引"
                    relative = Relative.NONE;
                    behavior = Behavior.BACKWARD;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.lineDirBB(turn, toSq, Direct.D), fromSq) && (
                    fromPt == Piece.SILVER || fromPt == Piece.GOLD ||
                    fromPt == Piece.PRO_PAWN || fromPt == Piece.PRO_LANCE ||
                    fromPt == Piece.PRO_KNIGHT || fromPt == Piece.PRO_SILVER
                ))
                {
                    // "直"
                    relative = Relative.STRAIGHT;
                    behavior = Behavior.NONE;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.dirOrBB(turn, fromSq, Direct.L), fromSq))
                {
                    // "左"
                    relative = Relative.LEFT;
                    behavior = Behavior.NONE;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.dirOrBB(turn, fromSq, Direct.R), fromSq))
                {
                    // "右"
                    relative = Relative.RIGHT;
                    behavior = Behavior.NONE;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.lineDirOrBB(turn, fromSq, Direct.L) & Bitboard.RankBB(toSq.ToRank()), fromSq))
                {
                    // "左寄"
                    relative = Relative.LEFT;
                    behavior = Behavior.SLIDE;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.lineDirOrBB(turn, fromSq, Direct.R) & Bitboard.RankBB(toSq.ToRank()), fromSq))
                {
                    // "右寄"
                    relative = Relative.RIGHT;
                    behavior = Behavior.SLIDE;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.dirBB(turn, toSq, Direct.D) & KifUtil.dirOrBB(turn, fromSq, Direct.L), fromSq))
                {
                    // "左上"
                    relative = Relative.LEFT;
                    behavior = Behavior.FORWARD;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.dirBB(turn, toSq, Direct.D) & KifUtil.dirOrBB(turn, fromSq, Direct.R), fromSq))
                {
                    // "右上"
                    relative = Relative.RIGHT;
                    behavior = Behavior.FORWARD;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.dirBB(turn, toSq, Direct.U) & KifUtil.dirOrBB(turn, fromSq, Direct.L), fromSq))
                {
                    // "左引"
                    relative = Relative.LEFT;
                    behavior = Behavior.BACKWARD;
                }
                else if (KifUtil.checkBB(sameBB, KifUtil.dirBB(turn, toSq, Direct.U) & KifUtil.dirOrBB(turn, fromSq, Direct.R), fromSq))
                {
                    // "右引"
                    relative = Relative.RIGHT;
                    behavior = Behavior.BACKWARD;
                }
                else
                {
                    // 通常、ここまで到達することはないはず
                    relative = Relative.NONE;
                    behavior = Behavior.NONE;
                }

                promote = move.IsPromote() ? Promote.PROMOTE :
                    (
                        !fromPt.IsPromote() &&
                        fromPt != Piece.GOLD &&
                        fromPt != Piece.KING &&
                        (
                            Util.CanPromote(pos.sideToMove, move.From()) ||
                            Util.CanPromote(pos.sideToMove, move.To())
                        )
                    ) ? Promote.NOPROMOTE : Promote.NONE;
                drop = Drop.NONE;
                legal = pos.IsLegal(move);
            }
        }
    }

    public static class KifUtil
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
        public static string ToKif(this Position pos, Move move) =>
            KifFormatter.Kif.format(pos, move);

        /// <summary>
        /// ある指し手をKIF形式で出力する
        /// Move.ToSfen()のKIF版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static string ToKifC(this Position pos, Move move) =>
            KifFormatter.KifC.format(pos, move);

        /// <summary>
        /// ある指し手をKI2形式で出力する
        /// Move.ToSfen()のKI2版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static string ToKi2(this Position pos, Move move) =>
            KifFormatter.Ki2.format(pos, move);

        /// <summary>
        /// ある指し手をKI2形式で出力する
        /// Move.ToSfen()のKI2版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static string ToKi2C(this Position pos, Move move) =>
            KifFormatter.Ki2C.format(pos, move);

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
