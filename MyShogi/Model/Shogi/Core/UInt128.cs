using System;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 128bit型
    /// C#ではサポートされていないので自前実装
    /// </summary>
    public struct UInt128
    {
        public UInt64 p0;
        public UInt64 p1;

        /// <summary>
        /// 64bitの値を2つ指定して初期化できるコンストラクタ
        /// </summary>
        /// <param name="p0_"></param>
        /// <param name="p1_"></param>
        public UInt128(UInt64 p0_,UInt64 p1_)
        {
            p0 = p0_;
            p1 = p1_;
        }

        /// <summary>
        /// 64bitの値 2つで初期化する
        /// </summary>
        /// <param name="p0_"></param>
        /// <param name="p1_"></param>
        public void Set(UInt64 p0_,UInt64 p1_)
        {
            p0 = p0_;
            p1 = p1_;
        }

        public override bool Equals(object key)
        {
            UInt128 k = (UInt128)key;
            return p0 == k.p0 && p1 == k.p1;
        }

        public override int GetHashCode()
        {
            return (int)(p0^p1);
        }

        /// <summary>
        /// 上位64bitと下位64bitをbitwise orして、64bit整数にする
        /// Bitboardで、演算の結果、1bitでも立っているかどうかを判定するときに用いる
        /// </summary>
        /// <returns></returns>
        public UInt64 ToU()
        {
            return p0 | p1;
        }

        public static UInt128 operator +(UInt128 c1, UInt128 c2)
        {
            return new UInt128(c1.p0 + c2.p0, c1.p1 + c2.p1);
        }

        public static UInt128 operator -(UInt128 c1, UInt128 c2)
        {
            return new UInt128(c1.p0 - c2.p0, c1.p1 - c2.p1);
        }

        public static UInt128 operator &(UInt128 c1, UInt128 c2)
        {
            return new UInt128(c1.p0 & c2.p0, c1.p1 & c2.p1);
        }

        public static UInt128 operator |(UInt128 c1, UInt128 c2)
        {
            return new UInt128(c1.p0 | c2.p0, c1.p1 | c2.p1);
        }

        public static UInt128 operator ^(UInt128 c1, UInt128 c2)
        {
            return new UInt128(c1.p0 ^ c2.p0, c1.p1 ^ c2.p1);
        }

        public static UInt128 operator *(UInt128 c1, int n)
        {
            return new UInt128(c1.p0 * (UInt64)n , c1.p1 *(UInt64)n);
        }

        public static UInt128 operator <<(UInt128 c1, int n)
        {
            // このbit shiftは、p[0]とp[1]をまたがない。
            return new UInt128(c1.p0 << n, c1.p1 << n);
        }

        public static UInt128 operator >>(UInt128 c1, int n)
        {
            // このbit shiftは、p[0]とp[1]をまたがない。
            return new UInt128(c1.p0 >> n, c1.p1 >> n);
        }

        public static bool operator ==(UInt128 lhs , UInt128 rhs)
        {
            return lhs.p0 == rhs.p0 && lhs.p1 == rhs.p1;
        }

        public static bool operator !=(UInt128 lhs , UInt128 rhs)
        {
            return !(lhs.p0 == rhs.p0 && lhs.p1 == rhs.p1);
        }


    }
}
