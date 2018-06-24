using System;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜の読み書きの時に用いる例外
    /// </summary>
    public class KifuException : Exception
    {
        /// <summary>
        /// message_  : エラー内容
        /// </summary>
        /// <param name="message_"></param>
        /// <param name="line_"></param>
        public KifuException(string message_) : base(message_){ }

        /// <summary>
        /// message_  : エラー内容
        /// line_     : 当該行の内容
        /// </summary>
        /// <param name="message_"></param>
        /// <param name="line_"></param>
        public KifuException(string message_, string line_) : base($"{message_} : {line_}") {}

        /// <summary>
        /// inner exeptionも指定できる版
        /// </summary>
        /// <param name="message_"></param>
        /// <param name="line_"></param>
        /// <param name="inner"></param>
        public KifuException(string message_, string line_, Exception inner) : base($"{message_} : {line_}", inner) {}
    }
}
