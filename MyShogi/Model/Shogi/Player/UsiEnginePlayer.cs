using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// USIプロトコルでやりとりする思考エンジンを
    /// Player派生クラスとして実装してある
    /// </summary>
    public class UsiEnginePlayer : Player
    {
        public PlayerTypeEnum PlayerType
        {
            get { return PlayerTypeEnum.Human; }
        }

        /// <summary>
        /// 対局者名(これが画面上に表示する名前として使われる)
        /// </summary>
        public string DisplayName
        {
            get {
                return !string.IsNullOrEmpty(Name) ? Name : AliasName;
            }
        }

        /// <summary>
        /// エンジンからUSIプロトコルによって渡されたエンジン名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ユーザーがエンジンに対して別名をつけるときの名前
        /// これがnullか空の文字列であれば、RawNameがそのままNameになる。
        /// </summary>
        public string AliasName { get; set; }

    }
}
