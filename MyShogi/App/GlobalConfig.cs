using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using MyShogi.Model.Common.String;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.Model.Common.Tool;

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
        ///
        /// 『将棋神やねうら王』(2018年8月末発売)のマスターアップ版[2018/08/06]は"1.0.0"
        /// 『将棋神やねうら王』のUpdate1.3 [2018/09/03]は、"1.1.3"。→　マイナビ公式で配布[2018/09/03]
        /// 『将棋神やねうら王』のUpdate2.5 [2018/09/XX]は、"1.2.5"を予定。
        /// </summary>
        public static readonly string MYSHOGI_VERSION_STRING = "1.2.0";


        public GlobalConfig()
        {
            // -- 表示設定

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
            DisplayNameTurnVersion = 1;

            // -- 駒音

            PieceSoundInTheGame = 1;
            PieceSoundOffTheGame = 1;
            //CrashPieceSoundInTheGame = 1;

            // -- 読み上げ

            ReadOutKifu = 1;
            ReadOutSenteGoteEverytime = 1;
            ReadOutCancelWhenGameEnd = 1;
            ReadOutByoyomi = 1;

            // -- 対局エフェクト

            EnableGameEffect = 1;

            // -- 検討設定

            //EngineConsiderationWindowEnableWhenVsHuman = true;
            ConsiderationMultiPV = 5;
            ConsiderationWindowFollowMainWindow = true;
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
                ReadOutKifu = 0;

                // 秒の読み上げ
                ReadOutByoyomi = 0;
            }

            // -- その他

            // GloablConfigに持たせてはいるが、実際は、デシリアライズされたものを使用しないフラグ群。

            // List<T>が空の状態でDeserializeすると何かの条件でnullが突っ込まれたりするのでそれに対処。
            // List<T>をメンバーとして持つクラスにはOnDeserialize()を用意して、これを呼び出すことを強制する。

            {
                MRUF.OnDeserialize();
            }

        }

        /// <summary>
        /// 設定ファイルに書き出し
        /// </summary>
        public void Save()
        {
            Serializer.Serialize(xmlFile, this);
        }

        /// <summary>
        /// 設定ファイルの削除
        ///
        /// 例外は投げない。
        /// </summary>
        public void Delete()
        {
            try { File.Delete(xmlFile); } catch { }
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
        public int CommercialVersion { get; private set; }

        #region Display Setting

        /// <summary>
        /// 盤画像のバージョン
        /// </summary>
        [DataMember]
        public int BoardImageVersion
        {
            get { return GetValue<int>("BoardImageVersion"); }
            set { SetValue<int>("BoardImageVersion",value); }
        }

        /// <summary>
        /// 畳画像のバージョン
        /// </summary>
        [DataMember]
        public int TatamiImageVersion
        {
            get { return GetValue<int>("TatamiImageVersion"); }
            set { SetValue<int>("TatamiImageVersion", value); }
        }

        /// <summary>
        /// 駒画像のバージョン
        /// </summary>
        [DataMember]
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
        [DataMember]
        public int PromotePieceColorType
        {
            get { return GetValue<int>("PromotePieceColorType"); }
            set { SetValue<int>("PromotePieceColorType", value); }
        }

        /// <summary>
        /// 駒の移動できる方向が描いてある画像
        /// 0 : なし
        /// </summary>
        [DataMember]
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
        [DataMember]
        public int BoardNumberImageVersion
        {
            get { return GetValue<int>("BoardNumberImageVersion"); }
            set { SetValue<int>("BoardNumberImageVersion", value); }
        }

        /// <summary>
        /// 棋譜ウィンドウに表示する棋譜の種類
        ///
        /// 0 : 標準(KI2形式) 「８八同金右」 ←　デフォルト
        /// 1 : 簡易(KIF形式) 「88金(79)」
        /// 2 : Csa式        「7988KI」
        /// 3 : Chess式       「7i8h」
        /// </summary>
        [DataMember]
        public int KifuWindowKifuVersion
        {
            get { return GetValue<int>("KifuWindowKifuVersion"); }
            set { SetValue<int>("KifuWindowKifuVersion", value); }
        }

        /// <summary>
        /// 検討ウィンドウの読み筋に表示する棋譜の種類
        ///
        /// 0 : 標準(KI2形式) 「８八同金右」 ←　デフォルト
        /// 1 : 簡易(KIF形式) 「88金(79)」
        /// 2 : Csa式        「7988KI」
        /// 3 : Chess式       「7i8h」
        /// </summary>
        [DataMember]
        public int ConsiderationWindowKifuVersion
        {
            get { return GetValue<int>("ConsiderationWindowKifuVersion"); }
            set { SetValue<int>("ConsiderationWindowKifuVersion", value); }
        }

        /// <summary>
        /// 対局者の表示名(メインウインドウ上)の先頭に手番を意味する文字を付与するか。
        ///
        /// 0 : しない
        /// 1 : する 「☗」「☖」←デフォルト
        /// 2 : する 「▲」「△」
        /// </summary>
        [DataMember]
        public int DisplayNameTurnVersion
        {
            get { return GetValue<int>("DisplayNameTurnVersion"); }
            set { SetValue<int>("DisplayNameTurnVersion", value); }
        }


        /// <summary>
        /// 最終手の駒の移動元の升の背景色
        /// 0 : なし
        /// 1 : 朱色
        /// 2 : 青色
        /// 3 : 緑色
        /// 4 : 駒のシャドウのみ
        /// </summary>
        [DataMember]
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
        [DataMember]
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
        [DataMember]
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
        [DataMember]
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
        [DataMember]
        public int TurnDisplay
        {
            get { return GetValue<int>("TurnDisplay"); }
            set { SetValue<int>("TurnDisplay", value); }
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
        [DataMember]
        public bool EngineConsiderationWindowEnableWhenVsHuman
        {
            get { return GetValue<bool>("EngineConsiderationWindowEnableWhenVsHuman"); }
            set { SetValue<bool>("EngineConsiderationWindowEnableWhenVsHuman", value); }
        }
#endif

        /// <summary>
        /// 検討ウィンドウで思考エンジンが後手番のときに評価値を反転させるか(先手から見た評価値にするか)のフラグ
        /// デフォルト : false
        /// </summary>
        [DataMember]
        public bool NegateEvalWhenWhite
        {
            get { return GetValue<bool>("NegateEvalWhenWhite"); }
            set { SetValue<bool>("NegateEvalWhenWhite", value); }
        }

        /// <summary>
        /// 棋譜ウィンドウの横幅
        /// 0 = 100%
        /// 1 = 125%
        /// 2 = 150%
        /// 3 = 175%
        /// 4 = 200%
        /// </summary>
        [DataMember]
        public int KifuWindowWidthType
        {
            get { return GetValue<int>("KifuWindowWidthType"); }
            set { SetValue<int>("KifuWindowWidthType", value); }
        }

        /// <summary>
        /// 棋譜ウインドウの、ウインドウ時のフォントサイズ。
        /// </summary>
        [DataMember]
        public float KifuWindowFontSize { get; set; } = 11F;

        /// <summary>
        /// 対局中のエフェクト
        ///
        /// 0 : なし
        /// 1 : あり (デフォルト)
        /// </summary>
        [DataMember]
        public int EnableGameEffect
        {
            get { return GetValue<int>("EnableGameEffect"); }
            set { SetValue<int>("EnableGameEffect", value); }
        }

        #endregion

        #region Sound Setting

        /// <summary>
        /// 対局時の駒音
        /// 0 : なし
        /// 1 : あり(デフォルト)
        /// </summary>
        [DataMember]
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
        [DataMember]
        public int CrashPieceSoundInTheGame
        {
            get { return GetValue<int>("CrashPieceSoundInTheGame"); }
            set { SetValue<int>("CrashPieceSoundInTheGame", value); }
        }
#endif

        /// <summary>
        /// 非対局時の駒音
        /// 0 : なし
        /// 1 : あり(デフォルト)
        /// </summary>
        [DataMember]
        public int PieceSoundOffTheGame
        {
            get { return GetValue<int>("PieceSoundOffTheGame"); }
            set { SetValue<int>("PieceSoundOffTheGame", value); }
        }

        /// <summary>
        /// 棋譜の読み上げ
        /// 0 : なし
        /// 1 : あり(デフォルト) : 商用版のみ素材あり。
        /// </summary>
        [DataMember]
        public int ReadOutKifu
        {
            get { return GetValue<int>("ReadOutKifu"); }
            set { SetValue<int>("ReadOutKifu", value); }
        }

        /// <summary>
        /// 棋譜の読み上げの時の「先手」「後手」を毎回読み上げるのか
        /// 0 : なし
        /// 1 : あり(デフォルト)
        /// </summary>
        [DataMember]
        public int ReadOutSenteGoteEverytime
        {
            get { return GetValue<int>("ReadOutSenteGoteEverytime"); }
            set { SetValue<int>("ReadOutSenteGoteEverytime", value); }
        }

        /// <summary>
        /// 終局時に以降の音声読み上げをキャンセルする。
        /// 0 : なし
        /// 1 : あり(デフォルト)
        /// </summary>
        [DataMember]
        public int ReadOutCancelWhenGameEnd
        {
            get { return GetValue<int>("ReadOutCancelWhenGameEnd"); }
            set { SetValue<int>("ReadOutCancelWhenGameEnd", value); }
        }

        /// <summary>
        /// 秒読みの有無
        /// ※　ただし、対局設定で1手X秒の指定があるときのみ。
        /// また、人間側プレイヤーの時のみ。
        ///
        /// 0 : なし
        /// 1 : あり(デフォルト)
        /// </summary>
        [DataMember]
        public int ReadOutByoyomi
        {
            get { return GetValue<int>("ReadOutByoyomi"); }
            set { SetValue<int>("ReadOutByoyomi", value); }
        }

        #endregion

        #region Debug Setting

        /// <summary>
        /// ファイルロギング
        /// false : なし(デフォルト)
        ///  true : あり
        /// </summary>
        [DataMember]
        public bool FileLoggingEnable
        {
            get { return GetValue<bool>("FileLoggingEnable"); }
            set { SetValue<bool>("FileLoggingEnable", value); }
        }

        #endregion

        #region Window Location Settings

        /// <summary>
        /// メインウィンドウのサイズ。
        /// 記憶しておいて、次回同じサイズで生成する。
        /// </summary>
        [DataMember]
        public Size MainDialogClientSize { get; set; }

        /// <summary>
        /// メインウインドウのデスクトップ上の位置。
        /// 記憶しておいて、次回同じ位置に生成する。
        /// </summary>
        [DataMember]
        public Point? DesktopLocation { get; set; }

        /// <summary>
        /// 検討ウィンドウのサイズ。
        /// 記憶しておいて、次回同じサイズで生成する。
        /// </summary>
        [DataMember]
        public Size ConsiderationDialogClientSize { get; set; }

        /// <summary>
        /// 検討ウィンドウの位置。(メインのウィンドウ相対)
        /// 記憶しておいて、次回同じサイズで生成する。
        /// </summary>
        [DataMember]
        public Point ConsiderationDialogClientLocation { get; set; }

        /// <summary>
        /// 検討ウィンドウがメインウインドウを追随するか
        /// デフォルト : true
        /// </summary>
        [DataMember]
        public bool ConsiderationWindowFollowMainWindow
        {
            get { return GetValue<bool>("ConsiderationWindowFollowMainWindow"); }
            set { SetValue<bool>("ConsiderationWindowFollowMainWindow", value); }
        }

        /// <summary>
        /// 棋譜ウインドウはフローティングモードであるのかなどを管理する構造体。
        /// </summary>
        [DataMember]
        public DockManager KifuWindowDockManager = new DockManager();

        /// <summary>
        /// 検討ウィンドウの各Columnの幅(先後共通)
        /// </summary>
        [DataMember]
        public NotifyCollection<int> ConsiderationColumnWidth { get; set; } = new NotifyCollection<int>(6);

        #endregion

        #region Consideration Setting

        /// <summary>
        /// 検討時の候補手(MultiPV)の数。
        /// デフォルト : 5
        /// </summary>
        [DataMember]
        public int ConsiderationMultiPV { get; set; }

        #endregion

        #region MRUF

        /// <summary>
        /// 最近使ったファイル(自動保存されているものは除く)
        /// </summary>
        [DataMember]
        public MostRecentUsedFiles MRUF { get; set; } = new MostRecentUsedFiles();

        #endregion

        /// ← 細かい設定は、ここに追加していく

        #region Misc Settings

        /// <summary>
        /// 対局設定。
        /// 対局設定ダイアログとdata bindingして使う。
        /// </summary>
        [DataMember]
        public GameSetting GameSetting { get; set; } = new GameSetting();

        /// <summary>
        /// 検討用エンジン設定
        /// 検討用エンジン設定ダイアログとdata bindingして使う。
        /// </summary>
        [DataMember]
        public ConsiderationEngineSetting ConsiderationEngineSetting { get; set; } = new ConsiderationEngineSetting();

        /// <summary>
        /// 詰検討用エンジン設定
        /// 詰検討用エンジン設定ダイアログとdata bindして使う。
        /// </summary>
        [DataMember]
        public ConsiderationEngineSetting MateEngineSetting { get; set; } = new ConsiderationEngineSetting();

        /// <summary>
        /// 対局結果の保存設定など。
        /// 対局結果一覧ウィンドウの設定用のダイアログなどとdata bindして使う。
        /// </summary>
        [DataMember]
        public GameResultSetting GameResultSetting { get; set; } = new GameResultSetting();

        #endregion
    }
}
