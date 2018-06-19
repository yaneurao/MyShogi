using System.Text;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// 各種Utility
    /// </summary>
    public static class Encode
    {
        /// <summary>
        /// エンコーディング判定用
        /// 
        /// BOMなどからencodingを判定する。
        /// BOMがなくて判定できなかった場合には、引数で指定されているdefaultEncondingが返る。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="defaultEnconding"></param>
        public static Encoding DetectEncoding(byte[] bytes, System.Text.Encoding defaultEnconding)
        {
            // -- 2バイトBOM

            if (bytes.Length < 2)
            {
                return defaultEnconding;
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
                return defaultEnconding;
            }

            if ((bytes[0] == 0xef) && (bytes[1] == 0xbb) && (bytes[2] == 0xbf))
            {
                //UTF-8
                return new System.Text.UTF8Encoding(true, true);
            }

            // -- 4バイトBOM

            if (bytes.Length < 4)
            {
                return defaultEnconding;
            }

            if ((bytes[0] == 0x00) && (bytes[1] == 0x00) &&
                (bytes[2] == 0xfe) && (bytes[3] == 0xff))
            {
                //UTF-32 BE
                return new System.Text.UTF32Encoding(true, true);
            }

            // BOMがなくEncoding不明。もう少し頑張ったほうがいいかも。
            return defaultEnconding;
        }
    }
}
