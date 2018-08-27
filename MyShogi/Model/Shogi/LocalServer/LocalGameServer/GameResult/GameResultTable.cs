using MyShogi.Model.Common.Utility;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局結果を保存するクラス
    /// </summary>
    public class GameResultTable
    {
        public void Read()
        {

        }

        public void Test()
        {
#if false
            //var table = new CsvTable();
            var table = CsvTable.Read("test.csv", true);
            table.AppendKeys(new[] { "Black2", "White2" , "KIF2"});

            table.GetNamedLine(0)["Black"] = "わたし123";
            table.GetNamedLine(0)["White"] = "あなた234";
            table.GetNamedLine(0)["KIF"] = "abc\r\ndef";
            table.GetNamedLine(0)["Black2"] = "わたし345";
            table.GetNamedLine(0)["White2"] = "あなた456";
            table.GetNamedLine(0)["KIF2"] = "abc\r\ndef222";

            table.Write("test.csv");
#endif
        }
    }
}
