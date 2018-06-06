using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Test;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// メニューの更新部分だけ切り出したもの
    /// </summary>
    public partial class MainDialog
    {
        /// <summary>
        /// メニューのitemを動的に追加する。
        /// 商用版とフリーウェア版とでメニューが異なるのでここで動的に追加する必要がある。
        /// </summary>
        public void UpdateMenuItems()
        {
            var app = TheApp.app;
            var config = app.config;

            // Commercial Version GUI
            bool CV_GUI = config.CommercialVersion;
            if (CV_GUI)
                Text = "将棋神やねうら王";
            // 商用版とどこで差別化するのか考え中

            // -- メニューの追加。あとで考える。
            {
                var menu = new MenuStrip();

                //レイアウトロジックを停止する
                SuspendLayout();
                menu.SuspendLayout();

                // 前回設定されたメニューを除去する
                if (old_menu != null)
                    Controls.Remove(old_menu);

                var item_file = new ToolStripMenuItem();
                item_file.Text = "ファイル";
                menu.Items.Add(item_file);
                // あとで追加する。

                var item_playgame = new ToolStripMenuItem();
                item_playgame.Text = "対局";
                menu.Items.Add(item_playgame);
                // あとで追加する。

                var item_display = new ToolStripMenuItem();
                item_display.Text = "表示";
                menu.Items.Add(item_display);

                // -- 「表示」配下のメニュー
                {
                    { // -- 盤面反転
                        var item = new ToolStripMenuItem();
                        item.Text = "盤面反転"; // これは全体設定。個別設定もある。
                        item.Checked = config.BoardReverse;
                        item.Click += (sender, e) => { config.BoardReverse = !config.BoardReverse; };

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 段・筋の画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "筋・段の表示";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "非表示";
                        item1.Checked = config.BoardNumberImageVersion == 0;
                        item1.Click += (sender, e) => { config.BoardNumberImageVersion = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "標準";
                        item2.Checked = TheApp.app.config.BoardNumberImageVersion == 1;
                        item2.Click += (sender, e) => { config.BoardNumberImageVersion = 1; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "Chess式";
                        item3.Checked = TheApp.app.config.BoardNumberImageVersion == 2;
                        item3.Click += (sender, e) => { config.BoardNumberImageVersion = 2; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 盤画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "盤画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "白色";
                        item1.Checked = config.BoardImageVersion == 1;
                        item1.Click += (sender, e) => { config.BoardImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "黄色";
                        item2.Checked = config.BoardImageVersion == 2;
                        item2.Click += (sender, e) => { config.BoardImageVersion = 2; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 盤画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "畳画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "薄い";
                        item1.Checked = config.TatamiImageVersion == 1;
                        item1.Click += (sender, e) => { config.TatamiImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "濃い";
                        item2.Checked = config.TatamiImageVersion == 2;
                        item2.Click += (sender, e) => { config.TatamiImageVersion = 2; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 駒画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "駒画像";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "一文字駒";
                        item1.Checked = config.PieceImageVersion == 2;
                        item1.Click += (sender, e) => { config.PieceImageVersion = 2; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "二文字駒";
                        item2.Checked = TheApp.app.config.PieceImageVersion == 1;
                        item2.Click += (sender, e) => { config.PieceImageVersion = 1; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "英文字駒";
                        item3.Checked = TheApp.app.config.PieceImageVersion == 3;
                        item3.Click += (sender, e) => { config.PieceImageVersion = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    { // -- 成駒の画像の選択メニュー

                        var item = new ToolStripMenuItem();
                        item.Text = "成駒の色";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "黒";
                        item1.Checked = config.PromotePieceColorType == 0;
                        item1.Click += (sender, e) => { config.PromotePieceColorType = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "赤";
                        item2.Checked = TheApp.app.config.PromotePieceColorType == 1;
                        item2.Click += (sender, e) => { config.PromotePieceColorType = 1; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                    // -- 最終手のエフェクト
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "最終手の移動元";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし";
                        item0.Checked = config.LastMoveFromColorType == 0;
                        item0.Click += (sender, e) => { config.LastMoveFromColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "朱色";
                        item1.Checked = config.LastMoveFromColorType == 1;
                        item1.Click += (sender, e) => { config.LastMoveFromColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色";
                        item2.Checked = TheApp.app.config.LastMoveFromColorType == 2;
                        item2.Click += (sender, e) => { config.LastMoveFromColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色";
                        item3.Checked = TheApp.app.config.LastMoveFromColorType == 3;
                        item3.Click += (sender, e) => { config.LastMoveFromColorType = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "最終手の移動先";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし";
                        item0.Checked = config.LastMoveToColorType == 0;
                        item0.Click += (sender, e) => { config.LastMoveToColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "朱色";
                        item1.Checked = config.LastMoveToColorType == 1;
                        item1.Click += (sender, e) => { config.LastMoveToColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色";
                        item2.Checked = TheApp.app.config.LastMoveToColorType == 2;
                        item2.Click += (sender, e) => { config.LastMoveToColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色";
                        item3.Checked = TheApp.app.config.LastMoveToColorType == 3;
                        item3.Click += (sender, e) => { config.LastMoveToColorType = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "駒を掴んだ時の移動元";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし";
                        item0.Checked = config.PickedMoveFromColorType == 0;
                        item0.Click += (sender, e) => { config.PickedMoveFromColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "黄色";
                        item1.Checked = config.PickedMoveFromColorType == 1;
                        item1.Click += (sender, e) => { config.PickedMoveFromColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "青色";
                        item2.Checked = TheApp.app.config.PickedMoveFromColorType == 2;
                        item2.Click += (sender, e) => { config.PickedMoveFromColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "緑色";
                        item3.Checked = TheApp.app.config.PickedMoveFromColorType == 3;
                        item3.Click += (sender, e) => { config.PickedMoveFromColorType = 3; };
                        item.DropDownItems.Add(item3);

                        item_display.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "駒を掴んだ時の移動候補";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "エフェクトなし";
                        item0.Checked = config.PickedMoveToColorType == 0;
                        item0.Click += (sender, e) => { config.PickedMoveToColorType = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "移動できない升を少し暗くする";
                        item1.Checked = config.PickedMoveToColorType == 1;
                        item1.Click += (sender, e) => { config.PickedMoveToColorType = 1; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "移動できない升を暗くする";
                        item2.Checked = TheApp.app.config.PickedMoveToColorType == 2;
                        item2.Click += (sender, e) => { config.PickedMoveToColorType = 2; };
                        item.DropDownItems.Add(item2);

                        var item3 = new ToolStripMenuItem();
                        item3.Text = "移動できない升をかなり暗くする";
                        item3.Checked = TheApp.app.config.PickedMoveToColorType == 3;
                        item3.Click += (sender, e) => { config.PickedMoveToColorType = 3; };
                        item.DropDownItems.Add(item3);

                        var item4 = new ToolStripMenuItem();
                        item4.Text = "移動できる升を少し明るくする";
                        item4.Checked = TheApp.app.config.PickedMoveToColorType == 4;
                        item4.Click += (sender, e) => { config.PickedMoveToColorType = 4; };
                        item.DropDownItems.Add(item4);

                        var item5 = new ToolStripMenuItem();
                        item5.Text = "移動できる升を明るくする";
                        item5.Checked = TheApp.app.config.PickedMoveToColorType == 5;
                        item5.Click += (sender, e) => { config.PickedMoveToColorType = 5; };
                        item.DropDownItems.Add(item5);

                        item_display.DropDownItems.Add(item);
                    }

                    // 駒の移動方向
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "移動方角マーカー";

                        var item0 = new ToolStripMenuItem();
                        item0.Text = "なし";
                        item0.Checked = config.PieceAttackImageVersion == 0;
                        item0.Click += (sender, e) => { config.PieceAttackImageVersion = 0; };
                        item.DropDownItems.Add(item0);

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "あり";
                        item1.Checked = config.PieceAttackImageVersion == 1;
                        item1.Click += (sender, e) => { config.PieceAttackImageVersion = 1; };
                        item.DropDownItems.Add(item1);

                        item_display.DropDownItems.Add(item);
                    }

                }

                    // 「その他」
                    {
                    var item_others = new ToolStripMenuItem();
                    item_others.Text = "その他";
                    menu.Items.Add(item_others);

                    // aboutダイアログ

                    var item1 = new ToolStripMenuItem();
                    item1.Text = "about..";
                    item1.Click += (sender, e) =>
                    {
                        if (AboutDialog == null)
                            AboutDialog = new AboutYaneuraOu();

                        AboutDialog.ShowDialog();
                    };
                    item_others.DropDownItems.Add(item1);

                }

#if DEBUG
                {
                    var item_demo = new ToolStripMenuItem();
                    item_demo.Text = "デモ用";

                    var item1 = new ToolStripMenuItem();
                    item1.Text = "対局画面";
                    item1.Click += (sender, e) => { new GameSettingDialog().Show(); };
                    item_demo.DropDownItems.Add(item1);

                    menu.Items.Add(item_demo);
                }

                // デバッグ用にメニューにテストコードを実行する項目を追加する。
                {
                    var item_debug = new ToolStripMenuItem();
                    item_debug.Text = "デバッグ";

                    var item1 = new ToolStripMenuItem();
                    item1.Text = "DevTest1.Test1()";
                    item1.Click += (sender, e) => { DevTest1.Test1(); };
                    item_debug.DropDownItems.Add(item1);

                    var item2 = new ToolStripMenuItem();
                    item2.Text = "DevTest1.Test2()";
                    item2.Click += (sender, e) => { DevTest1.Test2(); };
                    item_debug.DropDownItems.Add(item2);

                    var item3 = new ToolStripMenuItem();
                    item3.Text = "DevTest1.Test3()";
                    item3.Click += (sender, e) => { DevTest1.Test3(); };
                    item_debug.DropDownItems.Add(item3);

                    var item4 = new ToolStripMenuItem();
                    item4.Text = "DevTest1.Test4()";
                    item4.Click += (sender, e) => { DevTest1.Test4(); };
                    item_debug.DropDownItems.Add(item4);

                    var item5 = new ToolStripMenuItem();
                    item5.Text = "DevTest2.Test1()";
                    item5.Click += (sender, e) => { DevTest2.Test1(); };
                    item_debug.DropDownItems.Add(item5);

                    menu.Items.Add(item_debug);
                }
#endif

                Controls.Add(menu);
                //フォームのメインメニューとする
                MainMenuStrip = menu;
                old_menu = menu;

                //レイアウトロジックを再開する
                menu.ResumeLayout(false);
                menu.PerformLayout();
                ResumeLayout(false);
                PerformLayout();
            }

            Invalidate();
        }

        private MenuStrip old_menu { get; set; } = null;
    }
}
