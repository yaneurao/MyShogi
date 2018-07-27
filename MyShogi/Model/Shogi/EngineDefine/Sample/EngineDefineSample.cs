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
                    rating =  295.14 ln( nodes/1000) + 1319.8
                  の関係があるらしいのでここからnodes数を計算。
                  ratingは将棋倶楽部24のものとする。またlnは自然対数を意味する。

                    ln (nodes/1000) = (rating - 1319.8) / 295.14
                     右辺をXとすると nodes = 1000 * e^X

                    Excelの式で言うとA1にratingが入っているとして、
                    =1000*2.718^((A1-1319.8)/295.14)
                 */

                // 8段 = R3000なので上式に代入するとnodes = 296575。以下同様。
                // 9段は将棋倶楽部24では存在しない(?) 初段からはR200ごとの増加なので、おそらくR3200。

                new EnginePreset( "九段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","583985"),
                }),
                new EnginePreset("八段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","296575"),
                }),
                new EnginePreset("七段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","150615"),
                }),
                new EnginePreset("六段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","76489"),
                }),
                new EnginePreset("五段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","38844"),
                }),
                new EnginePreset("四段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","19727"),
                }),
                new EnginePreset("三段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","10018"),
                }),
                new EnginePreset("二段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","5087"),
                }),
                new EnginePreset("初段" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","2583"),
                }),
                new EnginePreset("一級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","1841"),
                }),
                new EnginePreset("二級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","1312"),
                }),
                new EnginePreset("三級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","935"),
                }),
                new EnginePreset("四級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","666"),
                }),
                new EnginePreset("五級" , new EngineOption[] {
                        new EngineOption("Threads","4"),
                        new EngineOption("NodesLimit","474"),
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

            var default_extend = new List<ExtendedProtocol>( new[] { ExtendedProtocol.UseHashCommandExtension } );
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
                    SupportedExtendedProtocol = default_extend,
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
