using System.IO;
using System.Text;

namespace MyShogi.Model.Common.Utility
{
    public static partial class Utility
    {
        /// <summary>
        /// ファイル名として使えない文字列をescapeして返す。
        ///
        /// Pathに対して使うと "c:/.."の":"がescapeされてしまい、保存できなくなってしまうので注意。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string EscapeFileName(string filename)
        {
            // どのみち"CON"とか"PRN"とか"AUX"とか"COM1"とか使えないが…。
            // これらはファイル作成時の例外を捕捉するしかない。

            var sb = new StringBuilder();

            var invalidChars = Path.GetInvalidFileNameChars();

            // invalidCharsに含まれる文字なら、falseを返す。
            bool is_valid(char c)
            {
                foreach (var c2 in invalidChars)
                    if (c == c2)
                        return false;
                return true;
            }

            // invalidCharsは'_'に置換する。
            foreach (var c in filename)
                if (is_valid(c))
                    sb.Append(c);
                else
                    sb.Append('_'); // escape

            return sb.ToString();
        }

        /// <summary>
        /// filenameのほうをescapeしてからPath.Combine()を行う。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string PathCombine(string path , string filename)
        {
            return Path.Combine(path, EscapeFileName(filename));
        }
    }
}
