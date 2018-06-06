using System.Collections.Generic;

// WPFで使うNotifyObjectっぽい何か。
namespace MyShogi.Model.Common.ObjectModel
{
    /// <summary>
    /// プロパティが変更されたときに呼び出されるハンドラの型
    /// </summary>
    public delegate void PropertyChangedEventHandler();
    public delegate void PropertyChangedEventHandler2(object o);

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
                    || !GenericEquals(current , value))
                {
                    // 値が異なるときだけ代入して、そのときにイベントが発火する。
                    // 一度目はイベントは発火しない。
                    properties[name] = value;
                    RaisePropertyChanged(name , value);
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
        protected void RaisePropertyChanged(string name , object o)
        {
            // このpropertyをsubscribeしているobserverに更新通知を送る
            foreach(var prop in propery_changed_handlers)
                if (prop.Key == name)
                    prop.Value();

            // こちらは引数にobjectをとり、変更のあったobjectを引数にもらえる。
            foreach (var prop in propery_changed_handlers2)
                if (prop.Key == name)
                    prop.Value(o);
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

        public void AddPropertyChangedHandler(string name, PropertyChangedEventHandler2 h)
        {
            lock (lockObject)
            {
                if (!propery_changed_handlers2.ContainsKey(name))
                    propery_changed_handlers2.Add(name, h);
                else
                    propery_changed_handlers2[name] += h;
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

        public void RemovePropertyChangedHandler(string name, PropertyChangedEventHandler2 h)
        {
            lock (lockObject)
            {
                if (propery_changed_handlers2.ContainsKey(name))
                {
                    propery_changed_handlers2[name] -= h;
                    // delegateを削除した結果、nullになったなら、このentryを削除しておく。
                    if (propery_changed_handlers2[name] == null)
                        propery_changed_handlers2.Remove(name);
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
        private Dictionary<string, PropertyChangedEventHandler2> propery_changed_handlers2
            = new Dictionary<string, PropertyChangedEventHandler2>();

        /// <summary>
        /// lockに用いるobject
        /// </summary>
        private object lockObject = new object();


        /// <summary>
        /// オブジェクトをnull値などを考慮しながら比較する。
        /// </summary>
        /// <remarks>
        /// 主にoperator==の実装で使われる。
        /// 
        /// ここでしか使わないので別のclassにしてない。
        /// </remarks>
        private static bool GenericEquals<T>(T lhs, T rhs)
        {
            if (!typeof(T).IsValueType)
            {
                // どちらもnullか、同じオブジェクトなら真を返します。
                if (ReferenceEquals(lhs, rhs))
                {
                    return true;
                }

                // objectとして比較します。比較の仕方を間違えると
                // 無限ループになるので注意が必要。
                if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                {
                    return false;
                }
            }

            return lhs.Equals(rhs);
        }

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
