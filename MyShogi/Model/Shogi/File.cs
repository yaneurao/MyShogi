namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 将棋の「筋」(列)を表現する型
    /// 例) FILE_3なら3筋。
    /// </summary>
    public enum File : int
    {
        FILE_1, FILE_2, FILE_3, FILE_4, FILE_5, FILE_6, FILE_7, FILE_8, FILE_9, NB , ZERO = 0
    };

    /// <summary>
    /// Fileに関するextension methodsを書くクラス
    /// </summary>
    public static class FileExtensions
    {
        public static bool IsOk(this File f)
        {
            return File.ZERO <= f && f < File.NB;
        }

        /// <summary>
        /// Fileを綺麗に出力する(USI形式ではない)
        /// 日本語文字での表示になる。例 → ８
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string Pretty(this File f)
        {
            // C#では全角1文字が1つのcharなので注意。
            return "１２３４５６７８９".Substring(f.ToInt(),1);
        }

        /// <summary>
        /// USI文字列へ変換する。
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string ToUSI(this File f)
        {
            return new string((char)((int)'1' + f.ToInt()),1);
        }


        /// <summary>
        /// int型への変換子
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int ToInt(this File f)
        {
            return (int)f;
        }

        /// <summary>
        /// USIの指し手文字列などで筋を表す文字列をここで定義されたFileに変換する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static File ToFile(this char c)
        {
            return (File)(c - '1');
        }

    }
}
