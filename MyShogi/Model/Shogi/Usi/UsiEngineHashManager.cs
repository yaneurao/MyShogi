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
            var autoHash = new bool[2];               // 自動Hashなのか
            var numOfAutoHash = 0;                    // AutoHashになっているEngineの数(0..2)
            var autoHashPercentage = new int[2];      // その時のHash割合[%]
            var hashValues = new long[2];             // そうでない時のHashの設定値[MB]
            var evalShare = new bool[2];              // EvalShareが有効か

            var engine_working_memory = new long[2];  // エンジンのWorking用のメモリ量[MB]
            var engine_eval_memory = new long[2];     // エンジンのEval用のメモリ量[MB]
            var engine_min_hash = new long[2];        // エンジンの最低Hash[MB]

            var min_total = (long)0;                  // 最低必要メモリの合計。(Working + Eval + Hash の先後分)

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
                hashValues[c] = int.Parse(GetOptionValue( "Hash_" ));
                evalShare[c] = GetOptionValue("EvalShare") == "true";

                numOfAutoHash += autoHash[c] ? 1 : 0;

                // エンジンの使用するメモリ
                engine_eval_memory[c] = engineDefine.EvalMemory;
                engine_working_memory[c] = engineDefine.WorkingMemory;
                engine_min_hash[c] = engineDefine.MinimumHashMemory;

                // 必要最低メモリ
                min_total += engine_eval_memory[c] + engine_working_memory[c] + engine_min_hash[c];
            }

            // 先後が同じエンジンでかつ、EvalShareしているなら、その分、必要メモリ量は下がるはず。
            if (EngineDefines[0] == EngineDefines[1] && evalShare[0] && evalShare[1] )
            {
                min_total -= engine_eval_memory[0]; // [0]も[1]も同じはず。同じエンジンなので…。
            }

            // 空き物理メモリ[MB]
            var physicalMemory = (long)Enviroment.GetFreePhysicalMemory() / 1024;

            // 人間側は、engine_req_memory[c] == 0になっているはずなので普通に計算していいはず…。

            // 必要最低メモリのほうが空き物理メモリ以上なので、この最低値に設定しておく。
            // 固定で確保すると足りなくなるので仕方がない。
            if (min_total >= physicalMemory)
            {
                foreach (var c in All.IntColors())
                {
                    var hash = engine_min_hash[c];
                    HashSize[c] = hash;
                    min_total += hash;
                }
                goto EndCalcHash;
            }

            // totalからhash分を除いて、再度割当てする。
            min_total -= (engine_min_hash[0] + engine_min_hash[1]);

            // 片側だけAutoHashなら、そちらは後回しにして先に固定で確保して、残りメモリをAutoHashで割り当てる。
            foreach (var c in All.IntColors())
            {
                if (!autoHash[c])
                {
                    var hash = hashValues[c];
                    // hashValues[c]と、engine_min_hash[c]の大きいほうを選択したほうがいいような気は少しする。
                    // しかし、そうするとhash sizeを小さくして勝率を見るようなことが出来なくなるのでユーザーの選択を尊重する。

                    min_total += hash;
                    HashSize[c] = hash;
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

            EndCalcHash:;
            // Hash割当が大きくて物理メモリを大量に割り込む時、警告を出す必要があると思う。
            // しかし連続対局でこれが毎回表示されるとうざい気はしなくもない。
            // とりあえず連続対局のことはあとで考える。まずは警告を出す。
            if (physicalMemory <= min_total)
                TheApp.app.MessageShow("空き物理メモリが足りないので動作が不安定になる可能性があります。" +
                    $"エンジンの動作のために、物理メモリが{min_total - physicalMemory}[MB] 足りません。");

            // スレッド数の自動マネージメントに関して


            // ここで計算された先後の最終的なHashSize、Thread数などをログに出力しておく。
            Log.Write(LogInfoType.UsiServer, $"自動Hash = {{ {HashSize[0]} [MB] , {HashSize[1]} [MB] }}");
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
