using System.Xml;
using System.Runtime.Serialization;
using System.Diagnostics;
using MyShogi.Model.Common.Collections;

namespace MyShogi.Model.Common.String
{
    /// <summary>
    /// ファイルにSerialize/ファイルからDeserializeするヘルパークラス
    ///
    /// Tには、DataContract 属性をつけて、シリアライズしたいメンバーに DataMember属性がついていることが前提。
    /// 
    /// ただし、DataContractをつけると型に厳しくなるのでNotifyObject派生クラスがシリアライズできなくなる。
    /// DataContractをつけるメリットはList型,Dictionary型などが使えることだが、単なる配列しか使っていないならば
    /// つけなくとも問題ない。
    ///
    /// .NETのDataContractSerializer()を用いているのだが、これが仕様的に、とても使いにくい。
    /// 自作したほうがいいように思う。
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// ファイルにオブジェクトをSerializeする。
        /// DataContractSerializerを用いる。
        /// 
        /// 注意点)
        /// ・配列を用いるクラスにはDataMember属性が必要。配列を用いないならDataMember属性は実は不要。(あっても良い)
        /// ・クラスのほうにDataMember属性をつけるか、使用するときにその変数に対してDataMember属性をつけるか、どちらでも効果は同じ。
        /// ・ int[] a = new {1,2,3} のようにしてあるとき、一度目のdeserializeではこの要素が存在しないので{1,2,3}を持つ配列になるが、
        /// 　これをserializeしたのちに、a = new { 2,3 } のように書き換えて、そのあとdeserializeしてもa = { 1,2,3 }になるので注意。
        /// 
        /// ・Listクラスを用いるクラスにはDataContract属性が必要。
        /// ・このとき、OnDeserialize()を用意して、そのなかでnew List()をしてlistの確保が必要。(Deserializeされたときに
        ///   コレクションがなければnullになりうるので)　このOnDeserialize()は、自前で呼び出す必要がある。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="obj"></param>
        public static void Serialize<T>(string filename , T obj)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var settings = new XmlWriterSettings();
            settings.Encoding = new System.Text.UTF8Encoding(false);
            var xw = XmlWriter.Create(filename, settings);
            serializer.WriteObject(xw, obj);
            xw.Close();
        }

        /// <summary>
        /// Serialize()を使ってシリアライズしたものをデシリアライズする。
        /// ファイルがなかった場合などは、default(T)が返る。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string filename)
        {
            T result;
            try
            {
                var serializer = new DataContractSerializer(typeof(T));
                var xr = XmlReader.Create(filename);
                result = (T)serializer.ReadObject(xr);
                xr.Close();
            }
            catch /* (Exception ex) */
            {
                //Console.WriteLine(ex.Message);
                result = default(T);
            }

            return result;
        }

        /// <summary>
        /// バージョン文字列をint型にする。
        /// 例) "1.2.3"→ 1230 , "1.2.3a"→1231 , "12.3.4"→ 12340
        /// 不明なら"0"
        /// </summary>
        /// <param name="versionString"></param>
        /// <returns></returns>
        public static int VersionStringToInt(string versionString)
        {
            // まず"."を除去して、末尾にアルファベットがあるなら、それを1の位の数字とみなす。なければ末尾に"0"を付与。
            if (versionString.Empty())
                return 0;

            // 末尾の文字列
            var lastChar = char.ToLower(versionString.Right(1)[0]);

            if ('0' <= lastChar && lastChar <= '9')
            {
                return int.Parse(versionString.Replace(".","")) * 10;
            }
            else
            {
                var lastDecimal = (lastChar) - 'a' + 1; /* 'a'を1 , 'b'を2 , ..に変換する */

                return int.Parse(versionString.Left(versionString.Length-1).Replace(".","")) * 10 + lastDecimal;
            }
        }
    }
}
