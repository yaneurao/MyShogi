using System;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;

namespace MyShogi.Model.Test
{
    /// <summary>
    /// Mizarさん用の開発時テストコード
    /// </summary>
    public static class DevTest2
    {
        public static void Test1()
        {
#if true
            var pos = new Position();

            // Csa/Kif/Ki2形式の局面・指し手出力
            // ～寄のテスト
            pos.UsiPositionCmd("startpos moves 7g7f 8c8d 2g2f 8d8e 2f2e 4a3b 8h7g 3c3d 7i6h 2b7g+ 6h7g 3a2b 3i3h 2b3c 3h2g 7c7d 2g2f 7a7b 2f1e B*4e 6i7h 1c1d 2e2d 2c2d 1e2d P*2g 2h4h 3c2d B*5e 6c6d 5e1a+ 2a3c 1a2a 3b4b P*2c S*2h 4g4f 4e6c 2c2b+ 3c2e 4f4e 2d3e 3g3f 2e3g+ 2i3g 2h3g+ 4h6h 3e4f 2b3b");
            Console.WriteLine(pos.Pretty());
            Console.WriteLine(Shogi.Converter.CsaExtensions.ToCsa(pos));
            Console.WriteLine(Shogi.Converter.KifExtensions.ToBod(pos));
            Move m7 = Shogi.Core.Util.FromUsiMove("4b5b");
            Console.WriteLine(Shogi.Converter.CsaExtensions.ToCSA(pos, m7));
            Console.WriteLine(Shogi.Converter.KifExtensions.ToKif(pos, m7, pos.State().lastMove));
            Console.WriteLine(Shogi.Converter.KifExtensions.ToKi2(pos, m7, pos.State().lastMove));
            Console.WriteLine(new Shogi.Converter.KifFormatter(
                Shogi.Converter.ColorFormat.KIF,
                Shogi.Converter.SquareFormat.FullWidthArabic,
                Shogi.Converter.SamePosFormat.KI2sp,
                Shogi.Converter.FromSqFormat.KI2
                ).format(pos, m7, pos.State().lastMove));
            // ☗☖⛊⛉はShift_JISでは表現できないのでコンソールでは化ける
            Console.WriteLine(new Shogi.Converter.KifFormatter(
                Shogi.Converter.ColorFormat.Piece,
                Shogi.Converter.SquareFormat.ASCII,
                Shogi.Converter.SamePosFormat.KI2sp,
                Shogi.Converter.FromSqFormat.KI2
                ).format(pos, m7, pos.State().lastMove));

            // 同～成のテスト
            pos.UsiPositionCmd("startpos moves 7g7f 8c8d 2g2f 8d8e 2f2e 4a3b 8h7g 3c3d 7i6h 2b7g+ 6h7g 3a2b 3i3h 2b3c 3h2g 7c7d 2g2f 7a7b 2f1e B*4e 6i7h 1c1d 2e2d 2c2d 1e2d P*2g 2h4h 3c2d B*5e 6c6d 5e1a+ 2a3c 1a2a 3b4b P*2c S*2h 4g4f 4e6c 2c2b+ 3c2e 4f4e 2d3e 3g3f 2e3g+ 2i3g");
            Console.WriteLine(pos.Pretty());
            Console.WriteLine(Shogi.Converter.CsaExtensions.ToCsa(pos));
            Console.WriteLine(Shogi.Converter.KifExtensions.ToBod(pos));
            Move m8 = Shogi.Core.Util.FromUsiMove("2h3g+");
            Console.WriteLine(Shogi.Converter.CsaExtensions.ToCSA(pos, m8));
            Console.WriteLine(Shogi.Converter.KifExtensions.ToKif(pos, m8, pos.State().lastMove));
            Console.WriteLine(Shogi.Converter.KifExtensions.ToKi2(pos, m8, pos.State().lastMove));
            Console.WriteLine(new Shogi.Converter.KifFormatter(
                Shogi.Converter.ColorFormat.KIF,
                Shogi.Converter.SquareFormat.FullWidthArabic,
                Shogi.Converter.SamePosFormat.Verbose,
                Shogi.Converter.FromSqFormat.KIF
                ).format(pos, m8, pos.State().lastMove));
            // ☗☖⛊⛉はShift_JISでは表現できないのでコンソールでは化ける
            Console.WriteLine(new Shogi.Converter.KifFormatter(
                Shogi.Converter.ColorFormat.Piece,
                Shogi.Converter.SquareFormat.ASCII,
                Shogi.Converter.SamePosFormat.Verbose,
                Shogi.Converter.FromSqFormat.KI2
                ).format(pos, m8, pos.State().lastMove));
#endif
        }
    }
}
