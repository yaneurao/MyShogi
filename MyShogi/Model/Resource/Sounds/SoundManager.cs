using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MyShogi.Model.Shogi.Core;
using SCore = MyShogi.Model.Shogi.Core;

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
        /// 棋譜文字列を頑張って読み上げる。
        /// </summary>
        /// <param name="text"></param>
        public void ReadOut(string kif)
        {
            // ">+ 16.☖３六飛行成　32秒"
            // みたいなフォーマットなので"."までまず読み飛ばす。

            var index = kif.IndexOf('.');
            if (index == -1)
                return; // そんな阿呆な..

            ++index;
            // 次の1文字を得る
            char next()
            {
                while (true)
                {
                    if (index >= kif.Length)
                        return (char)0;

                    var c = kif[index++];
                    // 半角スペース、全角スペースの読み飛ばし
                    if (c == '　' || c == ' ')
                        continue;

                    return c;
                }
            }

            // 升目の読み上げ

            // ▲△▼▽☗☖⛊⛉があるので、全角数字まで読み飛ばす

            int file;
            do
            {
                var c = next();
                if (c == 0)
                    return;

                file = "１２３４５６７８９同".IndexOf(c);
                if (file >= 9)
                {
                    // 一文字、バッファに差し戻す
                    --index;
                    goto Rest;
                }
            } while (file == -1);
            
            // 漢数字は、Unicode上、連続していない。(部首画数順なので…)
            var rank = "一二三四五六七八九".IndexOf(next());
            if (rank == -1)
                return;

            var sq = Util.MakeSquare((SCore.File)file, (Rank)rank);
            Play(sq);

            Rest:;
            while (true)
            {
                // あとはそのまま読み上げていく。(数字以外を)
                // 数字に遭遇したら終了。
                var c = next();
                if (('0' <= c && c <= '9') || c == 0)
                    return;

                switch (c)
                {
                    case '歩': Play(Piece.PAWN); break;
                    case '香': Play(Piece.LANCE); break;
                    case '桂': Play(Piece.KNIGHT); break;
                    case '銀': Play(Piece.SILVER); break;
                    case '金': Play(Piece.GOLD); break;
                    case '角': Play(Piece.BISHOP); break;
                    case '飛': Play(Piece.ROOK); break;
                    case '玉': Play(Piece.KING); break;
                    case '王': Play(Piece.KING); break;
                    case '馬': Play(Piece.HORSE); break;
                    case '龍': Play(Piece.DRAGON); break;
                    case 'と': Play(Piece.PRO_PAWN); break;

                    case '同': Play(SoundEnum.Onajiku); break;
                    case '右': Play(SoundEnum.Migi); break;
                    case '左': Play(SoundEnum.Hidari); break;
                    case '直': Play(SoundEnum.Sugu); break;
                    case '引': Play(SoundEnum.Hiku); break;
                    case '打': Play(SoundEnum.Utsu); break;
                    case '寄': Play(SoundEnum.Yoru); break;
                    case '上': Play(SoundEnum.Agaru); break;
                    case '行': Play(SoundEnum.Yuku); break;

                    case '成':
                        switch (next())
                        {
                            case '香': Play(Piece.PRO_LANCE); break;
                            case '桂': Play(Piece.PRO_KNIGHT); break;
                            case '銀': Play(Piece.PRO_SILVER); break;
                            default: Play(SoundEnum.Naru); break;
                        }
                        break;

                    case '不':
                        // 「不成」
                        next(); // 読み捨てる
                        Play(SoundEnum.Narazu);
                        break;

                    default: // 送り仮名？無視する。
                        break;
                }
            }

        }

        /// <summary>
        /// 棋譜文字列を頑張って読み上げる。
        /// m == special moveのみ
        /// </summary>
        /// <param name="text"></param>
        public void ReadOut(Move m)
        {
            Debug.Assert(m.IsSpecial());

            switch (m)
            {
                // 連続王手の千日手による反則勝ちも音声上は「千日手」
                case Move.REPETITION_WIN : Play(SoundEnum.Sennichite); break;
                case Move.REPETITION_LOSE: Play(SoundEnum.Sennichite); break;
                case Move.REPETITION_DRAW: Play(SoundEnum.Sennichite); break;

                // 最大手数による引き分けも音声上は「持将棋」
                case Move.WIN            : Play(SoundEnum.Jisyougi); break;
                case Move.MAX_MOVES_DRAW : Play(SoundEnum.Jisyougi); break;

                case Move.MATED          : Play(SoundEnum.Tsumi); break;
                case Move.TIME_UP        : Play(SoundEnum.Jikangire); break;
            }
            // その他は知らん。
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
