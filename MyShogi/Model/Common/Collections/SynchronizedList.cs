using System.Collections.Generic;

namespace MyShogi.Model.Common.Collections
{
    /// <summary>
    /// 同期版のList
    /// Javaでよく出てくるやつ。
    /// </summary>
    public class SynchronizedList<T>
    {
        /// <summary>
        /// [async] List.Add(T t)と同じ。
        /// </summary>
        /// <param name="t"></param>
        public void Add(T t)
        {
            lock (sync_object)
                list.Add(t);
        }

        // その他、気が向いたら追加する。とりま、いまAdd()しか要らない。

        /// <summary>
        /// [async] 内部的に保持しているListを返し、内部的に保持しているListは、
        ///   List.Clear()を呼び出した状態に初期化される。
        /// </summary>
        /// <returns></returns>
        public List<T> GetList()
        {
            lock (sync_object)
            {
                var result = list;
                list = new List<T>(); // これで空に。
                return result;
            }
        }

        /// <summary>
        /// 内部的に保持しているListの実体。
        /// 直接外部からアクセスしてはならない。
        /// </summary>
        private List<T> list = new List<T>();

        /// <summary>
        /// lock用のobject
        /// </summary>
        private object sync_object = new object();
    }
}
