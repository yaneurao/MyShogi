using System;
using System.IO;
using System.Text;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;

namespace MyShogi.Model.Test
{
    public static class DevTest1
    {
        public static void Test1()
        {
            Piece p = Piece.GOLD;
            Console.WriteLine(p.Pretty());
            Console.WriteLine(p.ToUsi());
            Piece p2 = Shogi.Core.Util.MakePiecePromote(Color.WHITE, p);
            Console.WriteLine(p2.ToUsi());

#if false
            // Squareのテスト
            Square sq = Square.SQ_56;
            //Console.WriteLine(sq.ToFile().ToUSI() + sq.ToRank().ToUSI());
            Console.WriteLine(sq.ToUsi());
            Console.WriteLine(sq.Pretty());
#endif

            Move m = Shogi.Core.Util.MakeMove(Square.SQ_56, Square.SQ_45);
            Console.WriteLine(m.ToUsi());

            Move m2 = Shogi.Core.Util.MakeMoveDrop(Piece.SILVER, Square.SQ_45);
            Console.WriteLine(m2.ToUsi());

            Move m3 = Shogi.Core.Util.MakeMovePromote(Square.SQ_84, Square.SQ_83);
            Console.WriteLine(m3.ToUsi());

            Move m4 = Shogi.Core.Util.FromUsiMove("8h2b+");
            Console.WriteLine(m4.Pretty());

            Move m5 = Shogi.Core.Util.FromUsiMove("G*3b");
            Console.WriteLine(m5.Pretty());

            Move m6 = Shogi.Core.Util.FromUsiMove("7g7f");

            Hand h = Hand.ZERO;
            h.Add(Piece.PAWN, 5);
            h.Add(Piece.KNIGHT, 1);
            Console.WriteLine(h.Pretty());
            Console.WriteLine(h.ToUsi(Color.BLACK));
            Console.WriteLine(h.ToUsi(Color.WHITE));

            var pos = new Position();
            //pos.UsiPositionCmd("startpos moves 7g7f 3c3d 8h3c+");

            pos.InitBoard(BoardType.NoHandicap);
            MoveGen.GenTest(pos);

            pos.UsiPositionCmd("sfen lnsgkgsnl/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1 moves 5a6b 7g7f 3a3b");

            Console.WriteLine(pos.Pretty());

#if false
            // UndoMove()のテスト
            pos.UndoMove();
            Console.WriteLine(pos.Pretty());

            pos.UndoMove();
            Console.WriteLine(pos.Pretty());

            pos.UndoMove();
            Console.WriteLine(pos.Pretty());
            Console.WriteLine(pos.PrettyPieceNo());

#endif

            // 駒番号のテスト
            //Console.WriteLine(pos.PrettyPieceNo());

            //  Console.WriteLine(pos.Pieces().Pretty());
            //  Console.WriteLine(pos.Pieces(Color.BLACK).Pretty());


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
            // 角・馬の利きのテスト
            Bitboard occupied = new Bitboard(Square.SQ_33);
            Console.WriteLine(occupied.Pretty());
            Bitboard bb = Bitboard.BishopEffect(Square.SQ_55, occupied);
            Console.WriteLine(bb.Pretty());

            Bitboard bb2 = Bitboard.BishopStepEffect(Square.SQ_56);
            Console.WriteLine(bb2.Pretty());

            Bitboard bb3 = Bitboard.BishopEffect(Square.SQ_56 , Bitboard.AllBB());
            Console.WriteLine(bb3.Pretty());

            Bitboard bb4 = Bitboard.HorseEffect(Square.SQ_55, occupied);
            Console.WriteLine(bb4.Pretty());

#endif

#if false
            // 飛車・龍の利きのテスト
            Bitboard occupied = new Bitboard(Square.SQ_53);
            Console.WriteLine(occupied.Pretty());
            Bitboard bb = Bitboard.RookEffect(Square.SQ_55, occupied);
            Console.WriteLine(bb.Pretty());

            Bitboard bb2 = Bitboard.RookStepEffect(Square.SQ_56);
            Console.WriteLine(bb2.Pretty());

            Bitboard bb3 = Bitboard.RookEffect(Square.SQ_56, Bitboard.AllBB());
            Console.WriteLine(bb3.Pretty());

            Bitboard bb4 = Bitboard.DragonEffect(Square.SQ_55, occupied);
            Console.WriteLine(bb4.Pretty());

#endif

#if false
            // 香りの利きのテスト
            Bitboard occupied = new Bitboard(Square.SQ_53);
            Bitboard bb = Bitboard.LanceEffect(Color.BLACK , Square.SQ_55, occupied);
            Console.WriteLine(bb.Pretty());

            Bitboard bb3 = Bitboard.LanceStepEffect(Color.BLACK , Square.SQ_56);
            Console.WriteLine(bb3.Pretty());
#endif

#if false
            // 歩、桂、銀、金、玉の利きのテスト
            Bitboard bb = Bitboard.PawnEffect(Color.BLACK , Square.SQ_55);
            Console.WriteLine(bb.Pretty());

            Bitboard bb2 = Bitboard.KnightEffect(Color.BLACK, Square.SQ_55);
            Console.WriteLine(bb2.Pretty());

            Bitboard bb3 = Bitboard.SilverEffect(Color.BLACK, Square.SQ_55);
            Console.WriteLine(bb3.Pretty());

            Bitboard bb4 = Bitboard.GoldEffect(Color.BLACK, Square.SQ_55);
            Console.WriteLine(bb4.Pretty());

            Bitboard bb5 = Bitboard.KingEffect( Square.SQ_55);
            Console.WriteLine(bb5.Pretty());

#endif

#if false
            // EffectsFrom()のテスト
            var bb = Bitboard.EffectsFrom(Piece.W_DRAGON, Square.SQ_54, pos.Pieces());
            Console.WriteLine(bb.Pretty());

            var bb2 = Bitboard.EffectsFrom(Piece.W_GOLD, Square.SQ_54, pos.Pieces());
            Console.WriteLine(bb2.Pretty());
#endif

#if false
            // BitboardのPop()のテスト
            for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
            {
                Console.Write("sq = " + sq.Pretty() + " ");
                Bitboard b = new Bitboard(sq);
                Square r = b.Pop();
                Console.WriteLine(r.Pretty());
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
            // 駒番号のデバッグ
            Console.WriteLine(pos.PrettyPieceNo());

            //            Console.WriteLine(pos.Pieces().Pretty());
            //           Console.WriteLine(pos.Pieces(Color.BLACK).Pretty());
#endif

#if false
            // BitweenBB()のテスト
            var bb = Bitboard.BetweenBB(Square.SQ_77, Square.SQ_33);
            Console.WriteLine(bb.Pretty());

            var bb2 = Bitboard.BetweenBB(Square.SQ_58, Square.SQ_52);
            Console.WriteLine(bb2.Pretty());
#endif

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

#if false
            // UsiPositionCmdのテスト。非合法手が混じっているパターン

            var pos2 = new Position();
            try
            {
                pos2.UsiPositionCmd("startpos moves 7g7f 3c4d 2g2f "); // 2手目が非合法手
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
#endif

#if false
            {
                // 千日手の引き分けのテスト
                var pos2 = new Position();
                pos.UsiPositionCmd("startpos moves");

                var moves = new[]{
                    Util.MakeMove(Square.SQ_59, Square.SQ_58),
                    Util.MakeMove(Square.SQ_51, Square.SQ_52),
                    Util.MakeMove(Square.SQ_58, Square.SQ_59),
                    Util.MakeMove(Square.SQ_52, Square.SQ_51),
                };

                int ply = 0;
                for (int j = 0; j < 5; ++j)
                {
                    for (int i=0;i<moves.Length;++i)
                    {
                        pos.DoMove(moves[i]);
                        var rep = pos.IsRepetition();

                        ply++;
                        Console.WriteLine(string.Format("ply = {0} , rep = {1} ", ply, rep.ToString()));

                        // 16手目を指した局面(17手目)が先手番で千日手引き分けの局面になるはず
                    }
                }
            }
#endif

#if false
            {
                // 連続王手の千日手の負けのテスト
                var pos2 = new Position();
                pos.UsiPositionCmd("startpos moves 7g7f 5c5d 8h3c 5a6b");

                var moves = new[]{
                    Util.MakeMove(Square.SQ_33, Square.SQ_44),
                    Util.MakeMove(Square.SQ_62, Square.SQ_51),
                    Util.MakeMove(Square.SQ_44, Square.SQ_33),
                    Util.MakeMove(Square.SQ_51, Square.SQ_62),
                };

                int ply = 4;
                for (int j = 0; j < 5; ++j)
                {
                    for (int i = 0; i < moves.Length; ++i)
                    {
                        pos.DoMove(moves[i]);
                        //Console.WriteLine(pos.Pretty());
                        //Console.WriteLine(pos.State().checkersBB.Pretty());

                        var rep = pos.IsRepetition();

                        ply++;
                        Console.WriteLine(string.Format("ply = {0} , rep = {1} ", ply, rep.ToString()));

                        // 19手目の局面(を指した直後の局面=20手目,後手番)で、ここで後手勝ちになるはず。
                    }
                }
            }
#endif

#if false
            //  UndoMove()でcapture,promoteが戻るかのテスト
            var pos2 = new Position();
            pos2.UsiPositionCmd("startpos moves 7g7f 3c3d 8h2b+");
            Console.WriteLine(pos2.Pretty());
            pos2.UndoMove();
            Console.WriteLine(pos2.Pretty());
            pos2.UndoMove();
            Console.WriteLine(pos2.Pretty());
            pos2.UndoMove();
            Console.WriteLine(pos2.Pretty());
            Console.WriteLine(pos2.PrettyPieceNo());

#endif

        }

        // 棋譜の読み込みテスト
        public static void Test2()
        {
            using (var sr = new StreamReader("records20151115.sfen"))
            {
                string line;
                // 行番号
                int lineNo = 1;

                var pos = new Position();

                while ((line = sr.ReadLine()) != null)
                {
                    //Console.WriteLine(line);

                    try
                    {
                        pos.UsiPositionCmd(line);
                    }catch (Exception e)
                    {
                        Console.WriteLine(line);
                        Console.WriteLine(string.Format("{0}行目で例外発生。{1}", lineNo , e.Message));
                    }

                    lineNo++;

                    // 進捗用のマーカー出力
                    if ((lineNo % 1000) == 0)
                    {
                        Console.WriteLine(".");
                    }
                }
            }
            Console.WriteLine("Finished");
        }

#if false
            // 指し手生成のテスト
            // Position.SetSfen()のなかでこのテストをする
            Move[] moves = new Move[(int)Move.MAX_MOVES];

            int endIndex = MoveGen.LegalAll(this, moves, 0);
            for (int j = 0; j < endIndex;  ++j)
                if (move == moves[j])
                    goto Success;
            Console.WriteLine("MoveGenで生成されていない指し手がある : " + move.Pretty());
            Console.WriteLine(Pretty());

            Success:;
#endif

        /// <summary>
        /// KifuManager関連のテスト用コード
        /// </summary>
        public static void Test3()
        {
#if false
            // KifuManagerのテスト

            // sfenの読み込み
            var manager = new KifuManager();

            var sfen = "startpos moves 7g7f 8c8d 7f7e 7c7d 7e7d";
            var error = manager.FromString(sfen);
            Console.WriteLine(manager.Tree.position.Pretty());
            Console.WriteLine("Error = " + error);

            // sfenの書き出し
            var sfen2 = manager.ToString(KifuFileType.SFEN);
            Console.WriteLine(sfen2);
#endif

#if true
            // KifuManagerのテスト

            // psnの読み込み
            var manager = new KifuManager();

            var psn = System.IO.File.ReadAllText("kif/2.psn" , Encoding.GetEncoding("Shift_JIS"));
            var error = manager.FromString(psn);

            Console.WriteLine(manager.Tree.position.Pretty());
            Console.WriteLine("Error = " + error);

            // psnの書き出し
            var psn2 = manager.ToString(KifuFileType.PSN);
            Console.WriteLine(psn2);

#endif

        }

    }
}
