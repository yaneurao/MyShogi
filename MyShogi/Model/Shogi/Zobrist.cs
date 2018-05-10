using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// Positionクラスで同一局面の判定のために用いるHashKey
    /// hash衝突を回避するため128bitで持つことにする。
    /// これで一局の将棋のなかでハッシュ衝突する確率は天文学的な確率のはず…。
    /// </summary>
    public struct HASH_KEY
    {
        public UInt64 p0;
        public UInt64 p1;

        public override bool Equals(object key)
        {
            HASH_KEY k = (HASH_KEY)key;
            return p0 == k.p0 && p1 == k.p1;
        }

        public override int GetHashCode()
        {
            return (int)(p0 ^ p1);
        }

        /// <summary>
        /// 16進数16桁×2で文字列化
        /// </summary>
        /// <returns></returns>
        public string Pretty()
        {
            // 16進数16桁×2で表現
            return string.Format("{0,0:X16}:{1,0:X16}", p0, p1);
        }
    }

    // 局面のhash keyを求めるときに用いるZobrist key
    public static class Zobrist
    {
        public static HASH_KEY zero;                          // ゼロ(==0)
        public static HASH_KEY side;                          // 手番(==1)
        public static HASH_KEY[,] psq = new HASH_KEY[Square.NB_PLUS1.ToInt(),Piece.NB.ToInt()];	// 駒pcが盤上sqに配置されているときのZobrist Key
	    public static HASH_KEY[,] hand = new HASH_KEY[Color.NB.ToInt(),Piece.HAND_NB.ToInt()];	// c側の手駒prが一枚増えるごとにこれを加算するZobristKey

        // static constructorで初期化するの、筋が良くないのでは…。
        /*
        static Zobrist()
        {
            Init();
        }
        */

        /// <summary>
        /// 上のテーブルを初期化する
        /// これは起動時に自動的に行われる
        /// </summary>
        public static void Init()
        {
            var rng = new PRNG(20151225); // 開発開始日 == 電王トーナメント2015,最終日

            // 手番としてbit0を用いる。それ以外はbit0を使わない。これをxorではなく加算して行ってもbit0は汚されない。
            SET_HASH(ref side, 1, 0, 0, 0);
            SET_HASH(ref zero, 0, 0, 0, 0);

            // 64bit hash keyは256bit hash keyの下位64bitという解釈をすることで、256bitと64bitのときとでhash keyの下位64bitは合致するようにしておく。
            // これは定跡DBなどで使うときにこの性質が欲しいからである。
            // またpc==NO_PIECEのときは0であることを保証したいのでSET_HASHしない。
            // psqは、C++の規約上、事前にゼロであることは保証される。
            for (Piece pc = Piece.ZERO + 1; pc < Piece.NB; ++ pc)
                for (Square sq = Square.ZERO; sq < Square.NB; ++ sq)
                    {
                        var r0 = rng.Rand() & ~1UL;
                        var r1 = rng.Rand();
                        var r2 = rng.Rand();
                        var r3 = rng.Rand();
                        SET_HASH(ref psq[sq.ToInt(),pc.ToInt()], r0, r1, r2, r3);
                    }

            // またpr==NO_PIECEのときは0であることを保証したいのでSET_HASHしない。
            for (Color c = Color.ZERO; c < Color.NB; ++c )
                for (Piece pr = Piece.ZERO + 1; pr < Piece.HAND_NB; ++pr)
                    {
                        var r0 = rng.Rand();
                        var r1 = rng.Rand();
                        var r2 = rng.Rand();
                        var r3 = rng.Rand();
                        SET_HASH(ref hand[c.ToInt(),pr.ToInt()], r0, r1, r2, r3);
                    }
        }

        /// <summary>
        /// HASH_KEYに乱数を代入する
        /// </summary>
        /// <param name="h"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        private static void SET_HASH(ref HASH_KEY h , UInt64 a,UInt64 b,UInt64 c , UInt64 d)
        {
            h.p0 = a;
            h.p1 = b;

            // 残り128bitは使用しない
        }
    }
}
