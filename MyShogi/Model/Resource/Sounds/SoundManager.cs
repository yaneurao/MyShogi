using MyShogi.Model.Shogi.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyShogi.Model.Resource.Sounds
{
    /// <summary>
    /// サウンドのリソースを取り扱うクラス。
    /// 汎用クラスではなく、本ソフト専用。
    /// </summary>
    public class SoundManager : IDisposable
    {
        /// <summary>
        /// 開始する
        /// </summary>
        public void Start()
        {
            queue.Start();
        }

        /// <summary>
        /// 再生queueに追加する。
        /// このメソッドは毎回同じスレッドから呼び出されるものとする。
        /// </summary>
        /// <param name=""></param>
        public void Play(SoundEnum e)
        {
            if (!dic.ContainsKey(e))
            {
                var subFolder = e.IsKoma() ? KomaSoundPath : ReadOutSoundPath;
                var filename = Path.Combine(Path.Combine(SoundPath, subFolder), SoundHelper.FileNameOf(e));
                var s = new Sound();
                s.ReadFile(filename);
                dic.Add(e, s);
            }

            var sound = dic[e];
            queue.AddQueue(sound);
        }

        /// <summary>
        /// 升の名前の読み上げ音声を再生する。
        /// </summary>
        /// <param name="sq"></param>
        public void Play(Square sq)
        {
            Play(SoundEnum.SQ_11 + (int)sq);
        }

        /// <summary>
        /// 駒の名前の読み上げ音声を再生する。
        /// </summary>
        /// <param name="pc"></param>
        public void Play(Piece pc)
        {
            Play(SoundEnum.PiecePAWN + ((int)pc - 1));
        }

        /// <summary>
        /// Start()で開始させていたスレッドを停止させる。
        /// </summary>
        public void Dispose()
        {
            queue.Dispose();
        }

        /// <summary>
        /// サウンドのフォルダ
        /// </summary>
        public string SoundPath = "sound";

        /// <summary>
        /// 駒音の音声のpath
        /// SoundPath配下にあるものとする。
        /// </summary>
        public string KomaSoundPath = "koma";

        /// <summary>
        /// 読み上げ用の音声のpath
        /// SoundPath配下にあるものとする。
        /// </summary>
        public string ReadOutSoundPath = "takemata";

        // -- privates

        /// <summary>
        /// 再生キュー
        /// </summary>
        private SoundQueue queue = new SoundQueue();

        /// <summary>
        /// SoundEnumから、それに対応するSoundへのmap
        /// </summary>
        private Dictionary<SoundEnum,Sound> dic = new Dictionary<SoundEnum,Sound>();
    }
}
