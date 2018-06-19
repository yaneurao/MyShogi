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
        /// <summary>
        /// デフォルトの先手対局者名。
        /// </summary>
        static public string defaultPlayerNameBlack = "先手";
        /// <summary>
        /// デフォルトの後手対局者名。
        /// </summary>
        static public string defaultPlayerNameWhite = "後手";

        /// <summary>
        /// 初期状態に戻す
        /// </summary>
        public void Init()
        {
            header_dic = new Dictionary<string, string>()
            {
                { "先手", defaultPlayerNameBlack },
                { "後手", defaultPlayerNameWhite }
            };
        }

        /// <summary>
        /// 対局ヘッダ情報。
        /// キー文字列は柿木形式で使われているヘッダのキー文字列に準ずる。
        /// </summary>
        public Dictionary<string, string> header_dic = new Dictionary<string, string>()
        {
            { "先手", defaultPlayerNameBlack },
            { "後手", defaultPlayerNameWhite }
        };

        /// <summary>
        /// 先手/下手 対局者名。
        ///   playerNameBlack : 先手の名前(駒落ちの場合、下手)
        /// </summary>
        public string PlayerNameBlack
        {
            get
            {
                string name;
                return header_dic.TryGetValue("先手", out name) ? name : defaultPlayerNameBlack;
            }
            set
            {
                header_dic["先手"] = value;
            }
        }

        /// <summary>
        /// 後手/上手 対局者名。
        ///   playerNameWhite : 後手の名前(駒落ちの場合、上手)
        /// </summary>
        public string PlayerNameWhite
        {
            get
            {
                string name;
                return header_dic.TryGetValue("後手", out name) ? name : defaultPlayerNameWhite;
            }
            set
            {
                header_dic["後手"] = value;
            }
        }

        /// <summary>
        /// PlayerNameのgetter
        /// </summary>
        public string GetPlayerName(Color c)
        {
            return c == Color.BLACK ? PlayerNameBlack : PlayerNameWhite;
        }
        
        /// <summary>
        /// PlayerNameのsetter
        /// </summary>
        /// <param name="c"></param>
        /// <param name="name"></param>
        public void SetPlayerName(Color c, string name)
        {
            if (c == Color.BLACK)
                PlayerNameBlack = name;
            else
                PlayerNameWhite = name;
        }
    }
}
