using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MyShogi.App;
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
        /// 再生を停止させる。
        /// queueに積まれているものをクリアする。
        /// </summary>
        public void Stop()
        {
            queue.ClearQueue();
        }

        /// <summary>
        /// 再生queueに追加する。
        /// このメソッドは毎回同じスレッドから呼び出されるものとする。
        /// 
        /// GlobalConfig.KifuReadOutがオフ(0)だと再生されない。
        /// </summary>
        /// <param name=""></param>
        public void ReadOut(SoundEnum e)
        {
            if (TheApp.app.Config.KifuReadOut == 0)
                return;

            if (!dic.ContainsKey(e))
            {
                var subFolder = e.IsKoma() ? KomaSoundPath : ReadOutSoundPath;
                var filename = Path.Combine(Path.Combine(SoundPath, subFolder), SoundHelper.FileNameOf(e));
                var s = new SoundLoader();
                s.ReadFile(filename);
                dic.Add(e, s);
            }

            var sound = dic[e];
            queue.AddQueue(sound);
        }

        /// <summary>
        /// すぐに再生する。他の再生を待たずに。
        /// </summary>
        /// <param name="e"></param>
        public void Play(SoundEnum e)
        {
            if (!dic.ContainsKey(e))
            {
                var subFolder = e.IsKoma() ? KomaSoundPath : ReadOutSoundPath;
                var filename = Path.Combine(Path.Combine(SoundPath, subFolder), SoundHelper.FileNameOf(e));
                var s = new SoundLoader();
                s.ReadFile(filename);
                dic.Add(e, s);
            }

            var sound = dic[e];
            sound.Play();
        }

        /// <summary>
        /// 駒音の再生。これは即座に再生される。
        /// </summary>
        public void PlayPieceSound(SoundEnum e)
        {
            TheApp.app.SoundManager.Play(e);
        }

        /// <summary>
        /// 升の名前の読み上げ音声を再生する。
        /// </summary>
        /// <param name="sq"></param>
        public void ReadOut(Square sq)
        {
            ReadOut(SoundEnum.SQ_11 + (int)sq);
        }

        /// <summary>
        /// 駒の名前の読み上げ音声を再生する。
        /// </summary>
        /// <param name="pc"></param>
        public void ReadOut(Piece pc)
        {
            ReadOut(SoundEnum.PiecePAWN + ((int)pc - 1));
        }

        /// <summary>
        /// 棋譜文字列を頑張って読み上げる。
        /// </summary>
        /// <param name="text"></param>
        public void ReadOut(string kif)
        {
            // オプションの反映
            if (TheApp.app.Config.KifuReadOut == 0)
                return;

            // KIF2形式。"☗３六飛行成" みたいなフォーマット

            var index = 0;

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
            ReadOut(sq);

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
                    case '歩': ReadOut(Piece.PAWN); break;
                    case '香': ReadOut(Piece.LANCE); break;
                    case '桂': ReadOut(Piece.KNIGHT); break;
                    case '銀': ReadOut(Piece.SILVER); break;
                    case '金': ReadOut(Piece.GOLD); break;
                    case '角': ReadOut(Piece.BISHOP); break;
                    case '飛': ReadOut(Piece.ROOK); break;
                    case '玉': ReadOut(Piece.KING); break;
                    case '王': ReadOut(Piece.KING); break;
                    case '馬': ReadOut(Piece.HORSE); break;
                    case '龍': ReadOut(Piece.DRAGON); break;
                    case 'と': ReadOut(Piece.PRO_PAWN); break;

                    case '同': ReadOut(SoundEnum.Onajiku); break;
                    case '右': ReadOut(SoundEnum.Migi); break;
                    case '左': ReadOut(SoundEnum.Hidari); break;
                    case '直': ReadOut(SoundEnum.Sugu); break;
                    case '引': ReadOut(SoundEnum.Hiku); break;
                    case '打': ReadOut(SoundEnum.Utsu); break;
                    case '寄': ReadOut(SoundEnum.Yoru); break;
                    case '上': ReadOut(SoundEnum.Agaru); break;
                    case '行': ReadOut(SoundEnum.Yuku); break;

                    case '成':
                        switch (next())
                        {
                            case '香': ReadOut(Piece.PRO_LANCE); break;
                            case '桂': ReadOut(Piece.PRO_KNIGHT); break;
                            case '銀': ReadOut(Piece.PRO_SILVER); break;
                            default: ReadOut(SoundEnum.Naru); break;
                        }
                        break;

                    case '不':
                        // 「不成」
                        next(); // 読み捨てる
                        ReadOut(SoundEnum.Narazu);
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

            // オプションの反映
            if (TheApp.app.Config.KifuReadOut == 0)
                return;

            switch (m)
            {
                // 連続王手の千日手による反則勝ちも音声上は「千日手」
                case Move.REPETITION_WIN : ReadOut(SoundEnum.Sennichite); break;
                case Move.REPETITION_LOSE: ReadOut(SoundEnum.Sennichite); break;
                case Move.REPETITION_DRAW: ReadOut(SoundEnum.Sennichite); break;

                // 最大手数による引き分けも音声上は「持将棋」
                case Move.WIN            : ReadOut(SoundEnum.Jisyougi); break;
                case Move.WIN_THEM       : ReadOut(SoundEnum.Jisyougi); break;
                case Move.MAX_MOVES_DRAW : ReadOut(SoundEnum.Jisyougi); break;

                case Move.MATED          : ReadOut(SoundEnum.Tsumi); break;
                case Move.TIME_UP        : ReadOut(SoundEnum.Jikangire); break;
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
        private Dictionary<SoundEnum,SoundLoader> dic = new Dictionary<SoundEnum,SoundLoader>();
    }
}
