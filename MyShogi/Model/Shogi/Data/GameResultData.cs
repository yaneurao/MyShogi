using System;
using System.Collections.Generic;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Data
{
    /// <summary>
    /// 対局結果を入れておく構造体
    /// </summary>
    public class GameResultData
    {
        /// <summary>
        /// 対局者氏名、先後分
        /// </summary>
        public string[] PlayerNames = new string[2];

        /// <summary>
        /// 対局開始日時
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// 対局終了時刻
        /// </summary>
        public DateTime EndTime;

        /// <summary>
        /// 終局時の指し手
        /// (SpecialMoveである)
        /// </summary>
        public Move LastMove;

        /// <summary>
        /// 終局時の手番(Resultから勝敗を判定するのに用いる)
        /// </summary>
        public Color LastSide;

        /// <summary>
        /// 棋譜ファイル名
        ///
        /// ここにフォルダ名は含まれていないものとする。
        /// </summary>
        public string KifuFileName;

        /// <summary>
        /// コメント行用のコメント。
        ///
        /// コメント行では、
        /// PlayerNames[0]と[1]などがnull(無効)。
        /// </summary>
        public string Comment;

        /// <summary>
        /// この構造体のデータを文字列の並びにする。
        /// </summary>
        /// <returns></returns>
        public List<string> ToList()
        {
            var list = new List<string>();
            list.Add(PlayerNames[0]);
            list.Add(PlayerNames[1]);
            list.Add(StartTime.ToString());
            list.Add(EndTime.ToString());

            /* EnumのToString()あまり使いたくないけど、intにcastすると定義変えたときに困るので…。*/
            list.Add(LastMove.MoveToString());
            list.Add(LastSide.ToUsi());

            list.Add(KifuFileName);
            list.Add(Comment);

            return list;
        }

        /// <summary>
        /// CSV形式で読み込んだ1行から、この構造体を構築する。
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static GameResultData FromLine(List<string> list)
        {
            // データ壊れてない？おかしすぎ。この行をskipすべき。
            if (list.Count < 8)
                return null;

            var result = new GameResultData();
            result.PlayerNames[0] = list[0];
            result.PlayerNames[1] = list[1];
            result.StartTime = DateTime.Parse(list[2]);
            result.EndTime = DateTime.Parse(list[3]);
            result.LastMove = Util.MoveFromString(list[4]);
            result.LastSide = Util.FromUsiColor(list[5].FirstChar());
            result.KifuFileName = list[6];
            result.Comment = list[7];

            return result;
        }
    }

}
