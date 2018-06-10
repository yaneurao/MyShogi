using System.IO;

namespace MyShogi.Model.Common.Process
{
    /// <summary>
    /// USIプロトコルで実行ファイルに接続するときに必要なデータ
    /// あとで(ネットワークごしのエンジンに接続する場合などに向けて)抽象化するかも。
    /// 
    /// working_dirを省略したときは、実行ファイルの存在するフォルダ(file_pathのフォルダ)と同じになる。
    /// </summary>
    public class ProcessNegotiatorData
    {
        public ProcessNegotiatorData(string file_path , string working_dir = null , string args = null)
        {
            ExeFilePath = file_path;
            if (working_dir == null)
                working_dir = Path.GetDirectoryName(ExeFilePath);

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

        /// <summary>
        /// 実行ファイルの優先度を探す
        /// </summary>
        public bool IsLowPriority { get; set; }
    }
}
