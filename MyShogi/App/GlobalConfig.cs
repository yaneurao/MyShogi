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
    public partial class GlobalConfig : NotifyObject
    {
        #region publics

        /// <summary>
        /// MyShogiのバージョン文字列。Aboutダイアログに表示される。
        ///
        /// 『将棋神やねうら王』(2018年8月末発売)のマスターアップ版[2018/08/06]は"1.0.0"
        /// 『将棋神やねうら王』のUpdate1.3 は、"1.1.3"。→　マイナビ公式で配布[2018/09/03]
        /// 『将棋神やねうら王』のUpdate2   は、"1.2.4"。→　マイナビ公式で配布[2018/10/10]
        ///
        /// このバージョン文字列は、Serializer.VersionStringToInt()によって数値に変換できるものとする。
        /// </summary>
        public static readonly string MYSHOGI_VERSION_STRING = "1.4.0";

        /// <summary>
        /// このファイルがシリアライズされて保存された時のバージョン文字列
        /// </summary>
        public string Version { get; set; }

        public GlobalConfig()
        {
            // -- 表示設定

            BoardImageVersion = 1;
            BoardImageColorVersion = 0;
            TatamiImageVersion = 1;
            TatamiImageColorVersion = 0;
            PieceImageVersion = 1;
            PieceImageColorVersion = 0;
            PieceAttackImageVersion = 0;
            BoardNumberImageVersion = 1;
            LastMoveFromColorType = 0;
            LastMoveToColorType = 1;
            PickedMoveFromColorType = 1;
            PickedMoveToColorType = 4;
            PromotePieceColorType = 0;
            TurnDisplay = 1;
            DisplayNameTurnVersion = 1;
            FlipWhitePromoteDialog = 1;

            EnableMouseDrag = 1;
            PickedMoveDisplayStyle = 1;

            // 棋譜

            KifuWindowDisplayTotalTime = 0;
            MovesWhenKifuOpen = 1;

            // 対局エフェクト

            EnableGameGreetingEffect = 1;
            EnablePieceTossEffect = 1;

            // 評価値

            NegateEvalWhenWhite = true;
            DisplayEvalJudgement = 1;

            // -- 音声設定

            EnableSound = 1;

            // 駒音

            PieceSoundInTheGame = 1;
            PieceSoundOffTheGame = 1;
            //CrashPieceSoundInTheGame = 1;

            // 読み上げ

            ReadOutKifu = 1;
            ReadOutGreeting = 1;
            ReadOutSenteGoteEverytime = 1;
            ReadOutCancelWhenGameEnd = 1;
            ReadOutByoyomi = 1;


            // -- 操作設定

            KifuWindowPrevNextKey = 1;
            KifuWindowFirstLastKey = 2;
            KifuWindowNextSpecialKey = 1;

            ConsiderationWindowPrevNextKey = 1;
            ConsiderationWindowHeadTailKey = 2;
            ConsiderationPvSendKey = 2;

            MiniBoardPrevNextKey = 1;
            MiniBoardHeadTailKey = 2;


            // -- 検討設定

            //EngineConsiderationWindowEnableWhenVsHuman = true;
            //ConsiderationWindowFollowMainWindow = true;
            ConsiderationMultiPV = 5;

            // -- エンジン補助設定

            UsiOkTimeOut = 15;

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
            else
                // 以前のデータ構造からのマイグレーション
                config.Migrate();

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
            // 保存するときのバージョンを埋め込んでおく。
            Version = MYSHOGI_VERSION_STRING;
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

        #endregion

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
        /// 盤の色味のバリエーション
        /// 0 : 元のまま (デフォルト)
        /// 1 : 暗め
        /// 2 : かなり暗め
        /// 3 : キャラメル色
        /// 4 : 明るめの木目
        /// 5 : プラスチック
        /// </summary>
        [DataMember]
        public int BoardImageColorVersion
        {
            get { return GetValue<int>("BoardImageColorVersion"); }
            set { SetValue<int>("BoardImageColorVersion", value); }
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
        /// 畳の色味のバリエーション
        /// 0 : 元のまま (デフォルト)
        /// 1 : 暗め
        /// 2 : かなり暗め
        /// 3 : キャラメル色
        /// 4 : 古い畳風
        /// 5 : 新しい畳風
        /// </summary>
        [DataMember]
        public int TatamiImageColorVersion
        {
            get { return GetValue<int>("TatamiImageColorVersion"); }
            set { SetValue<int>("TatamiImageColorVersion", value); }
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
        /// 駒の色味のバリエーション
        /// 0 : 元のまま (デフォルト)
        /// 1 : 暗め
        /// 2 : かなり暗め
        /// 3 : キャラメル色
        /// 4 : 明るめの木目
        /// 5 : プラスチック
        /// </summary>
        [DataMember]
        public int PieceImageColorVersion
        {
            get { return GetValue<int>("PieceImageColorVersion"); }
            set { SetValue<int>("PieceImageColorVersion", value); }
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
        /// 1 : 打強制(KI2形式+打つ)「８八金打」
        /// 2 : 簡易(KIF形式) 「88金(79)」
        /// 3 : Csa式        「7988KI」
        /// 4 : Chess式       「7i8h」
        /// 
        /// 打つは、紛らわしくないときは書かないのが正式。(KI2形式が正式)
        /// cf.日本将棋連盟 「棋譜の表記方法」: https://www.shogi.or.jp/faq/kihuhyouki.html
        /// しかし、書かないとわかりにくいと将棋初心者には不評。
        /// </summary>
        [DataMember]
        public int KifuWindowKifuVersion
        {
            get { return GetValue<int>("KifuWindowKifuVersion"); }
            set { SetValue<int>("KifuWindowKifuVersion", value); }
        }

        /// </summary>
        [DataMember]
        public int KifuWindowKifuDropVersion
        {
            get { return GetValue<int>("KifuWindowKifuDropVersion"); }
            set { SetValue<int>("KifuWindowKifuDropVersion", value); }
        }

        /// <summary>
        /// 棋譜ウインドウに総消費時間を表示するのか
        ///
        /// 0 : しない ←　デフォルト
        /// 1 : する
        /// </summary>
        [DataMember]
        public int KifuWindowDisplayTotalTime
        {
            get { return GetValue<int>("KifuWindowDisplayTotalTime"); }
            set { SetValue<int>("KifuWindowDisplayTotalTime", value); }
        }

        /// <summary>
        /// 棋譜を開いた時の手数
        ///
        /// 0 : 開始局面
        /// 1 : 終局図
        /// </summary>
        [DataMember]
        public int MovesWhenKifuOpen
        {
            get { return GetValue<int>("MovesWhenKifuOpen"); }
            set { SetValue<int>("MovesWhenKifuOpen", value); }
        }

        /// <summary>
        /// 検討ウィンドウの読み筋に表示する棋譜の種類
        ///
        /// 0 : 標準(KI2形式) 「８八同金右」 ←　デフォルト
        /// 1 : 打強制(KI2形式+打つ)「８八金打」
        /// 2 : 簡易(KIF形式) 「88金(79)」
        /// 3 : Csa式        「7988KI」
        /// 4 : Chess式       「7i8h」
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
        /// 後手番の成り・不成のダイアログは反転(180度回転)させるのか。
        /// 0 : させない
        /// 1 : させる  ←　デフォルト
        /// </summary>
        public int FlipWhitePromoteDialog
        {
            get { return GetValue<int>("FlipWhitePromoteDialog"); }
            set { SetValue<int>("FlipWhitePromoteDialog", value); }
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
        /// 駒を選択したときの表示
        /// 0 : 駒を持ち上げる(移動元の升で少し浮いている表現にする)
        /// 1 : マウスカーソルに持ち上げた駒を追随させる。 : デフォルト
        /// </summary>
        [DataMember]
        public int PickedMoveDisplayStyle
        {
            get { return GetValue<int>("PickedMoveDisplayStyle"); }
            set { SetValue<int>("PickedMoveDisplayStyle", value); }
        }

        /// <summary>
        /// 手番表示の有無
        /// 0 : なし
        /// 1 : 通常の手番マーク(デフォルト)
        /// 2 : 手番側が赤色
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
        /// デフォルト : true (V1.34から)
        /// </summary>
        [DataMember]
        public bool NegateEvalWhenWhite
        {
            get { return GetValue<bool>("NegateEvalWhenWhite"); }
            set { SetValue<bool>("NegateEvalWhenWhite", value); }
        }

        /// <summary>
        /// 形勢判断の文字列を評価値のところに出力する。
        /// 0 : 出力しない
        /// 1 : 出力する(デフォルト)
        ///   激指に合わせる。300以上 有利 , 800以上 優勢 , 2000以上 勝勢
        /// </summary>
        [DataMember]
        public int DisplayEvalJudgement
        {
            get { return GetValue<int>("DisplayEvalJudgement"); }
            set { SetValue<int>("DisplayEvalJudgement", value); }
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
        /// 検討ウィンドウの縦幅
        /// 0 = 100%
        /// 1 = 125%
        /// 2 = 150%
        /// 3 = 175%
        /// 4 = 200%
        /// </summary>
        [DataMember]
        public int ConsiderationWindowHeightType
        {
            get { return GetValue<int>("ConsiderationWindowHeightType"); }
            set { SetValue<int>("ConsiderationWindowHeightType", value); }
        }


        /// <summary>
        /// 対局開始・終了時のエフェクト
        ///
        /// 0 : なし
        /// 1 : あり (デフォルト)
        /// </summary>
        [DataMember]
        public int EnableGameGreetingEffect
        {
            get { return GetValue<int>("EnableGameGreetingEffect"); }
            set { SetValue<int>("EnableGameGreetingEffect", value); }
        }

        /// <summary>
        /// 振り駒のエフェクト
        ///
        /// 0 : なし
        /// 1 : あり (デフォルト)
        /// </summary>
        [DataMember]
        public int EnablePieceTossEffect
        {
            get { return GetValue<int>("EnablePieceTossEffect"); }
            set { SetValue<int>("EnablePieceTossEffect", value); }
        }

        #endregion

        #region Sound Setting

        /// <summary>
        /// すべてのサウンドを有効/無効にする
        /// (有効にしていても個別に無効には出来る)
        ///
        /// 0 : 無効
        /// 1 : 有効
        /// </summary>
        [DataMember]
        public int EnableSound
        {
            get { return GetValue<int>("EnableSound"); }
            set { SetValue<int>("EnableSound", value); }
        }

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
        /// 対局開始時の挨拶
        /// 0 : なし
        /// 1 : あり(デフォルト) : 商用版のみ素材あり。
        /// </summary>
        [DataMember]
        public int ReadOutGreeting
        {
            get { return GetValue<int>("ReadOutGreeting"); }
            set { SetValue<int>("ReadOutGreeting", value); }
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

        #region Operation Setting

        /// <summary>
        /// 駒の移動にマウスドラッグを許容する。
        /// </summary>
        [DataMember]
        public int EnableMouseDrag
        {
            get { return GetValue<int>("EnableMouseDrag"); }
            set { SetValue<int>("EnableMouseDrag", value); }
        }

        /// <summary>
        /// 棋譜ウインドウでのキー操作その1
        ///
        /// 1手進む/戻るキー
        /// 0 : なし
        /// 1 : ←と→  :  デフォルト
        /// 2 : ↑と↓
        /// </summary>
        [DataMember]
        public int KifuWindowPrevNextKey
        {
            get { return GetValue<int>("KifuWindowPrevNextKey"); }
            set { SetValue<int>("KifuWindowPrevNextKey", value); }
        }

        /// <summary>
        /// 棋譜ウインドウでのキー操作その2
        ///
        /// 次の1手に移動する特殊キー
        /// 0 : なし
        /// 1 : スペースキー : デフォルト
        /// 2 : Enterキー
        /// </summary>
        [DataMember]
        public int KifuWindowNextSpecialKey
        {
            get { return GetValue<int>("KifuWindowNextSpecialKey"); }
            set { SetValue<int>("KifuWindowNextSpecialKey", value); }
        }

        /// <summary>
        /// 棋譜ウインドウでのキー操作その3
        ///
        /// 最初に戻る/最後に進むキー
        /// 0 : なし
        /// 1 : ←と→
        /// 2 : ↑と↓  :  デフォルト
        /// 3 : PageUpとPageDown
        /// </summary>
        [DataMember]
        public int KifuWindowFirstLastKey
        {
            get { return GetValue<int>("KifuWindowFirstLastKey"); }
            set { SetValue<int>("KifuWindowFirstLastKey", value); }
        }

        /// <summary>
        /// 検討ウインドウでのキー操作その1
        ///
        /// 選択行の上下移動
        /// 0 : なし
        /// 1 : Shift←→  : デフォルト
        /// 2 : Shift↑↓
        /// 3 : ←と→
        /// 4 : ↑と↓ 
        /// 5 : ，(カンマ)と ．(ピリオド)
        /// 6 : PageUpとPageDown
        /// </summary>
        [DataMember]
        public int ConsiderationWindowPrevNextKey
        {
            get { return GetValue<int>("ConsiderationWindowPrevNextKey"); }
            set { SetValue<int>("ConsiderationWindowPrevNextKey", value); }
        }

        /// <summary>
        /// 検討ウインドウでのキー操作その2
        ///
        /// 選択行の先頭/末尾移動
        /// 0 : なし
        /// 1 : Shift←→
        /// 2 : Shift↑↓ : デフォルト 
        /// 3 : ←と→
        /// 4 : ↑と↓ 
        /// 5 : ，(カンマ)と ．(ピリオド)
        /// 6 : PageUpとPageDown
        /// </summary>
        [DataMember]
        public int ConsiderationWindowHeadTailKey
        {
            get { return GetValue<int>("ConsiderationWindowHeadTailKey"); }
            set { SetValue<int>("ConsiderationWindowHeadTailKey", value); }
        }
        
        /// <summary>
        /// 検討ウインドウでのキー操作その3
        ///
        /// 選択行のミニ盤面へのPVの転送
        /// 0 : なし
        /// 1 : Enter : デフォルト
        /// 2 : Space
        /// </summary>
        [DataMember]
        public int ConsiderationPvSendKey
        {
            get { return GetValue<int>("ConsiderationPvSendKey"); }
            set { SetValue<int>("ConsiderationPvSendKey", value); }
        }

        /// <summary>
        /// ミニ盤面のキー操作その1
        ///
        /// 選択行の上下移動
        /// 0 : なし
        /// 1 : Ctrl←→  : デフォルト
        /// 2 : Ctrl↑↓
        /// 3 : ←と→
        /// 4 : ↑と↓ 
        /// 5 : ，(カンマ)と ．(ピリオド)
        /// 6 : PageUpとPageDown
        /// </summary>
        [DataMember]
        public int MiniBoardPrevNextKey
        {
            get { return GetValue<int>("MiniBoardPrevNextKey"); }
            set { SetValue<int>("MiniBoardPrevNextKey", value); }
        }

        /// <summary>
        /// ミニ盤面のキー操作その2
        ///
        /// 選択行の上下移動
        /// 0 : なし
        /// 1 : Ctrl←→
        /// 2 : Ctrl↑↓ : デフォルト 
        /// 3 : ←と→
        /// 4 : ↑と↓ 
        /// 5 : ，(カンマ)と ．(ピリオド)
        /// 6 : PageUpとPageDown
        /// </summary>
        [DataMember]
        public int MiniBoardHeadTailKey
        {
            get { return GetValue<int>("MiniBoardHeadTailKey"); }
            set { SetValue<int>("MiniBoardHeadTailKey", value); }
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
        /// 棋譜ウィンドウの各Columnの幅 将来的な拡張を考慮して6個用意しとく
        /// </summary>
        [DataMember]
        public NotifyCollection<int> KifuColumnWidth { get; set; } = new NotifyCollection<int>(6);

        /// <summary>
        /// 検討ウィンドウの各Columnの幅(先後共通)
        /// </summary>
        [DataMember]
        public NotifyCollection<int> ConsiderationColumnWidth { get; set; } = new NotifyCollection<int>(6);

        // -- DockWindow

        /// <summary>
        /// 棋譜ウインドウはフローティングモードであるのかなどを管理する構造体。
        /// </summary>
        [DataMember]
        public DockManager KifuWindowDockManager = new DockManager();

        /// <summary>
        /// 検討ウインドウをフローティングモードであるのかなどを管理する構造体。
        /// </summary>
        [DataMember]
        public DockManager EngineConsiderationWindowDockManager = new DockManager();

        /// <summary>
        /// ミニ盤面をフローティングモードであるのかなどを管理する構造体。
        /// </summary>
        [DataMember]
        public DockManager MiniShogiBoardDockManager = new DockManager();

        /// <summary>
        /// 評価値グラフを管理する構造体。
        /// </summary>
        [DataMember]
        public DockManager EvalGraphDockManager = new DockManager();
        #endregion

        #region Consideration Setting

        /// <summary>
        /// 検討時の候補手(MultiPV)の数。
        /// デフォルト : 5
        /// </summary>
        [DataMember]
        public int ConsiderationMultiPV { get; set; }

        #endregion

        #region エンジン補助設定

        /// <summary>
        /// "usi"に対する"usiok"までのtime out時間。(デフォルト15秒)
        /// 0 が設定されていれば無制限。
        /// </summary>
        [DataMember]
        public int UsiOkTimeOut
        {
            get { return GetValue<int>("UsiOkTimeOut"); }
            set { SetValue<int>("UsiOkTimeOut", value); }
        }

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

        #region FontSetting

        /// <summary>
        /// 各Dialogなどのフォント設定
        /// </summary>
        [DataMember]
        public FontManager FontManager { get; set; } = new FontManager();

        #endregion
    }
}
