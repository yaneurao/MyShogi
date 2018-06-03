using System;
using System.Drawing;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.Converter;

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
            {
                FontFamily[] ffs = FontFamily.Families;
                //FontFamilyの名前を列挙する
                foreach (FontFamily ff in ffs)
                {
                    Console.WriteLine(ff.Name);
                }
            }
#endif
#if true
            {
                // 文字幅計算テスト
                foreach (var s in new string[]{
                    "α", // U+03B1 Ambiguous
                    "Ａ", // U+FF21 Fullwidth
                    "ｱ", // U+FF71 Halfwidth
                    "À", // U+00C0 Neutral
                    "A", // U+0041 Narrow
                    "ア", // U+30A2 Wide
                    "𠮷", // U+20BB7 Wide
                    "𩸽", // U+29E3D Wide
                    "🤔", // U+1F914 Wide
                    "▲", // U+25B2 Ambiguous
                    "△", // U+25B3 Ambiguous
                    "▼", // U+25BC Ambiguous
                    "▽", // U+25BD Ambiguous
                    "☗", // U+2617 Neutral
                    "☖", // U+2616 Neutral
                    "⛊", // U+26CA Ambiguous
                    "⛉", // U+26C9 Ambiguous
                    "　", // U+3000 Fullwidth
                    " ", // U+0020 Narrow
                    "\t", // U+0009 Neutral
                    "\n", // U+000A Neutral
                    "\u1eaf", // U+1EAF Neutral (LATIN SMALL LETTER A WITH BREVE AND ACUTE)
                    "\u0103\u0301", // U+1EAF の合字表記1 (LATIN SMALL LETTER A WITH BREVE + COMBINING ACUTE ACCENT)
                    "\u0061\u0306\u0301", // U+1EAF の合字表記2 (LATIN SMALL LETTER A + COMBINING BREVE + COMBINING ACUTE ACCENT)
                    "\u304c", // U+304C (HIRAGANA LETTER GA)
                    "\u304b\u3099", // U+304B U+3099 (HIRAGANA LETTER KA + COMBINING KATAKANA-HIRAGANA VOICED SOUND)
                })
                {
                    Console.Out.WriteLine(String.Format(
                        "\"{0}\" width {1},{2}",
                        s,
                        EastAsianWidth.legacyWidth(s),
                        EastAsianWidth.modernWidth(s)
                    ));
                }
            }
#endif
#if true
            {
                // KIF形式の局面・指し手入力テスト
                Position pos = new Position();
                string sfen = KifExtensions.BodToSfen(new string[] {
                    "後手の持駒：なし",
                    "  ９ ８ ７ ６ ５ ４ ３ ２ １",
                    "+---------------------------+",
                    "|v玉vと ・ ・ ・ 馬 ・ ・ ・|一",
                    "|vとvと ・ 金 ・ ・ ・ ・ 馬|二",
                    "| ・ ・ ・ 金 金 金 ・ ・ ・|三",
                    "| ・ ・ ・ ・ ・ ・ ・ ・ ・|四",
                    "| ・ ・ ・ ・ 龍 ・ ・ ・ 龍|五",
                    "| ・ ・ ・ ・ ・ ・ ・ ・ ・|六",
                    "| ・ と ・ ・ ・ ・ 銀 ・ 銀|七",
                    "| と ・ と ・ ・ ・ ・ ・ ・|八",
                    "| と と と 桂 玉 桂 銀 銀 ・|九",
                    "+---------------------------+",
                    "先手の持駒：桂二 香四 歩九"
                });
                pos.SetSfen(sfen);
                Console.Out.WriteLine(pos.Pretty());
                foreach (string kif in new string[] {
                    "８８と左上",
                    "８８と直",
                    "８８と右上",
                    "８８と左寄",
                    "８８と右寄",
                    "８８と引",
                    "５７桂左",
                    "５７桂右",
                    "２８銀左上",
                    "２８銀直",
                    "２８銀左引",
                    "２８銀右",
                    "３５龍左",
                    "３５龍右",
                    "５２金左上",
                    "５２金寄",
                    "５２金直",
                    "５２金右",
                    "２３馬左",
                    "２３馬右",
                })
                {
                    Move m = pos.FromKif(kif);
                    Console.Out.WriteLine(string.Format("org:{0} usi:{1} csa:{2} kif:{3} ki2:{4} pretty:{5}", kif, m.ToUsi(), pos.ToCSA(m), pos.ToKif(m), pos.ToKi2(m), m.Pretty()));
                }
            }
#endif
#if true
            {
                // CSA形式の局面・指し手入力テスト
                Position pos = new Position();
                string sfen = CsaExtensions.CsaToSfen(new string[]{
                    "N+",
                    "N-",
                    "P1-OU-TO *  *  * +UM *  *  * ",
                    "P2-TO-TO * +KI *  *  *  * +UM",
                    "P3 *  *  * +KI+KI+KI *  *  * ",
                    "P4 *  *  *  *  *  *  *  *  * ",
                    "P5 *  *  *  * +RY *  *  * +RY",
                    "P6 *  *  *  *  *  *  *  *  * ",
                    "P7 * +TO *  *  *  * +GI * +GI",
                    "P8+TO * +TO *  *  *  *  *  * ",
                    "P9+TO+TO+TO+KE+OU+KE+GI+GI * ",
                    "P+00KE00KE00KY00KY00KY00KY00FU00FU00FU00FU00FU00FU00FU00FU00FU",
                    "P-",
                    "+"
                });
                pos.SetSfen(sfen);
                Console.Out.WriteLine(pos.Pretty());
                foreach (string csa in new string[] {
                    "+9988TO",
                    "+8988TO",
                    "+7988TO",
                    "+9888TO",
                    "+7888TO",
                    "+8788TO",
                    "+6957KE",
                    "+4957KE",
                    "+3928GI",
                    "+2928GI",
                    "+3728GI",
                    "+1728GI",
                    "+5535RY",
                    "+1535RY",
                    "+6352KI",
                    "+6252KI",
                    "+5352KI",
                    "+4352KI",
                    "+4123UM",
                    "+1223UM",
                    "9988TO",
                    "8988TO",
                    "7988TO",
                    "9888TO",
                    "7888TO",
                    "8788TO",
                    "6957KE",
                    "4957KE",
                    "3928GI",
                    "2928GI",
                    "3728GI",
                    "1728GI",
                    "5535RY",
                    "1535RY",
                    "6352KI",
                    "6252KI",
                    "5352KI",
                    "4352KI",
                    "4123UM",
                    "1223UM",
                })
                {
                    Move m = pos.FromCSA(csa);
                    Console.Out.WriteLine(string.Format("org:{0} usi:{1} csa:{2} kif:{3} ki2:{4} pretty:{5}", csa, m.ToUsi(), pos.ToCSA(m), pos.ToKif(m), pos.ToKi2(m), m.Pretty()));
                }
            }
#endif
#if true
            {
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
            }
#endif
        }
    }
}
