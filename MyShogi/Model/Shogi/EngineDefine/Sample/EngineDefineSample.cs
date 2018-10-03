using MyShogi.Model.Common.String;
using System.Collections.Generic;
using System.Linq;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// 『将棋神　やねうら王』の各エンジンの"engine_define.xml"を書き出すサンプル
    /// </summary>
    public static class EngineDefineSample
    {
        /// <summary>
        /// 『将棋神　やねうら王』の5つのエンジンの"engine_define.xml"を書き出す。
        /// engine/フォルダ配下の各フォルダに書き出す。
        /// </summary>
        public static void WriteEngineDefineFiles2018()
        {
            // 各棋力ごとのエンジンオプション
            // (これでエンジンのdefault optionsがこれで上書きされる)
            var preset_default_array = new[] {

                // -- 棋力制限なし
                new EnginePreset("将棋神" ,
                    "棋力制限一切なしで強さは設定された持ち時間、PCスペックに依存します。\r\n" +
                    "CPU負荷率が気になる方は、詳細設定の「スレッド数」のところで調整してください。"
                        ,new EngineOption[] {
                            // スレッドはエンジンの詳細設定に従う
                            new EngineOption("NodesLimit","0"),
                            new EngineOption("DepthLimit","0"),
                            new EngineOption("MultiPv","1"),

                        // 他、棋力に関わる部分は設定すべき…。
                }) ,

                // -- 段位が指定されている場合は、NodesLimitで調整する。

                // スレッド数で棋力が多少変わる。4スレッドで計測したのでこれでいく。
                // 実行環境の論理スレッド数がこれより少ない場合は、自動的にその数に制限される。

                // ここの段位は、持ち時間15分切れ負けぐらいの時の棋力。

                /*
                  uuunuuunさんの実験によるとthreads = 4で、
                    rating =  386.16 ln( nodes/1000) + 1198.8
                  の関係があるらしいのでここからnodes数を計算。
                  ratingは将棋倶楽部24のものとする。またlnは自然対数を意味する。

                  二次式で近似したほうが正確らしく、uuunuuunさんいわく「この式を使ってください」とのこと。
                  NodesLimit = 1000*Exp[(537-Sqrt[537^2 + 4*26.13(975-rate)]/(2*26.13))]

                  Excelの式で言うと　=1000*EXP((537-SQRT(537^2+4*26.13*(975-A1)))/(2*26.13))

                        4600  32565907 // 16段
                        4400  16537349 // 15段
                        4200   8397860 // 14段
                        4000   4264532 // 13段
                        3800   2165579 // 12段 // ここまで使う
                        3600   1099707 // 11段
                        3400    558444 // 10段
                        3200	315754→283584 // 九段
                        3000	144832→144007 // 八段
                        2800	73475 // 七段
                        2600	39959 // 六段
                        2400	22885 // 五段
                        2200	13648 // 四段
                        2000	8410 // 三段
                        1800	5325 // 二段
                        1600	3450 // 初段
                        1500	2799 // 1級
                        1400	2281 // 2級 →　新1級
                        1300	1867 // 3級
                        1200	1534 // 4級 →　新2級
                        1100	1266 // 5級
                        1000	1048 // 6級 →　新3級
                        900	870 //  7級
                        800	726 //  8級 → 新4級
                        700	607 //  9級
                        600	509 // 10級 → 新5級

                        500 428 // 11級
                        400 361 // 12級 → 新6級
                        300 305 // 13級
                        200 258 // 14級 → 新7級
                        100 219 // 15級

                        以下、線形補間で外挿するなら
                        0    187 // 16級 → 新8級
                        -100 159 // 17級
                        -200 136 // 18級 → 新9級
                        -300 117 // 19級
                        -400 100 // 20級 → 新10級

                        // このへん、SkillLevel使わないと調整不可能。

                        -500 86
                        -600 74 // → 新11級(相当)
                        -700 64
                        -800 55 // → 新12級(相当)
                        -900 48


                 */

                // 8段 = R3000
                // 9段は将棋倶楽部24では存在しない(?) 初段からはR200ごとの増加なので、おそらくR3200。

                // 10段、11段、12段は、自動スレッドでのスレッド割り当て。
                // 理由1. node数が多いので、スレッド数が増えてもそこまで強さがバラけないと思われるため
                // 理由2. node数が多いので2スレッドで探索すると時間がかかるため

                new EnginePreset( "十二段" , new EngineOption[] {
                        //new EngineOption("AutoThread_","true"), // 自動スレッドでのスレッド割り当て
                        // →　エンジン詳細設定の値、上書きするのあまりいい挙動ではないのでやめとく。
                        // (MyShogiを２つ起動したいかも知れないので..)
                        new EngineOption("NodesLimit","2165579"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset( "十一段" , new EngineOption[] {
                        //new EngineOption("AutoThread_","true"), // 自動スレッドでのスレッド割り当て
                        new EngineOption("NodesLimit","1099707"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset( "十段" , new EngineOption[] {
                        //new EngineOption("AutoThread","true"), // 自動スレッドでのスレッド割り当て
                        new EngineOption("NodesLimit","558444"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset( "九段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","283584"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("八段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","144007"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("七段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","73475"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("六段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","39959"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("五段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","22885"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("四段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","13648"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("三段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","8410"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("二段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","5325"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("初段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","3450"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("１級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","2281"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("２級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","1534"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("３級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","1048"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("４級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","726"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("５級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","509"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("６級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","361"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("７級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","258"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("８級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","187"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("９級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","136"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),
                new EnginePreset("10級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","100"),
                        new EngineOption("SkillLevel","20"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","1"),
                }),

                // 以下、SkillLevelを用いた人間らしい棋力バランスのプリセット

                // スレッド数を増やすと探索が全体的に浅くなる→SkillLevelのmove_to_pickに達しなくなる→単なるMultiPVになる→つよい
                // みたいな流れはあるようだ。これは困った。スレッド数を2に固定する。

                new EnginePreset("Ｓ九段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","39959"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ八段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","1200000"),       // [2018/10/03 15:30] , 257-5-251 R-4
                        new EngineOption("SkillLevel","11"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ七段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","500000"),        // [2018/10/02 16:40]
                        new EngineOption("SkillLevel","11"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ六段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","230000"),        // [2018/10/02 01:00]
                        new EngineOption("SkillLevel","9"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ五段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","130000"),        // [2018/10/02 13:20]
                        new EngineOption("SkillLevel","8"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ四段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","150000"),         // [2018/10/02 17:00]
                        new EngineOption("SkillLevel","7"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ三段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","66000"),         // [2018/10/03 06:20] , 431-8-366 R-28
                        new EngineOption("SkillLevel","6"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ二段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","30000"),         // [2018/10/03 03:30] , 316-6-319 R+2
                        new EngineOption("SkillLevel","6"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ初段" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","22000"),          // [2018/10/03 02:15] , 281-4-268 R+8
                        new EngineOption("SkillLevel","5"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ１級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","2281"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ２級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","1534"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ３級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","1048"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ４級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","726"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ５級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","509"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ６級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","361"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ７級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","258"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ８級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","187"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ９級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","136"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ10級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","100"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ11級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","100"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ12級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","100"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ13級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","100"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ14級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","100"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),
                new EnginePreset("Ｓ15級" , new EngineOption[] {
                        new EngineOption("AutoThread_","false"),
                        new EngineOption("Threads","2"),
                        new EngineOption("NodesLimit","100"),
                        new EngineOption("SkillLevel","3"),
                        new EngineOption("DepthLimit","0"),
                        new EngineOption("MultiPv","4"),
                }),

            };

            // presetのDescription
            {
                // 2個目以降を設定。
                for (var i = 1; i < preset_default_array.Length; ++i)
                {
                    var preset = preset_default_array[i];

                    if (preset.Name.IsEmpty())
                        preset.Description = null;
                    else if (!preset.Name.StartsWith("Ｓ"))
                        preset.Description = preset.Name + "ぐらいの強さになるように棋力を調整したものです。持ち時間、PCのスペックにほとんど依存しません。" +
                            "短い持ち時間だと切れ負けになるので持ち時間無制限での対局をお願いします。";
                    else
                        preset.Description = preset.Name.Substring(1) + "ぐらいの強さになるように棋力を調整したものです。" +
                            "棋力名が「Ｓ」で始まるは、序盤が弱いのに終盤だけ強いということはなく、まんべんなく同じ強さになるように調整されています。" +
                            "思考時間は「Ｓ」のつかない同じ段位のものの10倍ぐらい必要となります。";

                        // + "また、段・級の設定は、将棋倶楽部24基準なので町道場のそれより少し辛口の調整になっています。";
                        // あえて書くほどでもないか…。
                }
            }

            var default_preset = new List<EnginePreset>(preset_default_array);

            var default_cpus = new List<CpuType>(new[] { CpuType.NO_SSE, CpuType.SSE2, CpuType.SSE41, CpuType.SSE42, CpuType.AVX2 });

            var default_extend = new List<ExtendedProtocol>( new[] { ExtendedProtocol.UseHashCommandExtension , ExtendedProtocol.HasEvalShareOption } );
            var default_nnue_extend = new List<ExtendedProtocol>(new[] { ExtendedProtocol.UseHashCommandExtension });
            var gps_extend = new List<ExtendedProtocol>( new[] { ExtendedProtocol.UseHashCommandExtension } );

            // EngineOptionDescriptionsは、エンジンオプション共通設定に使っているDescriptionsと共用。
            var common_setting = EngineCommonOptionsSample.CreateEngineCommonOptions(new EngineCommonOptionsSampleOptions() {
                UseEvalDir = true, // ただし、"EvalDir"オプションはエンジンごとに固有に異なる値を保持しているのが普通であるから共通オプションにこの項目を足してやる。
            });

            var default_descriptions = common_setting.Descriptions;
            var default_descriptions_nnue = new List<EngineOptionDescription>(default_descriptions);
            default_descriptions_nnue.RemoveAll(x => x.Name == "EvalShare"); // NNUEはEvalShare持ってない。

            // -- 各エンジン用の設定ファイルを生成して書き出す。

            {
                // やねうら王
                var engine_define = new EngineDefine()
                {
                    DisplayName = "やねうら王",
                    EngineExeName = "yaneuraou2018_kpp_kkpt",
                    SupportedCpus = default_cpus ,
                    EvalMemory = 480, // KPP_KKPTは、これくらい？
                    WorkingMemory = 200 ,
                    StackPerThread = 40, // clangでコンパイルの時にstack size = 25[MB]に設定している。ここに加えてheapがスレッド当たり15MBと見積もっている。
                    Presets = default_preset,
                    DescriptionSimple = "やねうら王 2018年度版",
                    Description = "プロの棋譜を一切利用せずに自己学習で身につけた異次元の大局観。"+
                        "従来の将棋の常識を覆す指し手が飛び出すかも？",
                    DisplayOrder = 10005,
                    SupportedExtendedProtocol = default_extend,
                    EngineOptionDescriptions = default_descriptions,
                };
                EngineDefineUtility.WriteFile("engine/yaneuraou2018/engine_define.xml", engine_define);

                // 試しに実行ファイル名を出力してみる。
                //Console.WriteLine(EngineDefineUtility.EngineExeFileName(engine_define));
            }

            {
                // tanuki_sdt5
                var engine_define = new EngineDefine()
                {
                    DisplayName = "tanuki- SDT5",
                    EngineExeName = "YaneuraOu2018KPPT",
                    SupportedCpus = default_cpus,
                    EvalMemory = 850, // KPPTは、これくらい？
                    WorkingMemory = 150,
                    StackPerThread = 40, // clangでコンパイルの時にstack size = 25[MB]に設定している。ここに加えてheapがスレッド当たり15MBと見積もっている。
                    Presets = default_preset,
                    DescriptionSimple = "tanuki- SDT5版",
                    Description = "SDT5(第5回 将棋電王トーナメント)で絶対王者Ponanzaを下し堂々の優勝を果たした実力派。" +
                        "SDT5 出場名『平成将棋合戦ぽんぽこ』",
                    DisplayOrder = 10004,
                    SupportedExtendedProtocol = default_extend,
                    EngineOptionDescriptions = default_descriptions,
                };
                EngineDefineUtility.WriteFile("engine/tanuki_sdt5/engine_define.xml", engine_define);
            }

            {
                // tanuki2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "tanuki- 2018",
                    EngineExeName = "YaneuraOu2018NNUE",
                    SupportedCpus = default_cpus,
                    EvalMemory = 200, // NNUEは、これくらい？
                    WorkingMemory = 200,
                    StackPerThread = 40, // clangでコンパイルの時にstack size = 25[MB]に設定している。ここに加えてheapがスレッド当たり15MBと見積もっている。
                    Presets = default_preset,
                    DescriptionSimple = "tanuki- 2018年版",
                    Description = "WCSC28(第28回 世界コンピュータ将棋選手権)に出場した時からさらに強化されたtanuki-シリーズ最新作。" +
                        "ニューラルネットワークを用いた評価関数で、他のソフトとは毛並みの違う新時代のコンピュータ将棋。"+
                        "PC性能を極限まで使うため、CPUの温度が他のソフトの場合より上がりやすいので注意してください。",
                    DisplayOrder = 10003,
                    SupportedExtendedProtocol = default_nnue_extend,
                    EngineOptionDescriptions = default_descriptions_nnue,
                };
                EngineDefineUtility.WriteFile("engine/tanuki2018/engine_define.xml", engine_define);
            }

            {
                // qhapaq2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "Qhapaq 2018",
                    EngineExeName = "YaneuraOu2018KPPT",
                    SupportedCpus = default_cpus,
                    EvalMemory = 850, // KPPTは、これくらい？
                    WorkingMemory = 200,
                    StackPerThread = 40, // clangでコンパイルの時にstack size = 25[MB]に設定している。ここに加えてheapがスレッド当たり15MBと見積もっている。
                    Presets = default_preset,
                    DescriptionSimple = "Qhapaq 2018年版",
                    Description = "河童の愛称で知られるQhapaqの最新版。"+
                        "非公式なレーティング計測ながら2018年6月時点で堂々の一位の超強豪。",
                    DisplayOrder = 10002,
                    SupportedExtendedProtocol = default_extend,
                    EngineOptionDescriptions = default_descriptions,
                };
                EngineDefineUtility.WriteFile("engine/qhapaq2018/engine_define.xml", engine_define);
            }

            {
                // yomita2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "読み太 2018",
                    EngineExeName = "YaneuraOu2018KPPT",
                    SupportedCpus = default_cpus,
                    EvalMemory = 850, // KPPTは、これくらい？
                    WorkingMemory = 200,
                    StackPerThread = 40, // clangでコンパイルの時にstack size = 25[MB]に設定している。ここに加えてheapがスレッド当たり15MBと見積もっている。
                    Presets = default_preset,
                    DescriptionSimple = "読み太 2018年版",
                    Description = "直感精読の個性派、読みの確かさに定評あり。" +
                        "毎回、大会で上位成績を残している常連組。",
                    DisplayOrder = 10001,
                    SupportedExtendedProtocol = default_extend,
                    EngineOptionDescriptions = default_descriptions,
                };
                EngineDefineUtility.WriteFile("engine/yomita2018/engine_define.xml", engine_define);
            }

#if false
            {
                // gpsfish(動作テスト用) 『将棋神　やねうら王』には含めない。
                var engine_define = new EngineDefine()
                {
                    DisplayName = "gpsfish",
                    EngineExeName = "gpsfish",
                    SupportedCpus = new List<CpuType>(new[] { CpuType.SSE2 }),
                    EvalMemory = 10, // gpsfishこれくらいで動くような？
                    WorkingMemory = 100,
                    StackPerThread = 25,
                    Presets = default_preset,
                    DescriptionSimple = "GPS将棋(テスト用)",
                    Description = "いまとなっては他のソフトと比べると棋力的には見劣りがするものの、" +
                        "ファイルサイズが小さいので動作検証用に最適。",
                    DisplayOrder = 10000,
                    SupportedExtendedProtocol = gps_extend,
                    EngineOptionDescriptions = null,
                };
                EngineDefineUtility.WriteFile("engine/gpsfish/engine_define.xml", engine_define);

                //Console.WriteLine(EngineDefineUtility.EngineExeFileName(engine_define));
            }

            {
                // gpsfish2(動作テスト用) 『将棋神　やねうら王』には含めない。
                // presetの動作テストなどに用いる。
                var engine_define = new EngineDefine()
                {
                    DisplayName = "Gpsfish2",
                    EngineExeName = "gpsfish",
                    SupportedCpus = new List<CpuType>(new[] { CpuType.SSE2 }),
                    EvalMemory = 10, // gpsfishこれくらいで動くような？
                    WorkingMemory = 100,
                    StackPerThread = 25,
                    Presets = default_preset,
                    DescriptionSimple = "GPS将棋2(テスト用)",
                    Description = "presetなどのテスト用。",
                    DisplayOrder = 9999,
                    SupportedExtendedProtocol = gps_extend,
                    EngineOptionDescriptions = default_descriptions,
                };
                EngineDefineUtility.WriteFile("engine/gpsfish2/engine_define.xml", engine_define);
            }

#endif

            {
                // -- 詰将棋エンジン

                // このnamesにあるもの以外、descriptionから削除してしまう。
                var names = new[] { "AutoHash_","Hash_" ,"AutoThread_","Threads","MultiPV","WriteDebugLog","NetworkDelay","NetworkDelay2", "MinimumThinkingTime",
                    "SlowMover","DepthLimit","NodesLimit","Contempt","ContemptFromBlack","EvalDir","MorePreciseMatePV","MaxMovesToDraw"};

                // このnamesHideにあるものは隠す。
                var namesHide = new[] { "SlowMover", "Comtempt", "ContemptFromBlack" , "EvalDir"};

                var descriptions = new List<EngineOptionDescription>();
                foreach (var d in default_descriptions)
                {
                    // この見出し不要
                    if (d.DisplayName == "定跡設定" || d.DisplayName == "評価関数の設定")
                        continue;

                    if (names.Contains(d.Name) || d.Name == null /* 見出し項目なので入れておく */)
                    {
                        if (namesHide.Contains(d.Name))
                            d.Hide = true;

                        descriptions.Add(d);
                    }
                }

                {
                    var d = new EngineOptionDescription("MorePreciseMatePv", null , "なるべく正確な詰み手順を返します。",
                        "この項目をオンにすると、なるべく正確な詰み手順を返すようになります。\r\n" +
                        "オフにしていると受け方(詰みを逃れる側)が最長になるように逃げる手順になりません。\r\n" +
                        "ただし、この項目をオンにしても攻め方(詰ます側)が最短手順の詰みになる手順を選択するとは限りません。",
                        "option name MorePreciseMatePv type check default true");

                    // 「読み筋の表示」の直後に挿入
                    var index = descriptions.FindIndex((x) => x.DisplayName == "読み筋の表示") + 1;
                    descriptions.Insert(index , d);
                }

                var engine_define = new EngineDefine()
                {
                    DisplayName = "tanuki-詰将棋エンジン",
                    EngineExeName = "tanuki_mate",
                    SupportedCpus = default_cpus,
                    EvalMemory = 0, // KPPTは、これくらい？
                    WorkingMemory = 200,
                    StackPerThread = 40, // clangでコンパイルの時にstack size = 25[MB]に設定している。ここに加えてheapがスレッド当たり15MBと見積もっている。
                    Presets = new List<EnginePreset>() ,
                    DescriptionSimple = "tanuki-詰将棋エンジン",
                    Description = "長手数の詰将棋が解ける詰将棋エンジンです。\r\n" +
                        "詰手順が最短手数であることは保証されません。\r\n" +
                        "複数スレッドでの探索には対応していません。",
                    DisplayOrder = 10006,
                    SupportedExtendedProtocol = default_extend,
                    EngineOptionDescriptions = descriptions,
                    EngineType = 1, // go mateコマンドに対応している。通常探索には使えない。
                };
                EngineDefineUtility.WriteFile("engine/tanuki_mate/engine_define.xml", engine_define);
            }

        }
    }
}
