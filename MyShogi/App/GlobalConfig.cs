using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MyShogi.App
{
    /// <summary>
    /// 全体設定。
    /// 駒画像番号、サウンドの有無、ウインドウ比率など…
    /// </summary>
    public class GlobalConfig
    {
        public GlobalConfig()
        {
            // カレントフォルダに"YaneuraOuGUI2018.txt"というファイルがあるなら、
            // 商用版のやねうら王用のモード。
            YaneuraOu2018_GUI_MODE = System.IO.File.Exists("YaneuraOuGUI2018.txt");
        }

        /// <summary>
        /// 設定ファイルからの読み込み、GlobalConfigオブジェクトを生成する。
        /// </summary>
        public static GlobalConfig CreateInstance()
        {
            GlobalConfig config;
            var xmlSerializer = new XmlSerializer(typeof(GlobalConfig));
            var xmlSettings = new System.Xml.XmlReaderSettings()
            {
                CheckCharacters = false,
            };
            try
            {
                using (var streamReader = new StreamReader(xmlFile, Encoding.UTF8))
                using (var xmlReader
                        = System.Xml.XmlReader.Create(streamReader, xmlSettings))
                {
                    config = (GlobalConfig)xmlSerializer.Deserialize(xmlReader);
                }
            } catch
            {
                // 読み込めなかったので新規に作成する。
                config = new GlobalConfig();
            }
            return config;
        }

        /// <summary>
        /// 設定ファイルに書き出し
        /// </summary>
        public void Save()
        {
            // シリアライズする
            var xmlSerializer = new XmlSerializer(typeof(GlobalConfig));

            using (var streamWriter = new StreamWriter(xmlFile, false, Encoding.UTF8))
            {
                xmlSerializer.Serialize(streamWriter, this);
                streamWriter.Flush();
            }
        }

        /// <summary>
        /// シリアライズ先のファイル
        /// </summary>
        private const string xmlFile = @"global_config.xml";

        // -- 以下、property

        /// <summary>
        /// 商用版のやねうら王用のモードであるか。
        /// 
        /// シリアライズするためにsetterもpublicになっているが、この値は起動時に
        /// 別の方法で判定しているので、setterには意味がない。
        /// </summary>
        public bool YaneuraOu2018_GUI_MODE { get; set; }

        /// <summary>
        /// メインウィンドゥの画面比率
        /// ScreenRatio == 0 : 自動選択
        /// ScreenRatio == 1 : 4:3モード
        /// ScreenRatio == 2 : 3:2モード
        /// </summary>
        public int ScreenRatio { get; set; } = 0;

    }
}
