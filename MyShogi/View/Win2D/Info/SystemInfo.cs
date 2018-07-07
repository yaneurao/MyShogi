using System;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    public partial class SystemInfo : Form
    {
        public SystemInfo()
        {
            InitializeComponent();

            CpuInfo();
        }

        /// <summary>
        ///  CPU情報を調べてTextBox1に設定する。
        /// </summary>
        private void CpuInfo()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var sb = new StringBuilder();

            sb.AppendLine($"Environment.Is64BitOperatingSystem: {Environment.Is64BitOperatingSystem}");
            sb.AppendLine($"Environment.Is64BitProcess: {Environment.Is64BitProcess}");
            sb.AppendLine($"Environment.ProcessorCount: {Environment.ProcessorCount}");

            var cpuid = Model.Common.Utility.CpuId.flags;

#if false
            for (UInt32 i = 0; i < cpuid.basicLength; ++i)
                for (UInt32 j = 0; j < 4; ++j)
                {
                    Console.WriteLine($"{(i):X8}{(char)(j + 'a')}: {cpuid.getBasic(i, j):X8}");
                }
            for (UInt32 i = 0; i < cpuid.extendLength; ++i)
                for (UInt32 j = 0; j < 4; ++j)
                {
                    Console.WriteLine($"{(i | 0x80000000):X8}{(char)(j + 'a')}: {cpuid.getExtend(i, j):X8}");
                }
#endif

            sb.AppendLine($"processorArchitecture: {cpuid.processorArchitecture}");
            sb.AppendLine($"cpuTarget: {cpuid.cpuTarget}");
            sb.AppendLine($"vendorId: {cpuid.vendorId}");
            sb.AppendLine($"brand: {cpuid.brand}");
            sb.AppendLine($"hasSSE2: {cpuid.hasSSE2}");
            sb.AppendLine($"hasSSE41: {cpuid.hasSSE41}");
            sb.AppendLine($"hasSSE42: {cpuid.hasSSE42}");
            sb.AppendLine($"hasAVX2: {cpuid.hasAVX2}");
            sb.AppendLine($"hasAVX512F: {cpuid.hasAVX512F}");

            using (ManagementClass mc = new ManagementClass("Win32_OperatingSystem"))
            using (ManagementObjectCollection moc = mc.GetInstances())
                foreach (ManagementObject mo in moc)
                {
                    foreach (string key in new[] {
                        // OSに利用可能な物理メモリのサイズ(kB)
                        "TotalVisibleMemorySize",
                        // 現在使用されていない利用可能な物理メモリのサイズ(kB)
                        "FreePhysicalMemory",
                        // 仮想メモリのサイズ(kB)
                        "TotalVirtualMemorySize",
                        // 現在使用されていない利用可能な仮想メモリのサイズ(kB)
                        "FreeVirtualMemory",
                        // ほかのページをスワップアウトすることなくOSのページングファイルにマップできるサイズ(kB)
                        "FreeSpaceInPagingFiles",
                        // OSのページングファイルで格納されるサイズ(kB)
                        "SizeStoredInPagingFiles",
                    })
                    {
                        sb.AppendLine($"{key}: {mo[key]:N0}kB");
                    }
                    mo.Dispose();
                }

            stopwatch.Stop();
            sb.AppendLine();
            sb.AppendLine($"計測に要した時間 { stopwatch.ElapsedMilliseconds } [ms]");
            // 計測に要した時間 245 [ms]

            textBox1.Text = sb.ToString();
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = 0;
        }
    }
}
