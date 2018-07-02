using System;

namespace MyShogi.Model.Shogi.Data
{
    /// <summary>
    /// EngineConsiderationControlで用いる、エンジンから送られてきた固定情報(npsなど)
    /// 片方のエンジン分。
    /// </summary>
    public class EngineConsiderationInfoData
    {
        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string PlayerName;

        /// <summary>
        /// 予想手
        /// </summary>
        public string PonderMove;

        /// <summary>
        /// 現在の探索手
        /// </summary>
        public string SearchingMove;

        /// <summary>
        /// 探索深さ
        /// </summary>
        public int Depth;

        /// <summary>
        /// 選択探索深さ(探索した時の一番深かったところの深さ)
        /// </summary>
        public int SelDepth;

        /// <summary>
        /// 探索ノード数
        /// </summary>
        public Int64 Nodes;

        /// <summary>
        /// 1秒間の探索ノード数
        /// </summary>
        public Int64 NPS;

        /// <summary>
        /// ハッシュ使用率
        /// </summary>
        public float HashPercentage;

        /// <summary>
        /// スレッド数
        /// </summary>
        public int ThreadNum;
    }
}
