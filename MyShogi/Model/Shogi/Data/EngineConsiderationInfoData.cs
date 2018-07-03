using System;

namespace MyShogi.Model.Shogi.Data
{
    /// <summary>
    /// EngineConsiderationControlで用いる、エンジンから送られてきた固定情報(npsなど)
    /// 片方のエンジン分。
    /// 
    /// すべて文字列として保持している。
    /// 例えばNpsなら使う時は、NpsStringのほうを用いて、セットする時はNpsのsetterを用いる。
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
        /// 深さ文字列。
        /// 設定にはSetDepth()を用いると良い。
        /// </summary>
        public string DepthString;

        /// <summary>
        /// 探索深さと選択深さを引数に渡してDepth文字列を構築する。
        /// </summary>
        public void SetDepth(int depth , int selDepth)
        {
            DepthString = $"{depth}/{selDepth}";
        }

        /// <summary>
        /// 探索ノード数
        /// </summary>
        public string NodesString;
        public UInt64 Nodes { set { NodesString = string.Format("{0:#,0}", value); } }

        /// <summary>
        /// 1秒間の探索ノード数
        /// </summary>
        public string NpsString;
        public UInt64 Nps { set { NpsString = string.Format("{0:#,0}", value); } }

        /// <summary>
        /// ハッシュ使用率
        /// </summary>
        public string HashPercentageString;
        public float HashPercentage { set { HashPercentageString = string.Format("{0:F1}", value) + '%'; } }

        /// <summary>
        /// スレッド数
        /// </summary>
        public string ThreadNumString;
        public int ThreadNum { set { ThreadNumString = value.ToString(); } }
    }
}
