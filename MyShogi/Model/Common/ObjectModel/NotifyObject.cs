using System;
using System.Collections.Generic;
using System.Windows.Forms;

// WPFで使うNotifyObjectっぽい何か。
namespace MyShogi.Model.Common.ObjectModel
{
    /// <summary>
    /// プロパティに代入が起きたときに呼び出されるハンドラ用の引数
    /// </summary>
    public class PropertyChangedEventArgs
    {
        public PropertyChangedEventArgs() { }

        public PropertyChangedEventArgs( string name_ , object value_)
        {
            name = name_;
            value = value_;
        }

        /// <summary>
        /// このClone()実装では、valueはobjectなのでCloneしない。注意。
        /// valueはimmutableだからこれで問題ないはずだが…。
        /// </summary>
        /// <returns></returns>
        public PropertyChangedEventArgs Clone()
        {
            var args = new PropertyChangedEventArgs();

            args.sender = sender;
            args.name = name;
            args.value = value;

            return args;
        }

        /// <summary>
        /// 送信元のNotifyObject
        /// </summary>
        public object sender;

        /// <summary>
        /// プロパティ名
        /// </summary>
        public string name;

        /// <summary>
        /// プロパティに代入された値
        /// </summary>
        public object value;
    }

    public enum DataBindWay
    {
        OneWay , // 片方向データバインド
        TwoWay , // 両方向データバインド
    }

    /// <summary>
    /// プロパティが変更されたときに呼び出されるハンドラの型
    /// </summary>
    public delegate void PropertyChangedEventHandler(PropertyChangedEventArgs args);

    /// <summary>
    /// NotifyObjectで使う、あるプロパティ名に対応するデータ
    /// </summary>
    public class PropertyObject
    {
        /// <summary>
        /// 呼び出されるハンドラ
        /// </summary>
        public PropertyChangedEventHandler handler;

        /// <summary>
        /// 格納されているオブジェクト
        /// </summary>
        public object obj;

        /// <summary>
        /// イベントハンドラを呼び出す時にUIスレッドで実行しないといけないため、
        /// Formが指定されているとき、それを格納しておくためのもの。
        /// </summary>
        public Control form;

        /// <summary>
        /// このプロパティがdata-bindされているNotifyObject
        /// lockしてないのでimmutable objectにすること。
        /// </summary>
        public List<NotifyObject> notifies;
    }

    /// <summary>
    /// MVVMのViewModelで用いる、プロパティが変更されたときに、それをsubscribe(購読)しているobserverに
    /// 通知を送るための仕組み。
    /// 
    /// ※ これを派生したクラスをDataContractで使うために、このクラスもDataContractにはなってはいるが、
    /// 実際はDataMemberを一つも持たない。
    /// </summary>
    public class NotifyObject
    {
        /// <summary>
        /// propertyのsetterを実装するときに使う。
        /// 値が変わった時に変更通知が来るようになる。
        /// 
        /// 普通の使用用途で直接呼び出すことはない。
        /// ControlBinderで必要になるのでpublicにしてある。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetValue<T>(string name, T value )
        {
            if (lockObject == null)
                return;

            var raise = false;
            lock (lockObject)
            {
                var current = GetProperty(name);

                if (!GenericEquals(current.obj , value))
                {
                    // 値が異なるときだけ代入して、そのときにイベントが発火する。
                    // 一度目はTryGetValue()に失敗するのでイベントが必ず発火する。
                    current.obj = value;

                    // UI以外のスレッドがInvoke()するときにUIスレッドがこのlockObject待ちになっていると
                    // dead lockしてしまうので、このlockが解除されてからRaisePropertyChanged()が呼び出す。
                    
                    if (PropertyChangedEventEnable)
                        raise = true;
                }
            }
            if (raise)
                RaisePropertyChanged(name, value);
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
            var raise = false;
            lock (lockObject)
            {
                var current = GetProperty(args.name);
                
                if (!GenericEquals(current.obj, args.value))
                {
                    current.obj = args.value;

                    if (PropertyChangedEventEnable)
                        raise = true;
                }
            }
            if (raise)
                RaisePropertyChanged(args);
        }

        /// <summary>
        /// SetValueして、強制的にRaisePropertyChanged()を呼び出す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetValueAndRaisePropertyChanged<T>(string name, T value)
        {
            if (lockObject == null)
                return;

            lock (lockObject)
            {
                var current = GetProperty(name);
                if (!GenericEquals(current.obj, value))
                    current.obj = value;
            }
            if (PropertyChangedEventEnable)
                RaisePropertyChanged(name, value);
        }

        /// <summary>
        /// propretyのgetterを実装するときに使う。
        /// SetValueと対にして用いる。
        /// 
        /// 普通の使用用途で直接呼び出すことはない。
        /// ControlBinderで必要になるのでpublicにしてある。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetValue<T>(string name)
        {
            lock (lockObject)
            {
                var current = GetProperty(name);
                return current.obj == null ? default(T) : (T)current.obj;
            }
        }

        /// <summary>
        /// nameに紐づけられているPropertyObjectを取得する。
        /// なければ新たに生成して返す。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        protected PropertyObject GetProperty(string name)
        {
            PropertyObject current;
            if (!propery_objects.TryGetValue(name, out current))
            {
                current = propery_objects[name] = new PropertyObject();
                // current.obj == null
            }
            return current;
        }

        /// <summary>
        /// name の propertyが変更されたときに、これを購読しているobserverに更新通知を送る。
        /// SetValue()を使わずに自力で名前に対応するイベントハンドラを呼びたい時にも用いる。
        /// 遅延呼び出しではなく、即座にハンドラが呼び出される。
        /// 
        /// notify_otherがtrueなら、DataBindしている他のNotifyObjectにも通知がいく。
        /// 
        /// DataBindで双方向bindingしているときに無限再帰になるのを防ぐためにsenderをつけている。
        /// A <-> B <-> C <-> A
        /// のような循環Bindingしている場合は、無限再帰になってしまうが、MVVMアーキテクチャでそういうようなBindingは
        /// しないと考えられるので問題ない。(MVVMでは送信元のデータソースは一箇所にあると考えられるので)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            if (!PropertyChangedEventEnable)
                return;

            PropertyObject current = null;
            Control form = null;

            lock (lockObject)
            {
                // このpropertyが見つからないということはないはず。(事前にハンドラが登録されているはずで…)
                current = GetProperty(e.name);

                // このformのBegineInvoke()で呼び出すことが要求されている。
                if (current.form != null)
                {
                    // Form生成前。無理ぽ
                    if (!current.form.IsHandleCreated)
                        return;

                    // UIスレッド以外からの実行か？
                    if (current.form.InvokeRequired)
                    {
                        // このlockのなかでInvoke()するとdead lockになるから駄目。
                        //form.Invoke(new Action(() => RaisePropertyChanged(e)));
                        //return;

                        form = current.form;
                        // このlockを抜けてからcallbackする。
                    }
                }
            }

            // このpropertyをsubscribeしているobserverに更新通知を送る重複名はないことは保証されている。

            // cloneしてsenderを書き換えて送る。
            var e2 = e.Clone();
            e2.sender = this;

            // lockの外側でコールバックしないとデッドロックになる。
            // Invoke()をする場合であって、BegineInvoke()なら大丈夫か…。
            var h = current.handler;
            if (h != null)
            {
                // UIスレッドからの実行が必要なのであればForm.BeginInvoke()を用いてコールバックする。
                // Invoke()と違ってこちらは非同期に実行される。
                if (form == null)
                    h(e2);
                else
                    // 対局中だとFormがDisposeされているのにcallbackが起きることがある。
                    // form.Disposingとform.Disposedだけで判定できると言えないので、try～catchで書いておく。
                    try
                    {
                        form.BeginInvoke(new Action(() => h(e2)));
                    }
                    catch { }
            }

            // data bindされているならそれらのオブジェクトにも通知
            // これは同じスレッドで通知して良い。
            if (current.notifies != null)
            {
                foreach (var notify in current.notifies)
                    // 無限再帰になるのを防ぐため、送信元を付与して呼び出す。
                    if (notify != e.sender /* is original sender */)
                        notify.RaisePropertyChanged(e2);
            }
        }

        /// <summary>
        /// ↑の、引数をそれぞれ指定できる版
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void RaisePropertyChanged(string name, object value = null)
        {
            RaisePropertyChanged(new PropertyChangedEventArgs(name, value));
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
        public void AddPropertyChangedHandler(string name , PropertyChangedEventHandler h , Control form = null)
        {
            lock (lockObject)
            {
                var current = GetProperty(name);
                current.handler += h;
                current.form = form; // 上書きする
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
                var current = GetProperty(name);
                current.handler -= h;
            }
        }

        /// <summary>
        /// DataBindする。別のNotifyObjectの同名のプロパティと紐づけられる。
        /// way : OneWay  片方向のbinding(thisのnameが変更されたときにnotify.nameに値がコピーされる。)
        /// way : TwoWay  双方向のbinding
        /// </summary>
        /// <param name="name"></param>
        /// <param name="notify"></param>
        /// <param name="way"></param>
        public void Bind(string name , NotifyObject notify , DataBindWay way)
        {
            lock(lockObject)
            {
                bind_helper(name, notify);

                // 双方向なので逆からもbindしてやる。
                if (way == DataBindWay.TwoWay)
                    notify.bind_helper(name, this);
            }
        }

        /// <summary>
        /// Bind()したものを解除する。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="notify"></param>
        public void Unbind(string name , NotifyObject notify)
        {
            lock(lockObject)
            {
                bind_helper(name, notify);

                // 双方向かどうかは知らんがあるなら削除しとく。
                notify.bind_helper(name, this);
            }
        }

        protected void bind_helper(string name, NotifyObject notify)
        {
            var current = GetProperty(name);

            // notifiesをimmutableにするため、コピーして編集して代入する。
            var notifies = (current.notifies == null) ? new List<NotifyObject>() : new List<NotifyObject>(current.notifies);
            notifies.Add(notify);
            current.notifies = notifies;
        }

        protected void unbind_helper(string name, NotifyObject notify)
        {
            var current = GetProperty(name);
            if (current.notifies == null)
                return;

            var notifies = new List<NotifyObject>(current.notifies);
            notifies.Remove(notify);
            current.notifies = notifies;
        }

        /// <summary>
        /// このフラグがfalseの時、SetValue()でプロパティ変更イベントが発生しなくなる。
        /// default = true
        /// 一時的にイベントを抑制したい時に用いると良いと思う。
        /// </summary>
        public bool PropertyChangedEventEnable { get; set; } = true;

        // --- 以下 private 

        /// <summary>
        /// プロパティが変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        private Dictionary<string, PropertyObject> propery_objects = new Dictionary<string, PropertyObject>();

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
