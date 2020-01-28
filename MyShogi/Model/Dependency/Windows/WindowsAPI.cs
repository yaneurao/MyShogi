#if !MONO

// Windows環境に依存するクラス群はすべてここに突っ込んである。
// Mono(Mac、Linux環境のほうは、MonoAPI.csのほうを参照すること)

using System;
using System.Drawing;
using System.IO;
using System.Management;
using System.Windows.Forms;
using System.Windows.Media; // PresentationCore.dllのアセンブリ参照が必要。正直使いたくないのだが…。
using MyShogi.Model.Shogi.EngineDefine;

// --- 単体で呼び出して使うAPI群

namespace MyShogi.Model.Dependency
{
    /// <summary>
    /// Windows環境に依存するAPI群
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
            var sumOfCores = 0;
            using (ManagementClass mc = new ManagementClass("Win32_Processor"))
            using (ManagementObjectCollection moc = mc.GetInstances())
                // これプロセッサが複数ある環境だとmocが複数あるので足し合わせたものにする。
                foreach (ManagementObject mo in moc)
                {
                    var obj = mo["NumberOfCores"];
                    if (obj != null) // 普通、このプロパティが取得できないことはないはずなのだが…。
                        sumOfCores += (int)(uint)obj;
                }

            return sumOfCores;
        }

        /// <summary>
        /// CPUの種別を判定して返す。
        /// </summary>
        /// <returns></returns>
        public static CpuType GetCurrentCpu()
        {
            var cpuid = CpuId.flags;

            CpuType c;
            if (cpuid.hasAVX512F)
                c = CpuType.AVX512;
            else if (cpuid.hasAVX2)
                c = CpuType.AVX2;
            else if (cpuid.hasAVX)
                c = CpuType.AVX;
            else if (cpuid.hasSSE42)
                c = CpuType.SSE42;
            else if (cpuid.hasSSE41)
                c = CpuType.SSE41;
            else if (cpuid.hasSSE2)
                c = CpuType.SSE2;
            else
                // そんな阿呆な…。
                throw new Exception("CPU判別に失敗しました。");

            return c;
        }

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
                        result = (ulong)mo["FreePhysicalMemory"]; // このメンバ存在しないということはないはず。

                        mo.Dispose(); // これ要るのか？
                    }
            }
            catch { }

            if (!Environment.Is64BitOperatingSystem)
            {
                // 32bitで動作しているプロセス空間では、2GBまでしか物理メモリを扱えないので
                // 物理メモリがいかにあろうと2GBであるとみなす。
                result = System.Math.Min(result, 2 * 1024 * 1024ul);
            }

            return result;
        }

    }

    /// <summary>
    /// MonoやUbuntuではClipboardの仕組みが異なるので、標準のClipboardクラスをwrapしておく。
    /// 
    /// cf.Mono, Ubuntu and Clipboard : https://www.medo64.com/2011/01/mono-ubuntu-and-clipboard/
    /// </summary>
    public static class ClipboardEx
    {
        // System.Windows.Clipboardの同名のメソッドに委譲するだけ。

        public static void SetText(string text) { Clipboard.SetText(text); }
        public static string GetText() { return Clipboard.ContainsText() ? Clipboard.GetText() : null; }
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
            g.DrawImage(src, dstRect, srcRect, unit);
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
        public static readonly string DefaultFont1 = "MS UI Gothic";
        public static readonly string DefaultFont2 = "ＭＳ ゴシック";
        public static readonly string DefaultFont3 = "Yu Gothic UI";

        // 特徴的なフォントに変更してみて、フォントの置換が適切に行われているかをテストする。
        //public static readonly string DefaultFont = "HGP行書体";

        /// <summary>
        /// 設定ダイアログ
        /// </summary>
        public static readonly string SettingDialog = DefaultFont1;

        /// <summary>
        /// メニューのフォント
        /// </summary>
        public static readonly string MenuStrip = DefaultFont2;

        /// <summary>
        /// メインウインドウのToolStrip(ボタン)のフォント
        /// ここ、Yu Gothic UI にしておかないと、◀ ▶ が小さい。(ＭＳ ゴシックだとこの文字かなり小さい)
        /// </summary>
        public static readonly string MainToolStrip = DefaultFont3;

        /// <summary>
        /// ミニ盤面下のToolStrip(ボタン)のフォント
        /// </summary>
        public static readonly string SubToolStrip = DefaultFont3;
        
        /// <summary>
        /// メッセージダイアログのフォント
        /// </summary>
        public static readonly string MessageDialog = DefaultFont1;

        /// <summary>
        /// メインウインドウ上のフォント(対局者名など)
        /// </summary>
        public static readonly string MainWindow = DefaultFont1;

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
    }
}

// --- 音声の再生

namespace MyShogi.Model.Resource.Sounds
{
    /// <summary>
    /// wavファイル一つのwrapper。
    /// 
    /// ※ MediaPlayerを使った実装に変更した。
    ///  ・System.Windows.Media.MediaPlayerを利用するためのアセンブリ"PresentationCore.dll"アセンブリを参照に追加。
    ///	 ・System.Windows.Freezableを利用するためのアセンブリ"WindowsBase.dll"アセンブリを参照に追加。
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
                player.Close();
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
                    player = new MediaPlayer();
                    player.Open(new System.Uri(Path.GetFullPath(filename)));
                }

                /*
                // player.MediaEnded += (sender,args) => { playing = false; };
                // 再生の完了イベントを拾いたいのだが、どうもMediaEndedバグっているのではないかと…。
                // cf. https://stackoverflow.com/questions/21231577/mediaplayer-mediaended-not-called-if-playback-is-started-from-a-task
                // WMPのバージョンが変わって、イベントの定数が変更になって、イベントが発生しないパターンっぽい。
                */

                // Positionをセットしなおすと再度Play()で頭から再生できるようだ。なんぞこの裏技。
                player.Position = TimeSpan.Zero;
                player.Play();

            }
            catch { }
        }

        /// <summary>
        /// 再生中であるかを判定して返す。
        ///
        /// 棋譜読み上げのときは、IsPlaying()がfalseを返すまで次のPlay()は呼び出されないが、
        /// 駒音などは即座に再生する必要があるので、IsPlaying()は呼び出されず、Play()が呼び出される。
        /// この場合、現在のそのファイルの再生を中断して即座にそのファイルを再生しなおす必要がある。
        /// /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            // 終了イベント捕捉できないので再生カーソルの位置を見て判定する(´ω｀)
            return player != null &&
                (!player.NaturalDuration.HasTimeSpan
                /* これtrueになってからでないと、TimeSpanにアクセスできない。また、これがfalseである間は、再生準備中。*/
                || player.Position != player.NaturalDuration.TimeSpan);
        }

        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// 読み込んでいるサウンド
        /// </summary>
        private MediaPlayer player = null;

        /// <summary>
        /// 読み込んでいるサウンドファイル名
        /// </summary>
        private string filename;

    }
}


#endif
