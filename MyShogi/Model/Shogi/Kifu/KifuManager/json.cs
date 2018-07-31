using System;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Converter;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// jsonの読み書き
    /// </summary>
    public partial class KifuManager
    {
        // JSON形式棋譜の読み込み
        private string FromJsonString(string content, KifuFileType kf)
        {
            switch (kf)
            {
                case KifuFileType.JKF:
                    return FromJkfString(content, kf);
                case KifuFileType.JSON:
                    return FromLiveJsonString(content, kf);
                default:
                    return string.Empty;
            }
        }

        // JSON Kifu Format
        private string FromJkfString(string content, KifuFileType kf)
        {
            try
            {
                var CSA_PIECE = new string[] {
                    "  ","FU","KY","KE","GI","KA","HI","KI",
                    "OU","TO","NY","NK","NG","UM","RY","QU",
                    "  ","FU","KY","KE","GI","KA","HI","KI",
                    "OU","TO","NY","NK","NG","UM","RY","QU",
                };
                var jsonObj = JkfUtil.FromString(content);
                if (jsonObj == null)
                {
                    return "有効なデータが得られませんでした";
                }
                if (jsonObj.header != null)
                {
                    foreach (var key in jsonObj.header.Keys)
                    {
                        var trimedKey = key.Trim(' ', '\t', '\n', '\r', '　', '\x0b', '\x00');
                        KifuHeader.header_dic[trimedKey] = jsonObj.header[key];
                    }
                }

                // Treeに局面をセットする
                void SetTree(BoardType bt)
                {
                    Tree.rootSfen = bt.ToSfen();
                    Tree.position.SetSfen(Tree.rootSfen);
                    Tree.rootBoardType = bt;
                }

                if (jsonObj.initial != null)
                {
                    switch (jsonObj.initial.preset)
                    {
                        case "HIRATE": SetTree(BoardType.NoHandicap); break;
                        case "KY": SetTree(BoardType.HandicapKyo); break;
                        case "KY_R": SetTree(BoardType.HandicapRightKyo); break;
                        case "KA": SetTree(BoardType.HandicapKaku); break;
                        case "HI": SetTree(BoardType.HandicapHisya); break;
                        case "HIKY": SetTree(BoardType.HandicapHisyaKyo); break;
                        case "2": SetTree(BoardType.Handicap2); break;
                        case "3": SetTree(BoardType.Handicap3); break;
                        case "4": SetTree(BoardType.Handicap4); break;
                        case "5": SetTree(BoardType.Handicap5); break;
                        case "5_L": SetTree(BoardType.HandicapLeft5); break;
                        case "6": SetTree(BoardType.Handicap6); break;
                        case "8": SetTree(BoardType.Handicap8); break;
                        case "10": SetTree(BoardType.Handicap10); break;

                        case "OTHER":
                            Tree.rootBoardType = BoardType.Others;
                            if (jsonObj.initial.data == null)
                                return "初期局面が指定されていません";
                            var color = jsonObj.initial.data.color == 0 ? Color.BLACK : Color.WHITE;
                            var board = new Piece[81];
                            for (File f = File.FILE_1; f <= File.FILE_9; ++f)
                            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
                            {
                                var sqi = Util.MakeSquare(f, r).ToInt();
                                var p = jsonObj.initial.data.board[f.ToInt(), r.ToInt()];
                                switch (p.color)
                                {
                                    case 0:
                                        switch (p.kind)
                                        {
                                            case "FU": board[sqi] = Piece.B_PAWN; break;
                                            case "KY": board[sqi] = Piece.B_LANCE; break;
                                            case "KE": board[sqi] = Piece.B_KNIGHT; break;
                                            case "GI": board[sqi] = Piece.B_SILVER; break;
                                            case "KA": board[sqi] = Piece.B_BISHOP; break;
                                            case "HI": board[sqi] = Piece.B_ROOK; break;
                                            case "KI": board[sqi] = Piece.B_GOLD; break;
                                            case "OU": board[sqi] = Piece.B_KING; break;
                                            case "TO": board[sqi] = Piece.B_PRO_PAWN; break;
                                            case "NY": board[sqi] = Piece.B_PRO_LANCE; break;
                                            case "NK": board[sqi] = Piece.B_PRO_KNIGHT; break;
                                            case "NG": board[sqi] = Piece.B_PRO_SILVER; break;
                                            case "UM": board[sqi] = Piece.B_HORSE; break;
                                            case "RY": board[sqi] = Piece.B_DRAGON; break;
                                            default: board[sqi] = Piece.NO_PIECE; break;
                                        }
                                        break;
                                    case 1:
                                        switch (p.kind)
                                        {
                                            case "FU": board[sqi] = Piece.W_PAWN; break;
                                            case "KY": board[sqi] = Piece.W_LANCE; break;
                                            case "KE": board[sqi] = Piece.W_KNIGHT; break;
                                            case "GI": board[sqi] = Piece.W_SILVER; break;
                                            case "KA": board[sqi] = Piece.W_BISHOP; break;
                                            case "HI": board[sqi] = Piece.W_ROOK; break;
                                            case "KI": board[sqi] = Piece.W_GOLD; break;
                                            case "OU": board[sqi] = Piece.W_KING; break;
                                            case "TO": board[sqi] = Piece.W_PRO_PAWN; break;
                                            case "NY": board[sqi] = Piece.W_PRO_LANCE; break;
                                            case "NK": board[sqi] = Piece.W_PRO_KNIGHT; break;
                                            case "NG": board[sqi] = Piece.W_PRO_SILVER; break;
                                            case "UM": board[sqi] = Piece.W_HORSE; break;
                                            case "RY": board[sqi] = Piece.W_DRAGON; break;
                                            default: board[sqi] = Piece.NO_PIECE; break;
                                        }
                                        break;
                                    default:
                                        board[sqi] = Piece.NO_PIECE;
                                        break;
                                }
                            }
                            var hands = new Hand[2] { Hand.ZERO, Hand.ZERO };
                            if (jsonObj.initial.data.hands != null && jsonObj.initial.data.hands.Count >= 2)
                            foreach (var c in new Color[] { Color.BLACK, Color.WHITE })
                            {
                                if (jsonObj.initial.data.hands[c.ToInt()] != null)
                                foreach (var p in new Piece[] { Piece.PAWN, Piece.LANCE, Piece.KNIGHT, Piece.SILVER, Piece.GOLD, Piece.BISHOP, Piece.ROOK })
                                {
                                    int value;
                                    if (jsonObj.initial.data.hands[c.ToInt()].TryGetValue(CSA_PIECE[p.ToInt()], out value))
                                    {
                                        hands[c.ToInt()].Add(p, value);
                                    }
                                }
                            }
                            Tree.rootSfen = Position.SfenFromRawdata(board, hands, color, 1);
                            Tree.position.SetSfen(Tree.rootSfen);
                            break;
                        default:
                            return "初期局面が不明です";
                    }
                }
                if (jsonObj.moves != null)
                {
                    Move m = Move.NONE;
                    foreach (var jkfMove in jsonObj.moves)
                    {
                        TimeSpan spend = (jkfMove.time != null && jkfMove.time.now != null) ?
                            new TimeSpan(jkfMove.time.now.h ?? 0, jkfMove.time.now.m, jkfMove.time.now.s):
                            TimeSpan.Zero;
                        if (!string.IsNullOrWhiteSpace(jkfMove.special))
                        {
                            switch (jkfMove.special)
                            {
                                case "TORYO":           m = Move.RESIGN; break;
                                case "CHUDAN":          m = Move.INTERRUPT; break;
                                case "SENNICHITE":      m = Move.REPETITION_DRAW; break;
                                case "TIME_UP":         m = Move.TIME_UP; break;
                                case "JISHOGI":         m = Move.MAX_MOVES_DRAW; break;
                                case "KACHI"  :         m = Move.WIN; break;
                                case "HIKIWAKE":        m = Move.DRAW; break;
                                case "TSUMI":           m = Move.MATED; break;
                                case "ILLEGAL_MOVE"   : m = Move.ILLEGAL_MOVE; break;
                                case "+ILLEGAL_ACTION": m = Tree.position.sideToMove == Color.BLACK ? Move.ILLEGAL_ACTION_LOSE : Move.ILLEGAL_ACTION_WIN; break;
                                case "-ILLEGAL_ACTION": m = Tree.position.sideToMove == Color.BLACK ? Move.ILLEGAL_ACTION_WIN : Move.ILLEGAL_ACTION_LOSE; break;
                                // 以下、適切な変換先不明
                                case "ERROR":
                                case "FUZUMI":
                                case "MATTA":
                                default: m = Move.NONE; break;
                            }
                        }
                        else if (jkfMove.move == null)
                            m = Move.NONE;
                        else if (jkfMove.move.to == null)
                            m = Move.NONE;
                        else if (jkfMove.move.from == null)
                        {
                            File f = (File)(jkfMove.move.to.x - 1);
                            Rank r = (Rank)(jkfMove.move.to.y - 1);
                            if (f < File.FILE_1 || f > File.FILE_9 || r < Rank.RANK_1 || r > Rank.RANK_9)
                                m = Move.NONE;
                            else
                            {
                                Square sq = Util.MakeSquare(f, r);
                                switch (jkfMove.move.piece)
                                {
                                    case "FU": m = Util.MakeMoveDrop(Piece.PAWN, sq); break;
                                    case "KY": m = Util.MakeMoveDrop(Piece.LANCE, sq); break;
                                    case "KE": m = Util.MakeMoveDrop(Piece.KNIGHT, sq); break;
                                    case "GI": m = Util.MakeMoveDrop(Piece.SILVER, sq); break;
                                    case "KI": m = Util.MakeMoveDrop(Piece.GOLD, sq); break;
                                    case "KA": m = Util.MakeMoveDrop(Piece.BISHOP, sq); break;
                                    case "HI": m = Util.MakeMoveDrop(Piece.ROOK, sq); break;
                                    default: m = Move.NONE; break;
                                }
                            }
                        }
                        else
                        {
                            File frF = (File)(jkfMove.move.from.x - 1);
                            Rank frR = (Rank)(jkfMove.move.from.y - 1);
                            File toF = (File)(jkfMove.move.to.x - 1);
                            Rank toR = (Rank)(jkfMove.move.to.y - 1);
                            if (
                                frF < File.FILE_1 || frF > File.FILE_9 ||
                                frR < Rank.RANK_1 || frR > Rank.RANK_9 ||
                                frF < File.FILE_1 || toF > File.FILE_9 ||
                                frR < Rank.RANK_1 || toR > Rank.RANK_9
                            )
                                m = Move.NONE;
                            else
                            {
                                Square frSq = Util.MakeSquare(frF, frR);
                                Square toSq = Util.MakeSquare(toF, toR);
                                if (jkfMove.move.promote == true)
                                    m = Util.MakeMovePromote(frSq, toSq);
                                else
                                    m = Util.MakeMove(frSq, toSq);
                            }
                        }
                        Tree.AddNode(m, KifuMoveTimes.Zero /*ToDo:あとでちゃんと書く*/ /* spend */);
                        if (m.IsSpecial() || !Tree.position.IsLegal(m))
                            break;
                        Tree.DoMove(m);
                    }
                }
                // ToDo: 分岐棋譜を読み込む
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return null;

        }

        // とりあえずJSON中継棋譜形式に部分対応
        private string FromLiveJsonString(string content, KifuFileType kf)
        {
            try
            {
                var times = KifuMoveTimes.Zero;
                var timeSettings = KifuTimeSettings.TimeLimitless.Clone();
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
                DateTime? lasttime = null;
                var jsonObj = LiveJsonUtil.FromString(content);
                if (jsonObj == null || jsonObj.data == null || jsonObj.data.Count == 0)
                {
                    return "有効なデータが得られませんでした。";
                }
                // 先頭のデータのみ読み込む
                var data = jsonObj.data[0];
                {
                    // 対局者名
                    if (data.side != "後手")
                    {
                        KifuHeader.PlayerNameBlack = data.player1;
                        KifuHeader.PlayerNameWhite = data.player2;
                    }
                    else
                    {
                        KifuHeader.PlayerNameBlack = data.player2;
                        KifuHeader.PlayerNameWhite = data.player1;
                    }
                }
                if (data.realstarttime != null)
                {
                    var starttime = epoch.AddMilliseconds((double)data.realstarttime);
                    lasttime = starttime;
                    Tree.RootKifuLog.moveTime = starttime;
                    Tree.rootNode.comment = starttime.ToString("o");
                }
                else if (!string.IsNullOrWhiteSpace(data.starttime))
                {
                    Tree.RootKifuLog.moveTime = DateTime.ParseExact(data.starttime, "s", null);
                }
                if (!string.IsNullOrWhiteSpace(data.timelimit) && int.TryParse(data.timelimit, out int time_limit))
                {
                    timeSettings = new KifuTimeSettings(
                        new KifuTimeSetting[]
                        {
                            new KifuTimeSetting() { Hour = 0, Minute = time_limit, Second = 0 },
                            new KifuTimeSetting() { Hour = 0, Minute = time_limit, Second = 0 },
                        },
                        false
                    );
                }
                if (!string.IsNullOrWhiteSpace(data.countdown) && int.TryParse(data.countdown, out int countdown))
                {
                    foreach (var players in timeSettings.Players)
                    {
                        if (players.TimeLimitless)
                        {
                            players.Hour = 0;
                            players.Minute = 0;
                            players.Second = 0;
                            players.TimeLimitless = false;
                        }
                        players.Byoyomi = countdown;
                        players.ByoyomiEnable = true;
                    }
                }
                // 残り時間の計算用。
                times = timeSettings.GetInitialKifuMoveTimes();
                Tree.SetKifuMoveTimes(times.Clone()); // root局面での残り時間の設定
                Tree.KifuTimeSettings = timeSettings.Clone();
                foreach (var kif in data.kif)
                {
                    Move move;
                    DateTime? time = null;
                    if (kif.time != null)
                    {
                        time = epoch.AddMilliseconds((double)kif.time);
                    }
                    // 特殊な着手
                    if (kif.frX == null || kif.frY == null || kif.toX == null || kif.toY == null || kif.prmt == null)
                    {
                        switch (kif.move)
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
                            case "持将棋":
                                move = Move.DRAW;
                                break;
                            default:
                                return "不明な終局理由です。";
                        }
                    }
                    // varidation
                    else if (kif.frX < 1 || kif.frX > 9 || kif.toY < 0 || kif.toY > 10)
                    {
                        return "無効な移動元座標を検出しました。";
                    }
                    else if (kif.toX < 1 || kif.toX > 9 || kif.toY < 1 || kif.toY > 9)
                    {
                        return "無効な移動先座標を検出しました。";
                    }
                    // 先手駒台からの着手
                    else if (kif.frY == 10)
                    {
                        Piece pc;
                        switch (kif.frX)
                        {
                            case 1: pc = Piece.PAWN; break;
                            case 2: pc = Piece.LANCE; break;
                            case 3: pc = Piece.KNIGHT; break;
                            case 4: pc = Piece.SILVER; break;
                            case 5: pc = Piece.GOLD; break;
                            case 6: pc = Piece.BISHOP; break;
                            case 7: pc = Piece.ROOK; break;
                            default:
                                return "先手の無効な駒打ちを検出しました。";
                        }
                        var toSq = Util.MakeSquare((File)(kif.toX - 1), (Rank)(kif.toY - 1));
                        move = Util.MakeMoveDrop(pc, toSq);
                    }
                    // 後手駒台からの着手
                    else if (kif.frY == 0)
                    {
                        Piece pc;
                        switch (kif.frX)
                        {
                            case 9: pc = Piece.PAWN; break;
                            case 8: pc = Piece.LANCE; break;
                            case 7: pc = Piece.KNIGHT; break;
                            case 6: pc = Piece.SILVER; break;
                            case 5: pc = Piece.GOLD; break;
                            case 4: pc = Piece.BISHOP; break;
                            case 3: pc = Piece.ROOK; break;
                            default:
                                return "後手の無効な駒打ちを検出しました。";
                        }
                        var toSq = Util.MakeSquare((File)(kif.toX - 1), (Rank)(kif.toY - 1));
                        move = Util.MakeMoveDrop(pc, toSq);
                    }
                    // 通常の着手
                    else
                    {
                        var frSq = Util.MakeSquare((File)(kif.frX - 1), (Rank)(kif.frY - 1));
                        var toSq = Util.MakeSquare((File)(kif.toX - 1), (Rank)(kif.toY - 1));
                        if (kif.prmt == 1)
                        {
                            move = Util.MakeMovePromote(frSq, toSq);
                        }
                        else
                        {
                            move = Util.MakeMove(frSq, toSq);
                        }
                    }
                    TimeSpan thinking_time = TimeSpan.FromSeconds((double)(kif.spend ?? 0));
                    TimeSpan realthinking_time = (time != null && lasttime != null) ? time.GetValueOrDefault().Subtract(lasttime.GetValueOrDefault()) : thinking_time;
                    var turn = Tree.position.sideToMove;
                    times.Players[(int)turn] = times.Players[(int)turn].Create(
                        timeSettings.Player(turn),
                        thinking_time, realthinking_time
                        );
                    // 棋譜ツリーへの追加処理
                    Tree.AddNode(move, times.Clone());
                    if (time != null)
                    {
                        lasttime = time;
                        var kifumove = Tree.currentNode.moves.Find((x) => x.nextMove == move);
                        kifumove.moveTime = (DateTime)time;
                        Tree.currentNode.comment = ((DateTime)time).ToString("o");
                    }
                    if (move.IsSpecial())
                    {
                        return null;
                    }
                    if (!Tree.position.IsLegal(move))
                    {
                        return $"{Tree.gamePly}手目で不正着手を検出しました。";
                    }
                    Tree.DoMove(move);
                    continue;
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return null;
        }

        private string ToJkfString()
        {
            Jkf jkf = new Jkf();
            jkf.header = new Dictionary<string, string>();
            foreach (var key in KifuHeader.header_dic.Keys)
            {
                jkf.header[key] = KifuHeader.header_dic[key];
            }
            jkf.initial = new Jkf.Initial();
            var CSA_PIECE = new string[] {
                "  ","FU","KY","KE","GI","KA","HI","KI",
                "OU","TO","NY","NK","NG","UM","RY","QU",
                "  ","FU","KY","KE","GI","KA","HI","KI",
                "OU","TO","NY","NK","NG","UM","RY","QU",
            };
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:       jkf.initial.preset = "HIRATE"; break;
                case BoardType.HandicapKyo:      jkf.initial.preset = "KY"; break;
                case BoardType.HandicapRightKyo: jkf.initial.preset = "KY_R"; break;
                case BoardType.HandicapKaku:     jkf.initial.preset = "KA"; break;
                case BoardType.HandicapHisya:    jkf.initial.preset = "HI"; break;
                case BoardType.HandicapHisyaKyo: jkf.initial.preset = "HIKY"; break;
                case BoardType.Handicap2:        jkf.initial.preset = "2"; break;
                case BoardType.Handicap3:        jkf.initial.preset = "3"; break;
                case BoardType.Handicap4:        jkf.initial.preset = "4"; break;
                case BoardType.Handicap5:        jkf.initial.preset = "5"; break;
                case BoardType.HandicapLeft5:    jkf.initial.preset = "5_L"; break;
                case BoardType.Handicap6:        jkf.initial.preset = "6"; break;
                case BoardType.Handicap8:        jkf.initial.preset = "8"; break;
                case BoardType.Handicap10:       jkf.initial.preset = "10"; break;
                default:
                    jkf.initial.preset = "OTHER";
                    jkf.initial.data = new Jkf.Data()
                    {
                        color = Tree.position.sideToMove == Color.BLACK ? 0 : 1,
                        board = new Jkf.Board[9,9],
                        hands = new List<Dictionary<string, int>> {
                            new Dictionary<string, int>(),
                            new Dictionary<string, int>(),
                        },
                    };
                    for (File f = File.FILE_1; f <= File.FILE_9; ++f)
                    for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
                    {
                        var p = Tree.position.PieceOn(Util.MakeSquare(f, r));
                        jkf.initial.data.board[f.ToInt(), r.ToInt()] = p == Piece.NO_PIECE ?
                            new Jkf.Board():
                            new Jkf.Board()
                            {
                                kind = CSA_PIECE[p.PieceType().ToInt()],
                                color = p.PieceColor() == Color.BLACK ? 0 : 1,
                            };
                    }
                    foreach (var c in new Color[] { Color.BLACK, Color.WHITE })
                    foreach (var p in new Piece[] { Piece.PAWN, Piece.LANCE, Piece.KNIGHT, Piece.SILVER, Piece.GOLD, Piece.BISHOP, Piece.ROOK })
                    {
                        jkf.initial.data.hands[c.ToInt()][CSA_PIECE[p.ToInt()]] = Tree.position.Hand(c).Count(p);
                    }
                    break;
            }
            {
                // 初期局面情報の出力
                var root = new Jkf.MoveFormat()
                {
                    comments = new List<string>(),
                };
                foreach (var line in Tree.rootNode.comment.Split('\n'))
                {
                    root.comments.Add(line);
                }
                foreach (var line in Tree.RootKifuLog.engineComment.Split('\n'))
                {
                    root.comments.Add(line);
                }

                // 棋譜の出力
                var outList = jkf.moves = new List<Jkf.MoveFormat>() { root };
                var inStack = new Stack<Node>();
                var outStack = new Stack<List<List<Jkf.MoveFormat>>>();
                var endNode = false;
                Func<KifMoveInfo, string> relStr = (moveInfo) =>
                {
                    string rel, beh;
                    switch (moveInfo.relative)
                    {
                        case KifMoveInfo.Relative.LEFT:     rel = "L"; break;
                        case KifMoveInfo.Relative.STRAIGHT: rel = "C"; break;
                        case KifMoveInfo.Relative.RIGHT:    rel = "R"; break;
                        default: rel = ""; break;
                    }
                    switch (moveInfo.behavior)
                    {
                        case KifMoveInfo.Behavior.FORWARD:  beh = "U"; break;
                        case KifMoveInfo.Behavior.SLIDE:    beh = "M"; break;
                        case KifMoveInfo.Behavior.BACKWARD: beh = "D"; break;
                        default: beh = ""; break;
                    }
                    // ToDo: 省略可能な駒打ちの場合に"H"と出力すべきか確認する
                    // 省略可能な駒打ちの場合でも"H"と出力
                    string drop = moveInfo.drop == KifMoveInfo.Drop.NONE ? "" : "H";
                    // 省略可能な駒打ちの場合は"H"と出力しない
                    //string drop = moveInfo.drop == KifMoveInfo.Drop.EXPLICIT ? "H" : "";
                    return rel + beh + drop;
                };

                while (!endNode || inStack.Count != 0)
                {
                    int select = 0;
                    var jkfMove = new Jkf.MoveFormat()
                    {
                        forks = new List<List<Jkf.MoveFormat>>(),
                    };

                    if (endNode)
                    {
                        endNode = false;
                        var inNode = inStack.Pop();
                        var outBranches = outStack.Pop();
                        outBranches.Add(outList = new List<Jkf.MoveFormat>());
                        while (inNode.ply < Tree.gamePly)
                            Tree.UndoMove();
                        select = inNode.select;
                    }
                    else
                    {
                        int count = Tree.currentNode.moves.Count;
                        if (count == 0)
                        {
                            endNode = true;
                            continue;
                        }
                        if (count != 1)
                        {
                            for (int i = 1; i < count; ++i)
                            {
                                inStack.Push(new Node(Tree.gamePly, i));
                                outStack.Push(jkfMove.forks);
                            }
                        }
                    }

                    var kifMove = Tree.currentNode.moves[select];
                    Move m = kifMove.nextMove;

                    if (m.IsSpecial())
                    {
                        endNode = true;
                        switch (m)
                        {
                            case Move.MATED:           jkfMove.special = "TYORYO"; break;
                            case Move.INTERRUPT:       jkfMove.special = "CHUDAN"; break;
                            case Move.REPETITION_WIN:  jkfMove.special = "SENNICHITE"; break;
                            case Move.REPETITION_DRAW: jkfMove.special = "SENNICHITE"; break;
                            case Move.WIN:             jkfMove.special = "KACHI"; break;
                            case Move.WIN_THEM:        jkfMove.special = "KACHI"; break; // ないので仕方がない..
                            case Move.MAX_MOVES_DRAW:  jkfMove.special = "JISHOGI"; break;
                            case Move.RESIGN:          jkfMove.special = "TORYO"; break;
                            case Move.TIME_UP:         jkfMove.special = "TIME_UP"; break;
                            default: continue;
                        }
                    }
                    else
                    {
                        var moveInfo = new KifMoveInfo(Tree.position, kifMove.nextMove);
                        jkfMove.move = new Jkf.Move()
                        {
                            color = Tree.position.sideToMove == Color.BLACK ? 0 : 1,
                            from = m.IsDrop() ? null : new Jkf.PlaceFormat()
                            {
                                x = m.From().ToFile().ToInt() + 1,
                                y = m.From().ToRank().ToInt() + 1,
                            },
                            to = new Jkf.PlaceFormat()
                            {
                                x = m.To().ToFile().ToInt() + 1,
                                y = m.To().ToRank().ToInt() + 1,
                            },
                            piece = CSA_PIECE[moveInfo.fromPc.ToInt()],
                            same = moveInfo.same,
                            promote =
                                (moveInfo.promote == KifMoveInfo.Promote.NONE) ? (bool?)null :
                                (moveInfo.promote == KifMoveInfo.Promote.PROMOTE),
                            capture = moveInfo.capPc == Piece.NO_PIECE ? null : CSA_PIECE[moveInfo.capPc.ToInt()],
                            relative = relStr(moveInfo),
                        };
                    }

                    var turn = Tree.position.sideToMove;
                    var kifuTime = kifMove.kifuMoveTimes.Player(turn);
                    var thinkingTime = kifuTime.ThinkingTime;
                    var totalTime = kifuTime.TotalTime;
                    jkfMove.time = new Jkf.Time()
                    {
                        now = new Jkf.TimeFormat()
                        {
                            h = thinkingTime.Days * 24 + thinkingTime.Hours,
                            m = thinkingTime.Minutes,
                            s = thinkingTime.Seconds,
                        },
                        total = new Jkf.TimeFormat()
                        {
                            h = totalTime.Days * 24 + totalTime.Hours,
                            m = totalTime.Minutes,
                            s = totalTime.Seconds,
                        },
                    };

                    outList.Add(jkfMove);

                }
            }
            return jkf.ToJson();
        }

        private static KifFormatterImmutableOptions livejsonkifformat = new KifFormatterImmutableOptions(ColorFormat.NONE, SquareFormat.FullWidthMix, SamePosFormat.ZEROsp, FromSqFormat.KI2);
        private string ToJsonString()
        {
            // 平手以外の出力は現状対応しない
            if (Tree.rootBoardType != BoardType.NoHandicap)
            {
                throw new KifuException("平手以外の初期局面には対応していません。");
            }
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
            var data = new LiveJson.Data();
            data.handicap = "平手";
            data.side = "先手";
            data.player1 = KifuHeader.PlayerNameBlack;
            data.player2 = KifuHeader.PlayerNameWhite;
            if (Tree.RootKifuLog != null && Tree.RootKifuLog.moveTime != null)
            {
                data.realstarttime = (long)Tree.RootKifuLog.moveTime.Subtract(epoch).TotalMilliseconds;
                data.starttime = Tree.RootKifuLog.moveTime.ToString("s");
            }
            var kifList = new List<LiveJson.Kif>();
            while (Tree.currentNode.moves.Count > 0)
            {
                var kifMove = Tree.currentNode.moves[0];
                var kif = new LiveJson.Kif();
                if (kifMove.moveTime != null)
                {
                    kif.time = (long)(kifMove.moveTime.Subtract(epoch).TotalMilliseconds);
                }
                var turn = Tree.position.sideToMove;
                var thinkingTime = kifMove.kifuMoveTimes.Player(turn).ThinkingTime;
                kif.spend = (long)thinkingTime.TotalSeconds;
                var nextMove = kifMove.nextMove;
                if (nextMove.IsSpecial())
                {
                    if (kifMove.moveTime != null)
                    {
                        data.endtime = kifMove.moveTime.ToString("s");
                    }
                    switch (nextMove)
                    {
                        case Move.RESIGN:
                            kif.move = "投了";
                            break;
                        case Move.INTERRUPT:
                            kif.move = "中断";
                            break;
                        default:
                            goto jailbreak;
                    }
                }
                else if (nextMove.IsDrop())
                {
                    var droppedPiece = nextMove.DroppedPiece();
                    switch (droppedPiece)
                    {
                        case Piece.PAWN:   kif.type = "FU"; break;
                        case Piece.LANCE:  kif.type = "KYO"; break;
                        case Piece.KNIGHT: kif.type = "KEI"; break;
                        case Piece.SILVER: kif.type = "GIN"; break;
                        case Piece.GOLD:   kif.type = "KIN"; break;
                        case Piece.BISHOP: kif.type = "KAKU"; break;
                        case Piece.ROOK:   kif.type = "HI"; break;
                    }
                    var state = Tree.position.State();
                    kif.move = livejsonkifformat.format(Tree.position, nextMove, state != null ? state.lastMove : Move.NONE);
                    if (Tree.position.sideToMove == Color.BLACK)
                    {
                        kif.frY = 10;
                        switch (droppedPiece)
                        {
                            case Piece.PAWN:   kif.frX = 1; break;
                            case Piece.LANCE:  kif.frX = 2; break;
                            case Piece.KNIGHT: kif.frX = 3; break;
                            case Piece.SILVER: kif.frX = 4; break;
                            case Piece.GOLD:   kif.frX = 5; break;
                            case Piece.BISHOP: kif.frX = 6; break;
                            case Piece.ROOK:   kif.frY = 7; break;
                        }
                    }
                    else
                    {
                        kif.frY = 0;
                        switch (droppedPiece)
                        {
                            case Piece.PAWN:   kif.frX = 9; break;
                            case Piece.LANCE:  kif.frX = 8; break;
                            case Piece.KNIGHT: kif.frX = 7; break;
                            case Piece.SILVER: kif.frX = 6; break;
                            case Piece.GOLD:   kif.frX = 5; break;
                            case Piece.BISHOP: kif.frX = 4; break;
                            case Piece.ROOK:   kif.frY = 3; break;
                        }
                    }
                    kif.toX = nextMove.To().ToFile().ToInt() + 1;
                    kif.toY = nextMove.To().ToFile().ToInt() + 1;
                    kif.prmt = 0;
                }
                else
                {
                    var state = Tree.position.State();
                    kif.move = livejsonkifformat.format(Tree.position, nextMove, state != null ? state.lastMove : Move.NONE);
                    kif.frX = nextMove.From().ToFile().ToInt() + 1;
                    kif.frY = nextMove.From().ToRank().ToInt() + 1;
                    kif.toX = nextMove.To().ToFile().ToInt() + 1;
                    kif.toY = nextMove.To().ToRank().ToInt() + 1;
                    kif.prmt = nextMove.IsPromote() ? 1 : 0;
                    switch (Tree.position.PieceOn(nextMove.From()).PieceType())
                    {
                        case Piece.PAWN:       kif.type = "FU"; break;
                        case Piece.LANCE:      kif.type = "KYO"; break;
                        case Piece.KNIGHT:     kif.type = "KEI"; break;
                        case Piece.SILVER:     kif.type = "GIN"; break;
                        case Piece.GOLD:       kif.type = "KIN"; break;
                        case Piece.BISHOP:     kif.type = "KAKU"; break;
                        case Piece.ROOK:       kif.type = "HI"; break;
                        case Piece.KING:       kif.type = "OU"; break;
                        case Piece.PRO_PAWN:   kif.type = "NFU"; break;
                        case Piece.PRO_LANCE:  kif.type = "NKYO"; break;
                        case Piece.PRO_KNIGHT: kif.type = "NKEI"; break;
                        case Piece.PRO_SILVER: kif.type = "NGIN"; break;
                        case Piece.HORSE:      kif.type = "NKAKU"; break;
                        case Piece.DRAGON:     kif.type = "NHI"; break;
                    }
                }
                kif.num = Tree.gamePly;
                kifList.Add(kif);
                if (nextMove.IsSpecial())
                {
                    break;
                }
                Tree.DoMove(kifMove);
            }
            jailbreak:;
            data.kif = kifList;
            data.v = kifList.Count;
            data.breaktime = new List<LiveJson.BreakTime>();
            var jsonObj = new LiveJson();
            jsonObj.data = new List<LiveJson.Data> { data };
            jsonObj.cacheUpdateTime = (long)(DateTime.Now.Subtract(epoch).TotalMilliseconds);
            return jsonObj.ToJson();
        }
    }
}
