using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace MyShogi.Model.Common.ObjectModel
{
    /// <summary>
    /// プロパティが変更されたときに呼び出されるハンドラの型
    /// </summary>
    public delegate void CollectionChangedEventHandler(NotifyCollectionChangedEventArgs args);

    /// <summary>
    /// NotifyObjectで使う、あるプロパティ名に対応するデータ
    /// </summary>
    public class CollectionPropertyObject<T>
    {
        /// <summary>
        /// 呼び出されるハンドラ
        /// </summary>
        public CollectionChangedEventHandler handler;

        /// <summary>
        /// イベントハンドラを呼び出す時にUIスレッドで実行しないといけないため、
        /// Formが指定されているとき、それを格納しておくためのもの。
        /// </summary>
        public Control form;

        /// <summary>
        /// このプロパティがdata-bindされているNotifyObject
        /// lockしてないのでimmutable objectにすること。
        /// </summary>
        //public List<NotifyCollection<T>> notifies;
    }


    /// <summary>
    /// Collectionに対してどんな種類の変更が生じたのかを記述するための定数
    /// </summary>
    public enum NotifyCollectionChangedAction
    {
        // とりま、まだReplaceしか実装してない。

        Add,     // 一部追加
        Move,    // 移動
        Remove,  // 一部削除
        Replace, // 置き換え(代入含め)
        Reset,   // コレクションのクリア
    }

    /// <summary>
    /// Collectionに対する変更の詳細を記述するためのクラス。
    /// .NET Frameworkの同名のクラスと同じ作りにしておく。
    /// </summary>
    public class NotifyCollectionChangedEventArgs
    {
        /// <summary>
        /// Reset用の初期化
        /// </summary>
        /// <param name="action"></param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Debug.Assert(action == NotifyCollectionChangedAction.Reset);
            Action = action;
        }

        /// <summary>
        /// Replace、単一object用 ≒ assign
        /// </summary>
        /// <param name="action"></param>
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action , object newObject , object oldObject , int index)
        {
            Debug.Assert(action == NotifyCollectionChangedAction.Replace);
            Action = action;
            NewItems = new[] { newObject };
            OldItems = new[] { oldObject };
            OldStartIndex = NewStartIndex = index; // このindexの要素が置き換わる
        }

        // -- Replace以外は未実装。あとで実装する(かも)。

        /// <summary>
        /// Collectionに対して生じた変更の種別
        /// </summary>
        public NotifyCollectionChangedAction Action { get; }

        /// <summary>
        /// 増えたitems
        /// </summary>
        public IList NewItems { get; }

        /// <summary>
        /// 削除、移動のアクションの影響を受けるitems
        /// </summary>
        public IList OldItems { get; }

        /// <summary>
        /// 変更が発生した位置のindex
        /// AddならAddされた位置
        /// InsertならInsertされた位置
        /// </summary>
        public int NewStartIndex { get;}

        /// <summary>
        /// Move,Remove,Replaceが生じた位置のindex
        /// </summary>
        public int OldStartIndex { get; }
    }

    /// <summary>
    /// NotifyObjectの配列版
    /// </summary>
    public class NotifyCollection<T>
    {
        public NotifyCollection()
        {}

        public NotifyCollection(int size)
        {
            list = new List<T>(new T[size]);
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="rhs"></param>
        public NotifyCollection(NotifyCollection<T> rhs)
        {
            list = new List<T>(rhs.list);
        }

        public T this[int index]
        {
            get { return list[index]; }
            set { SetValue(index, value); }
        }

        /// <summary>
        /// これがこのCollectionの実体
        /// </summary>
        public List<T> list;

        public void SetValue(int index,T value)
        {
            var raise = false;
            lock (lockObject)
            {
                if (!GenericEquals(list[index], value))
                {
                    list[index] = value;

                    if (PropertyChangedEventEnable)
                        raise = true;
                }
            }

            if (raise)
                // 単一オブジェクトの代入
                RaisePropertyChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        value,       // new value
                        list[index], // old value
                        index        // index
                        )
                );
        }

        public void RaisePropertyChanged(NotifyCollectionChangedEventArgs args)
        {
            if (!PropertyChangedEventEnable)
                return;

            var current = propertyObject;
            Control form = null;

            lock (lockObject)
            {
                // このformのBegineInvoke()で呼び出すことが要求されている。
                if (current.form != null)
                {
                    // Form生成前。無理ぽ
                    if (!current.form.IsHandleCreated)
                        return;

                    // UIスレッド以外からの実行か？
                    if (current.form.InvokeRequired)
                    {
                        form = current.form;
                        // このlockを抜けてからcallbackする。
                    }
                }
            }

            // lockの外側でコールバックしないとデッドロックになる。
            // Invoke()をする場合であって、BegineInvoke()なら大丈夫か…。
            var h = current.handler;
            if (h != null)
            {
                // UIスレッドからの実行が必要なのであればForm.BeginInvoke()を用いてコールバックする。
                // Invoke()と違ってこちらは非同期に実行される。
                if (form == null)
                    h(args);
                else
                    // 対局中だとFormがDisposeされているのにcallbackが起きることがある。
                    // form.Disposingとform.Disposedだけで判定できると言えないので、try～catchで書いておく。
                    try
                    {
                        form.BeginInvoke(new Action(() => h(args)));
                    }
                    catch { }
            }

            // 未実装
#if false
            // data bindされているならそれらのオブジェクトにも通知
            // これは同じスレッドで通知して良い。
            if (current.notifies != null)
            {
                foreach (var notify in current.notifies)
                    // 無限再帰になるのを防ぐため、送信元を付与して呼び出す。
                    if (notify != e.sender /* is original sender */)
                        notify.SetValueAndRaisePropertyChanged(e2);
            }
#endif
        }

        public void AddPropertyChangedHandler(CollectionChangedEventHandler h, Control form = null)
        {
            lock (lockObject)
            {
                var current = propertyObject;
                current.handler += h;
                current.form = form; // 上書きする
            }
        }


        /// <summary>
        /// プロパティが変更されたときに呼び出されるハンドラを削除する。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="h"></param>
        public void RemovePropertyChangedHandler(CollectionChangedEventHandler h)
        {
            lock (lockObject)
            {
                var current = propertyObject;
                current.handler -= h;
            }
        }


        /// <summary>
        /// このフラグがfalseの時、SetValue()でプロパティ変更イベントが発生しなくなる。
        /// default = true
        /// 一時的にイベントを抑制したい時に用いると良いと思う。
        /// </summary>
        public bool PropertyChangedEventEnable { get; set; } = true;

        // -- privates

        /// <summary>
        /// lockに用いるobject
        /// </summary>
        private object lockObject = new object();

        private CollectionPropertyObject<T> propertyObject = new CollectionPropertyObject<T>();

        /// <summary>
        /// オブジェクトをnull値などを考慮しながら比較する。
        /// </summary>
        /// <remarks>
        /// 主にoperator==の実装で使われる。
        /// 
        /// ここでしか使わないので別のclassにしてない。
        /// </remarks>
        private static bool GenericEquals(T lhs, T rhs)
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

    public void test()
    {
            var c = new NotifyCollection<int>(5);
            c.AddPropertyChangedHandler((args) =>
            {
                // Replace , index = 2 / 2 , 3 / 3
                Console.WriteLine($"{args.Action.ToString()} , index = {args.OldStartIndex} / {args.NewStartIndex} , {args.OldItems[0]} / {args.NewItems[0]}");
            });
            c[2] = 3;
    }

#endif

}
