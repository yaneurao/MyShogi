using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Converter;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// kif/kif2の読み書き
    /// </summary>
    public partial class KifuManager
    {
        /// <summary>
        /// Kif/KI2形式の読み込み
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="kf"></param>
        /// <returns></returns>
        private string FromKifString(string[] lines, KifuFileType kf)
        {
            // 消費時間、残り時間、消費時間を管理する。
            // TimeLimitlessの設定を書き換えてしまう恐れがあるためCloneする
            KifuTimeSettings timeSettings = KifuTimeSettings.TimeLimitless.Clone();
            KifuMoveTimes times = KifuMoveTimes.Zero;

            var lineNo = 1;

            try
            {

                // ヘッダ検出用正規表現
                var rHead = new Regex(@"^([^：]+)：(.*)");
                // 変化手数用正規表現
                var rHenka = new Regex(@"^([0-9]+)手?");
                // KIF指し手検出用正規表現
                var rKif = new Regex(@"^\s*([0-9]+)\s*(?:((?:[1-9１-９][1-9１-９一二三四五六七八九]|同\s?)成?[歩香桂銀金角飛と杏圭全馬竜龍玉王][打不成左直右上寄引]*(?:\([1-9][1-9]\))?)|(\S+))\s*(\(\s*([0-9]+):([0-9]+(?:\.[0-9]+)?)\s*\/\s*([0-9]+):([0-9]+):([0-9]+(?:\.[0-9]+)?)\))?");
                // KI2指し手検出用正規表現
                var rKi2 = new Regex(@"[-+▲△▼▽☗☖⛊⛉](?:[1-9１-９][1-9１-９一二三四五六七八九]|同\s?)成?[歩香桂銀金角飛と杏圭全馬竜龍玉王][打不成左直右上寄引]*");
                // 終局検出用正規表現
                var rSpecial = new Regex(@"^まで([0-9]+)手(.+)");
                // 持ち時間/秒読み検出用正規表現
                var rTime = new Regex(@"^各?(\d+)(時間|分|秒)");

                var bod = new List<string>();

                var isBody = false;

                KifuHeader.header_dic.Clear();

                // 初期局面の遅延処理
                Func<string> lazyHead = () =>
                {
                    isBody = true;
                    if (bod.Count > 0)
                    {
                        // 柿木将棋IXでは、初期局面指定（詰将棋など）の時でも、KIF形式で書き出すと「手合割：平手」とヘッダ出力される。
                        // その場合の手合割の意味が理解出来ないが、エラーを出さずに黙って初期局面図の方で上書きする。
                        // if (KifuHeader.header_dic.ContainsKey("手合割")) return "手合割と初期局面文字列が同時に指定されています。";
                        var sfen = Converter.KifExtensions.BodToSfen(bod.ToArray());
                        Tree.SetRootSfen(sfen);
                    }
                    if (KifuHeader.header_dic.ContainsKey("持ち時間"))
                    {
                        var mTime = rTime.Match(KifuHeader.header_dic["持ち時間"]);
                        if (mTime.Success)
                        {
                            var sb = new StringBuilder();
                            foreach (char c in mTime.Groups[1].Value)
                                sb.Append((c < '０' || c > '９') ? c : (char)(c - '０' + '0'));
                            if (int.TryParse(sb.ToString(), out int mTimeVal))
                            //if (int.TryParse(Regex.Replace(mTime.Groups[1].Value, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString())), out int mTimeVal);
                            switch (mTime.Groups[2].Value)
                            {
                                case "時間":
                                    timeSettings = new KifuTimeSettings(
                                        new KifuTimeSetting[]
                                        {
                                            new KifuTimeSetting() { Hour = mTimeVal, Minute = 0, Second = 0 },
                                            new KifuTimeSetting() { Hour = mTimeVal, Minute = 0, Second = 0 },
                                        },
                                        false
                                    );
                                    break;
                                case "分":
                                    timeSettings = new KifuTimeSettings(
                                        new KifuTimeSetting[]
                                        {
                                            new KifuTimeSetting() { Hour = 0, Minute = mTimeVal, Second = 0 },
                                            new KifuTimeSetting() { Hour = 0, Minute = mTimeVal, Second = 0 },
                                        },
                                        false
                                    );
                                    break;
                                case "秒":
                                    timeSettings = new KifuTimeSettings(
                                        new KifuTimeSetting[]
                                        {
                                            new KifuTimeSetting() { Hour = 0, Minute = 0, Second = mTimeVal },
                                            new KifuTimeSetting() { Hour = 0, Minute = 0, Second = mTimeVal },
                                        },
                                        false
                                    );
                                    break;
                            }
                        }
                    }
                    if (KifuHeader.header_dic.ContainsKey("秒読み"))
                    {
                        var mTime = rTime.Match(KifuHeader.header_dic["秒読み"]);
                        if (mTime.Success)
                        {
                            var sb = new StringBuilder();
                            foreach (char c in mTime.Groups[1].Value)
                                sb.Append((c < '０' || c > '９') ? c : (char)(c - '０' + '0'));
                            if (int.TryParse(sb.ToString(), out int mTimeVal))
                            //if (int.TryParse(Regex.Replace(mTime.Groups[1].Value, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString())), out int mTimeVal);
                            foreach (var players in timeSettings.Players)
                            {
                                if (players.TimeLimitless)
                                {
                                    players.Hour = 0;
                                    players.Minute = 0;
                                    players.Second = 0;
                                    players.TimeLimitless = false;
                                }
                                switch (mTime.Groups[2].Value)
                                {
                                    case "時間":
                                        players.Byoyomi = int.Parse(mTime.Groups[1].Value) * 3600;
                                        players.ByoyomiEnable = true;
                                        break;
                                    case "分":
                                        players.Byoyomi = int.Parse(mTime.Groups[1].Value) * 60;
                                        players.ByoyomiEnable = true;
                                        break;
                                    case "秒":
                                        players.Byoyomi = int.Parse(mTime.Groups[1].Value);
                                        players.ByoyomiEnable = true;
                                        break;
                                }
                            }
                        }
                    }
                    // 残り時間の計算用。
                    times = timeSettings.GetInitialKifuMoveTimes();
                    Tree.SetKifuMoveTimes(times.Clone()); // root局面での残り時間の設定
                    Tree.KifuTimeSettings = timeSettings.Clone();
                    return string.Empty;
                };

                // ブロック分割走査
                for (; lineNo <= lines.Length; ++lineNo)
                {
                    var line = lines[lineNo - 1].Trim('\r', '\n');
                    // 空文
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var firstChar = line[0];
                    // 無効行
                    if (firstChar == '#') continue;
                    // 棋譜コメント文
                    if (firstChar == '*')
                    {
                        Tree.currentNode.comment += line + "\n";
                        continue;
                    }
                    // 局面図
                    if (firstChar == '|')
                    {
                        bod.Add(line);
                        continue;
                    }
                    // ヘッダ検出
                    var mHead = rHead.Match(line);
                    if (mHead.Success)
                    {
                        var headerKey = mHead.Groups[1].Value;
                        var headerValue = mHead.Groups[2].Value;
                        switch (headerKey)
                        {
                            case "先手の持駒":
                            case "下手の持駒":
                            case "後手の持駒":
                            case "上手の持駒":
                                if (isBody) throw new KifuException("対局開始後にヘッダが指定されました。", line);
                                bod.Add(line);
                                goto nextline;
                            case "変化":
                                if (!isBody) throw new KifuException("初期局面からは変化できません。");
                                var mHenka = rHenka.Match(headerValue);
                                if (!mHenka.Success) throw new KifuException("変化する手数を検出できませんでした。");
                                var ply = int.Parse(mHenka.Groups[1].Value);
                                while (ply < Tree.gamePly)
                                    Tree.UndoMove();
                                // このnodeでの残り時間に戻す
                                times = Tree.GetKifuMoveTimes();
                                goto nextline;
                            case "手合割":
                                if (isBody) throw new KifuException("対局開始後にヘッダが指定されました。", line);
                                KifuHeader.header_dic.Add(headerKey, headerValue);

                                // 局面を指定されたBoardTypeで初期化する。
                                void SetTree(BoardType bt) { Tree.SetRootBoardType(bt); }
                                switch (headerValue)
                                {
                                    case "平手": SetTree(BoardType.NoHandicap); goto nextline;
                                    case "香落ち": SetTree(BoardType.HandicapKyo); goto nextline;
                                    case "右香落ち": SetTree(BoardType.HandicapRightKyo); goto nextline;
                                    case "角落ち": SetTree(BoardType.HandicapKaku); goto nextline;
                                    case "飛車落ち": SetTree(BoardType.HandicapHisya); goto nextline;
                                    case "飛香落ち": SetTree(BoardType.HandicapHisyaKyo); goto nextline;
                                    case "二枚落ち": SetTree(BoardType.Handicap2); goto nextline;
                                    case "三枚落ち": SetTree(BoardType.Handicap3); goto nextline;
                                    case "四枚落ち": SetTree(BoardType.Handicap4); goto nextline;
                                    case "五枚落ち": SetTree(BoardType.Handicap5); goto nextline;
                                    case "左五枚落ち": SetTree(BoardType.HandicapLeft5); goto nextline;
                                    case "六枚落ち": SetTree(BoardType.Handicap6); goto nextline;
                                    case "八枚落ち": SetTree(BoardType.Handicap8); goto nextline;
                                    case "十枚落ち": SetTree(BoardType.Handicap10); goto nextline;

                                    default:
                                        // このときlazyHead()で設定される。
                                        break;
                                }
                                goto nextline;
                            case "先手":
                            case "下手":
                                if (isBody) throw new KifuException("対局開始後にヘッダが指定されました。", line);
                                KifuHeader.header_dic.Add(headerKey, headerValue);
                                KifuHeader.PlayerNameBlack = headerValue;
                                goto nextline;
                            case "後手":
                            case "上手":
                                if (isBody) throw new KifuException("対局開始後にヘッダが指定されました。", line);
                                KifuHeader.header_dic.Add(headerKey, headerValue);
                                KifuHeader.PlayerNameWhite = headerValue;
                                goto nextline;
                            default:
                                if (isBody) throw new KifuException("対局開始後にヘッダが指定されました。", line);
                                KifuHeader.header_dic.Add(headerKey, headerValue);
                                goto nextline;
                        }
                    }

                    foreach (var bodKey in new string[] {
                        "先手番",
                        "後手番",
                        "上手番",
                        "下手番",
                        "手数＝",
                    })
                    {
                        if (line.StartsWith(bodKey))
                        {
                            if (isBody) throw new KifuException("対局開始後にヘッダが指定されました。", line);
                            bod.Add(line);
                            goto nextline;
                        }
                    }

                    // KIF形式検出
                    var mKif = rKif.Match(line);
                    if (mKif.Success)
                    {
                        if (!isBody)
                        {
                            var headRes = lazyHead();
                            if (headRes != string.Empty)
                                return headRes;
                        }
                        var ply = int.Parse(mKif.Groups[1].Value);
                        if (Tree.gamePly != ply)
                            throw new KifuException($"手数({Tree.gamePly})が一致しません。", line);
                        Move move;
                        if (mKif.Groups[2].Success)
                        {
                            move = Tree.position.FromKif(mKif.Groups[2].Value);
                            if (!Tree.position.IsLegal(move))
                                // これだと不正着手後の棋譜コメントを取れないがとりあえず解析を中止する
                                throw new KifuException("不正着手を検出しました。", line);
                        }
                        else switch (mKif.Groups[3].Value)
                        {
                            case "投了":
                                move = Move.RESIGN;
                                break;
                            case "中断":
                            case "封じ手":
                                move = Move.INTERRUPT;
                                break;
                            case "千日手":
                                move = Move.REPETITION_DRAW;
                                break;
                            case "詰み":
                                move = Move.MATED;
                                break;
                            case "時間切れ":
                            case "切れ負け":
                                move = Move.TIME_UP;
                                break;
                            case "パス":
                                move = Move.NULL;
                                break;
                            case "持将棋":
                                move = Move.MAX_MOVES_DRAW;
                                break;
                            case "勝ち宣言":
                                move = Move.WIN;
                                break;
                            default:
                                move = Move.NONE;
                                break;
                        }
                        if (move == Move.NONE)
                            throw new KifuException("指し手を解析できませんでした。", line);
                        TimeSpan thinking_time = TimeSpan.Zero;
                        TimeSpan total_time = TimeSpan.Zero;
                        if (mKif.Groups[4].Success)
                        {
                            // TimeSpan.TryParse 系では "80:00" とかを解釈しないので自前処理する
                            thinking_time =
                                TimeSpan.FromMinutes(double.Parse(mKif.Groups[5].Value)) +
                                TimeSpan.FromSeconds(double.Parse(mKif.Groups[6].Value));
                            total_time =
                                TimeSpan.FromHours(double.Parse(mKif.Groups[7].Value)) +
                                TimeSpan.FromMinutes(double.Parse(mKif.Groups[8].Value)) +
                                TimeSpan.FromSeconds(double.Parse(mKif.Groups[9].Value));
                        }
                        var turn = Tree.position.sideToMove;
                        times.Players[(int)turn] = times.Players[(int)turn].Create(
                            timeSettings.Player(turn),
                            thinking_time, thinking_time,
                            total_time /*消費時間は棋譜に記録されているものをそのまま使用する*/
                            /*残り時間は棋譜上に記録されていない*/
                            );
                        Tree.AddNode(move, times.Clone());
                        if (move.IsOk())
                            Tree.DoMove(move);
                        goto nextline;
                    }

                    // KI2形式検出
                    var mKi2 = rKi2.Matches(line);
                    if (mKi2.Count > 0)
                    {
                        if (!isBody)
                        {
                            var headRes = lazyHead();
                            if (headRes != string.Empty)
                                return headRes;
                        }
                        foreach (Match m in mKi2)
                        {
                            var move = Tree.position.FromKif(m.Groups[0].Value);
                            if (move == Move.NONE)
                                throw new KifuException("指し手を解析できませんでした。", line);
                            Tree.AddNode(move, KifuMoveTimes.Zero);
                            if (!Tree.position.IsLegal(move))
                                // これだと不正着手後の棋譜コメントを取れないがとりあえず解析を中止する
                                throw new KifuException($"不正着手を検出しました。", line);
                            if (move.IsOk())
                                Tree.DoMove(move);
                        }
                        goto nextline;
                    }

                    var mSpecial = rSpecial.Match(line);
                    if (mSpecial.Success)
                    {
                        var move = Move.NONE;
                        var reason = mSpecial.Groups[2].Value;
                        switch (reason)
                        {
                            case "で先手の勝ち":
                            case "で下手の勝ち":
                                move = Tree.position.sideToMove == Color.BLACK ?
                                    Move.ILLEGAL_ACTION_WIN:
                                    Move.RESIGN;
                                break;
                            case "で後手の勝ち":
                            case "で上手の勝ち":
                                move = Tree.position.sideToMove == Color.WHITE ?
                                    Move.ILLEGAL_ACTION_WIN:
                                    Move.RESIGN;
                                break;
                            case "で先手の反則勝ち":
                            case "で下手の反則勝ち":
                            case "で後手の反則負け":
                            case "で上手の反則負け":
                                move = Tree.position.sideToMove == Color.BLACK ?
                                    Move.ILLEGAL_ACTION_WIN:
                                    Move.ILLEGAL_ACTION_LOSE;
                                break;
                            case "で後手の反則勝ち":
                            case "で上手の反則勝ち":
                            case "で先手の反則負け":
                            case "で下手の反則負け":
                                move = Tree.position.sideToMove == Color.WHITE ?
                                    Move.ILLEGAL_ACTION_WIN:
                                    Move.ILLEGAL_ACTION_LOSE;
                                break;
                            case "で時間切れにより先手の勝ち":
                            case "で時間切れにより後手の勝ち":
                            case "で時間切れにより上手の勝ち":
                            case "で時間切れにより下手の勝ち":
                                move = Move.TIME_UP;
                                break;
                            case "で中断":
                                move = Move.INTERRUPT;
                                break;
                            case "で持将棋":
                                move = Move.MAX_MOVES_DRAW;
                                break;
                            case "で千日手":
                                move = Move.REPETITION_DRAW;
                                break;
                            case "詰":
                            case "詰み":
                            case "で詰":
                            case "で詰み":
                                move = Move.MATED;
                                break;
                        }
                        if (move != Move.NONE)
                            Tree.AddNode(move, KifuMoveTimes.Zero);
                    }

                    nextline:;
                    continue;
                }

                if (!isBody)
                {
                    var headRes = lazyHead();
                    if (headRes != string.Empty) return headRes;
                }
            } catch (Exception e)
            {
                return $"棋譜読み込みエラー : {lineNo}行目。\n{e.Message}";
            }

            return null;
        }

        /// <summary>
        /// KIF形式の文字列に変換
        /// これをこのままファイルに書き出すとKIF形式のファイルになる。
        /// </summary>
        /// <returns></returns>
        private string ToKifString()
        {
            var sb = new StringBuilder();

            // Kifu for Windows V7 ( http://kakinoki.o.oo7.jp/Kifuw7.htm ) 向けのヘッダ、これが無いとUTF-8形式の棋譜と認識して貰えない
            sb.AppendLine("#KIF version=2.0 encoding=UTF-8");

            // 手合割 / 局面図
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    sb.AppendLine("手合割：平手");
                    break;
                case BoardType.HandicapKyo:
                    sb.AppendLine("手合割：香落ち");
                    break;
                case BoardType.HandicapRightKyo:
                    sb.AppendLine("手合割：右香落ち");
                    break;
                case BoardType.HandicapKaku:
                    sb.AppendLine("手合割：角落ち");
                    break;
                case BoardType.HandicapHisya:
                    sb.AppendLine("手合割：飛車落ち");
                    break;
                case BoardType.HandicapHisyaKyo:
                    sb.AppendLine("手合割：飛香落ち");
                    break;
                case BoardType.Handicap2:
                    sb.AppendLine("手合割：二枚落ち");
                    break;
                case BoardType.Handicap3:
                    sb.AppendLine("手合割：三枚落ち");
                    break;
                case BoardType.Handicap4:
                    sb.AppendLine("手合割：四枚落ち");
                    break;
                case BoardType.Handicap5:
                    sb.AppendLine("手合割：五枚落ち");
                    break;
                case BoardType.HandicapLeft5:
                    sb.AppendLine("手合割：左五枚落ち");
                    break;
                case BoardType.Handicap6:
                    sb.AppendLine("手合割：六枚落ち");
                    break;
                case BoardType.Handicap8:
                    sb.AppendLine("手合割：八枚落ち");
                    break;
                case BoardType.Handicap10:
                    sb.AppendLine("手合割：十枚落ち");
                    break;
                default:
                    sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));
                    break;
            }

            // 先手対局者名
            if (KifuHeader.header_dic.ContainsKey("先手"))
                sb.AppendLine($"先手：{KifuHeader.PlayerNameBlack}");
            else if (KifuHeader.header_dic.ContainsKey("下手"))
                sb.AppendLine($"下手：{KifuHeader.PlayerNameBlack}");
            else switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    sb.AppendLine($"先手：{KifuHeader.PlayerNameBlack}");
                    break;
                default:
                    sb.AppendLine($"下手：{KifuHeader.PlayerNameBlack}");
                    break;
            }

            // 後手対局者名
            if (KifuHeader.header_dic.ContainsKey("後手"))
                sb.AppendLine($"後手：{KifuHeader.PlayerNameWhite}");
            else if (KifuHeader.header_dic.ContainsKey("上手"))
                sb.AppendLine($"上手：{KifuHeader.PlayerNameWhite}");
            else switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    sb.AppendLine($"後手：{KifuHeader.PlayerNameWhite}");
                    break;
                default:
                    sb.AppendLine($"上手：{KifuHeader.PlayerNameWhite}");
                    break;
            }

            // その他ヘッダ
            foreach (var key in KifuHeader.header_dic.Keys)
            {
                switch (key)
                {
                    case "先手":
                    case "後手":
                    case "上手":
                    case "下手":
                    case "手合割":
                        break;
                    default:
                        sb.AppendLine($"{key}：{KifuHeader.header_dic[key]}");
                        break;
                }
            }

            string[] sides;
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    sides = new string[] { "先手", "後手" };
                    break;
                default:
                    sides = new string[] { "下手", "上手" };
                    break;
            }

            sb.AppendLine("手数----指手---------消費時間--");

            var stack = new Stack<Node>();
            bool endNode = false;
            while (!endNode || stack.Count != 0)
            {
                int select = 0;
                if (endNode)
                {
                    endNode = false;
                    // 次の分岐まで巻き戻して、また出力していく。

                    var node = stack.Pop();

                    sb.AppendLine();
                    sb.AppendLine($"変化：{node.ply}手");

                    while (node.ply < Tree.gamePly)
                        Tree.UndoMove();

                    select = node.select;
                    goto SELECT;
                }

                int count = Tree.currentNode.moves.Count;
                if (count == 0)
                {
                    // ここで指し手終わっとる。終端ノードであるな。
                    endNode = true;
                    continue;
                }

                // このnodeの分岐の数
                if (count != 1)
                {
                    // あとで分岐しないといけないので残りをstackに記録しておく
                    for(int i = 1; i < count; ++i)
                        stack.Push(new Node(Tree.gamePly, i));
                }

                SELECT:;
                var move = Tree.currentNode.moves[select];

                Move m = move.nextMove;
                string mes;

                if (m.IsSpecial())
                {
                    // 特殊な指し手なら、それを出力して終わり。

                    endNode = true;

                    switch (m)
                    {
                        case Move.MATED:
                            mes = "詰み";
                            break;
                        case Move.INTERRUPT:
                            mes = "中断";
                            break;
                        case Move.REPETITION_WIN:
                            mes = "王手連続千日手";
                            break;
                        case Move.REPETITION_DRAW:
                            mes = "千日手";
                            break;
                        case Move.WIN:
                            mes = "勝ち宣言";
                            break;
                        case Move.WIN_THEM: // トライルールに対応するものがKIF形式にないので仕方がない。
                            mes = "勝ち宣言";
                            break;
                        case Move.MAX_MOVES_DRAW:
                            mes = "最大手数超過";
                            break;
                        case Move.RESIGN:
                            mes = "投了";
                            break;
                        case Move.TIME_UP:
                            mes = "時間切れ";
                            break;
                        default:
                            // mes = ""; // これ書き出してしまうと読み込み時に解析できない不正な棋譜になってしまう。注意すべき。
                            //break;

                            goto NextMove;
                    }
                }
                else
                {
                    mes = Tree.position.ToKif(m);
                }

                {
                    var mesEaw = EastAsianWidth.legacyWidth(mes);
                    var padEaw = 14;
                    if (mesEaw < padEaw)
                        mes = mes.PadRight(padEaw - mesEaw + mes.Length);
                }

                var turn = Tree.position.sideToMove;
                var time1 = move.kifuMoveTimes.Player(turn).ThinkingTime;
                var time2 = move.kifuMoveTimes.Player(turn).TotalTime;
                var time_string1 = $"{Math.Truncate(time1.TotalMinutes),2:0}:{time1:ss}";
                var time_string2 = $"{Math.Truncate(time2.TotalHours):00}:{time2:mm\\:ss}";

                sb.AppendLine($"{Tree.gamePly,3} {mes}({time_string1}/{time_string2})");

            NextMove:;
                if (m.IsSpecial())
                {
                    switch (m)
                    {
                        case Move.RESIGN:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.Not().ToInt()]}の勝ち");
                            break;
                        case Move.ILLEGAL_ACTION_WIN:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.Not().ToInt()]}の反則負け");
                            break;
                        case Move.ILLEGAL_ACTION_LOSE:
                        case Move.ILLEGAL_MOVE:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.ToInt()]}の反則負け");
                            break;
                        case Move.WIN:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.ToInt()]}の勝ち");
                            break;
                        case Move.WIN_THEM: // トライしたので一つ前の手番側の勝ち
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.Not().ToInt()]}の勝ち");
                            break;
                        case Move.TIME_UP:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で時間切れにより{sides[Tree.position.sideToMove.Not().ToInt()]}の勝ち");
                            break;
                        case Move.INTERRUPT:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で中断");
                            break;
                        case Move.REPETITION_DRAW:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で千日手");
                            break;
                        case Move.MAX_MOVES_DRAW:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で持将棋");
                            break;
                        case Move.MATED:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で詰み");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Tree.DoMove(move);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// KIF2形式の文字列に変換。
        /// これをこのままファイルに書き出すとKIF2形式のファイルになる。
        /// </summary>
        /// <returns></returns>
        private string ToKi2String()
        {
            var sb = new StringBuilder();

            // Kifu for Windows V7 ( http://kakinoki.o.oo7.jp/Kifuw7.htm ) 向けのヘッダ、これが無いとUTF-8形式の棋譜と認識して貰えない
            sb.AppendLine("#KI2 version=2.0 encoding=UTF-8");

            // 手合割 / 局面図
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    sb.AppendLine("手合割：平手");
                    break;
                case BoardType.HandicapKyo:
                    sb.AppendLine("手合割：香落ち");
                    break;
                case BoardType.HandicapRightKyo:
                    sb.AppendLine("手合割：右香落ち");
                    break;
                case BoardType.HandicapKaku:
                    sb.AppendLine("手合割：角落ち");
                    break;
                case BoardType.HandicapHisya:
                    sb.AppendLine("手合割：飛車落ち");
                    break;
                case BoardType.HandicapHisyaKyo:
                    sb.AppendLine("手合割：飛香落ち");
                    break;
                case BoardType.Handicap2:
                    sb.AppendLine("手合割：二枚落ち");
                    break;
                case BoardType.Handicap3:
                    sb.AppendLine("手合割：三枚落ち");
                    break;
                case BoardType.Handicap4:
                    sb.AppendLine("手合割：四枚落ち");
                    break;
                case BoardType.Handicap5:
                    sb.AppendLine("手合割：五枚落ち");
                    break;
                case BoardType.HandicapLeft5:
                    sb.AppendLine("手合割：左五枚落ち");
                    break;
                case BoardType.Handicap6:
                    sb.AppendLine("手合割：六枚落ち");
                    break;
                case BoardType.Handicap8:
                    sb.AppendLine("手合割：八枚落ち");
                    break;
                case BoardType.Handicap10:
                    sb.AppendLine("手合割：十枚落ち");
                    break;
                default:
                    sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));
                    break;
            }

            // 先手対局者名
            if (KifuHeader.header_dic.ContainsKey("先手"))
                sb.AppendLine($"先手：{KifuHeader.PlayerNameBlack}");
            else if (KifuHeader.header_dic.ContainsKey("下手"))
                sb.AppendLine($"下手：{KifuHeader.PlayerNameBlack}");
            else switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    sb.AppendLine($"先手：{KifuHeader.PlayerNameBlack}");
                    break;
                default:
                    sb.AppendLine($"下手：{KifuHeader.PlayerNameBlack}");
                    break;
            }

            // 後手対局者名
            if (KifuHeader.header_dic.ContainsKey("後手"))
                sb.AppendLine($"後手：{KifuHeader.PlayerNameWhite}");
            else if (KifuHeader.header_dic.ContainsKey("上手"))
                sb.AppendLine($"上手：{KifuHeader.PlayerNameWhite}");
            else switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    sb.AppendLine($"後手：{KifuHeader.PlayerNameWhite}");
                    break;
                default:
                    sb.AppendLine($"上手：{KifuHeader.PlayerNameWhite}");
                    break;
            }

            // その他ヘッダ
            foreach (var key in KifuHeader.header_dic.Keys)
            {
                switch (key)
                {
                    case "手合割":
                    case "先手":
                    case "後手":
                    case "上手":
                    case "下手":
                        break;
                    default:
                        sb.AppendLine($"{key}：{KifuHeader.header_dic[key]}");
                        break;
                }
            }

            string[] sides;
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    sides = new string[] { "先手", "後手" };
                    break;
                default:
                    sides = new string[] { "下手", "上手" };
                    break;
            }

            var lineEaw = 0;
            var stack = new Stack<Node>();
            bool endNode = false;
            while (!endNode || stack.Count != 0)
            {
                int select = 0;
                if (endNode)
                {
                    endNode = false;
                    // 次の分岐まで巻き戻して、また出力していく。

                    var node = stack.Pop();

                    sb.AppendLine();
                    sb.AppendLine($"変化：{node.ply}");
                    lineEaw = 0;

                    while (node.ply < Tree.gamePly)
                        Tree.UndoMove();

                    select = node.select;
                    goto SELECT;
                }

                int count = Tree.currentNode.moves.Count;
                if (count == 0)
                {
                    // ここで指し手終わっとる。終端ノードであるな。
                    endNode = true;
                    continue;
                }

                // このnodeの分岐の数
                if (count != 1)
                {
                    // あとで分岐しないといけないので残りをstackに記録しておく
                    for(int i = 1; i < count; ++i)
                        stack.Push(new Node(Tree.gamePly, i));
                }

                SELECT:;
                var move = Tree.currentNode.moves[select];

                Move m = move.nextMove;
                string mes;

                if (m.IsSpecial())
                {
                    // 特殊な指し手なら、それを出力して終わり。

                    endNode = true;

                    lineEaw = 0;
                    sb.AppendLine();
                    switch (m)
                    {
                        case Move.RESIGN:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.Not().ToInt()]}の勝ち");
                            break;
                        case Move.ILLEGAL_ACTION_WIN:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.Not().ToInt()]}の反則負け");
                            break;
                        case Move.ILLEGAL_ACTION_LOSE:
                        case Move.ILLEGAL_MOVE:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.ToInt()]}の反則負け");
                            break;
                        case Move.WIN:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.ToInt()]}の勝ち");
                            break;
                        case Move.WIN_THEM: // トライしたので一つ前の手番側の勝ち
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で{sides[Tree.position.sideToMove.Not().ToInt()]}の勝ち");
                            break;
                        case Move.TIME_UP:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で時間切れにより{sides[Tree.position.sideToMove.Not().ToInt()]}の勝ち");
                            break;
                        case Move.INTERRUPT:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で中断");
                            break;
                        case Move.REPETITION_DRAW:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で千日手");
                            break;
                        case Move.MAX_MOVES_DRAW:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で持将棋");
                            break;
                        case Move.MATED:
                            sb.AppendLine($"まで{Tree.gamePly - 1}手で詰み");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    mes = Tree.position.ToKi2C(m);
                    Tree.DoMove(move);
                    if (lineEaw >= 60)
                    {
                        sb.AppendLine(mes);
                        lineEaw = 0;
                    }
                    else
                    {
                        var mesEaw = EastAsianWidth.legacyWidth(mes);
                        var padEaw = 12;
                        lineEaw += Math.Max(mesEaw, padEaw);
                        if (mesEaw < padEaw)
                            mes = mes.PadRight(padEaw - mesEaw + mes.Length);
                        sb.Append(mes);
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private string ToKifPositionString(KifuFileType kt = KifuFileType.KIF)
        {
            var sb = new StringBuilder();

            // Kifu for Windows V7 ( http://kakinoki.o.oo7.jp/Kifuw7.htm ) 向けのヘッダ、これが無いとUTF-8形式の棋譜と認識して貰えない
            switch (kt)
            {
                case KifuFileType.KIF:
                    sb.AppendLine("#KIF version=2.0 encoding=UTF-8");
                    break;
                case KifuFileType.KI2:
                    sb.AppendLine("#KI2 version=2.0 encoding=UTF-8");
                    break;
            }

            // 局面出力
            sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));

            // 先手対局者名
            if (KifuHeader.header_dic.ContainsKey("先手"))
                sb.AppendLine($"先手：{KifuHeader.PlayerNameBlack}");
            else if (KifuHeader.header_dic.ContainsKey("下手"))
                sb.AppendLine($"下手：{KifuHeader.PlayerNameBlack}");
            else switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    sb.AppendLine($"先手：{KifuHeader.PlayerNameBlack}");
                    break;
                default:
                    sb.AppendLine($"下手：{KifuHeader.PlayerNameBlack}");
                    break;
            }

            // 後手対局者名
            if (KifuHeader.header_dic.ContainsKey("後手"))
                sb.AppendLine($"後手：{KifuHeader.PlayerNameWhite}");
            else if (KifuHeader.header_dic.ContainsKey("上手"))
                sb.AppendLine($"上手：{KifuHeader.PlayerNameWhite}");
            else switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                case BoardType.Others:
                    sb.AppendLine($"後手：{KifuHeader.PlayerNameWhite}");
                    break;
                default:
                    sb.AppendLine($"上手：{KifuHeader.PlayerNameWhite}");
                    break;
            }

            // その他ヘッダ情報
            foreach (var key in KifuHeader.header_dic.Keys)
            {
                switch (key)
                {
                    case "先手":
                    case "後手":
                    case "上手":
                    case "下手":
                    case "手合割":
                        break;
                    default:
                        sb.AppendLine($"{key}：{KifuHeader.header_dic[key]}");
                        break;
                }
            }

            return sb.ToString();
        }

    }
}
