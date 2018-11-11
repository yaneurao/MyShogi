using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Tool;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.LocalServer;
using MyShogi.View.Win2D.Setting;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局盤面などがあるメインウィンドゥ
    ///
    /// メニュー関連だけをこのファイルに切り離してある。
    /// </summary>
    public partial class MainDialog : Form
    {
        /// <summary>
        /// [UI thread] : メニューのitemを動的に追加する。
        /// 商用版とフリーウェア版とでメニューが異なるのでここで動的に追加する必要がある。
        /// </summary>
        public void UpdateMenuItems(PropertyChangedEventArgs args = null)
        {
            // 頑張れば高速化出来るが、対局中はこのメソッド呼び出されていないし、
            // ToolStripも、CPU×CPUの対局中は更新は発生していないし、
            // CPU×人間のときは多少遅くても誤差だし、まあいいか…。

            var config = TheApp.app.Config;
            var shortcut = TheApp.app.KeyShortcut;
            shortcut.InitEvent1(); // このdelegateにShortcutキーのハンドラを登録していく。

            // 使ったファイル名をメインウインドウのText部に描画する必要がある。
            UpdateCaption();

            // -- メニューの追加。
            {
                // MenuStripだと非アクティブ状態からのクリックで反応しないのでMenuStripExを使う。
                var menu = new MenuStripEx();

                // -- LocalGameServerの各フラグ。
                // ただし、初期化時にgameServer == nullで呼び出されることがあるのでnull checkが必要。

                // LocalGameServer.GameModeは値がいま書き換わっている可能性があるので、イベントを除かしてしまう可能性がある。
                // ゆえに、引数で渡ってきたargs.value (GameModeEnum)を用いる必要があるが、しかし、args.valueが他の型である可能性もある。(BoardReverseなどを渡すとき)
                // このため、args.valueがGameModeEnumなら、これを用いて、さもなくば仕方ないので前回渡されたものをそのまま用いる。
                // (LocalGameServerの値はメニューには直接使わない)
                var gameMode =
                    (args != null && args.value != null && args.value is GameModeEnum) ? (GameModeEnum)args.value :
                    gameServer == null ? GameModeEnum.NotInit :
                    lastGameMode;
                lastGameMode = gameMode;

                // 検討モード(通常エンジン)
                var consideration = gameMode == GameModeEnum.ConsiderationWithEngine;
                // 検討モード(詰将棋用)
                var mate_consideration = gameMode == GameModeEnum.ConsiderationWithMateEngine;
                // 対局中
                var inTheGame = gameMode == GameModeEnum.InTheGame;
                // 盤面編集中
                var inTheBoardEdit = gameMode == GameModeEnum.InTheBoardEdit;
                // 盤面反転
                var boardReverse = gameServer == null ? false : gameServer.BoardReverse;

                var item_file = new ToolStripMenuItem();
                item_file.Text = "ファイル(&F)";
                menu.Items.Add(item_file);

                // 対局中などは、ファイルメニュー項目は丸ごと無効化
                item_file.Enabled = gameMode == GameModeEnum.ConsiderationWithoutEngine;

                // -- 「ファイル」配下のメニュー
                {
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜を開く(&O)";
                        item.ShortcutKeys = Keys.Control | Keys.O;
                        // サブウインドウでのショートカットキーの処理
                        shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.O) { item.PerformClick(); e.Handled = true; } });
                        item.Click += (sender, e) =>
                        {
                            using (var fd = new OpenFileDialog())
                            {
                                    // [ファイルの種類]に表示される選択肢を指定する
                                    // 指定しないとすべてのファイルが表示される
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

                                    // ダイアログを表示する
                                    if (fd.ShowDialog() == DialogResult.OK)
                                    ReadKifuFile(fd.FileName);
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜の上書き保存(&S)";
                        item.ShortcutKeys = Keys.Control | Keys.S;
                        // サブウインドウでのショートカットキーの処理
                        shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.S) { item.PerformClick(); e.Handled = true; } });
                        item.Enabled = ViewModel.LastFileName != null; // 棋譜を読み込んだ時などにしか有効ではない。
                        item.Click += (sender, e) =>
                        {
                            var path = ViewModel.LastFileName;

                                // 「開く」もしくは「名前をつけて保存無したファイルに上書きする。
                                // 「局面の保存」は棋譜ではないのでこれは無視する。
                                // ファイル形式は、拡張子から自動判別する。
                                gameServer.KifuWriteCommand(path, KifuFileTypeExtensions.StringToKifuFileType(path));

                                //UseKifuFile(path);
                                // 上書き保存の直前にこのファイルを開いていて、そのときにMRUFに記録されているはず。
                            };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "棋譜に名前をつけて保存(&N)";
                        item.ShortcutKeys = Keys.Control | Keys.S | Keys.Shift;
                        shortcut.AddEvent1( e => { if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.S) { item.PerformClick(); e.Handled = true; } });
                        item.Click += (sender, e) =>
                        {
                            using (var fd = new SaveFileDialog())
                            {

                                    // [ファイルの種類]に表示される選択肢を指定する
                                    // 指定しないとすべてのファイルが表示される
                                    fd.Filter = "KIF形式(*.KIF)|*.KIF|KIF2形式(*.KI2)|*.KI2|CSA形式(*.CSA)|*.CSA"
                                        + "|PSN形式(*.PSN)|*.PSN|PSN2形式(*.PSN2)|*.PSN2"
                                        + "|SFEN形式(*.SFEN)|*.SFEN|すべてのファイル(*.*)|*.*";
                                fd.FilterIndex = 1;
                                fd.Title = "棋譜を保存するファイル形式を選択してください";
                                    // デフォルトでは、先手名 + 後手名 + YYYYMMDDhhmmss.kif
                                    // 柿木やkifu for Windowsがこの形式らしい。
                                    var default_filename = $"{gameServer.DefaultKifuFileName()}.KIF";
                                fd.FileName = default_filename;
                                    // これでescapeされているし、ダイアログが使えないファイル名は返さないから、以降のescapeは不要。

                                    // ダイアログを表示する
                                    if (fd.ShowDialog() == DialogResult.OK)
                                {
                                    var path = fd.FileName;
                                    try
                                    {
                                        KifuFileType kifuType;
                                        switch (fd.FilterIndex)
                                        {
                                            case 1: kifuType = KifuFileType.KIF; break;
                                            case 2: kifuType = KifuFileType.KI2; break;
                                            case 3: kifuType = KifuFileType.CSA; break;
                                            case 4: kifuType = KifuFileType.PSN; break;
                                            case 5: kifuType = KifuFileType.PSN2; break;
                                            case 6: kifuType = KifuFileType.SFEN; break;

                                                // ファイル名から自動判別すべき
                                                default:
                                                kifuType = KifuFileTypeExtensions.StringToKifuFileType(path);
                                                if (kifuType == KifuFileType.UNKNOWN)
                                                    kifuType = KifuFileType.KIF; // わからんからKIF形式でいいや。
                                                    break;
                                        }

                                        gameServer.KifuWriteCommand(path, kifuType);
                                        ViewModel.LastFileName = path; // 最後に保存したファイルを記録しておく。
                                            UseKifuFile(path);
                                    }
                                    catch
                                    {
                                        TheApp.app.MessageShow("ファイル書き出しエラー", MessageShowType.Error);
                                    }
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "局面の保存(&I)"; // Pは印刷(Print)で使いたいため、positionの"I"をショートカットキーにする。
                        item.Click += (sender, e) =>
                        {
                            using (var fd = new SaveFileDialog())
                            {

                                    // [ファイルの種類]に表示される選択肢を指定する
                                    // 指定しないとすべてのファイルが表示される
                                    fd.Filter = "KIF形式(*.KIF)|*.KIF|KIF2形式(*.KI2)|*.KI2|CSA形式(*.CSA)|*.CSA"
                                        + "|PSN形式(*.PSN)|*.PSN|PSN2形式(*.PSN2)|*.PSN2"
                                        + "|SFEN形式(*.SFEN)|*.SFEN|SVG形式(*.SVG)|*.SVG|すべてのファイル(*.*)|*.*";
                                fd.FilterIndex = 1;
                                fd.Title = "局面を保存するファイル形式を選択してください";

                                    // ダイアログを表示する
                                    if (fd.ShowDialog() == DialogResult.OK)
                                {
                                    var path = fd.FileName;
                                    try
                                    {
                                        KifuFileType kifuType;
                                        switch (fd.FilterIndex)
                                        {
                                            case 1: kifuType = KifuFileType.KIF; break;
                                            case 2: kifuType = KifuFileType.KI2; break;
                                            case 3: kifuType = KifuFileType.CSA; break;
                                            case 4: kifuType = KifuFileType.PSN; break;
                                            case 5: kifuType = KifuFileType.PSN2; break;
                                            case 6: kifuType = KifuFileType.SFEN; break;
                                            case 7: kifuType = KifuFileType.SVG; break;

                                                // ファイル名から自動判別すべき
                                                default:
                                                kifuType = KifuFileTypeExtensions.StringToKifuFileType(path);
                                                if (kifuType == KifuFileType.UNKNOWN)
                                                    kifuType = KifuFileType.KIF; // わからんからKIF形式でいいや。
                                                    break;
                                        }

                                        gameServer.PositionWriteCommand(path, kifuType);

                                            // このファイルを用いたのでMRUFに記録しておく。
                                            UseKifuFile(path);
                                    }
                                    catch
                                    {
                                        TheApp.app.MessageShow("ファイル書き出しエラー", MessageShowType.Error);
                                    }
                                }
                            }
                        };
                        item_file.DropDownItems.Add(item);
                    }

                    item_file.DropDownItems.Add(new ToolStripSeparator());

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "クリップボードに棋譜/局面をコピー(&C)";

                        var itemk1 = new ToolStripMenuItem();
                        itemk1.Text = "棋譜KIF形式(&1)";
                        itemk1.ShortcutKeys = Keys.Control | Keys.C;
                        shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C) { item.PerformClick(); e.Handled = true; } });

                        // このショートカットキーを設定すると対局中などにも書き出せてしまうが、書き出しはまあ問題ない。
                        itemk1.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.KIF); };
                        item.DropDownItems.Add(itemk1);

                        var itemk2 = new ToolStripMenuItem();
                        itemk2.Text = "棋譜KI2形式(&2)";
                        itemk2.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.KI2); };
                        item.DropDownItems.Add(itemk2);

                        var itemk3 = new ToolStripMenuItem();
                        itemk3.Text = "棋譜CSA形式(&3)";
                        itemk3.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.CSA); };
                        item.DropDownItems.Add(itemk3);

                        var itemk4 = new ToolStripMenuItem();
                        itemk4.Text = "棋譜SFEN形式(&4)";
                        itemk4.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.SFEN); };
                        item.DropDownItems.Add(itemk4);

                        var itemk5 = new ToolStripMenuItem();
                        itemk5.Text = "棋譜PSN形式(&5)";
                        itemk5.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.PSN); };
                        item.DropDownItems.Add(itemk5);

                        var itemk6 = new ToolStripMenuItem();
                        itemk6.Text = "棋譜PSN2形式(&6)";
                        itemk6.Click += (sender, e) => { gameServer.KifuWriteClipboardCommand(KifuFileType.PSN2); };
                        item.DropDownItems.Add(itemk6);

                        item.DropDownItems.Add(new ToolStripSeparator());

                        var itemp1 = new ToolStripMenuItem();
                        itemp1.Text = "局面KIF(BOD)形式(&A)";
                        itemp1.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.KI2); };
                        item.DropDownItems.Add(itemp1);

                        var itemp2 = new ToolStripMenuItem();
                        itemp2.Text = "局面CSA形式(&B)";
                        itemp2.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.CSA); };
                        item.DropDownItems.Add(itemp2);

                        var itemp3 = new ToolStripMenuItem();
                        itemp3.Text = "局面SFEN形式(&C)";
                        itemp3.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.SFEN); };
                        item.DropDownItems.Add(itemp3);

                        var itemp4 = new ToolStripMenuItem();
                        itemp4.Text = "局面PSN形式(&D)";
                        itemp4.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.PSN); };
                        item.DropDownItems.Add(itemp4);

                        var itemp5 = new ToolStripMenuItem();
                        itemp5.Text = "局面PSN2形式(&E)";
                        itemp5.Click += (sender, e) => { gameServer.PositionWriteClipboardCommand(KifuFileType.PSN2); };
                        item.DropDownItems.Add(itemp5);

                        item_file.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "クリップボードから棋譜/局面を貼り付け(&P)";
                        // このショートカットキーを設定すると対局中などにも貼り付けが出来てしまうが、
                        // GameModeを見て、対局中などには処理しないようにしてある。
                        item.ShortcutKeys = Keys.Control | Keys.V;
                        shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V) { item.PerformClick(); e.Handled = true; } });
                        item.Click += (sender, e) => { CopyFromClipboard(); };
                        item_file.DropDownItems.Add(item);
                    }

                    item_file.DropDownItems.Add(new ToolStripSeparator());

                    // -- 「棋譜編集」

                    var kifu_edit = new ToolStripMenuItem();
                    kifu_edit.Text = "棋譜編集(&K)"; // Kifu edit
                    kifu_edit.Enabled = !inTheGame;
                    item_file.DropDownItems.Add(kifu_edit);

                    // -- 「棋譜編集」配下のメニュー
                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "本譜以外の分岐をクリアする(&C)"; // Clear
                        item.Click += (sender, e) =>
                        {
                            if (TheApp.app.MessageShow("この操作により現在の棋譜上の本譜以外の分岐は削除されます。",
                                MessageShowType.WarningOkCancel) == DialogResult.OK)
                            {
                                gameServer.ClearSubKifuTreeCommand();
                            }
                        };
                        kifu_edit.DropDownItems.Add(item);
                    }

                    item_file.DropDownItems.Add(new ToolStripSeparator());

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "終了(&X)";
                        item.Click += (sender, e) => { TheApp.app.ApplicationExit(); };
                        item_file.DropDownItems.Add(item);
                    }

                    // MRUF : 最近使ったファイル

                    {
                        var mruf = TheApp.app.Config.MRUF;
                        ToolStripMenuItem sub_item = null;
                        for (int i = 0; i < mruf.Files.Count; ++i)
                        {
                            var display_name = mruf.GetDisplayFileName(i);
                            if (display_name == null)
                                break;

                            if (i == 0)
                                item_file.DropDownItems.Add(new ToolStripSeparator());
                            else if (i == 3)
                            {
                                sub_item = new ToolStripMenuItem();
                                sub_item.Text = "ファイルヒストリーのつづき(&R)";
                                item_file.DropDownItems.Add(sub_item);
                            }

                            {
                                var item = new ToolStripMenuItem();
                                item.Text = display_name;
                                var kifu_file_path = mruf.Files[i];
                                item.Click += (sender, e) => { ReadKifuFile(kifu_file_path); };
                                if (i < 3)
                                    item_file.DropDownItems.Add(item);
                                else
                                    sub_item.DropDownItems.Add(item);
                            }

                            if (i == mruf.Files.Count - 1) // 最後の要素
                            {
                                var item = new ToolStripMenuItem();
                                item.Text = "ファイルヒストリーのクリア(&T)";
                                item.Click += (sender, e) =>
                                {
                                    if (TheApp.app.MessageShow("ファイルヒストリーをクリアしますか？「OK」を押すとクリアされます。", MessageShowType.ConfirmationOkCancel) == DialogResult.OK)
                                    {
                                        mruf.Clear();
                                        UpdateMenuItems();
                                    }
                                };
                                item_file.DropDownItems.Add(item);
                            }
                        }
                    }
                }

                var item_playgame = new ToolStripMenuItem();
                item_playgame.Text = "対局(&P)"; // PlayGame
                item_playgame.Enabled = gameServer != null && !gameServer.InTheGame; // 対局中はこのメニューを無効化
                menu.Items.Add(item_playgame);

                // -- 「対局」配下のメニュー
                {
                    { // -- 通常対局
                        var item = new ToolStripMenuItem();
                        item.Text = "通常対局(&N)"; // NormalGame
                        item.ShortcutKeys = Keys.Control | Keys.N; // NewGameのN
                        shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.N) { item.PerformClick(); e.Handled = true; } });
                        item.Click += (sender, e) =>
                        {
                            using (var dialog = new GameSettingDialog(this))
                            {
                                FormLocationUtility.CenteringToThisForm(dialog, this);
                                dialog.ShowDialog(this); // Modal Dialogにしておく。
                            }
                        };

                        item_playgame.DropDownItems.Add(item);
                    }

                    item_playgame.DropDownItems.Add(new ToolStripSeparator());

                    { // -- 検討モード

                        var item = new ToolStripMenuItem();
                        item.Text = consideration ? "検討モードを終了する(&C)" : "検討エンジン設定(&C)"; // ConsiderationMode

                        // toolStripのボタンのテキストを検討モードであるかどうかにより変更する。
                        toolStripButton5.Text = consideration ? "終" : "検";
                        toolStripButton5.ToolTipText = consideration ? "検討モードを終了します。" : "検討モードに入ります。";
                        toolStripButton5.Enabled = !inTheGame;
                        item.Click += (sender, e) =>
                        {
                            if (consideration)
                                ToggleConsideration(); // 検討モードを終了させる
                                else
                                ShowConsiderationEngineSettingDialog(); // 検討エンジンの選択画面に
                            };

                        item_playgame.DropDownItems.Add(item);
                    }


                    // 「解」ボタン : 棋譜解析
                    //toolStripButton6.Enabled = !inTheGame;

                    { // -- 検討モード

                        var item = new ToolStripMenuItem();
                        item.Text = mate_consideration ? "詰検討モードを終了する(&M)" : "詰検討エンジン設定(&M)"; // MateMode

                        // toolStripのボタンのテキストを検討モードであるかどうかにより変更する。
                        toolStripButton7.Text = mate_consideration ? "終" : "詰";
                        toolStripButton7.ToolTipText = mate_consideration ? "詰検討モードを終了します。" : "詰検討モードに入ります。";
                        // 「詰」ボタン : 詰将棋ボタン
                        toolStripButton7.Enabled = !inTheGame;
                        item.Click += (sender, e) =>
                        {
                            if (mate_consideration)
                                ToggleMateConsideration();
                            else
                                ShowMateEngineSettingDialog(); // 詰検討エンジンの選択画面に

                            };

                        item_playgame.DropDownItems.Add(item);
                    }

                    item_playgame.DropDownItems.Add(new ToolStripSeparator());

                    { // -- 対局結果一覧

                        var item_ = new ToolStripMenuItem();
                        item_.Text = "対局結果一覧(&R)"; // game Result
                        item_.Click += (sender, e) =>
                        {
                            using (var dialog = new GameResultDialog())
                            {
                                dialog.ViewModel.AddPropertyChangedHandler("KifuClicked", (args_) =>
                                {
                                    var filename = (string)args_.value;
                                    // このファイルを読み込む。
                                    var path = Path.Combine(TheApp.app.Config.GameResultSetting.KifuSaveFolder, filename);
                                    try
                                    {
                                        ReadKifuFile(path);
                                    }
                                    catch
                                    {
                                        TheApp.app.MessageShow("棋譜ファイルが読み込めませんでした。", MessageShowType.Error);
                                    }
                                });

                                FormLocationUtility.CenteringToThisForm(dialog, this);
                                dialog.ShowDialog(this);
                            }
                        };

                        item_playgame.DropDownItems.Add(item_);
                    }


                    { // -- 対局結果の保存設定

                        var item_ = new ToolStripMenuItem();
                        item_.Text = "対局結果の保存設定(&S)"; // アルファベット的にRの次
                        item_.Click += (sender, e) =>
                        {
                            using (var dialog = new GameResultWindowSettingDialog())
                            {
                                FormLocationUtility.CenteringToThisForm(dialog, this);
                                dialog.ShowDialog(this);
                            }
                        };

                        item_playgame.DropDownItems.Add(item_);
                    }

                }

                // 「設定」
                var item_settings = new ToolStripMenuItem();
                item_settings.Text = "設定(&S)"; // Settings
                menu.Items.Add(item_settings);
                {
                    var item = new ToolStripMenuItem();
                    item.Text = "音声設定 (&S)"; // Sound setting
                    item.Enabled = config.CommercialVersion != 0; // 商用版のみ選択可
                    item.Click += (sender, e) =>
                    {
                        using (var dialog = new SoundSettingDialog())
                        {
                            FormLocationUtility.CenteringToThisForm(dialog, this);
                            dialog.ShowDialog(this);
                        }
                    };
                    item_settings.DropDownItems.Add(item);
                }

                {
                    var item = new ToolStripMenuItem();
                    item.Text = "表示設定 (&D)"; // Display setting
                    item.Click += (sender, e) =>
                    {
                        using (var dialog = new DisplaySettingDialog())
                        {
                            FormLocationUtility.CenteringToThisForm(dialog, this);
                            dialog.ShowDialog(this);
                        }
                    };
                    item_settings.DropDownItems.Add(item);
                }

                {
                    var item = new ToolStripMenuItem();
                    item.Text = "操作設定 (&O)"; // Operation setting
                    item.Click += (sender, e) =>
                    {
                        using (var dialog = new OperationSettingDialog())
                        {
                            FormLocationUtility.CenteringToThisForm(dialog, this);
                            dialog.ShowDialog(this);
                        }
                    };
                    item_settings.DropDownItems.Add(item);
                }

                {
                    var item = new ToolStripMenuItem();
                    item.Text = "エンジン補助設定 (&E)"; // Engine Subsetting
                    item.Click += (sender, e) =>
                    {
                        using (var dialog = new EngineSubSettingDialog())
                        {
                            FormLocationUtility.CenteringToThisForm(dialog, this);
                            dialog.ShowDialog(this);
                        }
                    };
                    item_settings.DropDownItems.Add(item);
                }

                item_settings.DropDownItems.Add(new ToolStripSeparator());

                // -- 設定の初期化
                {
                    var item_init = new ToolStripMenuItem();
                    item_init.Text = "設定の初期化(&I)";
                    item_settings.DropDownItems.Add(item_init);

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "各エンジン設定の初期化(&E)";
                        item.Click += (sender, e) =>
                        {
                            if (TheApp.app.MessageShow("すべてのエンジン設定を初期化しますか？「OK」を押すと初期化され、次回起動時に反映されます。", MessageShowType.ConfirmationOkCancel) == DialogResult.OK)
                            {
                                TheApp.app.DeleteEngineOption = true;
                            }
                        };
                        item_init.DropDownItems.Add(item);
                    }

                    {
                        var item = new ToolStripMenuItem();
                        item.Text = "各表示設定などの初期化(&D)";
                        item.Click += (sender, e) =>
                        {
                            if (TheApp.app.MessageShow("すべての表示設定・音声設定を初期化しますか？「OK」を押すと初期化され、次回起動時に反映されます。", MessageShowType.ConfirmationOkCancel) == DialogResult.OK)
                            {
                                TheApp.app.DeleteGlobalOption = true;
                            }
                        };
                        item_init.DropDownItems.Add(item);
                    }
                }

                var item_boardedit = new ToolStripMenuItem();
                item_boardedit.Text = "盤面編集(&E)"; // board Edit
                item_boardedit.Enabled = !inTheGame;
                menu.Items.Add(item_boardedit);

                // 盤面編集の追加
                {
                    {   // -- 盤面編集の開始
                        var item = new ToolStripMenuItem();
                        item.Text = inTheBoardEdit ? "盤面編集の終了(&B)" : "盤面編集の開始(&B)"; // Board edit
                        item.ShortcutKeys = Keys.Control | Keys.E; // boardEdit
                        shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.E) { item.PerformClick(); e.Handled = true; } });
                        item.Click += (sender, e) =>
                        {
                            gameServer.ChangeGameModeCommand(
                                inTheBoardEdit ?
                                    GameModeEnum.ConsiderationWithoutEngine :
                                    GameModeEnum.InTheBoardEdit
                            );
                        };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 手番の変更
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "手番の変更(&T)"; // Turn change
                        item.Click += (sender, e) =>
                        {
                            var raw_pos = gameServer.Position.CreateRawPosition();
                            raw_pos.sideToMove = raw_pos.sideToMove.Not();
                            var sfen = Position.SfenFromRawPosition(raw_pos);
                            gameScreenControl1.SetSfenCommand(sfen);
                        };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 平手の初期局面
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "平手の初期局面配置(&N)"; // No handicaped
                        item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.NoHandicap.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 駒落ちの局面
                        var item_handicap = new ToolStripMenuItem();
                        item_handicap.Enabled = inTheBoardEdit;
                        item_handicap.Text = "駒落ち初期局面配置(&H)"; // Handicaped
                        item_boardedit.DropDownItems.Add(item_handicap);

                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "香落ち(&1)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "右香落ち(&2)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapRightKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "角落ち(&3)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapKaku.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "飛車落ち(&4)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapHisya.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "飛香落ち(&5)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapHisyaKyo.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "二枚落ち(&6)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap2.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "三枚落ち(&7)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap3.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "四枚落ち(&8)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap4.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "五枚落ち(&9)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap5.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "左五枚落ち(&A)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapLeft5.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "六枚落ち(&B)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap6.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "八枚落ち(&C)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap8.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "十枚落ち(&D)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Handicap10.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }
                        {
                            var item = new ToolStripMenuItem();
                            item.Enabled = inTheBoardEdit;
                            item.Text = "歩三枚(&E)";
                            item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.HandicapPawn3.ToSfen()); };
                            item_handicap.DropDownItems.Add(item);
                        }

                    }

                    {   // -- 詰将棋用の配置(駒箱に)
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "詰将棋用に配置(&M)"; // Mate
                        item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Mate1.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {   // -- 双玉詰将棋用の局面
                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "双玉詰将棋用に配置(&D)"; // Dual king mate
                        item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Mate2.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }

                    {
                        // - 双玉で玉以外すべて駒箱に

                        var item = new ToolStripMenuItem();
                        item.Enabled = inTheBoardEdit;
                        item.Text = "双玉で玉以外すべて駒箱に配置(&U)"; // dUal king
                        item.Click += (sender, e) => { gameScreenControl1.SetSfenCommand(BoardType.Mate3.ToSfen()); };
                        item_boardedit.DropDownItems.Add(item);
                    }
                }

                // -- 「ウインドウ」

                var item_window = new ToolStripMenuItem();
                item_window.Text = "ウインドウ(&W)"; // Window
                menu.Items.Add(item_window);

                // -- 「ウインドウ」配下のメニュー
                {

                    { // -- 棋譜ウィンドウ

                        var item_ = new ToolStripMenuItem();
                        item_.Text = "棋譜ウィンドウ(&K)"; // Kifu window

                        item_window.DropDownItems.Add(item_);

                        var dock = config.KifuWindowDockManager;

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = dock.Visible ? "非表示(&V)" : "再表示(&V)"; // visible // 
                            item.ShortcutKeys = Keys.Control | Keys.K; // KifuWindow
                            shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.K) { item.PerformClick(); e.Handled = true; } });
                            item.Click += (sender, e) => { dock.Visible ^= true; dock.RaisePropertyChanged("DockState", dock.DockState); };
                            item_.DropDownItems.Add(item);
                        }


                        { // フローティングの状態
                            var item = new ToolStripMenuItem();
                            item.Text = "表示位置(&F)"; // Floating window mode
                            item_.DropDownItems.Add(item);

                            {

                                var item1 = new ToolStripMenuItem();
                                item1.Text = "メインウインドウに埋め込む(&0)(EmbeddedMode)";
                                item1.Checked = dock.DockState == DockState.InTheMainWindow;
                                item1.Click += (sender, e) => { dock.DockState = DockState.InTheMainWindow; };
                                item.DropDownItems.Add(item1);

                                var item2 = new ToolStripMenuItem();
                                item2.Text = "メインウインドウから浮かせ、相対位置を常に保つ(&1)(FollowMode)";
                                item2.Checked = dock.DockState == DockState.FollowToMainWindow;
                                item2.Click += (sender, e) => { dock.DockState = DockState.FollowToMainWindow; };
                                item.DropDownItems.Add(item2);

                                var item3a = new ToolStripMenuItem();
                                item3a.Text = "メインウインドウから浮かせ、メインウインドウの上側に配置する(&2)(DockMode)";
                                item3a.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Top;
                                item3a.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Top); };
                                item.DropDownItems.Add(item3a);

                                var item3b = new ToolStripMenuItem();
                                item3b.Text = "メインウインドウから浮かせ、メインウインドウの左側に配置する(&3)(DockMode)";
                                item3b.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Left;
                                item3b.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Left); };
                                item.DropDownItems.Add(item3b);

                                var item3c = new ToolStripMenuItem();
                                item3c.Text = "メインウインドウから浮かせ、メインウインドウの右側に配置する(&4)(DockMode)";
                                item3c.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Right;
                                item3c.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Right); };
                                item.DropDownItems.Add(item3c);

                                var item3d = new ToolStripMenuItem();
                                item3d.Text = "メインウインドウから浮かせ、メインウインドウの下側に配置する(&5)(DockMode)";
                                item3d.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Bottom;
                                item3d.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Bottom); };
                                item.DropDownItems.Add(item3d);

                                var item4 = new ToolStripMenuItem();
                                item4.Text = "メインウインドウから浮かせ、自由に配置する(&6)(FloatingMode)";
                                item4.Checked = dock.DockState == DockState.FloatingMode;
                                item4.Click += (sender, e) => { dock.DockState = DockState.FloatingMode; };
                                item.DropDownItems.Add(item4);
                            }
                        }

                        { // 横幅
                            var item = new ToolStripMenuItem();
                            item.Text = "メインウインドウに埋め込み時の横幅(&W)"; // Width
                            item_.DropDownItems.Add(item);

                            {
                                var item1 = new ToolStripMenuItem();
                                item1.Text = "100%(通常)(&1)"; // None
                                item1.Checked = config.KifuWindowWidthType == 0;
                                item1.Click += (sender, e) => { config.KifuWindowWidthType = 0; };
                                item.DropDownItems.Add(item1);

                                var item2 = new ToolStripMenuItem();
                                item2.Text = "125%(&2)";
                                item2.Checked = config.KifuWindowWidthType == 1;
                                item2.Click += (sender, e) => { config.KifuWindowWidthType = 1; };
                                item.DropDownItems.Add(item2);

                                var item3 = new ToolStripMenuItem();
                                item3.Text = "150%(&3)";
                                item3.Checked = config.KifuWindowWidthType == 2;
                                item3.Click += (sender, e) => { config.KifuWindowWidthType = 2; };
                                item.DropDownItems.Add(item3);

                                var item4 = new ToolStripMenuItem();
                                item4.Text = "175%(&4)";
                                item4.Checked = config.KifuWindowWidthType == 3;
                                item4.Click += (sender, e) => { config.KifuWindowWidthType = 3; };
                                item.DropDownItems.Add(item4);

                                var item5 = new ToolStripMenuItem();
                                item5.Text = "200%(&5)";
                                item5.Checked = config.KifuWindowWidthType == 4;
                                item5.Click += (sender, e) => { config.KifuWindowWidthType = 4; };
                                item.DropDownItems.Add(item5);
                            }
                        }

                    }

                    { // ×ボタンで消していた検討ウィンドウの復活

                        var item_ = new ToolStripMenuItem();
                        item_.Text = "検討ウィンドウ(&C)"; // Consideration window
                        item_window.DropDownItems.Add(item_);

                        var dock = config.EngineConsiderationWindowDockManager;

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = dock.Visible ? "非表示(&V)" : "再表示(&V)"; // visible // 
                            item.ShortcutKeys = Keys.Control | Keys.R; // EngineConsiderationWindowのR。Eが盤面編集のEditのEで使ってた…。
                            shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.R) { item.PerformClick(); e.Handled = true; } });
                            item.Click += (sender, e) => { dock.Visible ^= true; dock.RaisePropertyChanged("DockState", dock.DockState); };
                            item_.DropDownItems.Add(item);
                        }


                        { // フローティングの状態
                            var item = new ToolStripMenuItem();
                            item.Text = "表示位置(&F)"; // Floating window mode
                            item_.DropDownItems.Add(item);

                            {

                                var item1 = new ToolStripMenuItem();
                                item1.Text = "メインウインドウに埋め込む(&0)(EmbeddedMode)";
                                item1.Checked = dock.DockState == DockState.InTheMainWindow;
                                item1.Click += (sender, e) => { dock.DockState = DockState.InTheMainWindow; };
                                item.DropDownItems.Add(item1);

                                var item2 = new ToolStripMenuItem();
                                item2.Text = "メインウインドウから浮かせ、相対位置を常に保つ(&1)(FollowMode)";
                                item2.Checked = dock.DockState == DockState.FollowToMainWindow;
                                item2.Click += (sender, e) => { dock.DockState = DockState.FollowToMainWindow; };
                                item.DropDownItems.Add(item2);

                                var item3a = new ToolStripMenuItem();
                                item3a.Text = "メインウインドウから浮かせ、メインウインドウの上側に配置する(&2)(DockMode)";
                                item3a.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Top;
                                item3a.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Top); };
                                item.DropDownItems.Add(item3a);

                                var item3b = new ToolStripMenuItem();
                                item3b.Text = "メインウインドウから浮かせ、メインウインドウの左側に配置する(&3)(DockMode)";
                                item3b.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Left;
                                item3b.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Left); };
                                item.DropDownItems.Add(item3b);

                                var item3c = new ToolStripMenuItem();
                                item3c.Text = "メインウインドウから浮かせ、メインウインドウの右側に配置する(&4)(DockMode)";
                                item3c.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Right;
                                item3c.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Right); };
                                item.DropDownItems.Add(item3c);

                                var item3d = new ToolStripMenuItem();
                                item3d.Text = "メインウインドウから浮かせ、メインウインドウの下側に配置する(&5)(DockMode)";
                                item3d.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Bottom;
                                item3d.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Bottom); };
                                item.DropDownItems.Add(item3d);

                                var item4 = new ToolStripMenuItem();
                                item4.Text = "メインウインドウから浮かせ、自由に配置する(&6)(FloatingMode)";
                                item4.Checked = dock.DockState == DockState.FloatingMode;
                                item4.Click += (sender, e) => { dock.DockState = DockState.FloatingMode; };
                                item.DropDownItems.Add(item4);
                            }
                        }

                        { // 縦幅
                            var item = new ToolStripMenuItem();
                            item.Text = "メインウインドウに埋め込み時の高さ(&H)"; // Height
                            item_.DropDownItems.Add(item);

                            {
                                var item1 = new ToolStripMenuItem();
                                item1.Text = "100%(通常)(&1)"; // None
                                item1.Checked = config.ConsiderationWindowHeightType == 0;
                                item1.Click += (sender, e) => { config.ConsiderationWindowHeightType = 0; };
                                item.DropDownItems.Add(item1);

                                var item2 = new ToolStripMenuItem();
                                item2.Text = "125%(&2)";
                                item2.Checked = config.ConsiderationWindowHeightType == 1;
                                item2.Click += (sender, e) => { config.ConsiderationWindowHeightType = 1; };
                                item.DropDownItems.Add(item2);

                                var item3 = new ToolStripMenuItem();
                                item3.Text = "150%(&3)";
                                item3.Checked = config.ConsiderationWindowHeightType == 2;
                                item3.Click += (sender, e) => { config.ConsiderationWindowHeightType = 2; };
                                item.DropDownItems.Add(item3);

                                var item4 = new ToolStripMenuItem();
                                item4.Text = "175%(&4)";
                                item4.Checked = config.ConsiderationWindowHeightType == 3;
                                item4.Click += (sender, e) => { config.ConsiderationWindowHeightType = 3; };
                                item.DropDownItems.Add(item4);

                                var item5 = new ToolStripMenuItem();
                                item5.Text = "200%(&5)";
                                item5.Checked = config.ConsiderationWindowHeightType == 4;
                                item5.Click += (sender, e) => { config.ConsiderationWindowHeightType = 4; };
                                item.DropDownItems.Add(item5);
                            }
                        }

                    }

                    { // ×ボタンで消していた検討ウィンドウの復活

                        var item_ = new ToolStripMenuItem();
                        item_.Text = "ミニ盤面(&M)"; // Mini shogi board
                        item_window.DropDownItems.Add(item_);

                        var dock = config.MiniShogiBoardDockManager;

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = dock.Visible ? "非表示(&V)" : "再表示(&V)"; // visible // 
                            item.ShortcutKeys = Keys.Control | Keys.M; // Mini shogi boardのM。
                            shortcut.AddEvent1(e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.M) { item.PerformClick(); e.Handled = true; } });
                            item.Click += (sender, e) => { dock.Visible ^= true; dock.RaisePropertyChanged("DockState", dock.DockState); };
                            item_.DropDownItems.Add(item);
                        }


                        { // フローティングの状態
                            var item = new ToolStripMenuItem();
                            item.Text = "表示位置(&F)"; // Floating window mode
                            item_.DropDownItems.Add(item);

                            {

                                var item1 = new ToolStripMenuItem();
                                item1.Text = "検討ウインドウに埋め込む(&0)(EmbeddedMode)";
                                item1.Checked = dock.DockState == DockState.InTheMainWindow;
                                item1.Click += (sender, e) => { dock.DockState = DockState.InTheMainWindow; };
                                item.DropDownItems.Add(item1);

                                var item2 = new ToolStripMenuItem();
                                item2.Text = "検討ウインドウから浮かせ、相対位置を常に保つ(&1)(FollowMode)";
                                item2.Checked = dock.DockState == DockState.FollowToMainWindow;
                                item2.Click += (sender, e) => { dock.DockState = DockState.FollowToMainWindow; };
                                item.DropDownItems.Add(item2);

                                var item3a = new ToolStripMenuItem();
                                item3a.Text = "検討ウインドウから浮かせ、メインウインドウの上側に配置する(&2)(DockMode)";
                                item3a.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Top;
                                item3a.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Top); };
                                item.DropDownItems.Add(item3a);

                                var item3b = new ToolStripMenuItem();
                                item3b.Text = "メインウインドウから浮かせ、メインウインドウの左側に配置する(&3)(DockMode)";
                                item3b.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Left;
                                item3b.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Left); };
                                item.DropDownItems.Add(item3b);

                                var item3c = new ToolStripMenuItem();
                                item3c.Text = "検討ウインドウから浮かせ、メインウインドウの右側に配置する(&4)(DockMode)";
                                item3c.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Right;
                                item3c.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Right); };
                                item.DropDownItems.Add(item3c);

                                var item3d = new ToolStripMenuItem();
                                item3d.Text = "検討ウインドウから浮かせ、メインウインドウの下側に配置する(&5)(DockMode)";
                                item3d.Checked = dock.DockState == DockState.DockedToMainWindow && dock.DockPosition == DockPosition.Bottom;
                                item3d.Click += (sender, e) => { dock.SetState(DockState.DockedToMainWindow, DockPosition.Bottom); };
                                item.DropDownItems.Add(item3d);

                                var item4 = new ToolStripMenuItem();
                                item4.Text = "検討ウインドウから浮かせ、自由に配置する(&6)(FloatingMode)";
                                item4.Checked = dock.DockState == DockState.FloatingMode;
                                item4.Click += (sender, e) => { dock.DockState = DockState.FloatingMode; };
                                item.DropDownItems.Add(item4);
                            }
                        }

                    }

                    item_window.DropDownItems.Add(new ToolStripSeparator());

                    {
                        // デバッグウィンドウ

                        var item_ = new ToolStripMenuItem();
                        item_.Text = "デバッグ用のログ(&D)"; // Debug window

                        item_window.DropDownItems.Add(item_);

                        {
                            // デバッグ

                            {
                                // デバッグウィンドウ

                                var item1 = new ToolStripMenuItem();
                                item1.Text = "デバッグウィンドウの表示(&D)"; // Debug Window
                                item1.ShortcutKeys = Keys.Control | Keys.D;
                                shortcut.AddEvent1( e => { if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D) { item1.PerformClick(); e.Handled = true; } });
                                item1.Click += (sender, e) =>
                                {
                                    if (debugDialog != null)
                                    {
                                        debugDialog.Dispose();
                                        debugDialog = null;
                                    }

                                    var log = Log.log1;
                                    if (log != null)
                                    {
                                            // セットされているはずなんだけどなぁ…。おかしいなぁ…。
                                            debugDialog = new DebugWindow((MemoryLog)log);
                                    }

                                    if (debugDialog != null)
                                    {
                                        FormLocationUtility.CenteringToThisForm(debugDialog, this);
                                        debugDialog.Show();
                                    }
                                };
                                item_.DropDownItems.Add(item1);
                            }

                            {
                                // ファイルへのロギング

                                var item1 = new ToolStripMenuItem();
                                var enabled = config.FileLoggingEnable;
                                item1.Text = enabled ? "ファイルへのロギング終了(&L)" : "ファイルへのロギング開始(&L)"; // Logging
                                item1.Checked = enabled;

                                item1.Click += (sender, e) => { config.FileLoggingEnable ^= true; };
                                item_.DropDownItems.Add(item1);
                            }

                            //item_.DropDownItems.Add(new ToolStripSeparator());

                        }

                    }

#if false // マスターアップに間に合わなさそう。
                    { // ×ボタンで消していた形勢グラフウィンドウの復活

                        var item = new ToolStripMenuItem();
                        item.Text = "形勢グラフウィンドウの表示(&G)"; // eval Graph
                        item.Click += (sender, e) =>
                        {
                            if (evalGraphDialog == null || evalGraphDialog.IsDisposed)
                            {
                                evalGraphDialog = new Info.EvalGraphDialog();
                            }
                            evalGraphDialog.DispatchEvalGraphUpdate(gameServer);
                            evalGraphDialog.Visible = true;
                        };
                        item_window.DropDownItems.Add(item);
                    }
#endif
                }

                // 「ヘルプ」
                {
                    var item_others = new ToolStripMenuItem();
                    item_others.Text = "ヘルプ(&H)"; // Help
                    menu.Items.Add(item_others);

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "よくある質問 (&F)"; // Faq
                        item1.Click += (sender, e) =>
                        {
                                // MyShogi公式のFAQ
                                var url = "https://github.com/yaneurao/MyShogi/tree/master/MyShogi/docs/faq.md";

                            System.Diagnostics.Process.Start(url);
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "操作説明(オンラインマニュアル) (&M)"; // Manual
                        item1.Click += (sender, e) =>
                        {
                                // MyShogi公式のonline manual
                                var url = "https://github.com/yaneurao/MyShogi/tree/master/MyShogi/docs/online_manual.md";

                            System.Diagnostics.Process.Start(url);
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    item_others.DropDownItems.Add(new ToolStripSeparator());

                    {
                        // aboutダイアログ

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "バージョン情報(&V)"; // Version
                        item1.Click += (sender, e) =>
                        {
                            using (var dialog = new AboutYaneuraOu())
                            {
                                FormLocationUtility.CenteringToThisForm(dialog, this);
                                dialog.ShowDialog(this);
                            }
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    {
                        // システム情報ダイアログ

                        var item1 = new ToolStripMenuItem();
                        item1.Text = "システム情報(&S)"; // System Infomation
                        item1.Click += (sender, e) =>
                        {
                            using (var dialog = new SystemInfoDialog())
                            {
                                FormLocationUtility.CenteringToThisForm(dialog, this);
                                dialog.ShowDialog(this);
                            }
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                    item_others.DropDownItems.Add(new ToolStripSeparator());

                    {
                        var item1 = new ToolStripMenuItem();
                        item1.Text = "アップデートの確認(&U)"; // Update
                        item1.Click += (sender, e) =>
                        {
                                // ・オープンソース版は、MyShogiのプロジェクトのサイト
                                // ・商用版は、マイナビの公式サイトのアップデートの特設ページ
                                // が開くようにしておく。
                                var url = config.CommercialVersion == 0 ?
                                    "https://github.com/yaneurao/MyShogi" :
                                    "https://book.mynavi.jp/ec/products/detail/id=92007"; // 予定地

                                System.Diagnostics.Process.Start(url);
                        };
                        item_others.DropDownItems.Add(item1);
                    }

                }

                // 開発時にだけオンにして使う。
#if false //DEBUG

                    // デバッグ用にメニューにテストコードを実行する項目を追加する。
                    {
                        var item_debug = new ToolStripMenuItem();
                        item_debug.Text = "デバッグ(&G)"; // debuG

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "DevTest1.Test1()";
                            item.Click += (sender, e) => { DevTest1.Test1(); };
                            item_debug.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "DevTest1.Test2()";
                            item.Click += (sender, e) => { DevTest1.Test2(); };
                            item_debug.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "DevTest1.Test3()";
                            item.Click += (sender, e) => { DevTest1.Test3(); };
                            item_debug.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "DevTest1.Test4()";
                            item.Click += (sender, e) => { DevTest1.Test4(); };
                            item_debug.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "DevTest1.Test5()";
                            item.Click += (sender, e) =>
                            {
                                // 何か実験用のコード、ここに書く。
                            };
                            item_debug.DropDownItems.Add(item);
                        }

                        {
                            var item = new ToolStripMenuItem();
                            item.Text = "DevTest2.Test1()";
                            item.Click += (sender, e) => { DevTest2.Test1(); };
                            item_debug.DropDownItems.Add(item);
                        }

                        menu.Items.Add(item_debug);
                    }
#endif



                // メニューのフォントを設定しなおす。
                FontUtility.ReplaceFont(menu, config.FontManager.MenuStrip);

                // レイアウトロジックを停止する
                // メニューの差し替え時間をなるべく小さくしてちらつきを防止する。
                // (しかしこれでもちらつく。なんぞこれ…)
                using (var slb = new SuspendLayoutBlock(this))
                {
                    // フォームのメインメニューとする
                    MainMenuStrip = menu;

                    Controls.Add(menu);

                    // 前回設定されたメニューを除去する
                    // 古いほうのmenu、removeしないと駄目
                    if (old_menu != null)
                    {
                        Controls.Remove(old_menu);
                        old_menu.Dispose();
                    }

                    old_menu = menu;
                    // 次回このメソッドが呼び出された時にthis.Controls.Remove(old_menu)する必要があるので
                    // 記憶しておかないと駄目。
                }
                // レイアウトロジックを再開する
            }

            // 画面の描画が必要になるときがあるので..
            gameScreenControl1.ForceRedraw();
        }

        /// <summary>
        /// 前回のメニュー項目。
        /// </summary>
        private MenuStripEx old_menu { get; set; } = null;

        /// <summary>
        /// 前回にUpdateMenuItems()が呼び出された時のGameMode。
        /// </summary>
        private GameModeEnum lastGameMode = GameModeEnum.ConsiderationWithoutEngine;
    }
}

