using MyShogi.Model.Common.Process;
using MyShogi.Model.Common.Utility;

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

            negotiator.CommandReceived += UsiCommandHandler;
            negotiator.Connect(data);
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

        // -- private

        private ProcessNegotiator negotiator;

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
#if false
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
#endif
                
                // u2bやBonadapterのためのスペシャルコマンド
                case "B<":
                    break;

                default:
                    //Log.Error("unknown usi command: {0}", trimmedCommand);
                    break;
            }
        }
    }
}
