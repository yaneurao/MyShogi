using System;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Data
{
    /// <summary>
    /// エンジンの1回のPVを表現する。
    /// </summary>
    public class EngineConsiderationPvData
    {
        /// <summary>
        /// 思考時間(今回の思考開始からの消費時間)
        /// </summary>
        public TimeSpan ThinkingTime;

        /// <summary>
        /// 探索ノード数
        /// </summary>
        public Int64 Nodes;

        /// <summary>
        /// 探索深さ
        /// </summary>
        public int Depth;

        /// <summary>
        /// 探索の選択深さ(一番深くまで読んだ深さ)
        /// </summary>
        public int SelDepth;

        /// <summary>
        /// 評価値
        /// </summary>
        public EvalValue Eval;

        /// <summary>
        /// 読み筋
        /// </summary>
        public List<Move> Moves;
    }
}
