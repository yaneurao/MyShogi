using System.Xml;
using System.Runtime.Serialization;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// ファイルにSerialize/ファイルからDeserializeするヘルパークラス
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// ファイルにオブジェクトをSerializeする。
        /// DataContractSerializerを用いる。
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
            catch
            {
                result = default(T);
            }

            return result;

        }
    }
}
