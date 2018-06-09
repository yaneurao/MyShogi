using System;
using System.IO;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// ログを出力するときに、その出力される情報の種別
    /// </summary>
    public enum LogInfoType
    {
        SendCommandToEngine      , // エンジンへのコマンドの送信
        ReceiveCommandFromEngine , // エンジンからのコマンドの受信
        UsiServer                , // USIのコマンドを受理している側のメッセージ
        SystemError              , // システムエラー
    }

    /// <summary>
    /// Logクラスの基底
    /// </summary>
    public interface ILog
    {
        void Write(LogInfoType logType,string log);
    }

    /// <summary>
    /// 何も出力しないLogクラス
    /// </summary>
    public class NullLog : ILog
    {
        public void Write(LogInfoType logType, string log) { }
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

        public void Write(LogInfoType logType, string log)
        {
            var now = DateTime.Now;
            var date = now.ToShortDateString();
            var time = now.ToLongTimeString();
            var ms = now.Millisecond.ToString().PadLeft(3,'0');

            switch(logType)
            {
                case LogInfoType.ReceiveCommandFromEngine:
                    log = $"{date} {time}'{ms} > {log}";
                    break;

                case LogInfoType.SendCommandToEngine:
                    log = $"{date} {time}'{ms} < {log}";
                    break;

                case LogInfoType.SystemError:
                    log = $"{date} {time}'{ms} Error : {log}";
                    break;

                case LogInfoType.UsiServer:
                    log = $"{date} {time}'{ms} USI Server : {log}";
                    break;
            }
            sw.WriteLine(log);
            sw.Flush();
        }

        public void Dispose()
        {
            sw.Dispose();
        }

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
        public static void Write(LogInfoType logType, string message)
        {
            log.Write(logType, message);
        }

        public static ILog log { get; set; } = new NullLog();
    }
}
