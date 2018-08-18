// IEや Edgeを無視する
// #define NeglectEdge
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using System.Collections.Generic;
using System.Linq;

namespace MyShogi.Model.Shogi.Kifu
{
    public partial class KifuManager
    {
        /// <summary>
        /// 現在の局面をSVG形式で書き出す。
        /// 棋譜ではなく図に近いので逆変換は出来ない。
        /// </summary>
        /// <returns></returns>
        private string ToSvgString() =>
            new Converter.Svg.Svg().ToString(Position, KifuHeader);
    }
}

namespace MyShogi.Model.Shogi.Converter.Svg
{
    /// <summary>
    /// 要素構築クラス
    /// </summary>
    public class Element
    {
        public string name;
        public string content;
        public Dictionary<string, string> attrs;
        public Element(string name, string content = "", Dictionary<string, string> attrs = null)
        {
            this.name = name;
            this.content = content;
            this.attrs = attrs ?? new Dictionary<string, string>();
        }
        override public string ToString()
        {
            var attrsBuf = new List<string>();
            foreach (var kvp in attrs)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    attrsBuf.Add($"{kvp.Key}=\"{kvp.Value}\"");
                }
            }
            return string.IsNullOrWhiteSpace(content) ?
                $"<{name} {string.Join(" ", attrsBuf)}/>" :
                $"<{name} {string.Join(" ", attrsBuf)}>{content}</{name}>";
        }
        public Paragraph ToParagraph() => new Paragraph(ToString());
    }

    /// <summary>
    /// stringにインデント量を付与した構造体
    /// </summary>
    public struct IndentString
    {
        public string str;
        public int indent;
        public IndentString(string s = "", int i = 0) { str = s; indent = i; }
    }

    /// <summary>
    /// IndentStringのリストを管理する
    /// </summary>
    public class Paragraph
    {
        // データの実体
        List<IndentString> p;

        // コンストラクタ
        public Paragraph() { p = new List<IndentString>(); }
        public Paragraph(string str)
        {
            p = new List<IndentString>();
            p.Add(new IndentString(str));
        }

        /// <summary>
        /// 引数のオブジェクトを末尾に結合する
        /// </summary>
        public void Concat(Paragraph addP) => p.AddRange(addP.p);

        /// <summary>
        /// 引数の文字列をインデント0で末尾に結合する
        /// </summary>
        public void Concat(string addString) => p.Add(new IndentString(addString));

        /// <summary>
        /// 全体のインデントを1つ上げる
        /// </summary>
        public void IncreaseIndent()
        {
            List<IndentString> newP = new List<IndentString>();
            foreach (var indentString in p)
            {
                newP.Add(new IndentString(indentString.str, indentString.indent + 1));
            }
            p = newP;
        }

        /// <summary>
        /// 全体のインデントを1つ上げてheaderとfooterを挿入する
        /// </summary>
        public void Insert(string header, string footer)
        {
            var topIndent = p.Count == 0 ? 0 : p[0].indent;
            IncreaseIndent();
            var h = new IndentString(header, topIndent);
            var f = new IndentString(footer, topIndent);
            p.Insert(0, h);
            p.Add(f);
        }

        /// <summary>
        /// tagと属性のstringからInsertを呼ぶラッパー
        /// </summary>
        public void InsertTag(string tag, string attribute) =>
            Insert($"<{tag} {attribute}>", $"</{tag}>");

        /// <summary>
        /// インデントを考慮して文字列化する
        /// </summary>
        override public string ToString()
        {
            const string IndentString = "    ";
            var t = "";
            foreach (var indentString in p)
            {
                for (int i = 0; i < indentString.indent; i++)
                {
                    t += IndentString;
                }
                t += indentString.str;
                t += "\r\n";
            }
            return t;
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        public void ToFile()
        {
            var filePath = System.IO.Path.Combine(@".\test.svg");
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath))
            {
                sw.Write(ToString());
            }
        }
    }

    /// <summary>
    /// SVG描画
    /// </summary>
    public class Svg
    {
        Paragraph MakeStyle()
        {
            string[] MinchoList = {
                "游明朝", "Yu Mincho", "YuMincho",
                "Noto Serif CJK JP Medium", "Source Han Serif JP Medium",
                "Noto Serif CJK JP", "Source Han Serif JP",
                "Hiragino Mincho ProN",
                "Kozuka Mincho Pr6N", "Kozuka Mincho Pro",
                "HGS明朝E", "HG明朝E",
                "serif"
            };
            string[] GothicList = {
                "游ゴシック Medium", "Yu Gothic Medium", "YuGothicM", "Yu Gothic", "YuGothic",
                "Noto Sans CJK JP Medium", "Source Han Sans JP Medium",
                "Noto Sans CJK JP", "Source Han Sans JP",
                "Hiragino Kaku Gothic ProN",
                "Kozuka Gothic Pr6N", "Kozuka Gothic Pro",
                "sans-serif"
            };
            string exList(string[] strArray) => $"font-family: {string.Join(", ", strArray.Select(s => $"\"{s}\""))};";
            Paragraph Mincho = new Paragraph(exList(MinchoList));
            Mincho.Insert(".piece, .state, .hand, .board {", "}");
            Paragraph Gothic = new Paragraph(exList(GothicList));
            Gothic.Insert(".pieceb {", "}");
            Paragraph PieceFontSize = new Paragraph("font-size: 50px;");
            PieceFontSize.Insert(".piece, .pieceb {", "}");
            Paragraph InfoFontSize = new Paragraph("font-size: 30px;");
            InfoFontSize.Insert(".state, .hand {", "}");
            Paragraph BoardFontSize = new Paragraph("font-size: 16px;");
            BoardFontSize.Insert(".board {", "}");
            Paragraph CenterText = new Paragraph("text-anchor: middle;");
            CenterText.Insert(".piece, .pieceb, .state, .board {", "}");
            Paragraph Hand = new Paragraph();
            // SVG2, CSS Writing Modes Level 3 仕様案では "writing-mode: vertical-rl;" と変更される
            // SVG1.1仕様での "writing-mode: tb-rl;" は非推奨となるが、互換性のため当面はサポートが残される見込み
            // https://www.w3.org/TR/SVG11/text.html#WritingModeProperty
            // https://www.w3.org/TR/SVG2/text.html#WritingModeProperty
            // https://www.w3.org/TR/css-writing-modes-3/#block-flow
            Hand.Concat("-webkit-writing-mode: tb-rl;");
            Hand.Concat("-ms-writing-mode: tb-rl;");
            Hand.Concat("writing-mode: tb-rl;");
            // 縦書きモードでは位置指定の基準線(baseline)は "central" が使われる
            // 但し、IE11/Edge はそもそも dominant-baseline の指定や仕様を無視して
            // 縦書きモードでも常に "dominant-baseline: alphabetic;" の動作をする？
            // https://www.w3.org/TR/SVG11/text.html#DominantBaselineProperty
            // https://www.w3.org/TR/SVG2/text.html#DominantBaselineProperty
            // https://www.w3.org/TR/css-inline-3/#propdef-dominant-baseline
            Hand.Concat("dominant-baseline: central;");
            // https://www.w3.org/TR/css-inline-3/#propdef-alignment-baseline
            Hand.Concat("alignment-baseline: central;");
            // https://www.w3.org/TR/css-writing-modes-3/#propdef-text-orientation
            Hand.Concat("text-orientation: mixed;");
            Hand.Insert(".hand {", "}");
            Paragraph Text = new Paragraph();
#if NeglectEdge
            Text.Concat("dominant-baseline: central;");
#endif
            Text.Concat("stroke: none;");
            Text.Concat("fill: black;");
            Text.Insert("text {", "}");
            Paragraph Path = new Paragraph();
            Path.Concat("stroke: black;");
            Path.Insert("path {", "}");
            Paragraph Circle = new Paragraph();
            Circle.Concat("stroke: none;");
            Circle.Concat("fill: black;");
            Circle.Insert("circle {", "}");
            Paragraph tempP = new Paragraph();
            tempP.Concat(Mincho);
            tempP.Concat(Gothic);
            tempP.Concat(PieceFontSize);
            tempP.Concat(InfoFontSize);
            tempP.Concat(BoardFontSize);
            tempP.Concat(CenterText);
            tempP.Concat(Hand);
            tempP.Concat(Text);
            tempP.Concat(Path);
            tempP.Concat(Circle);
            tempP.Insert("<style>", "</style>");
            return tempP;
        }

        Paragraph Draw(Position pos, KifuHeader kifuHeader)
        {
            Color sideToView = Color.BLACK; // どちらの手番から見た盤面を出力するか
            int gamePly = pos.gamePly; // 手数が1以外なら直前の指し手を出力する

            const float boardTopMargin = 56;
            const float boardLeftMargin = 56;
            const float boardRightMargin = 56;
            const float boardBottomMargin = 56;
            const float boardBorder = 24; // 座標の表示領域
            const float blockSize = 60; // マス目
                                        // const int boardSize = blockSize * 9 + boardBorder * 2;
            const float starSize = 5;
            const float pieceSize = 50;
            const float fontSize = 30;
            const float boardFontSize = 16;

            const float boardLeft = boardLeftMargin + boardBorder;
            const float boardRight = boardLeftMargin + boardBorder + blockSize * 9;
            const float boardTop = boardTopMargin + boardBorder;
            const float boardBottom = boardTopMargin + boardBorder + blockSize * 9;

            const float svgWidth = boardRight + boardBorder + boardRightMargin;
            const float svgHeight = boardBottom + boardBorder + boardBottomMargin;

            // 直線群の描画
            Paragraph drawLines(IEnumerable<string> pathes, float strokeWidth = 1) =>
                new Element("path", "", new Dictionary<string, string>(){
                    { "d", string.Join(" ", pathes) },
                    { "stroke-width", float.IsNaN(strokeWidth) ? "" : $"{strokeWidth}" },
                }).ToParagraph();

            // 四角形の描画
            Paragraph drawSquare(float x, float y, float size, float strokeWidth = 1, string fill = "none") =>
                new Element("path", "", new Dictionary<string, string>(){
                    { "d", $"M {x},{y} h {size} v {size} h {-size} z" },
                    { "stroke-width", float.IsNaN(strokeWidth) ? "" : $"{strokeWidth}" },
                    { "fill", fill },
                }).ToParagraph();

            // 円の描画
            Paragraph drawCircle(float cx, float cy, float r) =>
                new Element("circle", "", new Dictionary<string, string>(){
                    { "cx", $"{cx}" },
                    { "cy", $"{cy}" },
                    { "r", $"{r}" },
                }).ToParagraph();

            // 駒形五角形の描画
            // x, yは図形の上端中央を表す
            Paragraph drawPentagon(float x, float y, bool isBlack = true)
            {
                var x1 = x;
                var y1 = y;
                var x2 = x + fontSize * 0.37f;
                var y2 = y + fontSize * 0.20f;
                var x3 = x + fontSize * 0.46f;
                var y3 = y + fontSize;
                var x4 = x - fontSize * 0.46f;
                var y4 = y + fontSize;
                var x5 = x - fontSize * 0.37f;
                var y5 = y + fontSize * 0.20f;

                // 座標を整数に丸めて出力する
                return new Element("path", "", new Dictionary<string, string>(){
                    { "d", $"M {x1:f0},{y1:f0} L {x2:f0},{y2:f0} {x3:f0},{y3:f0} {x4:f0},{y4:f0} {x5:f0},{y5:f0} z" },
                    { "stroke-width", "1" },
                    { "fill", isBlack ? "black" : "none" },
                }).ToParagraph();
            }

            // 文字の描画
            Element textElement(float x, float y, string t, float size = float.NaN) =>
                new Element("text", t, new Dictionary<string, string>(){
                    { "x", $"{x:f0}" },
                    { "y", $"{y:f0}" },
                    { "font-size", float.IsNaN(size) ? "" : $"{size}" },
                });
            Paragraph drawText(float x, float y, string t, float size = float.NaN) => textElement(x, y, t, size).ToParagraph();

            // 直前の指し手情報の描画
            Paragraph drawState()
            {
                var move = pos.State().lastMove;
                var moveStr = "";
                if (move != Move.NONE && !move.IsSpecial())
                {
                    var lastPos = pos.Clone();
                    lastPos.UndoMove();
                    moveStr = lastPos.ToKi2(move);
                }
                Paragraph tempP = new Paragraph();
                if (gamePly == 1)
                {}
                else if (move != Move.NONE && !move.IsSpecial())
                {
                    var x = boardLeft + blockSize * 4.5f;
#if NeglectEdge
                    var y = boardTopMargin * 0.5f;
#else
                    var y = boardTopMargin * 0.5f + fontSize * 0.5f;
#endif
                    var str1 = $"【図は{gamePly - 1}手目 　";
                    var str2 = $"{moveStr} まで】";
                    var str = $"{str1}{str2}";
                    tempP.Concat(drawText(x, y, str, fontSize));
                    // 駒文字はフォントで出した方が簡単だけど……
                    Color prevSide = pos.sideToMove == Color.BLACK ? Color.WHITE : Color.BLACK;
                    // おおよその長さ
                    var lenStr = Converter.EastAsianWidth.legacyWidth(str);
                    var lenStr2 = Converter.EastAsianWidth.legacyWidth(str2);

                    var px = x + (lenStr * fontSize / 4) - (lenStr2 * fontSize / 2) - fontSize / 2;
#if NeglectEdge
                    var py = y - fontSize / 2;
#else
                    var py = y - fontSize;
#endif
                    tempP.Concat(drawPentagon(px, py, prevSide == Color.BLACK));
                }
                else
                {
                    var x = boardLeft + blockSize * 4.5f;
                    var y = fontSize + 10f;
                    var str = $"【図は{gamePly - 1}手目まで】";
                    tempP.Concat(drawText(x, y, str, fontSize));
                }
                tempP.InsertTag("g", "class=\"state\"");
                return tempP;
            }

            // 駒の描画
            Paragraph drawPiece(int file, int rank, string t, bool isReverse = false, bool isBold = false)
            {
                float baseX = boardRight + blockSize / 2;
                float baseY = boardTop - blockSize / 2;
                float offsetX = baseX - file * blockSize;
                float offsetY = baseY + rank * blockSize;
                float correct; // 駒はマス目の中央よりやや下に置く補正
#if NeglectEdge
                correct = pieceSize * 0f;
#else
                correct = pieceSize * 0.4f; // IEでは縦のセンタリングが効かないのを無理矢理補正
#endif
                var textElem = textElement(offsetX, offsetY + correct, t);
                if (isReverse)
                {
                    textElem.attrs["transform"] = $"rotate(180,{offsetX},{offsetY})";
                }
                if (isBold)
                {
                    textElem.attrs["class"] = "pieceb";
                }
                return textElem.ToParagraph();
            }

            // 名前、持駒領域の描画
            // とりあえず正位置でレンダリングして盤の中央で180回転させれば逆位置に行く
            // 縦書きで複数文字レンダリングするとブラウザによって挙動が異なるが我慢する
            // - IE11/Edgeでは縦書き文字の位置が半文字分ほどずれる
            Paragraph drawHands()
            {
                var x = boardRight + boardBorder + boardRightMargin * 0.4f;
                var y = boardTop + boardTopMargin + fontSize / 2;
                var offsetX = boardLeft + blockSize * 4.5f;
                var offsetY = boardTop + blockSize * 4.5f;

                var playerName = new[]{
                    kifuHeader.PlayerNameBlack,
                    kifuHeader.PlayerNameWhite,
                };
                var playerNameLen = new[]{
                    Converter.EastAsianWidth.legacyWidth(playerName[0]),
                    Converter.EastAsianWidth.legacyWidth(playerName[1]),
                };
                var handBlack = pos.Hand(Color.BLACK);
                var handWhite = pos.Hand(Color.WHITE);
                var handStr = new[]{
                    handBlack == Hand.ZERO ? "なし" : handBlack.Pretty2(),
                    handWhite == Hand.ZERO ? "なし" : handWhite.Pretty2(),
                };
                var HandStrLen = new[]{
                    Converter.EastAsianWidth.legacyWidth(handStr[0]),
                    Converter.EastAsianWidth.legacyWidth(handStr[1]),
                };
#if NeglectEdge
                var fixLength = 26;
                var justifyLength = 22;
#else
                var fixLength = 24;
#endif
                var handFullMaxLen = System.Math.Max(playerNameLen[0] + HandStrLen[0], playerNameLen[1] + HandStrLen[1]);
                var twoSided = 32 < handFullMaxLen;

                var tempP = new Paragraph();

                foreach (var c in All.Colors())
                {
                    var cI = c.ToInt();
                    var sideP = drawPentagon(x, y - fontSize * 1.3f, c == Color.BLACK);

                    if (twoSided)
                    {
                        var hand1 = playerName[cI];
                        var hand2 = $"持駒　{handStr[cI]}";
                        var hand1Len = playerNameLen[cI];
                        var hand2Len = HandStrLen[cI] + 6;
                        // 文字数に応じて適当にフォントサイズを変更する
                        var renderSize1 = System.Math.Min(fontSize / 1.5f, hand1Len <= fixLength ? fontSize : fontSize * fixLength / hand1Len);
                        var renderSize2 = System.Math.Min(fontSize / 1.5f, hand2Len <= fixLength ? fontSize : fontSize * fixLength / hand2Len);
                        var handSize = System.Math.Max(renderSize1, renderSize2);

                        // 出力を簡潔にするため、フォントサイズがデフォルトのときはfont-sizeを省略する
                        var hand1Elem = textElement(x + handSize * 0.6f, y, hand1, fontSize != renderSize1 ? renderSize1 : float.NaN);
                        var hand2Elem = textElement(x - handSize * 0.6f, y, hand2, fontSize != renderSize2 ? renderSize2 : float.NaN);

#if NeglectEdge
                        // 両端揃え
                        // textLength を設定すると IE / Edge で描画が崩壊する
                        if (hand1Len > justifyLength)
                        {
                            hand1Elem.attrs["textLength"] = $"{boardBottom - y}";
                            hand1Elem.attrs["lengthAdjust"] = "spacingAndGlyphs";
                        }
                        if (hand2Len > justifyLength)
                        {
                            hand2Elem.attrs["textLength"] = $"{boardBottom - y}";
                            hand2Elem.attrs["lengthAdjust"] = "spacingAndGlyphs";
                        }
#endif

                        sideP.Concat(hand1Elem.ToString());
                        sideP.Concat(hand2Elem.ToString());
                    }
                    else
                    {
                        var handFull = $"{playerName[cI]}　持駒　{handStr[cI]}";
                        // 1段組の時は両者のフォントサイズを揃える
                        var handLen = handFullMaxLen + 8;
                        // 出力を簡潔にするため、フォントサイズがデフォルトのときはfont-sizeを省略する
                        var handElem = textElement(x, y, handFull, handLen > fixLength ? fontSize * fixLength / handLen : float.NaN);

#if NeglectEdge
                        // 両端揃え
                        // textLength を設定すると IE / Edge で描画が崩壊する
                        if (handLen > justifyLength)
                        {
                            handElem.attrs["textLength"] = $"{boardBottom - y}";
                            handElem.attrs["lengthAdjust"] = "spacingAndGlyphs";
                        }
#endif

                        sideP.Concat(handElem.ToString());
                    }

                    if (sideToView != c)
                    {
                        sideP.InsertTag("g", $"transform=\"rotate(180,{offsetX},{offsetY})\"");
                    }

                    tempP.Concat(sideP);
                }

                tempP.InsertTag("g", "class=\"hand\"");

                return tempP;
            }

            // 将棋盤の描画
            Paragraph drawBoard()
            {
                var tempP = drawSquare(boardLeft, boardTop, blockSize * 9, 4);
                var pathBuf = new List<string>();
                for (int i = 1; i < 9; ++i)
                {
                    pathBuf.Add($"M {boardLeft},{boardTop + blockSize * i} h {blockSize * 9}");
                    pathBuf.Add($"M {boardLeft + blockSize * i},{boardTop} v {blockSize * 9}");
                }
                tempP.Concat(drawLines(pathBuf));
                tempP.Concat(drawCircle(boardLeft + blockSize * 3, boardTop + blockSize * 3, starSize));
                tempP.Concat(drawCircle(boardLeft + blockSize * 3, boardTop + blockSize * 6, starSize));
                tempP.Concat(drawCircle(boardLeft + blockSize * 6, boardTop + blockSize * 3, starSize));
                tempP.Concat(drawCircle(boardLeft + blockSize * 6, boardTop + blockSize * 6, starSize));

                string[] fileStr = { "９", "８", "７", "６", "５", "４", "３", "２", "１" };
                string[] rankStr = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
                for (var i = 0; i < 9; ++i)
                {
                    var xf = boardLeft + blockSize * (i + 0.5f);
                    var xr = boardRight + boardBorder * 0.5f;
#if NeglectEdge
                    var yf = boardTopMargin + boardBorder * 0.5f;
                    var yr = boardTop + blockSize * (i + 0.5f);
#else
                    var yf = boardTopMargin + boardBorder * 0.5f + boardFontSize * 0.4f;
                    var yr = boardTop + blockSize * (i + 0.5f) + boardFontSize * 0.4f;
#endif
                    tempP.Concat(drawText(xf, yf, fileStr[i]));
                    tempP.Concat(drawText(xr, yr, rankStr[i]));
                }

                tempP.InsertTag("g", "class=\"board\"");

                return tempP;
            }

            // 盤上の駒の描画
            Paragraph drawBoardPiece()
            {
                var tempP = new Paragraph();
                var lastMove = pos.State().lastMove;

                for (SquareHand sqh = SquareHand.SquareZero; sqh < SquareHand.SquareNB; ++sqh)
                {
                    var pi = pos.PieceOn(sqh);
                    if (pi != Piece.NO_PIECE)
                    {
                        var sq = (Square)sqh;
                        var file = sideToView == Color.BLACK ? (int)sq.ToFile() + 1 : 9 - (int)sq.ToFile();
                        var rank = sideToView == Color.BLACK ? (int)sq.ToRank() + 1 : 9 - (int)sq.ToRank();
                        char[] piChar = { pi.Pretty2() };
                        var piStr = new string(piChar);
                        var isReverse = pi.PieceColor() == Color.WHITE;
                        if (sideToView == Color.WHITE)
                        {
                            isReverse = !isReverse;
                        }
                        // 直前の指し手を強調する
                        bool isBold = gamePly != 1 && lastMove != Move.NONE && !lastMove.IsSpecial() && sq == pos.State().lastMove.To();
                        if (isBold)
                        {
                            tempP.Concat(drawSquare(boardRight - file * blockSize, boardTop + (rank - 1) * blockSize, blockSize, float.NaN, "#ffff80"));
                        }
                        tempP.Concat(drawPiece(file, rank, piStr, isReverse, isBold));
                    }
                }
                tempP.InsertTag("g", "class=\"piece\"");
                return tempP;
            }

            var p = new Paragraph();
            p.Concat(MakeStyle());
            p.Concat(drawState());
            p.Concat(drawHands());
            p.Concat(drawBoardPiece());
            p.Concat(drawBoard());

            // svgのヘッダーとフッターを追加
            p.InsertTag("svg", $"xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" xml:lang=\"ja-JP\" viewBox=\"0 0 {svgWidth} {svgHeight}\" width=\"{svgWidth}\" height=\"{svgHeight}\"");

            return p;
        }

        public void Output(Position pos, KifuHeader kifuHeader)
        {
            Draw(pos, kifuHeader).ToFile();
        }

        /// <summary>
        /// SVG形式の文字列化をして返す。
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public string ToString(Position pos, KifuHeader kifuHeader) => Draw(pos, kifuHeader).ToString();
    }
}
