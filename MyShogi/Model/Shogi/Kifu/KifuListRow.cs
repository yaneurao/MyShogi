namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜ウインドウに表示している1行分のデータ
    /// </summary>
    public class KifuListRow
    {
        public KifuListRow(string ply_string,string move_string ,string consume_time /* あとで ,string total_time */)
        {
            PlyString = ply_string;
            MoveString = move_string;
            ConsumeTime = consume_time;
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
        public string ConsumeTime { get; private set; }
    }
}
