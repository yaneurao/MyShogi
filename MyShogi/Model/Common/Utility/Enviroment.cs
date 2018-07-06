using System;
using System.Management;

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
            ulong result = 0;
            try
            {
                using (var mc = new ManagementClass("Win32_OperatingSystem"))
                using (var moc = mc.GetInstances())
                    foreach (var mo in moc) // 何故か複数あることが想定されている。NUMA環境か？
                    {
                        result = (ulong) mo["FreePhysicalMemory"];

                        mo.Dispose(); // これ要るのか？
                    }
            }
            catch { }

            return result;
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
    }
}
