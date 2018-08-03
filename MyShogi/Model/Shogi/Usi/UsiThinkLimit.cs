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
        /// 時間制限なしのUsiThinkLimitの定数オブジェクト
        /// </summary>
        public static readonly UsiThinkLimit TimeLimitLess = new UsiThinkLimit() { LimitType = UsiThinkLimitEnum.Infinite };

        /// <summary>
        /// KifuTimeSettingから、このクラスのインスタンスを構築して返す。
        /// </summary>
        /// <param name="kifuTimeSettings"></param>
        public static UsiThinkLimit FromTimeSetting(PlayTimers timer, Color us)
        {
            var limit = new UsiThinkLimit();

            limit.LimitType = UsiThinkLimitEnum.Time;

            var blackPlayer = timer.Player(Color.BLACK).KifuTimeSetting;
            var whitePlayer = timer.Player(Color.WHITE).KifuTimeSetting;

            // USIプロトコルでは先後の秒読みの違いを表現できないが、無理やり表現する
            //limit.ByoyomiTime = new TimeSpan(0, 0, ourPlayer.Byoyomi);

            // byoyomiとinctimeを同時に選択は出来ないので、IncTimeEnableを見て代入するほうを切り替える。
            if (blackPlayer.IncTimeEnable)
                limit.IncTimeBlack = new TimeSpan(0, 0, blackPlayer.IncTime);
            else
                limit.ByoyomiTimeBlack = new TimeSpan(0, 0, blackPlayer.Byoyomi);

            if (whitePlayer.IncTimeEnable)
                limit.IncTimeWhite = new TimeSpan(0, 0, whitePlayer.IncTime);
            else
                limit.ByoyomiTimeWhite = new TimeSpan(0, 0, whitePlayer.Byoyomi);

            // 先後の残り時間を保存
            limit.RestTimeBlack = timer.GetKifuMoveTimes().Player(Color.BLACK).RestTime;
            limit.RestTimeWhite = timer.GetKifuMoveTimes().Player(Color.WHITE).RestTime;

            var ourPlayer = timer.Player(us).KifuTimeSetting;
            if (ourPlayer.TimeLimitless)
            {
                // 検討モードでもないのに無制限に思考するわけにはいかないので、1手5秒とかに設定しておいてやる。

                //limit.LimitType = UsiThinkLimitEnum.Infinite;

                limit.LimitType = UsiThinkLimitEnum.Time;
                if (us == Color.BLACK)
                {
                    limit.ByoyomiTimeBlack = new TimeSpan(0, 0, 5);
                    limit.IncTimeBlack = TimeSpan.Zero;
                    limit.RestTimeBlack = TimeSpan.Zero;
                }
                else
                {
                    limit.ByoyomiTimeWhite = new TimeSpan(0, 0, 5);
                    limit.IncTimeWhite = TimeSpan.Zero;
                    limit.RestTimeWhite = TimeSpan.Zero;
                }
            }

            return limit;
        }

        /// <summary>
        /// 指定秒だけ思考するUsiTimeLimitを生成して返す。
        /// </summary>
        /// <param name="second"></param>
        /// <param name="us"></param>
        /// <returns></returns>
        public static UsiThinkLimit FromSecond(int second)
        {
            var limit = new UsiThinkLimit();

            limit.LimitType = UsiThinkLimitEnum.Time;
            limit.ByoyomiTimeBlack = limit.ByoyomiTimeWhite = new TimeSpan(0, 0, second);

            return limit;
        }

        /// <summary>
        /// この条件を元に、USIプロトコルで用いる"goコマンド"の"go","go ponder"以降の文字列を構築する。
        /// </summary>
        /// <returns></returns>
        public string ToUsiString(Color sideToMove)
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

                    // 手番側の秒読み
                    var byoyomiTime = sideToMove == Color.BLACK ? ByoyomiTimeBlack : ByoyomiTimeWhite;
                    var b1 = byoyomiTime == null || byoyomiTime == TimeSpan.Zero;

                    // IncTimeがどちらも0相当である。
                    var b2 = (IncTimeBlack == null || IncTimeBlack == TimeSpan.Zero) &&
                             (IncTimeWhite == null || IncTimeWhite == TimeSpan.Zero);

                    // inctimeとbyoyomiは併用できない。
                    // どちらもない場合は"byoyomi 0"を指定しなければならない。
                    if (b1 && b2)
                        sb.Append(" byoyomi 0");
                    else
                    {
                        // 持ち時間の拡張プロトコルが採用されていれば何も考えずに全部出力すれば良いのだが…。

                        if (!b1)
                        {
                            // この手番側の秒読みが指定されているので、これを出力しておくしかない。このとき binc/wincは出力できない。
                            sb.Append($" byoyomi { byoyomiTime.TotalMilliseconds.ToString()}");
                        } else // if (b1 && !b2)
                        {
                            // 秒読みが指定されていないのでinctimeを出力する。

                            sb.Append(" binc ");
                            sb.Append(IncTimeBlack == null ? "0" : IncTimeBlack.TotalMilliseconds.ToString());

                            sb.Append(" winc ");
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
        /// byoyomi (1手ごとの秒数) 先手
        ///
        /// USIプロトコル上は、byoyomiとinctimeの併用不可で、
        /// byoyomiは先後に同時に適用されるが、本GUIでは先後個別に設定できるので、
        /// 先手だけbyoyomiということがありうる。
        ///
        /// この場合、先手に対してはbyoyomiで渡して、後手に対してはwincで渡すようにする。
        /// </summary>
        public TimeSpan ByoyomiTimeBlack
        {
            get; set;
        }

        /// <summary>
        /// byoyomi (1手ごとの秒数) 後手
        /// </summary>
        public TimeSpan ByoyomiTimeWhite
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
