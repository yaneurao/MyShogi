using System;
using System.IO;
using System.Text;

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
        /// (先頭のBOMは除去して、stringに変換して返す。)
        /// 
        /// 例外は捕捉しない。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadText(string path)
        {
            var bytes = File.ReadAllBytes(path);
            return Encode.ConvertToString(bytes);
        }

        /// <summary>
        /// ファイルに書き出す。encodingを指定しなければutf8 BOMつき。
        /// 
        /// 例外は捕捉しない。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static void WriteFile(string path , string content , Encoding encoding = null)
        {
            // これをStreamWriterに指定した場合、BOMが出力されることは保証されている。
            // Encoding.UTF8でもBOMは出力されるが、これはMSDNのドキュメント上に記載されていない(?)ため、未定義動作である可能性がある。
            if (encoding == null)
                encoding = new UTF8Encoding(true);

            // フォルダが存在することを確認する。なければ新たに作成する。
            CreateDirectory(path);

            using (var sw = new StreamWriter(path , false , encoding))
            {
                sw.Write(content);
            }
        }

        /// <summary>
        /// ファイルのpathを指定し、その親フォルダが存在しなければ作成する。
        /// </summary>
        /// <param name="file_path"></param>
        public static void CreateDirectory(string file_path)
        {
            var folder = Path.GetDirectoryName(file_path);
            if (Directory.Exists(folder))
                return;

            try
            {
                Directory.CreateDirectory(folder);
            } catch
            {
                throw new Exception($"フォルダの作成に失敗しました。\r\nフォルダ名 = {folder}");
            }

            /*
                .NETのCreateDirectory()、セキュリティ的なbugがあるので自作すべきかも知れない。
         
               cf.Directory.CreateDirectory() method bug fixed
               https://www.codeproject.com/Articles/10160/Directory-CreateDirectory-method-bug-fixed

                Scriptingを使う実装は移植性下がりそうで嫌だな…。これはやめとく。
             */
        }
    }
}
