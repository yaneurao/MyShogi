
using System;

namespace MyShogi.Model.Common.Utility
{
    public class Rand
    {
        /// <summary>
        /// 0からmax-1までの乱数を生成して返す。
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Next(int max)
        {
            return rand.random.Next(max);
        }

        /// <summary>
        /// bool型の乱数を得る。
        /// (trueかfalseが、それぞれ1/2の確率で返ってくる)
        /// </summary>
        /// <returns></returns>
        public static bool NextBool()
        {
            return Next(2) != 0;
        }

        /// <summary>
        /// .NETの乱数
        /// </summary>
        private Random random = new Random();

        /// <summary>
        /// singleton instance
        /// </summary>
        private static Rand rand = new Rand();
    }
}
