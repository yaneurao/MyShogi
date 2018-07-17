using System.Collections.Generic;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// generic classのListのextension methods
    /// </summary>
    public static class ListExtensions
    {
        public static IEnumerable<T> ReverseIterator<T>(this List<T> list)
        {
            for (int i = list.Count - 1; i >= 0; --i)
                yield return list[i];
        }
    }
}
