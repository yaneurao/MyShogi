using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Converter;
using System;

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
            var lineNo = 1;

            // ヘッダ検出用正規表現
            var rHead = new Regex(@"^([^：]+)：(.*)");
            // 変化手数用正規表現
            var rHenka = new Regex(@"^([0-9]+)手");
            // KIF指し手検出用正規表現
            var rKif = new Regex(@"^\s*(\d+)\s*(?:((?:[1-9１-９][1-9１-９一二三四五六七八九]|同\s?)成?[歩香桂銀金角飛と杏圭全馬竜龍玉王][打不成]*(?:\([1-9][1-9]\))?)|(\S+))\s*(\(\s*([0-9]+):([0-9]+)/([0-9]+):([0-9]+):([0-9])\))?");
            // KI2指し手検出用正規表現
            var rKi2 = new Regex(@"(?:([-+▲△▼▽☗☖⛊⛉](?:[1-9１-９][1-9１-９一二三四五六七八九]|同\s?)成?[歩香桂銀金角飛と杏圭全馬竜龍玉王][打不成左直右上寄引]*)|())");

            var bod = new List<string>();

            var isBody = false;

            // 初期局面の遅延処理
            Func<string> lazyHead = () =>
            {
                isBody = true;
                if (bod.Count > 0)
                {
                    if (KifuHeader.header_dic.ContainsKey("手合割")) return "手合割と初期局面文字列が同時に指定されています";
                    Tree.rootSfen = Converter.KifExtensions.BodToSfen(bod.ToArray());
                    Tree.position.SetSfen(Tree.rootSfen);
                    Tree.rootBoardType = BoardType.Others;
                }
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
                            if (isBody) return $"対局開始後にヘッダが指定されました: {line}";
                            bod.Add(line);
                            goto nextline;
                        case "変化":
                            if (!isBody) return "初期局面からは変化できません";
                            var mHenka = rHenka.Match(headerValue);
                            if (!mHenka.Success) return "変化する手数を検出できませんでした";
                            var iHenka = int.Parse(mHenka.Groups[1].Value);
                            while (Tree.gamePly > iHenka)
                                Tree.UndoMove();
                            goto nextline;
                        case "手合割":
                            if (isBody) return $"対局開始後にヘッダが指定されました: {line}";
                            KifuHeader.header_dic.Add(headerKey, headerValue);
                            switch (headerValue)
                            {
                                case "平手":
                                    Tree.rootSfen = Sfens.HIRATE;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.NoHandicap;
                                    goto nextline;
                                case "香落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_KYO;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.HandicapKyo;
                                    break;
                                case "右香落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_RIGHT_KYO;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.HandicapRightKyo;
                                    break;
                                case "角落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_KAKU;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.HandicapKaku;
                                    break;
                                case "飛車落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_HISYA;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.HandicapHisya;
                                    break;
                                case "飛香落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_HISYA_KYO;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.HandicapHisyaKyo;
                                    break;
                                case "二枚落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_2;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.Handicap2;
                                    break;
                                case "三枚落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_3;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.Handicap3;
                                    break;
                                case "四枚落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_4;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.Handicap4;
                                    break;
                                case "五枚落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_5;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.Handicap5;
                                    break;
                                case "左五枚落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_LEFT_5;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.HandicapLeft5;
                                    break;
                                case "六枚落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_6;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.Handicap6;
                                    break;
                                case "八枚落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_8;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.Handicap8;
                                    break;
                                case "十枚落ち":
                                    Tree.rootSfen = Sfens.HANDICAP_10;
                                    Tree.position.SetSfen(Tree.rootSfen);
                                    Tree.rootBoardType = BoardType.Handicap10;
                                    break;
                                default:
                                    Tree.rootBoardType = BoardType.Others;
                                    break;
                            }
                            goto nextline;
                        case "先手":
                        case "下手":
                            if (isBody) return $"対局開始後にヘッダが指定されました: {line}";
                            KifuHeader.header_dic.Add(headerKey, headerValue);
                            KifuHeader.PlayerNameBlack = headerValue;
                            goto nextline;
                        case "後手":
                        case "上手":
                            if (isBody) return $"対局開始後にヘッダが指定されました: {line}";
                            KifuHeader.header_dic.Add(headerKey, headerValue);
                            KifuHeader.PlayerNameWhite = headerValue;
                            goto nextline;
                        default:
                            if (isBody) return $"対局開始後にヘッダが指定されました: {line}";
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
                        if (isBody) return $"対局開始後にヘッダが指定されました: {line}";
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
                        return $"手数が一致しません: {Tree.gamePly} != {line}";
                    Move move;
                    if (mKif.Groups[2].Success)
                        move = Tree.position.FromKif(mKif.Groups[2].Value);
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
                        return $"指し手を解析できませんでした: {line}";
                    if (mKif.Groups[4].Value.Length > 0)
                        Tree.AddNode(
                            move,
                            // ToDo: あとでちゃんと書く
                            KifuMoveTimes.Zero
                        /*
                        TimeSpan.FromSeconds(
                            int.Parse(mKif.Groups[5].Value) * 60 +
                            int.Parse(mKif.Groups[6].Value)),
                        TimeSpan.FromSeconds(
                            int.Parse(mKif.Groups[7].Value) * 3600 +
                            int.Parse(mKif.Groups[8].Value) * 60 +
                            int.Parse(mKif.Groups[9].Value))
                        */
                        );
                    else
                        Tree.AddNode(move, KifuMoveTimes.Zero);
                    if (Tree.position.IsLegal(move))
                        // これだと不正着手後の棋譜コメントを取れないがとりあえず解析を中止する
                        return $"不正着手を検出しました: {line}";
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
                    foreach (var m in mKi2)
                    {
                        var move = Tree.position.FromKif(mKif.Groups[1].Value);
                        if (move == Move.NONE)
                            return $"指し手を解析できませんでした: {line}";
                        Tree.AddNode(move, KifuMoveTimes.Zero);
                        if (Tree.position.IsLegal(move))
                            // これだと不正着手後の棋譜コメントを取れないがとりあえず解析を中止する
                            return $"不正着手を検出しました: {line}";
                        if (move.IsOk())
                            Tree.DoMove(move);
                    }
                    goto nextline;
                }

                nextline:;
                continue;
            }

            if (!isBody)
            {
                var headRes = lazyHead();
                if (headRes != string.Empty) return headRes;
            }

            return string.Empty;
        }

        /// <summary>
        /// Kif形式の文字列に変換
        /// </summary>
        /// <returns></returns>
        private string ToKifString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("先手：", KifuHeader.PlayerNameBlack).AppendLine();
            sb.AppendFormat("後手：", KifuHeader.PlayerNameWhite).AppendLine();
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    sb.AppendLine("手合割：平手");
                    break;
                default:
                    sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));
                    break;
            }
            // ToDo: ここに実装する
            return sb.ToString();
        }

        private string ToKi2String()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("先手：", KifuHeader.PlayerNameBlack).AppendLine();
            sb.AppendFormat("後手：", KifuHeader.PlayerNameWhite).AppendLine();
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    sb.AppendLine("手合割：平手");
                    break;
                default:
                    sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));
                    break;
            }
            // ToDo: ここに実装する
            return sb.ToString();
        }
    }
}
