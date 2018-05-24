using System.Windows.Forms;

// とりま、Windows用
// あとで他環境用を用意する
using MyShogi.View.Win2D;

using MyShogi.ViewModel;

namespace MyShogi.App
{
    /// <summary>
    /// このアプリケーション
    /// singletonで生成
    /// </summary>
    public class TheApp
    {
        /// <summary>
        /// ここが本アプリのエントリーポイント
        /// </summary>
        public void Run()
        {
            var mainDialog = new MainDialog();
            var mainDialogViewModel = new MainDialogViewModel();
            // mainDialog.bind(mainDialogViewModel);

            Application.Run(mainDialog);
        }
    }
}
