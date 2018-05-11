using System;
using System.Text;

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

        // -------------------------------------------------------------------------
        // コンストラクタ
        // -------------------------------------------------------------------------

        /// <summary>
        /// 128bit構造体で初期化するコンストラクタ
        /// </summary>
        /// <param name="p_"></param>
        public Bitboard(UInt128 p_)
        {
            p = p_;
        }

        /// <summary>
        /// 64bit整数２つで初期化するコンストラクタ
        /// </summary>
        /// <param name="p_"></param>
        public Bitboard(UInt64 p0_,UInt64 p1_)
        {
            p = new UInt128(p0_,p1_);
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="b"></param>
        public Bitboard(Bitboard b)
        {
            p = b.p;
        }

        /// <summary>
        /// sqの升が1のBitboardとして初期化する。
        /// </summary>
        /// <param name="sq"></param>
        public Bitboard(Square sq)
        {
            p = SQUARE_BB[sq.ToInt()].p;
        }

        // -------------------------------------------------------------------------
        // bitboardに関するビット単位のand/or/xor演算
        // -------------------------------------------------------------------------

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

        // 単項演算子
        // →　NOTで書くと、使っていないbit(p[0]のbit63)がおかしくなるのでALL_BBでxorしないといけない。
        public static Bitboard operator ~ (Bitboard a)
        {
            return a ^ ALL_BB;
        }

        // -------------------------------------------------------------------------
        // public methods
        // -------------------------------------------------------------------------

        /// <summary>
        /// sqの升のbitが立っているかを判定する。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public bool IsSet(Square sq)
        {
            return (p & SQUARE_BB[sq.ToInt()].p).ToU() != 0;
        }

        /// <summary>
        /// すべてのbitが0であるかどうかを判定する。
        /// </summary>
        /// <returns></returns>
        public bool Zero()
        {
            return p.ToU() == 0;
        }

        /// <summary>
        /// 1bitでもbitが立っているかどうかを判定する。
        /// </summary>
        /// <returns></returns>
        public bool NotZero()
        {
            return p.ToU() != 0;
        }

        /// <summary>
        /// bitboardを綺麗に出力する
        /// </summary>
        /// <returns></returns>
        public string Pretty()
        {
            var sb = new StringBuilder();

            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
            {
                for (File f = File.FILE_9; f >= File.FILE_1; --f)
                {
                    sb.Append(IsSet(Util.MakeSquare(f,r)) ? '*':'.');
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        // -------------------------------------------------------------------------
        // 以下、public tables
        // -------------------------------------------------------------------------

        /// <summary>
        /// 筋を表現するbitboardを返す
        /// </summary>
        /// <param name="f"></param>
        public static Bitboard FileBB(File f)
        {
            return FILE_BB[f.ToInt()];
        }

        /// <summary>
        /// 段を表すbitboardを返す
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Bitboard RankBB(Rank r)
        {
            return RANK_BB[r.ToInt()];
        }

        /// <summary>
        /// sqの升が1であるbitboardを返す
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Bitboard SquareBB(Square sq)
        {
            return SQUARE_BB[sq.ToInt()];
        }

        // -------------------------------------------------------------------------
        // 以下、private methods / tables
        // -------------------------------------------------------------------------

        /// <summary>
        /// staticなテーブルの初期化
        /// 起動時にInitializerから一度だけ呼び出される。
        /// 普段は呼び出してはならない。
        /// </summary>
        public static void Init()
        {
            ALL_BB = new Bitboard(0x7FFFFFFFFFFFFFFFUL, 0x3FFFFUL);
            ZERO_BB = new Bitboard(0UL, 0UL);

            Bitboard FILE1_BB = new Bitboard((0x1ffUL) << (9 * 0), 0);
            Bitboard FILE2_BB = new Bitboard((0x1ffUL) << (9 * 1), 0);
            Bitboard FILE3_BB = new Bitboard((0x1ffUL) << (9 * 2), 0);
            Bitboard FILE4_BB = new Bitboard((0x1ffUL) << (9 * 3), 0);
            Bitboard FILE5_BB = new Bitboard((0x1ffUL) << (9 * 4), 0);
            Bitboard FILE6_BB = new Bitboard((0x1ffUL) << (9 * 5), 0);
            Bitboard FILE7_BB = new Bitboard((0x1ffUL) << (9 * 6), 0);
            Bitboard FILE8_BB = new Bitboard(0, 0x1ffUL << (9 * 0));
            Bitboard FILE9_BB = new Bitboard(0, 0x1ffUL << (9 * 1));

            FILE_BB = new Bitboard[(int)File.NB]
                { FILE1_BB,FILE2_BB,FILE3_BB,FILE4_BB,FILE5_BB,FILE6_BB,FILE7_BB,FILE8_BB,FILE9_BB };

            Bitboard RANK1_BB = new Bitboard((0x40201008040201UL) << 0, 0x201 << 0);
            Bitboard RANK2_BB = new Bitboard((0x40201008040201UL) << 1, 0x201 << 1);
            Bitboard RANK3_BB = new Bitboard((0x40201008040201UL) << 2, 0x201 << 2);
            Bitboard RANK4_BB = new Bitboard((0x40201008040201UL) << 3, 0x201 << 3);
            Bitboard RANK5_BB = new Bitboard((0x40201008040201UL) << 4, 0x201 << 4);
            Bitboard RANK6_BB = new Bitboard((0x40201008040201UL) << 5, 0x201 << 5);
            Bitboard RANK7_BB = new Bitboard((0x40201008040201UL) << 6, 0x201 << 6);
            Bitboard RANK8_BB = new Bitboard((0x40201008040201UL) << 7, 0x201 << 7);
            Bitboard RANK9_BB = new Bitboard((0x40201008040201UL) << 8, 0x201 << 8);

            RANK_BB = new Bitboard[(int)Rank.NB]
            { RANK1_BB, RANK2_BB, RANK3_BB, RANK4_BB, RANK5_BB, RANK6_BB, RANK7_BB, RANK8_BB, RANK9_BB };

            SQUARE_BB = new Bitboard[(int)Square.NB_PLUS1];

            // SQUARE_BBは上記のRANK_BBとFILE_BBを用いて初期化すると楽。
            for(Square sq = Square.ZERO; sq < Square.NB; ++sq)
            {
                File f = sq.ToFile();
                Rank r = sq.ToRank();

                // 筋と段が交差するところがSQUARE_BB
                SQUARE_BB[sq.ToInt()] = FILE_BB[f.ToInt()] & RANK_BB[r.ToInt()];
            }

        }

        /// <summary>
        /// Bitboard(Square)で用いるテーブル
        /// </summary>
        private static Bitboard[] SQUARE_BB = new Bitboard[Square.NB.ToInt()];

        /// <summary>
        /// すべてのSquareが1であるBitboard
        /// </summary>
        private static Bitboard ZERO_BB;

        /// <summary>
        /// すべてのSquareが1であるBitboard
        /// </summary>
        private static Bitboard ALL_BB;

        /// <summary>
        /// 筋を表現するBitboard
        /// </summary>
        private static Bitboard [] FILE_BB;

        /// <summary>
        /// 段を表現するBitboard
        /// </summary>
        private static Bitboard [] RANK_BB;
        
    }
}
