using MyShogi.Model.Common.Process;
using MyShogi.Model.Common.Utility;
using System.Collections.Generic;
using System.Linq;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// USI engineとのやりとりを抽象化するクラス
    /// </summary>
    public class UsiEngine
    {
        /// <summary>
        /// 思考エンジンに接続する。
        /// </summary>
        /// <param name="data"></param>
        public void Connect(ProcessNegotiatorData data)
        {
            Disconnect(); // 前の接続があるなら切断する。

            negotiator = new ProcessNegotiator();
            negotiator.Connect(data);
            negotiator.CommandReceived += UsiCommandHandler;
            // ProcessNegotiator.Read()を呼び出すまではハンドラの処理が行われないので、
            // この形で書いても、最初のメッセージを取りこぼすことはない。
        }

        /// <summary>
        /// エンジンに対して応答をしたい時に定期的に呼び出す。
        /// これを呼び出したスレッドで処理される。
        /// </summary>
        public void OnIdle()
        {
            negotiator.Read();
        }

        public void Disconnect()
        {
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

        // -- private members

        private ProcessNegotiator negotiator;

        private UsiEngineState State {get;set;}


        // -- private methods

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

#if false
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
#if false
            if (State != UsiEngineState.Connected)
            {
                throw new UsiException(
                    "usiコマンドが不正なタイミングで送られました。");
            }
#endif

            ComplementOptions();
            LoadDefaultOption();
            //EndGoState(UsiEngineState.UsiOk);
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

    }
}
