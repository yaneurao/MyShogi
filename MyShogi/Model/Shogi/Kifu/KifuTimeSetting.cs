using System.Text;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 対局時間設定
    /// 片側のプレイヤー分
    /// </summary>
    public class KifuTimeSetting
    {
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
        /// 持ち時間の[時]
        /// </summary>
        public int Hour;

        /// <summary>
        /// 持ち時間の[分]
        /// </summary>
        public int Minute;

        /// <summary>
        /// 持ち時間の[秒]
        /// </summary>
        public int Second;

        /// <summary>
        /// 持ち時間を使い切ったときの
        /// 秒読みの[秒]
        /// </summary>
        public int Byoyomi;

        /// <summary>
        /// Byoyomiは有効か？
        /// これがfalseならByoyomiの値は無効。
        /// </summary>
        public bool ByoyomiEnable;

        /// <summary>
        /// 1手ごとの加算(秒)
        /// </summary>
        public int IncTime;

        /// <summary>
        /// IncTimeは有効か？
        /// これがfalseならIncTimeの値は無効。
        /// </summary>
        public bool IncTimeEnable;

        /// <summary>
        /// 時間切れを負けにしない
        /// </summary>
        public bool IgnoreTime;

        /// <summary>
        /// 時間制限なし
        /// (残り時間のところが"無制限"になる。
        /// 消費時間が減っていくのが気になる人向け)
        /// </summary>
        public bool TimeLimitless;

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
    }

    /// <summary>
    /// 対局時間設定 先後の両方の分
    /// </summary>
    public class KifuTimeSettings
    {
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

        /// <summary>
        /// 後手の対局時間設定を先手とは別に設定する。
        /// </summary>
        public bool WhiteEnable;

        // -- properties

        /// <summary>
        /// 持ち時間制限なしのsingleton instance
        /// </summary>
        public static readonly KifuTimeSettings TimeLimitless = CreateTimeLimitless();

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

        /// <summary>
        /// 対局時間設定、先後分
        /// </summary>
        private KifuTimeSetting[] Players;
    }
}
