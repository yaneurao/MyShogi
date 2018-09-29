#if !MONO

// Windows環境に依存するクラス群はすべてここに突っ込んである。
// Mono(Mac、Linux環境のほうは、MonoAPI.csのほうを参照すること)

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;
using MyShogi.Model.Shogi.EngineDefine;

// --- 単体で呼び出して使うAPI群

namespace MyShogi.Model.Dependent
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

    /// <summary>
    /// Controlのフォントの一括置換用
    /// </summary>
    public static class FontReplacer
    {
        /// <summary>
        /// 引数で与えられたFontに対して、必要ならばこの環境用のフォントを生成して返す。
        /// FontUtility.ReplaceFont()から呼び出される。
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Font ReplaceFont(Font f)
        {
#if true
            // Windows環境では置換する必要はないのでそのまま返す。
            // あとで、棋譜ウインドウ、検討ウインドウ、対局者、ダイアログ、それぞれのフォントを変更できるようにするが…。
            return f;
#else
            // 試しに全部変わったフォントにしてみて、うまく動作しているかをテスト。

            // なぜか、OriginalFontNameがnullのことがある。MenuStrip.Items.Fontとかそう。そのときはf.Nameを参照しないといけない。
            var name = f.OriginalFontName ?? f.Name;
            var size = f.Size;

            //Console.WriteLine($"{name} : size ={size}");

            switch (name)
            {
                case "MS Gothic":
                case "MS UI Gothic":
                case "ＭＳ ゴシック":
                case "MSPゴシック":
                case "Yu Gothic UI":
                case "Microsoft Sans Serif":
                    return new Font("HGP行書体", size);

                default:
                    // 見知らぬフォント(OriginalFontNameが空欄でsizeしかないだとか…)
                    //Console.WriteLine($"{name} : size ={size}");

                    return f;
            }
#endif
        }
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
        /// </summary>
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
