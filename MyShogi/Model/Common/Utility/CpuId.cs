using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MyShogi.Model.Common.Utility
{
    public static class CpuId
    // "x86/x64 CPUID in C#" based on https://stackoverflow.com/a/8390726
    // license terms CC-BY-SA:
    //   https://stackoverflow.com/legal/terms-of-service/public#licensing
    //   https://creativecommons.org/licenses/by-sa/4.0/
    {
        public static byte[] Invoke(uint leaf, uint subleaf = 0)
        {
            IntPtr codePointer = IntPtr.Zero;
            try
            {
                // compile
                byte[] codeBytes = Environment.Is64BitProcess ? x64CodeBytes : x86CodeBytes;

                codePointer = VirtualAlloc(
                    IntPtr.Zero,
                    new UIntPtr((UInt32)codeBytes.Length),
                    AllocationType.COMMIT | AllocationType.RESERVE,
                    MemoryProtection.EXECUTE_READWRITE
                );

                Marshal.Copy(codeBytes, 0, codePointer, codeBytes.Length);

                CpuIDDelegate cpuIdDelg = (CpuIDDelegate)Marshal.GetDelegateForFunctionPointer(codePointer, typeof(CpuIDDelegate));

                // invoke
                GCHandle handle = default(GCHandle);
                var buffer = new byte[16];

                try
                {
                    handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    cpuIdDelg(subleaf, leaf, buffer);
                }
                finally
                {
                    if (handle != default(GCHandle))
                    {
                        handle.Free();
                    }
                }

                return buffer;
            }
            finally
            {
                if (codePointer != IntPtr.Zero)
                {
                    VirtualFree(codePointer, 0, 0x8000);
                    codePointer = IntPtr.Zero;
                }
            }
        }

        public class Registers
        {
            public UInt32[] regs;
            public byte[] bytes;
            public Registers(byte[] _bytes)
            {
                bytes = (byte[])(_bytes.Clone());
                regs = new[]
                {
                    BitConverter.ToUInt32(bytes, 0),
                    BitConverter.ToUInt32(bytes, 4),
                    BitConverter.ToUInt32(bytes, 8),
                    BitConverter.ToUInt32(bytes, 12),
                };
            }
            public UInt32 eax { get => regs[0]; }
            public UInt32 ebx { get => regs[1]; }
            public UInt32 ecx { get => regs[2]; }
            public UInt32 edx { get => regs[3]; }
        }

        public enum CpuTarget
        {
            INTEL_x86_NOSSE,
            INTEL_x86_SSE2,
            INTEL_x86_SSE42,
            INTEL_x64_SSE2,
            INTEL_x64_SSE42,
            INTEL_x64_AVX2,
            INTEL_x64_AVX512F,
            ARM_32,
            ARM_64,
            IA64,
            UNKNOWN,
        }
        public static Flags flags { get => Flags.flags; }
        public class Flags
        {
            public static Flags flags { get; } = new Flags();
            private SystemInfo systeminfo;
            private Registers[] basic;
            private Registers[] extend;
            public Flags()
            {
                systeminfo = new SystemInfo();
                GetSystemInfo(ref systeminfo);

                switch (systeminfo.ProcessorArchitecture)
                {
                    case ProcessorArchitecture.INTEL:
                    case ProcessorArchitecture.AMD64:
                        byte[] b0 = Invoke(0);
                        UInt32 basicLength = System.Math.Min(BitConverter.ToUInt32(b0, 0), 255);
                        basic = new Registers[basicLength + 1];
                        basic[0] = new Registers(b0);
                        for (UInt32 i = 1; i <= basicLength; ++i) basic[i] = new Registers(Invoke(i));

                        byte[] e0 = Invoke(0x80000000);
                        UInt32 extendLength = BitConverter.ToUInt32(e0, 0);
                        extendLength = extendLength < 0x80000000 ? 0 : System.Math.Min(extendLength & 0x7fffffff, 255);
                        extend = new Registers[extendLength + 1];
                        extend[0] = new Registers(e0);
                        for (UInt32 i = 1; i <= extendLength; ++i) extend[i] = new Registers(Invoke(i | 0x80000000));
                    break;
                    default:
                        basic = new Registers[0];
                        extend = new Registers[0];
                        break;
                }
            }
            public UInt32 getBasic(UInt32 id, UInt32 subid) => (id >= basic.Length || subid >= 4) ? 0 : basic[id].regs[subid];
            public UInt32 getExtend(UInt32 id, UInt32 subid) => (id >= extend.Length || subid >= 4) ? 0 : extend[id].regs[subid];
            public bool getBasicBit(UInt32 id, UInt32 subid, UInt32 bitid) => (bitid >= 32) ? false : ((getBasic(id, subid) & (1U << (int)bitid)) != 0);
            public bool getExtendBit(UInt32 id, UInt32 subid, UInt32 bitid) => (bitid >= 32) ? false : ((getExtend(id, subid) & (1U << (int)bitid)) != 0);
            public byte getBasicByte(UInt32 id, UInt32 byteid) => (id >= basic.Length || byteid >= 16) ? (byte)0 : basic[id].bytes[byteid];
            public byte getExtendByte(UInt32 id, UInt32 byteid) => (id >= extend.Length || byteid >= 16) ? (byte)0 : extend[id].bytes[byteid];
            public CpuTarget cpuTarget
            {
                get
                {
                    switch (systeminfo.ProcessorArchitecture)
                    {
                        case ProcessorArchitecture.AMD64:
                        case ProcessorArchitecture.INTEL:
                            if (Environment.Is64BitOperatingSystem)
                            {
                                if (hasAVX512F) return CpuTarget.INTEL_x64_AVX512F;
                                if (hasAVX2) return CpuTarget.INTEL_x64_AVX2;
                                if (hasSSE42) return CpuTarget.INTEL_x64_SSE42;
                                return CpuTarget.INTEL_x64_SSE2;
                            }
                            else
                            {
                                if (hasSSE42) return CpuTarget.INTEL_x86_SSE42;
                                if (hasSSE2) return CpuTarget.INTEL_x86_SSE2;
                                return CpuTarget.INTEL_x86_NOSSE;
                            }
                        case ProcessorArchitecture.ARM64:
                            if (Environment.Is64BitOperatingSystem)
                                return CpuTarget.ARM_64;
                            return CpuTarget.ARM_32;
                        case ProcessorArchitecture.ARM:
                            return CpuTarget.ARM_32;
                        case ProcessorArchitecture.IA64:
                            return CpuTarget.IA64;
                        default:
                            return CpuTarget.UNKNOWN;
                    }
                }
            }
            public int basicLength { get => basic.Length; }
            public int extendLength { get => extend.Length; }
            public bool hasMMX { get => getBasicBit(1, 3, 23); }
            public bool hasSSE { get => getBasicBit(1, 3, 25); }
            public bool hasSSE2 { get => getBasicBit(1, 3, 26); }
            public bool hasSSE41 { get => getBasicBit(1, 2, 19); }
            public bool hasSSE42 { get => getBasicBit(1, 2, 20); }
            public bool hasPOPCNT { get => getBasicBit(1, 2, 23); }
            public bool hasAVX { get => getBasicBit(1, 2, 28); }
            public bool hasFMA { get => getBasicBit(1, 2, 12); }
            public bool hasAVX2 { get => getBasicBit(7, 1, 5); }
            public bool hasBMI1 { get => getBasicBit(7, 1, 3); }
            public bool hasBMI2 { get => getBasicBit(7, 1, 8); }
            public bool hasMOVBE { get => getBasicBit(1, 2, 22); }
            public bool hasRTM { get => getBasicBit(7, 1, 11); }
            public bool hasHLE { get => getBasicBit(7, 1, 4); }
            public bool hasAVX512F { get => getBasicBit(7, 1, 16); }
            public bool hasAVX512DQ { get => getBasicBit(7, 1, 17); }
            public bool hasAVX512IFMA52 { get => getBasicBit(7, 1, 21); }
            public bool hasAVX512PF { get => getBasicBit(7, 1, 26); }
            public bool hasAVX512ER { get => getBasicBit(7, 1, 27); }
            public bool hasAVX512CD { get => getBasicBit(7, 1, 28); }
            public bool hasAVX512BW { get => getBasicBit(7, 1, 30); }
            public bool hasAVX512VL { get => getBasicBit(7, 1, 31); }
            public bool hasAVX512VBMI { get => getBasicBit(7, 2, 1); }
            public string vendorId
            {
                get
                {
                    if (basic.Length < 1) return string.Empty;
                    byte[] venderBytes = new byte[12];
                    Array.Copy(basic[0].bytes,  4, venderBytes, 0, 4);
                    Array.Copy(basic[0].bytes, 12, venderBytes, 4, 4);
                    Array.Copy(basic[0].bytes,  8, venderBytes, 8, 4);
                    return Encoding.ASCII.GetString(venderBytes).Trim('\0').TrimStart(' ').TrimEnd(' ');
                }
            }
            public string brand
            {
                get
                {
                    if (extend.Length < 5) return string.Empty;
                    byte[] brandBytes = new byte[48];
                    extend[2].bytes.CopyTo(brandBytes, 0);
                    extend[3].bytes.CopyTo(brandBytes, 16);
                    extend[4].bytes.CopyTo(brandBytes, 32);
                    return Encoding.ASCII.GetString(brandBytes).Trim('\0').TrimStart(' ').TrimEnd(' ');
                }
            }
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CpuIDDelegate(uint subleaf, uint leaf, byte[] buffer);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32")]
        private static extern bool VirtualFree(IntPtr lpAddress, UInt32 dwSize, UInt32 dwFreeType);

        [DllImport("kernel32")]
        private static extern void GetSystemInfo(ref SystemInfo psi);

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        internal struct SystemInfo
        {
            public ProcessorArchitecture ProcessorArchitecture;
            UInt16 Reserved;
            public UInt32 PageSize;
            public IntPtr MinimumApplicationAddress;
            public IntPtr MaximumApplicationAddress;
            public IntPtr ActiveProcessorMask;
            public UInt32 NumberOfProcessors;
            public ProcessorType ProcessorType;
            public UInt32 AllocationGranularity;
            public UInt16 ProcessorLevel;
            public UInt16 ProcessorRevision;
        }

        internal enum ProcessorArchitecture : UInt16
        {
            AMD64 = 9, // x64
            ARM = 5,
            ARM64 = 12,
            IA64 = 6,
            INTEL = 0, // x86
            UNKNOWN = 0xffff,
        }

        internal enum ProcessorType : UInt32
        {
            INTEL_386 = 386,
            INTEL_486 = 486,
            INTEL_PENTIUM = 586,
            INTEL_IA64 = 2200,
            AMD_X8664 = 8664,
        }

        [Flags]
        private enum AllocationType : UInt32
        {
            COMMIT = 0x1000,
            RESERVE = 0x2000,
            RESET = 0x80000,
            LARGE_PAGES = 0x20000000,
            PHYSICAL = 0x400000,
            TOP_DOWN = 0x100000,
            WRITE_WATCH = 0x200000
        }

        [Flags]
        internal enum MemoryProtection : UInt32
        {
            EXECUTE = 0x10,
            EXECUTE_READ = 0x20,
            EXECUTE_READWRITE = 0x40,
            EXECUTE_WRITECOPY = 0x80,
            NOACCESS = 0x01,
            READONLY = 0x02,
            READWRITE = 0x04,
            WRITECOPY = 0x08,
            GUARD_Modifierflag = 0x100,
            NOCACHE_Modifierflag = 0x200,
            WRITECOMBINE_Modifierflag = 0x400,
        }

        // Basic ASM strategy --
        // void x86CpuId(uint subleaf, uint leaf, byte* buffer)
        // {
        //    eax = leaf
        //    ecx = subleaf
        //    cpuid
        //    buffer[0] = eax
        //    buffer[4] = ebx
        //    buffer[8] = ecx
        //    buffer[12] = edx
        // }

        internal readonly static byte[] x86CodeBytes =
        {
            0x55,                   // push        ebp
            0x8B, 0xEC,             // mov         ebp,esp
            0x53,                   // push        ebx
            0x57,                   // push        edi

            0x8B, 0x4D, 0x08,       // mov         ecx, dword ptr [ebp+8] (move subleaf into ecx)
            0x8B, 0x45, 0x0C,       // mov         eax, dword ptr [ebp+12] (move leaf into eax)
            0x0F, 0xA2,             // cpuid

            0x8B, 0x7D, 0x10,       // mov         edi, dword ptr [ebp+16] (move address of buffer into edi)
            0x89, 0x07,             // mov         dword ptr [edi+0], eax  (write eax, ... to buffer)
            0x89, 0x5F, 0x04,       // mov         dword ptr [edi+4], ebx
            0x89, 0x4F, 0x08,       // mov         dword ptr [edi+8], ecx
            0x89, 0x57, 0x0C,       // mov         dword ptr [edi+12],edx

            0x5F,                   // pop         edi
            0x5B,                   // pop         ebx
            0x8B, 0xE5,             // mov         esp,ebp
            0x5D,                   // pop         ebp
            0xc3                    // ret
        };

        internal readonly static byte[] x64CodeBytes =
        {
            0x53,                       // push rbx    (this gets clobbered by cpuid)

            // WindowsとUnix系OSでは呼び出し規約でのレジスタの割当順が異なるので注意
            // https://en.wikipedia.org/wiki/X86_calling_conventions#x86-64_calling_conventions
            // Register Usage: https://msdn.microsoft.com/en-us/library/9z1stfyw.aspx
            // レジスタの使用: https://msdn.microsoft.com/ja-jp/library/9z1stfyw.aspx

            // rcx is subleaf
            // rdx is leaf
            // r8 is buffer.

            // Move rdx (leaf) to rax to call cpuid, call cpuid
            0x48, 0x89, 0xD0,           // mov    rax, rdx
            0x0F, 0xA2,                 // cpuid

            // Write eax et al to buffer
            0x41, 0x89, 0x40, 0x00,     // mov    dword ptr [r8+0],  eax
            0x41, 0x89, 0x58, 0x04,     // mov    dword ptr [r8+4],  ebx
            0x41, 0x89, 0x48, 0x08,     // mov    dword ptr [r8+8],  ecx
            0x41, 0x89, 0x50, 0x0c,     // mov    dword ptr [r8+12], edx

            0x5b,                       // pop rbx
            0xc3                        // ret
        };
    }
}
