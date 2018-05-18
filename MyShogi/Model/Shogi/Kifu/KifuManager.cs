using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 1局の対局棋譜を全般的に管理するクラス
    /// ・分岐棋譜をサポート
    /// ・着手時間をサポート
    /// ・対局相手の名前をサポート
    /// ・CSA/KIF/KI2/SFEN形式での入出力をサポート
    /// ・千日手の管理、検出をサポート
    /// </summary>
    public class KifuManager
    {
        /// <summary>
        /// 対局者名。
        ///   playerName[(int)Color.BLACK] : 先手の名前(駒落ちの場合、下手)
        ///   playerName[(int)Color.WHITE] : 後手の名前(駒落ちの場合、上手)
        /// </summary>
        public string[] playerName = new string[2];

        /// <summary>
        /// 棋譜ファイルを読み込む。
        /// フォーマットは自動判別。
        /// CSA/KIF/KI2/SFEN形式を読み込める。
        /// </summary>
        /// <param name="filename"></param>
        public void Load(string filename /* , KifFileType kf */)
        {
        }

        /// <summary>
        /// 棋譜ファイルを書き出す
        /// フォーマットは引数のkfで指定する。
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="kf"></param>
        public void Save(string filename, KifuFileType kf)
        {
        }

        /// <summary>
        /// 棋譜本体。分岐棋譜。
        /// </summary>
        public KifuTree Tree = new KifuTree(); 

    }
}
