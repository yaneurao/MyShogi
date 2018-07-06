using MyShogi.Model.Common.Utility;
using System;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// EngineDefineのユーティリティ
    /// </summary>
    public static class EngineDefineUtility
    {
        /// <summary>
        /// "engine_define.xml"というファイルを読み込んで、デシリアライズする。
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static EngineDefine ReadFile(string filepath)
        {
            var def = Serializer.Deserialize<EngineDefine>(filepath);

            // 読み込めなかったので新規に作成する。
            if (def == null)
                def = new EngineDefine();

            return def;
        }

        /// <summary>
        /// ファイルにEngineDefineをシリアライズして書き出す。
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="engine_define"></param>
        public static void WriteFile(string filepath, EngineDefine engine_define)
        {
            Serializer.Serialize(filepath, engine_define);
        }

        /// <summary>
        /// 現在の環境で動作するエンジンのファイル名を決定する。
        /// </summary>
        /// <param name="engine_define"></param>
        /// <returns></returns>
        public static string EngineExeFileName(this EngineDefine engine_define)
        {
            // 現在の環境を確定させる。
            var current_cpu = CpuUtil.GetCurrentCpu();

            // サポートしている実行ファイルのなかで、一番いいものにする。
            var cpu = Cpu.UNKNOWN;
            foreach (var c in engine_define.SupportedCpus)
                if (c <= current_cpu /* 現在のCPUで動作する*/ && cpu < c /* 一番ええやつ */)
                    cpu = c;

            return $"{ engine_define.EngineExeName }_{ cpu.ToSuffix()}.exe";
        }

    }
}
