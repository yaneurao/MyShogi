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
            var auto_setting_default = new [] {

                // -- 棋力制限なし
                new EngineAutoSetting("将棋神" , null ) ,

                // -- 段位が指定されている場合は、NodesLimitで調整する。

                // スレッド数で棋力が多少変わる。4スレッドで計測したのでこれでいく。
                // 実行環境の論理スレッド数がこれより少ない場合は、自動的にその数に制限される。

                // ここの段位は、持ち時間15分切れ負けぐらいの時の棋力。

                new EngineAutoSetting("九段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","200000"),
                }),
                new EngineAutoSetting("八段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","100000"),
                }),
                new EngineAutoSetting("七段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","80000"),
                }),
                new EngineAutoSetting("六段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","40000"),
                }),
                new EngineAutoSetting("五段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","20000"),
                }),
                new EngineAutoSetting("四段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","10000"),
                }),
                new EngineAutoSetting("三段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","5000"),
                }),
                new EngineAutoSetting("二段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","3000"),
                }),
                new EngineAutoSetting("初段" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","500"),
                }),
                new EngineAutoSetting("一級" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","400"),
                }),
                new EngineAutoSetting("二級" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","300"),
                }),
                new EngineAutoSetting("三級" , new EngineOption[] {
                        new EngineOption("Thread","4"),
                        new EngineOption("NodesLimit","200"),
                }),
            };

            // -- 各エンジン用の設定ファイルを生成して書き出す。

            {
                // やねうら王
                var engine_define = new EngineDefine()
                {
                    DisplayName = "やねうら王",
                    EngineExeName = "yaneuraou2018_kpp_kkpt",
                    SupportedCpus = new [] { Cpu.NO_SSE, Cpu.SSE2, Cpu.SSE41, Cpu.SSE42, Cpu.AVX2 },
                    RequiredMemory = 512, // KPP_KKPTは、これくらい？
                    AutoSettings = auto_setting_default,
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
                    EngineExeName = "yaneuraou2018_kppt",
                    SupportedCpus = new[] { Cpu.NO_SSE, Cpu.SSE2, Cpu.SSE41, Cpu.SSE42, Cpu.AVX2 },
                    RequiredMemory = 1024, // KPPTは、これくらい？
                    AutoSettings = auto_setting_default,
                };
                EngineDefineUtility.WriteFile("engine/tanuki_sdt5/engine_define.xml", engine_define);
            }

            {
                // tanuki2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "tanuki- 2018",
                    EngineExeName = "yaneuraou2018_nuee",
                    SupportedCpus = new[] { Cpu.NO_SSE, Cpu.SSE2, Cpu.SSE41, Cpu.SSE42, Cpu.AVX2 },
                    RequiredMemory = 512, // NNUEは、これくらい？
                    AutoSettings = auto_setting_default,
                };
                EngineDefineUtility.WriteFile("engine/tanuki2018/engine_define.xml", engine_define);
            }

            {
                // qhapaq2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "Qhapaq 2018",
                    EngineExeName = "yaneuraou2018_kppt",
                    SupportedCpus = new[] { Cpu.NO_SSE, Cpu.SSE2, Cpu.SSE41, Cpu.SSE42, Cpu.AVX2 },
                    RequiredMemory = 1024, // KPPTはこれくらい？
                    AutoSettings = auto_setting_default,
                };
                EngineDefineUtility.WriteFile("engine/qhapaq2018/engine_define.xml", engine_define);
            }

            {
                // yomita2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "読み太 2018",
                    EngineExeName = "yaneuraou2018_kppt",
                    SupportedCpus = new[] { Cpu.NO_SSE, Cpu.SSE2, Cpu.SSE41, Cpu.SSE42, Cpu.AVX2 },
                    RequiredMemory = 1024, // KPPTはこれくらい？
                    AutoSettings = auto_setting_default,
                };
                EngineDefineUtility.WriteFile("engine/yomita2018/engine_define.xml", engine_define);
            }

            {
                // gpsfish(動作テスト用) 『将棋神　やねうら王』には含めない。
                var engine_define = new EngineDefine()
                {
                    DisplayName = "gpsfish",
                    EngineExeName = "gpsfish",
                    SupportedCpus = new[] { Cpu.SSE2 },
                    RequiredMemory = 10, // gpsfishこれくらいで動くような？
                    AutoSettings = auto_setting_default,
                };
                EngineDefineUtility.WriteFile("engine/gpsfish/engine_define.xml", engine_define);

                //Console.WriteLine(EngineDefineUtility.EngineExeFileName(engine_define));
            }
        }
    }
}
