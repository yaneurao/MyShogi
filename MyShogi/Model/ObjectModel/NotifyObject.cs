using System.Collections.Generic;

namespace MyShogi.Model.ObjectModel
{
    /// <summary>
    /// プロパティが変更されたときに呼び出されるハンドラの型
    /// </summary>
    public delegate void PropertyChangedEventHandler();

    /// <summary>
    /// MVVMのViewModelで用いる、プロパティが変更されたときに、それをsubscribe(購読)しているobserverに
    /// 通知を送るための仕組み。
    /// </summary>
    public class NotifyObject
    {

        /// <summary>
        /// propertyのsetterを実装するときに使う。
        /// 値が変わった時に変更通知が来るようになる。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void SetValue<T>(string name, T value)
        {
            lock (lockObject)
            {
                object current;
                if (!this.properties.TryGetValue(name, out current)
                    || !Util.GenericEquals(current , value))
                {
                    // 値が異なるときだけ代入して、そのときにイベントが発火する。
                    // 一度目はイベントは発火しない。
                    properties[name] = value;
                    RaisePropertyChanged(name);
                }
            }
        }

        /// <summary>
        /// propretyのgetterを実装するときに使う。
        /// SetValueと対にして用いる。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        protected T GetValue<T>(string name)
        {
            lock (lockObject)
            {
                object current;
                if (!this.properties.TryGetValue(name, out current))
                    return default(T);

                return (T)current;
            }
        }

        /// <summary>
        /// name の propertyが変更されたときに、これを購読しているobserverに更新通知を送る。
        /// </summary>
        /// <param name="name"></param>
        protected void RaisePropertyChanged(string name)
        {
            // このpropertyをsubscribeしているobserverに更新通知を送る
            foreach(var prop in propery_changed_handlers)
                if (prop.Key == name)
                    prop.Value();
        }

        /// <summary>
        /// プロパティが変更されたときに呼び出されるハンドラを追加する。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="h"></param>
        public void AddPropertyChangedHandler(string name , PropertyChangedEventHandler h)
        {
            lock (lockObject)
            {
                if (!propery_changed_handlers.ContainsKey(name))
                    propery_changed_handlers.Add(name,h);
                else
                    propery_changed_handlers[name] += h;
            }
        }

        /// <summary>
        /// プロパティが変更されたときに呼び出されるハンドラを削除する。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="h"></param>
        public void RemovePropertyChangedHandler(string name , PropertyChangedEventHandler h)
        {
            lock (lockObject)
            {
                if (propery_changed_handlers.ContainsKey(name))
                {
                    propery_changed_handlers[name] -= h;
                    // delegateを削除した結果、nullになったなら、このentryを削除しておく。
                    if (propery_changed_handlers[name] == null)
                        propery_changed_handlers.Remove(name);
                }
            }
        }

        // --- 以下 private 

        /// <summary>
        /// 移植性を考慮し、reflection/DynamicObjectを使いたくないので
        /// プロパティ名と、それに対応するプロパティを自前でもっておく。
        /// </summary>
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        /// プロパティが変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        private Dictionary<string,PropertyChangedEventHandler> propery_changed_handlers 
            = new Dictionary<string, PropertyChangedEventHandler>();
        
        /// <summary>
        /// lockに用いるobject
        /// </summary>
        private object lockObject = new object();
    }


#if false

    public class NotifyTestViewModel : NotifyObject
    {
        public int X
        {
            get { return GetValue<int>("X"); }
            set { SetValue<int>("X", value); }
        }

        public int Y
        {
            get { return GetValue<int>("Y"); }
            set { SetValue<int>("Y", value); }
        }
    }

    public class NotifyTestView
    {
        public void X_Changed()
        {
            System.Console.WriteLine("X_Changed");
        }
    }

    public static class NotifyTest
    {
        public static void Test()
        {
            var testViewModel = new NotifyTestViewModel();
            var testView = new NotifyTestView();

            testViewModel.AddPropertyChangedHandler("X", testView.X_Changed);

            testViewModel.X = 1;
            testViewModel.Y = 2;

            testViewModel.RemovePropertyChangedHandler("X", testView.X_Changed);

            testViewModel.X = 1;
            testViewModel.Y = 2;

            System.Console.WriteLine(testViewModel.X);
            System.Console.WriteLine(testViewModel.Y);
        }
    }

#endif

}
