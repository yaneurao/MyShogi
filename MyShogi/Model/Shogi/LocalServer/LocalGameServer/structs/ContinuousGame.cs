using MyShogi.App;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using System;
using System.IO;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 連続対局のための情報に関する構造体
    ///
    /// 表示用の対局者名などもここに入っている。
    /// </summary>
    public class ContinuousGame
    {
        #region publics

        /// <summary>
        /// 繰り返す回数を設定する。その他、変数一式初期化する。
        /// 連続対局の1局目が始まるときに1回だけ呼び出される。
        /// </summary>
        /// <param name="playCount"></param>
        /// <param name="playLimit"></param>
        public void SetPlayLimit(int playLimit)
        {
            PlayCount = 0;
            PlayLimit = PlayLimit2 = playLimit;
            WinCount = LoseCount = DrawCount = 0;
            Swapped = false;
            presetNames = new string[2];
            presetNames2 = new string[2];

            // これはのちほど必ずSetPlayerNameが呼び出されるはずなのでここではやらない。
            //displayNames = new string[2];

            // subfolderを作る設定になっているなら、それを取得する。
            // 途中で棋譜ファイルの保存をする/しないを切り替える可能性はあるので、
            // とりあえずここでフォルダ名は確定させておく必要がある。
            kifuSubfolder = TheApp.app.Config.GameResultSetting.CreateSubfolderOnContinuousGame ?
                DateTime.Now.ToString("yyyyMMddHHmmss") + Path.DirectorySeparatorChar :
                null;

            // このタイミングでそれ以外のメンバーもクリア
            LastMove = Move.NONE;
            Handicapped = false;
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
        /// これが連続対局の最終局であるか。
        /// </summary>
        /// <returns></returns>
        public bool IsLastGame()
        {
            return PlayLimit > 1 /* 連続対局 */
                && PlayCount + 1 >= PlayLimit; /* 最終局 */
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
        private string[] presetNames = new string[2];

        /// <summary>
        /// Preset名の設定(1局目の対局開始前に呼び出される)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="presetName"></param>
        public void SetPresetName(Color c , string presetName)
        {
            presetNames[(int)c] = presetNames2[(int)c] = presetName;
        }

        /// <summary>
        /// 対局者の表示用の名前
        ///
        /// GameStart()時にこのクラスのSetDiaplayName()で設定されるものとする。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string DisplayName(Color c) { return displayNames[(int)c]; }
        private string[] displayNames
            = new string[2] { "あなた", "わたし" }; // 起動時に画面に表示しておく名前はこれ。

        /// <summary>
        /// GameStart()時に対局者名を設定する。
        /// ここで設定したものはDisplayName()で取得できる。
        ///
        /// SwapPlayer()の影響を受ける。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="displayName"></param>
        public void SetDisplayName(Color c , string displayName)
        {
            displayNames[(int)c] = displayName;
        }


        /// <summary>
        /// 先後のプリセット名(PresetName)、表示名(DisplayName)の入れ替え。
        /// 連続対局や振り駒の処理において行う。
        /// </summary>
        public void SwapPlayer()
        {
            Utility.Swap(ref presetNames[0], ref presetNames[1]);
            Utility.Swap(ref displayNames[0], ref displayNames[1]);
        }

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
        /// (1局目の対局の)先手から見て、勝ち、負け、引き分けの回数
        /// </summary>

        public int WinCount;
        public int LoseCount;
        public int DrawCount;

        /// <summary>
        /// GameStart()のときの対局設定(GameSetting)から、先後入れ替えているか。
        /// (本局が元の手番から先後を入れ替えての対局中であるか。)
        ///
        /// 振り駒で先後が入れ替わるときもこれをtrueにする。
        /// </summary>
        public bool Swapped;

        /// <summary>
        /// 駒落ちでの対局なのか
        /// </summary>
        public bool Handicapped;

        /// <summary>
        /// 振り駒をするのかのフラグ
        ///
        /// SetGameLimit()のあと呼び出し側で設定する。
        /// </summary>
        public bool EnablePieceToss;

        /// <summary>
        /// 振り駒したときの駒の裏表を表現する。
        /// 
        /// PieceTossPieceColor[x] == trueならx枚目として歩、falseなら「と」を描画。
        /// </summary>
        public bool[] PieceTossPieceColor = new bool[5];

        /// <summary>
        /// 対局結果から、勝敗カウンターを加算する。
        /// また、このときのlastMove,lastColorを保存しておく。(直後に取り出して使うことが出来る)
        /// </summary>
        /// <param name="lastMove"></param>
        /// <param name="lastColor"></param>
        public void IncResult(Move lastMove,Color lastColor)
        {
            LastMove = lastMove;
            LastColor = lastColor;

            var result = lastMove.GameResult();

            // 先手から見た勝敗に変換する
            if (lastColor == Color.WHITE)
                result = result.Not();

            // この変数は先後入れ替えを考慮しない。
            LastGameResult = result;

            // 先後入れ替えての対局中であるなら、逆手番側の勝ちとしなければならない。
            if (Swapped)
                result = result.Not();

            switch (result)
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

                //return $"【連続対局】({TotalCount}/{PlayLimit2}) : {GetRatingString()}\r\n{PlayerName(Color.BLACK)} vs {PlayerName(Color.WHITE)}";
                return $"【連続対局】 {PlayerName(Color.BLACK)} vs {PlayerName(Color.WHITE)} ({TotalCount}/{PlayLimit2}) : {GetRatingString()}";
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

                return $"{WinCount}-{DrawCount}-{LoseCount} ({100*win_rate:f1}% R{rating:f1})";
            }
            return $"{WinCount}-{DrawCount}-{LoseCount}";
        }

        /// <summary>
        /// 対局結果の文字列を取得。(連続対局結果ウィンドウに出力)
        /// </summary>
        /// <returns></returns>
        public string GetGameResultString()
        {
            return $"{GetGamePlayingString()} : {PlayerName(Color.BLACK)} vs {PlayerName(Color.WHITE)}";
        }

        /// <summary>
        /// 棋譜ファイルを保存するsubfolder。
        /// subfolderに保存しない設定であれば、nullが返る。
        /// </summary>
        /// <returns></returns>
        public string GetKifuSubfolder()
        {
            return kifuSubfolder;
        }

        /// <summary>
        /// 直前の試合の結果文字列。(IncResult()したときの情報に基づく)
        /// </summary>
        /// <returns></returns>
        public string GetGameResultStringForLastGame()
        {
            // セットされていない？
            if (LastMove == Move.NONE)
                return null;

            switch (LastGameResult)
            {
                case MoveGameResult.WIN: return Handicapped ? "下手勝ち" : "先手勝ち";
                case MoveGameResult.LOSE: return Handicapped ? "上手勝ち" : "後手勝ち";
                case MoveGameResult.DRAW: return "引き分け";
                case MoveGameResult.UNKNOWN: return LastMove.SpecialMoveToKif();
                default: return null;
            }
        }

        #endregion

        #region privates

        /// <summary>
        /// 終了するときにPlayLimitをリセットしてしまうので保存しておく。
        /// </summary>
        private int PlayLimit2;

        /// <summary>
        /// 対局開始時のpreset名
        /// SwapPlayer()の影響を受けない。
        /// </summary>
        private string[] presetNames2 = new string[2];

        /// <summary>
        /// 1局目開始時のプレイヤー名。プリセットつき。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private string PlayerName(Color c)
        {
            var playerName = GameSetting.PlayerSetting(c).PlayerName;
            var preset = presetNames2[(int)c];
            return (preset == null) ? playerName : $"{playerName}({preset})";
        }

        /// <summary>
        /// 棋譜ファイルを保存するsubfolder。
        /// </summary>
        private string kifuSubfolder;

        /// <summary>
        /// IncResult()されたときに引数を保存しておいたもの。
        /// ここから直前のゲームの勝敗がわかる。
        ///
        /// LastGameResultは先手から見た勝敗。Swappedは考慮せず、本対局の先手側が勝ったかどうかで判定したもの。
        /// </summary>
        private Move LastMove;
        private Color LastColor;
        private MoveGameResult LastGameResult;
        
        #endregion
    }
}
