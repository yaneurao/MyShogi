using MyShogi.Model.Shogi.Core;
using System;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 連続対局のための情報に関する構造体
    /// </summary>
    public class ContinuousGame
    {
        /// <summary>
        /// 繰り返す回数を設定する。
        /// </summary>
        /// <param name="playCount"></param>
        /// <param name="playLimit"></param>
        public void SetPlayLimit(int playLimit)
        {
            PlayCount = 0;
            PlayLimit = PlayLimit2 = playLimit;
            WinCount = LoseCount = DrawCount = 0;
        }

        /// <summary>
        /// カウンターを初期化する。(0にする)
        /// </summary>
        public void ResetCounter()
        {
            PlayLimit = 0;
            PlayCount = 0;
        }

        /// <summary>
        /// 繰り返す回数を記録しているカウンターをインクリメントする。
        /// </summary>
        public void IncPlayCount()
        {
            PlayCount++;
        }

        /// <summary>
        /// 終局した時点で繰り返しが必要であるか。
        /// (IncCount()は先行して呼び出してあるものとする。)
        /// </summary>
        /// <returns></returns>
        public bool MustRestart()
        {
            return PlayCount < PlayLimit;
        }

        /// <summary>
        /// 連続対局が設定されているのか？
        /// </summary>
        /// <returns></returns>
        public bool IsContinuousGameSet()
        {
            return PlayLimit > 1;
        }

        /// <summary>
        /// これが最終局であるか。
        /// </summary>
        /// <returns></returns>
        public bool IsLastGame()
        {
            return PlayCount + 1 >= PlayLimit;
        }

        /// <summary>
        /// この回数だけ連続対局を行う。
        /// </summary>
        public int PlayLimit;

        /// <summary>
        /// // 連続対局の何回目であるかというカウンター
        /// </summary>
        public int PlayCount;

        /// <summary>
        /// 対局開始を行う局面(「現在の局面」から開始となっている場合)
        ///
        /// 棋譜を文字列化したもの。
        /// </summary>
        public string Kif;

        /// <summary>
        /// 通常対局のときにエンジンの選択しているPreset名。
        /// エンジンでなければ、nullが返る。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string PresetName(Color c) { return presetNames[(int)c]; }
        public string[] presetNames = new string[2];

        /// <summary>
        /// 今回の対局の開始時刻
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// 今回の対局の終了時刻
        /// </summary>
        public DateTime EndTime;

        /// <summary>
        /// 連続対局開始前のゲーム設定を保存しておく。(プレイヤーの交換などを行うため)
        /// </summary>
        public GameSetting GameSetting;

        /// <summary>
        /// 先手から見て、勝ち、負け、引き分けの回数
        /// </summary>

        public int WinCount;
        public int LoseCount;
        public int DrawCount;

        /// <summary>
        /// 対局結果から、勝敗カウンターを加算する。
        /// </summary>
        /// <param name="lastMove"></param>
        /// <param name="lastColor"></param>
        public void IncResult(Move lastMove,Color lastColor)
        {
            var result = lastMove.GameResult();

            // 先手から見た勝敗に変換する
            if (lastColor == Color.WHITE)
                result = result.Not();

            switch(result)
            {
                case MoveGameResult.WIN: ++WinCount; break;
                case MoveGameResult.DRAW:++DrawCount;break;
                case MoveGameResult.LOSE:++LoseCount;break;

                // それ以外は引き分け扱いにしておく。(トータルカウントを計算するときに狂ってしまうため)
                default: ++DrawCount; break;
            }
        }

        /// <summary>
        /// 連続対局中の文字列
        /// </summary>
        public string GetGamePlayingString()
        {
            if (PlayLimit2 > 1)
            {
                // 対局こなした回数
                // 1局目が "1/100"のように表示されて欲しいので TotalCountに+1 しておく。
                var TotalCount = Math.Min(WinCount + LoseCount + DrawCount + 1 , PlayLimit2);

                return $"【連続対局】({TotalCount}/{PlayLimit2}) : {GetRatingString()}";
            }
            return null;
        }

        /// <summary>
        /// 勝敗数とレーティング文字列
        /// </summary>
        /// <returns></returns>
        public string GetRatingString()
        {
            // 勝ちと負けの回数(引き分けを除外)
            var total = WinCount + LoseCount;

            if (total != 0)
            {
                var win_rate = WinCount / (float)total;
                var rating = -400 * Math.Log(1 / win_rate - 1, 10);

                return $"{WinCount}-{ LoseCount }-{ DrawCount} ({100*win_rate:f1}% R{rating:f1})";
            }
            return $"{WinCount}-{ LoseCount }-{ DrawCount}";
        }

        /// <summary>
        /// 終了するときにPlayLimitをリセットしてしまうので保存しておく。
        /// </summary>
        public int PlayLimit2;

    }
}
