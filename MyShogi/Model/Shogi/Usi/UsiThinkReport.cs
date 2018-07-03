using System;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// エンジンの1回のPVを表現する。
    /// 
    /// USIプロトコルの"info …"で表現されるもの。
    /// 
    /// EngineConsiderationControlで用いる、エンジンから送られてきた固定情報(npsなど)
    /// 片方のエンジン分。
    /// 
    /// すべて文字列として保持している。
    /// 例えばNpsなら使う時は、NpsStringのほうを用いて、セットする時はNpsのsetterを用いる。
    /// </summary>
    public class UsiThinkReport
    {
        /// <summary>
        /// 予想手
        /// </summary>
        public string PonderMove;

        /// <summary>
        /// 現在の探索手
        /// 
        /// これ本GUIでは、画面に表示しないので扱いテキトウ。
        /// </summary>
        public string CurrentMove;

        /// <summary>
        /// 探索深さ
        // ここに文字が入っている可能性があるので(upperbound/lowerboundを表現するための"↑"など)文字列として扱う。
        /// </summary>
        public string Depth;

        /// <summary>
        /// 探索の選択深さ(一番深くまで読んだ深さ)
        /// </summary>
        public string SelDepth;

        /// <summary>
        /// 思考時間(今回の思考開始からの経過時間)
        /// </summary>
        public TimeSpan ElapsedTime;

        /// <summary>
        /// info multipv x .. で渡されたMultiPVの値
        /// </summary>
        public int MultiPV;

        /// <summary>
        /// 評価値
        /// </summary>
        public EvalValueEx Eval;

        /// <summary>
        /// 読み筋
        /// </summary>
        public List<Move> Moves;

        /// <summary>
        /// "info string ..."で渡された文字列
        /// </summary>
        public string InfoString;

        /// <summary>
        /// 探索ノード数
        /// </summary>
        public string NodesString;
        public Int64 Nodes { set { NodesString = string.Format("{0:#,0}", value); } }

        /// <summary>
        /// 1秒間の探索ノード数
        /// </summary>
        public string NpsString;
        public Int64 Nps { set { NpsString = string.Format("{0:#,0}", value); } }

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
