using System;
using System.Text;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.LocalServer;

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
        /// KifuTimeSettingから、このクラスのインスタンスを構築して返す。
        /// </summary>
        /// <param name="kifuTimeSettings"></param>
        public static UsiThinkLimit FromTimeSetting(PlayTimers timer, Color us)
        {
            var limit = new UsiThinkLimit();

            var ourPlayer = timer.Player(us).KifuTimeSetting;
            if (ourPlayer.IgnoreTime)
                limit.LimitType = UsiThinkLimitEnum.Infinite;
            else
            {
                limit.LimitType = UsiThinkLimitEnum.Time;

                // USIプロトコルでは先後の秒読みの違いを表現できない…。
                limit.ByoyomiTime = new TimeSpan(0,0,ourPlayer.Byoyomi);

                var blackPlayer = timer.Player(Color.BLACK).KifuTimeSetting;
                var whitePlayer = timer.Player(Color.WHITE).KifuTimeSetting;

                if (blackPlayer.IncTimeEnable)
                    limit.IncTimeBlack = new TimeSpan(0, 0, blackPlayer.IncTime);
                if (whitePlayer.IncTimeEnable)
                    limit.IncTimeBlack = new TimeSpan(0, 0, whitePlayer.IncTime);

                // 先後の残り時間を保存
                limit.RestTimeBlack = timer.GetKifuMoveTimes().Player(Color.BLACK).RestTime;
                limit.RestTimeWhite = timer.GetKifuMoveTimes().Player(Color.WHITE).RestTime;
            }

            return limit;
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

                    // ByoyomiTimeが0相当である。
                    var b1 = ByoyomiTime == null || ByoyomiTime == TimeSpan.Zero;
                    // IncTimeがどちらも0相当である。
                    var b2 = (IncTimeBlack == null || IncTimeBlack == TimeSpan.Zero) &&
                             (IncTimeWhite == null || IncTimeWhite == TimeSpan.Zero);

                    // inctimeとbyoyomiは併用できない。
                    // どちらもない場合は"byoyomi 0"を指定しなければならない。
                    if (b1 && b2)
                        sb.Append(" byoyomi 0");
                    else
                    {
                        if (b2)
                        {
                            // inc timeが指定されていないのでbyoyomiを出力しておくしかない。
                            sb.Append($" byoyomi {ByoyomiTime.TotalMilliseconds.ToString()}");
                        } else // if (!b2)
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
