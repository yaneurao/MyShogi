using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MyShogi.App;
using MyShogi.Model.Common.Collections;
using MyShogi.Model.Common.Math;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Resource.Sounds;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Data;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.Player;
using MyShogi.Model.Shogi.Usi;

namespace MyShogi.Model.Shogi.LocalServer
{
    public partial class LocalGameServer
    {

        #region 対局監視スレッド

        /// <summary>
        /// スレッドによって実行されていて、対局を管理している。
        /// pooling用のthread。少しダサいが、通知によるコールバックモデルでやるよりコードがシンプルになる。
        /// どのみち持ち時間の監視などを行わないといけないので、このようなworker threadは必要だと思う。
        ///
        /// ※　No Threadモードだとこのメソッドは実行されないので注意。
        /// </summary>
        private void thread_worker()
        {
            try
            {
                var sw = new Stopwatch();
                while (!workerStop)
                {
                    // 時間を計測する。
                    sw.Reset();
                    sw.Start();

                    // 各プレイヤーのプロセスの標準入出力に対する送受信の処理
                    foreach (var player in Players)
                    {
                        player.OnIdle();
                    }

                    // UI側からのコマンドがあるかどうか。あればdispatchする。
                    CheckUICommand();

                    // 各プレイヤーから指し手が指されたかのチェック
                    CheckPlayerMove();

                    // 時間消費のチェック。時間切れのチェック。
                    CheckTime();

                    // ここでGC掃除
                    //GC.Collect();

                    // (最大で)10msのSleep。
                    var sleep_ms = (int)Math.Max(0, 10 - sw.ElapsedMilliseconds);
                    Thread.Sleep(sleep_ms);
                }
            } catch (Exception ex)
            {
                TheApp.app.MessageShow(ex);
            }
        }

        /// <summary>
        /// UI側からのコマンドがあるかどうかを調べて、あれば実行する。
        /// </summary>
        private void CheckUICommand()
        {
            List<UICommand> commands = null;
            lock (UICommandLock)
            {
                if (UICommands.Count != 0)
                {
                    // コピーしてからList.Clear()を呼ぶぐらいなら参照をすげ替えて、newしたほうが速い。
                    commands = UICommands;
                    UICommands = new List<UICommand>();
                }
            }
            // lockの外側で呼び出さないとdead lockになる。
            if (commands != null)
                foreach (var command in commands)
                    command();
        }

        /// <summary>
        /// 「先手」「後手」の読み上げは、ゲーム開始後、初回のみなので
        /// そのためのフラグ
        /// </summary>
        private bool[] sengo_read_out;

        /// <summary>
        /// 対局開始のためにGameSettingの設定に従い、ゲームを初期化する。
        /// </summary>
        /// <param name="gameSetting"></param>
        private void GameStart(GameSetting gameSetting)
        {
            // エンジン検討中であるなら、まずそれを停止させる。(通常検討モードに移行する)
            // これは、GameModeへの代入によって自動的に処理がなされる。
            if (GameMode.IsConsiderationWithEngine())
                GameMode = GameModeEnum.ConsiderationWithoutEngine;

            // 持ち時間などの設定が必要なので、
            // GameStart()時点のGameSettingをこのクラスのpropertyとしてコピーしておく。
            GameSetting = gameSetting;

            var nextGameMode = GameModeEnum.InTheGame;

            // -- 連続対局の回数をセット

            var misc = gameSetting.MiscSettings;
            // CPU同士の時のみ連続対局が有効である。

            // ContinuousGameの初期化
            {
                continuousGame.SetPlayLimit(
                    gameSetting.PlayerSetting(Color.BLACK).IsCpu &&
                    gameSetting.PlayerSetting(Color.WHITE).IsCpu &&
                    misc.ContinuousGameEnable ? misc.ContinuousGame : 0
                    );

                // 連続対局時にはプレイヤー入れ替えなどで壊す可能性があるのでClone()して保存しておく。
                continuousGame.GameSetting = gameSetting.Clone();

                // 対局開始時の振り駒のアニメーションのため、こちらにコピーして使う。
                continuousGame.EnablePieceToss = gameSetting.MiscSettings.EnablePieceToss;

                // 振り駒をするのかのチェック
                CheckPieceToss(nextGameMode);
            }

            // 以下の初期化中に駒が動かされるの気持ち悪いのでユーザー操作を禁止しておく。
            CanUserMove = false;
            Initializing = true;

            var config = TheApp.app.Config;

            // 音声:「よろしくお願いします。」
            TheApp.app.SoundManager.Stop(); // 再生中の読み上げをすべて停止
            if (config.ReadOutGreeting != 0)
                TheApp.app.SoundManager.ReadOut(SoundEnum.Start);

            // 初回の指し手で、「先手」「後手」と読み上げるためのフラグ
            sengo_read_out = new bool[2] { false, false };

            // プレイヤーの生成
            UsiEngineHashManager.Init();
            foreach (var c in All.Colors())
            {
                var gamePlayer = gameSetting.PlayerSetting(c);
                var playerType = gamePlayer.IsHuman ? PlayerTypeEnum.Human : PlayerTypeEnum.UsiEngine;
                Players[(int)c] = PlayerBuilder.Create(playerType);
            }

            // Players[]の生成が終わったので、必要ならば画面に「エンジン初期化中」の画像を描画する。
            UpdateEngineInitializing();

            foreach (var c in All.Colors())
            {
                // これ書くの2度目だが、まあ、しゃーない。
                var gamePlayer = gameSetting.PlayerSetting(c);
                var playerType = gamePlayer.IsHuman ? PlayerTypeEnum.Human : PlayerTypeEnum.UsiEngine;

                if (playerType == PlayerTypeEnum.UsiEngine)
                {
                    var engineDefineEx = TheApp.app.EngineDefines.Find(x => x.FolderPath == gamePlayer.EngineDefineFolderPath);
                    if (engineDefineEx == null)
                    {
                        TheApp.app.MessageShow("エンジンがありませんでした。" + gamePlayer.EngineDefineFolderPath, MessageShowType.Error);
                        return;
                    }
                    var usiEnginePlayer = Players[(int)c] as UsiEnginePlayer;
                    var ponder = gamePlayer.Ponder;
                    InitUsiEnginePlayer(c, usiEnginePlayer, engineDefineEx, gamePlayer.SelectedEnginePreset, nextGameMode, ponder);
                }
            }

            // 局面の初期化

            kifuManager.EnableKifuList = true;
            InitBoard(gameSetting.BoardSetting, false);

            // 本譜の手順に変更したので現在局面と棋譜ウィンドウのカーソルとを同期させておく。
            UpdateKifuSelectedIndex(int.MaxValue /* 末尾に移動 */);

            // エンジンに与えるHashSize,Threadsの計算
            var firstOfContinuousGame = continuousGame.PlayCount == 0; // 連続対局の初回局である
            if (UsiEngineHashManager.CalcHashSize(firstOfContinuousGame) != 0)
            {
                // Hash足りなくてダイアログ出した時にキャンセルボタン押されとる
                Disconnect();

                // ゲームが終局したことを通知するために音声があったほうがよさげ。
                TheApp.app.SoundManager.ReadOut(SoundEnum.End);

                return;
            }

            // エンジンを開始させることが確定したので実際に子プロセスとして起動する。
            // 1) このタイミングにしないと、Hashが足りなくてユーザーがキャンセルする可能性があって、
            // それまでにエンジンがタイムアウトになりかねない。
            // 2) エンジンを起動させてから、Hashの計算をするのでは、エンジンを起動させる時間が無駄である。
            foreach (var c in All.Colors())
            {
                // これ書くの3度目だが、まあしゃーない…。
                var gamePlayer = gameSetting.PlayerSetting(c);
                var playerType = gamePlayer.IsHuman ? PlayerTypeEnum.Human : PlayerTypeEnum.UsiEngine;

                if (playerType == PlayerTypeEnum.UsiEngine)
                {
                    var engineDefineEx = TheApp.app.EngineDefines.Find(x => x.FolderPath == gamePlayer.EngineDefineFolderPath);
                    var usiEnginePlayer = Players[(int)c] as UsiEnginePlayer;

                    // これで子プロセスとして起動する。
                    StartEngine(usiEnginePlayer, engineDefineEx);
                }
            }

            // Restart処理との共通部分
            GameStartInCommon(nextGameMode);

            // 新規対局で手番が変わった。
            //NotifyTurnChanged();
            // →　エンジンの初期化が現時点では終わっていない。
            // 　ゆえに、UpdateInitializing()のなかで初期化が終わったタイミングでNotifyTurnChanged()を呼ぶ。
            // それまではTimeUpの処理をしてはならない。
        }

        /// <summary>
        /// 連続対局の2局目以降の開始処理。
        /// GameStart()をコピペして、プレイヤーの生成と、エンジンの初期化部分をはしょってある。
        /// ・先後プレイヤーの入替えの処理
        /// ・先後の対局設定の入替えの処理
        /// ・検討ウィンドウへのリダイレクトのしなおし(先後入れ替わるので)
        /// </summary>
        public void GameRestart()
        {
            // 両方がエンジンでなければおかしい。
            Debug.Assert(
                Players[0].PlayerType == PlayerTypeEnum.UsiEngine &&
                Players[1].PlayerType == PlayerTypeEnum.UsiEngine
            );

            Initializing = true;
            // Players[]の生成が終わっているので、画面に「エンジン初期化中」の画像を描画する。
            // (エンジンの再初期化には時間がほとんど必要ないため一瞬で終わるだろうが…)
            UpdateEngineInitializing();

            var nextGameMode = GameModeEnum.InTheGame;

            // 音声:「よろしくお願いします。」
            TheApp.app.SoundManager.Stop(); // 再生中の読み上げをすべて停止
            TheApp.app.SoundManager.ReadOut(SoundEnum.Start);

            // 初回の指し手で、「先手」「後手」と読み上げるためのフラグ
            sengo_read_out = new bool[2] { false, false };

            // プレイヤーの生成
            // →　生成されているはず
            // エンジンの初期化も終わっているはず。

            var gameSetting = GameSetting;

            // 対局の持ち時間設定などの先後入替
            // (対局設定ダイアログで「連続対局のときに先後入れ替えない」にチェックが入っていなければ)
            if (!gameSetting.MiscSettings.ContinuousGameNoSwapPlayer)
                SwapPlayer();

            // 振り駒をするのかのチェック
            CheckPieceToss(nextGameMode);

            // 検討ウィンドウのリダイレクト、先後入れ替えるのでいったんリセット
            foreach (var c in All.Colors())
            {
                var engine_player = Player(c) as UsiEnginePlayer;
                engine_player.Engine.RemovePropertyChangedHandler("ThinkReport");

                // エンジンに対して再度、"isready"を送信して…。
                engine_player.SendIsReady();
            }

            // 局面の初期化

            kifuManager.EnableKifuList = true;
            InitBoard(gameSetting.BoardSetting , true);

            // 本譜の手順に変更したので現在局面と棋譜ウィンドウのカーソルとを同期させておく。
            UpdateKifuSelectedIndex(int.MaxValue);

            GameStartInCommon(nextGameMode);

            // NotifyTurnChanged();
            // →　GameStart()と同じ理由により、これはここで呼び出してはならない。
        }

        /// <summary>
        /// 先後のプレイヤーを入れ替える。
        /// エンジンの名前、プリセットなども入れ替える。
        /// </summary>
        private void SwapPlayer()
        {
            // プレイヤーの実体の先後入替え
            Utility.Swap(ref Players[0], ref Players[1]);
            Utility.Swap(ref EngineDefineExes[0], ref EngineDefineExes[1]);

            continuousGame.SwapPlayer();

            // GameStart()のときの対局設定(GameSetting)から、先後入れ替えていることを明示
            continuousGame.Swapped ^= true;

            GameSetting.SwapPlayer();
        }

        /// <summary>
        /// 盤面を初期化する
        /// </summary>
        /// <param name="board"></param>
        /// <param name="restart">GameRestart()時か？</param>
        private void InitBoard(BoardSetting board , bool restart)
        {

            if (board.BoardTypeCurrent)
            {
                // 現在の局面から

                if (restart)
                {
                    // 「現在の局面」からのリスタート。KIF形式の文字列経由で復元する。
                    kifuManager.FromString(continuousGame.Kif);

                } else {

                    // 現在の局面からなので、いま以降の局面を削除する。
                    // ただし、いまの局面と棋譜ウィンドウとが同期しているとは限らない。
                    // まず現在局面以降の棋譜を削除しなくてはならない。

                    // 元nodeが、special moveであるなら、それを削除しておく。
                    if (kifuManager.Tree.IsSpecialNode())
                        kifuManager.Tree.UndoMove();

                    kifuManager.Tree.ClearForward();

                    // 分岐棋譜かも知れないので、現在のものを本譜の手順にする。
                    kifuManager.Tree.MakeCurrentNodeMainBranch(); // View側の都合により選択行が移動してしまう可能性がある。

                    // 連続対局が設定されているなら、現在の局面を棋譜文字列として保存しておく。
                    if (continuousGame.IsContinuousGameSet())
                        continuousGame.Kif = kifuManager.ToString(KifuFileType.KIF);
                }
            }
            else if (board.BoardTypeEnable)
            {
                // 典型的な初期局面が指定されている
                kifuManager.Init();
                kifuManager.InitBoard(board.BoardType);
            }
            else if (board.BoardTypeShogi960)
            {
                // Shogi960のルールにて開始
                kifuManager.Init();
                kifuManager.Tree.SetRootSfen(ExtendedGame.Shogi960());
            }
            else
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// 振り駒を行うかのチェック
        /// </summary>
        private void CheckPieceToss(GameModeEnum nextGameMode)
        {
            if (nextGameMode == GameModeEnum.InTheGame
                && continuousGame.EnablePieceToss)
            {
                // -- 振り駒ありなので駒を振る

                // 表の枚数
                var total_black = 0;
                foreach (var i in All.Int(5))
                {
                    var r = Rand.NextBool();
                    continuousGame.PieceTossPieceColor[i] = r;
                    if (r)
                        ++total_black;
                }

                // 表の枚数が3枚未満なら先手を元のGameSettingからswapされている状態にする。
                var swap_needed = total_black < 3;
                if (continuousGame.Swapped != swap_needed)
                    SwapPlayer();

                // 振り駒の画像が表示されないことがある。
                // エンジン初期化直後だし初回読み込みのとき0.5秒で画面素材の読み込みが間に合わないことがあるのか…。
                // 振り駒での対局が確定した時点で先読みしておく。

                var piece_toss_image = TheApp.app.ImageManager.GamePieceTossImage.image;
            }
        }

        /// <summary>
        /// GameStart()とGameRestart()の共通部分
        /// </summary>
        private void GameStartInCommon(GameModeEnum nextGameMode)
        {
            var gameSetting = GameSetting;

            // 現在の時間設定を、KifuManager.Treeに反映させておく(棋譜保存時にこれが書き出される)
            kifuManager.Tree.KifuTimeSettings = gameSetting.KifuTimeSettings;

            // 対局者氏名の設定
            // 対局設定ダイアログの名前をそのまま引っ張ってくる。
            foreach (var c in All.Colors())
            {
                var name = gameSetting.PlayerSetting(c).PlayerName;
                SetPlayerName(c, name);
            }

            // 持ち時間設定の表示文字列の構築(最初に構築してしまい、対局中には変化させない)
            foreach (var c in All.Colors())
            {
                var pc = PlayTimer(c);
                pc.KifuTimeSetting = GameSetting.KifuTimeSettings.Player(c);
                pc.GameStart();

                var left = pc.KifuTimeSetting.ToShortString();
                var right = PresetWithBoardTypeString(c);
                string total;
                if (right.Empty())
                    total = left;
                else if (left.Empty())
                    total = right;                  // このとき連結のための"/"要らない
                else if (left.UnicodeLength() + right.UnicodeLength() < 24)
                    total = $"{left}/{right}";      // 1行で事足りる
                else
                    total = $"{left}\r\n{right}";   // 2行に分割する。 

                timeSettingStrings[(int)c] = total;
            }

            // rootの持ち時間設定をここに反映させておかないと待ったでrootまで持ち時間が戻せない。
            // 途中の局面からだとここではなく、現局面のところに書き出す必要がある。
            kifuManager.Tree.SetKifuMoveTimes(PlayTimers.GetKifuMoveTimes());

            // コンピュータ vs 人間である場合、人間側を手前にしてやる。
            // 人間 vs 人間の場合も最初の手番側を手前にしてやる。
            // ただし、人間 vs 人間で振り駒でこの手番が決まったのであれば、
            // それを反映しなければならない。

            var stm = kifuManager.Position.sideToMove;
            if (gameSetting.PlayerSetting(stm).IsHuman)
            {
                // 1. 両方が人間である場合、普通は手番側が手前だが、振り駒をしたなら
                // 　対局設定の左側のプレイヤーがつねに手前。
                if (gameSetting.PlayerSetting(stm.Not()).IsHuman)
                    BoardReverse = (stm == Color.WHITE) ^ continuousGame.Swapped;

                // 2. 手番側が人間である場合(非手番側がCPU)
                else
                    BoardReverse = (stm == Color.WHITE);
            }
            // 3. 手番側がCPUで、非手番側が人間である場合。
            else if (gameSetting.PlayerSetting(stm).IsCpu && gameSetting.PlayerSetting(stm.Not()).IsHuman)
                BoardReverse = (stm == Color.BLACK);

            // プレイヤー情報などを検討ダイアログに反映させる。
            InitEngineConsiderationInfo(nextGameMode);

            // 検討モードならそれを停止させる必要があるが、それはGameModeのsetterがやってくれる。
            GameMode = nextGameMode;
        }

        /// <summary>
        /// エンジンを初期化する。
        /// </summary>
        /// <param name="usiEnginePlayer"></param>
        /// <param name="selectedPresetIndex">プリセットの選択番号+1。選んでなければ0。</param>
        private void InitUsiEnginePlayer(Color c , UsiEnginePlayer usiEnginePlayer ,
            EngineDefineEx engineDefineEx , int selectedPresetIndex , GameModeEnum nextGameMode , bool ponder)
        {
            EngineDefineExes[(int)c] = engineDefineEx; // ここに保存しておく。
            var presets = engineDefineEx.EngineDefine.Presets;
            // 選択されているpreset
            var preset = (selectedPresetIndex < presets.Count) ? presets[selectedPresetIndex] : null;

            continuousGame.SetPresetName(c, preset == null ? null : preset.Name);

            var engine_config = TheApp.app.EngineConfigs;
            EngineConfig config = null;

            switch (nextGameMode)
            {
                case GameModeEnum.InTheGame:
                    config = engine_config.NormalConfig;
                    break;
                case GameModeEnum.ConsiderationWithEngine:
                    config = engine_config.ConsiderationConfig;
                    break;
                case GameModeEnum.ConsiderationWithMateEngine:
                    config = engine_config.MateConfig;
                    break;
            }

            // Hash、Threadsのマネージメントのために代入しておく。
            UsiEngineHashManager.SetValue(c, engineDefineEx , config , preset , ponder);
            
            var engine = usiEnginePlayer.Engine;
            var engineDefine = engineDefineEx.EngineDefine;

            // "usiok"に対してオプションを設定するので、Stateの変更イベントをハンドルする。
            engine.AddPropertyChangedHandler("State", (args) =>
            {
                try
                {
                    var state = (UsiEngineState)args.value;
                    if (state == UsiEngineState.UsiOk)
                    {
                        // オプションの値を設定しなおす。
                        EngineDefineUtility.SetDefaultOption( engine.OptionList, engineDefineEx, selectedPresetIndex,
                            config , UsiEngineHashManager.HashSize[(int)c] , UsiEngineHashManager.Threads[(int)c] , ponder);
                    }
                } catch (Exception ex)
                {
                    TheApp.app.MessageShow(ex);
                }
            });

            // 通常探索なのか、詰将棋探索なのか。
            usiEnginePlayer.IsMateSearch =
                nextGameMode == GameModeEnum.ConsiderationWithMateEngine;

            // 実行ファイルを起動する
            //usiEnginePlayer.Start(engineDefine.EngineExeFileName());

            // →　このタイミング、早すぎる。
            // CalcHashでHashが足りることを確認してからにすべき。
        }

        /// <summary>
        /// エンジンを開始する。UsiEnginePlayer.Start()を呼び出す。
        /// InitUsiEnginePlayer()をしたのちに、エンジンに接続したいタイミングで呼び出すべし。
        /// </summary>
        /// <param name="usiEnginePlayer"></param>
        /// <param name="engineDefineEx"></param>
        private void StartEngine(UsiEnginePlayer usiEnginePlayer , EngineDefineEx engineDefineEx)
        {
            // 実行ファイルを起動する
            usiEnginePlayer.Start(engineDefineEx.EngineDefine.EngineExeFileName());
        }

        /// <summary>
        /// プレイヤー情報を検討ダイアログにリダイレクトする設定をする。
        /// </summary>
        private void InitEngineConsiderationInfo(GameModeEnum nextGameMode)
        {
            // CPUの数をNumberOfEngineに反映。
            int num = 0;

            if (nextGameMode.IsConsiderationWithEngine())
                num = 1; // エンジンによる検討モードなら出力は一つ。
            else 
                foreach (var c in All.Colors())
                    if (GameSetting.PlayerSetting(c).IsCpu)
                        ++num;
            NumberOfEngine = num;

            // エンジン数が確定したので、検討ウィンドウにNumberOfInstanceメッセージを送信してやる。
            ThinkReport = new UsiThinkReportMessage()
            {
                type = UsiEngineReportMessageType.NumberOfInstance,
                number = NumberOfEngine,
            };
            ThinkReport = new UsiThinkReportMessage()
            {
                type = UsiEngineReportMessageType.SetGameMode,
                data = nextGameMode
            };

            // 各エンジンの情報を検討ウィンドウにリダイレクトするようにハンドラを設定
            num = 0;
            foreach (var c in All.Colors())
            {
                if ((nextGameMode == GameModeEnum.InTheGame && GameSetting.PlayerSetting(c).IsCpu) ||
                    (nextGameMode.IsConsiderationWithEngine() && c == Color.BLACK) // // 検討用エンジンがぶら下がっていると考えられる。
                    )
                {
                    var num_ = num; // copy for capturing

                    var engineName = GetEngineDefine(c).EngineDefine.DisplayName;
                    var engineName2 = continuousGame.PresetName(c) == null ? engineName : $"{engineName} { continuousGame.PresetName(c)}";
                    var playerName = (nextGameMode.IsConsiderationWithEngine() || DisplayName(c) == engineName) ?
                        // 検討時には、エンジンの名前をそのまま表示。
                          engineName2 :
                        // 通常対局モードなら対局者名に括弧でエンジン名を表記。
                          $"{DisplayName(c)}({engineName2})";

                    ThinkReport = new UsiThinkReportMessage()
                    {
                        type = UsiEngineReportMessageType.SetEngineName,
                        number = num_, // is captured
                        data = playerName,
                    };

                    // UsiEngineのThinkReportプロパティを捕捉して、それを転送してやるためのハンドラをセットしておく。
                    var engine_player = Player(c) as UsiEnginePlayer;
                    engine_player.Engine.AddPropertyChangedHandler("ThinkReport", (args) =>
                    {
                        //// 1) 読み筋の抑制条件その1
                        //// 人間対CPUで、メニューの「ウィンドウ」のところで表示するになっていない場合。
                        //var surpress1 = NumberOfEngine == 1 && !TheApp.app.config.EngineConsiderationWindowEnableWhenVsHuman;

                        if (ThinkReportEnable
                            /* && !(surpress1) */ )
                        {
                            var report = args.value as UsiThinkReport;

                            if (report != null)
                            {
                                // このクラスのpropertyのsetterを呼び出してメッセージを移譲してやる。
                                ThinkReport = new UsiThinkReportMessage()
                                {
                                    type = UsiEngineReportMessageType.UsiThinkReport,
                                    number = num_, // is captrued
                                    data = report,
                                };
                            } else if (args.value is UsiEngineReportMessageType)
                            {
                                // これ、このまま投げとく。
                                ThinkReport = new UsiThinkReportMessage()
                                {
                                    type = (UsiEngineReportMessageType)args.value,
                                    number = num_, // is captrued
                                    data = null,
                                };
                            }
                        }
                    });

                    num++;
                }
            }
        }

        /// <summary>
        /// このLocalGameServerのインスタンスの管理下で現在動作しているエンジンの数 (0～2)
        /// これが0のときは人間同士の対局などなので、検討ウィンドウを表示しない。
        /// これが1のときは、1つしかないので、EngineConsiderationDialogには、そいつの出力を0番のインスタンスとして読み筋を出力。
        /// これが2のときは、EngineConsiderationDialogに、先手を0番、後手を1番として、読み筋を出力。
        /// </summary>
        private int NumberOfEngine;

        /// <summary>
        /// 駒音を再生する。
        /// </summary>
        /// <param name="gameMode"></param>
        /// <param name="to">移動先の升</param>
        /// <param name="stm">手番</param>
        private void PlayPieceSound(GameModeEnum gameMode , Square to , Color stm)
        {
            if ((gameMode == GameModeEnum.InTheGame && TheApp.app.Config.PieceSoundInTheGame != 0) ||
                (gameMode.IsConsideration() && TheApp.app.Config.PieceSoundOffTheGame != 0)
                )
            {
                // 移動先の升の下に別の駒があるときは、駒がぶつかる音になる。
                var delta = stm == Color.BLACK ? Square.SQ_D : Square.SQ_U;
                var to2 = to + (int)delta;
                // to2が盤外であることがあるので、IsOk()を通すこと。
                var e = (to2.IsOk() && Position.PieceOn(to2) != Piece.NO_PIECE)
                    ? SoundEnum.KOMA_B1 /*ぶつかる音*/: SoundEnum.KOMA_S1 /*ぶつからない音*/;

#if false
                            // あまりいい効果音作れなかったのでコメントアウトしとく。
                            if (TheApp.app.config.CrashPieceSoundInTheGame != 0)
                            {
                                // ただし、captureか捕獲する指し手であるなら、衝撃音に変更する。
                                if (Position.State().capturedPiece != Piece.NO_PIECE || Position.InCheck())
                                    e = SoundEnum.KOMA_C1;
                            }
#endif
                var soundManager = TheApp.app.SoundManager;
                soundManager.PlayPieceSound(e);
            }
        }

        /// <summary>
        /// 指し手が指されたかのチェックを行う
        /// </summary>
        private void CheckPlayerMove()
        {
            // 現状の局面の手番側
            var stm = Position.sideToMove;
            var stmPlayer = Player(stm);

            var config = TheApp.app.Config;

            // -- 指し手

            Move bestMove;
            if (GameMode.IsConsiderationWithEngine())
            {
                // 検討モードなのでエンジンから送られてきたbestMoveの指し手は無視。
                bestMove = stmPlayer.SpecialMove;
            }
            else
            {
                // 対局設定ダイアログの「コンピューターは1手に必ずこれだけ使う」が設定されていれば、
                // その時間になるまでbest moveを無視する。
                var stmBestMove = stmPlayer.BestMove;
                if (stmBestMove != Move.NONE
                    && GameMode == GameModeEnum.InTheGame /* 通常対局中 */
                    && stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine
                    && GameSetting.MiscSettings.EnableBestMoveIgnoreTimeForEngine
                    && PlayTimer(stm).ElapsedTime() < GameSetting.MiscSettings.BestMoveIgnoreTimeForEngine
                    )
                    stmBestMove = Move.NONE;

                // TIME_UPなどのSpecialMoveが積まれているなら、そちらを優先して解釈する。
                bestMove = stmPlayer.SpecialMove != Move.NONE ? stmPlayer.SpecialMove : stmBestMove;
            }

            if (bestMove != Move.NONE)
            {
                PlayTimer(stm).ChangeToThemTurn(bestMove == Move.TIME_UP);

                stmPlayer.SpecialMove = Move.NONE; // クリア

                // 駒が動かせる状況でかつ合法手であるなら、受理する。

                bool specialMove = false;
                if (GameMode == GameModeEnum.InTheGame)
                {
                    // 送信されうる特別な指し手であるか？
                    specialMove = bestMove.IsSpecial();

                    // エンジンから送られてきた文字列が、illigal moveであるならエラーとして表示する必要がある。

                    if (specialMove)
                    {
                        switch (bestMove)
                        {
                            // 入玉宣言勝ち
                            case Move.WIN:
                                var rule = (EnteringKingRule)GameSetting.MiscSettings.EnteringKingRule;
                                if (Position.DeclarationWin(rule) != Move.WIN)
                                    // 入玉宣言条件を満たしていない入玉宣言
                                    goto ILLEGAL_MOVE;
                                break;

                            // 中断
                            // コンピューター同士の対局の時にも人間判断で中断できなければならないので
                            // 対局中であればこれを無条件で受理する。
                            case Move.INTERRUPT:
                            // 時間切れ
                            // 時間切れになるとBestMoveに自動的にTIME_UPが積まれる。これも無条件で受理する。
                            case Move.TIME_UP:
                                break;

                            // 投了
                            case Move.RESIGN:
                                break; // 手番側の投了は無条件で受理

                            // それ以外
                            default:
                                // それ以外は受理しない
                                goto ILLEGAL_MOVE;
                        }
                    }
                    else if (!Position.IsLegal(bestMove))
                        // 合法手ではない
                        goto ILLEGAL_MOVE;


                    // -- bestMoveを受理して、局面を更新する。

                    kifuManager.Tree.AddNode(bestMove, PlayTimers.GetKifuMoveTimes());

                    // 受理できる性質の指し手であることは検証済み
                    // special moveであってもDoMove()してしまう。
                    kifuManager.DoMove(bestMove);

                    KifuDirty = true; // 新しいnodeに到達したので棋譜は汚れた扱い。

                    // -- 音声の読み上げ

                    var soundManager = TheApp.app.SoundManager;

                    // special moveはMoveを直接渡して再生。
                    if (bestMove.IsSpecial())
                        soundManager.ReadOut(bestMove);
                    else
                    {
                        // -- 駒音

                        PlayPieceSound(GameMode, bestMove.To(), stm);

                        // -- 棋譜の読み上げ

                        // 「先手」と「後手」と読み上げる。
                        if (!sengo_read_out[(int)stm] || config.ReadOutSenteGoteEverytime != 0)
                        {
                            sengo_read_out[(int)stm] = true;

                            // 駒落ちの時は、「上手(うわて)」と「下手(したて)」
                            if (!Position.Handicapped)
                                soundManager.ReadOut(stm == Color.BLACK ? SoundEnum.Sente : SoundEnum.Gote);
                            else
                                soundManager.ReadOut(stm == Color.BLACK ? SoundEnum.Shitate : SoundEnum.Uwate);
                        }

                        // 棋譜文字列をそのまま頑張って読み上げる。
                        // ただし、棋譜ウィンドウの表示形式を変更できるので…。

                        var kif = kifuManager.Tree.LastKifuString;
                        soundManager.ReadOut(kif);
                    }

                }

                // -- 次のPlayerに、自分のturnであることを通知してやる。

                if (!specialMove)
                    // 相手番になったので諸々通知。
                    NotifyTurnChanged();
                else
                    // 特殊な指し手だったので、これにてゲーム終了
                    GameEnd(bestMove);
            }

            return;

        ILLEGAL_MOVE:

            // これ、棋譜に記録すべき
            Move m = Move.ILLEGAL_MOVE;
            kifuManager.Tree.AddNode(m, PlayTimers.GetKifuMoveTimes());
            kifuManager.Tree.AddNodeComment(m, stmPlayer.BestMove.ToUsi() /* String あとでなおす*/ /* 元のテキスト */);
            kifuManager.Tree.DoMove(m);

            GameEnd(m); // これにてゲーム終了。
        }

        /// <summary>
        /// 手番側のプレイヤーに自分の手番であることを通知するためにThink()を呼び出す。
        /// また、手番側のCanMove = trueにする。非手番側のプレイヤーに対してCanMove = falseにする。
        /// </summary>
        private void NotifyTurnChanged()
        {
            var stm = Position.sideToMove;

            // 検討モードでは、先手側のプレイヤーがエンジンに紐づけられている。
            if (GameMode.IsConsiderationWithEngine())
                stm = Color.BLACK;

            var stmPlayer = Player(stm);
            var isHuman = stmPlayer.PlayerType == PlayerTypeEnum.Human;

            // 手番が変わった時に特殊な局面に至っていないかのチェック
            if (GameMode == GameModeEnum.InTheGame)
            {
                var misc = TheApp.app.Config.GameSetting.MiscSettings;
                Move m = kifuManager.Tree.IsNextNodeSpecialNode(isHuman , misc);

                // 上で判定された特殊な指し手であるか？
                if (m != Move.NONE)
                {
                    // この特殊な状況を棋譜に書き出して終了。
                    kifuManager.Tree.AddNode(m, KifuMoveTimes.Zero);
                    // speical moveでもDoMoveできることは保証されている。
                    kifuManager.Tree.DoMove(m);

                    GameEnd(m);
                    return;
                }
            }

            // USIエンジンのときだけ、"position"コマンドに渡す形で局面図が必要であるから、
            // 生成して、それをPlayer.Think()の引数として渡してやる。
            var isUsiEngine = stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine;
            string usiPosition = isUsiEngine ? kifuManager.UsiPositionString : null;

            stmPlayer.CanMove = true;
            stmPlayer.SpecialMove = Move.NONE;
            LastCheckedByoyomiReadOut = 0;

            // BestMove,PonderMoveは、Think()以降、正常に更新されることは、Playerクラス側で保証されているので、
            // ここではそれらの初期化は行わない。

            // -- MultiPVの設定

            if (GameMode == GameModeEnum.ConsiderationWithEngine)
                // MultiPVは、GlobalConfigの設定を引き継ぐ
                (stmPlayer as UsiEnginePlayer).Engine.MultiPV = TheApp.app.Config.ConsiderationMultiPV;
            // それ以外のGameModeなら、USIのoption設定を引き継ぐので変更しない。


            // -- Think()

            // 通常対局モードのはずなので現在の持ち時間設定を渡してやる。
            // エンジン検討モードなら検討エンジン設定に従う

            UsiThinkLimit limit = UsiThinkLimit.TimeLimitLess;

            switch(GameMode)
            {
                case GameModeEnum.InTheGame:
                    limit = UsiThinkLimit.FromTimeSetting(PlayTimers, stm);
                    break;

                case GameModeEnum.ConsiderationWithEngine:
                    {
                        var setting = TheApp.app.Config.ConsiderationEngineSetting;
                        if (setting.Limitless)
                            limit = UsiThinkLimit.TimeLimitLess;
                        else // if (setting.TimeLimit)
                            limit = UsiThinkLimit.FromSecond(setting.Second);
                    }
                    break;

                case GameModeEnum.ConsiderationWithMateEngine:
                    {
                        var setting = TheApp.app.Config.MateEngineSetting;
                        if (setting.Limitless)
                            limit = UsiThinkLimit.TimeLimitLess;
                        else // if (setting.TimeLimit)
                            limit = UsiThinkLimit.FromSecond(setting.Second);
                    }
                    break;
            }

            stmPlayer.Think(usiPosition , limit , stm);

            // -- 検討ウィンドウに対して、ここをrootSfenとして設定
            if (ThinkReportEnable && isUsiEngine)
            {
                ThinkReport = new UsiThinkReportMessage()
                {
                    type = UsiEngineReportMessageType.SetRootSfen,
                    number = NumberOfEngine == 1  ? 0 : (int)stm, // CPU1つなら1番目の窓、CPU2つならColorに相当する窓に
                    data = Position.ToSfen(),
                    // ここ、せめて前の局面からのsfenを渡さないと、
                    // PVの1手目に同金みたいな表現が出来なくなってしまう。
                };
            }

            // 手番側のプレイヤーの時間消費を開始
            if (GameMode == GameModeEnum.InTheGame)
            {
                // InTheGame == trueならば、PlayerTimeSettingは適切に設定されているはず。
                // (対局開始時に初期化するので)

                PlayTimer(stm).ChangeToOurTurn();
            }

            // 非手番側のCanMoveをfalseに

            var nextPlayer = Player(stm.Not());
            nextPlayer.CanMove = false;

            // -- 手番が変わった時の各種propertyの更新

            EngineTurn = stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine;
            // 対局中でなければ自由に動かせる。対局中は人間のプレイヤーでなければ駒を動かせない。
            CanUserMove = stmPlayer.PlayerType == PlayerTypeEnum.Human || GameMode.CanUserMove();

            // 値が変わっていなくとも変更通知を送りたいので自力でハンドラを呼び出す。

            RaisePropertyChanged("TurnChanged", CanUserMove); // 仮想プロパティ"TurnChanged"
        }

        /// <summary>
        /// 時間チェック
        /// </summary>
        private void CheckTime()
        {
            // エンジンの初期化中であるか。この時は、時間消費は行わない。
            UpdateInitializing();

            // 双方の残り時間表示の更新
            UpdateTimeString();

            // 時間切れ判定(対局中かつ手番側のみ)
            CheckTimeUp();

            // 秒の読み上げ処理
            CheckByoyomiReadOut();
        }

        /// <summary>
        /// 時間切れの判定(手番側のみ)
        /// </summary>
        private void CheckTimeUp()
        {
            var stm = Position.sideToMove;
            if (GameMode == GameModeEnum.InTheGame && !Initializing && PlayTimer(stm).IsTimeUp())
                Player(stm).SpecialMove = Move.TIME_UP;
            // ここにエンジンの初期化が終わっているという条件も必要だが、初期化が終わっていないときは、
            // 消費時間の計測用のタイマーは停止しており、また、残り時間が0になっているはずで、
            // 0はtime upではないという解釈なのでtime upという判定にはならないはずではある。

            // エンジンで発生した例外の捕捉
            foreach (var c in All.Colors())
            {
                if (Player(c).PlayerType == PlayerTypeEnum.UsiEngine)
                {
                    var engine = (Player(c) as UsiEnginePlayer).Engine;
                    var ex = engine.Exception;
                    if (ex != null)
                    {
                        TheApp.app.MessageShow(ex);
                        // これリカバーするの難しいので終了させる。
                        // →　エンジン切断してしまえばあとは無害なはずだが、連続対局を正常に終了しないといけないなど
                        // 色々な制約があるので、そのへんの検証が必要だから、とりあえずこうしとく。あとで修正するかも。

                        engine.Disconnect(); // 切断しとかないと次のRead()でまた例外が発生しかねない。
                        Player(stm).SpecialMove = Move.INTERRUPT;
                    }
                }
            }
        }

        /// <summary>
        /// 秒の読み上げ処理
        /// </summary>
        private void CheckByoyomiReadOut()
        {
            var stm = Position.sideToMove;
            var stmPlayer = Player(stm);

            if (GameMode == GameModeEnum.InTheGame /* 通常対局中 */
                && TheApp.app.Config.ReadOutByoyomi == 1
                && stmPlayer.PlayerType == PlayerTypeEnum.Human
                && GameSetting.KifuTimeSettings.Player(stm).ByoyomiEnable
             )
            {
                // 現在の秒読みの秒(これが表示されているはず)
                var now_second = PlayTimer(stm).DisplayByoyomi();

                // 前回このメソッドでチェックを行った秒から変化していないならreturn
                if (now_second == 0 || now_second == LastCheckedByoyomiReadOut)
                    return;

                // 秒読み設定(この秒になったら時間切れ)
                var byoyomi = GameSetting.KifuTimeSettings.Player(stm).Byoyomi;

                if (now_second >= byoyomi)
                {
                    // 時間切れであるから構わない
                }
                else if ((now_second % 10) == 0)
                {
                    // 1の位が0であるから10秒単位の数字の読み上げが必要である。
                    if (now_second <= 50)
                        TheApp.app.SoundManager.Play(SoundEnum.BYOYOMI_10BYO + (now_second / 10) - 1);
                    // 60秒以上の時はどうするのか知らん。素材ないし…。

                }
                // 30秒設定なら20秒以降、1,2,3,…の読み上げが必要。
                // 60秒設定なら50秒以降、1,2,3,…の読み上げが必要。
                else if (now_second / 10 == byoyomi / 10 - 1)
                {
                    // 1秒単位の読み上げが必要である。
                    TheApp.app.SoundManager.Play(SoundEnum.BYOYOMI_1 + (now_second % 10) - 1);
                }

                // この秒までは読み上げのチェックを行った
                LastCheckedByoyomiReadOut = now_second;
            }
        }

        /// <summary>
        /// 残り時間の更新
        /// </summary>
        /// <param name="c"></param>
        private void UpdateTimeString()
        {
            // 前回と同じ文字列であれば実際は描画ハンドラが呼び出されないので問題ない。
            foreach (var c in All.Colors())
            {
                var ct = PlayTimer(c);
                SetRestTimeString(c, ct.DisplayShortString());
            }
        }

        /// <summary>
        /// ゲームの終了処理
        ///
        /// ※　連続対局がセットされているときは、このメソッドのなかでGameRestart()が呼び出されて連続対局になる。
        /// </summary>
        private void GameEnd(Move lastMove)
        {
            var stm = Position.sideToMove;
            var gamePly = Position.gamePly;

            // 対局中だったものが終了したのか？
            if (GameMode == GameModeEnum.InTheGame)
            {
                var config = TheApp.app.Config;
                if (config.ReadOutCancelWhenGameEnd != 0)
                    TheApp.app.SoundManager.Stop();

                // 音声:「ありがとうございました。またお願いします。」
                if (config.ReadOutGreeting != 0)
                    TheApp.app.SoundManager.ReadOut(SoundEnum.End);

                // --- 終局時にエンジンに対して"gameover win"などを送信するための処理

                foreach (var c in All.Colors())
                    if (Player(c).PlayerType == PlayerTypeEnum.UsiEngine)
                    {
                        var result = lastMove.GameResult();
                        (Player(c) as UsiEnginePlayer).SendGameOver(c == stm ? result : result.Not());
                    }

                // --- 終了時の「対局終了」の表示

                // 「対局開始」の画面素材を表示するためのイベント
                var game_result = MoveGameResult.UNKNOWN;
                // 片側だけが人間であるなら、人間にとって勝ちなのか負けなのかを返してやる。
                var human_vs_cpu =
                    Player(Color.BLACK).PlayerType == PlayerTypeEnum.Human ^
                    Player(Color.WHITE).PlayerType == PlayerTypeEnum.Human;
                if (human_vs_cpu)
                {
                    game_result = lastMove.GameResult(); // 手番側から見た勝敗
                    // 手番側が人間でないなら勝敗を反転させてやる。
                    if (Player(stm).PlayerType != PlayerTypeEnum.Human)
                        game_result = game_result.Not();
                }
                // 中断だけ別の画面素材を用意してあるので、この判定は特別に行う。
                if (lastMove == Move.INTERRUPT)
                    game_result = MoveGameResult.INTERRUPT;

                RaisePropertyChanged("GameEndEvent" , game_result);

            }

            GameMode = GameModeEnum.ConsiderationWithoutEngine;
            continuousGame.EndTime = DateTime.Now;

            // 時間消費、停止
            foreach (var c in All.Colors())
                PlayTimer(c).StopTimer();

            // 棋譜の自動保存
            AutomaticSaveKifu( lastMove );

            // 連続対局が設定されている時はDisconnect()はせずに、ここで次の対局のスタートを行う。
            continuousGame.IncPlayCount();
            if (continuousGame.MustRestart())
            {
                GameRestart();
                return;
            }

            // 棋譜ウィンドウ、勝手に書き換えられると困るのでこれでfixさせておく。
            kifuManager.EnableKifuList = false;

            // 連続対局でないなら..
            Disconnect();

            // 手番が変わったことを通知。
            NotifyTurnChanged();
        }

        /// <summary>
        /// エンジンなどの切断処理。
        /// 簡単な終了処理を兼ねている。
        /// </summary>
        private void Disconnect()
        {
            // Playerの終了処理をしてNullPlayerを突っ込んでおく。

            foreach (var c in All.IntColors())
            {
                Players[c].Dispose();
                Players[c] = new NullPlayer();
            }

            // 連続対局のためのカウンターをリセットする。
            continuousGame.ResetCounter();

            if (continuousGame.GameSetting != null)
            {
                GameSetting = continuousGame.GameSetting; // プレイヤーの入れ替えで破壊している可能性があるので復元する。
                //continuousGame.GameSetting = null; // これ次のGameStartまで残ってないと対局者名の表示などで困る。
            }
        }

        /// <summary>
        /// [Worker Thread] : 検討モードに入る。
        /// GameModeのsetterから呼び出される。
        /// </summary>
        /// <param name="nextGameMode">次に遷移するGameMode</param>
        /// <returns>返し値としてfalseを返すとcancel動作</returns>
        private bool StartConsiderationWithEngine(GameModeEnum nextGameMode)
        {
            try
            {
                CanUserMove = true;

                // 検討モード用のプレイヤーセッティングを行う。

                // 検討用エンジン
                //var engineDefineFolderPath = "\\engine\\gpsfish"; // 開発テスト用

                var engineDefineFolderPath =
                    (nextGameMode == GameModeEnum.ConsiderationWithEngine)     ? TheApp.app.Config.ConsiderationEngineSetting.EngineDefineFolderPath :
                    (nextGameMode == GameModeEnum.ConsiderationWithMateEngine) ? TheApp.app.Config.MateEngineSetting.EngineDefineFolderPath :
                    null;

                var engineDefineEx = TheApp.app.EngineDefines.Find(x => x.FolderPath == engineDefineFolderPath);

                if (engineDefineEx == null)
                    throw new Exception("検討用エンジンが存在しません。\r\n" +
                        "EngineDefineFolderPath = " + engineDefineFolderPath);

                {
                    // 検討モードの名前はエンジン名から取得
                    // →　ただし、棋譜を汚してはならないので棋譜の対局者名には反映しない。

                    var engineDefine = engineDefineEx.EngineDefine;

                    //var engineName = engineDefine.DisplayName;
                    //setting.PlayerSetting(Color.BLACK).PlayerName = engineName;
                    //setting.PlayerSetting(Color.BLACK).IsCpu = true;

                    switch (nextGameMode)
                    {
                        // 検討用エンジン
                        case GameModeEnum.ConsiderationWithEngine:
                            Players[0 /*検討用のプレイヤー*/ ] = PlayerBuilder.Create(PlayerTypeEnum.UsiEngine);
                            break;

                        // 詰将棋エンジン
                        case GameModeEnum.ConsiderationWithMateEngine:
                            Players[0 /* 詰将棋用のプレイヤー */] = PlayerBuilder.Create(PlayerTypeEnum.UsiEngine);
                            break;
                    }
                }

                // 局面の設定
                kifuManager.EnableKifuList = false;

                // 検討用エンジンの開始

                var usiEnginePlayer = Players[0] as UsiEnginePlayer;
                InitUsiEnginePlayer(Color.BLACK, usiEnginePlayer, engineDefineEx, 0, nextGameMode, false);

                // エンジンに与えるHashSize,Threadsの計算
                if (UsiEngineHashManager.CalcHashSize() != 0)
                    // Hash足りなくてダイアログ出した時にキャンセルボタン押されとる
                    // すぐ下でcatchされるので心配いらない。
                    throw new Exception("");

                // エンジンを開始させることが確定したので実際に子プロセスとして起動する。
                StartEngine(usiEnginePlayer, engineDefineEx);

                // 検討ウィンドウへの読み筋などのリダイレクトを設定
                InitEngineConsiderationInfo(nextGameMode);

                return true;

            } catch (Exception ex)
            {
                if (!ex.Message.Empty())
                    TheApp.app.MessageShow(ex.Message , MessageShowType.Error);
                Disconnect();

                // 失敗。GameModeの状態遷移をcancelすべき。
                return false;
            }
        }

        /// <summary>
        /// [Worker Thread] : 検討モードを抜けるコマンド
        /// GameModeのsetterから呼び出される。
        /// </summary>
        private void EndConsideration()
        {
            // disconnect the consideration engine
            Disconnect();
        }

        /// <summary>
        /// 棋譜の自動保存処理
        /// </summary>
        private void AutomaticSaveKifu(Move lastMove)
        {
            try
            {
                var lastColor = Position.sideToMove;

                // 1) 勝敗のカウンターを加算
                // (これは棋譜の自動保存が無効であってもなされなくてはならない)

                continuousGame.IncResult(lastMove, lastColor);

                // この対局棋譜を保存しなければならないなら保存する。
                var setting = TheApp.app.Config.GameResultSetting;
                if (!setting.AutomaticSaveKifu)
                    return;

                // 2) 棋譜ファイルを保存する。

                // プレイヤー名を棋譜上に反映させる。
                // →　これは、DisplayName()と同等であればすでに設定されている。

                var kifu = kifuManager.ToString(setting.KifuFileType);
                var filename = $"{continuousGame.GetKifuSubfolder()}{DefaultKifuFileName()}{setting.KifuFileType.ToExtensions()}";
                var filePath = Path.Combine(setting.KifuSaveFolder, filename);

                FileIO.WriteFile(filePath, kifu);

                // 3) csvファイルに情報をappendする。

                var table = new GameResultTable();
                var csv_path = setting.CsvFilePath();
                var handicapped = Position.Handicapped;
                var timeSettingStrings = !handicapped ?
                    $"先手:{TimeSettingString(Color.BLACK)},後手:{TimeSettingString(Color.WHITE)}" :
                    $"下手:{TimeSettingString(Color.BLACK)},上手:{TimeSettingString(Color.WHITE)}";

                var result = new GameResultData()
                {
                    PlayerNames = new[] { DisplayNameWithPreset(Color.BLACK), DisplayNameWithPreset(Color.WHITE) },
                    StartTime = continuousGame.StartTime,
                    EndTime = continuousGame.EndTime,
                    KifuFileName = filename,
                    LastMove = lastMove,
                    LastColor = lastColor,
                    GamePly = Position.gamePly - 1 /* 31手目で詰まされている場合、棋譜の手数としては30手であるため。 */,
                    BoardType = kifuManager.Tree.rootBoardType,
                    TimeSettingString = timeSettingStrings,
                    Handicapped = handicapped,
                    Comment = null,
                };
                table.AppendLine(csv_path, result);

                // 連続対局の最終局であるなら、連続対局のトータルの結果を出力する。

                if (continuousGame.IsLastGame())
                {
                    // これが連続対局の最終局であったのなら、結果を書き出す。
                    result = new GameResultData()
                    {
                        Comment = continuousGame.GetGameResultString()
                    };
                    table.AppendLine(csv_path, result);
                }
            } catch (Exception ex)
            {
                // ファイルの書き出しに失敗などで例外が出て落ちるのはちょっと格好が悪いので捕捉しておく。
                TheApp.app.MessageShow(ex, false);
            }
        }

#endregion
    }
}
