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
            var setting = new EngineOptionsForSetting();

            setting.Options = new List<EngineOptionForSetting>()
            {
                new EngineOptionForSetting("AutoHash_",
                    "option name AutoHash_ type check default true"),
                new EngineOptionForSetting("AutoHashPercentage_",
                    "option name AutoHashPercentage_ type spin default 80 min 0 max 100"),
                new EngineOptionForSetting("USI_Hash",
                    "option name USI_Hash type spin default 4096 min 0 max 999999999"),
                new EngineOptionForSetting("AutoThread_",
                    "option name AutoThread_ type check default true"),
                new EngineOptionForSetting("Thread",
                    "option name Thread type spin default 4 min 1 max 4096"),
                new EngineOptionForSetting("BookFile",
                    "option name BookFile type combo default standard_book.db "+
                    "var no_book var standard_book.db var yaneura_book1.db var yaneura_book2.db var yaneura_book3.db var yaneura_book4.db "+
                    "var user_book1.db var user_book2.db var user_book3.db var book.bin"),
                new EngineOptionForSetting("EnteringKingRule",
                    "option name EnteringKingRule type combo default CSARule27 var NoEnteringKing var CSARule24 var CSARule27 var TryRule"),
            };

            setting.Descriptions = new List<EngineOptionDescription>()
            {
                // -- これは仮想optionで、実際にエンジンはこのoption名を持っていることはありえない。
                // 内部的な設定に用いる。

                // nameがnullなら、これは見出し表示になる。

                new EngineOptionDescription(null           , "ハッシュメモリ管理" ,
                    null,
                    "コンピューターが思考する時にハッシュメモリというものを使います。"+
                    "これは、一度調べた局面の情報を保存しておくために必要です。" +
                    "ハッシュ用のメモリが不十分だと探索の効率が落ちます。(コンピューターの棋力が弱くなります)" ),

                new EngineOptionDescription("AutoHash_"           , "自動ハッシュ" ,
                    "ハッシュメモリを自動的に割り当てます。",
                    "コンピューターが思考する時にハッシュメモリというものを使います。"+
                    "これは、一度調べた局面の情報を保存しておくために必要です。" +
                    "このオプションを有効にすると、空き物理メモリから「Hash割合」の分だけ自動的にハッシュメモリを割当てます。"+
                    "このオプションを無効にすると、「Hash[MB]」の分だけ強制的にハッシュメモリを割り当てます。"
                    ),

                // 以下の2つのDescription、先頭にスペースを入れて字下げしておく。

                new EngineOptionDescription("AutoHashPercentage_" , "　Hash割合"   ,
                    "空き物理メモリの何%をハッシュメモリに用いるかを設定します。" ,
                    "上記の『自動ハッシュ割当』をオンにしている時に、空き物理メモリの何%をハッシュメモリに用いるかを設定します。"+
                    "0%～100%までの数値で指定できます。小さな値を指定した場合、思考エンジンの最低必要ハッシュメモリに足りなくなるので" +
                    "その場合、後者の数値が優先されます。"
                    ),
                new EngineOptionDescription("USI_Hash"            , "　Hash[MB]",
                    "何[MB]をハッシュメモリに用いるかを設定します。",
                    "上記の『自動ハッシュ割当』をオフにしている時に、何[MB]をハッシュメモリに用いるかを手動で設定します。ここで指定した値が"+
                    "思考エンジンの最低必要ハッシュメモリに足りない場合、後者の値が優先されます。"
                    ),

                new EngineOptionDescription(null           , "スレッド設定" ,
                    null,
                    "コンピューターが思考する時にCPUのコア数分までは並列的に探索したほうが強くなります。"+
                    "例えば、4スレッドなら4つ並列して探索するという意味です。ここではその設定を行います。" +
                    "スレッド数を増やすとCPU負荷率が上がるのでCPUの温度を下げたい時などはスレッド数を減らしてみてください。" ),

                new EngineOptionDescription("AutoThread_"           , "　自動スレッド" ,
                    "思考の時に用いるスレッド数を自動的に決定します。",
                    "このオプションを有効にすると、思考の時に使用するスレッド数が、自動的にこのPCに搭載されているCPUのコア数になります。"+
                    "（Hyper-Threadingを有効にしている時は、物理コア数ではなく、論理コア数になります。）" +
                    "このオプションを無効にすると、『スレッド数』で指定した数だけスレッドを強制的に割り当てます。"
                    ),

                new EngineOptionDescription("Thread"           , "　スレッド数" ,
                    "『自動スレッド』がオフの時に、手動でスレッド数を指定したい時に使います。",
                    "『自動スレッド』がオフの時は、ここで設定されたスレッド数に従って、思考時のスレッド数を決定します。"+
                    "並列的に探索するので、CPUコアの数までは増やしたほうが、強くなります。"+
                    "スレッド数を増やすとCPU負荷率が上がるのでCPUの温度を下げたい時などはスレッド数を減らしてみてください。" +
                    "なお、PCの論理コア数より増やしても棋力は普通、強くなりません。"
                    ),

                new EngineOptionDescription(null           , "定跡設定" ,
                    null,
                    "コンピューターが用いる定跡はここで設定します。"
                    ),

                new EngineOptionDescription("BookFile"      , "定跡ファイル" ,
                    "コンピューターが用いる定跡ファイル。\r\n",
                    "no_book          : 定跡なし。\r\n"+
                    "standard_book.db : やねうら大定跡\r\n"+
                    "yaneura_book1.db : 裏やねうら定跡\r\n"+
                    "yaneura_book2.db : 真やねうら定跡\r\n"+
                    "yaneura_book3.db : 極やねうら定跡\r\n"+
                    "yaneura_book4.db : やねうら定跡2017\r\n"+
                    "user_book1.db    : ユーザー定跡1\r\n"+
                    "user_book2.db    : ユーザー定跡2\r\n"+
                    "user_book3.db    : ユーザー定跡3\r\n"+
                    "book.db          : Aperyの定跡ファイル"
                    ),

                new EngineOptionDescription(null           , "入玉設定" ,
                    null,
                    "コンピューターの入玉条件を変更します。"
                    ),

                new EngineOptionDescription("EnteringKingRule"   , "入玉条件" ,
                    "コンピューターの入玉条件を変更します。",
                    "NoEnteringKing : 入玉なし。\r\n"+
                    "CSARule27      : 27点法(CSAルール) これがデフォルトです。\r\n"+
                    "CSARule24      : 24点法(CSAルール)\r\n"+
                    "TryRule        : トライルール\r\n"+
                    "この項目を変更する時は、対局設定のほうの入玉設定もこの設定に合わせて変更してください。"
                    ),

                //new EngineOptionDescription("入玉設定")
            };

            return setting;
        }

    }
}
