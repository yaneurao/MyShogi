using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.LocalServer;
using System.Drawing;

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
            PieceAttackImageVersion = 0;
            BoardNumberImageVersion = 1;
            LastMoveFromColorType = 0;
            LastMoveToColorType = 1;
            PickedMoveFromColorType = 1;
            PickedMoveToColorType = 4;
            PromotePieceColorType = 0;
            TurnDisplay = 1;
            PieceSoundInTheGame = 1;
            //CrashPieceSoundInTheGame = 1;
            KifuReadOut = 1;
            ReadOutSenteGoteEverytime = 1;
            //EngineConsiderationWindowEnableWhenVsHuman = true;
            ConsiderationMultiPV = 5;
        }

        /// <summary>
        /// 設定ファイルからの読み込み、GlobalConfigオブジェクトを生成する。
        /// </summary>
        public static GlobalConfig CreateInstance()
        {
            var config = Serializer.Deserialize<GlobalConfig>(xmlFile);

            // ファイルがなかったら新規に作成する。
            if (config == null)
                config = new GlobalConfig();

            config.Init();

            return config;
        }

        /// <summary>
        /// ファイルから設定をデシリアライズした直後に呼び出して、ファイルから読み込んだ内容の一部を
        /// 初期化するためのもの。
        /// </summary>
        private void Init()
        {
            // -- 商用版かどうかの判定

            // カレントフォルダ配下のhtmlフォルダに"CommercialVersion2018.txt"というファイルがあるなら、
            // 商用版のやねうら王用のモード。(シリアライズされた内容は関係ない)

            CommercialVersion = 0;
            if (System.IO.File.Exists("html/CommercialVersion2018.txt"))
                CommercialVersion = 1;
            // 他の商用版、今後増やすかも。

            // いまのところ商用版とオープンソース版とでの差別化はあまり考えていないが、
            // オープンソース版に対してこのファイルを用意した場合、素材が足りなくて落ちる可能性はある。
            // あと、
            // 「このファイル置いたら商用版になるぜー、うひょーｗｗｗｗｗ」
            // という記事を書いて公開するのはなるべくならやめてもらいたい。

            // 商用版でないなら、以下の機能は強制的にオフ。
            if (CommercialVersion == 0)
            {
                // 棋譜の読み上げ(音声素材がないため)
                KifuReadOut = 0;
            }

            // -- その他
            // GloablConfigに持たせてはいるが、実際は、デシリアライズされたものを使用しないフラグ群。

        }

        /// <summary>
        /// 設定ファイルに書き出し
        /// </summary>
        public void Save()
        {
            Serializer.Serialize(xmlFile, this);
        }

        /// <summary>
        /// シリアライズ先のファイル
        /// </summary>
        private const string xmlFile = @"MyShogi.setting.xml";

        // -- 以下、property

        /// <summary>
        /// 商用版のやねうら王用のモードであるか。
        /// 
        /// シリアライズするためにsetterもpublicになっているが、この値は起動時に
        /// 別の方法で判定しているので、setterには意味がない。
        /// 
        /// 0 : オープンソース版
        /// 1 : やねうら王 商用版(2018年度)
        /// 2 : 以下、来年以降の商用版の番号を振るかも。
        /// </summary>
        public int CommercialVersion { get; set; }


        /// <summary>
        /// 盤画像のバージョン
        /// </summary>
        public int BoardImageVersion
        {
            get { return GetValue<int>("BoardImageVersion"); }
            set { SetValue<int>("BoardImageVersion",value); }
        }

        /// <summary>
        /// 畳画像のバージョン
        /// </summary>
        public int TatamiImageVersion
        {
            get { return GetValue<int>("TatamiImageVersion"); }
            set { SetValue<int>("TatamiImageVersion", value); }
        }

        /// <summary>
        /// 駒画像のバージョン
        /// </summary>
        public int PieceImageVersion
        {
            get { return GetValue<int>("PieceImageVersion"); }
            set { SetValue<int>("PieceImageVersion",value); }
        }

        /// <summary>
        /// 成駒の色
        /// 0 : 黒
        /// 1 : 赤
        /// その他、あとで追加するかも知れないのでboolにしておくとまずい。
        /// </summary>
        public int PromotePieceColorType
        {
            get { return GetValue<int>("PromotePieceColorType"); }
            set { SetValue<int>("PromotePieceColorType", value); }
        }

        /// <summary>
        /// 駒の移動できる方向が描いてある画像
        /// 0 : なし
        /// </summary>
        public int PieceAttackImageVersion
        {
            get { return GetValue<int>("PieceAttackImageVersion"); }
            set { SetValue<int>("PieceAttackImageVersion", value); }
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
        /// 最終手の駒の移動元の升の背景色
        /// 0 : なし
        /// 1 : 朱色
        /// 2 : 青色
        /// 3 : 緑色
        /// 4 : 駒のシャドウのみ
        /// </summary>
        public int LastMoveFromColorType
        {
            get { return GetValue<int>("LastMoveFromColorType"); }
            set { SetValue<int>("LastMoveFromColorType", value); }
        }

        /// <summary>
        /// 最終手の駒の移動先の升の背景色
        /// 0 : なし
        /// 1 : 朱色
        /// 2 : 青色
        /// 3 : 緑色
        /// </summary>
        public int LastMoveToColorType
        {
            get { return GetValue<int>("LastMoveToColorType"); }
            set { SetValue<int>("LastMoveToColorType", value); }
        }

        /// <summary>
        /// 移動させる時の移動元の升に適用するエフェクト(ちょっと明るめ)
        /// 0 : なし
        /// 1 : 朱色
        /// 2 : 青色
        /// 3 : 緑色
        /// 4 : 駒のシャドウのみ
        /// </summary>
        public int PickedMoveFromColorType
        {
            get { return GetValue<int>("PickedMoveFromColorType"); }
            set { SetValue<int>("PickedMoveFromColorType", value); }
        }

        /// <summary>
        /// 移動させる時の移動先の候補以外の升に適用するエフェクト
        /// 0 : なし
        /// 1 : 少し暗い
        /// 2 : 暗い
        /// 3 : だいぶ暗い
        /// 4 : 少し明るい
        /// 5 : ずいぶん明るい
        /// </summary>
        public int PickedMoveToColorType
        {
            get { return GetValue<int>("PickedMoveToColorType"); }
            set { SetValue<int>("PickedMoveToColorType", value); }
        }

        /// <summary>
        /// 手番表示の有無
        /// 0 : なし
        /// 1 : あり
        /// </summary>
        public int TurnDisplay
        {
            get { return GetValue<int>("TurnDisplay"); }
            set { SetValue<int>("TurnDisplay", value); }
        }


        /// <summary>
        /// 対局時の駒音
        /// 0 : なし
        /// 1 : あり(デフォルト)
        /// </summary>
        public int PieceSoundInTheGame
        {
            get { return GetValue<int>("PieceSoundInTheGame"); }
            set { SetValue<int>("PieceSoundInTheGame", value); }
        }

#if false
        // あまりいい効果音作れなかったのでコメントアウトしとく。

        /// <summary>
        /// 対局時の駒音に、王手と駒を捕獲した時の衝撃音を用いるか
        /// 0 : なし
        /// 1 : あり(デフォルト)
        /// </summary>
        public int CrashPieceSoundInTheGame
        {
            get { return GetValue<int>("CrashPieceSoundInTheGame"); }
            set { SetValue<int>("CrashPieceSoundInTheGame", value); }
        }
#endif

        /// <summary>
        /// 棋譜の読み上げ
        /// 0 : なし
        /// 1 : あり(デフォルト) : 商用版のみ素材あり。
        /// </summary>
        public int KifuReadOut
        {
            get { return GetValue<int>("KifuReadOut"); }
            set { SetValue<int>("KifuReadOut", value); }
        }

        /// <summary>
        /// 棋譜の読み上げの時の「先手」「後手」を毎回読み上げるのか
        /// 0 : なし
        /// 1 : あり(デフォルト)
        /// </summary>
        public int ReadOutSenteGoteEverytime
        {
            get { return GetValue<int>("ReadOutSenteGoteEverytime"); }
            set { SetValue<int>("ReadOutSenteGoteEverytime", value); }
        }

        /// <summary>
        /// メモリロギング
        /// false : なし(デフォルト)
        ///  true : あり
        /// </summary>
        public bool MemoryLoggingEnable
        {
            get { return GetValue<bool>("MemoryLoggingEnable"); }
            set { SetValue<bool>("MemoryLoggingEnable", value); }
        }

        /// <summary>
        /// ファイルロギング
        /// false : なし(デフォルト)
        ///  true : あり
        /// </summary>
        public bool FileLoggingEnable
        {
            get { return GetValue<bool>("FileLoggingEnable"); }
            set { SetValue<bool>("FileLoggingEnable", value); }
        }

#if false
        /// <summary>
        /// 人間と対局中にコンピュータの読み筋ウィンドウを表示するか。
        /// デフォルト = true
        /// 
        /// COM vs COM の時　→　自動的に表示
        /// 人間 vs 人間の時 →　自動的に非表示
        /// COM vs 人間の時　→　この設定に依存
        /// 
        /// 検討モードの時　→　自動的に表示
        /// </summary>
        public bool EngineConsiderationWindowEnableWhenVsHuman
        {
            get { return GetValue<bool>("EngineConsiderationWindowEnableWhenVsHuman"); }
            set { SetValue<bool>("EngineConsiderationWindowEnableWhenVsHuman", value); }
        }
#endif

        /// <summary>
        /// メインウィンドウのサイズ。
        /// 記憶しておいて、次回同じサイズで生成する。
        /// </summary>
        public Size MainDialogClientSize { get; set; }

        /// <summary>
        /// 検討ウィンドウのサイズ。
        /// 記憶しておいて、次回同じサイズで生成する。
        /// </summary>
        public Size ConsiderationDialogClientSize { get; set; }

        /// <summary>
        /// 検討ウィンドウの位置。(メインのウィンドウ相対)
        /// 記憶しておいて、次回同じサイズで生成する。
        /// </summary>
        public Point ConsiderationDialogClientLocation { get; set; }

        /// <summary>
        /// 検討時の候補手(MultiPV)の数。
        /// デフォルト : 5
        /// </summary>
        public int ConsiderationMultiPV { get; set; }

        /// <summary>
        /// 検討ウィンドウで思考エンジンが後手番のときに評価値を反転させるか(先手から見た評価値にするか)のフラグ
        /// デフォルト : false
        /// </summary>
        public bool NegateEvalWhenWhite
        {
            get { return GetValue<bool>("NegateEvalWhenWhite"); }
            set { SetValue<bool>("NegateEvalWhenWhite",value); }
        }

        /// <summary>
        /// 対局ダイアログの設定
        /// </summary>
        public GameSetting GameSetting { get; set; } = new GameSetting();
    }
}
