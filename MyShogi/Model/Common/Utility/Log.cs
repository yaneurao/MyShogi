using System;
using System.IO;
using System.Collections.Generic;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// ログを出力するときに、その出力される情報の種別
    /// </summary>
    public enum LogInfoType
    {
        SendCommandToEngine, // エンジンへのコマンドの送信
        ReceiveCommandFromEngine, // エンジンからのコマンドの受信
        UsiServer, // USIのコマンドを受理している側のメッセージ
        SystemError, // システムエラー
    }

    /// <summary>
    /// Logクラスの基底
    /// </summary>
    public interface ILog : IDisposable
    {
        void Write(LogInfoType logType, string log , int pipe_id = -1);
    }

#if false
    // メソッドの呼び出しのオーバーヘッドが嫌なのでnull objectを使う実装、好ましくない。

    /// <summary>
    /// 何も出力しないLogクラス
    /// </summary>
    public class NullLog : ILog
    {
        public void Write(LogInfoType logType, string log , int pipe_id = -1) { }
        public void Dispose() { }
    }
#endif

    /// <summary>
    /// ログ出力の補助クラス
    /// </summary>
    public static class LogHelpper
    {
        /// <summary>
        /// ログ出力用に整形する。
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string Format(LogInfoType logType, string log , int pipe_id = -1)
        {
            var now = DateTime.Now;
            var date = now.ToShortDateString();
            var time = now.ToLongTimeString();
            var ms = now.Millisecond.ToString().PadLeft(3, '0');

            switch (logType)
            {
                case LogInfoType.ReceiveCommandFromEngine:
                    log = $"{date} {time}'{ms} [{pipe_id}>] {log}";
                    break;

                case LogInfoType.SendCommandToEngine:
                    log = $"{date} {time}'{ms} [{pipe_id}<] {log}";
                    break;

                case LogInfoType.SystemError:
                    log = $"{date} {time}'{ms} Error : {log}";
                    break;

                case LogInfoType.UsiServer:
                    log = $"{date} {time}'{ms} USI Server : {log}";
                    break;
            }
            return log;
        }
    }

    public delegate void ListAddedEventHandler(object sender);

    /// <summary>
    /// メモリ上に記録するタイプのログ
    /// </summary>
    public class MemoryLog : ILog
    {
        public void Write(LogInfoType logType, string log , int pipe_id)
        {
            var f_log = LogHelpper.Format(logType, log , pipe_id);

            List<string> c = null;
            lock (lock_object)
            {
                LogList.Add(f_log);

                if (ListAdded != null)
                    c = new List<string>(LogList); // clone
            }

            // イベントハンドラが設定されていればcallbackしたいが、lock解除してからでないとdead lockになる。
            // かと言って、LogListはmutableだし…。仕方ないのでClone()しといてそれ渡す。オーバーヘッドすごすぎ…。
            // まあ、ロギングしていない時は、オーバーヘッドなしと考えられるので、これはこれでいいや…。
            if (ListAdded != null)
                ListAdded(c);
        }

        public void Dispose() { }

        /// <summary>
        /// 書き出されたログ
        /// </summary>
        public List<string> LogList { get; private set; } = new List<string>();

        /// <summary>
        /// LogListが変更になった時のイベントハンドラ
        /// </summary>
        public ListAddedEventHandler ListAdded;

        private object lock_object = new object();
    }

    /// <summary>
    /// ファイルに出力するタイプのLogクラス
    /// </summary>
    public class FileLog : ILog
    {
        public FileLog(string filename)
        {
            sw = new StreamWriter(filename);
        }

        public void Write(LogInfoType logType, string log , int pipe_id)
        {
            var f_log = LogHelpper.Format(logType, log , pipe_id);
            lock (lock_object)
            {
                sw.WriteLine(f_log);
                sw.Flush();
            }
        }

        public void Dispose()
        {
            sw.Dispose();
        }

        private object lock_object = new object();
        private StreamWriter sw;
    }

    /// <summary>
    /// ログを出力するためのクラス
    /// 
    /// 事前に
    /// Log.log = new FileLog(filename);
    /// とやってファイルに出力するようにしておくと、以降、ログがファイルに出力される。
    /// </summary>
    public static class Log
    {
        public static void Write(LogInfoType logType, string message , int pipe_id = -1)
        {
            // logがアタッチされていないときは、なるべく小さなオーバーヘッドで済むように、
            // Format()はここでは呼び出さない。

            if (log1 != null)
                log1.Write(logType, message , pipe_id);

            if (log2 != null)
                log2.Write(logType, message , pipe_id);
        }

        /// <summary>
        /// MemoryLogを突っ込んでおく用
        /// </summary>
        public static ILog log1 { get; set; }

        /// <summary>
        /// FileLogを突っ込んでおく用
        /// </summary>
        public static ILog log2 { get; set; }
    }
}
