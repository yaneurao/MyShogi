using System;
using System.Media;

namespace MyShogi.Model.Resource.Sounds
{
    /// <summary>
    /// wavファイル一つのwrapper。
    /// 一度読み込んだものはメモリ上の残しておく。
    /// </summary>
    public class Sound : IDisposable
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
            // 読み込んでいるファイル名と異なる時のみ読み直す。
            if (filename != filename_)
            {
                Release();
                try
                {
                    player = new SoundPlayer(filename_);
                    try
                    {
                        player.Load();
                    } catch { }
                } catch { }
                filename = filename_;
            }
        }

        /// <summary>
        /// 読み込んでいるサウンドを開放する。
        /// </summary>
        public void Release()
        {
            if (player != null)
            {
                player.Dispose();
                player = null;
            }
        }

        /// <summary>
        /// サウンドを再生する。
        /// 再生が終わるまで制御は戻ってこない。
        /// ※　SoundPlayerでは再生の終了を検知できないので、このような構造になっている。
        /// 　　専用のスレッドを生成して、そのスレッドで再生すべき。
        /// このメソッドは例外を投げない。
        /// </summary>
        public void Play()
        {
            if (player != null)
            {
                try
                {
                    player.PlaySync();
                } catch { }
            }
        }
        
        public void Dispose()
        {
            Release();
        }

        /// <summary>
        /// 読み込んでいるサウンド
        /// </summary>
        private SoundPlayer player = null;

        /// <summary>
        /// 読み込んでいるサウンドファイル名
        /// </summary>
        private string filename;
    }
}
