using System;
using System.Management;
using System.Text;
using System.Windows.Forms;
/*
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Management;
using System.Text;
using System.Windows.Forms;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.Converter;
*/

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
                // EvalValue の .Pretty() 出力テスト
                for (Int32 i = -2; i <= 2; ++i)
                    Console.WriteLine($"{i}: {MyShogi.Model.Shogi.Core.EvalValueExtensions.Pretty((MyShogi.Model.Shogi.Core.EvalValue)i)}");
                for (Int32 i = Int32.MinValue; i <= Int32.MinValue + 3; ++i)
                    Console.WriteLine($"{i}: {MyShogi.Model.Shogi.Core.EvalValueExtensions.Pretty((MyShogi.Model.Shogi.Core.EvalValue)i)}");
                for (Int32 i = Int32.MaxValue; i >= Int32.MaxValue - 3; --i)
                    Console.WriteLine($"{i}: {MyShogi.Model.Shogi.Core.EvalValueExtensions.Pretty((MyShogi.Model.Shogi.Core.EvalValue)i)}");
            }
#endif
#if true
            try
            {
                // 評価値グラフの表示テスト
                var evaltest = new EvalControlTestForm();
                evaltest.Show();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
#endif
#if false
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine($"Environment.Is64BitOperatingSystem: {Environment.Is64BitOperatingSystem}");
                sb.AppendLine($"Environment.Is64BitProcess: {Environment.Is64BitProcess}");
                sb.AppendLine($"Environment.ProcessorCount: {Environment.ProcessorCount}");

                var cpuid = Model.Common.Utility.CpuId.flags;

                for (UInt32 i = 0; i < cpuid.basicLength; ++i)
                for (UInt32 j = 0; j < 4; ++j)
                {
                    Console.WriteLine($"{(i):X8}{(char)(j + 'a')}: {cpuid.getBasic(i, j):X8}");
                }
                for (UInt32 i = 0; i < cpuid.extendLength; ++i)
                for (UInt32 j = 0; j < 4; ++j)
                {
                    Console.WriteLine($"{(i | 0x80000000):X8}{(char)(j + 'a')}: {cpuid.getExtend(i, j):X8}");
                }

                sb.AppendLine($"processorArchitecture: {cpuid.processorArchitecture}");
                sb.AppendLine($"cpuTarget: {cpuid.cpuTarget}");
                sb.AppendLine($"vendorId: {cpuid.vendorId}");
                sb.AppendLine($"brand: {cpuid.brand}");
                sb.AppendLine($"hasSSE2: {cpuid.hasSSE2}");
                sb.AppendLine($"hasSSE41: {cpuid.hasSSE41}");
                sb.AppendLine($"hasSSE42: {cpuid.hasSSE42}");
                sb.AppendLine($"hasAVX2: {cpuid.hasAVX2}");
                sb.AppendLine($"hasAVX512F: {cpuid.hasAVX512F}");

                using (ManagementClass mc = new ManagementClass("Win32_OperatingSystem"))
                using (ManagementObjectCollection moc = mc.GetInstances())
                foreach (ManagementObject mo in moc)
                {
                    foreach (string key in new[] {
                        // OSに利用可能な物理メモリのサイズ(kB)
                        "TotalVisibleMemorySize",
                        // 現在使用されていない利用可能な物理メモリのサイズ(kB)
                        "FreePhysicalMemory",
                        // 仮想メモリのサイズ(kB)
                        "TotalVirtualMemorySize",
                        // 現在使用されていない利用可能な仮想メモリのサイズ(kB)
                        "FreeVirtualMemory",
                        // ほかのページをスワップアウトすることなくOSのページングファイルにマップできるサイズ(kB)
                        "FreeSpaceInPagingFiles",
                        // OSのページングファイルで格納されるサイズ(kB)
                        "SizeStoredInPagingFiles",
                    })
                    {
                        sb.AppendLine($"{key}: {mo[key]:N0}kB");
                    }
                    mo.Dispose();
                }

                Console.WriteLine(sb);
                MessageBox.Show(sb.ToString(), "SystemInfo", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
#endif
#if false
            try
            {
                var man = new KifuManager();

                // Blunder.Converterで書き出した棋譜の読み込み(CSA)
                using (var fs = new FileStream(@"kif\in_csa.sfen", FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                for (var i = 0; i < 66869; ++i)
                {
                    using (var sr = new StreamReader($"kif\\records20151115_csa\\records20151115_{i:00000}.csa", Encoding.GetEncoding(932)))
                        man.FromString(sr.ReadToEnd());
                    sw.WriteLine(man.ToString(KifuFileType.SFEN));
                }

                // Blunder.Converterで書き出した棋譜の読み込み(KIF)
                using (var fs = new FileStream(@"kif\in_kif.sfen", FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                for (var i = 0; i < 66869; ++i)
                {
                    using (var sr = new StreamReader($"kif\\records20151115_kif\\records20151115_{i:00000}.kif", Encoding.GetEncoding(932)))
                        man.FromString(sr.ReadToEnd());
                    sw.WriteLine(man.ToString(KifuFileType.SFEN));
                }

                // KifManagerで書き出した棋譜の読み込み(CSA)
                using (var fs = new FileStream(@"kif\out_csa.sfen", FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                for (var i = 1; i <= 66869; ++i)
                {
                    using (var sr = new StreamReader($"kif\\out_csa\\{i:00000000}.csa", Encoding.GetEncoding(932)))
                        man.FromString(sr.ReadToEnd());
                    sw.WriteLine(man.ToString(KifuFileType.SFEN));
                }

                // KifManagerで書き出した棋譜の読み込み(KIF)
                using (var fs = new FileStream(@"kif\out_kif.sfen", FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                for (var i = 1; i <= 66869; ++i)
                {
                    using (var sr = new StreamReader($"kif\\out_kif\\{i:00000000}.kif", Encoding.GetEncoding(932)))
                        man.FromString(sr.ReadToEnd());
                    sw.WriteLine(man.ToString(KifuFileType.SFEN));
                }

                // KifManagerで書き出した棋譜の読み込み(KI2)
                using (var fs = new FileStream(@"kif\out_ki2.sfen", FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                for (var i = 1; i <= 66869; ++i)
                {
                    using (var sr = new StreamReader($"kif\\out_ki2\\{i:00000000}.ki2", Encoding.GetEncoding(932)))
                        man.FromString(sr.ReadToEnd());
                    sw.WriteLine(man.ToString(KifuFileType.SFEN));
                }

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
#endif
#if false
            try
            {
                Directory.CreateDirectory(@"kif\out_csa");
                Directory.CreateDirectory(@"kif\out_kif");
                Directory.CreateDirectory(@"kif\out_ki2");
                var man = new KifuManager();
                using (var sr = new StreamReader(@"kif\records20151115.sfen", Encoding.GetEncoding(932)))
                for (var i = 1; !sr.EndOfStream; ++i)
                {
                    var line = sr.ReadLine();
                    var res = man.FromString(line);

                    using (var fs = new FileStream($"kif\\out_csa\\{i:00000000}.csa", FileMode.Create))
                    using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                    {
                        sw.Write(man.ToString(KifuFileType.CSA));
                    }
                    using (var fs = new FileStream($"kif\\out_kif\\{i:00000000}.kif", FileMode.Create))
                    using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                    {
                        sw.Write(man.ToString(KifuFileType.KIF));
                    }
                    using (var fs = new FileStream($"kif\\out_ki2\\{i:00000000}.ki2", FileMode.Create))
                    using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                    {
                        sw.Write(man.ToString(KifuFileType.KI2));
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
#endif
#if false
            try
            {
                foreach (var uri in new string[] {
                    // 2016-04-09第1期電王戦第１局山崎隆之叡王Ponanza
                    "https://s3-ap-northeast-1.amazonaws.com/prod-kifu-cache-strage-tokyo/caches/56f9e9f6813cd7030042378d.json",
                    // 2018-05-26第3期叡王戦決勝七番勝負第4局金井恒太六段高見泰地六段
                    "https://s3-ap-northeast-1.amazonaws.com/prod-kifu-cache-strage-tokyo/caches/5acf14350468f80004b24166.json",
                })
                {
                    WebRequest req = WebRequest.Create(uri);
                    req.Timeout = 3000;
                    WebResponse res = req.GetResponse();
                    MemoryStream ms = new MemoryStream();
                    Stream st = res.GetResponseStream();
                    st.CopyTo(ms);
                    string str = Encoding.UTF8.GetString(ms.ToArray());
                    var man = new KifuManager();
                    man.FromString(str);
                    var csa = man.ToString(KifuFileType.CSA);
                    var kif = man.ToString(KifuFileType.KIF);
                    var ki2 = man.ToString(KifuFileType.KI2);
                    man.FromString(csa);
                    var sfen_csa = man.ToString(KifuFileType.JSON);
                    man.FromString(kif);
                    var sfen_kif = man.ToString(KifuFileType.JSON);
                    man.FromString(ki2);
                    var sfen_ki2 = man.ToString(KifuFileType.JSON);
                    Console.WriteLine("# LiveJSON");
                    Console.WriteLine(str);
                    Console.WriteLine("# CSA");
                    Console.WriteLine(csa);
                    Console.WriteLine(sfen_csa);
                    Console.WriteLine("# KIF");
                    Console.WriteLine(kif);
                    Console.WriteLine(sfen_kif);
                    Console.WriteLine("# KI2");
                    Console.WriteLine(ki2);
                    Console.WriteLine(sfen_ki2);
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
#endif
#if false
            try
            {
                var epoch = new DateTimeOffset(1970, 1, 1, 9, 0, 0, new TimeSpan(9, 0, 0));
                foreach (var uri in new string[] {
                    // 2016-04-09第1期電王戦第１局山崎隆之叡王Ponanza
                    "https://s3-ap-northeast-1.amazonaws.com/prod-kifu-cache-strage-tokyo/caches/56f9e9f6813cd7030042378d.json",
                    // 2018-05-26第3期叡王戦決勝七番勝負第4局金井恒太六段高見泰地六段
                    "https://s3-ap-northeast-1.amazonaws.com/prod-kifu-cache-strage-tokyo/caches/5acf14350468f80004b24166.json",
                })
                {
                    WebRequest req = WebRequest.Create(uri);
                    req.Timeout = 3000;
                    WebResponse res = req.GetResponse();
                    MemoryStream ms = new MemoryStream();
                    Stream st = res.GetResponseStream();
                    st.CopyTo(ms);
                    string str = Encoding.UTF8.GetString(ms.ToArray());
                    Console.WriteLine(str);
                    KifuManager man = new KifuManager();
                    man.FromString(str);
                    Console.WriteLine(man.ToString(KifuFileType.JSON));
                    /*
                    var jsonObj = LiveJsonUtil.FromString(str);
                    foreach (var data in jsonObj.data)
                    {
                        Console.WriteLine(String.Format("fname:\"{0}\" event:\"{1}\"", data.fname, data.eventName));
                        Console.WriteLine(String.Format("side:\"{0}\" player1:\"{1}\" player2:\"{2}\"", data.side, data.player1, data.player2));
                        Console.WriteLine(String.Format("recordman:\"{0}\" handicap:\"{1}\"", data.recordman, data.handicap));
                        foreach (var breaktime in data.breaktime)
                        {
                            var start = breaktime.start != null ? epoch.AddMilliseconds((double)breaktime.start).ToString("o") : null;
                            var end = breaktime.end != null ? epoch.AddMilliseconds((double)breaktime.end).ToString("o") : null;
                            Console.WriteLine(String.Format("reason:\"{0}\" start:{1} end:{2}", breaktime.reason, start, end));
                        }
                        foreach (var kif in data.kif)
                        {
                            var time = kif.time != null ? epoch.AddMilliseconds((double)kif.time).ToString("o") : null;
                            Console.WriteLine(String.Format("num:{0}, move:{1}, frX:{2}, frY:{3}, toX:{4}, toY:{5}, type:{6}, prmt:{7}, spend:{8}, time:{9}", kif.num, kif.move, kif.frX, kif.frY, kif.toX, kif.toY, kif.type, kif.prmt, kif.spend, time));
                        }
                    }
                    Console.WriteLine(jsonObj.ToJson());
                    Console.WriteLine(jsonObj.ToXml());
                    */
                }
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e);
            }
#endif
#if false
            {
                FontFamily[] ffs = FontFamily.Families;
                //FontFamilyの名前を列挙する
                foreach (FontFamily ff in ffs)
                {
                    Console.WriteLine(ff.Name);
                }
            }
#endif
#if false
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
#if false
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
#if false
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
#if false
            {
                var pos = new Position();

                // Csa/Kif/Ki2形式の局面・指し手出力
                // ～寄のテスト
                pos.UsiPositionCmd("startpos moves 7g7f 8c8d 2g2f 8d8e 2f2e 4a3b 8h7g 3c3d 7i6h 2b7g+ 6h7g 3a2b 3i3h 2b3c 3h2g 7c7d 2g2f 7a7b 2f1e B*4e 6i7h 1c1d 2e2d 2c2d 1e2d P*2g 2h4h 3c2d B*5e 6c6d 5e1a+ 2a3c 1a2a 3b4b P*2c S*2h 4g4f 4e6c 2c2b+ 3c2e 4f4e 2d3e 3g3f 2e3g+ 2i3g 2h3g+ 4h6h 3e4f 2b3b");
                Console.WriteLine(pos.Pretty());
                Console.WriteLine(pos.ToCsa());
                Console.WriteLine(pos.ToBod());
                Move m7 = Shogi.Core.Util.FromUsiMove("4b5b");
                Console.WriteLine(pos.ToCSA(m7));
                Console.WriteLine(pos.ToKif(m7));
                Console.WriteLine(pos.ToKi2(m7));
                Console.WriteLine(new KifFormatterOptions()
                {
                    color = ColorFormat.KIF,
                    square = SquareFormat.FullWidthArabic,
                    samepos = SamePosFormat.KI2sp,
                    fromsq = FromSqFormat.KI2
                }.format(pos, m7));
                // ☗☖⛊⛉はShift_JISでは表現できないのでコンソールでは化ける
                Console.WriteLine(new KifFormatterOptions()
                {
                    color = ColorFormat.Piece,
                    square = SquareFormat.ASCII,
                    samepos = SamePosFormat.KI2sp,
                    fromsq = FromSqFormat.KI2
                }.format(pos, m7));

                // 同～成のテスト
                pos.UsiPositionCmd("startpos moves 7g7f 8c8d 2g2f 8d8e 2f2e 4a3b 8h7g 3c3d 7i6h 2b7g+ 6h7g 3a2b 3i3h 2b3c 3h2g 7c7d 2g2f 7a7b 2f1e B*4e 6i7h 1c1d 2e2d 2c2d 1e2d P*2g 2h4h 3c2d B*5e 6c6d 5e1a+ 2a3c 1a2a 3b4b P*2c S*2h 4g4f 4e6c 2c2b+ 3c2e 4f4e 2d3e 3g3f 2e3g+ 2i3g");
                Console.WriteLine(pos.Pretty());
                Console.WriteLine(pos.ToCsa());
                Console.WriteLine(pos.ToBod());
                Move m8 = Shogi.Core.Util.FromUsiMove("2h3g+");
                Console.WriteLine(pos.ToCSA(m8));
                Console.WriteLine(pos.ToKif(m8));
                Console.WriteLine(pos.ToKi2(m8));
                Console.WriteLine(new KifFormatterOptions()
                {
                    color = ColorFormat.KIF,
                    square = SquareFormat.FullWidthArabic,
                    samepos = SamePosFormat.Verbose,
                    fromsq = FromSqFormat.KIF
                }.format(pos, m8));
                // ☗☖⛊⛉はShift_JISでは表現できないのでコンソールでは化ける
                Console.WriteLine(new KifFormatterOptions()
                {
                    color = ColorFormat.Piece,
                    square = SquareFormat.ASCII,
                    samepos = SamePosFormat.Verbose,
                    fromsq = FromSqFormat.KI2
                }.format(pos, m8));
            }
#endif
        }
    }
}
