using System;
using MyShogi.Model.Dependent;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// 思考エンジンが対応しているCPU
    /// </summary>
    public enum CpuType : int
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

    public static class CpuTypeExtensions
    {
        /// <summary>
        /// 実行ファイルのファイル名の末尾につけるsuffixを返す。
        /// 
        /// 例) AVX2用なら、"engine_avx2"のようになるので"avx2"を返す。
        /// </summary>
        /// <param name="cpu"></param>
        /// <returns></returns>
        public static string ToSuffix(this CpuType cpu)
        {
            switch (cpu)
            {
                case CpuType.UNKNOWN : return "unknown";
                case CpuType.NO_SSE  : return "nosse";
                case CpuType.SSE2    : return "sse2";
                case CpuType.SSE41   : return "sse41";
                case CpuType.SSE42   : return "sse42";
                case CpuType.AVX     : return "avx";
                case CpuType.AVX2    : return "avx2";
                case CpuType.AVX512  : return "avx512";
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
        public static CpuType GetCurrentCpu()
        {
            CpuType c = cpu;

            // 一度調べたのであれば保存してあるのでそれを返す。
            if (c != CpuType.UNKNOWN)
                return c;

            // 64bit環境でなければ無条件でNO_SSEとして扱う
            if (!Environment.Is64BitOperatingSystem)
                c = CpuType.NO_SSE;
            else
                // 64bit環境である。
                // 思考エンジンは別プロセスで動作させるので、このプロセスが32bitであっても問題ない。
                c = API.GetCurrentCpu();

            cpu = c; // 調べた結果を保存しておく。
            return c;
        }

        /// <summary>
        /// 一度調べたら保存しておく。
        /// </summary>
        private static CpuType cpu;
    }
}
