namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// Positionクラスで同一局面の判定のために用いるHashKey
    /// hash衝突を回避するため128bitで持つことにする。
    /// これで一局の将棋のなかでハッシュ衝突する確率は天文学的な確率のはず…。
    /// </summary>
    public struct HASH_KEY
    {
        public UInt128 p;

        public HASH_KEY(UInt128 p_)
        {
            p = p_;
        }

        public override bool Equals(object key)
        {
            return p.Equals(((HASH_KEY)key).p);
        }

        public override int GetHashCode()
        {
            return p.GetHashCode();
        }

        public static bool operator ==(HASH_KEY lhs, HASH_KEY rhs)
        {
            return lhs.p == rhs.p;
        }

        public static bool operator !=(HASH_KEY lhs, HASH_KEY rhs)
        {
            return lhs.p != rhs.p;
        }

        /// <summary>
        /// 16進数16桁×2で文字列化
        /// </summary>
        /// <returns></returns>
        public string Pretty()
        {
            // 16進数16桁×2で表現
            return string.Format("{0,0:X16}:{1,0:X16}", p.p0, p.p1);
        }

        public static HASH_KEY operator +(HASH_KEY c1, HASH_KEY c2)
        {
            return new HASH_KEY(c1.p + c2.p);
        }

        public static HASH_KEY operator -(HASH_KEY c1, HASH_KEY c2)
        {
            return new HASH_KEY(c1.p - c2.p);
        }

        public static HASH_KEY operator &(HASH_KEY c1, HASH_KEY c2)
        {
            return new HASH_KEY(c1.p & c2.p);
        }

        public static HASH_KEY operator |(HASH_KEY c1, HASH_KEY c2)
        {
            return new HASH_KEY(c1.p | c2.p);
        }

        public static HASH_KEY operator ^(HASH_KEY c1, HASH_KEY c2)
        {
            return new HASH_KEY(c1.p ^ c2.p);
        }

        public static HASH_KEY operator *(HASH_KEY c1, int n)
        {
            return new HASH_KEY(c1.p * n);
        }

    }
}
