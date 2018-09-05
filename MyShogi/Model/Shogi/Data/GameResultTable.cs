using System;
using System.Collections.Generic;
using System.Text;
using MyShogi.App;
using MyShogi.Model.Common.String;
using MyShogi.Model.Common.Utility;

namespace MyShogi.Model.Shogi.Data
{
    /// <summary>
    /// 対局結果を保存するクラス
    /// </summary>
    public class GameResultTable
    {
        /// <summary>
        /// ファイルからGameResultTableを読み込む。
        ///
        /// // 解釈できなかった行はnullになっている。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<GameResultData> ReadOrCreate(string filePath)
        {
            var csv = CsvReadOrCreate(filePath);
            var list = new List<GameResultData>();
            foreach (var line in csv)
                list.Add(GameResultData.FromLine(line));
            return list;
        }

        /// <summary>
        /// CSVファイルに1行追加する。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        public void AppendLine(string filePath , GameResultData data)
        {
            AppendLine(filePath, data.ToList());
        }

        /// <summary>
        /// ファイルの入出力で用いるencoding
        /// </summary>
        public Encoding Encode = Encoding.UTF8;

        public void Test()
        {
#if false
            ReadOrCreate("test.csv");
            foreach (var line in Csv)
            {
                foreach (var e in line)
                    Console.Write(e);
                Console.WriteLine();
            }

            var line2 = new[] { "aaa", "bbb", "なんとか\"使えるか" ,"2行\r\nのテキスト"};
            AppendLine("test.csv" , line2);
#endif
        }

        #region privates

        /// <summary>
        /// ファイルからCsvTableを読み込む。
        /// ファイルが存在しないときは作成する。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private CsvTable CsvReadOrCreate(string filePath)
        {
            var csv = new CsvTable() { Encode = Encode };
            try
            {
                csv.ReadOrCreate(filePath);
            }
            catch (Exception ex)
            {
                TheApp.app.MessageShow(ex);
            }
            return csv;
        }

        /// <summary>
        /// CSVファイルに1行追加する。
        /// CSVファイルを読み込まずに何も考えずに1行appendする。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="line"></param>
        private CsvTable AppendLine(string filePath, IEnumerable<string> line)
        {
            var csv = new CsvTable() { Encode = Encode };
            try
            {
                csv.AppendLine(filePath, line);
            }
            catch (Exception ex)
            {
                TheApp.app.MessageShow(ex);
            }
            return csv;
        }

        #endregion
    }
}
