using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Usi;
using System.Collections.Generic;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// エンジン共通設定で使うEngineOptionsForSettingのオブジェクトを生成する。
    /// 
    /// シリアライズしてファイルに書き出しておいても良いが、二度手間なので、
    /// ここで生成したものをそのまま使う。
    /// </summary>
    public static class EngineCommonOptionsSample
    {
        /// <summary>
        /// エンジンオプションの個別設定に表示すべき、最小限のオブジェクトを生成する。
        /// これは、EngineDefineが提供されていないような外部エンジンを使用するときに用いられる。
        /// </summary>
        /// <returns></returns>
        public static EngineOptionsForSetting CreateEngineMinimumOptions()
        {
            var setting = new EngineOptionsForSetting()
            {

                Descriptions = new List<EngineOptionDescription>()
                {
                    // -- これは仮想optionで、実際にエンジンはこのoption名を持っていることはありえない。
                    // 内部的な設定に用いる。

                    // nameがnullなら、これは見出し表示になる。

                    // -- ハッシュ設定

                    new EngineOptionDescription(null           , "ハッシュメモリ管理" ,
                        null,
                        "コンピューターが思考する時にハッシュメモリというものを使います。"+
                        "これは、一度調べた局面の情報を保存しておくために必要です。" +
                        "ハッシュ用のメモリが不十分だと探索の効率が落ちます。(コンピューターの棋力が弱くなります)\r\n" +
                        "読み筋の表示されるウインドウの右上に『HASH使用率』というのが表示されています。" +
                        "1手に使う思考時間が伸びていくと、この『HASH使用率が』増えていきますが、その時に" +
                        "この数字があまりに高いと、ハッシュメモリが足りていなくて読みの効率が落ちていることを意味します。",
                        null),

                    new EngineOptionDescription("AutoHash_"           , "自動ハッシュ" ,
                        "ハッシュメモリを自動的に割り当てます。",
                        "このオプションを有効にすると、空き物理メモリから『Hash割合』の分だけ自動的にハッシュメモリを割当てます。"+
                        "このオプションを無効にすると、『Hash[MB]』の分だけ強制的にハッシュメモリを割り当てます。\r\n"+
                        "※　ハッシュメモリについての詳しい説明は『ハッシュメモリ管理』のところにマウスをホバーさせてください。",
                        "option name AutoHash_ type check default true"
                        ),


                    new EngineOptionDescription("AutoHashPercentage_" , "Hash割合"   ,
                        "空き物理メモリの何%をハッシュメモリに用いるかを設定します。" ,
                        "上記の『自動ハッシュ割当』をオンにしている時に、空き物理メモリの何%をハッシュメモリに用いるかを設定します。" +
                        "『自動ハッシュ割当』がオフの時は、この値は無視されます。"+
                        "0%～100%までの数値で指定できます。小さな値を指定した場合、思考エンジンの最低必要ハッシュメモリに足りなくなるので" +
                        "その場合、後者の数値が優先されます。\r\n" +
                        "※　ハッシュメモリについての詳しい説明は『ハッシュメモリ管理』のところにマウスをホバーさせてください。",
                        "option name AutoHashPercentage_ type spin default 80 min 0 max 100"
                        ),

                    // ※　"Hash_"という項目に設定しているが、これは、のちに"USI_Hash"か"Hash"に置換される。

                    new EngineOptionDescription("Hash_"          , "Hash[MB]",
                        "何MBをハッシュメモリに用いるかを設定します。",
                        "上記の『自動ハッシュ割当』をオフにしている時に、何[MB]をハッシュメモリに用いるかを手動で設定します。" +
                        "『自動ハッシュ割当』がオンの時は、この値は無視されます。"+
                        "ここで指定した値が思考エンジンの最低必要ハッシュメモリに満たない場合、後者の値が優先されます。\r\n"+
                        "※　ハッシュメモリについての詳しい説明は『ハッシュメモリ管理』のところにマウスをホバーさせてください。",
                        "option name Hash_ type spin default 4096 min 0 max 1048576"),


                    // -- スレッド設定

                    new EngineOptionDescription(null           , "スレッド設定" ,
                        null,
                        "コンピューターが思考する時にCPUのコア数分までは並列的に探索したほうが強くなります。"+
                        "例えば、4スレッドなら4つ並列して探索するという意味です。ここではその設定を行います。" +
                        "スレッド数を増やすとCPU負荷率が上がるのでCPUの温度を下げたい時などはスレッド数を減らしてみてください。",
                        null),

                    new EngineOptionDescription("AutoThread_"           , "自動スレッド" ,
                        "思考の時に用いるスレッド数を自動的に決定します。",
                        "このオプションを有効にすると、思考の時に使用するスレッド数が、自動的にこのPCに搭載されているCPUのコア数になります。"+
                        "（Hyper-Threadingを有効にしている時は、物理コア数ではなく、論理コア数になります。）" +
                        "このオプションを無効にすると、『スレッド数』で指定した数だけスレッドを強制的に割り当てます。\r\n" +
                        "※　スレッドについての詳しい説明は『スレッド設定』のところにマウスをホバーさせてください。",
                        "option name AutoThread_ type check default true"
                        ),

                    new EngineOptionDescription("Threads"           , "スレッド数" ,
                        "手動でスレッド数を指定したい時に使います。",
                        "『自動スレッド』がオフの時は、ここで設定されたスレッド数に従って、思考時のスレッド数を決定します。" +
                        "『自動スレッド』がオンの時は、ここで設定した値は無視されます。\r\n"+
                        "並列的に探索するので、CPUコアの数までは増やしたほうが、強くなります。"+
                        "スレッド数を増やすとCPU負荷率が上がるのでCPUの温度を下げたい時などはスレッド数を減らしてみてください。" +
                        "なお、PCの論理コア数より増やしても棋力は普通、強くなりません。\r\n"+
                        "※　スレッドについての詳しい説明は『スレッド設定』のところにマウスをホバーさせてください。",
                        "option name Threads type spin default 4 min 1 max 4096"),

                    // -- 以下、CreateEngineCommonOptions()のほうで設定いるものと重複している。
                    // こちらは、gpsfishなど外部エンジンに対して表示する用。

                    new EngineOptionDescription(null           , "思考設定" ,
                        null,
                        "エンジンの思考の設定です。棋力に直接的な影響のある部分です。",
                        null
                        ),

                    new EngineOptionDescription("MultiPV"   , null ,
                        "候補手の数を設定します",
                        "例えば、この項目(MultiPV)を5に設定すると、思考するときに候補手として5手挙げます。\r\n"+
                        "このとき、5手分調べないといけなくなるので、MultiPVを1に設定している時の約5倍の時間がかかるようになります。" +
                        "通常対局の時は1に設定しておくのが一番強くなります。\r\n" +
                        "また、検討モードの時は、この設定値は無視され、検討ウィンドウの『候補手』の数が反映されます。",

                        "option name MultiPV type spin default 1 min 1 max 800"),

                    // ここに水平線欲しいので「その他」という見出しを作る。

                    new EngineOptionDescription(null, "その他" ,
                        null ,
                        "その他の設定",
                        null
                        ),

                }
            };

            return setting;
        }


        /// <summary>
        /// エンジンオプションの共通設定に表示すべきオブジェクトを生成する。
        /// これをそのままエンジンオプションの設定ダイアログにbindすれば
        /// いい感じに表示される。
        /// </summary>
        /// <returns></returns>
        public static EngineOptionsForSetting CreateEngineCommonOptions()
        {
            // 生成に時間がかかる＆エンジンの起動ごとに必要なので
            // 生成したものを保存している。
            
            if (engineCommonOptions == null)
                engineCommonOptions = CreateEngineCommonOptions_();

            return engineCommonOptions;
        }

        /// <summary>
        /// singleton object
        /// </summary>
        private static EngineOptionsForSetting engineCommonOptions;

        private static EngineOptionsForSetting CreateEngineCommonOptions_()
        {
            var setting = new EngineOptionsForSetting()
            {

                Descriptions = new List<EngineOptionDescription>()
                {
                    // -- 定跡設定

                    new EngineOptionDescription(null           , "定跡設定" ,
                        null,
                        "コンピューターが用いる定跡はここで設定します。",
                        null),

                    new EngineOptionDescription("BookFile"      , "定跡ファイル" ,
                        "コンピューターが用いる定跡ファイル。",
                        "各エンジンのフォルダ配下のbookフォルダ内にある定跡ファイルが読み込まれます。",

                        "option name BookFile type combo default standard_book.db "+
                        "var no_book var standard_book.db var yaneura_book1.db var yaneura_book2.db var yaneura_book3.db var yaneura_book4.db "+
                        "var user_book1.db var user_book2.db var user_book3.db var book.bin"
                        ){
                        ComboboxDisplayName =
                        "no_book,定跡なし,standard_book.db,やねうら大定跡,yaneura_book1.db,裏やねうら定跡,"+
                        "yaneura_book2.db,真やねうら定跡,yaneura_book3.db,極やねうら定跡,yaneura_book4.db,やねうら定跡2017,"+
                        "user_book1.db,ユーザー定跡1,user_book2.db,ユーザー定跡2,user_book3.db,ユーザー定跡3,book.bin,Aperyの定跡ファイル"
                        },

                    new EngineOptionDescription("BookOnTheFly"      , null ,
                        "定跡ファイルを対局開始時にメモリに丸読みしない。",
                        "この設定をオンにすると、定跡ファイルを対局開始時にメモリに丸読みしません。" +
                        "大きな定跡ファイルに対しては、対局開始時に丸読みしなくて済むので起動時間が短くなる効果がありますが、"+
                        "そんな巨大な定跡ファイルを使うことはあまりないため、デフォルトではオフになっています。",

                        "option name BookOnTheFly type check default false"
                        ),

                    new EngineOptionDescription("BookMoves"      , null ,
                        "定跡を用いる手数(0=未使用)。",
                        "例えば、定跡を16手目まで使用したい(17手目からは定跡を使わない)なら16を指定します。",

                        "option name BookMoves type spin default 16 min 0 max 10000"
                        ),

                    new EngineOptionDescription("BookIgnoreRate"      , null ,
                        "一定の確率で定跡を無視して自力で思考させる確率[%]",
                        "定跡を用いると毎回同じような対局内容になるのを回避するために、" +
                        "ここで指定した確率で定跡を採択せずに自力で思考させることが出来ます。" +
                        "例えば、30を指定すると30%の確率で定跡にhitしてもそれを無視して自力で思考します。",

                        "option name BookIgnoreRate type spin default 0 min 0 max 100"
                        ),

                    new EngineOptionDescription("BookEvalDiff"      , null ,
                        "定跡の第一候補手との評価値の差",
                        "定跡の指し手で1番目の候補の指し手と、2番目以降の候補の指し手との評価値の差が、" +
                        "この範囲内であれば採用する。(1番目の候補の指し手しか選ばれて欲しくないときは0を指定する)\r\n" +
                        "指し手に評価値がついている定跡ファイルに対してのみ有効。",

                        "option name BookEvalDiff type spin default 30 min 0 max 99999"
                        ),

                    new EngineOptionDescription("BookEvalBlackLimit"      , null ,
                        "定跡の先手の評価値下限",
                        "定跡の指し手のうち、先手のときの評価値の下限。これより評価値が低くなる指し手は選択しない。\r\n" +
                        "指し手に評価値がついている定跡ファイルに対してのみ有効。",

                        "option name BookEvalBlackLimit type spin default 0 min -99999 max 99999"
                        ),

                    new EngineOptionDescription("BookEvalWhiteLimit"      , null ,
                        "定跡の後手の評価値下限",
                        "定跡の指し手のうち、後手のときの評価値の下限。これより評価値が低くなる指し手は選択しない。\r\n" +
                        "指し手に評価値がついている定跡ファイルに対してのみ有効。",

                        "option name BookEvalWhiteLimit type spin default -140 min -99999 max 99999"
                        ),

                    new EngineOptionDescription("BookDepthLimit"      , null ,
                        "定跡の指し手のdepth下限",
                        "定跡に登録されている指し手の(定跡生成時の)depthがこれを下回るなら採用しない。0を指定するとdepth無視。\r\n" +
                        "指し手にdepth情報がついている定跡ファイルに対してのみ有効。",

                        "option name BookDepthLimit type spin default 16 min 0 max 99999"
                        ),

                    new EngineOptionDescription("NarrowBook"      , null ,
                        "実現確率の低い定跡を採用しない",
                        "定跡ファイルの指し手に、出現頻度の情報がついている時に、出現頻度が低い指し手は採用しないためのオプション。",

                        "option name NarrowBook type check default false"
                        ),

                    new EngineOptionDescription("ConsiderBookMoveCount"      , null ,
                        "定跡の指し手の選択を出現頻度に比例させる",
                        "定跡ファイルの指し手に、出現頻度の情報がついている時に、その出現頻度に比例する形でランダムに指し手を選択する。\r\n" +
                        "このオプションをオフにしている時は、定跡の指し手が等確率で選択されます。",

                        "option name ConsiderBookMoveCount type check default false"
                        ),


                    // -- 持将棋の設定

                    new EngineOptionDescription(null           , "持将棋の設定" ,
                        null,
                        "コンピューター側の持将棋の設定を変更します。",
                        null
                        ),

                    new EngineOptionDescription("MaxMovesToDraw"   , null ,
                        "この手数になると持将棋で引き分けとして扱います。",
                        "この項目を変更する時は、対局設定のほうの『指定手数で引き分けとする』もこの設定に合わせて変更してください。\r\n"+
                        "またこの項目に0を指定した場合、この設定は無効(手数による持将棋を適用せず)になります。",

                        "option name MaxMovesToDraw type spin default 0 min 0 max 100000")
                        ,

                    new EngineOptionDescription("EnteringKingRule"   , "入玉条件" ,
                        "コンピューターの入玉条件を変更します。",
                        "この項目を変更する時は、対局設定のほうの入玉設定もこの設定に合わせて変更してください。\r\n"+
                        "デフォルトは、27点法です。",

                        "option name EnteringKingRule type combo default CSARule27 var NoEnteringKing var CSARule24 var CSARule27 var TryRule")
                        {
                        ComboboxDisplayName =
                        "NoEnteringKing,入玉ルールなし,CSARule27,27点法(CSAルール),CSARule24,24点法(CSAルール),TryRule,トライルール"
                        },
                    

                    // -- 思考時間設定

                    new EngineOptionDescription(null           , "思考時間設定" ,
                        null,
                        "コンピュータの最小思考時間や着手時の遅延時間を設定します。",
                        null
                        ),

                    new EngineOptionDescription("MinimumThinkingTime"   , "最小思考時間" ,
                        "コンピュータが1手に使う最小時間を設定します。単位は[ms]です。",
                        "コンピュータが1手に使う最小時間を設定します。単位は[ms]です。\r\n"+
                        "例えば、この値を2000に設定すると、コンピューターは少なくとも2秒は考えます。(実際には『NetworkDelay』の分だけ早く指し手を返します。)"+
                        "2秒の考慮時間でも秒未満が切り捨てられて計測上1秒になるルール下では2000に設定するのがベストです。",

                        "option name MinimumThinkingTime type spin default 2000 min 1000 max 100000"),

                    new EngineOptionDescription("NetworkDelay"   , null ,
                        "コンピュータの着手時の遅延時間の設定その1。単位は[ms]です。",
                        "コンピュータの着手時の遅延時間の設定その1。単位は[ms]です。\r\n"+
                        "コンピュータは、ここで指定した時間だけ早めに指し手を返します。例えば、1手3秒で指す予定の時にこの値を300[ms]に設定した場合、2.7秒で指し手を返します。",

                        "option name NetworkDelay type spin default 120 min 0 max 10000"),

                    new EngineOptionDescription("NetworkDelay2"   , null ,
                        "コンピュータの着手時の遅延時間の設定その2。単位は[ms]です。",
                        "コンピュータの着手時の遅延時間の設定その2。単位は[ms]です。\r\n"+
                        "コンピュータは、ここで指定した時間だけ早めに指し手を返します。(切れ負け時用)\r\n"+
                        "例えば、秒読み10秒(1手ごとに10秒で、それを過ぎると切れ負け)になる指し手の時に、この値を300[ms]に設定した場合、9.7秒で指し手を返します。\r\n"+
                        "ネットワーク対局で、サーバー側が重い場合やネットワーク遅延がある場合には大きめの値(1000～1500)にすることを推奨します。",

                        "option name NetworkDelay2 type spin default 1120 min 0 max 10000"),


                    // -- 思考設定

                    new EngineOptionDescription(null           , "思考設定" ,
                        null,
                        "エンジンの思考の設定です。棋力に直接的な影響のある部分です。",
                        null
                        ),

                    new EngineOptionDescription("MultiPV"   , null ,
                        "候補手の数を設定します",
                        "例えば、この項目(MultiPV)を5に設定すると、思考するときに候補手として5手挙げます。\r\n"+
                        "このとき、5手分調べないといけなくなるので、MultiPVを1に設定している時の約5倍の時間がかかるようになります。" +
                        "通常対局の時は1に設定しておくのが一番強くなります。\r\n" +
                        "また、検討モードの時は、この設定値は無視され、検討ウィンドウの『候補手』の数が反映されます。",

                        "option name MultiPV type spin default 1 min 1 max 800"),

                    new EngineOptionDescription("SlowMover"   , null ,
                        "序盤重視度。大きくすると持ち時間の序盤への配分が増えます。",
                        "デフォルトは、100[%]。例えば、この値を70にすると序盤の1手に用いる時間が本来の時間の70%になり、序盤にかける時間が短くなります。",

                        "option name SlowMover type spin default 100 min 1 max 1000"),

                    new EngineOptionDescription("ResignValue"   , null ,
                        "投了スコアです。(相手から見た)評価値がこの値を超えると投了します。",
                        "例えば、相手からの詰みが見えた時に投了するのであれば、30000(詰みはこれより大きな値)と設定します。\r\n"+
                        "また、序盤で形勢に差がついた時に思考エンジンに投了させたいのであれば、1000～3000ぐらいの数値を設定すると良いでしょう。",

                        "option name ResignValue type spin default 99999 min 0 max 99999"),

                    new EngineOptionDescription("Contempt"   , null ,
                        "千日手を受け入れるスコアを設定します。",
                        "大きな値にすると千日手を(無理にでも)打開しやすくなります。\r\n" +
                        "千日手の局面のスコア(評価値)は、ここで設定した値に -1 を掛けた値になります。" +
                        "例えば、この値を100に設定すれば、千日手の局面のスコアは-100とみなされます。" +
                        "このとき、-50(歩の半分ぐらいの価値だけ互角より損している局面)になる指し手があるなら、千日手の局面を選ばずにそちらを選びます。" +
                        "デフォルトは2(互角に近いなら千日手の局面を回避して欲しいが、無理な打開はして欲しくないので。)",

                        "option name Contempt type spin default 2 min -30000 max 30000"),

                    new EngineOptionDescription("ContemptFromBlack"   , null ,
                        "Contemptの設定値を先手番から見た値とするオプション。",
                        "先手のときは千日手を狙いたくなくて、後手のときは千日手を狙いたいような場合、"+
                        "このオプションをオンにすれば、Contemptをそういう解釈にしてくれる。"+
                        "(Contemptを常に先手から見たスコアだとみなしてくれる。) デフォルトではオフ。",

                        "option name ContemptFromBlack type check default false"),

                    new EngineOptionDescription("DepthLimit"   , null ,
                        "探索深さ制限。",
                        "この値を0以外に設定すると、その値の探索深さまでしか探索しません。(弱くなります)　強さを調整したい時に使います。",

                        "option name DepthLimit type spin default 0 min 0 max 2147483647"),

                    new EngineOptionDescription("NodesLimit"   , null ,
                        "探索ノード制限。",
                        "この値を0以外に設定すると、その値の探索ノード数(局面数)までしか探索しません。(弱くなります)　強さを調整したい時に使います。",

                        "option name NodesLimit type spin default 0 min 0 max 9223372036854775807"),

                    new EngineOptionDescription("nodestime"   , null ,
                        "node as timeモード。0以外に設定すると有効。",
                        "《上級者向けの設定項目です。普通使いません。》\r\n"+
                        "時間の代わりにノード時間を用いる。この値を0以外に設定すると、有効。\r\n" +
                        "時間の代わりに探索ノード数を決めて探索するときのミリ秒当たりのnode数。\r\n"+
                        "この値を600と指定した場合、持ち時間1秒に対して600000ノード(の時間が与えられたものとして)探索します。",

                        "option name nodestime type spin default 0 min 0 max 99999"),

                    // -- 評価関数

                    new EngineOptionDescription(null           , "評価関数の設定" ,
                        null,
                        "評価関数の設定です。",
                        null
                        ),

                    new EngineOptionDescription("EvalDir"   , null ,
                        "評価関数用のファイルのあるフォルダの設定。",
                        "評価関数用のファイルの配置するフォルダを変更する時にこの設定を使います。通常、変更する必要はありません。",
                        "option name EvalDir type string default eval"),

                    new EngineOptionDescription("EvalShare"   , null ,
                        "評価関数ファイルをメモリ上で共用する設定。",
                        "同じ思考エンジンを2つ以上動かす時に、評価関数ファイルをメモリ上で共用する設定です。\r\n" +
                        "これをオンにして、同じ思考エンジンを2つ動作させた場合、消費する物理メモリが節約できます。",
                        "option name EvalShare type check default true"),

                    // -- 読み筋の表示

                    new EngineOptionDescription(null           , "読み筋の表示" ,
                        null,
                        "読み筋の表示に関する設定です。",
                        null
                        ),

                    new EngineOptionDescription("PvInterval"   , null ,
                        "読み筋を出力する最小間隔を設定します。単位は[ms]",
                        "思考エンジンが読み筋を出力する最小間隔を設定します。" +
                        "これをあまり小さい値にすると読み筋が出力される回数が増えて、見づらくなります。" +
                        "また出力にも時間がかかるため、あまりたくさん出力すると棋力に影響します。",

                        "option name PvInterval type spin default 300 min 0 max 100000"),

                    new EngineOptionDescription("ConsiderationMode"   , null ,
                        "なるべく綺麗な読み筋を出力します。",
                        "この設定をオンにすると、読み筋が途中で途切れにくくなります。(読み筋の出力頻度は少し減ります。)",

                        "option name ConsiderationMode type check default false"),

                    new EngineOptionDescription("OutputFailLHPV"   , null ,
                        "fail low/highのときにPVを出力する。",
                        "fail highというのは、いまの読み筋より良い指し手が存在するらしきことがわかったので、" +
                        "それを調べ直している状態のことです。(fail lowはその逆) そのため、fail low/highのとき読み筋の表示は短くなります。" +
                        "このオプションをオフにすると、fail low/highの時には読み筋を出力しなくなるので、短い読み筋が表示されにくくなります。\r\n" +
                        "このオプションは、ConsiderationModeオンのときも有効。",

                        "option name OutputFailLHPV type check default true"),


                    // -- デバッグ用

                    new EngineOptionDescription(null           , "デバッグ用" ,
                        null,
                        "デバッグ用の項目です。一般ユーザーが使うことはありません。",
                        null
                        ),

                    new EngineOptionDescription("WriteDebugLog"   , null ,
                        "思考エンジンとGUIとのやりとりをエンジン側からファイルに書き出します。",
                        "思考エンジンとGUIとのやりとりをエンジン側からファイルに書き出します。",
                        "option name WriteDebugLog type check default false"),

                    // これは非表示にしておく。
                    new EngineOptionDescription("Param1") { Hide = true },
                    new EngineOptionDescription("Param2") { Hide = true },
                    new EngineOptionDescription("SkipLoadingEval") { Hide = true },
                    new EngineOptionDescription("EvalSaveDir") { Hide = true },

                    // -- その他

                    // すべて分類したので、このエンジン設定にはその他は必要ない。gpsfishのような他のエンジンを追加する時には必要。

                    new EngineOptionDescription(null, "その他" ,"その他の設定",null) { Hide = true },
                }
            };

            // ハッシュ設定、スレッド設定をいま生成したsettingの前方に挿入する。
            // MultiPVとPonderはすでに持っているので、こういう重複を除去しながら挿入する。
            var header = CreateEngineMinimumOptions();
            //setting.Descriptions.InsertRange(0, header.Descriptions);

            foreach (var h in header.Descriptions.ReverseIterator() /* 逆向きのiterator */)
                if (!setting.Descriptions.Exists(x => x.DisplayName == h.DisplayName))
                    setting.Descriptions.Insert(0, h);
            
            return setting;
        }

        /// <summary>
        /// 通常対局、検討モードの共通設定のデフォルト値を返す。
        /// </summary>
        /// <returns></returns>
        public static List<EngineOption> CommonOptionDefault()
        {
            if (commonOptionDefault == null)
            {
                var options = new List<EngineOption>();

                // エンジンオプションの共通設定のDescriptionからEngineOptionsをひねり出す。

                var opt = CreateEngineCommonOptions();
                foreach (var desc in opt.Descriptions)
                {
                    // 見出し行、非表示の奴はskipする。
                    if (desc.Name == null || desc.Hide || desc.UsiBuildString == null /* これ存在がおかしい気はする*/)
                        continue;

                    // この文字列でUsiOptionオブジェクトを構築して、nameとdefault値を得る
                    var usiOption = UsiOption.Parse(desc.UsiBuildString);
                    options.Add(new EngineOption(usiOption.Name, usiOption.GetDefault()));
                }

                commonOptionDefault = options;
            }
            return commonOptionDefault;
        }

        private static List<EngineOption> commonOptionDefault;
    }
}
