using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// USI2.0で規定されているエンジン設定ファイル。
    /// これをxml形式にシリアライズしたものを思考エンジンの実行ファイルのフォルダに配置する。
    /// </summary>
    public class EngineDefine
    {
        /// <summary>
        /// エンジンの表示名
        /// 
        /// この名前が画面に表示される。
        /// </summary>
        public string DisplayName { get; set; } = "思考エンジン";

        /// <summary>
        /// エンジンのバナー : 横512px×縦160pxのpng形式 推奨。
        /// このファイルがあるフォルダ相対
        /// </summary>
        public string BannerFileName { get; set; } = "banner.png";

        /// <summary>
        /// 使用するメモリ 評価関数が使用するメモリ＋探索で使用するメモリ(HASHは除く)
        /// 単位は[MB]
        /// </summary>
        public Int64 RequiredMemory { get; set; } = 500;

    }

    /// <summary>
    /// EngineDefineのユーティリティ
    /// </summary>
    public static class EngineDefineUtility
    {
        /// <summary>
        /// "engine_define.xml"というファイルを読み込んで、デシリアライズする。
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static EngineDefine ReadFile(string filepath)
        {
            EngineDefine def;
            var xmlSerializer = new XmlSerializer(typeof(EngineDefine));
            var xmlSettings = new System.Xml.XmlReaderSettings()
            {
                CheckCharacters = false,
            };
            try
            {
                using (var streamReader = new StreamReader(filepath, Encoding.UTF8))
                using (var xmlReader
                        = System.Xml.XmlReader.Create(streamReader, xmlSettings))
                {
                    def = (EngineDefine)xmlSerializer.Deserialize(xmlReader);
                }
            }
            catch
            {
                // 読み込めなかったので新規に作成する。
                def = new EngineDefine();
            }

            return def;
        }

        /// <summary>
        /// ファイルにEngineDefineをシリアライズして書き出す。
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="engine_define"></param>
        public static void WriteFile(string filepath, EngineDefine engine_define)
        {
            // シリアライズする
            var xmlSerializer = new XmlSerializer(typeof(EngineDefine));

            using (var streamWriter = new StreamWriter(filepath, false, Encoding.UTF8))
            {
                xmlSerializer.Serialize(streamWriter, engine_define);
                streamWriter.Flush();
            }
        }

    }

}
