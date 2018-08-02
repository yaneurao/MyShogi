using System;
using System.Text;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// 解析失敗時に投げられる例外です。
    /// </summary>
    public class ScannerException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScannerException()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScannerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScannerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 1行を渡して字句解析を行う。
    /// スペースを区切り文字として扱いながらトークンを読みこんで行く。
    /// 複数のスペースが連続する場合は、1つのスペースとして扱う。
    /// スペース以外にもタブも同じ意味と解釈する。
    /// そこそこ高速。
    /// 
    /// また、区切り文字を","にしたい場合などは、string.Split()してしまうほうが良いと思う…。
    /// </summary>
    public class Scanner
    {
        /// <summary>
        /// コンストラクタ
        /// textに1行渡す
        /// </summary>
        public Scanner(string text_)
        {
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
        /// 整数をパースする。
        /// 読み込んだ分だけindex(解析位置)を進める。
        /// 整数がなければ例外を投げる。
        /// </summary>
        public long ParseInt()
        {
            if (peek != null)
                throw new ScannerException(
                    "PeekText()したあと、ParseText()を呼び出さずにParseInt()が呼び出されました。");

            SkipSpace();
            CheckEof();

            var sb = new StringBuilder();
            while (index < Text.Length)
            {
                var c = Text[index];
                if ('0' <= c && c <= '9')
                {
                    sb.Append(c);
                    index++;
                }
                else
                {
                    // ここの文字が何であるかは知らん..
                    break;
                }
            }

            long result;
            if (!long.TryParse(sb.ToString(), out result))
            {
                throw new ScannerException(
                    "整数値の解析に失敗しました。");
            }

            return result;
        }

        /// <summary>
        /// 次の単語を取得する。
        /// 読み込んだ分だけindex(解析位置)を進める。
        /// 文字列がなければ例外を投げる。
        /// </summary>
        public string ParseWord()
        {
            if (peek != null)
                throw new ScannerException(
                    "PeekText()したあと、ParseText()を呼び出していないのにParseWord()が呼び出されました。");

            SkipSpace();
            CheckEof();

            var sb = new StringBuilder();
            while (index < Text.Length)
            {
                var c = Text[index++];
                if (c == ' ' || c == '\t')
                {
                    break;
                }
                sb.Append(c);
            }

            if (sb.Length == 0)
            {
                throw new ScannerException(
                    "文字列の解析に失敗しました。");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 文字列を読み込み、その読み込んだ文字列を取得する。
        /// </summary>
        /// <remarks>
        /// 前にPeekText()した場合は、そこで先読みした文字列を取得する。
        /// そうでない場合は、新規に文字列の解析を行う。
        /// 
        /// 先頭に"がある場合は、次の"までをまとめて取得する。
        /// 例外は投げない。文字列がない場合nullが返る。
        /// </remarks>
        public string ParseText()
        {
            var result = (peek != null ? peek : PeekText());

            peek = null;
            return result;
        }

        /// <summary>
        /// 次の文字列を先読みし、解析したものを返します。
        /// index(解析位置)は進めない。
        /// EOFなら、nullが返る。
        /// </summary>
        /// <remarks>
        /// ここで先読みされた文字列が、次のParseText()でも使わる。
        /// 先頭に"がある場合は、次の"までをまとめて取得する。
        /// PeekText()と対応するのはParseText()のみ。ParseInt()などは対応していない。
        /// 複数回、連続してPeekText()を呼ぶのは合法
        /// </remarks>
        public string PeekText()
        {
            // 複数回呼び出された時対策
            if (peek != null)
                return peek;

            SkipSpace();
            if (IsEof)
            {
                // このメソッドは例外を投げない
                return null;
            }

            var sb = new StringBuilder();
            bool quote = false; // 先頭に " があったか
            bool first = true;

            while (index < Text.Length)
            {
                var c = Text[index++];
                if (first)
                {
                    if (c == '"')
                    {
                        quote = true;
                        continue;
                    }
                    first = false;
                }

                if (!quote && (c == ' ' || c == '\t'))
                {
                    // スペースかタブに遭遇したらそこまで
                    break;
                } else if (quote && c == '"')
                {
                    // quoteスタートなので "に遭遇したら終了
                    break;
                }

                sb.Append(c);
            }

            var result = sb.Length == 0 ? null : sb.ToString();

            peek = result;
            return result;
        }

        /// <summary>
        /// PeekTextの結果をcompareと比較します
        /// upperLowerがfalseなら大文字小文字を区別しない
        /// </summary>
        public bool PeekText(string compare, bool upperLower=false)
        {
            string peek = PeekText();
            return upperLower ? peek == compare: peek.ToLower() == compare.ToLower();
        }

        // -- 以下 private

        /// <summary>
        /// 現在の解析位置からスペース相当の文字列を読み飛ばす。
        /// </summary>
        private void SkipSpace()
        {
            while (index < Text.Length)
            {
                var c = Text[index];
                if (c == ' ' || c == '\t' /* BOMは除去されている */)
                {
                    // 読み飛ばす
                    index++;
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// 現在解析しているテキスト
        /// </summary>
        private string text;

        /// <summary>
        /// 現在解析している場所
        /// </summary>
        private int index;

        /// <summary>
        /// 先読みした文字列。次回のParseText()で使い回される。
        /// </summary>
        private string peek;

    }
}
