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
                new EngineOptionForSetting("AutoHash_","true",
                    "option name USI_Ponder type check default true"),
                new EngineOptionForSetting("AutoHashPercentage_","80",
                    "option name USI_Hash type spin default 256"),
                new EngineOptionForSetting("USI_Hash", "4096",
                    "option name USI_Hash type spin default 4096"),
            };

            setting.Descriptions = new List<EngineOptionDescription>()
            {
                // -- これは仮想optionで、実際にエンジンはこのoption名を持っていることはありえない。
                // 内部的な設定に用いる。

                new EngineOptionDescription("AutoHash_"           , "自動ハッシュ割当" ,
                    "思考の時に用いるハッシュメモリ(一度調べた局面の情報を保存しておくために必要)を空き物理メモリから自動的に割当てます。" ),
                new EngineOptionDescription("AutoHashPercentage_" , "Hashの空き物理メモリに対する割合"   ,
                    "上記の『自動ハッシュ割当』をオンにしている時に、空き物理メモリの何%をハッシュメモリに用いるかを設定します。" ),
                new EngineOptionDescription("USI_Hash"            , "Hash[MB]",
                    "上記の『自動ハッシュ割当』をオフにしている時に、何[MB]をハッシュメモリに用いるかを設定します。"),

                //new EngineOptionDescription("入玉設定")
            };

            return setting;
        }

    }
}
