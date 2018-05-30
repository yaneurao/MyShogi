using MyShogi.Model.ObjectModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MyShogi.App
{
    /// <summary>
    /// 全体設定。
    /// 駒画像番号、サウンドの有無、ウインドウ比率など…
    /// </summary>
    public class GlobalConfig : NotifyObject
    {
        /// <summary>
        /// MyShogiのバージョン文字列。Aboutダイアログに表示される。
        /// </summary>
        public static readonly string MYSHOGI_VERSION_STRING = "0.01";

        public GlobalConfig()
        {
            BoardImageVersion = 1;
            TatamiImageVersion = 1;
            PieceImageVersion = 1;
            BoardNumberImageVersion = 1;
            LastMoveColorType = 1;
            BoardReverse = false;
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

            // カレントフォルダ配下のtextフォルダに"CommercialVersion.txt"というファイルがあるなら、
            // 商用版のやねうら王用のモード。(シリアライズされた内容は関係ない)

            config.CommercialVersion = System.IO.File.Exists("text/CommercialVersion.txt");

            // いまのところ商用版とオープンソース版とでの差別化はあまり考えていないが、
            // オープンソース版に対してこのファイルを用意した場合、素材が足りなくて落ちる可能性はある。
            // あと、
            // 「このファイル置いたら商用版になるぜー、うひょーｗｗｗｗｗ」
            // という記事を書いて公開するのはなるべくならやめてもらいたい。

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
        private const string xmlFile = @"settings.xml";

        // -- 以下、property

        /// <summary>
        /// 商用版のやねうら王用のモードであるか。
        /// 
        /// シリアライズするためにsetterもpublicになっているが、この値は起動時に
        /// 別の方法で判定しているので、setterには意味がない。
        /// </summary>
        public bool CommercialVersion { get; set; }


        /// <summary>
        /// 盤画像のバージョン
        /// 商用版のみ1以外を選択できる。(ようにするかも)
        /// </summary>
        public int BoardImageVersion
        {
            get { return GetValue<int>("BoardImageVersion"); }
            set { SetValue<int>("BoardImageVersion",value); }
        }

        /// <summary>
        /// 畳画像のバージョン
        /// 商用版のみ1以外を選択できる。(ようにするかも)
        /// </summary>
        public int TatamiImageVersion
        {
            get { return GetValue<int>("TatamiImageVersion"); }
            set { SetValue<int>("TatamiImageVersion", value); }
        }

        /// <summary>
        /// 駒画像のバージョン
        /// 商用版のみ1以外を選択できる。
        /// </summary>
        public int PieceImageVersion
        {
            get { return GetValue<int>("PieceImageVersion"); }
            set { SetValue<int>("PieceImageVersion",value); }
        }

        /// <summary>
        /// 盤のサイドにある段・筋を表現する駒画像のバージョン
        /// 商用版のみ1以外を選択できる。
        /// 
        /// 0 : 非表示
        /// 1 : 標準
        /// 2 : Chess式
        /// </summary>
        public int BoardNumberImageVersion
        {
            get { return GetValue<int>("BoardNumberImageVersion"); }
            set { SetValue<int>("BoardNumberImageVersion", value); }
        }

        /// <summary>
        /// 最終手の駒の移動先の升の背景色
        /// 1 : 朱色
        /// 2 : 青色
        /// 3 : 緑色
        /// </summary>
        public int LastMoveColorType
        {
            get { return GetValue<int>("LastMoveColorType"); }
            set { SetValue<int>("LastMoveColorType", value); }
        }
        
        /// <summary>
        /// 盤面反転(これはGlobalな設定)
        /// </summary>
        public bool BoardReverse
        {
            get { return GetValue<bool>("BoardReverse"); }
            set { SetValue<bool>("BoardReverse", value); }
        }
    }
}
