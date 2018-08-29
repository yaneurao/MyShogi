using System;
using System.IO;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Kifu;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局結果を保存するときの設定
    ///
    /// GameResultWindowSettingDialogとdata bindして使う。
    /// </summary>
    public class GameResultSetting : NotifyObject
    {
        public GameResultSetting()
        {
            // デフォルトでは自動保存、有効。
            AutomaticSaveKifu = true;

            // デフォルトではKIF形式
            KifuFileType = KifuFileType.KIF;

            // 棋譜の保存上限1000に設定しておく。
            // 対局結果ウィンドウでこのindexを丸読みするので、あまり大きくしたくないところである。
            KifuFileLimit = 1000;

            // デフォルトでは "Documents/YaneuraOuKifu" を保存先に設定しておく。
            KifuSaveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) , "YaneuraOuKifu");

            try
            {
                if (!Directory.Exists(KifuSaveFolder))
                    Directory.CreateDirectory(KifuSaveFolder);
            }
            catch { }
        }

        /// <summary>
        /// 棋譜を自動保存する。
        /// </summary>
        public bool AutomaticSaveKifu
        {
            get { return GetValue<bool>("AutomaticSaveKifu"); }
            set { SetValue("AutomaticSaveKifu", value); }
        }

        /// <summary>
        /// 保存する棋譜のファイル形式
        /// </summary>
        public KifuFileType KifuFileType
        {
            get { return GetValue<KifuFileType>("KifuFileType"); }
            set { SetValue("KifuFileType", value); }
        }

        /// <summary>
        /// 棋譜ファイルを保存する上限
        ///
        /// 棋譜ファイルは、対局結果ウィンドウを開いたときに、この上限数を上回っている分が自動的に削除される。
        /// 
        /// 0 = 制限なし
        /// </summary>
        public int KifuFileLimit
        {
            get { return GetValue<int>("KifuFileLimit"); }
            set { SetValue("KifuFileLimit", value); }
        }

        /// <summary>
        /// 棋譜の自動保存先のフォルダ
        /// </summary>
        public string KifuSaveFolder
        {
            get { return GetValue<string>("KifuSaveFolder"); }
            set { SetValue("KifuSaveFolder", value); }
        }

        /// <summary>
        /// csvファイルの保存pathを返す。
        /// これは棋譜保存フォルダに"game_result.csv"というファイル名で存在するものとする。
        /// </summary>
        /// <returns></returns>
        public string CsvFilePath()
        {
            return Path.Combine(KifuSaveFolder, "game_result.csv");
        }
    }
}
