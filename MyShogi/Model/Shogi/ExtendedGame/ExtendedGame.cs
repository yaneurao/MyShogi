using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 拡張ルールによる盤面の提供
    /// </summary>
    public static class ExtendedGame
    {
        /// <summary>
        /// Shogi960の初期局面を一つ生成して返す。
        /// sfen形式の文字列。
        /// </summary>
        /// <returns></returns>
        public static string Shogi960()
        {
            var pos = new Position();
            string sfen;
            do {

                pos.InitBoard(BoardType.NoHandicap);
                var raw = pos.CreateRawPosition();

                // 8段目、角を6,7,8筋のなかにランダム移動、飛車を2,3,4筋のなかでランダム移動

                Utility.Swap(ref raw.board[(int)Square.SQ_28], ref raw.board[(int)Util.MakeSquare(File.FILE_2 + (int)prng.Rand(3), Rank.RANK_8)]);
                Utility.Swap(ref raw.board[(int)Square.SQ_88], ref raw.board[(int)Util.MakeSquare(File.FILE_6 + (int)prng.Rand(3), Rank.RANK_8)]);

                // 9段目、玉以外を左翼4駒、右翼4駒の任意交換

                foreach (var i in All.Int(4))
                {
                    // Fisher–Yates shuffle

                    Utility.Swap(
                        ref raw.board[(int)Util.MakeSquare(File.FILE_1 + (int)i, Rank.RANK_9)],
                        ref raw.board[(int)Util.MakeSquare(File.FILE_1 + (int)prng.Rand((ulong)(4 - i)), Rank.RANK_9)]);

                    Utility.Swap(
                        ref raw.board[(int)Util.MakeSquare(File.FILE_6 + (int)i, Rank.RANK_9)],
                        ref raw.board[(int)Util.MakeSquare(File.FILE_6 + (int)prng.Rand((ulong)(4 - i)), Rank.RANK_9)]);
                }

                // 先手陣を180度回転させて後手陣とする。

                MakeWhiteFromBlackField(raw);

                sfen = Position.SfenFromRawPosition(raw);

                // 平手のsfenと同じのは、NG。
            } while (sfen == BoardType.NoHandicap.ToSfen());

            return sfen;
        }

        /// <summary>
        /// 先手陣を180度回転させて後手陣とする。
        /// (6～9段目を180度回転させて、1～4段目を生成)
        /// </summary>
        /// <param name="raw"></param>
        private static void MakeWhiteFromBlackField(RawPosition raw)
        {
            foreach (var r in All.Int(4))
                foreach (var f in All.Int(9))
                {
                    var pc = raw.board[(int)Util.MakeSquare(File.FILE_1 + (int)f, Rank.RANK_6 + (int)r)];
                    var white_pc = pc == Piece.NO_PIECE ? pc : (pc + (int)Piece.WHITE);
                    raw.board[(int)Util.MakeSquare(File.FILE_9 - (int)f, Rank.RANK_4 - (int)r)] = white_pc;
                }
        }

        private static PRNG prng = new PRNG();

    }
}
