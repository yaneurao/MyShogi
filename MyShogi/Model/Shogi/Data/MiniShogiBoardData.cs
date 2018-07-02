using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Data
{
    /// <summary>
    /// MiniShogiBoardで用いるデータ。
    /// 継ぎ盤用のデータ一式
    /// </summary>
    public class MiniShogiBoardData
    {
        /// <summary>
        /// 開始局面
        /// </summary>
        public string rootSfen;

        /// <summary>
        /// 開始局面からの指し手
        /// </summary>
        public List<Move> moves;
    }
}
