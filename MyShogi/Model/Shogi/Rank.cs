using System;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 段を表現する型
    /// 例) RANK_4なら4段目。
    /// </summary>
    public enum Rank : UInt32
    {
        RANK_1, RANK_2, RANK_3, RANK_4, RANK_5, RANK_6, RANK_7, RANK_8, RANK_9, NB, ZERO = 0
    };

    /// <summary>
    /// Rankに関するextension methodsを書くクラス
    /// </summary>
    public static class RankExtensions
    {
        public static bool IsOk(this Rank r)
        {
            return Rank.ZERO <= r && r < Rank.NB;
        }

        /// <summary>
        /// Rankを綺麗に出力する(USI形式ではない)
        /// 日本語文字での表示になる。例 → 八
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static string Pretty(this Rank r)
        {
            // C#では全角1文字が1つのcharなので注意。
            return "一二三四五六七八九".Substring((int)r.ToInt(), 1);
        }

        /// <summary>
        /// USI文字列に変換する。
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static string ToUSI(this Rank r)
        {
            return new string((char)((UInt32)'a' + r.ToInt()), 1);
        }

        /// <summary>
        /// UInt32型への変換子
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static UInt32 ToInt(this Rank r)
        {
            return (UInt32)r;
        }

        /// <summary>
        /// USIの指し手文字列などで筋を表す文字列をここで定義されたRankに変換する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Rank ToRank(this char c)
        {
            return (Rank)(c - 'a');
        }
    }

    /// <summary>
    /// Model.Shogi用のヘルパークラス
    /// </summary>
    public static partial class Util
    {
        /// <summary>
        /// 移動元、もしくは移動先の升のrankを与えたときに、そこが成れるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="fromOrToRank"></param>
        /// <returns></returns>
        public static bool CanPromote(Color c, Rank fromOrToRank)
        {
            // ASSERT_LV1(is_ok(c) && is_ok(fromOrToRank));
            // 先手9bit(9段) + 後手9bit(9段) = 18bitのbit列に対して、判定すればいい。
            // ただし ×9みたいな掛け算をするのは嫌なのでbit shiftで済むように先手16bit、後手16bitの32bitのbit列に対して判定する。
            // このcastにおいて、VC++2015ではwarning C4800が出る。
            return (0x1c00007u & (1u << (int)((c.ToInt() << 4) + fromOrToRank.ToInt()))) != 0;
        }
    }

}
