using System;
using System.IO;
using System.Collections.Generic;

namespace MyShogi.Model.Common.Tool
{
    /// <summary>
    /// ログを出力するときに、その出力される情報の種別
    /// </summary>
    public enum LogInfoType
    {
        SendCommandToEngine,      // エンジンへのコマンドの送信
        ReceiveCommandFromEngine, // エンジンからのコマンドの受信
        UsiServer,                // USIのコマンドを受理している側のメッセージ
        UsiParseError,            // USIのコマンドの解析エラー
        SystemError,              // システムエラー
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

                case LogInfoType.UsiParseError:
                    log = $"{date} {time}'{ms} USI Parse Error : {log}";
                    break;

                case LogInfoType.UsiServer:
                    log = $"{date} {time}'{ms} USI Server : {log}";
                    break;
            }
            return log;
        }
    }

    /// LogListが1行追加になった時に呼び出されるイベントハンドラの型
    public delegate void ListAddedEventHandler(string sender);

    /// <summary>
    /// メモリ上に記録するタイプのログ
    ///
    /// メモリ上のLogを1万行ぐらいまでの保存とする。
    /// - これでも10局分ぐらいは保存できるはず。
    /// - 1局100KB程度(1000行程度)なので1万局で1000万行、1GBぐらいになる。
    /// - 連続対局中、Hashサイズの変更は出来ないので(Hashの再初期化をしないので)、メモリギリギリの状態からだと1GBはきつすぎる。
    /// </summary>
    public class MemoryLog : ILog
    {
        /// <summary>
        /// 保存する行数の上限。
        ///
        /// ただし、デバッグウインドウを開いている間は、この行数を超えて表示される。
        /// (これはデバッグウインドウ側で保持しているQueueに追加されていくため)
        /// </summary>
        public int MaxLine { get; set; } = 10000;

        public void Write(LogInfoType logType, string log , int pipe_id)
        {
            var f_log = LogHelpper.Format(logType, log , pipe_id);

            lock (lock_object)
            {
                LogList.Enqueue(f_log);

                // 上限行数を超えていたら末尾のものを取り除く。
                if (LogList.Count > MaxLine)
                    LogList.Dequeue();
            }

            // イベントハンドラが設定されていればcallbackしたいが、lock解除してからでないとdead lockになる。

            ListAdded?.Invoke(f_log);
        }

        /// <summary>
        /// Listが1行追加されたときに呼び出されるハンドラをセットして、
        /// その時点でのlistをコピーして取得する。
        /// </summary>
        /// <param name="h"></param>
        /// <param name="list"></param>
        public void AddHandler(ListAddedEventHandler h , ref Queue<string> list)
        {
            lock (lock_object)
            {
                ListAdded += h;
                list = new Queue<string>(LogList); // その時点のlistをCloneして返す
            }
        }

        public void RemoveHandler(ListAddedEventHandler h)
        {
            lock(lock_object)
            {
                ListAdded -= h;
            }
        }

        /// <summary>
        /// ログの内容をクリアする。
        /// </summary>
        public void Clear()
        {
            lock (lock_object)
                LogList.Clear();
        }

        public void Dispose() { }

        /// <summary>
        /// 書き出されたログ
        /// 上限を1万行ぐらいに制限したいので、Queue構造にする。
        /// </summary>
        private Queue<string> LogList { get; set; } = new Queue<string>();

        /// <summary>
        /// LogListが1行追加になった時に呼び出されるイベントハンドラ
        /// </summary>
        private ListAddedEventHandler ListAdded;

        /// <summary>
        /// ListAdded , LogListを変更するときにlockされるべきlock用のobject。
        /// </summary>
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
