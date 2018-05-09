using System;
using System.Diagnostics;
using System.Text;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 手駒を表現するenum
    /// 歩の枚数を8bit、香、桂、銀、角、飛、金を4bitずつで持つ。
    /// こうすると16進数表示したときに綺麗に表示される。(なのはのアイデア)
    /// </summary>
    public enum Hand : UInt32
    {
        ZERO = 0,
    }


    /// <summary>
    /// Hand型に対するextension methods
    /// </summary>
    public static class HandExtensions
    {
        // 手駒の駒種 7枚
        private static readonly Piece[] PIECE_TYPE_ALL =
        {
            Piece.PAWN , Piece.LANCE , Piece.KNIGHT , Piece.SILVER , Piece.GOLD , Piece.BISHOP , Piece.ROOK,
        };

        /// <summary>
        /// 手駒をUSI形式で出力する
        /// colorの手番のほうの駒として出力する
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static string ToUSI(this Hand hand , Color color)
        {
            var sb = new StringBuilder();

            // 手駒を1枚でも出力したか
            bool found = false;

            // 手駒の出力順はUSIプロトコルでは規定されていないが、
            // USI原案によると、飛、角、金、銀、桂、香、歩の順である。
            // sfen文字列を一意にしておかないと定跡データーをsfen文字列で書き出したときに
            // 他のソフトで文字列が一致しなくて困るので、この順に倣うことにする。

            for (int i= 0; i< 7; ++i)
            {
                Piece piece = PIECE_TYPE_ALL[6 - i];
                int c = hand.Count(piece);

                if (c == 0)
                    continue;

                // 手駒が1枚でも見つかった
                found = true;

                // その種類の駒の枚数。1ならば出力を省略
                if (c != 1)
                    sb.Append(c.ToString());

                sb.Append(Util.MakePiece(color, piece).ToUSI());
            }
            return (found ? sb.ToString()+" " : "- ");
        }

        /// <summary>
        /// 手駒を日本語形式で出力する。
        /// 例) "歩1 金3"
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string Pretty(this Hand hand)
        {
            var sb = new StringBuilder();
            foreach (var pr in PIECE_TYPE_ALL)
            {
                int c = hand.Count(pr);
                // 0枚ではないなら出力。
                if (c != 0)
                {
                    // 1枚なら枚数は出力しない。2枚以上なら枚数を最初に出力
                    // PRETTY_JPが指定されているときは、枚数は後ろに表示。

                    // [1]は、先手の駒をPretty()に渡したとき先頭に空白が入るのでそれを除去するため
                    sb.Append(pr.Pretty()[1]);
                    if (c != 1)
                        sb.Append(c.ToString());
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// UInt32型に変換する。
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static UInt32 ToInt(this Hand hand)
        {
            return (UInt32)hand;
        }

        /// <summary>
        /// 手駒のbit位置
        /// </summary>
        private static readonly int[] PIECE_BITS = { 0, 0 /*歩*/, 8 /*香*/, 12 /*桂*/, 16 /*銀*/, 20 /*角*/, 24 /*飛*/ , 28 /*金*/ };

        /// <summary>
        /// その持ち駒を表現するのに必要なbit数のmask(例えば3bitなら2の3乗-1で7)
        /// </summary>
        private static readonly int[] PIECE_BIT_MASK = { 0, 31/*歩は5bit*/, 7/*香は3bit*/, 7/*桂*/, 7/*銀*/, 3/*角*/, 3/*飛*/, 7/*金*/ };

        /// <summary>
        /// 手駒pcの枚数を返す。
        /// </summary>
        public static int Count(this Hand hand, Piece pr)
        {
            Debug.Assert(Piece.PAWN <= pr && pr < Piece.KING);
            var p = (int)pr.ToInt();
            return ((int)hand.ToInt() >> PIECE_BITS[p]) & PIECE_BIT_MASK[p];
        }

        // Piece(歩,香,桂,銀,金,角,飛)を手駒に変換するテーブル
        private static readonly Hand[] PIECE_TO_HAND = {
            (Hand)0,
            (Hand) (1 << PIECE_BITS[Piece.PAWN.ToInt()]  ) /*歩*/,
            (Hand) (1 << PIECE_BITS[Piece.LANCE.ToInt()] ) /*香*/,
            (Hand) (1 << PIECE_BITS[Piece.KNIGHT.ToInt()]) /*桂*/,
            (Hand) (1 << PIECE_BITS[Piece.SILVER.ToInt()]) /*銀*/,
            (Hand) (1 << PIECE_BITS[Piece.BISHOP.ToInt()]) /*角*/,
            (Hand) (1 << PIECE_BITS[Piece.ROOK.ToInt()]  ) /*飛*/,
            (Hand) (1 << PIECE_BITS[Piece.GOLD.ToInt()]  ) /*金*/
        };

        /// <summary>
        /// 手駒にpcをc枚加える
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="pr"></param>
        /// <param name="c"></param>
        public static void Add(this ref Hand hand, Piece pr, int c = 1)
        {
            hand = (Hand)(hand.ToInt() + (UInt32)PIECE_TO_HAND[pr.ToInt()] * c);
        }

        /// <summary>
        /// 手駒からpcをc枚減ずる
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="pr"></param>
        /// <param name="c"></param>
        public static void Sub(this ref Hand hand, Piece pr, int c = 1)
        {
            hand = (Hand)(hand.ToInt() - (UInt32)PIECE_TO_HAND[pr.ToInt()] * c);
        }

    }

}
