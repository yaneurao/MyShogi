using System;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 指し手生成
    /// </summary>
    public static class MoveGen
    {
        /// <summary>
        /// 合法な指し手を生成する。
        /// moves[startIndex]から使っていく。返し値をendIndexとして、
        /// moves[startIndex]...moves[endIndex-1]まで使うものとする。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int LegalAll(Position pos , Move[] moves, int startIndex)
        {
            /// Position.IsLegal()が完璧に非合法手を弾くので、NonEvalsion()で指し手を生成して
            /// FilterNonLegalMoves()で排除する。少し遅くなるが、Evasionそんなにないからいいだろう…。

            // 愚直に81升調べてもどうってことないはずだが最低限の高速化をしとく
            var endIndex = startIndex;

            var us = pos.SideToMove;
            var ourPieces = pos.Pieces(us); // 自駒
            Square from , to;
            var enemyField = Bitboard.EnemyField(us); // 敵陣

            // 自分から見た1段目
            var rank1_for_us = us == Color.BLACK ? Rank.RANK_1 : Rank.RANK_9;
            var rank2_for_us = us == Color.BLACK ? Rank.RANK_2 : Rank.RANK_8;

            while (ourPieces.IsNotZero())
            {
                from = ourPieces.Pop();
                Piece pc = pos.PieceOn(from); // 移動元の駒
                Piece pt = pc.PieceType();

                // pcに駒を置いたときの利きに移動できて、自駒があるところには移動できない
                var target = Bitboard.EffectsFrom(pc, from, pos.Pieces()) & pos.Pieces(us);
                while (target.IsNotZero())
                {
                    to = target.Pop();

                    // pcをfromからtoに移動させる指し手を生成する

                    var r = to.ToRank();

                    // 行き場のない駒の移動は非合法手なのでそれを除外して指し手生成
                    if (!
                        (((pt == Piece.PAWN) || (pt == Piece.LANCE) && r == rank1_for_us)
                        ||(pt == Piece.KNIGHT && (r == rank1_for_us || r== rank2_for_us)))
                        )
    
                        moves[endIndex++] = Util.MakeMove(from, to);

                    // 成れる条件
                    //   1.移動させるのが成れる駒
                    //   2.移動先もしくは移動元が敵陣
                    if ((Piece.PAWN <= pt && pt < Piece.GOLD)
                        && (enemyField & (new Bitboard(from) | new Bitboard(to))).IsNotZero())

                        moves[endIndex++] = Util.MakeMovePromote(from, to);
                }
            }

            // 駒打ちの指し手

            var h = pos.Hand(us);
            for (Piece pt = Piece.PAWN; pt < Piece.KING; ++pt)
            {
                // その駒を持っていないならskip
                if (h.Count(pt) == 0)
                    continue;

                for (to = Square.ZERO; to < Square.NB; ++to)
                {
                    // 行き場のないところには打てない
                    var r = to.ToRank();
                    if (((pt == Piece.PAWN) || (pt == Piece.LANCE) && r == rank1_for_us)
                      || (pt == Piece.KNIGHT && (r == rank1_for_us || r == rank2_for_us)))
                        continue;

                    if (pos.PieceOn(to) != Piece.NO_PIECE)
                        continue;

                    // 二歩のチェックだけしとく
                    if (pt == Piece.PAWN
                        && (pos.Pieces(Piece.PAWN) & Bitboard.FileBB(to.ToFile())).IsNotZero())
                        continue;

                    moves[endIndex++] = Util.MakeMoveDrop(pt, to );
                }
            }

            // 非合法手を除外する。

            int p = startIndex;
            while (p < endIndex)
            {
                Move m = moves[p];
                if (pos.IsLegal(m))
                    ++p;
                else
                    moves[p] = moves[--endIndex]; // 非合法手でなかったので最後の指し手をここに埋める
            }
            return endIndex;
        }

        /// <summary>
        /// 現在の局面で合法手をすべて生成してそれを出力する(デバッグ用)
        /// </summary>
        /// <param name="pos"></param>
        public static void GenTest(Position pos)
        {
            Move[] moves = new Move[(int)Move.MAX_MOVES];
            int endIndex = MoveGen.LegalAll(pos, moves, 0);

            for (int i = 0; i < endIndex; ++i)
                Console.Write(moves[i].Pretty() + " ");

            Console.WriteLine();
        }
    }
}
