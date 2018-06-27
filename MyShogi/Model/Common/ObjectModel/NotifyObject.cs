using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

// WPFで使うNotifyObjectっぽい何か。
namespace MyShogi.Model.Common.ObjectModel
{
    /// <summary>
    /// プロパティに代入が起きたときに呼び出されるハンドラ用の引数
    /// </summary>
    public class PropertyChangedEventArgs
    {
        public PropertyChangedEventArgs(string name_ , object value_ , int start_ = -1, int end_ = -1)
        {
            name = name_;
            value = value_;
            start = start_;
            end = end_;
        }

        public string name;
        public object value;  // プロパティに代入された値
        public int start;     // 配列の場合など、部分更新が起こった場合、ここにその数値が入る。(さもなくば-1)
        public int end;       // 配列の場合など、部分更新が起こった場合、ここにその数値が入る。(さもなくば-1)
    }

    /// <summary>
    /// プロパティが変更されたときに呼び出されるハンドラの型
    /// </summary>
    public delegate void PropertyChangedEventHandler(PropertyChangedEventArgs args);

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
        protected void SetValue<T>(string name, T value , int start = -1 , int end = -1)
        {
            using (LazyLock())
            {
                object current;
                if (!this.properties.TryGetValue(name, out current)
                    || !GenericEquals(current , value))
                {
                    // 値が異なるときだけ代入して、そのときにイベントが発火する。
                    // 一度目はイベントは発火しない。
                    properties[name] = value;

                    // UI以外のスレッドがInvoke()するときにUIスレッドがこのlockObject待ちになっていると
                    // dead lockしてしまうので、このlockが解除されてからRaisePropertyChanged()が呼び出されて欲しい。

                    // LazyLockは、lockが解除されたときにまとめて変更通知を行う。
                    if (PropertyChangedEventEnable)
                        LazyRaisePropertyChanged(name, value, start, end);
                }
            }
        }

        /// <summary>
        /// propertyのsetterを実装するときに使う。
        /// 値が変わった時に変更通知が来るようになる。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void SetValue<T>(PropertyChangedEventArgs args)
        {
            using (LazyLock())
            {
                object current;
                if (!this.properties.TryGetValue(args.name, out current)
                    || !GenericEquals(current, args.value))
                {
                    // 値が異なるときだけ代入して、そのときにイベントが発火する。
                    // 一度目はイベントは発火しない。
                    properties[args.name] = args.value;

                    // UI以外のスレッドがInvoke()するときにUIスレッドがこのlockObject待ちになっていると
                    // dead lockしてしまうので、このlockが解除されてからRaisePropertyChanged()が呼び出されて欲しい。

                    // LazyLockは、lockが解除されたときにまとめて変更通知を行う。
                    if (PropertyChangedEventEnable)
                        LazyRaisePropertyChanged(args);
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
        /// SetValue()を使わずに自力で名前に対応するイベントハンドラを呼びたい時にも用いる。
        /// 遅延呼び出しではなく、即座にハンドラが呼び出される。
        /// </summary>
        /// <param name="name"></param>
        public void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            if (!PropertyChangedEventEnable)
                return;

            // いますぐ呼び出す
            PropertyChangedEventHandler h = null;
            Form form = null;

            lock (lockObject)
            {
                // UIスレッドで実行すべきなのか？
                if (forms_dic.ContainsKey(e.name))
                {
                    var form2 = forms_dic[e.name];

                    if (!form2.IsHandleCreated)
                        return;

                    if (form2.InvokeRequired)
                    {
                        // このlockのなかでInvoke()するとdead lockになるから駄目。
                        //form.Invoke(new Action(() => RaisePropertyChanged(e)));
                        //return;

                        form = form2;
                        // このlockを抜けてからcallbackする。
                    }
                }

                // このpropertyをsubscribeしているobserverに更新通知を送る重複名はないことは保証されている。
                foreach (var prop in propery_changed_handlers)
                    if (prop.Key == e.name)
                    {
                        h = prop.Value;
                        break;
                    }
            }

            // lockの外側でコールバックしないとデッドロックになる。
            if (h != null)
            {
                // UIスレッドからの実行が必要なのであればForm.Invoke()を用いてコールバックする。
                if (form == null)
                    h(e);
                else
                // 対局中だとFormがDisposeされているのにcallbackが起きることがある。
                    if (!form.Disposing)
                        form.Invoke(new Action(() => h(e)));
            }
        }

        /// <summary>
        /// ↑の、引数をそれぞれ指定できる版
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void RaisePropertyChanged(string name, object value, int start = -1, int end = -1)
        {
            RaisePropertyChanged(new PropertyChangedEventArgs(name, value, start, end));
        }

        /// <summary>
        /// プロパティが変更されたときに呼び出されるハンドラを追加する。
        /// form != nullなら、この名前を持つイベントはすべてUIスレッドで
        /// callbackされることが保証される。引数としてFormを指定する。
        /// 
        /// "[UI thread] :"とメソッドの説明に書いてあるメソッドは、
        /// UI threadからの呼び出しが必要であることを意味するアノテーションとする。
        /// 
		/// これらのメソッドはUI threadからしか呼び出されないので、このメソッド内からの呼び出しも
        /// UI threadで実行されることが保証される。
        /// 
		/// UI threadとついていないところからこのメソッドを呼び出してはならない。
		/// もしくは、このスレッドのObjectModelのUI callbackを利用する。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="h"></param>
        public void AddPropertyChangedHandler(string name , PropertyChangedEventHandler h , Form form = null)
        {
            lock (lockObject)
            {
                if (form != null)
                    if (!forms_dic.ContainsKey(name))
                        forms_dic.Add(name, form);
                    else
                        forms_dic[name] = form; // 念の為、上書きしておく。

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
                    {
                        propery_changed_handlers.Remove(name);

                        if (!forms_dic.ContainsKey(name))
                            forms_dic.Remove(name);
                    }
                }
            }
        }

        /// <summary>
        /// このフラグがfalseの時、SetValue()でプロパティ変更イベントが発生しなくなる。
        /// default = true
        /// 一時的にイベントを抑制したい時に用いると良いと思う。
        /// </summary>
        public bool PropertyChangedEventEnable { get; set; } = true;

        // --- 以下 private 

        /// <summary>
        /// 移植性を考慮し、reflection/DynamicObjectを使いたくないので
        /// プロパティ名と、それに対応するプロパティを自前でもっておく。
        /// </summary>
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        /// プロパティが変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        private Dictionary<string, PropertyChangedEventHandler> propery_changed_handlers 
            = new Dictionary<string, PropertyChangedEventHandler>();

        /// <summary>
        /// イベントハンドラを呼び出す時にUIスレッドで実行しないといけないため、
        /// Formが指定されているとき、それを格納しておくためのもの。
        /// </summary>
        private Dictionary<string, Form> forms_dic = new Dictionary<string, Form>();

        /// <summary>
        /// lockを抜けてからRaisePropertyChangedEventをまとめて呼び出す遅延lock
        /// </summary>
        public class LazyModelLock : IDisposable
        {
            public LazyModelLock(NotifyObject parent_)
            {
                parent = parent_;
                Monitor.Enter(parent.lockObject, ref taken);

                // いまからLockを抜けたときに呼び出すべきイベントを積んでいく。
                parent.events = new List<PropertyChangedEventArgs>();
            }

            // このタイミングで溜めていたイベントがまとめて呼び出される。
            public void Dispose()
            {
                // このタイミングでイベントをコピー
                var events = parent.events;
                parent.events = null;

                if (taken) Monitor.Exit(parent.lockObject);

                // ここでそれぞれのイベントを呼び出す。

                foreach(var e in events)
                    parent.RaisePropertyChanged(e);
            }

            private NotifyObject parent;

            // lockが取れたか
            private bool taken;
        }

        /// <summary>
        /// LazyModelLockのbuilder。using(LazyLock()) { ... }のようにして使う。
        /// </summary>
        /// <returns></returns>
        public LazyModelLock LazyLock()
        {
            return new LazyModelLock(this);
        }

        /// <summary>
        /// lockに用いるobject
        /// </summary>
        private object lockObject = new object();

        /// <summary>
        /// LazyModelLockのDispose()のときにまとめて呼び出されるハンドラ
        /// </summary>
        private List<PropertyChangedEventArgs> events;

        /// <summary>
        /// name の propertyが変更されたときに、これを購読しているobserverに更新通知を送る。
        /// SetValue()を使わずに自力で名前に対応するイベントハンドラを呼びたい時にも用いる。
        /// </summary>
        /// <param name="name"></param>
        private void LazyRaisePropertyChanged(string name, object value, int start = -1, int end = -1)
        {
            // ここに積んでおいて、lockを抜けるときにまとめて呼び出す
            LazyRaisePropertyChanged(new PropertyChangedEventArgs(name, value, start, end));
        }

        /// <summary>
        /// name の propertyが変更されたときに、これを購読しているobserverに更新通知を送る。
        /// SetValue()を使わずに自力で名前に対応するイベントハンドラを呼びたい時にも用いる。
        /// </summary>
        /// <param name="name"></param>
        private void LazyRaisePropertyChanged(PropertyChangedEventArgs args)
        {
            // ここに積んでおいて、lockを抜けるときにまとめて呼び出す
            events.Add(args);
        }

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
