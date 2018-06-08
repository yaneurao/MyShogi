using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyShogi.Model.Common.Network
{
    /// <summary>
    /// RemoteService.read()でコマンドを受信したときに呼び出されるハンドラの型
    /// </summary>
    /// <param name="command"></param>
    public delegate void CommandRecieveHandler(string command);

    /// <summary>
    /// 入出力streamを受け取って、送受信を補助する。
    /// </summary>
    public class RemoteService : IDisposable
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RemoteService(Stream readStream, Stream writeStream,
                             bool outLog = true)
        {
            this.readStream = readStream;
            this.writeStream = writeStream;

            LogName = string.Empty;
            IsOutLog = outLog;
            Encoding = Encoding.GetEncoding("Shift_JIS");
        }

        /// <summary>
        /// ログ出力時の名前を取得または設定します。
        /// </summary>
        public string LogName
        {
            get;
            set;
        }

        /// <summary>
        /// 送受信データのログ出力を行うかどうかを取得または設定します。
        /// </summary>
        public bool IsOutLog
        {
            get;
            set;
        }

        /// <summary>
        /// エンコーディングを取得または設定します。
        /// </summary>
        public Encoding Encoding
        {
            get;
            set;
        }

        /// <summary>
        /// read()で例外が起きたときにここにその例外が格納される
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }

        public void Dispose()
        {
            readCts.Cancel();
            if (readStream != null)
            {
                readStream.Close();
                readStream.Dispose();
                readStream = null;
            }

            if (writeStream != null)
            {
                writeStream.Close();
                writeStream.Dispose();
                writeStream = null;
            }
        }

        /// <summary>
        /// 受信処理を行う。
        /// 1行読み込んだら、コールバックが呼び出される。→　NotifyCommandReceived()
        /// 受信バッファ1本なので複数スレッドから呼び出してはならない。
        /// </summary>
        public void Read()
        {
            try
            {
                while (task == null || task.IsCompleted)
                {
                    // read()するbufferにデータが積まれているならその分だけ取得したいのだが、
                    // そういうメソッドがStreamに存在しない。仕方ないのでTaskを同期的に用いる。

                    if (task == null)
                        task = readStream.ReadAsync(readBuffer, 0, readBuffer.Length , readCts.Token);

                    // 前回のタスクが終了しているなら、その結果を取り出す
                    if (task.IsCompleted)
                    {
                        foreach (var command in SplitCommand(readBuffer, task.Result))
                        {
                            //if (IsOutLog)
                            //{
                            //    LibGlobal.DebugFormViewModel.AddLine(command, LogName, true);
                            //    Log.Trace("> {0}", command);
                            //}

                            CommandReceived(command);
                        }
                        task = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Exception = ex;
                //NotifyDisconnected();
            }
        }

        /// <summary>
        /// コマンドの受信時に呼ばれるイベントです。
        /// </summary>
        public CommandRecieveHandler CommandReceived;

        // --- 以下private methods

        /// <summary>
        /// 受信データを分析し、\n記号ごとにコマンドを分割します。
        /// </summary>
        private IEnumerable<string> SplitCommand(byte[] readBytes, int size)
        {
            for(int i = 0;i<size;++i)
            {
                var c = readBytes[i];
                readLine.Add(c);

                if (c == '\n')
                {
                    yield return Encoding.GetString(readLine.ToArray());
                    readLine.Clear();
                }
            }
            yield break;
        }

        // -- 以下、private members

        /// <summary>
        /// 子プロセスからリダイレクトしている入力stream
        /// </summary>
        private Stream readStream;

        /// <summary>
        /// 子プロセスへの出力stream
        /// </summary>
        private Stream writeStream;

        /// <summary>
        /// stream.Read()のための受信バッファ
        /// </summary>
        private readonly byte[] readBuffer = new byte[1024];

        /// <summary>
        /// stream.Read()したものを突っ込んでおくためのバッファ
        /// </summary>
        private List<byte> readLine = new List<byte>(2048);

        /// <summary>
        /// read()のcancel用
        /// </summary>
        private readonly CancellationTokenSource readCts = new CancellationTokenSource();

        /// <summary>
        /// read()で非同期処理しないといけないのでそのためのtask
        /// </summary>
        private Task<int> task;

        private bool disposed;
    }
}
