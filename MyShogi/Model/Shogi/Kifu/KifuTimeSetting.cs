using System;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 対局時間設定
    /// 片側のプレイヤー分
    /// </summary>
    public class KifuTimeSetting : NotifyObject
    {
        // -- DataMembers

        /// <summary>
        /// 持ち時間の[時]
        /// </summary>
        [DataMember]
        public int Hour
        {
            get { return GetValue<int>("Hour"); }
            set { SetValue("Hour", value); }
        }

        /// <summary>
        /// 持ち時間の[分]
        /// </summary>
        [DataMember]
        public int Minute
        {
            get { return GetValue<int>("Minute"); }
            set { SetValue("Minute", value); }
        }

        /// <summary>
        /// 持ち時間の[秒]
        /// </summary>
        [DataMember]
        public int Second
        {
            get { return GetValue<int>("Second"); }
            set { SetValue("Second", value); }
        }

        /// <summary>
        /// 持ち時間を使い切ったときの
        /// 秒読みの[秒]
        /// </summary>
        [DataMember]
        public int Byoyomi
        {
            get { return GetValue<int>("Byoyomi"); }
            set { SetValue("Byoyomi", value); }
        }

        /// <summary>
        /// Byoyomiは有効か？
        /// これがfalseならByoyomiの値は無効。
        /// </summary>
        [DataMember]
        public bool ByoyomiEnable
        {
            get { return GetValue<bool>("ByoyomiEnable"); }
            set { SetValue("ByoyomiEnable", value); }
        }

        /// <summary>
        /// 1手ごとの加算(秒)
        /// </summary>
        [DataMember]
        public int IncTime
        {
            get { return GetValue<int>("IncTime"); }
            set { SetValue("IncTime", value); }
        }

        /// <summary>
        /// IncTimeは有効か？
        /// これがfalseならIncTimeの値は無効。
        /// </summary>
        [DataMember]
        public bool IncTimeEnable
        {
            get { return GetValue<bool>("IncTimeEnable"); }
            set { SetValue("IncTimeEnable", value); }
        }

        /// <summary>
        /// 時間切れを負けにしない
        /// </summary>
        [DataMember]
        public bool IgnoreTime
        {
            get { return GetValue<bool>("IgnoreTime"); }
            set { SetValue("IgnoreTime", value); }
        }

        /// <summary>
        /// 時間制限なし
        /// (残り時間のところが"無制限"になる。
        /// 消費時間が減っていくのが気になる人向け)
        /// </summary>
        [DataMember]
        public bool TimeLimitless
        {
            get { return GetValue<bool>("TimeLimitless"); }
            set { SetValue("TimeLimitless", value); }
        }


        // -- public methods

        public KifuTimeSetting()
        {
            Minute = 15;
            ByoyomiEnable = true;
        }

        /// <summary>
        /// このインスタンスのClone()
        /// </summary>
        /// <returns></returns>
        public KifuTimeSetting Clone()
        {
            return (KifuTimeSetting)this.MemberwiseClone();
        }

        /// <summary>
        /// インスタンスがおかしい値ではないかをチェックする。
        /// 正常値ならtrueが返る。とりま、このクラスがvalidである条件をコードで明示するために書いてある。
        /// 
        /// これがfalseを返す設定をした時にUI側で警告を出すべきかも。
        /// まあ、いきなりタイムアップになるだけだから、警告、なくてもいいだろうけども。
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            // 秒読みかIncTimeかのどちらかは有効でなければならない。
            bool b1 = ByoyomiEnable ^ IncTimeEnable;

            // 持ち時間がないのに、秒読み0秒やIncTime == 0は許容できない。(いきなりタイムアップになる)
            bool b2 = (Hour == 0 && Minute == 0 && Second == 0) &&
                ((ByoyomiEnable && Byoyomi == 0) || (IncTimeEnable && IncTime == 0));
            
            return b1 && !b2; 
        }

        /// <summary>
        /// この持ち時間設定を文字列化する。
        /// </summary>
        /// <returns></returns>
        public string ToShortString()
        {
            var sb = new StringBuilder();
            if (TimeLimitless)
                return null; /* 消費時間のところに"無制限"と表示するので、ここでは何も出力しない */

            if (Hour != 0 || Minute != 0 || Second != 0)
            {
                //sb.Append("持ち時間");
                if (Hour != 0)
                    sb.Append($"{Hour}時間");
                if (Minute != 0)
                    sb.Append($"{Minute}分");
                if (Second != 0)
                    sb.Append($"{Second}秒");
            }

            if ((ByoyomiEnable && Byoyomi == 0)
                || (IncTimeEnable && IncTime == 0))
            {
                sb.Append("切れ負け");
            }

            if (ByoyomiEnable && Byoyomi != 0)
            {
                if (sb.Length != 0)
                    sb.Append(" "); // 前の文字があるならスペースを放り込む
                sb.Append($"秒読み{Byoyomi}秒");
            }

            if (IncTimeEnable && IncTime != 0)
            {
                if (sb.Length != 0)
                    sb.Append(" "); // 前の文字があるならスペースを放り込む
                sb.Append($"1手{IncTime}秒加算");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 設定を文字列化。
        /// PSN2形式で保存する時などに用いる。
        /// </summary>
        /// <returns></returns>
        public string ToKifuString()
        {
            // -- メンバーのそれぞれの値をカンマつなぎで出力する。

            // boolだとparse面倒なので整数化しておく。
            var byoyomiEnable = ByoyomiEnable ? 1 : 0;
            var incTimeEnable = IncTimeEnable ? 1 : 0;
            var ignoreTime = IgnoreTime ? 1 : 0;
            var timeLimitless = TimeLimitless ? 1 : 0;

            return $"{Hour},{Minute},{Second},{byoyomiEnable},{Byoyomi},{incTimeEnable},{IncTime},{ignoreTime},{timeLimitless}";
        }

        /// <summary>
        /// ToString()の逆変換
        /// 変換に失敗した場合、nullが返る。
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static KifuTimeSetting FromKifuString(string line)
        {
            var regex = new Regex(@"(\d+),(\d+),(\d+),(\d+),(\d+),(\d+),(\d+),(\d+),(\d+)");
            var match = regex.Match(line);
            if (!match.Success)
                return null;

            var setting = new KifuTimeSetting();
            for (int i = 0;i<9;++i)
            {
                int r;
                if (!int.TryParse(match.Groups[i+1].Value, out r))
                    return null; // parse失敗
                bool b = r != 0 ? true : false;

                // それぞれに代入
                switch (i)
                {
                    case 0: setting.Hour = r; break;
                    case 1: setting.Minute = r; break;
                    case 2: setting.Second = r; break;
                    case 3: setting.ByoyomiEnable = b; break;
                    case 4: setting.Byoyomi = r; break;
                    case 5: setting.IncTimeEnable = b; break;
                    case 6: setting.IncTime = r; break;
                    case 7: setting.IgnoreTime = b; break;
                    case 8: setting.TimeLimitless = b; break;
                }
            }

            return setting;
        }

        public KifuTimeSettingMin ToKifuTimeSettingMin()
        {
            return new KifuTimeSettingMin()
            {
                Hour = Hour,
                Minute = Minute,
                Second = Second,
                Byoyomi = Byoyomi,
                ByoyomiEnable = ByoyomiEnable,
                IncTime = IncTime,
                IncTimeEnable = IncTimeEnable,
                IgnoreTime = IgnoreTime,
                TimeLimitless = TimeLimitless,
            };
        }

        public static KifuTimeSetting FromKifuTimeSettingMin(KifuTimeSettingMin min)
        {
            return new KifuTimeSetting()
            {
                Hour = min.Hour,
                Minute = min.Minute,
                Second = min.Second,
                Byoyomi = min.Byoyomi,
                ByoyomiEnable = min.ByoyomiEnable,
                IncTime = min.IncTime,
                IncTimeEnable = min.IncTimeEnable,
                IgnoreTime = min.IgnoreTime,
                TimeLimitless = min.TimeLimitless,
            };
        }
    }

    [DataContract]
    public class KifuTimeSettingMin
    {
        [DataMember] public int Hour;
        [DataMember] public int Minute;
        [DataMember] public int Second;
        [DataMember] public int Byoyomi;
        [DataMember] public bool ByoyomiEnable;
        [DataMember] public int IncTime;
        [DataMember] public bool IncTimeEnable;
        [DataMember] public bool IgnoreTime;
        [DataMember] public bool TimeLimitless;
    }

    /// <summary>
    /// 対局時間設定 先後の両方の分
    /// </summary>
    public class KifuTimeSettings : NotifyObject
    {
        /// <summary>
        /// 対局時間設定、先後分
        /// getのときは、このメンバに直接アクセスせずにPlayer()のほうを用いること。
        /// </summary>
        [DataMember]
        public KifuTimeSetting[] Players;

        /// <summary>
        /// 後手の対局時間設定を先手とは別に設定する。
        /// </summary>
        [DataMember]
        public bool WhiteEnable
        {
            get { return GetValue<bool>("WhiteEnable"); }
            set { SetValue("WhiteEnable", value); }
        }

        // -- public methods

        public KifuTimeSettings()
        {
            Players = new KifuTimeSetting[2] { new KifuTimeSetting(), new KifuTimeSetting() };
            WhiteEnable = false;
        }

        public KifuTimeSettings(KifuTimeSetting[] players, bool WhiteEnable_)
        {
            Players = players;
            WhiteEnable = WhiteEnable_;
        }

        public KifuTimeSettings Clone()
        {
            return new KifuTimeSettings(
                new KifuTimeSetting[2] { Players[0].Clone(), Players[1].Clone() },
                WhiteEnable
                );
        }

        /// <summary>
        /// c側の対局設定。
        /// ただし、WhiteEnable == falseである時は、後手側の内容を無視して、先手側の対局に従うのでPlayers[0]のほうが返るので注意！
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public KifuTimeSetting Player(Color c)
        {
            if (!WhiteEnable)
                c = Color.BLACK;

            return Players[(int)c];
        }

        /// <summary>
        /// c側のプレイヤーの対局設定
        /// Player()との違いに注意。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public KifuTimeSetting RawPlayer(Color c)
        {
            return Players[(int)c];
        }

        /// <summary>
        /// 先手と後手のプレイヤーを入れ替える
        /// </summary>
        public void SwapPlayer()
        {
            Utility.Swap(ref Players[0], ref Players[1]);
        }

        // -- properties

        /// <summary>
        /// 持ち時間制限なしのsingleton instance
        /// </summary>
        public static readonly KifuTimeSettings TimeLimitless = CreateTimeLimitless();

        /// <summary>
        /// この対局設定に従う、初期局面でのKifuMoveTimesを生成して返す。
        /// (残り時間が持ち時間に設定されている)
        /// </summary>
        /// <returns></returns>
        public KifuMoveTimes GetInitialKifuMoveTimes()
        {
            var k = new KifuMoveTime[2];
            foreach (var c in All.Colors())
            {
                var p = Player(c);
                var restTime = new TimeSpan(p.Hour, p.Minute, p.Second);
                k[(int)c] = new KifuMoveTime(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, restTime);
            }

            return new KifuMoveTimes(k[0],k[1]);
        }

        // -- private members

        /// <summary>
        /// 持ち時間なしのinstanceの生成
        /// </summary>
        /// <returns></returns>
        private static KifuTimeSettings CreateTimeLimitless()
        {
            var player = new KifuTimeSetting();
            player.TimeLimitless = true;
            return new KifuTimeSettings(new KifuTimeSetting[2] { player, player } , true);
        }

        public KifuTimeSettingsMin ToKifuTimeSettingsMin()
        {
            return new KifuTimeSettingsMin()
            {
                Players = new KifuTimeSettingMin[2] { Players[0].ToKifuTimeSettingMin(), Players[1].ToKifuTimeSettingMin() },
                WhiteEnable = WhiteEnable,
            };
        }

        public static KifuTimeSettings FromKifuTimeSettingsMin(KifuTimeSettingsMin min)
        {
            return new KifuTimeSettings()
            {
                Players = new KifuTimeSetting[2] { KifuTimeSetting.FromKifuTimeSettingMin(min.Players[0]), KifuTimeSetting.FromKifuTimeSettingMin(min.Players[1]) },
                WhiteEnable = min.WhiteEnable,
            };
        }

    }

    [DataContract]
    public class KifuTimeSettingsMin
    {
        [DataMember] public KifuTimeSettingMin[] Players;
        [DataMember] public bool WhiteEnable;
    }
}
