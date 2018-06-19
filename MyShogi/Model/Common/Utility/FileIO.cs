using System.IO;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// ファイルの入出力
    /// 
    /// </summary>
    public static class FileIO
    {
        /// <summary>
        /// ファイルからのテキスト丸読み
        /// 
        /// 先頭にBOMがついていればutf8/utf16と自動的に判別する。
        /// さもなくばsjisとして読み込む。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var encoding = Encode.DetectEncoding(bytes, System.Text.Encoding.GetEncoding("Shift_JIS"));
            return encoding.GetString(bytes);
        }
    }
}
