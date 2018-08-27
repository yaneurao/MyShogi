using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// Csvファイルのメモリ上の実体
    /// </summary>
    [Serializable]
    public class CsvTable : ISerializable, IEnumerable<CsvLine>
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
            Table = new CsvParser().Parse(csvText);
        }

        public CsvTable(List<List<string>> csv)
        {
            Table = csv;
        }

        /// <summary>
        /// keyを持った空のラインを作成する
        /// </summary>
        /// <param name="keys"></param>
        public CsvTable(IEnumerable<string> keys, bool treatAsKeyValueCsv = false)
        {
            var table = new List<List<string>>();

            if (!treatAsKeyValueCsv)
            {
                var namedLine = new List<string> { "列名" };
                namedLine.AddRange(keys);
                table.Add(namedLine);
                //	table.Add(new List<string>());
                // あかん。↑これがあるとHeight==2になってしまい、named lineが一行あることになってしまう。
                // 初期状態ではnamed line == 0でないとAppendで追記する場所がわからなくなって困る。
                Table = table;
            }
            else
            {
                Table = table;
                this._ColumnNameStringPosition = Tuple.Create(-1, 0, "列名");
                if (keys != null)
                    AppendKeys(keys);
                // 以上の結果、高さがなければ空の行を挿入する。
                // ケース1) keysがnullまたは空だった。
                if (Height == 0)
                    Set(0, 0, "");
            }
        }

        /// <summary>
        /// Csvファイルを読み込む。Openできないときは例外が飛ぶ。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="treatAsKeyValueCsv">列名つきCsvとみなすのか。これがtrueだと(-1,0)に“列名”と書いてあるものとみなす。
        /// つまり1行目は列名が並んでいるものとみなす。
        /// </param>
        /// <returns></returns>
        public static CsvTable Read(string path, bool treatAsKeyValueCsv = false)
        {
            var csv = new CsvTable();
            var table = new CsvParser().ReadFile(path);
            csv.Table = table;

            if (treatAsKeyValueCsv)
            {
                var info = csv.FindColumnNameString();
                if (info.Item1 == int.MinValue)
                    csv._ColumnNameStringPosition = Tuple.Create(-1, 0, "列名");
                if (csv.Height == 0)
                    csv.Set(0, 0, "");
            }

            return csv;
        }

        /// <summary>
        /// ファイルを開くが、存在しなくて開けないときは指定された列名をもつCsvTableを新規作成。
        /// 
        /// [注意]
        /// ファイルが存在して開けなかったときは例外が飛ぶ。(そのファイルを上書きして破損させるとまずいので)
        /// また、ファイルを開いたときにkeysに指定されているkeyが足りなければAppendKeysでkeyの追加を行なう。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="keys">列名の配列。</param>
        /// <param name="treatAsKeyValueCsv">列名つきCsvとみなすのか。これがtrueだと(-1,0)に“列名”と書いてあるものとみなす。
        /// つまり1行目は列名が並んでいるものとみなす。
        /// </param>
        /// <returns></returns>
        public static CsvTable ReadOrCreate(string path, IEnumerable<string> keys = null, bool treatAsKeyValueCsv = false)
        {
            CsvTable csv;
            try
            {
                csv = Read(path, treatAsKeyValueCsv);
            }
            catch
            {
                if (File.Exists(path))
                    throw new IOException("指定されたCSVファイルは存在しますが、何らかの理由で開けませんでした。\r\n" + path);

                //	それ以外は空のCsvTableを作る。

                csv = new CsvTable();

                // treatAsKeyValueCsvがtrueならば列名があるものとして扱う。
                if (treatAsKeyValueCsv)
                {
                    csv._ColumnNameStringPosition = Tuple.Create(-1, 0, "列名");
                }
                else
                {
                    csv[0][0] = "列名";
                }
            }

            // keyのうち足りないものがあれば追加しておく。
            if (keys != null)
                csv.AppendKeys(keys);

            // 以上の結果、高さがなければ空の行を挿入する。
            //	ケース1) 空のファイルを読み込んだ
            //  ケース2) ファイルは新規作成だったがkeysがnullだった。
            if (treatAsKeyValueCsv && csv.Height == 0)
                csv.Set(0, 0, "");

            return csv;
        }


        /// <summary>
        /// 部分Viewをコンストラクタで作るため
        /// </summary>
        internal CsvTable(CsvTable refTable, int offsetX, int offsetY, bool transposed)
        {
            this.RefTable = refTable;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.Transposed = transposed;
            this.AutoTrimSpaces = refTable.AutoTrimSpaces;
        }

        /// <summary>
        /// テーブルの部分viewを作って返す。
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="transposed">転置する(X,Y軸を入れ換える)かどうか</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">xまたはyの値が負数の場合にスローされます。</exception>
        public CsvTable PartialView(int offsetX, int offsetY, bool transposed)
        {
            if (offsetX < 0 || offsetY < 0)
                throw new IndexOutOfRangeException();
            return new CsvTable(this, offsetX, offsetY, transposed);
        }

        /// <summary>
        /// このテーブルをcsvファイルとして保存する。
        /// </summary>
        /// <param name="path"></param>
        public void Write(string path)
        {
            if (RefTable == null)
                new CsvParser().WriteFile(path, Table);
            else
                // 部分ビューに対する保存
                new CsvParser().WriteFile(path, ToList());
        }

        /// <summary>
        /// 直接csvファイルに書きだす
        /// 
        /// static method。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="csv"></param>
        public static void WriteCsv(string path, List<List<string>> csv)
        {
            new CsvParser().WriteFile(path, csv);
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

            if (RefTable == null)
            {
                var csv = Table;

                // line数が足りなければ自動拡張する。
                while (csv.Count <= y)
                    csv.Add(new List<string>());
                // column数が足りなければ自動拡張する。
                while (csv[y].Count <= x)
                    csv[y].Add(string.Empty);

                // ここを書き換えることをsumOfCloumnsに反映させる
                {
                    // 横幅が足りないので拡張する
                    while (_sumOfColumns.Count <= x)
                        _sumOfColumns.Add(0);

                    // 書き換え前の値が空白でないなら1引く。
                    if (!string.IsNullOrEmpty(csv[y][x]))
                        _sumOfColumns[x]--;

                    // 書き換え後の値が空白でないなら1足す
                    if (!string.IsNullOrEmpty(value))
                        _sumOfColumns[x]++;

                    // 書き換えによって末尾がゼロになったらsumOfColumnsを縮める
                    var iter = _sumOfColumns.Count;
                    var count = 0;
                    while (true)
                    {
                        iter--;
                        // これ以上縮まないのか
                        if (iter < 0 || _sumOfColumns[iter] != 0)
                            break;
                        count++;
                    }
                    if (count != 0)
                        _sumOfColumns.RemoveRange(iter + 1, count);
                }

                // valueとしてnullは書き込めないようにしておく。
                if (value == null)
                    value = string.Empty;

                csv[y][x] = value;

            }
            else
            {
                if (!Transposed)
                    RefTable.Set(x + OffsetX, y + OffsetY, value);
                else
                    RefTable.Set(y + OffsetX, x + OffsetY, value);
            }
        }

        /// <summary>
        /// csvのセルの取得。範囲外でもエラーにならない。
        /// ただし、x,yが負の値だと例外が飛ぶ。
        /// (おそらくそれはロジックのエラーなので)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>nullが返ることはありません</returns>
        /// <exception cref="IndexOutOfRangeException">xまたはyの値が負数の場合にスローされます。</exception>
        public virtual string Get(int x, int y)
        {
            // 負の値ならば例外が飛ぶ。
            if (y < 0 || x < 0)
                throw new IndexOutOfRangeException();

            if (RefTable == null)
            {
                var csv = Table;

                // 自動拡張はしないので存在しないところにアクセスすると例外が出るので
                // そのときはstring.Emptyを返す
                try
                {
                    if (y < csv.Count)
                    {
                        var line = csv[y];
                        if (x < line.Count)
                            if (this.AutoTrimSpaces)
                                return line[x].Trim();
                            else
                                return line[x];
                    }
                }
                catch
                {
                }
                return string.Empty;
            }
            else
            {
                if (!Transposed)
                    return RefTable.Get(x + OffsetX, y + OffsetY);
                else
                    return RefTable.Get(y + OffsetX, x + OffsetY);
            }
        }

        #endregion

        #region 行に対する操作
        /// <summary>
        /// 指定された行番号のセルを全て置換します。
        /// </summary>
        /// <param name="lineNo">0以上の行番号</param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">newLineの値がnullの場合にthrowされます。</exception>
        /// <exception cref="IndexOutOfRangeException">lineNoの値が負数の場合にthrowされます。</exception>
        public void ReplaceLine(int lineNo, List<string> newLine)
        {
            if (lineNo < 0)
                throw new IndexOutOfRangeException();
            if (newLine == null)
                throw new ArgumentNullException("newLine");

            for (int i = 0; i < newLine.Count; ++i)
            {
                var str = newLine[i];
                Set(i, lineNo, str);
            }

            Truncate(newLine.Count, lineNo, true);
        }

        /// <summary>
        /// 1行をcloneして返します。
        /// </summary>
        /// <param name="lineNo">0以上の行番号</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">lineNoの値が負数の場合にthrowされます。</exception>
        public List<string> CloneLine(int lineNo)
        {
            if (lineNo < 0)
                throw new IndexOutOfRangeException();

            var list = new List<string>();
            var w = GetWidth(lineNo);
            for (int i = 0; i < w; ++i)
            {
                list.Add(Get(i, lineNo));
            }
            return list;
        }

        /// <summary>
        /// 指定したセル位置を開始位置として、指定した方向にあるセルを全て削除します。
        /// </summary>
        /// <param name="x">削除を開始するセルのカラム番号</param>
        /// <param name="y">削除を開始するセルの行番号</param>
        /// <param name="isRight">trueなら右方向へ削除。falseなら下方向へ削除</param>
        /// <exception cref="IndexOutOfRangeException">xまたはyの値が負数の場合にスローされます。</exception>
        public void Truncate(int x, int y, bool isRight)
        {
            // 負の値ならば例外が飛ぶ。
            if (y < 0 || x < 0)
                throw new IndexOutOfRangeException();

            if (RefTable == null)
            {
                if (isRight)
                {
                    if (y < Table.Count)
                    {
                        var line = Table[y];
                        if (line != null && x < line.Count)
                            line.RemoveRange(x, line.Count - x);
                    }
                }
                else
                {
                    var height = Table.Count;
                    for (int y1 = y; y1 < height; ++y1)
                    {
                        var line = Table[y1];
                        if (line != null && x < line.Count)
                            line[x] = string.Empty;
                    }
                }
            }
            else
            {
                if (!Transposed)
                    RefTable.Truncate(x + OffsetX, y + OffsetY, isRight);
                else
                    RefTable.Truncate(y + OffsetX, x + OffsetY, !isRight);
            }
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

        #region 馬鹿にみたいにつくったIndexOf。これなにげ便利だから…。

        /// <summary>
        /// 列名がname1の列で値がvalue1と一致する行を探す。
        /// なければ-1が返る。
        /// 
        /// ここで返ってきた値でGetNamedLine(index)して、そのCsvNamedLineに対して
        /// インデクサなどでアクセスすることを想定している。
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="value1"></param>
        /// <returns></returns>
        public int IndexOf(string name1, string value1)
        {
            var line1 = GetLineByName(name1);

            var index = -1;
            line1.For((s, i) =>
            {
                if (s == value1)
                {
                    index = i;
                    return;
                }
            });
            return index;
        }

        /// <summary>
        /// 列名がname1の列で値がvalue1
        /// かつ
        /// 列名がname2の列で値がvalue2
        /// と一致する行を探す。
        /// なければ-1が返る。
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="value1"></param>
        /// <returns></returns>
        public int IndexOf(string name1, string value1, string name2, string value2)
        {
            var line1 = GetLineByName(name1);
            var line2 = GetLineByName(name2);
            // line1と2は同じ行数あるはずだし、なくても自動拡張されるから心配ない

            var index = -1;
            line1.For((s, i) =>
            {
                if (s == value1 && line2[i] == value2)
                {
                    index = i;
                    return;
                }
            });
            return index;
        }

        /// <summary>
        /// 列名がname1の列で値がvalue1
        /// 列名がname2の列で値がvalue2
        /// 列名がname3の列で値がvalue3
        /// と一致する行を探す。
        /// なければ-1が返る。
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="value1"></param>
        /// <returns></returns>
        public int IndexOf(string name1, string value1, string name2, string value2, string name3, string value3)
        {
            var line1 = GetLineByName(name1);
            var line2 = GetLineByName(name2);
            var line3 = GetLineByName(name3);

            var index = -1;
            line1.For((s, i) =>
            {
                if (s == value1 && line2[i] == value2 && line3[i] == value3)
                {
                    index = i;
                    return;
                }
            });
            return index;
        }

        /// <summary>
        /// 列番号column1の行にvalue1が一致する行があるか。
        /// 
        /// 行があればその行の番号を返す。
        /// なければ-1が返る。
        /// </summary>
        /// <param name="column1"></param>
        /// <param name="value1"></param>
        /// <returns></returns>
        public int IndexOf(int column1, string value1)
        {
            var index = -1;
            var h = Height;
            h.For((i) =>
            {
                if (Get(column1, i) == value1)
                {
                    index = i;
                    return;
                }
            });
            return index;
        }

        /// <summary>
        /// 列番号column1の行にvalue1が一致
        /// かつ
        /// 列番号column2の行にvalue2が一致
        /// する行があるか。
        /// 
        /// 行があればその行の番号を返す。
        /// なければ-1が返る。
        /// </summary>
        /// <param name="column1"></param>
        /// <param name="value1"></param>
        /// <returns></returns>
        public int IndexOf(int column1, string value1, int column2, string value2)
        {
            var index = -1;
            var h = Height;
            h.For((i) =>
            {
                if ((Get(column1, i) == value1) && (Get(column2, i) == value2))
                {
                    index = i;
                    return;
                }
            });
            return index;
        }

        /// <summary>
        /// 列番号column1の行にvalue1が一致
        /// かつ
        /// 列番号column2の行にvalue2が一致
        /// かつ
        /// 列番号column3の行にvalue3が一致
        /// する行があるか。
        /// 
        /// 行があればその行の番号を返す。
        /// なければ-1が返る。
        /// </summary>
        /// <param name="column1"></param>
        /// <param name="value1"></param>
        /// <returns></returns>
        public int IndexOf(int column1, string value1, int column2, string value2, int column3, string value3)
        {
            var index = -1;
            var h = Height;
            h.For((i) =>
            {
                if ((Get(column1, i) == value1) && (Get(column2, i) == value2) && (Get(column3, i) == value3))
                {
                    index = i;
                    return;
                }
            });
            return index;
        }

        #endregion

        #region NamedLine関係

        /// <summary>
        /// 列名を指定してその列方向のViewを得る。
        /// 
        /// [例]
        /// 列名  氏名 年齢
        ///       山田  20
        ///       田中  18
        /// 
        /// となっているCsvファイルの場合、
        /// GetLineByName("氏名")とするとCsvColumnが返る。
        /// 
        ///   var line = csvTable.GetLineByName("氏名")
        ///   Console.WriteLine(line[0]); // 山田 が出力される
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CsvLine GetLineByName(string name)
        {
            // 列名もしくは行名というセルを10×10の範囲で探す
            var info = EnsureColumnNameStringExists();
            var x = info.Item1;
            var y = info.Item2;
            var text = info.Item3;
            if (text == "列名")
            {
                // 見つけた。ここを横方向に限界までスキャン
                for (++x; x < GetWidth(y); ++x)
                {
                    if (Get(x, y) == name)
                    {
                        // 列名見つけた。
                        // このひとつ下のセルからの縦方向のViewを返す
                        //								return new CsvColumn(this, x, y + 1);
                        return new CsvLine(PartialView(x, y + 1, true), 0, 0);
                    }
                }
                // 見つからない。
                throw new KeyNotFoundException("指定されたキー列「" + name + "」はCsvTableに存在しません。");
            }
            else if (text == "行名")
            {
                // 見つけた。ここを縦方向に限界までスキャン
                for (++y; y < GetHeight(x); ++y)
                {
                    if (Get(x, y) == name)
                    {
                        // 列名見つけた。
                        // このひとつ右のセルからの横方向のViewを返す
                        return new CsvLine(this, x + 1, y);
                    }
                }
                // 見つからない。
                throw new KeyNotFoundException("指定されたキー列「" + name + "」はCsvTableに存在しません。");
            }
            throw new KeyNotFoundException("「列名」または「行名」セルが見つかりません。");
        }

        /// <summary>
        /// "列名"・"行名"から数えてlineNo目の行を取得する。
        /// そのときに列名も保持してあるので列名で参照できる。
        /// 
        /// 例)
        /// 列名  氏名 年齢
        ///       山田  20
        ///       田中  18
        /// 
        /// var line = GetNamedLine(1); // 2行目を取得
        /// var name = line["氏名"]; // 田中
        /// </summary>
        /// <param name="lineNo"> -1を指定すると列名が得られる。列名は[0]からはじまり、Keys.Lengthだけ存在することが
        /// 保証されている。列名の追加は、こいつに対してline[Keys.Length]から書きこんでいけば追加される。
        /// </param>
        /// <returns></returns>
        public CsvNamedLine GetNamedLine(int lineNo)
        {
            var info = EnsureColumnNameStringExists();
            var x = info.Item1;
            var y = info.Item2;
            var text = info.Item3;

            if (text == "列名")
            {
                // 見つけた。ここを横方向に限界までスキャン
                // このひとつ下のセルからの横方向のViewを返す
                return new CsvNamedLine(this, x, y + 1 + lineNo, y);
            }
            if (text == "行名")
            {
                // 見つけた。ここを縦方向に限界までスキャン
                // このひとつ右のセルからの縦方向のViewを返す
                // return new CsvNamedColumn(this, x + 1 + lineNo, y, x);
                return new CsvNamedLine(PartialView(x, y, true), 0, 1 + lineNo, 0);
            }
            return null;
        }

        /// <summary>
        /// 列名だけ持っていて、空の1行を返す
        /// </summary>
        /// <returns></returns>
        public CsvNamedLine GetNamedLine()
        {
            return new CsvNamedLine(Keys);
        }

        /// <summary>
        /// 1行追加する。"列名"・"行名"があるとき用。
        /// </summary>
        /// <param name="line"></param>
        public void Append(CsvNamedLine line)
        {
            OverWrite(line, NamedLineCount);
        }

        /// <summary>
        /// 行の上書き。
        /// 
        /// CsvNamedLine側にCsvTable側の持っていないKeyがある場合、セルは一切更新されず例外が飛ぶ。
        /// トランザクション的な動作となっている。
        /// </summary>
        /// <param name="line"></param>
        /// <param name="namedLineNo">named lineとしてみたときの何行目であるか。元あった行に書き戻すならば、GetNamedLineで指定した引数の値。</param>
        public void OverWrite(CsvNamedLine line, int namedLineNo)
        {
            var info = EnsureColumnNameStringExists();
            var x = info.Item1;
            var y = info.Item2;

            var keys = line.Keys;
            var keysAndIndices = KeysAndIndices;

            switch (info.Item3)
            {
                case "列名":
                    {
                        foreach (var key in keys)
                            if (!keysAndIndices.ContainsKey(key))
                                throw new KeyNotFoundException("このCsvTableにCsvNamedLineのキー列「" + key + "」が存在しません。");

                        var i = 0;
                        foreach (var key in keys)
                        {
                            var value = line[i];
                            Set(x + keysAndIndices[key], y + 1 + namedLineNo, value);
                            ++i;
                        }
                        break;
                    }

                case "行名":
                    {
                        foreach (var key in keys)
                            if (!keysAndIndices.ContainsKey(key))
                                throw new KeyNotFoundException("このCsvTableにCsvNamedLineのキー列「" + key + "」が存在しません。");

                        var i = 0;
                        foreach (var key in keys)
                        {
                            var value = line[i];
                            Set(x + 1 + namedLineNo, y + keysAndIndices[key], value);
                            ++i;
                        }
                        break;
                    }

                default:
                    throw new KeyNotFoundException("「列名」または「行名」セルが見つかりません。");
            }
        }

        /// <summary>
        /// keyの追加を行なう。
        /// 
        /// [注意]
        /// "列名"・"行名"と書かれたセルがなければ例外がでる。
        /// </summary>
        /// <param name="keys"></param>
        public void AppendKeys(IEnumerable<string> keys)
        {
            var myKeys = new HashSet<string>(Keys);
            var newKeys = new HashSet<string>();

            var lastIndex = myKeys.Count;
            var line = GetNamedLine(-1); // 列名を得る。

            foreach (var key in keys)
            {
                if (!myKeys.Contains(key))
                {
                    // keyがなければ追加すればいいじゃない…。

                    // 追加済みであれば重複keyなので例外が飛ぶ
                    if (newKeys.Contains(key))
                        throw new InvalidOperationException("指定されたキー群の要素が重複しています。Key = " + key);

                    newKeys.Add(key);
                    line[lastIndex++] = key; // これでkeyの追加が出来ているはずなんだ…
                }
            }

        }

        /// <summary>
        /// NamedLineとして数えたときに何行あるか。
        /// 
        /// "列名"・"行名"がない場合、-1が返る。(例外は出ない)
        /// </summary>
        public int NamedLineCount
        {
            get
            {
                try
                {
                    var info = FindColumnNameString();
                    var x = info.Item1;
                    var y = info.Item2;
                    switch (info.Item3)
                    {
                        case "列名":
                            return Height - y - 1;

                        case "行名":
                            return Width - x - 1;

                        // おかしい名前が書き換わっている..
                        default:
                            return -1;
                    }
                }
                catch
                {
                    return -1;
                }
            }
        }


        /// <summary>
        /// 列名・行名に書かれているキーをすべて返す。
        /// "列名"・"行名"と書いたセルがないならば例外が飛ぶ。
        /// </summary>
        public string[] Keys
        {
            get
            {
                var info = EnsureColumnNameStringExists();
                var x = info.Item1;
                var y = info.Item2;
                switch (info.Item3)
                {
                    case "列名":
                        {
                            var list = new List<string>();
                            while (true)
                            {
                                var text = Get(++x, y);
                                if (string.IsNullOrEmpty(text))
                                    return list.ToArray();

                                list.Add(text);
                            }
                        }

                    case "行名":
                        {
                            var list = new List<string>();
                            while (true)
                            {
                                var text = Get(x, ++y);
                                if (string.IsNullOrEmpty(text))
                                    return list.ToArray();

                                list.Add(text);
                            }
                        }

                    default:
                        throw new KeyNotFoundException("「列名」または「行名」セルが見つかりません。");
                }
            }
        }
        /// <summary>
        /// 列名・行名に書かれているキーとそのindexをすべて返す。
        /// "列名"・"行名"と書いたセルがないならば例外が飛ぶ。
        /// 
        /// [注意]
        /// ここで言うindexは、named index。すなわち、"列名"・"行名"のところから数えた数値。
        /// たとえば"列名","氏名"となっていれば"氏名"のindexは1。
        /// </summary>
        public Dictionary<string, int> KeysAndIndices
        {
            get
            {
                var info = EnsureColumnNameStringExists();
                var x = info.Item1;
                var y = info.Item2;
                switch (info.Item3)
                {
                    case "列名":
                        {
                            var list = new Dictionary<string, int>();
                            var i = 0;
                            while (true)
                            {
                                var text = Get(++x, y);
                                if (string.IsNullOrEmpty(text))
                                    return list;

                                list.Add(text, ++i);
                            }
                        }

                    case "行名":
                        {
                            var list = new Dictionary<string, int>();
                            var i = 0;
                            while (true)
                            {
                                var text = Get(x, ++y);
                                if (string.IsNullOrEmpty(text))
                                    return list;

                                list.Add(text, ++i);
                            }
                        }

                    default:
                        throw new KeyNotFoundException("「列名」または「行名」セルが見つかりません。");
                }
            }
        }

        /// <summary>
        /// "列名"・"行名"と書かれているセルの位置(x,y)を返す。
        /// 
        /// 見つからなければ(int.MinValue,int.MinValue)が返る。
        /// </summary>
        public Tuple<int, int, string> FindColumnNameString()
        {
            if (_ColumnNameStringPosition != null)
                return _ColumnNameStringPosition;

            // 一度見つけていれば、そこのセルの位置は不変だと仮定できるので二度目以降はサーチしない。

            // 列名もしくは行名というセルを10×10の範囲で探す
            for (int y = 0; y < System.Math.Min(10, Height); ++y)
            {
                for (int x = 0; x < System.Math.Min(10, GetWidth(y)); ++x)
                {
                    var text = Get(x, y);
                    if (text == "列名" || text == "行名")
                    {
                        _ColumnNameStringPosition = Tuple.Create(x, y, text);
                        return _ColumnNameStringPosition;
                    }
                }
            }
            return Tuple.Create(int.MinValue, int.MinValue, string.Empty);
        }


        /// <summary>
        /// "列名"・"行名"と書かれているセルの位置(x,y)と値を返す。
        /// 
        /// 見つからなければ例外が飛ぶ。
        /// </summary>
        protected Tuple<int, int, string> EnsureColumnNameStringExists()
        {
            var info = FindColumnNameString();
            if (info.Item1 == int.MinValue)
            {
                if (Height == 0 && Width == 0)
                {
                    //	空のCSVなら列名を追加して再試行する。
                    Set(0, 0, "列名");
                    info = FindColumnNameString();
                }
            }
            if (info.Item1 == int.MinValue)
            {
                //	列名(行名)セルが見つからず、空のCSVでもない。
                throw new InvalidOperationException("「列名」または「行名」セルが見つかりません。このCsvTableに対してキー列による操作を行うことはできません。");
            }
            return info;
        }

        /// <summary>
        /// 1行ずつ名前つきラインが返る。
        /// "列名" "行名"がなければ例外が飛ぶ
        /// </summary>
        public IEnumerable<CsvNamedLine> NamedLines
        {
            get
            {
                var namedLineLength = this.NamedLineCount;
                if (namedLineLength < 0)
                {
                    throw new InvalidOperationException("「列名」または「行名」セルが見つからないため、CsvNamedLineとして行を列挙できません。");
                }
                for (int i = 0; i < namedLineLength; i++)
                {
                    yield return GetNamedLine(i);
                }
            }
        }

        /// <summary>
        /// "列名"・"行名"と書かれた右下のセルを起点とするテーブルを返す。
        /// "行名"と書かれているときは反転させたものを返す。
        /// </summary>
        /// <returns></returns>
        public CsvTable GetNamedTable()
        {
            var info = EnsureColumnNameStringExists();
            var x = info.Item1;
            var y = info.Item2;
            var text = info.Item3;

            if (text == "列名")
            {
                return PartialView(x + 1, y + 1, false);
            }
            if (text == "行名")
            {
                return PartialView(x + 1, y + 1, true);
            }
            return null;
        }

        #endregion

        #region 変換子とToString

        /// <summary>
        /// まるまるClone
        /// 
        /// 部分Viewに対しても部分だけが正常にCloneされる。
        /// </summary>
        /// <returns></returns>
        public CsvTable Clone()
        {
            var table = new List<List<string>>();
            for (var i = 0; i < Height; ++i)
            {
                table.Add(CloneLine(i));
            }
            return new CsvTable(table);
        }

        /// <summary>
        /// cloneして返す。部分ビューを持っているなら部分ビューをコピーして返す
        /// </summary>
        /// <returns></returns>
        public List<List<string>> ToList()
        {
            var list = new List<List<string>>();
            for (int y = 0; y < Height; ++y)
            {
                var line = new List<string>();
                for (int x = 0; x < Width; ++x)
                {
                    line.Add(Get(x, y));
                }
                list.Add(line);
            }
            return list;
        }

        /// <summary>
        /// このテーブル（部分ビューの場合はビュー）が表す構造を、CSV形式の文字列に整形して取得します。
        /// 内部的にはCSVParserクラスを使っています。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(",", "\r\n");
        }

        /// <summary>
        /// このテーブル（部分ビューの場合はビュー）が表す構造を、CSV形式の文字列に整形して取得します。
        /// 内部的にはCSVParserクラスを使っています。
        /// </summary>
        /// <param name="elementSeparator">カラムを区切る文字列</param>
        /// <param name="lineSeparator">行を区切る文字列</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">elementSeparatorまたはlineSeparatorの値がnull</exception>
        public string ToString(string elementSeparator, string lineSeparator)
        {
            if (elementSeparator == null)
                throw new ArgumentNullException("elementSeparator");
            if (lineSeparator == null)
                throw new ArgumentNullException("lineSeparator");

            var cloned = this.ToList();
            using (var writer = new StringWriter())
            {
                var parser = new CsvParser();
                parser.element_separators = new List<string> { elementSeparator };
                parser.line_separators = new List<string> { lineSeparator };
                parser.Write(writer, cloned);
                return writer.ToString();
            }
        }

        #endregion

        #region プロパティとインデクサ

        /// <summary>
        /// テーブル全体のうち最大の横幅を持つ行の横幅を得る。
        /// 部分ビューを持っている場合は、オフセット分は引いた横幅。
        /// (部分ビューのなかで最大の横幅を持つ行の横幅とは限らない)
        /// </summary>
        public int Width
        {
            get
            {
                if (RefTable == null)
                    return _sumOfColumns.Count;
                else
                {
                    if (!Transposed)
                        return RefTable.Width - OffsetX;
                    else
                        return RefTable.Height - OffsetY;
                }
            }
        }

        /// <summary>
        /// テーブル全体のうち最大の縦幅を持つ列の縦幅を得る
        /// 部分ビューを持っている場合は、オフセット分は引いた縦幅
        /// (部分ビューのなかで最大の縦幅を持つ列の縦幅とは限らない)
        /// </summary>
        public int Height
        {
            get
            {
                if (RefTable == null)
                    return Table.Count;
                else
                {
                    if (!Transposed)
                        return RefTable.Height - OffsetY;
                    else
                        return RefTable.Width - OffsetX;
                }
            }
        }

        /// <summary>
        /// 指定したカラム番号に属するセルの総数を取得します。
        /// 正の範囲外の行番号を指定したときは0を返します。
        /// </summary>
        /// <param name="columnNo">0以上のカラム番号</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">columnNoの値が負数の場合にスローされます。</exception>
        public int GetHeight(int columnNo)
        {
            if (columnNo < 0)
                throw new IndexOutOfRangeException();

            if (RefTable == null)
            {
                //	プラス方向の範囲外になることはない
                return Table.Count;
            }
            else
            {
                if (!Transposed)
                    return RefTable.GetHeight(columnNo + OffsetX) - OffsetY;
                else
                    return RefTable.GetWidth(columnNo + OffsetY) - OffsetX;
            }
        }

        /// <summary>
        /// 指定した行番号に属するセルの総数を取得します。
        /// 正の範囲外の行番号を指定したときは0を返します。
        /// </summary>
        /// <param name="lineNo">0以上の行番号</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException">lineNoの値が負数の場合にスローされます。</exception>
        public int GetWidth(int lineNo)
        {
            if (lineNo < 0)
                throw new IndexOutOfRangeException();

            if (RefTable == null)
            {
                //	プラス方向の範囲外なら0を返す。
                if (lineNo >= Table.Count)
                    return 0;
                return Table[lineNo].Count;
            }
            else
            {
                if (!Transposed)
                    return RefTable.GetWidth(lineNo + OffsetY) - OffsetX;
                else
                    return RefTable.GetHeight(lineNo + OffsetX) - OffsetY;
            }
        }


        /// <summary>
        /// インデクサ
        /// 
        /// これによるアクセスは自動拡張される。マイナス方向へのアクセスは例外がthrowされる。
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public CsvLine this[int y]
        {
            get { return new CsvLine(this, y); }
        }

        /// <summary>
        /// 元になるCSVテーブルを直接設定します。
        /// </summary>
        public List<List<string>> Table
        {
            set
            {
                _table = value;
                InitSumOfColumns();
            }
            protected get { return _table; }
        }
        private List<List<string>> _table;

        /// <summary>
        /// Getメソッドで取得される値の先頭と末尾にある空白（改行やタブも含む）を自動でTrimするかどうかを指定します。
        /// デフォルトはtrueです。
        /// </summary>
        public bool AutoTrimSpaces
        {
            get { return _AutoTrimSpaces; }
            set { _AutoTrimSpaces = value; }
        }
        private bool _AutoTrimSpaces = true;

        #endregion

        #region 内部実装用

        /// <summary>
        /// 縦方向に空でないセルの数を数えたもの。
        /// これがないとWidth(テーブルのなかで横幅最大の行の横幅)が割り出せない。
        /// </summary>
        protected List<int> _sumOfColumns;

        /// <summary>
        /// _sumOfColumnsを初期化する。
        /// </summary>
        protected void InitSumOfColumns()
        {
            _sumOfColumns = new List<int>();

            // Tableの値が正しく設定されていることは保証されているものとする。
            foreach (var line in Table)
            {
                // 横幅が足りないので拡張する
                while (_sumOfColumns.Count < line.Count)
                    _sumOfColumns.Add(0);

                for (int x = 0; x < line.Count; ++x)
                {
                    // 空白でないなら合計値をインクリメントする
                    if (!string.IsNullOrEmpty(line[x]))
                        _sumOfColumns[x]++;
                }
            }
        }
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
        public IEnumerator<CsvLine> GetEnumerator()
        {
            var height = Height;
            for (var i = 0; i < height; ++i)
            {
                yield return new CsvLine(this, i);
            }
        }

        #endregion

        #region Serialize
        public CsvTable(SerializationInfo info, StreamingContext context)
        {
            var table = (List<List<string>>)info.GetValue("_Table", typeof(List<List<string>>));
            Table = table;
            _ColumnNameStringPosition = (Tuple<int, int, string>)
                info.GetValue("_ColumnNameStringPosition", typeof(Tuple<int, int, string>));
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            List<List<string>> table;
            if (RefTable == null)
                table = Table;
            else
                table = Clone().Table;

            info.AddValue("_Table", table);
            info.AddValue("_ColumnNameStringPosition", _ColumnNameStringPosition);
        }
        #endregion

        #region 内部実装メンバー

        // --- 部分viewのための仕組み

        // 部分viewの参照元
        protected readonly CsvTable RefTable = null;

        // 部分viewの開始座標（親となるViewの座標系での値なので注意！）
        protected readonly int OffsetX = 0;
        protected readonly int OffsetY = 0;

        // 部分viewで値にアクセスする時にX,Y軸を入れ替えるならtrue
        protected readonly bool Transposed = false;

        /// <summary>
        /// "列名"・"行名"と書かれているセルの位置。X座標、Y座標、セルの値。
        /// </summary>
        private Tuple<int, int, string> _ColumnNameStringPosition;

        #endregion
    }


    /// <summary>
    /// Csvの1行に相当する部分View
    /// List &lt;string&gt;のように使える。
    /// </summary>
    [Serializable]
    public class CsvLine : ISerializable, IEnumerable<string>
    {
        public CsvLine(CsvTable refTable, int x, int y)
        {
            RefTable = refTable;
            OffsetX = x;
            OffsetY = y;
        }

        public CsvLine(CsvTable refTable, int y)
        {
            RefTable = refTable;
            OffsetX = 0;
            OffsetY = y;
        }

        /// <summary>
        /// このコレクションの長さ
        /// </summary>
        public int Length
        {
            get { return RefTable.GetWidth(OffsetY) - OffsetX; }
        }

        /// <summary>
        /// セルの値の取得
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public string Get(int x)
        {
            return RefTable.Get(x + OffsetX, OffsetY);
        }

        /// <summary>
        /// セルの値の設定
        /// </summary>
        /// <param name="x"></param>
        /// <param name="value"></param>
        public void Set(int x, string value)
        {
            RefTable.Set(x + OffsetX, OffsetY, value);
        }

        /// <summary>
        /// インデクサ
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public string this[int x]
        {
            get { return Get(x); }
            set { Set(x, value); }
        }

        /// <summary>
        /// "1,2,3"のようにこのラインの内容をCSV的な文字列化する。
        /// そのままファイルに書きだすことによってCSVファイルになる。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var length = Length;
            var sb = new StringBuilder();
            for (int i = 0; i < length; ++i)
            {
                var text = this[i].EscapeForCsv();
                if (i != 0)
                    sb.Append(','); // 区切り記号としてのカンマ
                sb.Append(text);
            }
            return sb.ToString();
        }

        // よくわからんが、以下の2種類のEnumeratorを実装しないといけないようだ…。

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            var length = Length;
            for (int i = 0; i < length; ++i)
            {
                yield return this[i];
            }
        }

        #region Serialize
        public CsvLine(SerializationInfo info, StreamingContext context)
        {
            var items = (string[])info.GetValue("_Items", typeof(string[]));
            var csv = new List<List<string>>();
            if (items == null)
            {
                csv.Add(new List<string>());
            }
            else
            {
                csv.Add(new List<string>(items));
            }
            RefTable = new CsvTable(csv);
            OffsetX = 0;
            OffsetY = 0;
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var items = Enumerable.ToArray(this);
            info.AddValue("_Items", items);
        }
        #endregion

        // --- 親テーブルの情報

        /// <summary>
        /// 参照元テーブルの行番号を取得する。(0-origin)
        /// </summary>
        public int LineNo { get { return OffsetY; } }

        protected readonly CsvTable RefTable = null;
        protected readonly int OffsetX = 0;
        protected readonly int OffsetY = 0;
    }

    /// <summary>
    /// 列名を伴う
    /// Csvの1行に相当する部分View
    /// List &lt;string&gt;のように使える。
    /// </summary>
    [Serializable]
    public class CsvNamedLine : DynamicObject, ISerializable, IEnumerable<string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="refTable"></param>
        /// <param name="x">列名と書いてあるx座標</param>
        /// <param name="y"></param>
        /// <param name="name_y">列名の書いてあるセルのY座標</param>
        public CsvNamedLine(CsvTable refTable, int x, int y, int name_y)
        {
            RefTable = refTable;
            ValueStartX = x + 1;
            ValueStartY = y;

            NameY = name_y;
        }

        public CsvNamedLine(CsvTable refTable, int y, int name_y)
        {
            RefTable = refTable;
            ValueStartX = 0 + 1;
            ValueStartY = y;

            NameY = name_y;
        }

        /// <summary>
        /// keyを持った空のラインを作成する
        /// </summary>
        /// <param name="keys"></param>
        public CsvNamedLine(IEnumerable<string> keys)
        {
            RefTable = new CsvTable(keys);

            ValueStartX = 0 + 1;
            ValueStartY = 1;

            NameY = 0;
        }

        /// <summary>
        /// このコレクションの長さ
        /// </summary>
        public int Length
        {
            get
            {
                return
                    System.Math.Max(
                        RefTable.GetWidth(ValueStartY) - ValueStartX,
                        RefTable.GetWidth(NameY) - ValueStartX
                    );
            }
        }

        /// <summary>
        /// キーの一覧
        /// </summary>
        public IEnumerable<string> Keys
        {
            get { return RefTable.Keys; }
        }

        /// <summary>
        /// セルの値の取得
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public string Get(int x)
        {
            return RefTable.Get(x + ValueStartX, ValueStartY);
        }

        /// <summary>
        /// セルの値の設定
        /// </summary>
        /// <param name="x"></param>
        /// <param name="value"></param>
        public void Set(int x, string value)
        {
            RefTable.Set(x + ValueStartX, ValueStartY, value);
        }

        /// <summary>
        /// インデクサ
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public string this[int x]
        {
            get { return Get(x); }
            set { Set(x, value); }
        }

        /// <summary>
        /// 列名指定でセルを取得。
        /// 
        /// 列名が存在しなければ例外が飛ぶ。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get { return Get(FindName(key)); }
            set { Set(FindName(key), value); }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value as string;
            return true;
        }


        /// <summary>
        /// 列名を探して、そのindexを返す。
        /// そのあとthis[i]としてその要素にアクセスできる。
        /// 
        /// 見つからなければ例外がthrowされる。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int FindName(string key)
        {
            var length = Length;
            for (int i = 0; i < length; ++i)
            {
                if (RefTable.Get(i + ValueStartX, NameY) == key)
                {
                    return i;
                }
            }
            throw new KeyNotFoundException("指定されたキー「" + key + "」はCsvTableに存在しません。");
        }

        /// <summary>
        /// "1,2,3"のようにこのラインの内容をCSV的な文字列化する。
        /// そのままファイルに書きだすことによってCSVファイルになる。
        /// 
        /// 列名も一緒に書きだす。"列名"の文字も書きだす。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var length = Length;
            var sb = new StringBuilder();

            // --- 列名の書き出し
            sb.Append("列名");
            for (int i = 0; i < length; ++i)
            {
                sb.Append(','); // 区切り記号としてのカンマ
                var text = RefTable.Get(i + ValueStartX, NameY).EscapeForCsv();
                sb.Append(text);
            }
            sb.AppendLine();

            for (int i = 0; i < length; ++i)
            {
                sb.Append(','); // 区切り記号としてのカンマ
                var text = this[i].EscapeForCsv();
                sb.Append(text);
            }
            return sb.ToString();
        }

        // よくわからんが、以下の2種類のEnumeratorを実装しないといけないようだ…。

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            var length = Length;
            for (int i = 0; i < length; ++i)
            {
                yield return this[i];
            }
        }

        #region Serialize
        public CsvNamedLine(SerializationInfo info, StreamingContext context)
        {
            var keys = (string[])info.GetValue("_Keys", typeof(string[]));
            var values = (string[])info.GetValue("_Values", typeof(string[]));
            var csv = new List<List<string>>();
            if (keys == null || values == null)
            {
                // シリアライズに失敗しとる…。
                throw new SerializationException("CsvNamedLineのデシリアライズに失敗しました。");
            }

            // _Keysには"列名"という値は含まれていないので、先頭に追加する。
            var line = new List<string>() { "列名" };
            line.AddRange(keys);
            csv.Add(line);
            line = new List<string>() { "" }; // 列名のところに対応するValueがないことを示す。
            line.AddRange(values);
            csv.Add(line);
            ValueStartY = 1;

            RefTable = new CsvTable(csv);
            ValueStartX = 1;
            NameY = 0;
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var keys = Enumerable.Range(0, Length)
                .Select(i => RefTable.Get(i + ValueStartX, NameY))
                .ToArray();
            var values = Enumerable.ToArray(this);
            info.AddValue("_Keys", keys);
            info.AddValue("_Values", values);
        }
        #endregion

        // --- 親テーブルの情報

        /// <summary>
        /// 参照元テーブルの行番号を取得する。(0-origin)
        /// </summary>
        public int LineNo { get { return ValueStartY; } }

        protected readonly CsvTable RefTable = null;
        protected readonly int ValueStartX = 0;
        protected readonly int ValueStartY = 0;

        // 親テーブルの列名がどこにあるか。
        protected readonly int NameY;
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
