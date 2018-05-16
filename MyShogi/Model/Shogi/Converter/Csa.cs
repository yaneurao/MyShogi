using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Converter
{
    /// <summary>
    /// ToDo : なんやかや書くかも
    /// </summary>
    public class CsaConverter
    {
    }

    /// <summary>
    /// kif形式の入出力
    /// </summary>
    public static class CsaExtensions
    {
        /// <summary>
        /// 現在の局面図をCSA形式で出力する
        /// Position.ToSfen()のCSA版
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string ToCsa(this Position pos)
        {
            // ToDo : 実装する
            return "";
        }

        /// <summary>
        /// ある指し手をCSA形式で出力する
        /// Move.ToSfen()のCSA版
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static string ToCSA(this Position pos, Move move)
        {
            // ToDo : 実装する
            return "";
        }

        /// <summary>
        /// CSA形式の指し手を与えて、Moveに変換する。指し手の合法性のチェックはここでは行わない。
        /// 変換に失敗したときはMove.NONEが返る。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="csaMove"></param>
        /// <returns></returns>
        public static Move FromCSA(this Position pos, string csaMove)
        {
            // ToDo : あとで実装する
            return Move.NONE;
        }

        /// <summary>
        /// CSA形式の局面図をsfen形式にする
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="kif"></param>
        /// <returns></returns>
        public static string CsaToSfen(string csa)
        {
            return "";
        }
    }
}
