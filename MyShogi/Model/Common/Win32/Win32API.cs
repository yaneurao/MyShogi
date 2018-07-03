using System;
using System.Runtime.InteropServices;

namespace MyShogi.Model.Common.Win32API
{
    /// <summary>
    /// Win32 絡みのAPIを直接呼ばないと解決しない群。
    /// 他の環境ではとりあえず無視しておく。
    /// </summary>
    public static class Win32API
    {
#if !MONO
        // Win32 APIのインポート
        [DllImport("USER32.DLL")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, UInt32 bRevert);
        [DllImport("USER32.DLL")]
        private static extern UInt32 RemoveMenu(IntPtr hMenu, UInt32 nPosition, UInt32 wFlags);

        // ［閉じる］ボタンを無効化するための値
        private const UInt32 SC_CLOSE = 0x0000F060;
        private const UInt32 MF_BYCOMMAND = 0x00000000;

        /// <summary>
        /// 閉じるボタンの無効化
        /// </summary>
        public static void HideCloseButton(IntPtr WindowHandle)
        {
            // Win32APIを使う必要がある。
            IntPtr hMenu = GetSystemMenu(WindowHandle, 0);
            RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
        }
#else
        public static void HideCloseButton(IntPtr WindowHandle){}
#endif

    }
}
