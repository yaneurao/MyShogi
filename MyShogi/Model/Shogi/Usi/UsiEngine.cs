using MyShogi.Model.Common.Process;
using MyShogi.Model.Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// USI engineとのやりとりを抽象化するクラス
    /// </summary>
    public class UsiEngine
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
            switch(State)
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

        /// <summary>
        /// エンジンに対してコマンドを送信する。(デバッグ用)
        /// 普段は、このクラスが自動的にやりとりをするので外部からこのメソッドを呼び出すことはない。
        /// 
        /// 基本的にノンブロッキングだと考えられる。
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(string command)
        {
            negotiator.Write(command);
        }

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
        public string OriginalName { get; set;}

        /// <summary>
        /// エンジンの開発者名を取得または設定します。
        /// </summary>
        public string Author { get; set; }


        // -- private members

        private ProcessNegotiator negotiator;

        private UsiEngineState State {get;set;}
        
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

#if false
                case "option":
                    HandleOption(scanner);
                    break;
                case "bestmove":
                    HandleBestMove(scanner);
                    break;
                case "info":
                    HandleInfo(scanner);
                    break;
#endif

                // u2bやBonadapterのためのスペシャルコマンド
                case "B<":
                    break;

                default:
                    //Log.Error("unknown usi command: {0}", trimmedCommand);
                    break;
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

    }
}
