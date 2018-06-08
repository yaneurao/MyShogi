using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// 思考エンジンとやりとりするためのクラス
    /// エンジンはUSIプロトコルでやりとりする。
    /// </summary>
    public class UsiEngineNegotiator
    {
        public UsiEngineNegotiator()
        {
            state = UsiEngineState.Init;
        }

        /// <summary>
        /// 思考エンジンに接続する。
        /// </summary>
        public void Connect(UsiEngineData engineData)
        {
            lock (lockObject)
            {
                if (engineData == null)
                {
                    throw new ArgumentNullException("engineData");
                }

                var info = new ProcessStartInfo
                {
                    FileName = engineData.ExeFilePath,
                    WorkingDirectory = engineData.ExeWorkingDirectory,
                    Arguments = engineData.ExeArguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
                };

                var process = new Process
                {
                    StartInfo = info,
                };
                process.Start();

                // Exe用のプロセスをこっそり設定します。
                this.exeProcess = process;

                this.readStream = process.StandardOutput.BaseStream;
                this.writeStream = process.StandardInput.BaseStream;

                // 別スレッドで受信処理を開始する。
                worker_valid = true;
                var thread = new Thread(thread_worker);
                thread.Start();
            }
        }

        /// <summary>
        /// 接続している思考エンジンを切断する。
        /// 実行中であっても強制的に切断して大丈夫なはず…。(エンジン側がきちんと終了処理をするはず..)
        /// </summary>
        public void Disconnect()
        {
            lock (lockObject)
            {
                if (exeProcess != null)
                {
                    exeProcess.Close();
                    exeProcess = null;
                }
                readCts.Cancel();
                worker_valid = false;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public UsiEngineState state { get; set; }

        // --- 以下private methods

        // 別スレッドで受信処理を行う。
        private void thread_worker()
        {
            // 受信バッファ
            var readBuffer = new byte[1024];

            try
            {
                while (worker_valid)
                {
                    readStream.ReadAsync(readBuffer, 0, readBuffer.Length, readCts.Token)
                        .ContinueWith(HandleRecvCommand);

                }
            } catch (Exception /*ex*/)
            {
                //NotifyDisconnected();
            }
        }

        /// <summary>
        /// readStreamに文字列を受信したときに非同期に呼び出されるハンドラ
        /// </summary>
        /// <param name="task"></param>
        private void HandleRecvCommand(Task<int> task)
        {

        }

        // --- 以下private members

        /// <summary>
        /// Connect()で接続したプロセス
        /// </summary>
        private Process exeProcess;

        /// <summary>
        /// 子プロセスからリダイレクトしている入力stream
        /// </summary>
        private Stream readStream;

        /// <summary>
        /// 子プロセスへの出力stream
        /// </summary>
        private Stream writeStream;

        /// <summary>
        /// 別スレッドを終了させるためのシグナル
        /// これがfalseになると終了する。
        /// </summary>
        private bool worker_valid = false;

        /// <summary>
        /// ReadAsync()の途中キャンセル用。
        /// </summary>
        private CancellationTokenSource readCts = new CancellationTokenSource();

        /// <summary>
        /// 排他用
        /// </summary>
        private object lockObject = new object();


    }
}
