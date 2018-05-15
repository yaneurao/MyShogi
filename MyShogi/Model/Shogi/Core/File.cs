using System;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 将棋の「筋」(列)を表現する型
    /// 例) FILE_3なら3筋。
    /// </summary>
    public enum File : Int32
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
            return "１２３４５６７８９".Substring((int)f.ToInt(),1);
        }

        /// <summary>
        /// USI文字列へ変換する。
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string ToUsi(this File f)
        {
            return new string((char)((Int32)'1' + f.ToInt()),1);
        }


        /// <summary>
        /// Int32型への変換子
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Int32 ToInt(this File f)
        {
            return (Int32)f;
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

    public static partial class Util
    {
        /// <summary>
        /// 筋を表現するUSI文字列をFileに変換する
        /// 変換できないときはFile.NBが返る。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static File FromUsiFile(char c)
        {
            File f = (File)((int)c - (int)'1');
            if (!f.IsOk())
                f = File.NB;
            return f;
        }
    }
}
