using System;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Test;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// メニューの更新部分だけ切り出したもの
    /// </summary>
    public partial class MainDialog
    {
        // -- メニューが生成しうるダイアログ

        /// <summary>
        /// 「やねうら王について」のダイアログ
        /// </summary>
        public Form aboutDialog;

        /// <summary>
        /// 「通常対局」の設定ダイアログ
        /// </summary>
        public Form gameSettingDialog;

        /// <summary>
        /// メニューのitemを動的に追加する。
        /// 商用版とフリーウェア版とでメニューが異なるのでここで動的に追加する必要がある。
        /// </summary>
        public void UpdateMenuItems(PropertyChangedEventArgs args = null)
        {
            // UIスレッド以外から呼び出された時は、UIスレッドから呼び直す。
            if (InvokeRequired)
            {
                Invoke(new Action( ()=> UpdateMenuItems(args)));
                return;
            }

            var app = TheApp.app;
            var config = app.config;

            // コンストラクタから呼び出された時は、まだ初期化されていない。
            var gameServer = ViewModel != null ? ViewModel.gameServer : null;

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

                // 対局中は、ファイルメニュー項目は丸ごと無効化
                item_file.Enabled = gameServer != null ? !gameServer.InTheGame : false;

                // -- 「ファイル」配下のメニュー
                {
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜を開く";
                        item.Click += (sender, e) =>
                        {
                            var fd = new OpenFileDialog();

                            //[ファイルの種類]に表示される選択肢を指定する
                            //指定しないとすべてのファイルが表示される
                            fd.Filter = string.Join("|", new string[]
                            {
                                "棋譜ファイル|*.kif;*.kifu;*.ki2;*.kif2;*.ki2u;*.kif2u;*.csa;*.psn;*.psn2;*.sfen;*.json;*.jkf;*.txt",
                                "KIF形式|*.kif;*.kifu",
                                "KIF2形式|*.ki2;*.kif2;*.ki2u;*.kif2u",
                                "CSA形式|*.csa",
                                "PSN形式|*.psn",
                                "PSN2形式|*.psn2",
                                "SFEN形式|*.sfen",
                                "すべてのファイル|*.*",
                            });
                            fd.FilterIndex = 1;
                            fd.Title = "開く棋譜ファイルを選択してください";
                            //ダイアログを表示する
                            if (fd.ShowDialog() == DialogResult.OK)
                            {
                                var filename = fd.FileName;
                                try
                                {
                                    var kifu_text = FileIO.ReadText(filename);
                                    gameServer.KifuReadCommand(kifu_text);
                                }
                                catch
                                {
                                    TheApp.app.MessageShow("ファイル読み込みエラー");
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜の上書き保存";
                        item.Click += (sender, e) =>
                        {

                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜の名前をつけて保存";
                        item.Click += (sender, e) =>
                        {
                            var fd = new SaveFileDialog();

                            //[ファイルの種類]に表示される選択肢を指定する
                            //指定しないとすべてのファイルが表示される
                            fd.Filter = "KIF形式(*.KIF)|*.KIF|KIF2形式(*.KI2)|*.KI2|CSA形式(*.CSA)|*.CSA"
                                + "|PSN形式(*.PSN)|*.PSN|PSN2形式(*.PSN2)|*.PSN2"
                                + "|SFEN形式(*.SFEN)|*.SFEN|すべてのファイル(*.*)|*.*";
                            fd.FilterIndex = 1;
                            fd.Title = "保存する棋譜ファイルを選択してください";
                            //ダイアログを表示する
                            if (fd.ShowDialog() == DialogResult.OK)
                            {
                                var filename = fd.FileName;
                                try
                                {
                                    KifuFileType kifuType;
                                    switch(fd.FilterIndex)
                                    {
                                        case 1: kifuType = KifuFileType.KIF; break;
                                        case 2: kifuType = KifuFileType.KI2; break;
                                        case 3: kifuType = KifuFileType.CSA; break;
                                        case 4: kifuType = KifuFileType.PSN; break;
                                        case 5: kifuType = KifuFileType.PSN2; break;
                                        case 6: kifuType = KifuFileType.SFEN; break;

                                            // ファイル名から自動判別すべき
                                        default:
                                            kifuType = KifuFileTypeExtensions.StringToKifuFileType(filename);
                                            if (kifuType == KifuFileType.UNKNOWN)
                                                kifuType = KifuFileType.KIF; // わからんからKIF形式でいいや。
                                            break;
                                    }

                                    gameServer.KifuWriteCommand(filename, kifuType);
                                }
                                catch
                                {
                                    TheApp.app.MessageShow("ファイル書き出しエラー");
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "局面の保存";
                        item.Click += (sender, e) =>
                        {

                        };
                        item_file.DropDownItems.Add(item);
                    }
                }



                var item_playgame = new ToolStripMenuItem();
                item_playgame.Text = "対局";
                menu.Items.Add(item_playgame);

                // -- 「対局」配下のメニュー
                {
                    { // -- 通常対局
                        var item = new ToolStripMenuItem();
                        item.Text = "通常対局";
                        item.Click += (sender, e) =>
                        {

                               // ShowDialog()はリソースが開放されないので、都度生成して、Form.Show()で表示する。
                               if (gameSettingDialog != null)
                                gameSettingDialog.Dispose();

                            gameSettingDialog = new GameSettingDialog(this);
                            gameSettingDialog.Show();

                               // 閉じるときにせめて前回設定が選ばれていて欲しいが..
                               // あとで前回設定を復元するコードを書く。
                               // 前回設定、GlobalSettingに持たせるべきのような気がする。
                           };

                        item_playgame.DropDownItems.Add(item);
                    }

                }

                var item_display = new ToolStripMenuItem();
                item_display.Text = "表示";
                menu.Items.Add(item_display);

                // -- 「表示」配下のメニュー
                {
                    { // -- 盤面反転
                        var item = new ToolStripMenuItem();
                        item.Text = "盤面反転"; // これは全体設定。個別設定もある。
                        item.Checked = gameServer != null ? gameServer.BoardReverse : false;
                        item.Click += (sender, e) => { ViewModel.gameServer.BoardReverse = !ViewModel.gameServer.BoardReverse; };

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

#if false
                        var item4 = new ToolStripMenuItem();
                        item4.Text = "駒の影のみ";
                        item4.Checked = TheApp.app.config.LastMoveFromColorType == 4;
                        item4.Click += (sender, e) => { config.LastMoveFromColorType = 4; };
                        item.DropDownItems.Add(item4);
#endif

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

#if false
                        var item6 = new ToolStripMenuItem();
                        item6.Text = "駒の影のみ";
                        item6.Checked = TheApp.app.config.PickedMoveToColorType == 6;
                        item6.Click += (sender, e) => { config.PickedMoveToColorType = 6; };
                        item.DropDownItems.Add(item6);
#endif

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

                    { // -- 手番プレートの表示

                        var item = new ToolStripMenuItem();
                        item.Text = "手番表示";

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "なし";
                        item1.Checked = config.TurnDisplay == 0;
                        item1.Click += (sender, e) => { config.TurnDisplay = 0; };
                        item.DropDownItems.Add(item1);

                        var item2 = new ToolStripMenuItem();
                        item2.Text = "あり";
                        item2.Checked = TheApp.app.config.TurnDisplay == 1;
                        item2.Click += (sender, e) => { config.TurnDisplay = 1; };
                        item.DropDownItems.Add(item2);

                        item_display.DropDownItems.Add(item);
                    }

                }

                // 「音声」
                {
                    var item_sounds = new ToolStripMenuItem();
                    item_sounds.Text = "音声";
                    menu.Items.Add(item_sounds);

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "対局時の駒音";
                        item1.Checked = TheApp.app.config.PieceSoundInTheGame == 1;
                        item1.Click += (sender, e) => { TheApp.app.config.PieceSoundInTheGame ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

#if false
                        // あまりいい効果音作れなかったのでコメントアウトしとく。
                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "王手などの駒音を衝撃音に";
                        item1.Checked = TheApp.app.config.CrashPieceSoundInTheGame == 1;
                        item1.Click += (sender, e) => { TheApp.app.config.CrashPieceSoundInTheGame ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }
#endif
                    
                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "棋譜読み上げ";
                        item1.Checked = TheApp.app.config.KifuReadOut == 1;
                        item1.Enabled = TheApp.app.config.CommercialVersion; // 商用版のみ選択可
                        item1.Click += (sender, e) => { TheApp.app.config.KifuReadOut ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
                    }

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "「先手」「後手」を毎回読み上げる";
                        item1.Checked = TheApp.app.config.ReadOutSenteGoteEverytime == 1;
                        item1.Enabled = TheApp.app.config.CommercialVersion; // 商用版のみ選択可
                        item1.Click += (sender, e) => { TheApp.app.config.ReadOutSenteGoteEverytime ^= 1 /* 0,1反転 */; };
                        item_sounds.DropDownItems.Add(item1);
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
                        if (aboutDialog != null)
                            aboutDialog.Dispose();

                        aboutDialog = new AboutYaneuraOu();
                        aboutDialog.Show();
                    };
                    item_others.DropDownItems.Add(item1);

                }

#if DEBUG

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
