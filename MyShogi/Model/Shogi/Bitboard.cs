namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// Bitboard
    /// 駒の利きなどを表現する
    /// やねうら王から移植
    /// </summary>
    public struct Bitboard
    {
        /// <summary>
        /// Bitboardの実体
        /// </summary>
        public UInt128 p;

        /// <summary>
        /// 128bit構造体で初期化するコンストラクタ
        /// </summary>
        /// <param name="p_"></param>
        public Bitboard(UInt128 p_)
        {
            p = p_;
        }

        // sqの升が1のBitboardとして初期化する。
        //public Bitboard(Square sq)
        //{
        //    p = SquareBB[sq];
        //}

        // --- bitboardに関するビット単位のand/or/xor演算

        public static Bitboard operator &(Bitboard c1, Bitboard c2)
        {
            return new Bitboard(c1.p & c2.p);
        }

        public static Bitboard operator |(Bitboard c1, Bitboard c2)
        {
            return new Bitboard(c1.p | c2.p);
        }

        public static Bitboard operator ^(Bitboard c1, Bitboard c2)
        {
            return new Bitboard(c1.p ^ c2.p);
        }


    }
}
