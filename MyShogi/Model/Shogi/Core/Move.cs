using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 指し手を表現するenum
    /// 指し手 bit0..6 = 移動先のSquare、bit7..13 = 移動元のSquare(駒打ちのときは駒種)、bit14..駒打ちか、bit15..成りか
    /// </summary>
    public enum Move : UInt16
    {
        NONE = 0,             // 無効な移動

        DROP    = 1 << 14,       // 駒打ちフラグ
        PROMOTE = 1 << 15,       // 駒成りフラグ

        // 将棋のある局面の合法手の最大数。593らしいが、保険をかけて少し大きめにしておく。
        MAX_MOVES = 600,

        // 以下は、やねうら王から変更して、USIの通常の指し手文字列から変換したときに取りえない特殊な値にしておく。
        // 以下のことをspecial moveと呼び、Move.IsSpecial()でtrueが返る。

        SPECIAL = DROP + PROMOTE,

        NULL   ,  // NULL MOVEを意味する指し手
        RESIGN ,  // << で出力したときに"resign"と表示する投了を意味する指し手。自分による手番時の投了。
        WIN    ,  // 入玉時の宣言勝ちのために使う特殊な指し手
        WIN_THEM, // トライルールにおいて相手に入玉された局面であった
        DRAW   ,  // 引き分け。(CSAプロトコルにある) 引き分けの原因は不明。
        MATED  ,  // 詰み(合法手がない)局面(手番側が詰まされていて合法手がない) 
        REPETITION     , // 千日手(PSN形式で、DRAWかWINかわからない"Sennichite"という文字列が送られてくるのでその解釈用)
        REPETITION_DRAW, // 千日手引き分け
        REPETITION_WIN , // 千日手勝ち(相手の連続王手)
        REPETITION_LOSE, // 千日手負け(自分の連続王手)　この値は使うことはないはず
        TIME_UP        , // 時間切れによる負け
        INTERRUPT      , // ゲーム中断
        MAX_MOVES_DRAW , // 最大手数に達したために引き分け
        ILLEGAL_MOVE   , // 不正な指し手などによる反則負け
        ILLEGAL_ACTION_WIN , // 相手の不正なアクション(非手番の時に指し手を送ったなど)による反則勝ち(CSAプロトコルにある)
        ILLEGAL_ACTION_LOSE, // 自分の不正なアクション(手番時に送ってはいけない改行を送ったなど)による反則負け(CSAプロトコルにある)

        MATE_ENGINE_NO_MATE,         // 不詰を表現している。"go mate"に対してcheckmate nomateが返ってきたときにこれを用いる。
        MATE_ENGINE_NOT_IMPLEMENTED, // 手番側に王手がかかっている局面の詰検討は出来ません
    }

    /// <summary>
    /// special moveの指し手が勝ち・負け・引き分けのいずれに属するかを判定する時の結果
    /// </summary>
    public enum MoveGameResult
    {
        WIN,  // 勝ち
        LOSE, // 負け
        DRAW, // 引き分け
        UNKNOWN, // 分類不可のもの
    }

    /// <summary>
    /// Moveに関するextension methods
    /// </summary>
    public static class MoveExtensions
    {

        /// <summary>
        /// 指し手がおかしくないかをテストする
        /// ただし、盤面のことは考慮していない。
        /// Move.NONEとspecial moveのみがfalse。その他はtrue。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsOk(this Move m)
        {
            return !(m == Move.NONE || m.IsSpecial());
        }

        /// <summary>
        /// Uint32型へ変換。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Int32 ToInt(this Move m)
        {
            return (Int32)m;
        }

        /// <summary>
        /// 指し手がSPECIALな指し手(DoMove出来ない)であるかを判定する。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsSpecial(this Move m)
        {
            return m >= Move.SPECIAL;
        }

        /// <summary>
        /// mが、勝ち・負け・引き分けのいずれに属するかを返す。
        /// mは specail moveでなければならない。
        /// 
        /// 連続自己対局の時に結果の勝敗を判定する時などに用いる。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static MoveGameResult GameResult(this Move m)
        {
            Debug.Assert(m.IsSpecial());

            switch (m)
            {
                case Move.WIN:
                case Move.REPETITION_WIN:
                case Move.ILLEGAL_ACTION_WIN:
                    return MoveGameResult.WIN;

                case Move.RESIGN:
                case Move.MATED:
                case Move.REPETITION_LOSE:
                case Move.ILLEGAL_MOVE:
                case Move.TIME_UP:
                case Move.ILLEGAL_ACTION_LOSE:
                case Move.WIN_THEM:
                    return MoveGameResult.LOSE;

                case Move.DRAW:
                case Move.MAX_MOVES_DRAW:
                case Move.REPETITION_DRAW:
                    return MoveGameResult.DRAW;

                case Move.NULL:       // これもないと思うが..
                case Move.REPETITION: // 実際には使わない。PSNなどでこれがあるが、連続王手の千日手も含まれていて勝敗不明。
                case Move.INTERRUPT:  // 中断も決着がついていないので不明扱い。
                    return MoveGameResult.UNKNOWN;

                default:
                    return MoveGameResult.UNKNOWN;
            }
        }

        /// <summary>
        /// Move.IsOk()ではない指し手に対して棋譜ウィンドウで使うような文字列化を行う。
        /// KIF2ではきちんと規定されていないのでこれらの特別な指し手は棋譜ウィンドウでの表示において、
        /// 自前で文字列化しなくてはならない。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string SpecialMoveToKif(this Move m)
        {
            if (!m.IsOk())
                switch (m)
                {
                    // 棋譜ウィンドウへの出力では6文字目までしか入らないので
                    // 6文字目でちょん切られることを考慮して文字を決めないといけない。

                    case Move.NONE:            return "none"; // これは使わないはず
                    case Move.NULL:            return "null";
                    case Move.RESIGN:          return "投了";
                    case Move.WIN:             return "入玉宣言勝ち";
                    case Move.WIN_THEM:        return "入玉トライ勝ち";
                    case Move.DRAW:            return "引き分け";
                    case Move.MATED:           return "詰み";
                    case Move.REPETITION:      return "千日手";
                    case Move.REPETITION_DRAW: return "千日手引分";
                    case Move.REPETITION_WIN:  return "千日手反則勝ち";
                    case Move.REPETITION_LOSE: return "千日手反則負け";
                    case Move.TIME_UP:         return "時間切れ";
                    case Move.INTERRUPT:       return "中断";
                    case Move.MAX_MOVES_DRAW:  return "最大手数引分";
                    case Move.ILLEGAL_MOVE:    return "非合法手反則負け";
                    case Move.ILLEGAL_ACTION_WIN : return "反則勝ち";
                    case Move.ILLEGAL_ACTION_LOSE: return "反則負け";

                    case Move.MATE_ENGINE_NO_MATE:         return "不詰";                                               // 詰将棋エンジンで用いる
                    case Move.MATE_ENGINE_NOT_IMPLEMENTED: return "手番側に王手がかかっている局面の詰検討は出来ません"; // 詰将棋エンジンで用いる

                    default: return "UNKNOWN"; // おかしい。なんだろう..
                }
            else
                // エラーにはしないが..
                return "NonSpecialMove";
        }


        /// <summary>
        /// 見た目に、わかりやすい形式で表示する
        /// (盤面情報がないので移動させる駒がわからない。デバッグ用)
        /// </summary>
        public static string Pretty(this Move m)
        {
            if (m.IsDrop())
                return string.Format("{0}{1}打",m.To().Pretty() , m.DroppedPiece().Pretty2());
            else
                return string.Format("{0}{1}{2}",m.From().Pretty() , m.To().Pretty() , m.IsPromote() ? "成" : "");
        }

        /// <summary>
        /// 移動させた駒がわかっているときに指し手をわかりやすい表示形式で表示する。
        /// (盤面情報がないので、移動元候補の駒が複数ある場合は区別が出来ない。デバッグ用)
        /// </summary>
        public static string Pretty(this Move m, Piece movedPieceType)
        {
            if (m.IsDrop())
                return string.Format("{0}{1}打" , m.To().Pretty() , movedPieceType.Pretty() );
            else
                return string.Format("{0}{1}{2}",m.To().Pretty() , movedPieceType.Pretty() , m.IsPromote() ? "成" : "");

        }

        /// <summary>
        /// 指し手をUSI形式の文字列にする。
        ///
        /// 特殊な指し手は、Move.RESIGN , Move.WIN , Move.NULLしかサポートしていない。
        /// (USIでこれ以外の特殊な指し手は規定されていないため。
        /// "null"はUSIでサポートされていないが、Null Move Pruningを表現するのに使うことがあるので入れておく。)
        /// </summary>
        public static string ToUsi(this Move m)
        {
            if (m == Move.NONE)
                return "none";

            if (m.IsSpecial())
                return ((m == Move.RESIGN) ? "resign" :
                        (m == Move.WIN)    ? "win" :
                        (m == Move.NULL)   ? "null" :
                    "");

            else if (m.IsDrop())
                return string.Format("{0}*{1}", m.DroppedPiece().ToUsi(), m.To().ToUsi());

            else
                return string.Format("{0}{1}{2}",m.From().ToUsi() , m.To().ToUsi() , m.IsPromote() ? "+" : "");
        }

        /// <summary>
        /// 指し手の移動元の升を返す。
        /// </summary>
        public static Square From(this Move m)
        {
            // 駒打ちに対するmove_from()の呼び出しは不正。
            Debug.Assert((Move.DROP.ToInt() & m.ToInt()) == 0);

            return (Square)((m.ToInt() >> 7) & 0x7f);
        }

        /// <summary>
        /// 指し手の移動先の升を返す。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Square To(this Move m)
        {
            return (Square)(m.ToInt() & 0x7f);
        }

        /// <summary>
        /// 指し手が駒打ちか？
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsDrop(this Move m)
        {
            return (m.ToInt() & (UInt16)Move.DROP) != 0;
        }

        /// <summary>
        /// 指し手が成りか？
        /// </summary>
        public static bool IsPromote(this Move m)
        {
            return (m.ToInt() & (UInt16)Move.PROMOTE) != 0;
        }

        /// <summary>
        /// 駒打ち(is_drop()==true)のときの打った駒
        /// 先後の区別なし。PAWN～ROOKまでの値が返る。
        /// </summary>
        public static Piece DroppedPiece(this Move m)
        {
            return (Piece)((m.ToInt() >> 7) & 0x7f);
        }
    }

    /// <summary>
    /// Model.Shogi用のヘルパークラス
    /// </summary>
    public static partial class Util
    {
        /// <summary>
        /// fromからtoに移動する指し手を生成して返す(16bitの指し手)
        /// </summary>
        public static Move MakeMove(Square from, Square to)
        {
            return (Move)(to.ToInt() + (from.ToInt() << 7));
        }

        /// <summary>
        /// fromからtoに移動する、成りの指し手を生成して返す(16bit)
        /// </summary>
        public static Move MakeMovePromote(Square from, Square to)
        {
            return (Move)(to.ToInt() + (from.ToInt() << 7) + Move.PROMOTE.ToInt());
        }

        /// <summary>
        /// Pieceをtoに打つ指し手を生成して返す(16bitの指し手)
        /// </summary>
        public static Move MakeMoveDrop(Piece pt, Square to)
        {
            return (Move)(to.ToInt() + (pt.ToInt() << 7) + Move.DROP.ToInt());
        }

        /// <summary>
        /// 指し手を生成する
        /// 
        /// from : 盤上の升のみでなく手駒もありうる
        /// to   : 盤上の升
        /// promote : 成るかどうか
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="promote"></param>
        /// <returns></returns>
        public static Move MakeMove(SquareHand from , SquareHand to , bool promote)
        {
            // ありえないはずだが…。
            if (!to.IsBoardPiece())
                return Move.NONE;

            var to2 = (Square)to;

            if (from.IsHandPiece())
            {
                // 打ちと成りは共存できない
                if (promote)
                    return Move.NONE;

                return MakeMoveDrop(from.ToPiece(), to2);
            } else
            {
                var from2 = (Square)from;

                if (promote)
                    return MakeMovePromote(from2, to2);
                else
                    return MakeMove(from2,to2);
            }
        }

        /// <summary>
        /// USIの指し手文字列からMoveに変換
        /// 変換できないときはMove.NONEが返る。
        /// 盤面を考慮していないので指し手の合法性は考慮しない。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Move FromUsiMove(string str)
        {
            // さすがに3文字以下の指し手はおかしいだろ。
            if (str.Length <= 3)
                return Move.NONE;

            Square to = Util.FromUsiSquare(str[2], str[3]);
            if (!to.IsOk())
                return Move.NONE;

            bool promote = str.Length == 5 && str[4] == '+';
            bool drop = str[1] == '*';

            Move move = Move.NONE;
            if (!drop)
            {
                Square from = Util.FromUsiSquare(str[0], str[1]);
                if (from.IsOk())
                    move = promote ? Util.MakeMovePromote(from, to) : Util.MakeMove(from, to);
            }
            else
            {
                for (int i = 0; i < 7; ++i)
                    if (USI_MAIN_PIECE[i] == str[0])
                    {
                        move = Util.MakeMoveDrop((Piece)(i+1), to);
                        break;
                    }
            }
            return move;
        }

        /// <summary>
        /// 指し手のリストをUSIで使う指し手文字列に変換する。
        /// </summary>
        /// <param name="moves"></param>
        /// <returns></returns>
        public static string MovesToUsiString(List<Move> moves)
        {
            var sb = new StringBuilder();

            foreach (var m in moves)
            {
                if (sb.Length != 0)
                    sb.Append(' ');
                sb.Append(m.ToUsi());
            }

            return sb.ToString();
        }
    }

}
