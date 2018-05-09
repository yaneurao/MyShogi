using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using MyShogi.Model.Shogi;

namespace MyShogi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Piece p = Piece.GOLD;
            Console.WriteLine(p.Pretty());
            Console.WriteLine(p.ToUsi());
            Piece p2 = Util.MakePiecePromote(Model.Shogi.Color.WHITE, p);
            Console.WriteLine(p2.ToUsi());

            Square sq = Square.SQ_56;
            //Console.WriteLine(sq.ToFile().ToUSI() + sq.ToRank().ToUSI());
            Console.WriteLine(sq.ToUsi());
            Console.WriteLine(sq.Pretty());

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

            Hand h = Hand.ZERO;
            h.Add(Piece.PAWN, 5);
            h.Add(Piece.KNIGHT, 1);
            Console.WriteLine(h.Pretty());
            Console.WriteLine(h.ToUsi(Model.Shogi.Color.BLACK));
            Console.WriteLine(h.ToUsi(Model.Shogi.Color.WHITE));

            var pos = new Position();
            pos.SetSfen(Position.SFEN_HIRATE);
            Console.WriteLine(pos.ToUsi());

            // 指し手生成祭りの局面
            pos.SetSfen("l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w RGgsn5p 1");
            Console.WriteLine(pos.ToUsi());


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
