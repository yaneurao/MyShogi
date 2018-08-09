// IEや Edgeを無視する
// #define NeglectEdge
using System;
using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;


namespace MyShogi.Model.Common.Utility
{
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
            var topIndent = p[0].indent;
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
    /// Position(局面)の付随情報を格納する構造体
    /// </summary>
    public class Svg
    {
        /// <summary>
        /// svgのヘッダーとフッターを追加します
        /// </summary>
        void InsertSvg(Paragraph p) =>
            p.InsertTag("svg", "xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\"");

        Paragraph MakeStyle()
        {
            string[] MintyoList = {
                "ヒラギノ明朝体5", "IPAex明朝", "HiraMinProN-W5", "小塚明朝 Pro M", "HG明朝E", "ＭＳ 明朝", "sans-serif"
            };
            string[] GothicList = {
                "ヒラギノ角ゴ5", "IPAexゴシック", "ＭＳ ゴシック", "sans-serif"
            };
            string exList(string[] strArray)
            {
                string t = "";
                foreach (var str in strArray)
                {
                    t += $"'{str}', ";
                }
                t = t.Substring(0, t.Length - 2);
                return $"font-family: {t};";
            }
            Paragraph Mintyo = new Paragraph(exList(MintyoList));
            Paragraph Gothic = new Paragraph(exList(GothicList));
            Paragraph PieceFontSize = new Paragraph("font-size: 50px;");
            Paragraph HandFontSize = new Paragraph("font-size: 30px;");
            Paragraph Base = new Paragraph();
            Base.Concat("stroke: #000000;");
            Base.Concat("text-anchor: middle;");
            Paragraph PieceBold = new Paragraph();
            PieceBold.Concat(Gothic);
            PieceBold.Concat(PieceFontSize);
            PieceBold.Concat(Base);
            PieceBold.Insert(".pieceb {", "}");
            Paragraph Piece = new Paragraph();
            Piece.Concat(Mintyo);
            Piece.Concat(PieceFontSize);
            Piece.Concat(Base);
            Piece.Insert(".piece {", "}");
            Paragraph Hand = new Paragraph();
            Hand.Concat(Mintyo);
            Hand.Concat(HandFontSize);
            Hand.Concat(Base);
            Hand.Insert(".hand {", "}");
            Paragraph tempP = new Paragraph();
            tempP.Concat(PieceBold);
            tempP.Concat(Piece);
            tempP.Concat(Hand);
            tempP.Insert("<style>", "</style>");
            return tempP;
        }

        Paragraph Draw(Position pos)
        {
            Color sideToView = Color.BLACK; // 後手なら盤面の出力を反転する
            int gamePly = pos.gamePly; // 手数が1以外なら直前の指し手を出力する

            const int boardTopMargin = 50;
            const int boardLeftMargin = 40;
            const int boardRightMargin = 40;
            // const int boardBottomMargin = 50;
            const int boardBorder = 30; // 座標の表示領域
            const int blockSize = 60; // マス目
            // const int boardSize = blockSize * 9 + boardBorder * 2;
            const int starSize = 5;
            const int pieceSize = 50;
            const int fontSize = 30;

            const int boardLeft = boardLeftMargin + boardBorder;
            const int boardRight = boardLeftMargin + boardBorder + blockSize * 9;
            const int boardTop = boardTopMargin + boardBorder;
            // const int boardBottom = boardTopMargin + boardBorder + blockSize * 9;

            string s(int i) => i.ToString();

            // 直線の描写
            Paragraph pathLineH(int x, int y, int h, string color = "#000000") =>
                new Paragraph($"<path d=\"M {s(x)},{s(y)} h {s(h)}\" stroke=\"{color}\" />");
            Paragraph pathLineV(int x, int y, int v, string color = "#000000") =>
                new Paragraph($"<path d=\"M {s(x)},{s(y)} v {s(v)}\" stroke=\"{color}\" />");

            // 円の描写
            string circle(int cx, int cy, int r, string fill) =>
                $"<circle cx=\"{s(cx)}\" cy=\"{s(cy)}\" r=\"{s(r)}\" fill=\"{fill}\" />";
            Paragraph drawCircle(int cx, int cy, int r, string fill = "#000000") =>
                new Paragraph(circle(cx, cy, r, fill));

            // 四角形を描きます
            Paragraph drawSquare(int x, int y, int size, int storockWidth = 1, string color = "#000000")
            {
                var d = $"d =\"M {s(x)},{s(y)} h {s(size)} v {s(size)} h {s(-size)} z\"";
                var option = color == "#000000" ? "fill=\"none\" " : $"fill=\"{color}\" ";
                option += $"stroke-width=\"{storockWidth}\" stroke=\"#000000\"";
                return new Paragraph($"<path {d} {option} />");
            }

            // 駒形の五角形を描きます
            // x, yは図形の上端中央を表す
            Paragraph drawPentagon(int x, int y, bool isBlack = true, string color = "#000000")
            {
                var f = fontSize;
                int x1 = x;
                var y1 = y;
                int x2 = x + (int)(f * 0.38);
                var y2 = y + (int)(f * 0.2);
                int x3 = x + (int)(f * 0.45);
                var y3 = y + f;
                int x4 = x - (int)(f * 0.45);
                var y4 = y + f;
                int x5 = x - (int)(f * 0.38);
                var y5 = y + (int)(f * 0.2);

                var d = $"d=\"M {s(x1)},{s(y1)} L {x2},{y2} {x3},{y3} {x4},{y4} {x5},{y5} z\"";
                var fill = isBlack ? "" : "fill=\"none\"";
                var option = $"{fill} stroke-width=\"1\" stroke=\"{color}\"";
                return new Paragraph($"<path {d} {option} />");
            }

            // 文字の描写
#if NeglectEdge
            string text(int x, int y, int size, string t, string fill) =>
                $"<text x=\"{s(x)}\" y=\"{s(y)}\" dominant-baseline=\"central\">{t}</text>";
            string textEx(int x, int y, string t, int size) =>
                $"<text x=\"{s(x)}\" y=\"{s(y)}\" font-size=\"{s(size)}\" dominant-baseline=\"central\">{t}</text>";
#else
            string text(int x, int y, string t) =>
                $"<text x=\"{s(x)}\" y=\"{s(y)}\">{t}</text>";
            string textEx(int x, int y, string t, int size) =>
                $"<text x=\"{s(x)}\" y=\"{s(y)}\" font-size=\"{s(size)}\">{t}</text>";
#endif
            Paragraph drawText(int x, int y, string t) =>
                new Paragraph(text(x, y, t));
            Paragraph drawTextEx(int x, int y, string t, int size) =>
                new Paragraph(textEx(x, y, t, size));

            // 直前の指し手情報の描写
            Paragraph drawState()
            {
                // TODO: これでは同金とか上、打みたいな情報がつかないが方法が不明
                var move = pos.State().lastMove;
                var moveStr = move.Pretty();
                moveStr = moveStr.Substring(2, 2);
                moveStr += pos.PieceOn(move.To()).Pretty2();
                Paragraph tempP = new Paragraph();
                if (gamePly != 1)
                {
                    var x = boardLeft + blockSize * 9 / 2;
                    var y = fontSize + 10;
                    var str1 = $"【図は5{s(gamePly - 1)}手目 　";
                    var str2 = $"{moveStr} まで】";
                    var str = $"{str1}{str2}";
                    tempP.Concat(textEx(x, y, str, fontSize));
                    // 駒文字はフォントで出した方が簡単だけど……
                    Color prevSide = pos.sideToMove == Color.BLACK ? Color.WHITE : Color.BLACK;
                    // おおよその長さを知る関数
                    int LenB(string stTarget) => System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(stTarget);
                    var lenStr = LenB(str);
                    var lenStr2 = LenB(str2);

                    var px = x + (lenStr * fontSize / 4) - (lenStr2 * fontSize / 2) - fontSize / 2;
                    var py = y - fontSize;
                    tempP.Concat(drawPentagon(px, py, prevSide == Color.BLACK));
                }
                tempP.InsertTag("g", "class=\"hand\"");
                return tempP;
            }

            // 駒の描写
            Paragraph drawPiece(int file, int rank, string t, bool isReverse = false, bool isBold = false)
            {
                int baseX = boardRight + blockSize / 2;
                int baseY = boardTop - blockSize / 2;
                int offsetX = baseX - file * blockSize;
                int offsetY = baseY + rank * blockSize;
                int correct; // 駒はマス目の中央よりやや下に置く補正
#if NeglectEdge
                correct = (int)(pieceSize * 0.1);
#else
                correct = (int)(pieceSize * 0.4); // IEでは縦のセンタリングが効かないのを無理矢理補正
#endif
                var tempP = drawText(offsetX, offsetY + correct, t);
                if (isReverse)
                {
                    tempP.InsertTag("g", $"transform=\"rotate(180,{offsetX},{offsetY})\"");
                }
                if (isBold)
                {
                    tempP.InsertTag("g", "class=\"pieceb\"");
                }
                return tempP;

            }

            // 名前、持駒領域の描写
            // とりあえず正位置でレンダリングして盤の中央で180回転させれば逆位置に行く
            Paragraph drawHand(string hand, string name, bool isBlack, bool isReverse)
            {
                var x = boardRight + boardRightMargin;
                var y = boardTop + boardTopMargin + fontSize;
                var option = $"writing-mode=\"tb\" font-size=\"{s(fontSize)}\"";
                var offsetX = boardLeft + blockSize * 9 / 2;
                var offsetY = boardTop + blockSize * 9 / 2;

                var tempP = drawPentagon(x, y - fontSize * 2, isBlack);

                // 縦書きのレンダリングがうんこなので手動で1文字ずつ配置する
                var handStr = "持駒";
                var totalLength = name.Length + handStr.Length + hand.Length;
                var lenderSize = 0;
                // 文字数に応じて適当にフォントサイズを変更する
                if (totalLength <= 15)
                {
                    lenderSize = fontSize;
                }
                else if (totalLength <= 17)
                {
                    lenderSize = (int)(fontSize * 0.9);
                }
                else if (totalLength <= 19)
                {
                    lenderSize = (int)(fontSize * 0.8);
                }
                else if (totalLength <= 22)
                {
                    lenderSize = (int)(fontSize * 0.7);
                }
                else if (totalLength <= 24)
                {
                    lenderSize = (int)(fontSize * 0.65);
                }
                else
                {
                    lenderSize = (int)(fontSize * 0.6);
                }

                // もうちょっとすっきり書けるだろうが、愚直にdraw関数を呼んでいく
                // 出力を簡潔にするため、フォントサイズがデフォルトのときはExじゃない方を呼ぶ
                for (int i = 0; i < name.Length; ++i)
                {
                    char[] subTchar = { name[i] };
                    string subTstr = new string(subTchar);
                    if (lenderSize == fontSize)
                    {
                        tempP.Concat(drawText(x, y + i * lenderSize, subTstr));
                    }
                    else
                    {
                        tempP.Concat(drawTextEx(x, y + i * lenderSize, subTstr, lenderSize));
                    }
                }
                var offset = name.Length;
                for (int i = 0; i < handStr.Length; ++i)
                {
                    char[] subTchar = { handStr[i] };
                    string subTstr = new string(subTchar);
                    if (lenderSize == fontSize)
                    {
                        tempP.Concat(drawText(x, (int)(y + (offset + i + 0.5) * lenderSize), subTstr));
                    }
                    else
                    {
                        tempP.Concat(drawTextEx(x, (int)(y + (offset + i + 0.5) * lenderSize), subTstr, lenderSize));
                    }
                }
                offset += handStr.Length;
                for (int i = 0; i < hand.Length; ++i)
                {
                    char[] subTchar = { hand[i] };
                    string subTstr = new string(subTchar);
                    if (lenderSize == fontSize)
                    {
                        tempP.Concat(drawText(x, y + (offset + i + 1) * lenderSize, subTstr));
                    }
                    else
                    {
                        tempP.Concat(drawTextEx(x, y + (offset + i + 1) * lenderSize, subTstr, lenderSize));
                    }
                }

                if (isReverse)
                {
                    tempP.InsertTag("g", $"transform=\"rotate(180,{offsetX},{offsetY})\"");
                }
                return tempP;
            }

            // 将棋盤の描写
            // TODO: rank, fileの数字を描写する？
            Paragraph drawBoard()
            {
                var tempP = drawSquare(boardLeft, boardTop, blockSize * 9, 4);
                for (int i = 1; i < 9; ++i)
                {
                    tempP.Concat(pathLineH(boardLeft, boardTop + blockSize * i, blockSize * 9));
                    tempP.Concat(pathLineV(boardLeft + blockSize * i, boardTop, blockSize * 9));
                }
                tempP.Concat(drawCircle(boardLeft + blockSize * 3, boardTop + blockSize * 3, starSize));
                tempP.Concat(drawCircle(boardLeft + blockSize * 3, boardTop + blockSize * 6, starSize));
                tempP.Concat(drawCircle(boardLeft + blockSize * 6, boardTop + blockSize * 3, starSize));
                tempP.Concat(drawCircle(boardLeft + blockSize * 6, boardTop + blockSize * 6, starSize));
                return tempP;
            }

            // 手駒の描写
            // 先手、後手の表示も変更できたほうがいいかも
            Paragraph drawHands()
            {
                Hand b = pos.Hand(Color.BLACK);
                Hand w = pos.Hand(Color.WHITE);
                var tempP = drawHand(b.Pretty2(), "先手", true, sideToView == Color.WHITE);
                tempP.Concat(drawHand(w.Pretty2(), "後手", false, sideToView == Color.BLACK));
                tempP.InsertTag("g", "class=\"hand\"");
                return tempP;
            }

            // 盤上の駒の描写
            Paragraph drawBoardPiece()
            {
                var tempP = new Paragraph();

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
                        bool isBold = gamePly != 1 && sq == pos.State().lastMove.To();
                        if (isBold)
                        {
                            tempP.Concat(drawSquare(boardRight - file * blockSize, boardTop + (rank - 1) * blockSize, blockSize, 1, "#ffff80"));
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
            return p;
        }

        public void Output(Position pos)
        {
            Paragraph p = Draw(pos);
            InsertSvg(p);
            p.ToFile();
        }
    }
}
