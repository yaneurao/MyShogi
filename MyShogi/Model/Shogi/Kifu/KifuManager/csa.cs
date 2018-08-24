using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MyShogi.Model.Shogi.Converter;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Kifu
{
    // CSA標準棋譜ファイル形式 V2.2
    // http://www2.computer-shogi.org/protocol/record_v22.html

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
            // 消費時間、残り時間、消費時間を管理する。
            var timeSettings = KifuTimeSettings.TimeLimitless;
            var times = timeSettings.GetInitialKifuMoveTimes();

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
            KifuMove lastKifuMove = null;
            var move = Move.NONE;
            var rTimeLimit = new Regex(@"([0-9]+):([0-9]+)\+([0-9]+)");
            var rEvent = new Regex(@"^[0-9A-Za-z-_]+\+[0-9A-Za-z_]+-([0-9]+)-([0-9]+)(F?)\+");

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
                    // 現時点ではこの書式に対応せず、先頭の棋譜のみを読み込む。
                    // そもそも初期局面が異なる可能性もあり、Treeを構成できるとは限らないため。
                    break;
                }
                // マルチステートメント検出
                string[] sublines = line.Split(',');
                foreach (var subline in sublines)
                {
                    if (subline.StartsWith("$"))
                    {
                        // 棋譜ヘッダ
                        var keyvalue = subline.Substring(1).Split(":".ToCharArray(), 2);
                        if (string.IsNullOrWhiteSpace(keyvalue[0]))
                            continue;
                        var key = keyvalue[0];
                        var value = keyvalue[1] ?? "";
                        KifuHeader.header_dic[key] = value;
                        switch (key)
                        {
                            case "TIME_LIMIT":
                                var mTimeLimit = rTimeLimit.Match(value);
                                if (mTimeLimit.Success)
                                {
                                    int.TryParse(mTimeLimit.Groups[1].Value, out int hour);
                                    int.TryParse(mTimeLimit.Groups[2].Value, out int minute);
                                    int.TryParse(mTimeLimit.Groups[3].Value, out int byoyomi);
                                    timeSettings = new KifuTimeSettings(
                                        new KifuTimeSetting[]
                                        {
                                            new KifuTimeSetting() { Hour = hour, Minute = minute, Second = 0, Byoyomi = byoyomi, ByoyomiEnable = true, IncTime = 0 },
                                            new KifuTimeSetting() { Hour = hour, Minute = minute, Second = 0, Byoyomi = byoyomi, ByoyomiEnable = true, IncTime = 0 },
                                        },
                                        false
                                    );
                                    times = timeSettings.GetInitialKifuMoveTimes();
                                    Tree.SetKifuMoveTimes(times.Clone()); // root局面での残り時間の設定
                                    Tree.KifuTimeSettings = timeSettings.Clone();
                                }
                                break;
                            case "EVENT":
                                // floodgate特例、TIME_LIMITが設定されていない時にEVENT文字列から時間設定を拾う
                                if (KifuHeader.header_dic.ContainsKey("TIME_LIMIT"))
                                    continue;
                                var mEvent = rEvent.Match(value);
                                if (mEvent.Success)
                                {
                                    int.TryParse(mEvent.Groups[1].Value, out int initial);
                                    int.TryParse(mEvent.Groups[2].Value, out int add);
                                    if (mEvent.Groups[3].Value == "F")
                                    {
                                        timeSettings = new KifuTimeSettings(
                                            new KifuTimeSetting[]
                                            {
                                                new KifuTimeSetting() { Hour = 0, Minute = 0, Second = initial, Byoyomi = 0, ByoyomiEnable = false, IncTime = add, IncTimeEnable = true },
                                                new KifuTimeSetting() { Hour = 0, Minute = 0, Second = initial, Byoyomi = 0, ByoyomiEnable = false, IncTime = add, IncTimeEnable = true },
                                            },
                                            false
                                        );
                                    }
                                    else
                                    {
                                        timeSettings = new KifuTimeSettings(
                                            new KifuTimeSetting[]
                                            {
                                                new KifuTimeSetting() { Hour = 0, Minute = 0, Second = initial, Byoyomi = add, ByoyomiEnable = true, IncTime = 0, IncTimeEnable = false },
                                                new KifuTimeSetting() { Hour = 0, Minute = 0, Second = initial, Byoyomi = add, ByoyomiEnable = true, IncTime = 0, IncTimeEnable = false },
                                            },
                                            false
                                        );
                                    }
                                    times = timeSettings.GetInitialKifuMoveTimes();
                                    Tree.SetKifuMoveTimes(times.Clone()); // root局面での残り時間の設定
                                    Tree.KifuTimeSettings = timeSettings.Clone();
                                }
                                break;
                        }
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
                            Tree.position.SetSfen(Tree.rootSfen);
                            Tree.rootSfen = CsaExtensions.CsaToSfen(posLines.ToArray());
                            continue;
                        }
                        // 2回目以降は指し手とみなす
                        // 消費時間の情報がまだないが、取り敢えず追加
                        move = Tree.position.FromCSA(subline);
                        Tree.AddNode(move, times.Clone());
                        lastKifuMove = Tree.currentNode.moves.FirstOrDefault((x) => x.nextMove == move);
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
                            return $"line {lineNo}: 初期局面で着手時間が指定されました。";
                        }
                        long.TryParse(subline.Substring(1), out long time);
                        var lastMove = state.lastMove;
                        if (move == lastMove && move == lastKifuMove.nextMove)
                        {
                            var turn = Tree.position.sideToMove.Not();
                            var thinking_time = TimeSpan.FromSeconds(time);
                            times.Players[(int)turn] = times.Players[(int)turn].Create(
                                timeSettings.Player(turn),
                                thinking_time, thinking_time
                                );
                            lastKifuMove.kifuMoveTimes = times.Clone();
                        }
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
                        Tree.AddNode(move, times.Clone());
                        lastKifuMove = Tree.currentNode.moves.FirstOrDefault((x) => x.nextMove == move);
                        continue;
                    }
                }
            }
            if (headFlag) // まだ局面図が終わってない
                return $"CSA形式の{lineNo}行目で局面図が来ずにファイルが終了しました。";

            return null;
        }

        /// <summary>
        /// 局面のCSA文字列化。これをファイルに保存するとCSA形式のファイルになる。
        /// </summary>
        /// <returns></returns>
        private string ToCsaString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("V2.2");
            sb.AppendLine($"N+{KifuHeader.PlayerNameBlack}");
            sb.AppendLine($"N-{KifuHeader.PlayerNameWhite}");
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
            foreach (var headerKey in KifuHeader.header_dic.Keys)
            {
                switch (headerKey)
                {
                    case "EVENT":
                    case "SITE":
                    case "START_TIME":
                    case "END_TIME":
                    case "TIME_LIMIT":
                    case "OPENING":
                        sb.AppendLine($"${headerKey}:{KifuHeader.header_dic[headerKey]}");
                        break;
                    default:
                        sb.AppendLine($"'{headerKey}:{KifuHeader.header_dic[headerKey]}");
                        break;
                }
            }

            while (Tree.currentNode.moves.Count != 0)
            {
                var move = Tree.currentNode.moves[0];
                var m = move.nextMove;

                if (m.IsSpecial())
                {
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
                    break;
                }

                var thinkingTime = move.kifuMoveTimes.Player(Tree.position.sideToMove).ThinkingTime;
                if (!Tree.position.IsLegal(m))
                {
                    sb.AppendLine("%ILLEGAL_MOVE");
                    // 現時点の実装としては秒未満切り捨てとして出力。
                    sb.AppendLine($"'{Tree.position.ToCSA(m)},T{Math.Truncate(thinkingTime.TotalSeconds)}");
                    break;
                }

                // 現時点の実装としては秒未満切り捨てとして出力。
                sb.AppendLine($"{Tree.position.ToCSA(m)},T{Math.Truncate(thinkingTime.TotalSeconds)}");

                Tree.DoMove(move);

            }
            return sb.ToString();
        }
    }
}

