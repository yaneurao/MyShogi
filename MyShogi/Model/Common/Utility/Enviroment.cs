using System;
using System.Management;
using MyShogi.Model.Dependency;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// 物理空きメモリなどを調べるクラス
    /// </summary>
    public static class Enviroment
    {
        /// <summary>
        /// 現在使用されていない利用可能な物理メモリのサイズ(kB)
        /// </summary>
        /// <returns></returns>
        public static ulong GetFreePhysicalMemory()
        {
            return API.GetFreePhysicalMemory();
        }

        /// <summary>
        /// 実行環境のスレッドの数
        /// HTオンだと、論理スレッド数、
        /// HTオフだと、物理スレッド数が返る。
        /// 
        /// AutoSettingでThreadを指定してある場合、現環境のスレッド数に制限されるべきなので
        /// そのためにこのメソッドが必要。
        /// </summary>
        /// <returns></returns>
        public static int GetProcessorCount()
        {
            // 10[us]以下で実行できるのでcacheしない。
            return Environment.ProcessorCount;
        }

        /// <summary>
        /// 実行環境の物理コア数。
        /// 
        /// AutoSettingでThreadを指定してある場合、現環境のスレッド数に制限されるべきなので
        /// そのためにこのメソッドが必要。
        /// </summary>
        /// <returns></returns>
        public static int GetProcessorCores()
        {
            if (processor_cores == 0)
                processor_cores = API.GetProcessorCores();

            return processor_cores;
        }

        /// <summary>
        /// 一度調べた物理コア数をcacheしておくための変数。
        /// </summary>
        private static int processor_cores;


    }
}
