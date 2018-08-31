using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace MyShogiUpdater
{
    /// <summary>
    /// メインアプリ
    /// </summary>
    public class TheApp
    {
#if true
        /// <summary>
        /// やねうら王のバージョン
        /// 
        /// "2018" : 『将棋神やねうら王』
        /// </summary>
        public string YaneuraOuVersion = "2018";

        /// <summary>
        /// Update用のソースフォルダ
        /// </summary>
        public string UpdateSourcePath = "V100toV110"; // Update1
        //public string UpdateSourcePath = "V100toV120";
#endif 

        /// <summary>
        /// パッチを作成するときの比較元と比較先のフォルダ
        /// この2つのフォルダの差分を生成する。
        /// 
        /// MakePath == trueなら起動時にパッチを作成する。
        /// </summary>
        public bool MakePatchEnable = false;
        public string PatchSource1 = "V100";
        public string PatchSource2 = "V110";

        /// <summary>
        /// アプリのエントリーポイント
        /// </summary>
        public void Start(string [] args)
        {
            // Patchを作成するとき用。
            if (MakePatchEnable)
                MakePatch();

            var mainForm = new MainDialog();
            var model = mainForm.ViewModel;

            // やねうら王のバージョンに応じたダイアログを表示
            switch (YaneuraOuVersion)
            {
                case "2018":
                    // とりま、『将棋神やねうら王』用。
                    // メインのダイアログの表示

                    model.ProductName = "『将棋神 やねうら王』";
                    model.UpdateTextFile = Path.Combine(UpdateSourcePath , $"{UpdateSourcePath}_info.txt");
                    var program_files = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    model.InstallFolder = Path.Combine(program_files, "YaneuraOu2018");
                    model.SourceFolder = UpdateSourcePath;
                    break;

                default:
                    Debug.Assert(true);
                    break;
            }

            // 権限昇格しての実行であるか。
            if (args.Length >= 2)
            {
                if (args[0] == "FolderCopy")
                {
                    model.InstallFolder = args[1];
                    model.AutoInstall = true;
                }
            }

            mainForm.Init();
            Application.Run(mainForm);
        }

        /// <summary>
        /// Patchを作成するときのコード
        /// 実行ファイル配下にある2つのフォルダを対象として、その差分を生成する。
        /// </summary>
        private void MakePatch()
        {
            // 例) V100とV108との差分を生成する。
            //PatchMaker.MakePatch("V100", "V108");

            //PatchMaker.MakePatch("V108", "V108a");

            PatchMaker.MakePatch(PatchSource1, PatchSource2);
            Console.WriteLine("patch done");
        }

        /// <summary>
        /// singletonなinstance
        /// </summary>
        public static TheApp App = new TheApp();
    }
}
