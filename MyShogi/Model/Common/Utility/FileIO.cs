﻿using System.IO;
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
        /// 
        /// 例外は捕捉しない。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var encoding = Encode.DetectEncoding(bytes, System.Text.Encoding.GetEncoding("Shift_JIS"));
            return encoding.GetString(bytes);
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

            using (var sw = new StreamWriter(path , false , encoding))
            {
                sw.Write(content);
            }
        }
    }
}
