using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Converter
{
    /// <summary>
    /// ToDo : なんやかや書くかも
    /// </summary>
    public class KifConverter
    {
    }

    /// <summary>
    /// kif形式の入出力
    /// </summary>
    public static class KifExtensions
    {
        /// <summary>
        /// 現在の局面図をKIF形式で出力する
        /// Position.ToSfen()のKIF版
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string ToKif(this Position pos)
        {
            // ToDo : 実装する
            return "";
        }

        /// <summary>
        /// ある指し手をKIF形式で出力する
        /// Move.ToSfen()のKIF版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static string ToKif(this Position pos , Move move)
        {
            // ToDo : 実装する
            return "";
        }

        /// <summary>
        /// KIF形式の指し手を与えて、Moveに変換する。指し手の合法性のチェックはここでは行わない。
        /// 変換に失敗したときはMove.NONEが返る。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="kifMove"></param>
        /// <returns></returns>
        public static Move FromKif(this Position pos, string kifMove)
        {
            // ToDo : あとで実装する
            return Move.NONE;
        }

        /// <summary>
        /// KIF形式の局面図をsfen形式にする
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="kif"></param>
        /// <returns></returns>
        public static string KifToSfen(string kif)
        {
            return "";
        }
    }
}
