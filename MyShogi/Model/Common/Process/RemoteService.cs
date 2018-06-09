using MyShogi.Model.Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyShogi.Model.Common.Process
{
    /// <summary>
    /// RemoteService.Read()でコマンドを受信したときに呼び出されるハンドラの型
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
        public Exception ReadException
        {
            get;
            set;
        }

        /// <summary>
        /// write()で例外が起きたときにここにその例外が格納される。
        /// </summary>
        public Exception WriteException
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
                            Log.Write(LogInfoType.ReceiveCommandFromEngine, command);

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
                ReadException = ex;
                //NotifyDisconnected();
            }
        }

        /// <summary>
        /// コマンドの受信時に呼ばれるイベントです。
        /// 引数として1行文のstringが来るが、そこに改行コードは含まれない。
        /// </summary>
        public CommandRecieveHandler CommandReceived;

        /// <summary>
        /// WriteStreamに書き出し。改行は自動的に付与されるので引数で渡すstringには付与してはならない。
        /// </summary>
        /// <param name="command"></param>
        public void Write(string command)
        {
            try
            {
                // 送信で待たされることは現実的にはないはずなので非同期処理はしていない。
                // しかし、UIスレッドからもコマンド送信を行う可能性があるので排他処理は必要である。

                lock (writeLockObjcet)
                {
                    byte[] buffer = Encoding.GetBytes($"{command}\n");
                    writeStream.Write(buffer, 0, buffer.Length);
                    writeStream.Flush(); // これを行わないとエンジンに渡されないことがある。
                }

                Log.Write(LogInfoType.SendCommandToEngine , command);
            }
            catch (Exception ex)
            {
                WriteException = ex;
            }
        }

        // --- 以下private methods

        /// <summary>
        /// 受信データを分析し、\n記号ごとにコマンドを分割します。
        /// </summary>
        private IEnumerable<string> SplitCommand(byte[] readBytes, int size)
        {
            for(int i = 0;i<size;++i)
            {
                var c = readBytes[i];
                if (c == '\r')
                    continue; // この文字列はskipする。

                // 改行コードは追加しない。
                if (c != '\n')
                {
                    readLine.Add(c);
                }
                else
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

        /// <summary>
        /// Write()で排他する用。
        /// </summary>
        private object writeLockObjcet = new object();

    }
}
