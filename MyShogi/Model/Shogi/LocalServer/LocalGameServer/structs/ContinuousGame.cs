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
            PlayLimit = playLimit;
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
    }
}
