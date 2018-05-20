using MyShogi.Model.Shogi.Core;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 1局の対局棋譜を全般的に管理するクラス
    /// ・分岐棋譜をサポート
    /// ・着手時間をサポート
    /// ・対局相手の名前をサポート
    /// ・KIF/KI2/CSA/SFEN/PSN形式での入出力をサポート
    /// ・千日手の管理、検出をサポート
    /// </summary>
    public class KifuManager
    {
        // -------------------------------------------------------------------------
        // public members
        // -------------------------------------------------------------------------

        /// <summary>
        /// 対局者名。
        ///   playerName[(int)Color.BLACK] : 先手の名前(駒落ちの場合、下手)
        ///   playerName[(int)Color.WHITE] : 後手の名前(駒落ちの場合、上手)
        /// </summary>
        public string[] playerName;
        
        /// <summary>
        /// 棋譜本体。分岐棋譜。
        /// </summary>
        public KifuTree Tree = new KifuTree();

        /// <summary>
        /// rootの局面の局面タイプ
        /// 任意局面の場合は、BoardType.Others
        /// </summary>
        public BoardType rootBoardType;

        /// <summary>
        /// rootの局面図。sfen形式で。
        /// </summary>
        public string rootSfen;

        // -------------------------------------------------------------------------
        // public methods
        // -------------------------------------------------------------------------

        public KifuManager()
        {
            Init();
        }

        /// <summary>
        /// このクラスを初期化する。new KifuManager()とした時の状態になる。
        /// </summary>
        public void Init()
        {
            playerName = new string[2];
            Tree.Init();
            rootBoardType = BoardType.NoHandicap;
            rootSfen = Position.SFEN_HIRATE;
        }

        /// <summary>
        /// 棋譜ファイルを読み込む。
        /// this.Treeに反映する。また最終局面までthis.Tree.posを自動的に進める。
        /// フォーマットは自動判別。
        /// CSA/KIF/KI2/PSN/SFEN形式を読み込める。
        /// 
        /// ファイル丸ごと読み込んでstring型に入れて引数に渡すこと。
        /// 読み込めたところまでの棋譜を反映させる。読み込めなかった部分やエラーなどは無視する。
        /// 
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        /// <param name="filename"></param>
        public string FromString(string content /* , KifFileType kf */)
        {
            Init();

            var lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (lines.Length == 0)
                return "棋譜が空でした。";
            var line = lines[0];

            // sfen形式なのか？
            if ( line.StartsWith("sfen") || line.StartsWith("startpos"))
                return FromSfenString(line);

            // PSN形式なのか？
            if (line.StartsWith("[Sente"))
                return FromPsnString(lines);

            return string.Empty;
        }

        /// <summary>
        /// 棋譜ファイルを書き出す
        /// フォーマットは引数のkfで指定する。
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="kf"></param>
        public string ToString(KifuFileType kt)
        {
            switch(kt)
            {
                case KifuFileType.SFEN:
                    return ToSfenString();

                // ToDo : 他の形式もサポートする
            }

            return "";
        }

        // -------------------------------------------------------------------------
        // private members
        // -------------------------------------------------------------------------

        /// <summary>
        /// sfen文字列のparser
        /// USIプロトコルの"position"コマンドで使う文字列を読み込む。
        /// 便宜上、ここではsfenと呼んでいるが、本来はsfenには"moves .."は含まれない。
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        /// <param name="sfen"></param>
        private string FromSfenString(string sfen)
        {
            // Position.UsiPositionCmd()からの移植。

            // スペースをセパレータとして分離する
            var split = sfen.Split(
                new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            //// どうなっとるねん..
            //if (split.Length == 0)
            //    return;

            // 現在の指し手が書かれている場所 split[cur_pos]
            var cur_pos = 1;
            if (split[0] == "sfen")
            {
                // "sfen ... moves ..."形式かな..
                // movesの手前までをくっつけてSetSfen()する
                while (cur_pos < split.Length && split[cur_pos] != "moves")
                {
                    ++cur_pos;
                }

                if (!(cur_pos == 4 || cur_pos == 5))
                    throw new PositionException("Position.UsiPositionCmd()に渡された文字列にmovesが出てこない");

                if (cur_pos == 4)
                    rootSfen = string.Format("{0} {1} {2}", split[1], split[2], split[3]);
                else // if (cur_pos == 5)
                    rootSfen = string.Format("{0} {1} {2} {3}", split[1], split[2], split[3], split[4]);

                Tree.position.SetSfen(rootSfen);
                rootBoardType = BoardType.Others;
            }
            else if (split[0] == "startpos")
            {
                rootSfen = Position.SFEN_HIRATE;
                Tree.position.SetSfen(rootSfen);
                rootBoardType = BoardType.NoHandicap;
            }

            // "moves"以降の文字列をUSIの指し手と解釈しながら、局面を進める。
            if (cur_pos < split.Length && split[cur_pos] == "moves")
                for (int i = cur_pos + 1; i < split.Length; ++i)
                {
                    // デバッグ用に盤面を出力
                    //Console.WriteLine(Pretty());

                    var move = Util.FromUsiMove(split[i]);
                    if (!Tree.position.IsLegal(move))
                        // throw new PositionException(string.Format("{0}手目が非合法手です。", i - cur_pos));
                        return string.Format("{0}手目が非合法手です。", i - cur_pos);

                    // この指し手で局面を進める
                    Tree.Add(move , new TimeSpan());
                    Tree.DoMove(move);
                }

            return string.Empty;
        }

        /// <summary>
        /// PSN形式の棋譜ファイルのparser
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        private string FromPsnString(string[] lines)
        {
            if (lines.Length < 3)
                return "PSN形式の棋譜が短すぎます。";

            var r1 = new Regex(@"\[Sente ""(.*)""\]");
            var m1 = r1.Match(lines[0]);
            if (!m1.Success)
                return "PSN形式で先手の対局者名の部分がおかしいです。";
            playerName[(int)Color.BLACK] = m1.Groups[1].ToString();

            var r2 = new Regex(@"\[Gote ""(.*)""\]");
            var m2 = r2.Match(lines[1]);
            if (!m1.Success)
                return "PSN形式で後手の対局者名の部分がおかしいです。";
            playerName[(int)Color.WHITE] = m2.Groups[1].ToString();


            return string.Empty;
        }

        /// <summary>
        /// 現在の棋譜をUSIプロトコルの"position"コマンドで使う文字列化する。
        /// 便宜上、ここではsfenと呼んでいるが、本来はsfenには"moves .."は含まれない。
        /// 特殊な指し手は出力されない。
        /// </summary>
        /// <returns></returns>
        private string ToSfenString()
        {
            // 現在の局面をrootまで巻き戻す
            var moves = Tree.RewindToRoot();

            var sb = new StringBuilder();
            if (rootBoardType == BoardType.NoHandicap)
            {
                // 平手の初期局面
                sb.Append("startpos");

            } else if (rootBoardType == BoardType.Others)
            {
                // 任意局面なので"sfen"を出力する
                sb.Append(string.Format("sfen {0}",rootSfen));
            }

            // 候補の一つ目を選択していく。
            while(Tree.currentNode.moves.Count != 0)
            {
                // 1手目を出力するときには"moves"が必要。(このUSIの仕様、イケてないと思う)
                if (Tree.currentNode == Tree.rootNode)
                    sb.Append(" moves");

                // 複数候補があろうと1つ目の
                var m = Tree.currentNode.moves[0].nextMove;

                // 特殊な指し手文字列であればこれはsfenとしては出力できない。
                if (m.IsSpecial())
                    break;

                // 非合法手も出力せずにそこで中断することにする。
                if (!Tree.position.IsLegal(m))
                    break;

                sb.Append(" " + m.ToUsi());

                Tree.DoMove(m);
            }

            // 元の局面に戻す
            Tree.RewindToRoot();
            Tree.FastForward(moves);

            return sb.ToString();
        }

    }
}
