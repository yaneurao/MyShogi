namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜ウインドウに表示している1行分のデータ
    /// </summary>
    public class KifuListRow
    {
        public KifuListRow(string ply_string,string move_string ,string consumption_time , string total_consumption_time)
        {
            PlyString = ply_string;
            MoveString = move_string;
            ConsumptionTime = consumption_time;
            TotalConsumptionTime = total_consumption_time;
        }

        /// <summary>
        /// 何手目であるか
        /// </summary>
        public string PlyString { get; private set; }

        /// <summary>
        /// 指し手文字列
        /// </summary>
        public string MoveString { get; private set; }

        /// <summary>
        /// 消費時間
        /// </summary>
        public string ConsumptionTime { get; private set; }

        /// <summary>
        /// 総消費時間
        /// </summary>
        public string TotalConsumptionTime { get; private set; }
    }
}
