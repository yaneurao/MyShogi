using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// GDIで使用されているリソースの数などを調べる。
    /// リソースリークしていないかをデバッグ時に調べる用。
    /// 
    /// cf. https://phst.hateblo.jp/entry/2016/10/04/160502
    /// </summary>
    public static class GdiResourceWatcher
    {

        [DllImport("user32.dll")]
        private static extern uint GetGuiResources(IntPtr hProcess, uint uiFlags);
        const uint GR_GDIOBJECTS = 0;
        const uint GR_USEROBJECTS = 1;
        /// <summary>
        /// GDI オブジェクトの数を返す
        /// </summary>
        /// <returns></returns>
        public static uint GetGDIObjects()
        {
            return GetGuiResources(Process.GetCurrentProcess().Handle, GR_GDIOBJECTS);
        }
        /// <summary>
        /// User オブジェクトの数を返す
        /// </summary>
        /// <returns></returns>
        public static uint GetUserObjects()
        {
            return GetGuiResources(Process.GetCurrentProcess().Handle, GR_USEROBJECTS);
        }

        public static void DisplayMemory()
        {
            Console.WriteLine("Total memory  : {0:###,###,###,##0} bytes", GC.GetTotalMemory(false));
            Console.WriteLine("Private bytes   {0:###,###,###,##0} bytes", Process.GetCurrentProcess().PrivateMemorySize64); //プライベート メモリの量
            Console.WriteLine("Handle   count: {0}", Process.GetCurrentProcess().HandleCount); // ハンドル数
            Console.WriteLine("GDI  obj count: {0}", GetGDIObjects());
            Console.WriteLine("User obj count: {0}", GetUserObjects());
            Console.WriteLine();
        }
    }
}
