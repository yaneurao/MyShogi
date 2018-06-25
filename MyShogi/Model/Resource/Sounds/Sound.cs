using System;
using System.IO;
using System.Windows.Media;

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

            } catch {  }
        }

        /// <summary>
        /// 再生中であるかを判定して返す。
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            // 終了イベント捕捉できないので再生カーソルの位置を見て判定する(´ω｀)
            return player != null &&
                (! player.NaturalDuration.HasTimeSpan
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
