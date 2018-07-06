namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// 『将棋神　やねうら王』の5つのエンジンの"engine_define.xml"を書き出すサンプル
    /// </summary>
    public static class EngineDefine2018
    {
        /// <summary>
        /// 『将棋神　やねうら王』の5つのエンジンの"engine_define.xml"を書き出す。
        /// engine/フォルダ配下の各フォルダに書き出す。
        /// </summary>
        public static void WriteFiles()
        {
            {
                // やねうら王
                var engine_define = new EngineDefine()
                {
                    DisplayName = "やねうら王",
                    RequiredMemory = 512 , // KPP_KKPTは、これくらい？
                };
                EngineDefineUtility.WriteFile("engine/yaneuraou2018/engine_define.xml", engine_define);
            }

            {
                // tanuki_sdt5
                var engine_define = new EngineDefine()
                {
                    DisplayName = "tanuki- SDT5",
                    RequiredMemory = 1024, // KPPTは、これくらい？
                };
                EngineDefineUtility.WriteFile("engine/tanuki_sdt5/engine_define.xml", engine_define);
            }

            {
                // tanuki2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "tanuki- 2018",
                    RequiredMemory = 512, // NNUEは、これくらい？
                };
                EngineDefineUtility.WriteFile("engine/tanuki2018/engine_define.xml", engine_define);
            }

            {
                // qhapaq2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "Qhapaq 2018",
                    RequiredMemory = 1024, // KPPTはこれくらい？
                };
                EngineDefineUtility.WriteFile("engine/qhapaq2018/engine_define.xml", engine_define);
            }

            {
                // yomita2018
                var engine_define = new EngineDefine()
                {
                    DisplayName = "読み太 2018",
                };
                EngineDefineUtility.WriteFile("engine/yomita2018/engine_define.xml", engine_define);
            }
        }
    }
}
