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
        /// エンジンオプションの共通設定に表示すべきオブジェクトを生成する。
        /// これをそのままエンジンオプションの設定ダイアログにbindすれば
        /// いい感じに表示される。
        /// </summary>
        /// <returns></returns>
        public static EngineOptionsForSetting CreateEngineCommonOptions()
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
                        "ハッシュ用のメモリが不十分だと探索の効率が落ちます。(コンピューターの棋力が弱くなります)",
                        null),

                    new EngineOptionDescription("AutoHash_"           , "自動ハッシュ" ,
                        "ハッシュメモリを自動的に割り当てます。",
                        "コンピューターが思考する時にハッシュメモリというものを使います。"+
                        "これは、一度調べた局面の情報を保存しておくために必要です。" +
                        "このオプションを有効にすると、空き物理メモリから「Hash割合」の分だけ自動的にハッシュメモリを割当てます。"+
                        "このオプションを無効にすると、「Hash[MB]」の分だけ強制的にハッシュメモリを割り当てます。",
                        "option name AutoHash_ type check default true"
                        ),

                    // 以下の2つのDescription、先頭にスペースを入れて字下げしておく。

                    new EngineOptionDescription("AutoHashPercentage_" , "　Hash割合"   ,
                        "空き物理メモリの何%をハッシュメモリに用いるかを設定します。" ,
                        "上記の『自動ハッシュ割当』をオンにしている時に、空き物理メモリの何%をハッシュメモリに用いるかを設定します。"+
                        "0%～100%までの数値で指定できます。小さな値を指定した場合、思考エンジンの最低必要ハッシュメモリに足りなくなるので" +
                        "その場合、後者の数値が優先されます。",
                        "option name AutoHashPercentage_ type spin default 80 min 0 max 100"
                        ),

                    // ※　"Hash_"という項目に設定しているが、これは、のちに"USI_Hash"か"Hash"に置換される。

                    new EngineOptionDescription("Hash_"          , "　Hash[MB]",
                        "何[MB]をハッシュメモリに用いるかを設定します。",
                        "上記の『自動ハッシュ割当』をオフにしている時に、何[MB]をハッシュメモリに用いるかを手動で設定します。ここで指定した値が"+
                        "思考エンジンの最低必要ハッシュメモリに足りない場合、後者の値が優先されます。",
                        "option name Hash_ type spin default 4096 min 0 max 1048576"),


                    // -- スレッド設定

                    new EngineOptionDescription(null           , "スレッド設定" ,
                        null,
                        "コンピューターが思考する時にCPUのコア数分までは並列的に探索したほうが強くなります。"+
                        "例えば、4スレッドなら4つ並列して探索するという意味です。ここではその設定を行います。" +
                        "スレッド数を増やすとCPU負荷率が上がるのでCPUの温度を下げたい時などはスレッド数を減らしてみてください。",
                        null),

                    new EngineOptionDescription("AutoThread_"           , "　自動スレッド" ,
                        "思考の時に用いるスレッド数を自動的に決定します。",
                        "このオプションを有効にすると、思考の時に使用するスレッド数が、自動的にこのPCに搭載されているCPUのコア数になります。"+
                        "（Hyper-Threadingを有効にしている時は、物理コア数ではなく、論理コア数になります。）" +
                        "このオプションを無効にすると、『スレッド数』で指定した数だけスレッドを強制的に割り当てます。",
                        "option name AutoThread_ type check default true"
                        ),

                    new EngineOptionDescription("Threads"           , "　スレッド数" ,
                        "『自動スレッド』がオフの時に、手動でスレッド数を指定したい時に使います。",
                        "『自動スレッド』がオフの時は、ここで設定されたスレッド数に従って、思考時のスレッド数を決定します。"+
                        "並列的に探索するので、CPUコアの数までは増やしたほうが、強くなります。"+
                        "スレッド数を増やすとCPU負荷率が上がるのでCPUの温度を下げたい時などはスレッド数を減らしてみてください。" +
                        "なお、PCの論理コア数より増やしても棋力は普通、強くなりません。",
                        "option name Threads type spin default 4 min 1 max 4096"),


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



                    // -- 持将棋の設定

                    new EngineOptionDescription(null           , "持将棋の設定" ,
                        null,
                        "コンピューター側の持将棋の設定を変更します。",
                        null
                        ),

                    new EngineOptionDescription("MaxMovesToDraw"   , "手数による持将棋" ,
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

                    new EngineOptionDescription("SlowMover"   , null ,
                        "序盤重視度。大きくすると持ち時間の序盤への配分が増えます。",
                        "デフォルトは、100[%]。例えば、この値を70にすると序盤の1手に用いる時間が本来の時間の70%になり、序盤にかける時間が短くなります。",

                        "option name SlowMover type spin default 100 min 1 max 1000"),

                    new EngineOptionDescription("ResignValue"   , null ,
                        "投了スコアです。(相手から見た)評価値がこの値を超えると投了します。",
                        "例えば、相手からの詰みが見えた時に投了するのであれば、30000(詰みはこれより大きな値)と設定します。\r\n"+
                        "また、序盤で形勢に差がついた時に思考エンジンに投了させたいのであれば、1000～3000ぐらいの数値を設定すると良いでしょう。",

                        "option name ResignValue type spin default 99999 min 0 max 99999"),


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
                        "この値を600と指定した場合、持ち時間1秒に対して600000ノード(の時間が与えられたものとして)探索する。",

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


                    
                }
            };

            return setting;
        }

    }
}
