using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// CSVなどをparseするクラス。
    /// 
    /// 区切り記号をカンマ以外に変更することも可能。
    /// また、区切り記号をスペースにして、かつ
    /// "c:\program files\" 1 mail
    /// のようにダブルコーテーションで囲むことも可能。
    /// (この場合、 c:\program files\ と 1 と mail の3つの要素に分解される)
    /// 
    /// </summary>
    public class CsvParser
    {
        // 要素の区切り記号。CSV形式であれば","だろう。ディフォルトではそうなっている。
        // 複数設定できる。
        public IEnumerable<string> element_separators = new[] { "," };


        /// <summary>
        /// 行の区切り記号。CSV形式であれば改行だろう。ディフォルトではそうなっている。
        /// 複数設定できる。
        /// 
        /// デフォルトでは"\r\n"(CR LF),"\n"(LF),"\r"(CR)すべてを改行記号とみなす。
        /// 前から順番にマッチングされるので"\r\n"を改行二つとみなすことはない。
        /// ("\n\r"だと改行二つだとみなされてしまうがこんなパターンを出力してくるソフトはないだろう…)
        /// </summary>
        public IEnumerable<string> line_separators = new[] { "\r\n", "\r", "\n" };

        // 引用のための記号。CSV形式であれば '"' だろう。ディフォルトではそうなっている。
        // 複数設定できる。quote部分は、同じquote記号が出現するまでがひとつのブロックとして扱われる。
        // また、改行もquote出来る。詳しくはUnitTestのコードを見ること。
        public IEnumerable<string> quote_strings = new[] { "\"" };

        // 空行を無視するのか(default = true)
        public bool ignore_black_line = true;

        // quote記号が閉じていないときに例外を投げるのか(default = false)
        public bool throw_exception = false;

        // ParseFileで読み込むときのファイルのencodingを指定する。
        // defaultではsjis。ExcelではCSVで書き出したときはsjisと決まっているので
        // ここを下手に変更しないほうが無難。
        //public Encoding Encode = Encoding.GetEncoding("Shift_JIS");

        // → 普通のCSVと互換性がなくなるが、UTF8をデフォルトに変更しておく。
        public Encoding Encode = Encoding.UTF8;

        /// <summary>
        /// 入力文字列を↑のoption設定に基づきparseして返す。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<List<string>> Parse(string input)
        {
            var ret = new List<List<string>>();

            // 愚直に一文字ずつスキャンしていく。
            var sb = new StringBuilder();
            //			int isQuote = int.MaxValue; // "…"で囲まれた範囲なのか。その場合、引用記号がquote_stringsの何番目かが入る。
            var currentQuoteString = string.Empty;
            var line = new List<string>();

            for (int i = 0; i < input.Length;)
            {
                // Quote中は要素区切り記号と改行記号は無視する
                if (string.IsNullOrEmpty(currentQuoteString))
                {
                    // 現在quote中ではない

                    // 要素区切り記号か？
                    foreach (var match in element_separators)
                    {
                        Debug.Assert(match.Length != 0);
                        if (i + match.Length > input.Length)
                            continue;
                        if (input.Mid(i, match.Length) == match)
                        {
                            // 要素区切り記号が見つかった
                            line.Add(sb.ToString());
                            i += match.Length;
                            sb = new StringBuilder();
                            goto Next;
                        }
                    }

                    // 行区切り記号か？
                    foreach (var match in line_separators)
                    {
                        Debug.Assert(match.Length != 0);
                        if (i + match.Length > input.Length)
                            continue;
                        if (input.Mid(i, match.Length) == match)
                        {
                            // 行区切り記号が見つかった
                            line.Add(sb.ToString());
                            i += match.Length;
                            sb = new StringBuilder();
                            if (ignore_black_line && line.Count == 1 && line[0].Length == 0)
                            {
                                // 空行を無視するオプションをつけていて、かつ空行なのか？
                            }
                            else
                            {
                                ret.Add(line);
                            }
                            line = new List<string>();
                            goto Next;
                        }
                    }
                    // quote記号か？
                    // quote中でないので、すべてのquote記号を調べる。
                    foreach (var match in quote_strings)
                    {
                        Debug.Assert(match.Length != 0);
                        if (i + match.Length > input.Length)
                            continue;
                        if (input.Mid(i, match.Length) == match)
                        {
                            // quote記号が見つかった
                            i += match.Length;
                            currentQuoteString = match;
                            goto Next;
                        }
                    }
                }
                else
                {
                    // quote記号中なので対応するquote記号に遭遇するかだけチェックする。
                    // ただし、quote記号が2連続で出てきている場合は、それはquote記号のescapeであるとみなす。
                    // 例) """"  は、"を"…"というquoteで囲っていることになるので、すなわち " である。

                    string match = currentQuoteString;
                    Debug.Assert(match.Length != 0);
                    if (i + match.Length > input.Length)
                        continue;
                    if (input.Mid(i, match.Length) == match)
                    {
                        // もう一度出現するなら、それはescapeされたquote記号である。
                        // そうでないことを確認する必要がある。
                        if (input.Mid(i + match.Length, match.Length) == match)
                        {
                            i += match.Length; // ひとつは無視する。
                        }
                        else
                        {
                            // quote記号が見つかった
                            i += match.Length;
                            currentQuoteString = string.Empty;
                            goto Next;
                        }
                    }
                }

                var ch = input[i++];
                // 単独のLRは有害なので CR LF("\r\n")に置換する必要がある。
                // Excelのセル内改行を使った場合、LRだけになっているような…。
                if (ch == '\n' && (sb.Length == 0 || sb[sb.Length - 1] != '\r'))
                    sb.Append('\r');

                sb.Append(ch); // 一文字追加

                Next:
                ;
            }

            // 最後に解析バッファに残っている行を出力して終わり。
            // 上のソースからコピペして無駄な行をコメントアウト
            {
                // 行区切り記号が見つかった
                line.Add(sb.ToString());
                //	i += match.Length;
                //	sb = new StringBuilder();
                if (ignore_black_line && line.Count == 1 && line[0].Length == 0)
                {
                    // 空行を無視するオプションをつけていて、かつ空行なのか？
                }
                else
                {
                    ret.Add(line);
                }
                //	line = new List<string>();
                //	goto Next;
            }

            if (throw_exception && !string.IsNullOrEmpty(currentQuoteString))
                throw new Exception("quote記号が閉じられていない。");

            return ret;
        }

        // ストリームから直接parseする
        // csv、Excelで書き出すとSJISなのよね。
        // Stream OpenするときにSJISのencode指定しないとハマる。
        public List<List<string>> Parse(TextReader sr)
        {

            // そんな大きなCSVを食わせることはないだろうし、
            // quote記号で改行をまたぐときの処理が面倒なので、
            // 全部連結してから渡せばいいんじゃないかと。

            /*
			var ret = new List<List<string>>();
            while(!sr.EndOfStream)
            {
              	var line = sr.ReadLine();
				ret.AddRange(Parse(line));
            }
			return ret;
			 */

            var sb = new StringBuilder();
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                    break;
                sb.AppendLine(line);
            }
            return Parse(sb.ToString());
        }

        /// <summary>
        /// ファイル名を指定して直接parseする。
        /// ファイルが存在しないなどの場合、例外が飛ぶ。
        /// ファイル形式はsjisと仮定。(this.Encodeで変更はできる)
        /// 
        /// FileEx.ReadAllTextを用いているのでpack file対応!!
        /// </summary>
        /// <remarks>
        /// 取得のテスト
        ///var parser = new CsvParser();
        ///var result = parser.ParseFile(fileSelector1.FilePath);
        ///foreach (var v in result)
        ///{
        ///  foreach (var val in v)
        ///  {
        ///    Console.Write(val);
        ///  }
        ///  Console.WriteLine();
        ///}
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<List<string>> ReadFile(string path)
        {
            // ↑メソッド名がWriteFileと非対称なのは気持ち悪いので変更。(2010/10/03 16:00)

            // これ、sjisでもうまくparseしよるんか？

            // Encoding指定しないとutf-8でないとうまくいかない。
            // 英数字しか使ってなかったからutf-8扱いで、たまたまうまく動いてたのか…。

            var text = File.ReadAllText(path, Encode);
            return Parse(text);
        }

        /// <summary>
        /// csvファイルの書き出し。例外いろいろ飛んでくる。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="csv"></param>
        public void WriteFile(string path, IEnumerable<IEnumerable<string>> csv)
        {
            using (var writer = new StreamWriter(path, false, Encode))
            {
                Write(writer, csv);
            }
        }

        /// <summary>
        /// writerに対して1行書き出される。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="line"></param>
        /// <param name="firstLine"></param>
        public void WriteLine(TextWriter writer , IEnumerable<string> line , bool firstLine = false)
        {
            // 1行目であるか
            var firstCell = firstLine;

            // カラム番号
            var col_no = 0;

            foreach (var e in line)
            {
                // e == nullだとContains()などが使えないのでstring.Emptyに差し替えておく。
                var elm = e == null ? string.Empty : e;

                //	Excelのバグへの対処。
                //	1行1列のセルの値が”ID”だとSYLK形式だと誤認識される。
                bool needToQuote = firstCell && elm == "ID";
                firstCell = false;

                // セパレータ記号かquote記号が含まれていれば全体をescapeする必要がある。
                foreach (var sep in element_separators)
                {
                    if (elm.Contains(sep))
                    {
                        needToQuote = true;
                        goto Skip;
                    }
                }
                foreach (var quote in quote_strings)
                {
                    if (elm.Contains(quote))
                    {
                        needToQuote = true;
                        goto Skip;
                    }
                }

                foreach (var sep in line_separators)
                {
                    if (elm.Contains(sep))
                    {
                        needToQuote = true;

                        // セル内改行なので LF(\n)のみに変更したほうがいい。
                        // Excelではそうなっているらしい。
                        // これによってCsvParserとしての汎用性を損ねるのかどうかはよくわからない。

                        elm = elm.Replace("\r\n", "\n");

                        goto Skip;
                    }
                }

                Skip:
                ;

                if (needToQuote)
                {
                    // 第一セパレータ記号でquoteするので、もしそれが含まれていればescapeする必要がある。
                    if (!quote_strings.Any())
                    {
                        // 新たに独自の例外を作るほどではないのでそのままExceptionを投げておく。
                        throw new Exception("quote記号が設定されていないのでquote出来ない。");
                    }
                    var quote = quote_strings.First();

                    // quote記号をescapeして、両端をquote記号で囲む。
                    elm = quote + elm.Replace(quote, quote + quote) + quote;
                }

                if (col_no > 0)
                {
                    if (!element_separators.Any())
                    {
                        // 新たに独自の例外を作るほどではないのでそのままExceptionを投げておく。
                        throw new Exception("セパレータ記号が設定されていないので2列目を出力出来ない。");
                    }
                    writer.Write(element_separators.First()); // separator記号で区切る。
                }
                ++col_no;

                writer.Write(elm);
            }
        }

        /// <summary>
        /// TextWriterにcsvを書き出す。
        /// writerにStringWriterオブジェクトを指定すればstring型で結果を取得できるし、
        /// StreamWriterオブジェクトを指定すれば直接ファイルに書き込むこともできる。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="csv"></param>
        public void Write(TextWriter writer, IEnumerable<IEnumerable<string>> csv)
        {
            var i = 0;
            foreach (var line in csv)
            {
                WriteLine(writer, line , i++ == 0);
            }
        }

        /// <summary>
        /// CSVファイルに1行追加する。
        /// CSVファイルを読み込まずに何も考えずに1行appendする。
        /// </summary>
        public void AppendLine(string path, IEnumerable<string> line)
        {
            using (var writer = new StreamWriter(path , true , Encode))
            {
                WriteLine(writer, line);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// UnitTest
        /// </summary>
        public static void UnitTest()
        {
            try
            {

                {
                    var parser = new CsvParser();
                    var ret = parser.Parse("abc,def,123\r\nbcd,efg,234");
                    Debug.Assert(ret[1][2] == "234");
                }
                {
                    var parser = new CsvParser();
                    parser.element_separators = new List<string> { " " }; // スペース区切りに変更してみる
                    var ret = parser.Parse("\"c:\\program files\\\" 123 mail");
                    Debug.Assert(ret[0][1] == "123");
                }
                {
                    var parser = new CsvParser();
                    var ret = parser.Parse("\"途中に\"\"が出てくるだとか\"");
                    Debug.Assert(ret[0][0] == "途中に\"が出てくるだとか");
                }
                {
                    var parser = new CsvParser();
                    var ret = parser.Parse("script_system/include/header_script.txt,18,\"// 	using XXX in \"\"yyy.dll\"\"\"\r\nscript_system/include/header_script.txt,19,\"// のようにdllを指定してusingできる。\"");
                    // 末尾に " が出てくるパターン
                    Debug.Assert(ret[0][2] == "// 	using XXX in \"yyy.dll\"");
                }
                {
                    var parser = new CsvParser();
                    parser.ignore_black_line = true; // 空行を無視してみる
                    var ret = parser.Parse("123,abc\r\n\r\n345,XXX,def");
                    Debug.Assert(ret[1][2] == "def"); // 2行目の3つ目の要素
                }
                {
                    using (var sw = new StreamWriter("test.csv"))
                    {
                        sw.WriteLine("123,456,789");
                        sw.WriteLine("ABC,DEF,GHI");
                    }
                    var parser = new CsvParser();
                    List<List<string>> ret;
                    using (var sr = new StreamReader("test.csv"))
                    {
                        ret = parser.Parse(sr);
                    }
                    Debug.Assert(ret[1][2] == "GHI"); // 2行目の3つ目の要素
                }
                {
                    var parser = new CsvParser();
                    parser.quote_strings = new List<string> { "\"", "'" }; // " と ' をquote用の文字列にする。
                    var ret = parser.Parse("\"123'456\"'78\"9A\r\nBC'");
                    Debug.Assert(ret[0][0] == "123'45678\"9A\r\nBC"); // quote記号は同じquote記号が見つかるまで有効。
                }
                {
                    var parser = new CsvParser();
                    var ret = parser.Parse("\"123");
                    Debug.Assert(ret[0][0] == "123"); // quote記号は同じquote記号が見つかるまで有効。

                    try
                    {
                        parser.throw_exception = true; // 例外を投げる設定にしてみる。quote記号が閉じられていないので例外が飛ぶ。
                        ret = parser.Parse("\"123");
                        Debug.Assert(false);
                    }
                    catch
                    {
                        // 例外が飛ばなければおかしい
                        Debug.Assert(true);
                    }

                }

            }
            catch
            {
                Debug.Assert(false); // 例外が飛んできたらおかしい。
            }

            //// その他
            //var csv = new CsvParser();
            //using (var sr = new StreamReader("Book1.csv"))
            //{
            //  var list = csv.Parse(sr);
            //  Console.WriteLine(list[0][0] + "/" + list[1][0] + "/" + list[2][0]);
            //}
            /*
			 * quote中の""はquoteをescapeしているの意味なので、
""""      == "
""","     == ",
""","""   == ","
だときちんと読み込める。
			 */
        }
    }
}
