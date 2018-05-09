using System;
using System.Diagnostics;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 指し手を表現するenum
    /// 指し手 bit0..6 = 移動先のSquare、bit7..13 = 移動元のSquare(駒打ちのときは駒種)、bit14..駒打ちか、bit15..成りか
    /// </summary>
    public enum Move : UInt16
    {
        NONE = 0,             // 無効な移動

        NULL    = (1 << 7) + 1,  // NULL MOVEを意味する指し手。Square(1)からSquare(1)への移動は存在しないのでここを特殊な記号として使う。
        RESIGN  = (2 << 7) + 2,  // << で出力したときに"resign"と表示する投了を意味する指し手。
        WIN     = (3 << 7) + 3,  // 入玉時の宣言勝ちのために使う特殊な指し手

        DROP    = 1 << 14,       // 駒打ちフラグ
        PROMOTE = 1 << 15,       // 駒成りフラグ
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
        /// 見た目に、わかりやすい形式で表示する
        /// (盤面情報がないので移動させる駒がわからない。デバッグ用)
        /// </summary>
        public static string Pretty(this Move m)
        {
            if (m.IsDrop())
                return m.To().Pretty() + m.DroppedPiece().Pretty() + "打";
            else
                return m.From().Pretty() + m.To().Pretty() + (m.IsPromote() ? "成" : "");
        }

        /// <summary>
        /// 移動させた駒がわかっているときに指し手をわかりやすい表示形式で表示する。
        /// (盤面情報がないので、移動元候補の駒が複数ある場合は区別が出来ない。デバッグ用)
        /// </summary>
        public static string Pretty(this Move m, Piece movedPieceType)
        {
            if (m.IsDrop())
                return m.To().Pretty() + movedPieceType.Pretty() + "打";
            else
                return m.To().Pretty() + movedPieceType.Pretty() + (m.IsPromote() ? "成" : "");

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
                return m.DroppedPiece().ToUsi() + "*" + m.To().ToUsi();

            else
                return m.From().ToUsi() + m.To().ToUsi() + (m.IsPromote() ? "+" : "");
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
    }

}
