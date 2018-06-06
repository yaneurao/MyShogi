using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// 1行を渡して字句解析を行う。
    /// </summary>
    public class Scanner
    {
        /// <summary>
        /// コンストラクタ
        /// textに1行渡す
        /// </summary>
        public Scanner(string text_)
        {
            if (string.IsNullOrEmpty(text_))
            {
                throw new ArgumentNullException("text");
            }

            SetDelimiters(",");
            text = text_;
            index = 0;
        }

        /// <summary>
        /// 最初にコンストラクタの引数で渡した文字列
        /// </summary>
        public string Text
        {
            get { return text; }
            private set { text = value; }
        }

        /// <summary>
        /// 未パースの文字列を取得する。
        /// </summary>
        public string LastText
        {
            get { return Text.Substring(this.index); }
        }

        /// <summary>
        /// 解析文字列が終了しているか取得します。
        /// </summary>
        public bool IsEof
        {
            get { return (index >= Text.Length); }
        }

        /// <summary>
        /// 解析文字列が終了していれば例外を投げる。
        /// </summary>
        private void CheckEof()
        {
            if (IsEof)
            {
                throw new InvalidOperationException(
                    "文字列は既に終了しています。");
            }
        }

        /// <summary>
        /// int型の整数をパースします。
        /// 読み込んだ分だけindex(解析位置)を進める。
        /// </summary>
        public int ParseInt()
        {
            CheckEof();

            var m = this.intRegex.Match(this.text, this.index);
            if (!m.Success)
            {
                throw new ScannerException(
                    "整数値の解析に失敗しました。");
            }

            var result = int.Parse(m.Groups[1].Value);
            this.index += m.Length;
            return result;
        }


        /// <summary>
        /// long型の整数をパースします。
        /// 読み込んだ分だけindex(解析位置)を進める。
        /// </summary>
        public long ParseLong()
        {
            CheckEof();

            var m = this.intRegex.Match(this.text, this.index);
            if (!m.Success)
            {
                throw new ScannerException(
                    "整数値の解析に失敗しました。");
            }

            var result = long.Parse(m.Groups[1].Value);
            this.index += m.Length;
            return result;
        }

        /// <summary>
        /// 小数をパースします。
        /// 読み込んだ分だけindex(解析位置)を進める。
        /// </summary>
        public double ParseDouble()
        {
            CheckEof();

            var m = this.doubleRegex.Match(this.text, this.index);
            if (!m.Success)
            {
                throw new ScannerException(
                    "小数値の解析に失敗しました。");
            }

            var result = double.Parse(m.Groups[1].Value);
            this.index += m.Length;
            return result;
        }

        /// <summary>
        /// 次の単語を取得します。
        /// 読み込んだ分だけindex(解析位置)を進める。
        /// </summary>
        public string ParseWord()
        {
            CheckEof();

            var m = WordRegex.Match(this.text, this.index);
            if (!m.Success)
            {
                throw new ScannerException(
                    "文字列の解析に失敗しました。");
            }

            this.index += m.Length;
            return m.Groups[1].Value;
        }

        /// <summary>
        /// 文字列を読み込み、その読み込んだ文字列を取得します。
        /// </summary>
        /// <remarks>
        /// 前にPeekTextした場合は、そこで先読みした文字列を取得します。
        /// そうでない場合は、新規に文字列の解析を行います。
        /// 
        /// 先頭に"がある場合は、次の"までをまとめて取得します。
        /// </remarks>
        public string ParseText()
        {
            var result = (this.peek != null ? this.peek : PeekText());

            this.peek = null;
            return result;
        }

        /// <summary>
        /// 次の文字列を先読みし、解析したものを返します。
        /// index(解析位置)は進めない。
        /// EOFなら、nullが返る。
        /// </summary>
        /// <remarks>
        /// ここで先読みされた文字列が、次のParseTextでも使われます。
        /// 先頭に"がある場合は、次の"までをまとめて取得します。
        /// PeekText()と対応するのはParseText()のみ。ParseInt()などは対応していない。
        /// </remarks>
        public string PeekText()
        {
            if (IsEof)
            {
                return null;
            }

            string result;

            var m = this.quotedTextRegex.Match(this.text, this.index);
            if (m.Success)
            {
                // ""で囲まれた文字列の場合は、
                // 文字のエスケープを行います。
                result = m.Groups[1].Value.Replace(@"\n", "\n");
                result = result.Replace(@"\t", "\t");
                result = result.Replace(@"\\", "\\");
            }
            else
            {
                m = this.textRegex.Match(this.text, this.index);
                if (!m.Success)
                {
                    throw new ScannerException(
                        "文字列の解析に失敗しました。");
                }

                result = m.Groups[1].Value;
            }

            this.index += m.Length;
            this.peek = result;
            return result;
        }


        // -- 以下 private

        /// <summary>
        /// 現在解析しているテキスト
        /// </summary>
        private string text;

        /// <summary>
        /// 現在解析している場所
        /// </summary>
        private int index;

        /// <summary>
        /// 先読みした文字列。次回のpeekなどで使い回される。
        /// </summary>
        private string peek;

        private Regex quotedTextRegex;
        private Regex textRegex;
        private Regex intRegex;
        private Regex doubleRegex;

        // 区切り文字列の集合
        private string[] delimiters = { "," };

        // intやdoubleなどの正規表現文字列

        private static readonly string WordRegexPattern = @"\G\s*(\w+)(\s+|$)";
        private static readonly string QuotedTextRegexPattern = @"\G\s*""((\""|[^""])*?)""";
        private static readonly string TextRegexPattern = @"\G\s*(.*?)";
        private static readonly string IntRegexPattern = @"\G\s*((\+|\-)?[0-9]+)";
        private static readonly string DoubleRegexPattern = @"\G\s*((\+|\-)?[0-9]+([.][0-9.]+)?)";

        // カンマで区切られた時用の正規表現文字列
        // よく使うので事前に生成しておく。

        private static readonly Regex WordRegex = new Regex(
            WordRegexPattern, RegexOptions.Compiled);
        private static readonly Regex CommaQuotedTextRegex = CreateRegexWithDelimiters(
            QuotedTextRegexPattern, RegexOptions.Compiled, ",");
        private static readonly Regex CommaTextRegex = CreateRegexWithDelimiters(
            TextRegexPattern, RegexOptions.Compiled, ",");
        private static readonly Regex CommaIntRegex = CreateRegexWithDelimiters(
            IntRegexPattern, RegexOptions.Compiled, ",");
        private static readonly Regex CommaDoubleRegex = CreateRegexWithDelimiters(
            DoubleRegexPattern, RegexOptions.Compiled, ",");

        /// <summary>
        /// 最後にデリミタを付加した、正規表現オブジェクトを作成します。
        /// </summary>
        private static Regex CreateRegexWithDelimiters(string pattern,
                                                       RegexOptions options,
                                                       params string[] delimiters)
        {
            var escapedDelimiters =
                string.Join(
                    "|",
                    delimiters.Select(_ => Regex.Escape(_))
                    .ToArray());

            var newPattern = string.Format(
                @"{0}\s*(({1})\s*|$)",
                pattern,
                escapedDelimiters);

            return new Regex(newPattern, options);
        }

        /// <summary>
        /// 区切り文字を設定します。
        /// </summary>
        public void SetDelimiters(params string[] delimiters)
        {
            if (delimiters == null)
            {
                return;
            }

            this.delimiters = delimiters;

            if (delimiters.Count() == 1 && delimiters[0] == ",")
            {
                this.quotedTextRegex = CommaQuotedTextRegex;
                this.textRegex = CommaTextRegex;
                this.intRegex = CommaIntRegex;
                this.doubleRegex = CommaDoubleRegex;
            }
            else
            {
                this.quotedTextRegex = CreateRegexWithDelimiters(
                    QuotedTextRegexPattern, RegexOptions.None, delimiters);
                this.textRegex = CreateRegexWithDelimiters(
                    TextRegexPattern, RegexOptions.None, delimiters);
                this.intRegex = CreateRegexWithDelimiters(
                    IntRegexPattern, RegexOptions.None, delimiters);
                this.doubleRegex = CreateRegexWithDelimiters(
                    DoubleRegexPattern, RegexOptions.None, delimiters);
            }
        }


    }
}
