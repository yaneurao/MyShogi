using System;
using System.Text;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// 探索時間などを制限するときの種別
    /// </summary>
    public enum UsiThinkLimitEnum
    {
        /// <summary>
        /// 無制限
        /// </summary>
        Infinite,

        /// <summary>
        /// 思考時間(秒)
        /// btime , byoyomi , inctime
        /// </summary>
        Time,

        /// <summary>
        /// ノード数
        /// </summary>
        Node,

        /// <summary>
        /// 探索深さ
        /// </summary>
        Depth,
    }

    /// <summary>
    /// 将棋エンジンの思考制限を行うためのデータです。
    /// </summary>
    public sealed class UsiThinkLimit 
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiThinkLimit()
        {
            LimitType = UsiThinkLimitEnum.Infinite;
        }

        /// <summary>
        /// この条件を元に、USIプロトコルで用いる"goコマンド"の"go","go ponder"以降の文字列を構築する。
        /// </summary>
        /// <returns></returns>
        public string ToUsiString()
        {
            switch(LimitType)
            {
                case UsiThinkLimitEnum.Infinite:
                    return "infinite";

                case UsiThinkLimitEnum.Node:
                    return $"node {Nodes}";

                case UsiThinkLimitEnum.Depth:
                    return $"depth {Depth}";

                case UsiThinkLimitEnum.Time:

                    // RestTimeBlack,RestTimeWhiteがnullでありうる。
                    var sb = new StringBuilder();
                    sb.Append("btime ");
                    sb.Append(RestTimeBlack == null ? "0" : RestTimeBlack.TotalMilliseconds.ToString());

                    sb.Append(" wtime ");
                    sb.Append(RestTimeWhite == null ? "0" : RestTimeWhite.TotalMilliseconds.ToString());

                    // inctimeとbyoyomiは併用できない。
                    // どちらもない場合は"byoyomi 0"を指定しなければならない。

                    if (ByoyomiTime == null && IncTimeBlack == null)
                        sb.Append(" byoyomi 0");
                    else
                    {
                        if (ByoyomiTime != null)
                        {
                            sb.Append($" byoyomi {ByoyomiTime.TotalMilliseconds.ToString()}");
                        } else
                        {
                            sb.Append(" binc ");
                            sb.Append(IncTimeBlack == null ? "0" : IncTimeBlack.TotalMilliseconds.ToString());

                            sb.Append("winc ");
                            sb.Append(IncTimeWhite == null ? "0" : IncTimeWhite.TotalMilliseconds.ToString());
                        }
                    }

                    return sb.ToString();
            }

            return null;
        }

        /// <summary>
        /// 制限の種類を取得または設定します。
        /// </summary>
        public UsiThinkLimitEnum LimitType { get;set; }

        /// <summary>
        /// 探索を無制限で行うかどうかを取得します。
        /// </summary>
        public bool IsInfinite
        {
            get { return (LimitType == UsiThinkLimitEnum.Infinite); }
        }

        /// <summary>
        /// 持ち時間の残り(先手)
        /// </summary>
        public TimeSpan RestTimeBlack
        {
            get; set;
        }

        /// <summary>
        /// 持ち時間の残り(後手)
        /// </summary>
        public TimeSpan RestTimeWhite
        {
            get; set;
        }

        /// <summary>
        /// byoyomi (1手ごとの秒数)
        /// </summary>
        public TimeSpan ByoyomiTime
        {
            get; set;
        }

        /// <summary>
        /// 1手ごとの加算時間(先手)
        /// </summary>
        public TimeSpan IncTimeBlack
        {
            get; set;
        }

        /// <summary>
        /// 1手ごとの加算時間(後手)
        /// </summary>
        public TimeSpan IncTimeWhite
        {
            get; set;
        }

        /// <summary>
        /// 探索を時間で制限するかどうかを取得します。
        /// </summary>
        public bool IsLimitTime
        {
            get { return (LimitType == UsiThinkLimitEnum.Time); }
        }

        /// <summary>
        /// 制限するノード数を取得または設定します。
        /// </summary>
        public Int64 Nodes
        {
            get; set;
        }

        /// <summary>
        /// 探索をノード数で制限するかどうかを取得します。
        /// </summary>
        public bool IsLimitNode
        {
            get { return (LimitType == UsiThinkLimitEnum.Node); }
        }

        /// <summary>
        /// 制限する探索深さを取得または設定します。
        /// </summary>
        public int Depth
        {
            get; set;
        }

        /// <summary>
        /// 探索を探索深さで制限するかどうかを取得します。
        /// </summary>
        public bool IsLimitDepth
        {
            get { return (LimitType == UsiThinkLimitEnum.Depth); }
        }
    }
}
