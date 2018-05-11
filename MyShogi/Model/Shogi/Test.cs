using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi
{
    public static class Test
    {
        public static void Test1()
        {
            Piece p = Piece.GOLD;
            Console.WriteLine(p.Pretty());
            Console.WriteLine(p.ToUsi());
            Piece p2 = Util.MakePiecePromote(Model.Shogi.Color.WHITE, p);
            Console.WriteLine(p2.ToUsi());

#if false
            // Squareのテスト
            Square sq = Square.SQ_56;
            //Console.WriteLine(sq.ToFile().ToUSI() + sq.ToRank().ToUSI());
            Console.WriteLine(sq.ToUsi());
            Console.WriteLine(sq.Pretty());
#endif

            Move m = Util.MakeMove(Square.SQ_56, Square.SQ_45);
            Console.WriteLine(m.ToUsi());

            Move m2 = Util.MakeMoveDrop(Piece.SILVER, Square.SQ_45);
            Console.WriteLine(m2.ToUsi());

            Move m3 = Util.MakeMovePromote(Square.SQ_84, Square.SQ_83);
            Console.WriteLine(m3.ToUsi());

            Move m4 = Util.FromUsiMove("8h2b+");
            Console.WriteLine(m4.Pretty());

            Move m5 = Util.FromUsiMove("G*3b");
            Console.WriteLine(m5.Pretty());

            Move m6 = Util.FromUsiMove("7g7f");

            Hand h = Hand.ZERO;
            h.Add(Piece.PAWN, 5);
            h.Add(Piece.KNIGHT, 1);
            Console.WriteLine(h.Pretty());
            Console.WriteLine(h.ToUsi(Model.Shogi.Color.BLACK));
            Console.WriteLine(h.ToUsi(Model.Shogi.Color.WHITE));

            var pos = new Position();
            //pos.UsiPositionCmd("startpos moves 7g7f 3c3d 8h3c+");

            pos.UsiPositionCmd("sfen lnsgkgsnl/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1 moves 5a6b 7g7f 3a3b");

            Console.WriteLine(pos.Pretty());

#if false
            // Bitboard(Square)のテスト
            for (Square sq = Square.ZERO; sq < Square.NB ; ++sq)
            {
                Console.WriteLine("sq = " + sq.Pretty());
                Bitboard b = new Bitboard(sq);
                Console.WriteLine(b.Pretty());
            }
#endif

#if false
            // 駒落ちの局面のテスト
            pos.InitBoard(BoardType.Handicap2); // 2枚落ち
            Console.WriteLine(pos.Pretty());

            pos.InitBoard(BoardType.Handicap10); // 10枚落ち
            Console.WriteLine(pos.Pretty());
#endif

#if false
            pos.SetSfen(Position.SFEN_HIRATE);
            Console.WriteLine(pos.ToSfen());
            Console.WriteLine(pos.Pretty());
            pos.DoMove(m6);
            Console.WriteLine(pos.Pretty());
#endif

#if false
            // sfen化して、setしてhash keyが変わらないかのテスト
            //pos.SetSfen(pos.ToSfen());
            //Console.WriteLine(pos.Pretty());
#endif

            // 指し手生成祭りの局面
            pos.SetSfen("l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w RGgsn5p 1");
            Console.WriteLine(pos.ToSfen());
            Console.WriteLine(pos.Pretty());

#if false
            // 乱数テスト
            var rand = new PRNG(1234);
            Console.WriteLine(rand.Rand());
            Console.WriteLine(rand.Rand());
            Console.WriteLine(rand.Rand());

            var key_side = Zobrist.Side;
            Console.WriteLine(key_side.ToString());
#endif

#if false
            // serialization test

            var csa = new Model.CsaConnectData();
            var serializer = new DataContractJsonSerializer(typeof(Model.CsaConnectData));
            var ms = new MemoryStream();
            serializer.WriteObject(ms,csa);
            var json = Encoding.UTF8.GetString(ms.ToArray());
            MessageBox.Show(json);
#endif
        }
    }
}
