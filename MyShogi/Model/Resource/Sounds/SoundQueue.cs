using System;
using System.Collections.Generic;
using System.Threading;

namespace MyShogi.Model.Resource.Sounds
{
    /// <summary>
    /// サウンドを逐次再生するクラス。
    /// </summary>
    public class SoundQueue : IDisposable
    {
        /// <summary>
        /// 再生用のスレッドを開始する。
        /// </summary>        
        public void Start()
        {
            stop = false;
            new Thread(worker).Start();
        }

        /// <summary>
        /// 開始したスレッドを停止させる。
        /// </summary>
        public void Stop()
        {
            stop = true;
        }

        /// <summary>
        /// 終了処理。Start()で開始したスレッドを終了させる。
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// 指定したサウンドを再生queueに積む。
        /// 現在再生しているものがなくなり次第(再生が終了次第)再生する。
        /// </summary>
        /// <param name="sound"></param>
        public void AddQueue(Sound sound)
        {
            lock (lock_object)
            {
                queue.Enqueue(sound);
            }
        }

        /// <summary>
        /// queueに積まれているSoundをクリアする。
        /// (以降の音声は再生されないが、現在再生されているものが停止されるわけではない。)
        /// </summary>
        /// <param name="sound"></param>
        public void ClearQueue()
        {
            lock (lock_object)
            {
                queue.Clear();
            }
        }

        // -- 以下private

        /// <summary>
        /// 再生スレッド用のworker
        /// </summary>
        private void worker()
        {
            Sound playing = null;
            while (!stop)
            {
                if (playing != null)
                {
                    // 再生中のものがあるなら、その再生の終わりまで待機する。
                    if (!playing.IsPlaying())
                    {
                        // 再生が終わっているので開放する。
                        //playing.Dispose();
                        // Soundのメモリ使用量は知れてるので一度読み込んだものを開放しないようにする。

                        playing = null;
                    }
                }
                else
                {
                    // 再生中ではないので、1つqueueから取り出して再生する。
                    Sound sound = null;
                    lock (lock_object)
                    {
                        if (queue.Count != 0)
                            sound = queue.Dequeue();
                    }
                    if (sound != null)
                    {
                        sound.Play();
                        playing = sound;
                        // 10msで終わるとは考えられないのでこのあとSleep()して良い。
                    }
                }
                Thread.Sleep(10); // これくらいの再生遅延は許されるであろう。
            }
        }

        /// <summary>
        /// worker thread用の停止フラグ
        /// </summary>
        private bool stop = false;

        /// <summary>
        /// 再生queue
        /// ここに積んだものを順番に再生する。
        /// </summary>
        private Queue<Sound> queue = new Queue<Sound>();

        /// <summary>
        /// queueの操作時用のlock
        /// </summary>
        private object lock_object = new object();
    }
}
