using System;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// コマンドラインの解析用ヘルパ
    /// </summary>
    public class CommandLineParser
    {
        public CommandLineParser()
        {
            // CommandLineArgs()で取得するとargs[0]は、実行ファイルのpathが入っているので注意。
            args = Environment.GetCommandLineArgs();
            pos = 1; // 次の解析位置
        }

        /// <summary>
        /// コマンドラインの、現在の解析位置から次のstringを取得する。
        /// 解析位置は一つ進む。
        /// 終端まで行っていればnullが返る。
        /// </summary>
        public string GetText()
        {
            if (pos >= args.Length)
                return null;

            return args[pos++];
        }

        /// <summary>
        /// コマンドラインの、現在の解析位置から次のstringを取得する。
        /// 解析位置は進まない。(GetText()したときに進む)
        /// 終端まで行っていればnullが返る。
        /// </summary>
        /// <returns></returns>
        public string PeekText()
        {
            if (pos >= args.Length)
                return null;

            return args[pos];
        }

        private string[] args;
        private int pos;
    }
}
