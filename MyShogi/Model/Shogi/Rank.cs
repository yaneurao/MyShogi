namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 段を表現する型
    /// 例) RANK_4なら4段目。
    /// </summary>
    public enum Rank : int
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
        /// <param name="f"></param>
        /// <returns></returns>
        public static string Pretty(this Rank f)
        {
            return "一二三四五六七八九".Substring((int)f * 2, 2);
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
}
