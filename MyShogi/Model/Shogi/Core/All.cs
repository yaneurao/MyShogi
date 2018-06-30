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

        public static IEnumerable<File> Files()
        {
            for (var f = File.ZERO; f < File.NB; ++f)
                yield return f;
        }

        // 将棋とは関係ないがあると便利そうなのも追加しとく。

        public static IEnumerable<bool> Bools()
        {
            yield return false;
            yield return true;
        }

        /// <summary>
        /// foreach(var x in All.Int(5)) とすると x = 0,1,2,3,4でループを回る。
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IEnumerable<int> Int(int n)
        {
            for (int i = 0; i < n; ++i)
                yield return i;
        }

        /// <summary>
        /// a <= i < b の範囲で回す
        /// foreach(var x in All.Int(5,8)) とすると x = 5,6,7でループを回る。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IEnumerable<int> Int(int a , int b)
        {
            for (int i = a; i < b; ++i)
                yield return i;
        }


        // 他、気が向いたら追加する。
    }
}
