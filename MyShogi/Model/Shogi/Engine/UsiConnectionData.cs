using System;

namespace MyShogi.Model.Shogi.Engine
{
    /// <summary>
    /// USIプロトコルで実行ファイルに接続するときに必要なデータ
    /// あとで(ネットワークごしのエンジンに接続する場合などに向けて)抽象化するかも。
    /// </summary>
    public class UsiConnectionData
    {
        public UsiConnectionData(string file_path , string working_dir = null , string args = null)
        {
            ExeFilePath = file_path;
            ExeWorkingDirectory = working_dir;
            ExeArguments = args;
        }

        /// <summary>
        /// 実行ファイルへのpath
        /// </summary>
        public string ExeFilePath { get; set; }

        /// <summary>
        /// 実行ファイルのWorkingDirectory
        /// </summary>
        public string ExeWorkingDirectory { get; set; }

        /// <summary>
        /// 実行ファイルに渡す引数
        /// </summary>
        public string ExeArguments { get; set; }
    }
}
