using System;
using System.Collections.Generic;
using System.Threading;
using MyShogi.App;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Resource.Sounds;
using MyShogi.Model.Shogi.Core;
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
        /// </summary>
        private void thread_worker()
        {
            try
            {
                while (!workerStop)
                {
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

                    // 10msごとに各種処理を行う。
                    Thread.Sleep(10);
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
            // 以下の初期化中に駒が動かされるの気持ち悪いのでユーザー操作を禁止しておく。
            CanUserMove = false;
            Initializing = true;
            var nextGameMode = GameModeEnum.InTheGame;

            // 音声:「よろしくお願いします。」
            TheApp.app.soundManager.Stop(); // 再生中の読み上げをすべて停止
            TheApp.app.soundManager.ReadOut(SoundEnum.Start);

            // 初回の指し手で、「先手」「後手」と読み上げるためのフラグ
            sengo_read_out = new bool[2] { false, false };

            // プレイヤーの生成
            UsiEngineHashManager.Init();
            foreach (var c in All.Colors())
            {
                var gamePlayer = gameSetting.PlayerSetting(c);
                var playerType = gamePlayer.IsHuman ? PlayerTypeEnum.Human : PlayerTypeEnum.UsiEngine;
                Players[(int)c] = PlayerBuilder.Create(playerType);

                if (playerType == PlayerTypeEnum.UsiEngine)
                {
                    var engineDefineEx = TheApp.app.EngineDefines.Find(x => x.FolderPath == gamePlayer.EngineDefineFolderPath);
                    if (engineDefineEx == null)
                    {
                        TheApp.app.MessageShow("エンジンがありませんでした。" + gamePlayer.EngineDefineFolderPath , MessageShowType.Error);
                        return;
                    }
                    var usiEnginePlayer = Players[(int)c] as UsiEnginePlayer;
                    var ponder = gamePlayer.Ponder;
                    InitUsiEnginePlayer(c , usiEnginePlayer, engineDefineEx, gamePlayer.SelectedEnginePreset, nextGameMode , ponder);
                }
            }

            // 局面の設定
            kifuManager.EnableKifuList = true;
            if (gameSetting.BoardSetting.BoardTypeCurrent)
            {
                // 現在の局面からなので、いま以降の局面を削除する。
                // ただし、いまの局面と棋譜ウィンドウとが同期しているとは限らない。
                // まず現在局面以降の棋譜を削除しなくてはならない。

                // 元nodeが、special moveであるなら、それを削除しておく。
                if (kifuManager.Tree.IsSpecialNode())
                    kifuManager.Tree.UndoMove();

                kifuManager.Tree.ClearForward();

                // 分岐棋譜かも知れないので、現在のものを本譜の手順にする。
                kifuManager.Tree.MakeCurrentNodeMainBranch();
            }
            else // if (gameSetting.Board.BoardTypeEnable)
            {
                kifuManager.Init();
                kifuManager.InitBoard(gameSetting.BoardSetting.BoardType);
            }

            // 本譜の手順に変更したので現在局面と棋譜ウィンドウのカーソルとを同期させておく。
            UpdateKifuSelectedIndex();

            // エンジンに与えるHashSize,Threadsの計算
            if (UsiEngineHashManager.CalcHashSize() != 0)
            {
                // Hash足りなくてダイアログ出した時にキャンセルボタン押されとる
                Disconnect();

                // ゲームが終局したことを通知するために音声があったほうがよさげ。
                TheApp.app.soundManager.ReadOut(SoundEnum.End);

                return;
            }

            // 現在の時間設定を、KifuManager.Treeに反映させておく(棋譜保存時にこれが書き出される)
            kifuManager.Tree.KifuTimeSettings = gameSetting.KifuTimeSettings;

            // 対局者氏名の設定
            // 人間の時のみ有効。エンジンの時は、エンジン設定などから取得することにする。
            foreach (var c in All.Colors())
            {
                var player = Player(c);
                string name;
                switch (player.PlayerType)
                {
                    case PlayerTypeEnum.Human:
                        name = gameSetting.PlayerSetting(c).PlayerName;
                        break;

                    default:
                        name = c.Pretty();
                        break;
                }

                kifuManager.KifuHeader.SetPlayerName(c, name);
            }

            // 持ち時間などの設定が必要なので、コピーしておく。
            GameSetting = gameSetting;

            // 消費時間計算用
            foreach (var c in All.Colors())
            {
                var pc = PlayTimer(c);
                pc.KifuTimeSetting = GameSetting.KifuTimeSettings.Player(c);
                pc.GameStart();
                timeSettingStrings[(int)c] = pc.KifuTimeSetting.ToShortString();
            }

            // rootの持ち時間設定をここに反映させておかないと待ったでrootまで持ち時間が戻せない。
            // 途中の局面からだとここではなく、現局面のところに書き出す必要がある。
            kifuManager.Tree.SetKifuMoveTimes(PlayTimers.GetKifuMoveTimes());

            // コンピュータ vs 人間である場合、人間側を手前にしてやる。
            // 人間 vs 人間の場合も最初の手番側を手前にしてやる。
            var stm = kifuManager.Position.sideToMove;
            // 1. 手番側が人間である場合(非手番側が人間 or CPU)
            if (gameSetting.PlayerSetting(stm).IsHuman)
                BoardReverse = (stm == Color.WHITE);
            // 2. 手番側がCPUで、非手番側が人間である場合。
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

            presetNames[(int)c] = (selectedPresetIndex < presets.Count) ?
                presets[selectedPresetIndex].Name :
                null;

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
            UsiEngineHashManager.SetValue(c, engineDefineEx , config, ponder);
            
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
            usiEnginePlayer.Start(engineDefine.EngineExeFileName());

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
                if (GameSetting.PlayerSetting(c).IsCpu ||
                    (c == Color.BLACK && nextGameMode.IsConsiderationWithEngine()) // 検討用エンジンがぶら下がっている。
                    )
                {
                    var num_ = num; // copy for capturing

                    var engineName = GetEngineDefine(c).EngineDefine.DisplayName;
                    var engineName2 = PresetName(c) == null ? engineName : $"{engineName} {PresetName(c)}";
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

                            // このクラスのpropertyのsetterを呼び出してメッセージを移譲してやる。
                            ThinkReport = new UsiThinkReportMessage()
                            {
                                type = UsiEngineReportMessageType.UsiThinkReport,
                                number = num_, // is captrued
                                data = report,
                            };
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
            if ((gameMode == GameModeEnum.InTheGame && TheApp.app.config.PieceSoundInTheGame != 0) ||
                (gameMode.IsConsideration() && TheApp.app.config.PieceSoundOffTheGame != 0)
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
                var soundManager = TheApp.app.soundManager;
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

            var config = TheApp.app.config;

            // -- 指し手

            Move bestMove;
            if (GameMode.IsConsiderationWithEngine())
            {
                // 検討モードなのでエンジンから送られてきたbestMoveの指し手は無視。
                bestMove = stmPlayer.SpecialMove;
            }
            else
            {
                // TIME_UPなどのSpecialMoveが積まれているなら、そちらを優先して解釈する。
                bestMove = stmPlayer.SpecialMove != Move.NONE ? stmPlayer.SpecialMove : stmPlayer.BestMove;
            }

            if (bestMove != Move.NONE)
            {
                PlayTimer(stm).ChageToThemTurn(bestMove == Move.TIME_UP);

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

                    var soundManager = TheApp.app.soundManager;

                    var kif = kifuManager.KifuList[kifuManager.KifuList.Count - 1];
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
                        soundManager.ReadOut(kif);
                    }

                }

                // -- 次のPlayerに、自分のturnであることを通知してやる。

                if (!specialMove)
                    NotifyTurnChanged();
                else
                    // 特殊な指し手だったので、これにてゲーム終了
                    GameEnd();
            }

            return;

        ILLEGAL_MOVE:

            // これ、棋譜に記録すべき
            Move m = Move.ILLEGAL_MOVE;
            kifuManager.Tree.AddNode(m, PlayTimers.GetKifuMoveTimes());
            kifuManager.Tree.AddNodeComment(m, stmPlayer.BestMove.ToUsi() /* String あとでなおす*/ /* 元のテキスト */);
            kifuManager.Tree.DoMove(m);

            GameEnd(); // これにてゲーム終了。
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
                var misc = TheApp.app.config.GameSetting.MiscSettings;
                Move m = kifuManager.Tree.IsNextNodeSpecialNode(isHuman , misc);

                // 上で判定された特殊な指し手であるか？
                if (m != Move.NONE)
                {
                    // この特殊な状況を棋譜に書き出して終了。
                    kifuManager.Tree.AddNode(m, KifuMoveTimes.Zero);
                    // speical moveでもDoMoveできることは保証されている。
                    kifuManager.Tree.DoMove(m);

                    // 音声の読み上げ
                    TheApp.app.soundManager.ReadOut(m);

                    GameEnd();
                    return;
                }
            }

            // USIエンジンのときだけ、"position"コマンドに渡す形で局面図が必要であるから、
            // 生成して、それをPlayer.Think()の引数として渡してやる。
            var isUsiEngine = stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine;
            string usiPosition = isUsiEngine ? kifuManager.UsiPositionString : null;

            stmPlayer.CanMove = true;
            stmPlayer.SpecialMove = Move.NONE;

            // BestMove,PonderMoveは、Think()以降、正常に更新されることは、Playerクラス側で保証されているので、
            // ここではそれらの初期化は行わない。

            // -- MultiPVの設定

            if (GameMode == GameModeEnum.ConsiderationWithEngine)
                // MultiPVは、GlobalConfigの設定を引き継ぐ
                (stmPlayer as UsiEnginePlayer).Engine.MultiPV = TheApp.app.config.ConsiderationMultiPV;
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
                        var setting = TheApp.app.config.ConsiderationEngineSetting;
                        if (setting.Limitless)
                            limit = UsiThinkLimit.TimeLimitLess;
                        else // if (setting.TimeLimit)
                            limit = UsiThinkLimit.FromSecond(setting.Second);
                    }
                    break;

                case GameModeEnum.ConsiderationWithMateEngine:
                    {
                        var setting = TheApp.app.config.MateEngineSetting;
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
            var stm = Position.sideToMove;
            if (GameMode == GameModeEnum.InTheGame && PlayTimer(stm).IsTimeUp())
                Player(stm).SpecialMove = Move.TIME_UP;

            // エンジンで発生した例外の捕捉
            foreach(var c in All.Colors())
            {
                if (Player(c).PlayerType == PlayerTypeEnum.UsiEngine)
                {
                    var engine = (Player(c) as UsiEnginePlayer).Engine;
                    var ex = engine.Exception;
                    if (ex != null)
                    {
                        TheApp.app.MessageShow($"エンジン側で例外が発生しました。\n例外 : { ex.Message }\nスタックトレース : { ex.StackTrace}",
                            MessageShowType.Error);
                        engine.Exception = null;
                        Player(stm).SpecialMove = Move.INTERRUPT;
                    }
                }
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
        /// </summary>
        private void GameEnd()
        {
            // 対局中だったものが終了したのか？
            if (GameMode == GameModeEnum.InTheGame)
            {
                // 音声:「ありがとうございました。またお願いします。」
                TheApp.app.soundManager.ReadOut(SoundEnum.End);
            }

            GameMode = GameModeEnum.ConsiderationWithoutEngine;

            // 時間消費、停止
            foreach (var c in All.Colors())
                PlayTimer(c).StopTimer();

            // 棋譜ウィンドウ、勝手に書き換えられると困るのでこれでfixさせておく。
            kifuManager.EnableKifuList = false;

            // 連続対局が設定されている時はDisconnect()はせずに、ここで次の対局のスタートを行う。
            // (エンジンを入れ替えたりしないといけない)

            // 連続対局でないなら..
            Disconnect();

            // 手番が変わったことを通知。
            NotifyTurnChanged();
        }

        /// <summary>
        /// エンジンなどの切断処理
        /// </summary>
        private void Disconnect()
        {
            // Playerの終了処理をしてNullPlayerを突っ込んでおく。

            foreach (var c in All.IntColors())
            {
                Players[c].Dispose();
                Players[c] = new NullPlayer();
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
                    (nextGameMode == GameModeEnum.ConsiderationWithEngine)     ? TheApp.app.config.ConsiderationEngineSetting.EngineDefineFolderPath :
                    (nextGameMode == GameModeEnum.ConsiderationWithMateEngine) ? TheApp.app.config.MateEngineSetting.EngineDefineFolderPath :
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
                    throw new Exception("");

                // 検討ウィンドウへの読み筋などのリダイレクトを設定
                InitEngineConsiderationInfo(nextGameMode);

                return true;

            } catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
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

        #endregion
    }
}
