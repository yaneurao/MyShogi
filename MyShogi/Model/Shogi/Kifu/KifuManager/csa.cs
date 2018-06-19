using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// csaの読み書き
    /// </summary>
    public partial class KifuManager
    {
        /// <summary>
        /// CSA形式の棋譜ファイルのparser
        /// エラーがあった場合は、そのエラーの文字列が返る。
        /// エラーがなければstring.Emptyが返る。
        /// </summary>
        private string FromCsaString(string[] lines, KifuFileType kf)
        {
            var lineNo = 1;

            /*
             * 例)

                V2.2
                N+人間
                N-人間
                P1-KY-KE-GI-KI-OU-KI-GI-KE-KY
                P2 * -HI *  *  *  *  * -KA *
                P3-FU-FU-FU-FU-FU-FU-FU-FU-FU
                P4 *  *  *  *  *  *  *  *  *
                P5 *  *  *  *  *  *  *  *  *
                P6 *  *  *  *  *  *  *  *  *
                P7+FU+FU+FU+FU+FU+FU+FU+FU+FU
                P8 * +KA *  *  *  *  * +HI *
                P9+KY+KE+GI+KI+OU+KI+GI+KE+KY
                P+
                P-
                +
                +7776FU,T3
                -8384FU,T1
            */

            string line = string.Empty;
            var posLines = new List<string>();
            var headFlag = true;
            var move = Move.NONE;
            for (; lineNo <= lines.Length; ++lineNo)
            {
                line = lines[lineNo - 1];

                // コメント文
                if (line.StartsWith("'"))
                {
                    Tree.currentNode.comment += line.Substring(1).TrimEnd('\r', '\n') + "\n";
                    continue;
                }
                // セパレータ検出
                if (line.StartsWith("/"))
                {
                    // "/"だけの行を挟んで、複数の棋譜・局面を記述することができる。
                    // 現時点ではこのに対応せず、先頭の棋譜のみを読み込む。
                    // そもそも初期局面が異なる可能性もあり、Treeを構成できるとは限らないため。
                    break;
                }
                // マルチステートメント検出
                string[] sublines = line.Split(',');
                foreach (var subline in sublines)
                {
                    if (subline.StartsWith("$"))
                    {
                        // ToDo: 各種棋譜情報
                        continue;
                    }
                    if (subline.StartsWith("N+"))
                    {
                        KifuHeader.PlayerNameBlack = subline.Substring(2);
                        continue;
                    }
                    if (subline.StartsWith("N-"))
                    {
                        KifuHeader.PlayerNameWhite = subline.Substring(2);
                        continue;
                    }
                    if (subline.StartsWith("P"))
                    {
                        posLines.Add(subline);
                        continue;
                    }
                    if (subline.StartsWith("+") || subline.StartsWith("-"))
                    {
                        if (headFlag)
                        {
                            // 1回目は局面の先後とみなす
                            headFlag = false;
                            posLines.Add(subline);
                            Tree.rootSfen = CsaExtensions.CsaToSfen(posLines.ToArray());
                            Tree.position.SetSfen(Tree.rootSfen);
                            continue;
                        }
                        // 2回目以降は指し手とみなす
                        // 消費時間の情報がまだないが、取り敢えず追加
                        move = Tree.position.FromCSA(subline);
                        Tree.AddNode(move, KifuMoveTimes.Zero /* TODO: 消費時間あとでなんとかする*/);
                        // 特殊な指し手や不正な指し手ならDoMove()しない
                        if (move.IsSpecial() || !Tree.position.IsLegal(move))
                        {
                            continue;
                        }
                        Tree.DoMove(move);
                        continue;
                    }
                    if (subline.StartsWith("T"))
                    {
                        // 着手時間
                        var state = Tree.position.State();
                        if (state == null)
                        {
                            return string.Format("line {0}: 初期局面で着手時間が指定されました。", lineNo);
                        }
                        var time = long.Parse(subline.Substring(1));
                        // 1手戻って一旦枝を削除する（時間情報を追加できないので）
                        var lastMove = state.lastMove;
                        if (move == lastMove)
                        {
                            Tree.UndoMove();
                        }
                        Tree.Remove(move);
                        // 改めて指し手を追加
                        Tree.AddNode(move, KifuMoveTimes.Zero /*TODO:あとでちゃんと書く*//* TimeSpan.FromSeconds(time)*/);
                        // 特殊な指し手や不正な指し手ならDoMove()しない
                        if (move.IsSpecial() || !Tree.position.IsLegal(move))
                        {
                            continue;
                        }
                        Tree.DoMove(move);
                        continue;
                    }
                    if (subline.StartsWith("%"))
                    {
                        var match = new Regex("^%[A-Z_+-]+").Match(subline);
                        if (!match.Success)
                        {
                            continue;
                        }
                        switch (match.Groups[0].Value)
                        {
                            case "%TORYO":
                                move = Move.RESIGN;
                                break;
                            case "%CHUDAN":
                                move = Move.INTERRUPT;
                                break;
                            case "%SENNICHITE":
                                move = Move.REPETITION_DRAW;
                                break;
                            case "%TIME_UP":
                                move = Move.TIME_UP;
                                break;
                            case "%JISHOGI":
                                move = Move.MAX_MOVES_DRAW;
                                break;
                            case "%KACHI":
                                move = Move.WIN;
                                break;
                            case "%TSUMI":
                                move = Move.MATED;
                                break;

                            case "%ILLEGAL_MOVE":
                                move = Move.ILLEGAL_MOVE;
                                break;

                            case "%+ILLEGAL_ACTION":
                                move = Tree.position.sideToMove == Color.BLACK ? Move.ILLEGAL_ACTION_LOSE : Move.ILLEGAL_ACTION_WIN;
                                break;

                            case "%-ILLEGAL_ACTION":
                                move = Tree.position.sideToMove == Color.BLACK ? Move.ILLEGAL_ACTION_WIN : Move.ILLEGAL_ACTION_LOSE;
                                break;

                            case "%HIKIWAKE":
                                move = Move.DRAW;
                                break;

                            // 以下、適切な変換先不明
                            case "%FUZUMI":
                            case "%MATTA":
                            case "%ERROR":
                            default:
                                move = Move.NONE;
                                break;
                        }
                        Tree.AddNode(move, KifuMoveTimes.Zero);
                        continue;
                    }
                }
            }
            if (headFlag) // まだ局面図が終わってない
                return string.Format("CSA形式の{0}行目で局面図が来ずにファイルが終了しました。", lineNo);

            return string.Empty;
        }

        /// <summary>
        /// 局面のCSA文字列化。これをファイルに保存するとCSA形式のファイルになる。
        /// </summary>
        /// <returns></returns>
        private string ToCsaString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("V2.2");
            sb.AppendFormat("N+", KifuHeader.PlayerNameBlack).AppendLine();
            sb.AppendFormat("N-", KifuHeader.PlayerNameWhite).AppendLine();
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    // 平手の初期局面
                    sb.AppendLine("PI");
                    sb.AppendLine("+");
                    break;
                default:
                    // それ以外は任意局面として出力する
                    sb.AppendLine(Tree.position.ToCsa().TrimEnd('\r', '\n'));
                    break;
            }

            while (Tree.currentNode.moves.Count != 0)
            {
                var move = Tree.currentNode.moves[0];
                var m = move.nextMove;

                switch (m)
                {
                    case Move.MATED:
                        sb.AppendLine("%TORYO"); break;
                    case Move.INTERRUPT:
                        sb.AppendLine("%CHUDAN"); break;
                    case Move.REPETITION_WIN:
                        sb.AppendLine("%SENNICHITE"); break;
                    case Move.REPETITION_DRAW:
                        sb.AppendLine("%SENNICHITE"); break;
                    case Move.WIN:
                        sb.AppendLine("%KACHI"); break;
                    case Move.MAX_MOVES_DRAW:
                        sb.AppendLine("%JISHOGI"); break;
                    case Move.RESIGN:
                        sb.AppendLine("%TORYO"); break;
                    case Move.TIME_UP:
                        sb.AppendLine("%TIME_UP"); break;
                }

                if (m.IsSpecial())
                {
                    break;
                }

                var thinkingTime = Tree.currentNode.moves[0].kifuMoveTimes.Player(Tree.position.sideToMove).ThinkingTime;
                if (!Tree.position.IsLegal(m))
                {
                    sb.AppendLine("%ILLEGAL_MOVE");
                    // 現時点の実装としては秒未満切り捨てとして出力。
                    sb.AppendFormat("'{0},T{1}", Tree.position.ToCSA(m), System.Math.Truncate(thinkingTime.TotalSeconds)).AppendLine();
                    break;
                }

                // 現時点の実装としては秒未満切り捨てとして出力。
                sb.AppendFormat("'{0},T{1}", Tree.position.ToCSA(m), System.Math.Truncate(thinkingTime.TotalSeconds)).AppendLine();

                Tree.DoMove(move);
            }
            return sb.ToString();
        }
    }
}

