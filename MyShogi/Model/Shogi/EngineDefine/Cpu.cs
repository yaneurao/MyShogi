using System;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// 思考エンジンが対応しているCPU
    /// </summary>
    public enum Cpu : int
    {
        UNKNOWN, // 不明。未確定の時の値。

        NO_SSE, // 32bit版 SSEなし
        SSE2,   // 64bit版 SSE2
        SSE41,  // 64bit版 SSE4.1
        SSE42,  // 64bit版 SSE4.2
        AVX,    // 64bit版 AVX(使わないが…)
        AVX2,   // 64bit版 AVX2
        AVX512, // 64bit版 AVX512
    }

    public static class CpuExtensions
    {
        /// <summary>
        /// 実行ファイルのファイル名の末尾につけるsuffixを返す。
        /// 
        /// 例) AVX2用なら、"engine_avx2"のようになるので"avx2"を返す。
        /// </summary>
        /// <param name="cpu"></param>
        /// <returns></returns>
        public static string ToSuffix(this Cpu cpu)
        {
            switch (cpu)
            {
                case Cpu.UNKNOWN : return "unknown";
                case Cpu.NO_SSE  : return "no_sse";
                case Cpu.SSE2    : return "sse2";
                case Cpu.SSE41   : return "sse41";
                case Cpu.SSE42   : return "sse42";
                case Cpu.AVX     : return "avx";
                case Cpu.AVX2    : return "avx2";
                case Cpu.AVX512  : return "avx512";
                default:
                    throw new Exception("未知のCPU");
            }
        }
    }

    public static class CpuUtil
    {
        /// <summary>
        /// 現在の環境のCPU
        /// </summary>
        /// <returns></returns>
        public static Cpu GetCurrentCpu()
        {
            Cpu c = cpu;

            // 一度調べたのであれば保存してあるのでそれを返す。
            if (c != Cpu.UNKNOWN)
                return c;

            // 64bit環境でなければ無条件でNO_SSE
            if (!Environment.Is64BitOperatingSystem)
                c = Cpu.NO_SSE;
            else
            {
                // 64bit環境である。
                // 思考エンジンは別プロセスで動作させるので、このプロセスが32bitであっても問題ない。

                var cpuid = Model.Common.Utility.CpuId.flags;

                if (cpuid.hasAVX512F)
                    c = Cpu.AVX512;
                else if (cpuid.hasAVX2)
                    c = Cpu.AVX2;
                else if (cpuid.hasAVX)
                    c = Cpu.AVX;
                else if (cpuid.hasSSE42)
                    c = Cpu.SSE42;
                else if (cpuid.hasSSE41)
                    c = Cpu.SSE41;
                else if (cpuid.hasSSE2)
                    c = Cpu.SSE2;
                else
                    // そんな阿呆な…。
                    throw new Exception("CPU判別に失敗しました。");
            }

            cpu = c; // 調べた結果を保存しておく。
            return c;
        }

        /// <summary>
        /// 一度調べたら保存しておく。
        /// </summary>
        private static Cpu cpu;
    }
}
