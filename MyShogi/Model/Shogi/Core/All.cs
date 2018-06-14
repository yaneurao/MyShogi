using System.Collections.Generic;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// enum型に対するenumeratorの生成。
    /// 
    /// foreach(var c in All.Colors()) .. とか書けて気分いい。
    /// </summary>
    public static class All
    {
        public static IEnumerable<Color> Colors()
        {
            for (var c = Color.ZERO; c < Color.NB; ++c)
                yield return c;
        }

        public static IEnumerable<Square> Squares()
        {
            for (var sq = Square.ZERO; sq < Square.NB; ++sq)
                yield return sq;
        }

        // 他、気が向いたら追加する。
    }
}
