using System;
using System.Text;
using System.Collections.Generic;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// sfenの読み書き
    /// </summary>
    public partial class KifuManager
    {
        /// <summary>
        /// sfen文字列のparser
        /// USIプロトコルの"position"コマンドで使う文字列を読み込む。
        /// 便宜上、ここではsfenと呼んでいるが、本来はsfenには"moves .."は含まれない。
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければnullが返る。
        /// </summary>
        /// <param name="sfen"></param>
        private string FromSfenString(string sfen)
        {
            try
            {
                // Position.UsiPositionCmd()からの移植。

                // Scannerを用いた字句解析
                var scanner = new Scanner(sfen);

                //// どうなっとるねん..
                //if (split.Length == 0)
                //    return;

                if (scanner.PeekText() == "sfen")
                {
                    scanner.ParseText();

                    // "sfen ... moves ..."形式かな..
                    // movesの手前までをくっつけてSetSfen()する
                    var sfen_pos = new List<string>();
                    while (scanner.PeekText() != "moves")
                    {
                        if (sfen_pos.Count >= 4)
                            break;

                        sfen_pos.Add(scanner.ParseText());
                    }

                    Tree.rootSfen = string.Join(" ", sfen_pos.ToArray());
                    Tree.position.SetSfen(Tree.rootSfen);
                    Tree.rootBoardType = BoardType.Others;
                }
                else if (scanner.PeekText() == "startpos")
                {
                    scanner.ParseText();

                    Tree.rootSfen = Position.SFEN_HIRATE;
                    Tree.position.SetSfen(Tree.rootSfen);
                    Tree.rootBoardType = BoardType.NoHandicap;
                }

                // "moves"以降の文字列をUSIの指し手と解釈しながら、局面を進める。
                if (scanner.PeekText() == "moves")
                {
                    scanner.ParseText();
                    int ply = 1;
                    while (scanner.PeekText() != null)
                    {
                        // デバッグ用に盤面を出力
                        //Console.WriteLine(Pretty());

                        var move = Util.FromUsiMove(scanner.ParseText());
                        if (!Tree.position.IsLegal(move))
                            // throw new PositionException(string.Format("{0}手目が非合法手です。", i - cur_pos));
                            return string.Format("{0}手目が非合法手です。", ply);

                        // この指し手で局面を進める
                        Tree.AddNode(move, KifuMoveTimes.Zero /* 消費時間の記録がないので書けない */);
                        Tree.DoMove(move);
                        ++ply;
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return null;
        }

        /// <summary>
        /// 現在の棋譜をUSIプロトコルの"position"コマンドで使う文字列化する。
        /// 便宜上、ここではsfenと呼んでいるが、本来はsfenには"moves .."は含まれない。
        /// 特殊な指し手は出力されない。
        /// </summary>
        /// <returns></returns>
        private string ToSfenString()
        {
            var sb = new StringBuilder();
            if (Tree.rootBoardType == BoardType.NoHandicap)
            {
                // 平手の初期局面
                sb.Append("startpos");

            }
            else if (Tree.rootBoardType == BoardType.Others)
            {
                // 任意局面なので"sfen"を出力する
                sb.Append(string.Format("sfen {0}", Tree.rootSfen));
            }

            // 候補の一つ目を選択していく。
            while (Tree.currentNode.moves.Count != 0)
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

            return sb.ToString();
        }


    }
}
