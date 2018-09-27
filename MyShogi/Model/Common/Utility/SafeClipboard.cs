using System;
using System.Windows;
using MyShogi.App;
using MyShogi.Model.Common.Collections;

namespace MyShogi.Model.Common.Utility
{
    /// <summary>
    /// クリップボードへのアクセスは拒否されて例外が出ることがあるので例外のでない
    /// Clipboardの操作子を用意。
    ///
    /// cf. https://ja.stackoverflow.com/questions/47269/powershell%E3%82%84c%E3%81%8B%E3%82%89%E3%82%AF%E3%83%AA%E3%83%83%E3%83%97%E3%83%9C%E3%83%BC%E3%83%89%E8%BB%A2%E9%80%81%E6%99%82%E3%81%ABexternalexception%E3%81%8C%E9%A0%BB%E7%99%BA%E3%81%99%E3%82%8B%E5%8E%9F%E5%9B%A0%E3%81%A8%E5%AF%BE%E7%AD%96
    ///  > System.Windows.Forms.Clipboard のソースコードを確認してみると、
    ///  > OleSetClipboardとOleFlushClipboardというOle32.dllのWin32 APIを実行していることがわかります。
    ///  > これらの関数で戻り値0x800401D0は、CLIPBRD_E_CANT_OPENが対応しています。
    ///  > これは内部でOpenClipboardの実行に失敗したことを意味します。
    /// 
    /// </summary>
    public static class SafeClipboard
    {
        /// <summary>
        /// [UI Thread] : クリップボードに文字列を設定する。
        /// 
        /// 注意 : Clipboard.SetText() を実行するスレッドは Single Thread Apartment モードに設定されていなければならない
        /// UI Threadからこのメソッドを呼び出すこと。
        /// </summary>
        /// <param name="o"></param>
        public static void SetText(string s)
        {
            try
            {
                if (s.Empty()) return;

                Clipboard.SetText(s);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// [UI Thread] : クリップボードから文字列を取得する。
        /// 
        /// 空の場合や取得できない場合はnullが返る。
        /// </summary>
        /// <returns></returns>
        public static string GetText()
        {
            try
            {
                return Clipboard.ContainsText() ? Clipboard.GetText() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
