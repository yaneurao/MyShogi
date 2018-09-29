using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using MyShogi.Model.Common.Tool;

namespace MyShogi.Model.Common.Process
{
    /// <summary>
    /// 子プロセスを生成して、リダイレクトされた標準入出力経由でやりとりするためのクラス
    /// ローカルの思考エンジンに接続するときに用いる。
    /// </summary>
    public class ProcessNegotiator
    {
        #region properties
        public delegate void CommandReceiveHandler(string command);

        ///// <summary>
        ///// 子プロセスの標準出力から新しい行を受信したときのコールバック
        ///// Connect()のあとにRead()までにセットしておくこと。
        ///// </summary>
        public CommandReceiveHandler CommandReceived;

        /// <summary>
        /// エンジン側とやりとりする時のEncode。
        /// Connect()を呼び出すまでに設定すること。
        /// 
        /// USIでは定められていない(漢字はサポートされていない？)が、SJISとして扱っている気がする。
        /// いまどきSJISはないと思うので、デフォルトでUTF8とする。
        /// </summary>
        public Encoding Encode = Encoding.UTF8;

        /// <summary>
        /// 実行ファイルの優先度を普通より下げる。
        /// UpdateProcessPriority()したときに反映される。
        /// </summary>
        public bool IsLowPriority { get; set; }

        /// <summary>
        /// プロセスが終了したかのフラグ。
        /// </summary>
        public bool ProcessTerminated { get { return exeProcess == null ? false : exeProcess.HasExited; } }

        #endregion

        #region publics
        /// <summary>
        /// 思考エンジンに接続する。
        /// </summary>
        public void Connect(ProcessNegotiatorData engineData)
        {
            Disconnect();

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
                    StandardOutputEncoding = Encode,
                };

                var process = new System.Diagnostics.Process
                {
                    StartInfo = info,
                };

                // 子プロセスがいないとき、ここで例外が発生する。
                process.Start();

                // 非同期での受信開始
                writeStream = process.StandardInput.BaseStream;
                process.OutputDataReceived += DataReceived;
                process.BeginOutputReadLine();

                // EXE用のprocess
                exeProcess = process;

                IsLowPriority = engineData.IsLowPriority;
            }
        }

        /// <summary>
        /// 接続している子プロセスから行を読み込む。
        /// 読み込む行がなければ、ブロッキングせずにすぐ戻る。
        /// 
        /// このメソッドは、例外を投げる。
        /// </summary>
        public void Read()
        {
            lock (readLockObject)
            {
                // このタイミングで受信していたメッセージに対して、
                // callbackを呼び出す。
                foreach (var line in read_lines)
                    CommandReceived(line);
                read_lines.Clear();
            }
        }

        /// <summary>
        /// 接続している子プロセスに行を流し込む。
        ///
        /// このメソッドは、例外を投げる。
        /// </summary>
        /// <param name="s"></param>
        public void Write(string command)
        {
            if (writeStream == null)
                return;

            // WriteはUIスレッドからも行うのでlockが必要。
            lock (writeLockObject)
            {
                // 基本的に待たされることはないので、非同期には実行していない。
                byte[] buffer = Encode.GetBytes($"{command}\n");
                writeStream.Write(buffer, 0, buffer.Length);
                writeStream.Flush(); // これを行わないとエンジンに渡されないことがある。
                Log.Write(LogInfoType.SendCommandToEngine, command, pipe_id);
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
                    try
                    {
                        exeProcess.Close();
                        //exeProcess.Kill();

                        // Close()してからKill()できない。
                        // "quit"を送っているし、pipeは切断されているし、無事終了してくれることを祈るばかりだ。

                        // 一応、Dispose()も呼び出しておくか…。
                        exeProcess.Dispose();

                    } catch { }

                    exeProcess = null;
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// 接続した実行ファイルの優先度を変更します。
        /// IsLowPriorityがtrueであれば通常より低い優先度にします。
        /// (GUIがもっさりするときなど用)
        /// </summary>
        public void UpdateProcessPriority()
        {
            if (exeProcess == null)
            {
                throw new InvalidOperationException("exeProcess");
            }

            try
            {
                var priority = (IsLowPriority ?
                    ProcessPriorityClass.BelowNormal :
                    ProcessPriorityClass.Normal);

                if (this.exeProcess.PriorityClass != priority)
                {
                    this.exeProcess.PriorityClass = priority;
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogInfoType.SystemError,"エンジンの実行優先順位を下げることができませんでした。:" + ex.ToString());
            }
        }
        #endregion

        #region privates
        // --- 以下private members

        /// <summary>
        /// Connect()で接続したプロセス
        /// </summary>
        private System.Diagnostics.Process exeProcess;

        /// <summary>
        /// 排他用
        /// </summary>
        private object lockObject = new object();
        private object readLockObject = new object();
        private object writeLockObject = new object();

        /// <summary>
        /// Processから受信時に非同期で呼び出されるハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            // とりま、バッファに溜めておいて、Read()のタイミングで返す。
            lock (readLockObject)
                read_lines.Add(e.Data);
        }
        /// <summary>
        /// 受信した行を保存しておくバッファ。Read()で放出。
        /// </summary>
        private List<string> read_lines = new List<string>();

        private Stream writeStream;

        /// <summary>
        /// このインスタンスのunique idが返る。これによってログに書き出した時に
        /// どのインスタンス(思考エンジン)との通信であるかを識別する。
        /// インスタンス生成時にunique idが割り当たる。
        /// </summary>
        private int pipe_id = get_unique_id();

        /// <summary>
        /// pipe_idの発行用。取得するごとに1ずつ増える
        /// </summary>
        private static int g_pipe_id;
        private static object g_lock_object = new object();
        private static int get_unique_id()
        {
            int result;
            // g_pipe_idから値を取得して、インクリメントするまでがatomicであって欲しい。
            lock (g_lock_object) { result = g_pipe_id++; }
            return result;
        }
        #endregion
    }
}
