using System.Collections.Generic;

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
                new EnginePreset("将棋神" , "棋力制限一切なしで強さは持ち時間、PCスペックに依存します。",new EngineOption[] {
                        new EngineOption("NodesLimit","0"),

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

                        3200	315754 
                        3000	144832 
                        2800	73475 
                        2600	39959 
                        2400	22885 
                        2200	13648 
                        2000	8410 
                        1800	5325 
                        1600	3450 
                        1500	2799 
                        1400	2281 
                        1300	1867 
                        1200	1534 
                        1100	1266 
                        1000	1048 
                        900	870 
                        800	726 
                        700	607 
                        600	509 
                 */

                // 8段 = R3000
                // 9段は将棋倶楽部24では存在しない(?) 初段からはR200ごとの増加なので、おそらくR3200。

                new EnginePreset( "九段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","315754"),
                }),
                new EnginePreset("八段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","144832"),
                }),
                new EnginePreset("七段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","73475"),
                }),
                new EnginePreset("六段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","39959"),
                }),
                new EnginePreset("五段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","22885"),
                }),
                new EnginePreset("四段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","13648"),
                }),
                new EnginePreset("三段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","8410"),
                }),
                new EnginePreset("二段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","5325"),
                }),
                new EnginePreset("初段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","3450"),
                }),
                new EnginePreset("１級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","2799"),
                }),
                new EnginePreset("２級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","2281"),
                }),
                new EnginePreset("３級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","1867"),
                }),
                new EnginePreset("４級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","1534"),
                }),
                new EnginePreset("５級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","1266"),
                }),
                new EnginePreset("６級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","1048"),
                }),
                new EnginePreset("７級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","870"),
                }),
                new EnginePreset("８級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","726"),
                }),
                new EnginePreset("９級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","607"),
                }),
                new EnginePreset("10級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","509"),
                }),
            };

            // presetのDescription
            {
                // 2個目以降を設定。
                for (var i = 1; i < preset_default_array.Length; ++i)
                {
                    var preset = preset_default_array[i];

                    preset.Description = preset.Name + "ぐらいの強さになるように棋力を調整したものです。持ち時間、PCのスペックにほとんど依存しません。" +
                        "短い持ち時間だと切れ負けになるので持ち時間無制限での対局をお願いします。";
                }
            }

            var default_preset = new List<EnginePreset>(preset_default_array);

            var default_cpus = new List<CpuType>(new[] { CpuType.NO_SSE, CpuType.SSE2, CpuType.SSE41, CpuType.SSE42, CpuType.AVX2 });

            var default_extend = new List<ExtendedProtocol>( new[] { ExtendedProtocol.UseHashCommandExtension , ExtendedProtocol.HasEvalShareOption } );
            var default_nnue_extend = new List<ExtendedProtocol>(new[] { ExtendedProtocol.UseHashCommandExtension });
            var gps_extend = new List<ExtendedProtocol>( new[] { ExtendedProtocol.UseHashCommandExtension } );

            // EngineOptionDescriptionsは、エンジンオプション共通設定に使っているDescriptionsと共用。
            var common_setting = EngineCommonOptionsSample.CreateEngineCommonOptions();
            var default_descriptions = common_setting.Descriptions;

            // -- 各エンジン用の設定ファイルを生成して書き出す。

            {
                // やねうら王
                var engine_define = new EngineDefine()
                {
                    DisplayName = "やねうら王",
                    EngineExeName = "yaneuraou2018_kpp_kkpt",
                    SupportedCpus = default_cpus ,
                    EvalMemory = 480, // KPP_KKPTは、これくらい？
                    WorkingMemory = 30 , 
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
                    EvalMemory = 960, // KPPTは、これくらい？
                    WorkingMemory = 30,
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
                    EvalMemory = 300, // NNUEは、これくらい？
                    WorkingMemory = 30,
                    Presets = default_preset,
                    DescriptionSimple = "tanuki- 2018年版",
                    Description = "WCSC28(第28回 世界コンピュータ将棋選手権)に出場した時からさらに強化されたtanuki-シリーズ最新作。" +
                        "ニューラルネットワークを用いた評価関数で、他のソフトとは毛並みの違う新時代のコンピュータ将棋。"+
                        "PC性能を極限まで使うため、CPUの温度が他のソフトの場合より上がりやすいので注意してください。",
                    DisplayOrder = 10003,
                    SupportedExtendedProtocol = default_nnue_extend,
                    EngineOptionDescriptions = default_descriptions,
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
                    EvalMemory = 960, // KPPTは、これくらい？
                    WorkingMemory = 30,
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
                    EvalMemory = 960, // KPPTは、これくらい？
                    WorkingMemory = 30,
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

            {
                // gpsfish(動作テスト用) 『将棋神　やねうら王』には含めない。
                var engine_define = new EngineDefine()
                {
                    DisplayName = "gpsfish",
                    EngineExeName = "gpsfish",
                    SupportedCpus = new List<CpuType>(new[] { CpuType.SSE2 }),
                    EvalMemory = 10, // gpsfishこれくらいで動くような？
                    WorkingMemory = 30,
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
                    WorkingMemory = 30,
                    Presets = default_preset,
                    DescriptionSimple = "GPS将棋2(テスト用)",
                    Description = "presetなどのテスト用。",
                    DisplayOrder = 9999,
                    SupportedExtendedProtocol = gps_extend,
                    EngineOptionDescriptions = default_descriptions,
                };
                EngineDefineUtility.WriteFile("engine/gpsfish2/engine_define.xml", engine_define);
            }

        }
    }
}
