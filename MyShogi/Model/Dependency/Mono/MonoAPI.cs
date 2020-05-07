#if MONO

// MONO環境向けの、機種依存コードはすべてここに突っ込んである。
//
// 現在、Macで動くように作業中。
// Linux環境は適宜修正すべし。

// 定義済みシンボル
// ・MacかLinux環境 → MONO
// ・macOS →　MACOS
// ・Linux →　LINUX

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MyShogi.Model.Common.String;
using MyShogi.Model.Common.Tool;
using MyShogi.Model.Shogi.EngineDefine;

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
            // Linuxでは、nprocを用いるる

#if MACOS
            var FileName = "sysctl";
            var Arguments = "hw.activecpu";
#elif LINUX
            var FileName = "nproc";
            var Arguments = "--all";
#endif

            var info = new ProcessStartInfo
            {
                FileName = FileName,
                Arguments = Arguments,
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
#if MACOS
            // Linuxだとこれ不要
            result = result.Substring(14);
#endif

            int processor_cores;
            var success = int.TryParse(result, out processor_cores);
            if (!success)
            {
                // 失敗したのでログ上に出力しておく。
                Log.Write(LogInfoType.SystemError, $"GetProcessorCores() , success = {success} , processor_cores = {processor_cores}");
                processor_cores = 1;
            }

            return processor_cores;
        }

        /// <summary>
        /// CPUの種別を判定して返す。
        /// </summary>
        /// <returns></returns>
        public static CpuType GetCurrentCpu()
        {
#if MACOS
            var FileName = "sysctl";
            var Arguments = "machdep.cpu.features";
#elif LINUX
            var FileName = "grep";
            var Arguments = "flags /proc/cpuinfo";
#endif

            var info = new ProcessStartInfo
            {
                FileName = FileName,
                Arguments = Arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
            };

            using (var process = new Process { StartInfo = info })
            {
                process.Start();

                var result = process.StandardOutput.ReadToEnd();
                result = result.Trim();
#if MACOS
            result = result.Substring(22);
#elif LINUX
                // Linuxだともう少し長い文字列であり、小文字が混じる可能性がある。
                result = result.ToUpper();
#endif

                CpuType c;
                if (result.Contains("AVX512"))
                    c = CpuType.AVX512;

                else if (result.Contains("AVX2"))
                    c = CpuType.AVX2;

                else if (result.Contains("AVX1"))
                    c = CpuType.AVX;

                else if (result.Contains("SSE4.2") /* macOS */ || result.Contains("SSE4_2") /* Linux */)
                    c = CpuType.SSE42;

                else if (result.Contains("SSE4.1") /* macOS */ || result.Contains("SSE4_1") /* Linux */)
                    c = CpuType.SSE41;

                else if (result.Contains("SSE2"))
                    c = CpuType.SSE2;

                else
                    c = CpuType.NO_SSE;

                process.WaitForExit();
                return c;
            }
        }

        /// <summary>
        /// 現在使用されていない利用可能な物理メモリのサイズ(kB)
        /// </summary>
        /// <returns></returns>
        public static ulong GetFreePhysicalMemory()
        {

#if MACOS
			var FileName = "vm_stat";
			var Arguments = "";
#elif LINUX
            var FileName = "vmstat";
            var Arguments = "-s";
#endif

            var info = new ProcessStartInfo
            {
                FileName = FileName,
                Arguments = Arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
            };

            using (var process = new Process { StartInfo = info })
            {
                process.Start();

                var result = process.StandardOutput.ReadToEnd();
                var rows = result.Split('\n');

                ulong freeMemory = 0;
                // 始めの行はタイトルなので飛ばす
                for (int i = 1; i < rows.Length; i++)
                {
                    var row = rows[i];

#if MACOS
                var data = row.Split(':');
                if (data.Length != 2) continue;

                var key = data[0].Trim();
                var value = data[1].Trim();
                // 空きメモリとカーネルが持っているキャッシュを合計する(必要になったら開放できるもの)
                if (key == "Pages free" || key == "Pages purgeable" || key == "File-backed pages")
                {
                    // 1 page = 4[kB]なので、4をかけて[kB]単位に変換する。
                    freeMemory += ulong.Parse(value.Replace(".", "")) * 4ul;
                }
#elif LINUX
                    var data = row.Split('K');
                    if (data.Length != 2) continue;

                    var key = data[1].Trim();
                    var value = data[0].Trim();

                    // 空きメモリとカーネルが持っているキャッシュを合計する(必要になったら開放できるもの)
                    if (key == "inactive memory" || key == "free memory" || key == "buffer memory")
                    {
                        //  CentOSなど、"95724 K free memory"のように返ってくる。
                        // この"K"でSplitして、左側の数字。単位は、kB単位なので単位の変換不要。
                        freeMemory += ulong.Parse(value.Replace(".", ""));
                    }
#endif
                }
                process.WaitForExit();
                return freeMemory;
            }
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
            // Linux用 "xclip"がインストールされていることを前提とできるなら、xclipを呼べば良い。
            // Linux用、別途xclipのインストールが必要。
            if (!File.Exists("/usr/bin/xclip"))
                return;

            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo("xclip", "-selection clipboard")
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
            // Linux用、別途xclipのインストールが必要。
            if (!File.Exists("/usr/bin/xclip"))
            {
                pasteText = null;
            }
            else
            {
                using (var p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo("xclip", "-selection clipboard -o")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    };
                    p.Start();
                    pasteText = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                }
            }
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
        public static readonly string DefaultFont1 = "Hiragino Kaku Gothic Pro W3";
        public static readonly string DefaultFont2 = "Hiragino Kaku Gothic Pro W3";
        public static readonly string DefaultFont3 = "Hiragino Kaku Gothic Pro W3";

        /// <summary>
        /// 設定ダイアログ
        /// </summary>
        public static readonly string SettingDialog = DefaultFont1;

        /// <summary>
        /// メニューのフォント
        /// </summary>
        public static readonly string MenuStrip = DefaultFont1;

        /// <summary>
        /// メインウインドウのToolStrip(ボタン)のフォント
        /// ここ、◀ ▶ が大きく表示されるフォントでないとつらい。
        /// </summary>
        public static readonly string MainToolStrip = DefaultFont3;

        /// <summary>
        /// メッセージダイアログのフォント
        /// </summary>
        public static readonly string MessageDialog = DefaultFont1;

        /// <summary>
        /// メインウインドウ上のフォント(対局者名など)
        /// </summary>
        public static readonly string MainWindow = DefaultFont1;

        /// <summary>
        /// ミニ盤面下のToolStrip(ボタン)のフォント
        /// ここ、◀ ▶ が大きく表示されるフォントでないとつらい。
        /// </summary>
        public static readonly string SubToolStrip = DefaultFont3;

        /// <summary>
        /// 棋譜ウインドウ
        /// 棋譜ウインドウの文字、等幅フォントでないと秒の出力のところが表示がずれるのでデフォルトで等倍フォントにすべき。
        /// </summary>
        public static readonly string KifuWindow = DefaultFont2;

        /// <summary>
        /// 検討ウインドウ
        /// </summary>
        public static readonly string ConsiderationWindow = DefaultFont2;

        /// <summary>
        /// ミニ盤面の上のタブの文字列
        /// </summary>
        public static readonly string MiniBoardTab = DefaultFont1;

        /// <summary>
        /// ToolTip
        /// </summary>
        public static readonly string ToolTip = DefaultFont1;

        /// <summary>
        /// デバッグウインドウ
        /// </summary>
        public static readonly string DebugWindow = DefaultFont1;

        /// <summary>
        /// 形勢グラフコントロール
        /// </summary>
        public static readonly string EvalGraphControl = DefaultFont1;
    }
}

// --- 音声の再生

namespace MyShogi.Model.Resource.Sounds
{
    /// <summary>
    /// wavファイル一つのwrapper。
    ///
    /// 他の環境に移植する場合は、このクラスをその環境用に再実装すべし。
    /// 
    /// 仕様)
    /// 再生自体は、複数のwavファイルの同時再生できると仮定してる。
    /// SoundLoaderの1つのインスタンスは、1つのwavファイルと結びついていて、
    /// 複数インスタンスのそれぞれのPlay()を同時に呼び出せば同時に再生されるものとする。
    /// 
    /// 再生中かどうかはSoundLoader.IsPlay()によって、その紐づけられているwavファイルが再生中であるかを
    /// 照会できるものとする。
    /// 
    /// また、棋譜読み上げは前のファイルの再生が終わるまで次のファイルの再生が保留されるが、
    /// 駒音などはそのファイルが再生中であろうと問答無用で再生する。これによりの、読み上げと駒音は同時再生される。
    /// </summary>
    public class SoundLoader : IDisposable
    {
        public SoundLoader()
        {
            lock (lockObject)
            {
                if (refCount++ == 0)
                {
                    // ファイルが存在しないときはreturnするが、このとき、
                    // 参照カウント自体は非0でかつ、_playerProcess == null
                    var playerExePath = Path.Combine(Directory.GetCurrentDirectory(), "SoundPlayer.exe");
                    if (!File.Exists(playerExePath))
                        return;

                    var info = new ProcessStartInfo
                    {
                        FileName = "mono",
                        Arguments = $"\"{playerExePath}\" \"{Directory.GetCurrentDirectory()}\"",

                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = false,
                    };

                    var process = new Process
                    {
                        StartInfo = info,
                    };

                    process.Start();
                    _playerProcess = process;
                }
            }

        }

        /// <summary>
        /// ファイルからサウンドを読み込む。
        /// wavファイル。
        /// 以前に読み込んだファイル名と同じ時は読み直さない。
        /// このメソッドは例外を投げない。
        /// </summary>
        /// <param name="filename_"></param>
        public void ReadFile(string filename_)
        {
            // フルパスにして保持しておく。Play()のときに用いる。
            filename = Path.Combine(Directory.GetCurrentDirectory() , filename_);
        }

        /// <summary>
        /// 読み込んでいるサウンドを開放する。
        /// </summary>
        public void Release()
        {
            lock (lockObject)
            {
                _playerProcess?.StandardInput.WriteLine($"release {filename}");
            }
        }

        /// <summary>
        /// サウンドを非同期に再生する。
        /// </summary>
        public void Play()
        {
            lock (lockObject)
            {
                if (_playerProcess == null || _playerProcess.HasExited)
                    return;

                play_id = play_id_count++;
                _playerProcess.StandardInput.WriteLine($"play {play_id} {filename}");
            }
        }

        /// <summary>
        /// 再生中であるかを判定して返す。
        /// 
        /// 棋譜読み上げのときは、IsPlaying()がfalseを返すまで次のPlay()は呼び出されないが、
        /// 駒音などは即座に再生する必要があるので、IsPlaying()は呼び出されず、Play()が呼び出される。
        /// この場合、現在のそのファイルの再生を中断して即座にそのファイルを再生しなおす必要がある。
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            if (_playerProcess == null || _playerProcess.HasExited)
                return false;

            _playerProcess.StandardInput.WriteLine($"is_playing {play_id}");
            var result = _playerProcess.StandardOutput.ReadLine();
            if (String.IsNullOrWhiteSpace(result))
                return false;

            result = result.Trim();
            return result == "yes";
        }

        public void Dispose()
        {
            Release();

            lock (lockObject)
            {
                if (-- refCount == 0)
                {
                    _playerProcess?.StandardInput.WriteLine("exit");
                    _playerProcess = null;
                }
            }
        }

        // サウンド再生のための通しナンバー。再生中であるかはこれを用いて問い合わせる。
        private int play_id;

        /// <summary>
        /// 読み込んでいるサウンドファイル名(FullPathで)
        /// </summary>
        private string filename;

        // 音声再生サーバーの通信用
        private static Process _playerProcess;

        // このクラスのインスタンス数 (0になったらサーバーを止める)
        private static int refCount = 0;

        // play_idの発行用。
        private static int play_id_count = 0;

        // 排他制御用
        private static object lockObject = new object();
    }
}

#endif
