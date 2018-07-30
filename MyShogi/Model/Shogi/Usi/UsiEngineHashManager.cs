using MyShogi.App;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.Player;
using System;

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
            EngineDefines = new EngineDefineEx[2];      /*  { null, null }    */
            EngineConfigs = new EngineConfig[2];        /*  { null, null }    */
            Ponders = new bool[2];                      /*  { false , false } */
            HashSize = new long[2];                     /*  { 0, 0 }          */
            Threads = new int[2];                       /*  { 0, 0 }          */
        }

        /// <summary>
        /// Hashサイズ , Threads の自動計算を行う。
        /// ・SetValue()で初期化されているものとする。
        /// これに基づいて、
        /// ・this.HashSize[2]にそれぞれのエンジンの適切なHashサイズを設定する。
        /// エンジンオプションに従う場合は、0が設定される。
        /// ・this.Threads[2]にそれぞれのエンジンの適切なThreads数を設定する。
        /// </summary>
        public void CalcValue()
        {
            // エンジンの数
            int numOfEngines = (EngineDefines[0] != null ? 1 : 0) + (EngineDefines[1] != null ? 1 : 0);

            // 両方エンジンではない。Hashの計算の必要がない。
            if (numOfEngines == 0)
                return;

            // engine optionとして渡すHashの値など。
            var autoHash = new bool[2];               // 自動Hashなのか
            var numOfAutoHash = 0;                    // AutoHashになっているEngineの数(0..2)
            var autoHashPercentage = new int[2];      // その時のHash割合[%]
            var hashValues = new long[2];             // そうでない時のHashの設定値[MB]
            var evalShare = new bool[2];              // EvalShareが有効か

            var engine_working_memory = new long[2];  // エンジンのWorking用のメモリ量[MB]
            var engine_eval_memory = new long[2];     // エンジンのEval用のメモリ量[MB]
            var engine_min_hash = new long[2];        // エンジンの最低Hash[MB]

            var min_total = (long)0;                  // 最低必要メモリの合計。(Working + Eval + Hash の先後分)

            var autoThread = new bool[2];             // 自動Threadなのか
            var threadsValues = new int[2];           // そうでない時のThreadsの設定値。

            foreach (var c in All.IntColors())
            {
                var config = EngineConfigs[c];
                if (config == null)
                    continue;
                var engineDefineEx = EngineDefines[c];
                var engineDefine = engineDefineEx.EngineDefine;

                // 共通設定
                var commonSetting = config.CommonOptions;
                // 個別設定
                var indSetting = config.IndivisualEnginesOptions.Find(x => x.FolderPath == engineDefineEx.FolderPath);

                // option名を指定して、エンジン共通設定・個別設定を反映させた結果のValueを得る関数。
                string GetOptionValue(string name) { return config.GetOptionValue(name, commonSetting, indSetting, null); };

                autoHash[c] = GetOptionValue("AutoHash_") == "true";
                autoHashPercentage[c] = int.Parse(GetOptionValue("AutoHashPercentage_"));
                hashValues[c] = long.Parse(GetOptionValue( "Hash_" ));
                evalShare[c] = GetOptionValue("EvalShare") == "true";

                autoThread[c] = GetOptionValue("AutoThread_") == "true";
                threadsValues[c] = int.Parse(GetOptionValue("Threads"));

                numOfAutoHash += autoHash[c] ? 1 : 0;

                // エンジンの使用するメモリ
                engine_eval_memory[c] = engineDefine.EvalMemory;
                engine_working_memory[c] = engineDefine.WorkingMemory;
                engine_min_hash[c] = engineDefine.MinimumHashMemory;

                // 必要最低メモリ
                min_total += engine_eval_memory[c] + engine_working_memory[c];
            }

            // 先後が同じエンジンでかつ、EvalShare対応で、かつEvalShare trueなら、その分、必要メモリ量は下がるはず。
            if (EngineDefines[0] == EngineDefines[1]
                && EngineDefines[0].EngineDefine.IsSupported(ExtendedProtocol.HasEvalShareOption)
                && evalShare[0] && evalShare[1]
                )
            {
                min_total -= engine_eval_memory[0]; // [0]も[1]も同じはず。同じエンジンなので…。
            }

            // 空き物理メモリ[MB]
            // 1) GetFreePhysicalMemory()はkBなので1024で割っている。
            // 2) GUIの動作のために100MBぐらい必要なので100MBほど差し引いてから計算している。
            // (メモリが足りなくなれば少しぐらいならOSが他のソフトをswapさせて空き物理メモリを確保してくれるとは思うが..)
            var physicalMemory = (long)Enviroment.GetFreePhysicalMemory() / 1024 - 100;

            // 人間側は、engine_eval_memory[c]などは 0になっているはずなので普通に計算していいはず…。

            // 片側だけAutoHashなら、そちらは後回しにして先に固定で確保して、残りメモリをAutoHashで割り当てる。
            foreach (var c in All.IntColors())
            {
                if (!autoHash[c])
                {
                    var hash = hashValues[c];
                    // hashValues[c]と、engine_min_hash[c]の大きいほうを選択したほうがいいような気は少しする。
                    // しかし、そうするとhash sizeを小さくして勝率を見るようなことが出来なくなるのでユーザーの選択を尊重する。

                    HashSize[c] = hash;
                    min_total += hash;
                }
            }

            // auto hash分を残り物理メモリから割り当てる。
            if (numOfAutoHash > 0)
                foreach(var c in All.IntColors())
                {
                    if (autoHash[c])
                    {
                        // 余る予定のメモリ量をエンジンの数で割って、それをAutoHashによって割り当てる。
                        // こうすることによって、片側のエンジンがAutoHashで(空き物理メモリの)70%、もう片側がAutoHashで30%を
                        // 要求するようなケースにおいてもうまく35%,15%のように按分される。
                        var rest = (physicalMemory - min_total) / numOfAutoHash;
                        
                        // エンジンの要求する最低メモリを割ることは出来ない。
                        var hash = Math.Max(rest * autoHashPercentage[c] / 100, engine_min_hash[c]);

                        HashSize[c] = hash;
                        min_total += hash;
                    }
                }

            string error = null;

            // エンジン要求する最小要求hash量を満たしているか。
            foreach (var c in All.IntColors())
                if (HashSize[c] < engine_min_hash[c])
                    error += $"{((Color)c).Pretty()}側のエンジンのハッシュが少なすぎます。少なくとも{engine_min_hash[c]}[MB]必要なところ、" +
                        $"{HashSize[c]}[MB]しか確保されません。思考エンジンの動作が不安定になる可能性があります。";

            // Hash割当が大きくて物理メモリを大量に割り込む時、警告を出す必要があると思う。
            // しかし連続対局でこれが毎回表示されるとうざい気はしなくもない。
            // とりあえず連続対局のことはあとで考える。まずは警告を出す。
            if (physicalMemory <= min_total)
                error += $"エンジンの動作のために、物理メモリが{min_total - physicalMemory}[MB] 足りません。"
                    + "空き物理メモリが足りないので思考エンジンの動作が不安定になる可能性があります。";

            // スレッド数の自動マネージメントに関して

            // 1) AutoThread_なら基本的には、OSが返すコアの数にする。
            // 2) 2つのエンジンがそれぞれPonderありなら、スレッド数を2で割るべき。

            var os_threads = Enviroment.GetProcessorCount(); // 1)
            var ponder = Ponders[0] && Ponders[1] ? 2 : 1;   // 2)

            foreach (var c in All.IntColors())
            {
                Threads[c] = autoThread[c] ? (os_threads / ponder) : threadsValues[c];

                // エンジンがあるのにスレッド数が0に設定されている場合、警告ぐらい出すべきでは…。
                // 通常、ThreadsはMinValueが1なので、GUI上で1以上しか設定できないから、0が設定されていることはないはずだが、
                // エンジン側がMinValueの設定を忘れている可能性もあるので…。
                if (EngineDefines[c] != null && Threads[c] == 0)
                    error += $"{((Color)c).Pretty()}側の思考エンジンのスレッド数が0になっています。(エンジンオプションのThreadsの設定を見直してください。)";
            }

            if (error != null)
                TheApp.app.MessageShow(error);

            // ここで計算された先後の最終的なHashSize、Thread数などをログに出力しておく。
            Log.Write(LogInfoType.UsiServer, $"自動Hash = {{ {HashSize[0]} [MB] , {HashSize[1]} [MB] }} ," +
                $" 自動Threads = {{ { Threads[0] } , { Threads[1] } }}");
        }

        /// <summary>
        /// 各メンバーの値を設定する。CalcValue()の前に呼び出して設定しないといけない。
        /// </summary>
        public void SetValue(Color c , EngineDefineEx engineDefineEx , EngineConfig config , bool ponder)
        {
            EngineDefines[(int)c] = engineDefineEx;
            EngineConfigs[(int)c] = config;
            Ponders[(int)c] = ponder;
        }

        /// <summary>
        /// それぞれのEngineに設定するHash値。0の時はエンジンオプションに従う。
        /// CalcValue()によって計算される。
        /// </summary>
        public long[] HashSize;

        /// <summary>
        /// それぞれのEngineに設定するThreads値。
        /// CalcValue()によって計算される。
        /// </summary>
        public int[] Threads;

        // -- privates

        /// <summary>
        /// EngineDefineExの先後分。
        /// </summary>
        private EngineDefineEx[] EngineDefines;

        /// <summary>
        /// EngineConfigの先後分。
        /// </summary>
        private EngineConfig[] EngineConfigs;

        /// <summary>
        /// Ponderが有効なのか。先後分。
        /// </summary>
        private bool[] Ponders;

    }
}
