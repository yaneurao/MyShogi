#if MONO

// MONO環境向けの、機種依存コードはすべてここに突っ込んである。
//
// 現在、Macで動くように作業中。
// Linux環境は適宜修正すべし。

using System;
using System.Drawing;
using MyShogi.Model.Shogi.EngineDefine;

// --- 単体で呼び出して使うAPI群

namespace MyShogi.Model.Dependent
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

            var process = new System.Diagnostics.Process
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
            var info = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sysctl",
                Arguments = "machdep.cpu.features",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
            };

            var process = new System.Diagnostics.Process
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
            var name = f.OriginalFontName;
            var size = f.Size;
            switch (name)
            {
                case "MS Gothic":
                case "MS UI Gothic":
                case "ＭＳ ゴシック":
                case "MSPゴシック":
                case "YU Gothic UI":
                    return new Font("Hiragino Kaku Gothic Pro W3", size);

                default:
                    return f;
            }
        }
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
