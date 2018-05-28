using System;
using System.Windows.Forms;
using MyShogi.App;

namespace MyShogi
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // モデルの初期化一式
            Model.Shogi.Core.Initializer.Init();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // singletonなTheAppインスタンスを生成して実行するだけ
            TheApp.app.Run();
        }
    }
}
