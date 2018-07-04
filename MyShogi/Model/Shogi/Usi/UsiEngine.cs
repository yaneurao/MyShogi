using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Process;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// USI engineとのやりとりを抽象化するクラス
    /// 
    /// 読み筋をNotifyObjectを使って通知した時、UIスレッドに渡されて描画される。
	/// この時、次の局面に行っていないことを保証される。
    /// なぜなら、BeginInvoke()で呼び出すとき、UIスレッドのメソッドが呼び出される順が入れ替わることはなく、
    /// 最初に局面の初期化コマンド(EngineInfoのSetRootSfen)から、PV送信(EngineInfoのEngineConsiderationPvData)の
    /// 送信順は保証されるため、異なる局面のPVを検討ウィンドウが表示してしまうということはない。

    /// </summary>
    public class UsiEngine : NotifyObject
    {
        public UsiEngine()
        {
            State = UsiEngineState.Init;
        }

        /// <summary>
        /// 思考エンジンに接続する。
        /// </summary>
        /// <param name="data"></param>
        public void Connect(ProcessNegotiatorData data)
        {
            Disconnect(); // 前の接続があるなら切断する。

            try
            {
                negotiator = new ProcessNegotiator();
                negotiator.Connect(data);
                negotiator.CommandReceived += UsiCommandHandler;
                // ProcessNegotiator.Read()を呼び出すまではハンドラの処理が行われないので、
                // この形で書いても、最初のメッセージを取りこぼすことはない。

                ChangeState(UsiEngineState.Connected);
            }
            catch
            {
                // 思考エンジンに接続できないとき、Win32Exceptionが飛んでくる
                ChangeState(UsiEngineState.ConnectionFailed);
            }
        }

        /// <summary>
        /// エンジンに対して応答をしたい時に定期的に呼び出す。
        /// これを呼び出したスレッドで処理される。
        /// </summary>
        public void OnIdle()
        {
            switch (State)
            {
                case UsiEngineState.Connected:
                    if (DateTime.Now - connected_time >= new TimeSpan(0, 0, 30))
                    {
                        ChangeState(UsiEngineState.ConnectionTimeout);
                    }
                    break;
            }

            negotiator.Read();
        }

        /// <summary>
        /// エンジンに思考させる。
        /// Thinkingのときにこのmethodの呼び出しは不正であるものとする。
        /// 呼び出し側できちんと管理すべし。
        /// </summary>
        /// <param name="usiPositionString"></param>
        public void Think(string usiPositionString)
        {
#if false
            if (thinking)
            {
                // これ呼び出し側がまずい気がする。

                // ponderなどで思考させている途中で次の思考開始命令が来たので、stopを送信する。
                SendCommand("stop");

                // ここで"bestmove"が返ってくるのを待つ必要があるがこの仕組みではそれができない。あとで考える。
            }
#endif
            Debug.Assert(Thinking == false);

            Thinking = true;
            SendCommand($"position {usiPositionString}");
            SendCommand("go btime 10000 wtime 10000 byoyomi 1000"); // 1手1秒でとりあえず指させる。
        }

        /// <summary>
        /// いますぐに指させる。
        /// go ponderに対して bestmoveが欲しいときにもこれを用いる。
        /// </summary>
        public void MoveNow()
        {
            // 思考中であれば、stopコマンドを送信することで思考を中断できる(はず)
            if (Thinking)
                SendCommand("stop");
        }

        /// <summary>
        /// エンジンに対してコマンドを送信する。(デバッグ用)
        /// 普段は、このクラスが自動的にやりとりをするので外部からこのメソッドを呼び出すことはない。(はず)
        /// 
        /// 基本的にノンブロッキングだと考えられる。
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(string command)
        {
            negotiator.Write(command);
        }

        public void Disconnect()
        {
            if (State != UsiEngineState.Init)
                SendCommand("quit");

            if (negotiator != null)
            {
                negotiator.Dispose();
                negotiator = null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        // -- public members

        /// <summary>
        /// エンジンから受け取ったoptionの一覧
        /// </summary>
        public List<UsiOption> OptionList { get; set; } = new List<UsiOption>();

        /// <summary>
        /// デフォルトのオプション一覧を取得または設定します。
        /// </summary>
        /// <remarks>
        /// エンジン接続後のオプション一覧取得後に値を更新するために使います。
        /// それ以外で使われることはありません。
        /// 
        /// Connect()を呼び出す前にsetしておくべき。
        /// </remarks>
        public List<UsiOptionMin> DefaultOptionList { get; set; }

        /// <summary>
        /// エンジンの設定ダイアログ用であるか？
        /// Connect()の前に設定すべし。
        /// 
        /// これがtrueだと"usi"コマンドは送信するが、"isready"コマンドの送信はしない。
        /// これがfalseだと"usi"送信→"usiok"受信→"isready"送信→"readyok"受信→"usinewgame"送信 まで待つ。
        /// </summary>
        public bool EngineSetting { get; set; }

        // -- 以下、engine側から渡された情報など

        /// <summary>
        /// エンジンのオリジナル名を取得または設定します。
        /// "id name ..."と渡されたもの。
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// エンジンの開発者名を取得または設定します。
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// エンジンから送られてきた文字列の解釈などでエラーがあった場合に、
        /// ここに格納される。
        /// </summary>
        public Exception exception { get; private set; }

        /// <summary>
        /// USIプロトコルによってengine側から送られてきた"bestmove .."を解釈した指し手
        /// </summary>
        public Move BestMove { get; set; }

        /// <summary>
        /// USIプロトコルによってengine側から送られてきた"bestmove .. ponder .."のponderで指定された指し手を解釈した指し手
        /// </summary>
        public Move PonderMove { get; set; }

        /// <summary>
        /// 現在思考中であるかどうかのフラグ
        /// Thinking == true && BestMove == Move.NONE なら、思考中である。
        /// Thinking == true && BestMove != Move.NONE は、ありえない(無視すべき)
        /// Thinking == true && exception != null なら、例外が発生した。
        /// Thinking == false && BestMove != Move.NONEなら思考が終了して思考結果が返ってきている。
        /// Thinking == false && BestMove == Move.NONEなら思考結果取り出したあとの通常状態。
        /// </summary>
        public bool Thinking { get; set; }

        /// <summary>
        /// 読み筋。
        /// USIプロトコルの"info ..."をparseした内容が入る。
        /// 親では、このイベントを捕捉すれば良い。
        /// </summary>
        public UsiThinkReport ThinkReport
        {
            get { return GetValue<UsiThinkReport>("ThinkReport");}
            set { SetValue<UsiThinkReport>("ThinkReport",value); }
        }

        // -- private members

        private ProcessNegotiator negotiator;

        /// <summary>
        /// エンジンの状態。
        /// </summary>
        private UsiEngineState State
        {
            get { return GetValue<UsiEngineState>("State"); }
            set { SetValue<UsiEngineState>("State", value); }
        }
        
        /// <summary>
        /// "usi"コマンドを思考ンジンに送信した時刻。思考エンジンは起動時にすぐに応答するように作るべき。
        /// 一応、タイムアウトを監視する。
        /// </summary>
        private DateTime connected_time;

        // -- private methods

        private void ChangeState(UsiEngineState state)
        {
            switch(state)
            {
                case UsiEngineState.Connected:
                    // 接続されたので"usi"と送信
                    // これ、接続タイムアウトがある。
                    connected_time = DateTime.Now;
                    SendCommand("usi");
                    break;

                case UsiEngineState.UsiOk:
                    // このタイミングで状態が変更になったことを通知すべき
                    // エンジン設定時には、この状態で待たせておけば良い。

                    // 対局時には、このあと "isready"を送信して readyokを待つ。
                    if (!EngineSetting)
                    {
                        // これ時間制限はなく、タイムアウトは監視しない。どこかでisreadyは完了するものとする。
                        SendCommand("isready");
                    }
                    break;

                case UsiEngineState.InTheGame:
                    // "readyok"が来たのでusinewgameを送信して、対局の局面を送信できるようにしておく。
                    SendCommand("usinewgame");
                    break;
            }
            var oldState = State;
            State = state;

            Log.Write(LogInfoType.UsiServer,$"ChangeState()で{oldState.ToString()}から{state.ToString()}に状態が変化した。");
        }


        /// <summary>
        /// 思考エンジンの標準出力から送られてきたコマンドの解釈用
        /// </summary>
        /// <param name="command"></param>
        private void UsiCommandHandler(string command)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(command))
                {
                    return;
                }

                // 前後の空白は削除しておきます。
                var trimmedCommand = command.Trim();
                //Log.Info("{0}> {1}", LogName, trimmedCommand);

                var scanner = new Scanner(trimmedCommand);

                switch (scanner.ParseText())
                {
                    case "usiok":
                        HandleUsiOk();
                        break;

                    case "readyok":
                        HandleReadyOk();
                        break;

                    case "id":
                        HandleId(scanner);
                        break;

                    case "option":
                        HandleOption(scanner);
                        break;

                    case "bestmove":
                        HandleBestMove(scanner);
                        break;

                    case "info":
                        HandleInfo(scanner);
                        break;

                    // u2bやBonadapterのためのスペシャルコマンド
                    case "B<":
                        break;

                    default:
                        //Log.Error("unknown usi command: {0}", trimmedCommand);
                        break;
                }
            } catch (Exception ex)
            {
                // 例外をログに出力しておく。
                Log.Write(LogInfoType.UsiParseError, $"例外が発生しました。: {ex.Message}");
            }
        }

        /// <summary>
        /// usiok コマンドを処理します。
        /// </summary>
        private void HandleUsiOk()
        {
            if (State != UsiEngineState.Connected)
            {
                throw new UsiException(
                    "usiコマンドが不正なタイミングで送られました。");
            }

            // "usiok"に対してエンジン設定などを渡してやる。

            ComplementOptions();
            LoadDefaultOption();
            ChangeState(UsiEngineState.UsiOk);
        }

        /// <summary>
        /// 必要ならUSI_PonderやUSI_Hashなど必要なオプションを追加します。
        /// </summary>
        private void ComplementOptions()
        {
            // 判定はオプション名のみで行います。
            if (!OptionList.Any(_ => _.Name == UsiOption.USI_Hash.Name))
            {
                OptionList.Insert(0, UsiOption.USI_Hash.Clone());
            }

            if (!OptionList.Any(_ => _.Name == UsiOption.USI_Ponder.Name))
            {
                OptionList.Insert(0, UsiOption.USI_Ponder.Clone());
            }
        }

        /// <summary>
        /// デフォルトオプション一覧と取得したオプションとの
        /// 整合性を取りながら、オプション値を更新します。
        /// </summary>
        private void LoadDefaultOption()
        {
            // デフォルトオプションが設定されていないなら、この処理はskipする。
            if (DefaultOptionList == null)
                return;

            foreach (var option in OptionList)
            {
                if (option.OptionType == UsiOptionType.None ||
                    option.OptionType == UsiOptionType.Button)
                {
                    continue;
                }

                // 名前と型が同じオプションを保存済みオプションから検索します。
                var saved = DefaultOptionList.FirstOrDefault(_ =>
                    _.Name == option.Name &&
                    _.OptionType == option.OptionType);
                if (saved == null)
                {
                    continue;
                }

                // 例外は握りつぶします。
                //Util.SafeCall(() => option.SetDefault(saved.Value));
            }

            SendSetOptionList();
        }

        /// <summary>
        /// setoptionコマンドをまとめて送信します。
        /// </summary>
        public void SendSetOptionList()
        {
            var list = OptionList
                .Where(_ => _.OptionType != UsiOptionType.Button)
                .Select(_ => _.MakeSetOptionCommand())
                .Where(_ => !string.IsNullOrEmpty(_))
                //.Select(_ => _ + '\n')
                .ToArray();

            // 応答を待つ必要はない。どんどん流し込む。
            foreach (var command in list)
                SendCommand(command);
        }

        /// <summary>
        /// readyok コマンドを処理します。
        /// </summary>
        private void HandleReadyOk()
        {
            if (State != UsiEngineState.UsiOk)
            {
                throw new UsiException(
                    "readyokコマンドが不正なタイミングで送られました。");
            }

            // 読み込みが終わったタイミングでエンジンの優先度を下げます。
            negotiator.UpdateProcessPriority();

            ChangeState(UsiEngineState.InTheGame);
        }

        /// <summary>
        /// id name などのコマンドを処理します。
        /// </summary>
        private void HandleId(Scanner scanner)
        {
            switch (scanner.ParseWord())
            {
                case "name":
                    OriginalName = scanner.LastText;
                    break;
                case "author":
                case "auther": // typoも受け入れる
                    Author = scanner.LastText;
                    break;
                default:
                    throw new UsiException(
                        "invalid command: " + scanner.Text);
            }
        }

        /// <summary>
        /// option コマンドを処理します。
        /// </summary>
        private void HandleOption(Scanner scanner)
        {
            var option = UsiOption.Parse(scanner.Text);

            // "usi"は一度しか送っていないので同じ名前のoptionが二度送られてくることは想定しなくて良いはずなのだが、
            // 一応きちんと処理しておく。
            AddOption(option);
        }

        /// <summary>
        /// OptionListにoptionを追加する。
        /// nameが重複していれば追加せずに置き換える。
        /// </summary>
        /// <param name="option"></param>
        private void AddOption(UsiOption option)
        {
            for (int i = 0; i < OptionList.Count; ++i)
                if (OptionList[i].Name == option.Name)
                {
                    OptionList[i] = option;
                    return;
                }

            OptionList.Add(option);
        }

        /// <summary>
        /// bestmove コマンドを処理します。
        /// </summary>
        private void HandleBestMove(Scanner scanner)
        {
            try
            {
                // 指し手が返ってきた以上、思考中ではない。
                Thinking = false;

                Move move = Move.NONE , ponder = Move.NONE;
                var moveSfen = scanner.ParseText();

                // まず、特殊な指し手を調べます。
                switch (moveSfen)
                {
                    case "resign":
                        move = Move.RESIGN;
                        break;
                    case "win":
                        move = Move.WIN;
                        break;
                }

                // 上記に該当しなかった。
                if (move == Move.NONE)
                {
                    move = Core.Util.FromUsiMove(moveSfen);
                    if (move == Move.NONE)
                    {
                        // 解釈できない文字列
                        throw new UsiException(
                            moveSfen + ": SFEN形式の指し手が正しくありません。");
                    }
                }

                // 後続があって、"ponder"と書いてある。
                if (!scanner.IsEof)
                {
                    if (scanner.ParseText() != "ponder")
                    {
                        // "ponder"以外はこれないはずなのに…。
                        throw new UsiException(
                            "invalid command: " + scanner.Text);
                    }

                    // ponderの指し手は'(null)'などが指定されることもあるので、
                    // 指せなくてもエラーにはしません。
                    var ponderSfen = scanner.ParseText();
                    ponder = Core.Util.FromUsiMove(ponderSfen);
                }

                // 確定したので格納しておく。
                BestMove = move;
                PonderMove = ponder;

            }
            catch (UsiException ex)
            {
                // 例外を出力しておく。
                Log.Write(LogInfoType.UsiParseError, $"例外が発生しました。: {ex.Message}");
            }
        }

        /// <summary>
        /// infoコマンドを処理します。
        /// </summary>
        private void HandleInfo(Scanner scanner)
        {
            try
            {
                var info = new UsiThinkReport();
                var parseEnd = false;
                while (!scanner.IsEof && !parseEnd)
                {
                    switch (scanner.ParseText())
                    {
                        // hash使用率 1000分率返ってくるので10で割って100分率に変換して代入する。
                        case "hashfull":
                            info.HashPercentage = (float)scanner.ParseInt() / 10.0f;
                            break;

                        // nps
                        case "nps":
                            info.Nps = scanner.ParseInt();
                            break;

                        // 現在の探索手
                        case "currmove":
                            info.CurrentMove = scanner.ParseText();
                            break;

                        // 探索ノード数
                        case "nodes":
                            info.Nodes = scanner.ParseInt();
                            break;

                        // 探索深さ,選択探索深さ

                        // ここに文字が入っている可能性があるので(upperbound/lowerboundを表現するための"↑"など)文字列として扱う。
                        case "depth":
                            info.Depth = scanner.ParseText();
                            break;

                        case "seldepth":
                            info.SelDepth = scanner.ParseText();
                            break;

                        case "score":
                            info.Eval = HandleInfoScore(scanner);
                            break;

                        case "pv":
                            info.Moves = HandlePVSeq(scanner);
                            parseEnd = true; // "pv"はそのあと末尾まで。
                            break;

                        // リポート情報のみ更新
                        case "time":
                            info.ElapsedTime = TimeSpan.FromMilliseconds(scanner.ParseInt());
                            break;

                        case "multipv":
                            info.MultiPV = (int)scanner.ParseInt();
                            break;

                        case "string":
                            info.InfoString = scanner.LastText; // 残り全部
                            parseEnd = true;
                            break;
#if false
                        case "count":
                            GodwhaleCount = scanner.ParseInt();
                            break;
                        case "ranking":
                            GodwhaleRank = scanner.ParseInt();
                            break;

                        // 無視
                        case "currmovenumber":
                        case "cpuload":
                        case "refutation":
                        case "currline":
                        case "id": // クジラちゃん用
                            scanner.ParseText();
                            break;
#endif

                        // エラー
                        default:
                            throw new Exception();
                    }
                }

                ThinkReport = info;
            } catch
            {
                throw new UsiException("info 文字列の解析に失敗 : " + scanner.Text);
            }
        }

        /// <summary>
        /// USIのPVの文字列を構築する。
        /// </summary>
        /// <param name="scanner"></param>
        /// <returns></returns>
        private List<Move> HandlePVSeq(Scanner scanner)
        {
            var list = new List<Move>();

            while (!scanner.IsEof)
            {
                var move = Core.Util.FromUsiMove(scanner.ParseText());
                if (move == Move.NONE)
                    break;
                list.Add(move);
            }
            return list;
        }

        /// <summary>
        /// "info .. score xxx"の"score"の直後の文字列をparseする
        /// </summary>
        /// <param name="scanner"></param>
        /// <returns></returns>
        private EvalValueEx HandleInfoScore(Scanner scanner)
        {
            var bound = ScoreBound.Exact;
            switch (scanner.ParseText())
            {
                case "cp":
                    var valueText = scanner.ParseText();

                    // lowerbound/upperboundを取得
                    var peek = scanner.PeekText(); // peekします
                    if (peek == "upperbound")
                    {
                        bound = ScoreBound.Upper;
                        scanner.ParseText();
                    }
                    else if (peek == "lowerbound")
                    {
                        bound = ScoreBound.Lower;
                        scanner.ParseText();
                    }

                    // ここでエンジンによっては、"120↑"のような表現がありうる。
                    // "upperbound","lowerbound"がサポートされていなかったころの名残。
                    // ここではそれを許容しない。

                    return new EvalValueEx((EvalValue)int.Parse(valueText) , bound);

                case "mate":
                    return new EvalValueEx( ParseMate(scanner.ParseText()) , bound);

                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// 詰みになったときの手数をパースします。
        /// </summary>
        /// <example>
        /// +
        /// -10
        /// +5↑
        /// </example>
        public static EvalValue ParseMate(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            var trimmedText = text.Trim();
            var value = int.Parse(trimmedText);

            if (value == 0)
            {
                if (trimmedText[0] == '+')
                {
                    return EvalValue.MatePlus;
                }
                else if (trimmedText[0] == '-')
                {
                    return EvalValue.MatedMinus;
                }
                else
                {
                    //throw new ShogiException(
                    //    trimmedText + ": メイト手数が正しくありません。");

                    // 本来は先頭に+/-が必要ですが、そうなっていないソフトも多いので
                    // ここでは現状に合わせてエラーにはしないことにします。
                    return EvalValue.Mate;
                }
            }
            else if (value > 0)
                return EvalValue.Mate - value;
            else
                return EvalValue.Mated - value;
        }

    }
}
