using System;
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

        SPECIAL = DROP + PROMOTE,

        NULL   ,  // NULL MOVEを意味する指し手
        RESIGN ,  // << で出力したときに"resign"と表示する投了を意味する指し手。自分による手番時の投了。
        WIN    ,  // 入玉時の宣言勝ちのために使う特殊な指し手
        MATE   ,  // 詰み(合法手がない)局面 
        REPETITION     , // 千日手(PSN形式で、DRAWかWINかわからない"Sennichite"という文字列が送られてくるのでその解釈用)
        REPETITION_DRAW, // 千日手引き分け
        REPETITION_WIN , // 千日手勝ち(相手の連続王手)
        REPETITION_LOSE, // 千日手負け(自分の連続王手)　この値は使うことはないはず
        TIME_UP        , // 時間切れによる負け
        INTERRUPTION   , // ゲーム中断
    }

    /// <summary>
    /// Moveに関するextension methods
    /// </summary>
    public static class MoveExtensions
    {

        /// <summary>
        /// 指し手がおかしくないかをテストする
        /// ただし、盤面のことは考慮していない。MOVE_NULLとMOVE_NONEであるとfalseが返る。
        /// これら２つの定数は、移動元と移動先が等しい値になっている。このテストだけをする。
        /// MOVE_WIN(宣言勝ちの指し手は)は、falseが返る。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsOk(this Move m)
        {
            // return move_from(m)!=move_to(m);
            // とやりたいところだが、駒打ちでfromのbitを使ってしまっているのでそれだとまずい。
            // 駒打ちのbitも考慮に入れるために次のように書く。
            return (m.ToInt() >> 7) != (m.ToInt() & 0x7f);
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
        /// </summary>
        public static string ToUsi(this Move m)
        {
            if (!m.IsOk())
                return ((m == Move.RESIGN) ? "resign" :
                        (m == Move.WIN) ? "win" :
                        (m == Move.NULL) ? "null" :
                        (m == Move.NONE) ? "none" :
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
    }

}
