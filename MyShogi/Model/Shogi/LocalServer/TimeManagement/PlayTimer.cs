using System;
using System.Diagnostics;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;

namespace MyShogi.Model.Shogi.LocalServer
{

    /// <summary>
    /// プレイヤーの消費時間を計測する。
    /// 片側のプレイヤー分。
    /// 
    /// 両側分は、PlayTimersという複数形のものがあるのでそちらを用いる。
    /// </summary>
    public class PlayTimer
    {
        /// <summary>
        /// 一手の指し手に関する消費時間など。
        /// immutableである。
        /// 
        /// ChangeToThemTurn()を呼び出した時に新しい情報が反映される。
        /// </summary>
        public KifuMoveTime KifuMoveTime { get; private set; }

        /// <summary>
        /// 持ち時間設定
        /// この設定に従って、残り時間をカウントする。
        /// このクラスを使う時に、外部からsetする。
        /// </summary>
        public KifuTimeSetting KifuTimeSetting { set; get; }

        public PlayTimer()
        {
            KifuMoveTime = new KifuMoveTime();
            Stopwatch = new Stopwatch();
            Stopwatch.Stop();
            KifuTimeSetting = new KifuTimeSetting(); // デフォルトで何かセットしておかないと起動後の描画でnull参照になってしまう。
        }

        /// <summary>
        /// タイマーの初期化
        /// </summary>
        public void Init()
        {
            Stopwatch.Reset();
            startTime = endTime = 0;
        }

        /// <summary>
        /// KifuMoveTimeを内部の状態に反映させる。
        /// 中断局の再開処理や待ったの時に、KifuMoveTimeによってこのクラスの状態をリセットしないといけないので
        /// そのための処理。
        /// 
        /// このあと、ChangeToMyTurn()が呼び出されることを前提
        /// </summary>
        /// <param name="kifuMoveTime"></param>
        public void SetKifuMoveTime(KifuMoveTime kifuMoveTime)
        {
            KifuMoveTime = kifuMoveTime;
            Init();
        }

        /// <summary>
        /// ゲーム開始なので、TimeSettingの時間をRestTimeに反映させる。
        /// </summary>
        public void GameStart()
        {
            KifuMoveTime = new KifuMoveTime(
                TimeSpan.Zero, // まだ指してないので、指し手の時間 = 0
                TimeSpan.Zero, // まだ指してないので、指し手の時間 = 0
                TimeSpan.Zero, // 総消費時間 = 0
                new TimeSpan(KifuTimeSetting.Hour, KifuTimeSetting.Minute, KifuTimeSetting.Second) // 残り持ち時間
                );
            Init();
        }

        /// <summary>
        /// 自分の手番になった。
        /// </summary>
        public void ChangeToOurTurn()
        {
            var restTime = KifuMoveTime.RestTime;

            // byoyomiありかも知れないので残り時間がマイナスなのについては、いったんリセットする。
            if (restTime < TimeSpan.Zero)
                restTime = TimeSpan.Zero;

            // 秒加算
            if (KifuTimeSetting.IncTimeEnable)
                restTime += new TimeSpan(0, 0, KifuTimeSetting.IncTime);

            var k = KifuMoveTime;
            KifuMoveTime = new KifuMoveTime(k.ThinkingTime , k.RealThinkingTime , k.TotalTime , restTime);

            StartTimer();
            timeUp_ = false;
        }

        /// <summary>
        /// 自分の手番が終わり。
        /// タイマーを終了する。
        /// MoveTimeに今回の消費時間等を反映させる。
        /// IncTimeの時はタイマーが加算される。
        /// 
        /// 引数のtimeUpは、タイムアップになったかのフラグ
        /// TimeUpのときは、IncTimeによって残り時間が増えてしまうと、
        /// 残り時間があるにも関わらずタイムアップになったように見えるといけないので
        /// IncTimeしない。
        /// </summary>
        public void ChangeToThemTurn(bool timeUp)
        {
            StopTimer();

            // 今回の指し手の計測時間
            var thinkingTime = ThinkingTime();

            // ElapsedTime()が0を返すようにしておく。
            // (そうしないとDisplayShortString()で残り時間を表示するときにさらに消費時間を引かれてしまう)
            startTime = Stopwatch.ElapsedMilliseconds;

            var restTime = KifuMoveTime.RestTime;

            // 計測時間をRestTimeから減らす
            restTime -= thinkingTime;
            if (restTime < TimeSpan.Zero)
                restTime = TimeSpan.Zero;

#if false
            // IncTimeの処理
            // TimeUpのときは、IncTimeによって残り時間が増えてしまうと、
            // 残り時間があるにも関わらずタイムアップになったように見えるといけないので
            // IncTimeしない。
            if (!timeUp && KifuTimeSetting.IncTimeEnable)
                restTime += new TimeSpan(0,0,KifuTimeSetting.IncTime);

            // →　TIME UP以外の問題が発生したときに、この処理は正しくない。
            // ゆえに、自分の手番が来た瞬間にだけ加算するようにすべき。
#endif

            // 実消費時間
            var realThinkingTime = RealThinkingTime();

            // KifuMoveTimeに反映するので、そこから取り出すべし。
            KifuMoveTime = new KifuMoveTime(thinkingTime, realThinkingTime , KifuMoveTime.TotalTime + thinkingTime, restTime);

            timeUp_ = timeUp; // この情報を保存しておく。
        }
        private bool timeUp_ = false;

        /// <summary>
        /// 時間切れであるかの判定
        /// 
        /// inc time有効のときは、自分の手番が来て時間の加算が行われるまでの間、残り時間が0になっているので、
        /// (エンジン初期化時に)そのタイミングでチェックしてしまうことを考慮して、0はtime upではないという解釈をする。
        /// </summary>
        /// <returns></returns>
        public bool IsTimeUp()
        {
            // 時間切れを負けにしない
            if (KifuTimeSetting.IgnoreTime || KifuTimeSetting.TimeLimitless)
                return false;

            // 「(ElapsedTime() / 1000)」は、計測秒であり、10秒秒読みのときは、「10」を読まれてはならず、
            // 計測秒で10秒になった時点でtime upであるから、1秒多めに見積もらないといけない。
            var rest = KifuMoveTime.RestTime - new TimeSpan(0, 0, (int)( ElapsedTime() / 1000) + 1);

            // ここに秒読み時間も加算して負かどうかを調べる
            if (KifuTimeSetting.ByoyomiEnable)
                rest += new TimeSpan(0, 0, KifuTimeSetting.Byoyomi);

            // 0はセーフ(IncTimeでは次の手番が来た瞬間に加算されるので)
            return (rest < TimeSpan.Zero);
        }

        /// <summary>
        /// タイマーの開始。持ち時間を消費していく。
        /// </summary>
        public void StartTimer()
        {
            startTime = Stopwatch.ElapsedMilliseconds;
            Stopwatch.Start();
        }

        /// <summary>
        /// タイマーの終了。
        /// </summary>
        public void StopTimer()
        {
            if (Stopwatch.IsRunning)
                Stopwatch.Stop();
            endTime = Stopwatch.ElapsedMilliseconds;

            // ここで時間切れであるかどうかは、呼び出し元でIsTimeUp()で判定すべき。
        }

        /// <summary>
        /// 今回の指し手の消費時間
        /// StartTimer()～StopTimer()までの消費時間を計測秒に変換したもの
        /// 計測秒とは、1秒未満1秒。1秒以上は秒未満切り捨て。(例 : 1.999秒は、計測1秒)
        /// </summary>
        /// <returns></returns>
        public TimeSpan ThinkingTime()
        {
            return new TimeSpan(0,0,RoundTime(endTime - startTime));
        }

        /// <summary>
        /// ThinkingTime()の、丸めをせずにミリ秒単位まで返すバージョン
        /// </summary>
        /// <returns></returns>
        public TimeSpan RealThinkingTime()
        {
            return new TimeSpan(0, 0, 0, 0 , (int)(endTime - startTime));
        }

        /// <summary>
        /// StartTimer()を呼び出して、
        /// StopTimer()をまだ呼び出していない時の経過時刻
        /// </summary>
        /// <returns></returns>
        public long ElapsedTime()
        {
            return Stopwatch.ElapsedMilliseconds - startTime;
        }

        /// <summary>
        /// 1秒未満を繰り下げた経過時間[s]
        /// 残り時間を表示する時に使う。
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        public int RoundDownTime(long elapsedTime)
        {
            return (int)((elapsedTime + 0) / 1000);
        }

        /// <summary>
        /// 計測時間。
        /// 1秒未満は1秒、それ以上は端数秒切り捨てで経過時間を計測する。
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        public int RoundTime(long elapsedTime)
        {
            // 1.999秒は計測1秒
            if (elapsedTime <= 1999)
                return 1;

            // 繰り上げ
            //return (int)((elapsedTime + 999) / 1000);

            // 繰り下げ
            return (int)((elapsedTime) / 1000);
        }

        /// <summary>
        /// 残り時間を表現する時間を
        /// </summary>
        /// <returns></returns>
        public string DisplayShortString()
        {
            // 消費時間が減っていくのが目障りな人向けの設定
            if (KifuTimeSetting.TimeLimitless)
                return "無制限";

            var elapsed = RoundDownTime(ElapsedTime());
            var r = KifuMoveTime.RestTime - new TimeSpan(0, 0, elapsed);

            // 秒読みが有効でないなら、残りの時、分、秒だけを描画しておく。
            if (!KifuTimeSetting.ByoyomiEnable || KifuTimeSetting.Byoyomi == 0)
            {
                if (r < TimeSpan.Zero)
                    r = TimeSpan.Zero;
                return $"{r.Hours:D2}:{r.Minutes:D2}:{r.Seconds:D2}";
            }
            else
            {
                var rn = -r;
                if (r < TimeSpan.Zero)
                    r = TimeSpan.Zero;
                if (rn < TimeSpan.Zero)
                    rn = TimeSpan.Zero;

                // TimeUpになったときは、
                // 3/3のような表示になって欲しいが、RestTimeの計算などがすでに終わっているために、これを行うのは結構難しい。
                if (timeUp_)
                    rn = new TimeSpan(0, 0, KifuTimeSetting.Byoyomi);

                return $"{r.Hours:D2}:{r.Minutes:D2}:{r.Seconds:D2} {rn.TotalSeconds}/{KifuTimeSetting.Byoyomi}";
            }
        }
        
        // -- private members

        /// <summary>
        /// 前回StartTimer()を呼び出した時刻
        /// </summary>
        private long startTime;

        /// <summary>
        /// 前回StopTimer()を呼び出した時刻
        /// </summary>
        private long endTime;

        /// <summary>
        /// 時間計測用のタイマー
        /// </summary>
        private Stopwatch Stopwatch;
    }

    /// <summary>
    /// PlayTimerの二人分
    /// </summary>
    public class PlayTimers
    {
        public PlayTimers()
        {
            Players = new PlayTimer[2] { new PlayTimer(), new PlayTimer() };
        }

        /// <summary>
        /// c側のPlayer分
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public PlayTimer Player(Color c)
        {
            return Players[(int)c];
        }

        /// <summary>
        /// KifuMoveTime、二人分を返す。
        /// KifuMoveTime自体はimmutableと考えられるのでコピーする必要はない。
        /// </summary>
        /// <returns></returns>
        public KifuMoveTimes GetKifuMoveTimes()
        {
            return new KifuMoveTimes(Players[0].KifuMoveTime, Players[1].KifuMoveTime);
        }

        /// <summary>
        /// KifuMoveTimesを内部クラスにセットする。
        /// </summary>
        /// <param name="kifuMoveTimes"></param>
        public void SetKifuMoveTimes(KifuMoveTimes kifuMoveTimes)
        {
            foreach(var c in All.Colors())
                Player(c).SetKifuMoveTime(kifuMoveTimes.Player(c));
        }

        /// <summary>
        /// 持ち時間設定をそれぞれのプレイヤーに反映させる。
        /// </summary>
        /// <param name="kifuTimeSettins"></param>
        public void SetKifuTimeSettings(KifuTimeSettings kifuTimeSettings)
        {
            foreach (var c in All.Colors())
                Player(c).KifuTimeSetting = kifuTimeSettings.Player(c);
        }

        private PlayTimer[] Players;
    }

}
