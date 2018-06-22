using System.Text;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// Encode判定など
    /// </summary>
    public static class Encode
    {
        /// <summary>
        /// ファイルから読み込んだバイト列のencodeを自動判別して、string型にして返す。
        /// BOMがついていれば、BOMを除去して返す。
        /// BOMがついていない場合、Shift_JISとみなしてstring型に変換して返す。
        /// (あとでもう少し頑張って判定する。)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ConvertToString(byte[] bytes)
        {
            var encoding = Encode.DetectBOM(bytes);
            if (encoding != null)
            {
                // 先頭のBOMをskipしてdecode
                int bomLen = encoding.GetPreamble().Length;
                return encoding.GetString(bytes, bomLen, bytes.Length - bomLen);
            }

            // 先頭1024bytesからSJIS/UTF8判定してみる(判定できない時はSJIS優先)
            encoding = Shogi.Converter.DetectEncoding.getEncoding_sj(bytes, 1024) ?? Encoding.GetEncoding(932);
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// エンコーディング判定用
        ///
        /// BOMなどからencodingを判定する。
        /// BOMがなくて判定できなかった場合には、nullが返る。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="defaultEnconding"></param>
        public static Encoding DetectBOM(byte[] bytes)
        {
            // -- 2バイトBOM

            if (bytes.Length < 2)
            {
                return null;
            }

            if ((bytes[0] == 0xfe) && (bytes[1] == 0xff))
            {
                //UTF-16 BE
                return new System.Text.UnicodeEncoding(true, true);
            }
            if ((bytes[0] == 0xff) && (bytes[1] == 0xfe))
            {
                if ((4 <= bytes.Length) &&
                    (bytes[2] == 0x00) && (bytes[3] == 0x00))
                {
                    //UTF-32 LE
                    return new System.Text.UTF32Encoding(false, true);
                }
                else
                    //UTF-16 LE
                    return new System.Text.UnicodeEncoding(false, true);
            }

            // -- 3バイトBOM

            if (bytes.Length < 3)
            {
                return null;
            }

            if ((bytes[0] == 0xef) && (bytes[1] == 0xbb) && (bytes[2] == 0xbf))
            {
                //UTF-8
                return new System.Text.UTF8Encoding(true, true);
            }

            // -- 4バイトBOM

            if (bytes.Length < 4)
            {
                return null;
            }

            if ((bytes[0] == 0x00) && (bytes[1] == 0x00) &&
                (bytes[2] == 0xfe) && (bytes[3] == 0xff))
            {
                //UTF-32 BE
                return new System.Text.UTF32Encoding(true, true);
            }

            // BOMがなくEncoding不明。
            return null;
        }
    }
}
