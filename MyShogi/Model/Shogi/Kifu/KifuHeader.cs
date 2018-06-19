using MyShogi.Model.Shogi.Core;
using System.Collections.Generic;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// KifuManagerで、棋譜を読み込む時のheaderに書いてあるものを
    /// 保存したりしておく構造体的なもの。
    /// </summary>
    public class KifuHeader
    {
        #region public properties
        /// <summary>
        /// rootでの局面種別。NoHandicap,Others以外が設定されていれば、
        /// PlayerNameBlackなどで得られる対局者名を「上手」「下手」とする。
        /// </summary>
        public BoardType rootBoardType { get; set; }

        /// <summary>
        /// 対局ヘッダ情報。
        /// キー文字列は柿木形式で使われているヘッダのキー文字列に準ずる。
        /// </summary>
        public Dictionary<string, string> header_dic;

        /// <summary>
        /// 先手/下手 対局者名。
        ///   playerNameBlack : 先手の名前(駒落ちの場合、下手)
        ///   
        /// このgetterを呼び出す時は、rootBoardTypeが適切に設定されていなければならない。
        /// </summary>
        public string PlayerNameBlack
        {
            get { return playerName_getter(Color.BLACK); }
            set { playerName[(int)Color.BLACK] = value; }
        }

        /// <summary>
        /// 後手/上手 対局者名。
        ///   playerNameWhite : 後手の名前(駒落ちの場合、上手)
        /// </summary>
        public string PlayerNameWhite
        {
            get { return playerName_getter(Color.WHITE); }
            set { playerName[(int)Color.WHITE] = value; }
        }

        /// <summary>
        /// PlayerNameのgetter
        /// </summary>
        public string GetPlayerName(Color c)
        {
            return playerName_getter(c);
        }

        /// <summary>
        /// PlayerNameのsetter
        /// </summary>
        /// <param name="c"></param>
        /// <param name="name"></param>
        public void SetPlayerName(Color c, string name)
        {
            playerName[c.ToInt()] = name;
        }
        #endregion
        #region public members

        public KifuHeader()
        {
            Init();
        }

        /// <summary>
        /// 初期状態に戻す
        /// </summary>
        public void Init()
        {
            header_dic = new Dictionary<string, string>()
            {
                { "先手", defaultPlayerNames[(int)Color.BLACK] },
                { "後手", defaultPlayerNames[(int)Color.WHITE] }
            };
            rootBoardType = BoardType.NoHandicap;
        }

        #endregion

        #region privates

        /// <summary>
        /// プレイヤー名
        /// </summary>
        private string[] playerName = new string[2];

        /// <summary>
        /// デフォルトの対局者名。
        /// </summary>
        private static string [] defaultPlayerNames = { "先手", "後手" };

        /// <summary>
        /// デフォルトの駒落ち対局者名。
        /// </summary>
        private static string [] defaultHandicapPlayerNames = { "下手", "上手" };

        /// <summary>
        /// c側の対局者名の取得
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private string playerName_getter(Color c_)
        {
            int c = c_.ToInt();
            if (!string.IsNullOrEmpty(playerName[c]))
                return playerName[c];

            string name;
            switch (rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    return
                        header_dic.TryGetValue(defaultPlayerNames[c]        , out name) ? name :
                        header_dic.TryGetValue(defaultHandicapPlayerNames[c], out name) ? name : // fail safe
                        defaultPlayerNames[c];
                default:
                    return
                        header_dic.TryGetValue(defaultHandicapPlayerNames[c], out name) ? name :
                        header_dic.TryGetValue(defaultPlayerNames[c]        , out name) ? name : // fail safe
                        defaultHandicapPlayerNames[c];
            }
        }
        #endregion

    }
}
