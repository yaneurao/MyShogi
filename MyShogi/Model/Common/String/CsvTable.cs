using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyShogi.Model.Common.String
{
    /// <summary>
    /// Csvファイルのメモリ上の実体
    /// </summary>
    [Serializable]
    public class CsvTable : IEnumerable<List<string>>
    {
        #region コンストラクタとCsvファイルの読み書き。

        public CsvTable()
        {
            Table = new List<List<string>>();
        }

        /// <summary>
        /// 文字列から直接CsvTableを構築する
        /// </summary>
        /// <param name="csvText"></param>
        public CsvTable(string csvText)
        {
            var parser = new CsvParser() { Encode = Encode };
            Table = parser.Parse(csvText);
        }

        public CsvTable(List<List<string>> table)
        {
            Table = table;
        }

        /// <summary>
        /// ファイルを読み書きするときのencode。ExcelのCSVファイルならsjisであるべきだが、デフォルトではUTF8にしておく。
        /// </summary>
        public Encoding Encode { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Csvファイルを読み込む。Openできないときは例外が飛ぶ。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public void Read(string path)
        {
            var parser = new CsvParser() { Encode = Encode };
            var table = parser.ReadFile(path);
            Table = table;
        }

        /// <summary>
        /// ファイルを開くが、存在しなくて開けないときは指定された列名をもつCsvTableを新規作成。
        /// 
        /// [注意]
        /// ファイルが存在して開けなかったときは例外が飛ぶ。(そのファイルを上書きして破損させるとまずいので)
        /// また、ファイルを開いたときにkeysに指定されているkeyが足りなければAppendKeysでkeyの追加を行なう。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public void ReadOrCreate(string path)
        {
            try
            {
                Read(path);
            }
            catch
            {
                if (File.Exists(path))
                    throw new IOException("指定されたCSVファイルは存在しますが、何らかの理由で開けませんでした。\r\n" + path);

                //	それ以外は空のCsvTableを作る。
                Table = new List<List<string>>();
            }
        }

        /// <summary>
        /// このテーブルをcsvファイルとして保存する。
        /// </summary>
        /// <param name="path"></param>
        public void Write(string path)
        {
            var parser = new CsvParser() { Encode = Encode };
            parser.WriteFile(path, Table);
        }

        /// <summary>
        /// 直接csvファイルに書きだす
        /// 
        /// static method。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="csv"></param>
        public void WriteCsv(string path, IEnumerable<IEnumerable<string>> csv)
        {
            var parser = new CsvParser() { Encode = Encode };
            parser.WriteFile(path, csv);
        }

        /// <summary>
        /// CSVファイルに1行追加する。
        /// CSVファイルを読み込まずに何も考えずに1行appendする。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="line"></param>
        public void AppendLine(string path , IEnumerable<string> line)
        {
            var parser = new CsvParser() { Encode = Encode };
            parser.AppendLine(path , line);
        }

        #endregion

        #region セルの取得/設定をするための基本メソッド

        /// <summary>
        /// 指定した位置のセルに文字列を設定します。
        /// 範囲外の位置を指定した場合は自動拡張されます。
        /// ただしxまたはyの値が負数だと例外がスローされます。
        /// (おそらくそれはロジックのエラーなので)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <exception cref="IndexOutOfRangeException">xまたはyの値が負数の場合にthrowされます。</exception>
        public virtual void Set(int x, int y, string value)
        {
            // 負の値ならば例外が飛ぶ。
            if (y < 0 || x < 0)
                throw new IndexOutOfRangeException();

            var csv = Table;

            // line数が足りなければ自動拡張する。
            while (csv.Count <= y)
                csv.Add(new List<string>());

            // column数が足りなければ自動拡張する。
            while (csv[y].Count <= x)
                csv[y].Add(null);

            csv[y][x] = value;

        }

        /// <summary>
        /// csvのセルの取得。範囲外でもエラーにならない。nullが返る。
        /// ただし、x,yが負の値だと例外が飛ぶ。
        /// (おそらくそれはロジックのエラーなので)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <exception cref="IndexOutOfRangeException">xまたはyの値が負数の場合にスローされます。</exception>
        public virtual string Get(int x, int y)
        {
            // 負の値ならば例外が飛ぶ。
            if (y < 0 || x < 0)
                throw new IndexOutOfRangeException();

            var csv = Table;

            // 自動拡張はしないので存在しないところにアクセスすると例外が出るので
            // そのときはnullを返す
            try
            {
                if (y < csv.Count)
                {
                    var line = csv[y];
                    if (x < line.Count)
                        return line[x];
                }
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// y行目を得る。
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<string> GetLine(int y)
        {
            var csv = Table;
            if (y < csv.Count)
                return csv[y];
            return new List<string>();
        }

        /// <summary>
        /// 1行追加する。末尾に追加する。
        /// 
        /// [備考]
        /// CsvNamedLineを追加するAppendもあるので注意。
        /// </summary>
        /// <param name="line"></param>
        public void Append(IEnumerable<string> line)
        {
            var y = Height;
            line.For((s, i) => Set(i, y, s));
        }

        #endregion

        #region プロパティとインデクサ

        /// <summary>
        /// テーブル全体のうち最大の縦幅を持つ列の縦幅を得る
        /// </summary>
        public int Height
        {
            get
            {
                return Table.Count;
            }
        }

        /// <summary>
        /// インデクサ
        /// 
        /// これによるアクセスは自動拡張される。マイナス方向へのアクセスは例外がthrowされる。
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<string> this[int y]
        {
            get { return GetLine(y); }
        }

        /// <summary>
        /// 元になるCSVテーブルを直接設定します。
        /// </summary>
        public List<List<string>> Table
        {
            set
            {
                _table = value;
            }
            protected get { return _table; }
        }
        private List<List<string>> _table;

        #endregion

        #region Enumerator

        /// <summary>
        /// Csvの1行に相当する部分Viewを生成して返すEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Csvの1行に相当する部分Viewを生成して返すEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<List<string>> GetEnumerator()
        {
            var height = Height;
            for (var i = 0; i < height; ++i)
            {
                yield return Table[i];
            }
        }
        #endregion
    }

    public static class CsvExtensions
    {
        /// <summary>
        /// Csvファイルのセルの出力時に '"'が入っているとエスケープしないといけないので
        /// そのエスケープ処理をやってくれる。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string EscapeForCsv(this string text)
        {
            if (text.Contains("\"") || text.Contains("\r\n"))
            {
                // エスケープ必要!!
                // '"' を'""'に置換し、全体を'"'で囲む。
                text = "\"" + text.Replace("\"", "\"\"") + "\"";
            }
            return text;
        }

        #region アルゴリズム・反復子

        // Range(10).Do( ... ) みたいなに感じに実行するのに使う。

        // 引数取るバージョン
        public static void For<T>(this IEnumerable<T> enu, Action<T> act)
        {
            foreach (var el in enu)
                act(el);
        }
        // 引数取らないバージョン
        public static void For<T>(this IEnumerable<T> enu, Action act)
        {
            enu.For(act);
        }
        // 引数取り、かつ、ループ変数的なものもあるバージョン
        public static void For<T>(this IEnumerable<T> enu, Action<T, int> act)
        {
            int i = 0;
            foreach (var el in enu)
                act(el, i++);
        }

        // 3.For(.. ); みたいに書けて便利かも知れん。
        public static void For(this int num, Action act)
        {
            for (int i = 0; i < num; ++i)
                act();
        }
        public static void For(this int num, Action<int> act)
        {
            for (int i = 0; i < num; ++i)
                act(i);
        }

        public static IEnumerable<int> Range(int num)
        {
            return Enumerable.Range(0, num);
        }

        // 1..3.For{ i | state }
        // => Range(1,3-1).Do( i => state );

        public static IEnumerable<TSource> Shuffle<TSource>(this TSource[] self, Random r)
        {
            // Fisher-Yates
            var indices = Enumerable.Range(0, self.Length).ToArray();

            int i = self.Count();
            while (i > 1)
            {
                int j = r.Next(i--);
                var temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }

            foreach (var index in indices)
                yield return self[index];
        }

        public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> self, Random r)
        {
            // Fisher-Yates
            var array = self.ToArray();
            var indices = Enumerable.Range(0, array.Length).ToArray();

            int i = array.Count();
            while (i > 1)
            {
                int j = r.Next(i--);
                var temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }

            foreach (var index in indices)
                yield return array[index];
        }

        /// <summary>
        /// valueがこの何番目にあるのかを調べるメソッド
        /// 
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <returns>keyが見つからないときは-1。</returns>
        public static int IndexOf<T>(this IEnumerable<T> self, T value)
        {
            int i = 0;
            foreach (var x in self)
            {
                if (x.Equals(value))
                    return i;
                ++i;
            }
            return -1;
        }

        /// <summary>
        /// selfにvalueが何番目にあるのかを調べるメソッド
        /// 同じ位置においてself2はvalue2でなければならない。
        /// 
        /// つまり
        /// self[i]==value1 かつ self2[i]==value2
        /// であるiを返す。
        /// </summary>
        /// <param name="self1"></param>
        /// <param name="value1"></param>
        /// <param name="self2"></param>
        /// <param name="value2"></param>
        /// <returns>keyが見つからないときは-1。</returns>
        public static int IndexOf<T1, T2>(this IEnumerable<T1> self1, T1 value1, IEnumerable<T2> self2, T2 value2)
        {
            int i = 0;
            var ie = self2.GetEnumerator();
            foreach (var x in self1)
            {
                if (!ie.MoveNext())
                    break; // self2に要素がないのでもうダメポ
                if (x.Equals(value1) && ie.Current.Equals(value2))
                    return i;
                ++i;
            }
            return -1;
        }

        /// <summary>
        /// selfにvalueが何番目にあるのかを調べるメソッド
        /// 同じ位置においてself2はvalue2でなければならない。
        /// 
        /// つまり
        /// self[i]==value1 かつ self2[i]==value2 かつ self3[i]==value3
        /// であるiを返す。
        /// </summary>
        /// <param name="self1"></param>
        /// <param name="value1"></param>
        /// <param name="self2"></param>
        /// <param name="value2"></param>
        /// <param name="self3"></param>
        /// <param name="value3"></param>
        /// <returns>keyが見つからないときは-1。</returns>
        public static int IndexOf<T1, T2, T3>(this IEnumerable<T1> self1, T1 value1
            , IEnumerable<T2> self2, T2 value2
            , IEnumerable<T3> self3, T3 value3
            )
        {
            int i = 0;
            var ie2 = self2.GetEnumerator();
            var ie3 = self3.GetEnumerator();
            foreach (var x in self1)
            {
                if (!ie2.MoveNext() || !ie3.MoveNext())
                    break;
                if (x.Equals(value1) && ie2.Current.Equals(value2) && ie3.Current.Equals(value3))
                    return i;
                ++i;
            }
            return -1;
        }

        /// <summary>
        /// スクリプト側のforを実装するのにint型に対するEnumeratorが必要だったので作った。
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static IEnumerator<int> GetEnumerator(this int num)
        {
            for (int i = 0; i < num; ++i)
                yield return i;
        }
        #endregion

        #region コンテナ系
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// byte[] を結合する。
        /// 
        /// data1.Concat(data2).ToArray() 遅いような気がして専用のを書いた。
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns>片側がnullならもう片側が返る。両方nullならばnullが返る。</returns>
        public static byte[] Merge(this byte[] data1, byte[] data2)
        {
            if (data1 == null)
                return data2;
            if (data2 == null)
                return data1;

            var data = new byte[data1.Length + data2.Length];

            System.Buffer.BlockCopy(data1, 0, data, 0, data1.Length);
            System.Buffer.BlockCopy(data2, 0, data, data1.Length, data2.Length);

            return data;
        }

        #endregion

    }

}
