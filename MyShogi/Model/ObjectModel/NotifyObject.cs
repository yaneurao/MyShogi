using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.ObjectModel
{
    /// <summary>
    /// MVVMのViewModelで用いる、プロパティが変更されたときに、それをsubscribe(購読)しているobserverに
    /// 通知を送るための仕組み。
    /// </summary>
    public class NotifyObject
    {

        protected void SetValue<T>(string name , T value)
        {
            lock(lockObject)
            {
                object current;
                if (!this.propDic.TryGetValue(name, out current)
                    || current != (object)value)
                {
                    propDic[name] = value;
                    RaisePropertyChanged(name);
                }
            }
        }

        /// <summary>
        /// name の propertyが変更されたときに、これを購読しているobserverに更新通知を送る。
        /// </summary>
        /// <param name="name"></param>
        protected void RaisePropertyChanged(string name)
        {

        }

        /// <summary>
        /// 移植性を考慮し、reflection/DynamicObjectを使いたくないので
        /// プロパティ名と、それに対応するプロパティを自前でもっておく。
        /// </summary>
        private Dictionary<string, object> propDic = new Dictionary<string, object>();

        /// <summary>
        /// lockに用いるobject
        /// </summary>
        private object lockObject = new object();
    }
}
