#if MONO

// MONO環境向けの、機種依存コードはすべてここに突っ込んである。
//
// 現在、Macで動くように作業中。
// Linux環境は適宜修正すべし。

// 定義済みシンボル
// ・MacかLinux環境 → MONO
// ・macOS →　MACOS
// ・Linux →　LINUX

using MyShogi.Model.Shogi.EngineDefine;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

// --- 単体で呼び出して使うAPI群

namespace MyShogi.Model.Dependency
{
    /// <summary>
    /// Mac/Linux環境に依存するAPI群
    /// </summary>
    public static class API
    {
        /// <summary>
        /// CPUの物理コア数を返す。
        /// マルチCPUである場合は、物理コア数のトータルを返す。
        /// </summary>
        /// <returns></returns>
        public static int GetProcessorCores()
        {
            // MacOSではsysctlを呼び出して物理コア数が取得できる模様。
            // Linuxは未確認。

            var info = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sysctl",
                // WorkingDirectory = engineData.ExeWorkingDirectory,
                Arguments = "hw.activecpu",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
            };

            var process = new Process
            {
                StartInfo = info,
            };

            process.Start();

            var result = process.StandardOutput.ReadToEnd();
            result = result.Trim();
            result = result.Substring(14);

            int processor_cores;
            var success = Int32.TryParse(result, out processor_cores);
            if (!success)
                processor_cores = 1;

            return processor_cores;
        }

        /// <summary>
        /// CPUの種別を判定して返す。
        /// </summary>
        /// <returns></returns>
        public static CpuType GetCurrentCpu()
        {
            var info = new ProcessStartInfo
            {
                FileName = "sysctl",
                Arguments = "machdep.cpu.features",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
            };

            var process = new Process
            {
                StartInfo = info,
            };

            process.Start();

            string result = process.StandardOutput.ReadToEnd();
            result = result.Trim();
            result = result.Substring(22);

            CpuType c;
            if (result.Contains("AVX512"))
                c = CpuType.AVX512;

            else if (result.Contains("AVX2"))
                c = CpuType.AVX2;

            else if (result.Contains("AVX1"))
                c = CpuType.AVX;

            else if (result.Contains("SSE4.2"))
                c = CpuType.SSE42;

            else if (result.Contains("SSE4.1"))
                c = CpuType.SSE41;

            else if (result.Contains("SSE2"))
                c = CpuType.SSE2;

            else
                c = CpuType.NO_SSE;

            return c;
        }

        /// <summary>
        /// 現在使用されていない利用可能な物理メモリのサイズ(kB)
        /// </summary>
        /// <returns></returns>
        public static ulong GetFreePhysicalMemory()
        {
            var info = new ProcessStartInfo
            {
                FileName = "vm_stat",
                Arguments = "",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
            };

            var process = new Process
            {
                StartInfo = info,
            };

            process.Start();

            string result = process.StandardOutput.ReadToEnd();
            string[] rows = result.Split('\n');

            ulong freeMemory = 0;
            // 始めの行はタイトルなので飛ばす
            for(int i = 1; i < rows.Length; i++)
            {
                string row = rows[i];
                string[] data = row.Split(':');
                if (data.Length != 2) continue;

                string key = data[0].Trim();
                string value = data[1].Trim();
                // 空きメモリとカーネルが持っているキャッシュを合計する(必要になったら開放できるもの)
                if (key == "Pages free" || key == "Pages purgeable" || key == "File-backed pages")
                {
                    freeMemory += ulong.Parse(value.Replace(".", "")) * 4096ul;
                }
            }

            return freeMemory / 1024ul;
        }
    }

    /// <summary>
    /// MonoやUbuntuではClipboardの仕組みが異なるので、標準のClipboardクラスではなくこちらを用いる。
    /// 
    /// cf.
    /// Mono, Ubuntu and Clipboard : https://www.medo64.com/2011/01/mono-ubuntu-and-clipboard/
    /// Clipboard Plugin for Xamarin, Windows & Gtk2 : https://github.com/stavroskasidis/XamarinClipboardPlugin
    /// Mono Clipboard fix : http://bighow.org/questions/Mono-Clipboard-fix
    /// </summary>
    public static class ClipboardEx
    {
        public static void SetText(string text)
        {
#if MACOS
            // Macではpbcopyコマンドで実現。
            // この方法ならば追加でいかなるアセンブリ、ランタイムも不要。
            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo("pbcopy", "-pboard general - Prefer txt")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardInput = true,
                };
                p.Start();
                p.StandardInput.Write(text);
                p.StandardInput.Close();
                p.WaitForExit();
            }
#elif LINUX
            // Linux用、あとで実装する。
#endif
        }

        public static string GetText()
        {
            string pasteText;
#if MACOS
            // Macではpbpasteコマンドで実現。
            // この方法ならば追加でいかなるアセンブリ、ランタイムも不要。
            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo("pbpaste", "-pboard general")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                };
                p.Start();
                pasteText = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
#elif LINUX
            // Linux用、あとで実装する。
            pasteText = null;
#endif
            return pasteText;
        }
    }

    /// <summary>
    /// MonoでGraphics.DrawImage()で転送元が半透明かつ、転送先がCreateBitmap()したBitmapだと
    /// 転送元のalphaが無視されるので、DrawImage()をwrapする。
    /// 
    /// Monoではこの挙動、きちんと実装されていない。(bugだと言えると思う)
    /// Monoは、GDIPlusまわりの実装、いまだにおかしいところ多い。
    /// </summary>
    public static class DrawImageHelper
    {
        public static void DrawImage(Graphics g, Image dst, Image src, Rectangle dstRect, Rectangle srcRect, GraphicsUnit unit)
        {
            // Lockして自前で転送する。
            if (dstRect == srcRect &&
                dst.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb &&
                src.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb &&
                src is Bitmap &&
                dst is Bitmap
                )
            {
                var src_ = src as Bitmap;
                var data = src_.LockBits(srcRect, ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                var dst_ = dst as Bitmap;

                // Lockするとき、WriteOnlyにしてしまうと、Monoではゼロクリアされる。
                // これもMonoの実装上のバグだと思う。

                var data2 = dst_.LockBits(dstRect, /*ImageLockMode.WriteOnly*/ ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                byte[] buf = new byte[srcRect.Width * srcRect.Height * 4];
                Marshal.Copy(data.Scan0, buf, 0, buf.Length);
                for (int i = 0, j = 0; i < buf.Length; i += 4 , j+=3)
                {
                    /* alphaがある程度大きければ、このpixelは上書き転送 */
                    if (buf[i+3] > 128)
                    {
                        Marshal.WriteByte(data2.Scan0, j + 0, buf[i + 0]);
                        Marshal.WriteByte(data2.Scan0, j + 1, buf[i + 1]);
                        Marshal.WriteByte(data2.Scan0, j + 2, buf[i + 2]);
                    }
                }

                dst_.UnlockBits(data2);
                src_.UnlockBits(data);

            } else
            {
                // すまん。救済不可
                g.DrawImage(src, dstRect, srcRect, unit);
            }
        }
    }
}

// --- フォントの設定
namespace MyShogi.Model.Common.Tool
{
    /// <summary>
    /// 各ダイアログで用いるデフォルトフォント名の一覧。
    /// 表示設定ダイアログのフォントのところで変更できる。
    /// </summary>
    public static class FontList
    {
        public static readonly string DefaultFont = "Hiragino Kaku Gothic Pro W3";

        /// <summary>
        /// 設定ダイアログ
        /// </summary>
        public static readonly string SettingDialog = DefaultFont;

        /// <summary>
        /// メニューのフォント
        /// </summary>
        public static readonly string MenuStrip = DefaultFont;

        /// <summary>
        /// メインウインドウのToolStrip(ボタン)のフォント
        /// </summary>
        public static readonly string MainToolStrip = DefaultFont;

        /// <summary>
        /// メッセージダイアログのフォント
        /// </summary>
        public static readonly string MessageDialog = DefaultFont;

        /// <summary>
        /// メインウインドウ上のフォント(対局者名など)
        /// </summary>
        public static readonly string MainWindow = DefaultFont;

        /// <summary>
        /// ミニ盤面下のToolStrip(ボタン)のフォント
        /// </summary>
        public static readonly string SubToolStrip = DefaultFont;

        /// <summary>
        /// 棋譜ウインドウ
        /// </summary>
        public static readonly string KifuWindow = DefaultFont;

        /// <summary>
        /// 検討ウインドウ
        /// </summary>
        public static readonly string ConsiderationWindow = DefaultFont;

        /// <summary>
        /// デバッグウインドウ
        /// </summary>
        public static readonly string DebugWindow = DefaultFont;
    }
}

// --- 音声の再生

namespace MyShogi.Model.Resource.Sounds
{
    /// <summary>
    /// wavファイル一つのwrapper。
    /// 
    /// 他の環境に移植する場合は、このクラスをその環境用に再実装すべし。
    /// </summary>
    public class SoundLoader : IDisposable
    {
        /// <summary>
        /// ファイルからサウンドを読み込む。
        /// wavファイル。
        /// 以前に読み込んだファイル名と同じ時は読み直さない。
        /// このメソッドは例外を投げない。
        /// </summary>
        /// <param name="filename_"></param>
        public void ReadFile(string filename_)
        {
            filename = filename_;
        }

        /// <summary>
        /// 読み込んでいるサウンドを開放する。
        /// </summary>
        public void Release()
        {
            if (player != null)
            {
                //player.Stop();
                // Stop()ではリソースの開放がなされないようである…。
                // 明示的にClose()を呼び出す。
                //player.Close();
                player = null;
            }
        }

        /// <summary>
        /// サウンドを非同期に再生する。
        /// </summary>
        public void Play()
        {
            try
            {
                if (player == null)
                {
                    //player = new MediaPlayer();
                    //player.Open(new System.Uri(Path.GetFullPath(filename)));
                }

                // Positionをセットしなおすと再度Play()で頭から再生できるようだ。なんぞこの裏技。
                //player.Position = TimeSpan.Zero;
                //player.Play();

            }
            catch { }
        }

        /// <summary>
        /// 再生中であるかを判定して返す。
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            return false;
        }

        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// 読み込んでいるサウンド
        /// </summary>
        private object player = null;

        /// <summary>
        /// 読み込んでいるサウンドファイル名
        /// </summary>
        private string filename;

    }
}

#endif
