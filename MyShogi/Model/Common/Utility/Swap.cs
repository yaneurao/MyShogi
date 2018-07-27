namespace MyShogi.Model.Common.Utility
{
    public static partial class Utility
    {
        /// <summary>
        /// aとbの入替え。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }
    }
}
