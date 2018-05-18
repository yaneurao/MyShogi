using MyShogi.Model.Shogi.Core;
using System.Text;

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
        private static string[] csa_pos_rank = {
            "P1","P2","P3","P4","P5","P6","P7","P8","P9",
        };
        private static string[] csa_pos_piece = {
            " * ","+FU","+KY","+KE","+GI","+KA","+HI","+KI",
            "+OU","+TO","+NY","+NK","+NG","+UM","+RY","+QU",
            " * ","-FU","-KY","-KE","-GI","-KA","-HI","-KI",
            "-OU","-TO","-NY","-NK","-NG","-UM","-RY","-QU",
        };
        private static string[] csa_piece = {
            "  ","FU","KY","KE","GI","KA","HI","KI",
            "OU","TO","NY","NK","NG","UM","RY","QU",
            "  ","FU","KY","KE","GI","KA","HI","KI",
            "OU","TO","NY","NK","NG","UM","RY","QU",
        };
        private static readonly string[] HW_NUMBER = {
            "1","2","3","4","5","6","7","8","9",
        };
        private static readonly Piece[] HAND_ORDER = {
            Piece.ROOK,
            Piece.BISHOP,
            Piece.GOLD,
            Piece.SILVER,
            Piece.KNIGHT,
            Piece.LANCE,
            Piece.PAWN
        };

        /// <summary>
        /// 現在の局面図をCSA形式で出力する
        /// Position.ToSfen()のCSA版
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string ToCsa(this Position pos)
        {
            StringBuilder csa = new StringBuilder();
            // 盤面
            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
            {
                csa.Append(csa_pos_rank[r.ToInt()]);
                for (File f = File.FILE_9; f >= File.FILE_1; --f)
                {
                    csa.Append(csa_pos_piece[pos.PieceOn(Core.Util.MakeSquare(f, r)).ToInt()]);
                }
                csa.AppendLine();
            }
            // 持駒
            foreach (Color c in new Color[] { Color.BLACK, Color.WHITE })
            {
                if (pos.Hand(c) == Hand.ZERO)
                    continue;
                csa.Append((c == Color.BLACK) ? "P+" : "P-");
                foreach (Piece pc in HAND_ORDER)
                {
                    int cnt = pos.Hand(c).Count(pc);
                    for (int i = 0; i < cnt; ++i)
                        csa.Append(" ").Append(csa_piece[pc.ToInt()]);
                }
                csa.AppendLine();
            }
            // 手番
            csa.AppendLine(pos.SideToMove == Color.BLACK ? "+" : "-");
            return csa.ToString();
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
            switch (move)
            {
                case Move.NONE:
                case Move.NULL:
                    return "%ERROR";
                case Move.RESIGN:
                    return "%TORYO";
                case Move.WIN:
                    return "%WIN";
            }

            StringBuilder csa = new StringBuilder();

            // 手番
            csa.Append(pos.SideToMove != Color.WHITE ? "+" : "-");

            // 打つ指し手のときは移動元の升は"00"と表現する。
            // さもなくば、"77"のように移動元の升目をアラビア数字で表現。
            if (move.IsDrop())
                csa.Append("00");
            else
                csa.AppendFormat("{0}{1}", HW_NUMBER[move.From().ToFile().ToInt()], HW_NUMBER[move.From().ToRank().ToInt()]);

            csa.AppendFormat("{0}{1}", HW_NUMBER[move.To().ToFile().ToInt()], HW_NUMBER[move.To().ToRank().ToInt()]);

            Piece p = move.IsDrop() ? move.DroppedPiece() : pos.PieceOn(move.From());
            if (move.IsPromote()) p |= Piece.PROMOTE;
            csa.Append(csa_piece[p.ToInt()]);

            return csa.ToString();
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
            // ToDo : あとで実装する
            return "";
        }
    }
}
