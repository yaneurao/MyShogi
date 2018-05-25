using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.ObjectModel
{
    /// <summary>
    /// Propertyが変更されたときに呼び出されるハンドラの引数
    /// </summary>
    public class PropertyChangedEventArgs
    {
        public PropertyChangedEventArgs(string name)
        {
            propertyName = name;
        }

        /// <summary>
        /// 変更されたプロパティ名
        /// </summary>
        public string propertyName;
    }

        public delegate void PropertyChangedEventHandler(
            object sender,
            PropertyChangedEventArgs e
        );

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
                if (!this.propDic.TryGetValue(name, out current)
                    || current != (object)value)
                {
                    // 値が異なるときだけ代入して、イベントが発火する。
                    propDic[name] = value;
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
                if (!this.propDic.TryGetValue(name, out current))
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

            subscribers(/* sender */this, new PropertyChangedEventArgs(name));

            // nameのpropertyに依存しているpropertyも発火させる
            if (depends_from.ContainsKey(name))
            {
                foreach (var name2 in depends_from[name])
                    RaisePropertyChanged(name2);
            }
        }

        /// <summary>
        /// name1のプロパティがname2のプロパティに依存していることを示す
        /// つまり、name2が変更になったときに、name1のプロパティの値も変わる。
        /// なので、name2が変更になったときに、name1のイベントも発火する。
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        protected void DependsOn(string name1 , string name2)
        {
            if (!depends_from.ContainsKey(name1))
                depends_from.Add(name1, new List<string>());
            depends_from[name1].Add(name2);
        }

        public void AddListener(PropertyChangedEventHandler h)
        {
            subscribers += h;
        }

        // --- 以下 private 

        /// <summary>
        /// 移植性を考慮し、reflection/DynamicObjectを使いたくないので
        /// プロパティ名と、それに対応するプロパティを自前でもっておく。
        /// </summary>
        private Dictionary<string, object> propDic = new Dictionary<string, object>();

        /// <summary>
        /// プロパティが変更されたときに呼び出されるイベントハンドラ
        /// </summary>
        private PropertyChangedEventHandler subscribers;
        
        /// <summary>
        /// あるpropertyに依存しているpropertyのリスト
        /// </summary>
        private Dictionary<string, List<string /*property name*/>> depends_from = new Dictionary<string, List<string>>();

        /// <summary>
        /// lockに用いるobject
        /// </summary>
        private object lockObject = new object();
    }

#if true

    public class NotifyTestViewModel : NotifyObject
    {
        public NotifyTestViewModel()
        {
            DependsOn("X", "Y"); // XはYに依存する(Yの値が変わったときにXもそれに応じて変わる)
        }

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
        public void Bind(NotifyObject o)
        {
//            o.AddListener( this , "name" );
        }

        public void PropertyChanged(PropertyChangedEventArgs e)
        {

        }
    }

    public static class NotifyTest
    {
        public static void Test()
        {
            var testViewModel = new NotifyTestViewModel();
            var testView = new NotifyTestView();

            testViewModel.X = 1;
            testViewModel.Y = 2;

            Console.WriteLine(testViewModel.X);
            Console.WriteLine(testViewModel.Y);
        }
    }

#endif

}
