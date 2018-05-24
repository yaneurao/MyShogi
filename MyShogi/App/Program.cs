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

#if false
            // デバッグ用のFormを生成するとき
            Application.Run(new TestForm1());
#endif

#if true
            // 本番用
            // singletonなTheAppインスタンスを生成して実行するだけ
            var app = new TheApp();
            app.Run();
#endif
        }
    }
}
