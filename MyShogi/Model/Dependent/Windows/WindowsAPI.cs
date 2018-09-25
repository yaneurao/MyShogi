#if !MONO

// Windows環境に依存するクラス群はすべてここに突っ込んである。
// Mono(Mac、Linux環境のほうは、MonoAPI.csのほうを参照すること)

using System;
using System.Drawing;
using System.IO;
using System.Management;
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
    /// Controlのフォントの一括置換用
    /// </summary>
    public static class FontReplacer
    {
        /// <summary>
        /// 引数で与えられたFontに対して、この環境用のフォントを生成しなおして返す。
        /// </summary>
        /// <param name="master">Dialog自体に設定されているフォント。master == fなら、フォントを変更するときに解体しない。</param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Font ReplaceFont(Font master , Font f)
        {
#if true
            // Windows環境では置換する必要はないのでそのまま返す。
            return f;
#else
            // 試しに全部明朝体にしてみて、うまく動作しているかをテスト。

            var name = f.OriginalFontName;
            var size = f.Size;
            switch (name)
            {
                case "MS UI Gothic":
                    // TODO : ここで古いフォントを解体するの、所有権の問題があってややこしい。きちんと書かないと二重解放になる。あとでよく考える。
                    if (master != f)
                        f.Dispose();
                    return new Font("ＭＳ 明朝", size);

                case "ＭＳ ゴシック":
                    // TODO : ここで古いフォントを解体するの、所有権の問題があってややこしい。きちんと書かないと二重解放になる。あとでよく考える。
                    if (master != f)
                        f.Dispose();
                    return new Font("ＭＳ 明朝", f.Size);

                default:
                    return f;
            }
#endif
        }

        /// <summary>
        /// Controlを渡して、そのフォントを一括置換する。
        /// </summary>
        /// <param name="control"></param>
        public static void ReplaceFont(Control control)
        {
            foreach (Control c in control.Controls)
                ReplaceFont(control.Font, c.Font);
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
