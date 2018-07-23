using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.Player;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// UsiEngineのHashの自動マネージメントとスレッドの自動マネージメントを行う。
    /// </summary>
    public class UsiEngineHashManager
    {
        public UsiEngineHashManager()
        {
            Init();
        }

        /// <summary>
        /// 初期化する。
        /// </summary>
        public void Init()
        {
            Players = new UsiEnginePlayer[2];     /*  { null, null } */
            EngineDefines = new EngineDefineEx[2];/*  { null, null } */
            EngineConfigs = new EngineConfig[2];  /*  { null, null } */
            HashSize = new long[2];               /*  { 0, 0} */
        }

        /// <summary>
        /// Hashサイズの自動計算を行う。
        /// ・this.EngineDefines[2] にエンジンの設定が入っている。(ものとする)
        /// ・this.EngineConfigs[2] にエンジン共通設定が入っている。(ものとする)
        /// これらに基づいて、
        /// this.HashSize[2]にそれぞれのエンジンの適切なHashサイズを設定する。
        /// エンジンオプションに従う場合は、0が設定される。
        /// </summary>
        public void CalcHashSize()
        {
            // エンジンの数
            int numOfEngines = (EngineDefines[0] != null ? 1 : 0) + (EngineDefines[1] != null ? 1 : 0);

            // 両方エンジンではない。Hashの計算の必要がない。
            if (numOfEngines == 0)
                return;

            // engine optionとして渡すHashの値など。
            var autoHash = new bool[2];           // 自動Hashなのか
            var autoHashPercentage = new int[2];  // その時のHash割合
            var hashValues = new string[2];       // そうでない時のHashの設定値
            var engine_req_memory = new long[2];  // エンジンの必要メモリ
            var engine_min_hash = new long[2];    // エンジンの最低Hash
            var min_total = (long)0;              // 最低必要メモリの合計。

            foreach (var c_ in All.Colors())
            {
                int c = (int)c_;

                var config = EngineConfigs[c];
                if (config == null)
                    continue;
                var engineDefineEx = EngineDefines[c];
                var engineDefine = engineDefineEx.EngineDefine;

                // 共通設定
                var commonSetting = config.CommonOptions;
                // 個別設定
                var indSetting = config.IndivisualEnginesOptions.Find(x => x.FolderPath == engineDefineEx.FolderPath);

                autoHash[c] = config.GetDefault("AutoHash_", commonSetting, indSetting, null) == "true";
                autoHashPercentage[c] = int.Parse(config.GetDefault("AutoHashPercentage_", commonSetting, indSetting, null));
                hashValues[c] = config.GetDefault( "Hash_" , commonSetting, indSetting , null);

                engine_req_memory[c] = engineDefine.RequiredMemory;
                engine_min_hash[c] = engineDefine.MinimumHashMemory;

                // 必要最低メモリ
                min_total += engine_req_memory[c];
                min_total += engine_min_hash[c];
            }

            // 空き物理メモリ[MB]
            var physicalMemory = (long)Enviroment.GetFreePhysicalMemory();

            // 人間側は、engine_req_memory[c] == 0になっているはずなので普通に計算していいはず…。

            // 必要最低メモリのほうが空き物理メモリ以上なので、この最低値に設定しておく。
            if (min_total >= physicalMemory)
            {
                foreach (var c in All.Colors())
                    HashSize[(int)c] = engine_min_hash[(int)c];
                goto SetThread;
            }



            // スレッド数の自動マネージメントに関して
        SetThread:;

        }

        /// <summary>
        /// UsiEnginePlayerの先後分。
        /// </summary>
        public UsiEnginePlayer[] Players;

        /// <summary>
        /// EngineDefineExの先後分。
        /// </summary>
        public EngineDefineEx[] EngineDefines;

        /// <summary>
        /// EngineConfigの先後分。
        /// </summary>
        public EngineConfig[] EngineConfigs;

        /// <summary>
        /// それぞれのEngineに設定するHash値。0の時はエンジンオプションに従う。
        /// </summary>
        public long[] HashSize;
    }
}
