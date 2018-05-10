using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// Position(局面)の付随情報を格納する構造体
    /// </summary>
    public class StateInfo
    {
        /// <summary>
        /// 現在の局面のhash key
        /// </summary>
        public HASH_KEY Key;

        /// <summary>
        /// 一手前の局面へのポインタ
        /// previous == null であるとき、これ以上辿れない
        /// これを辿ることで千日手判定などを行う。
        /// </summary>
        public StateInfo previous;
    }

    /// <summary>
    /// 盤面を表現するクラス
    /// </summary>
    public class Position
    {
        // -------------------------------------------------------------------------

            /// <summary>
        /// 盤面、81升分の駒 + 1
        /// </summary>
        private Piece[] board = new Piece[Square.NB_PLUS1.ToInt()];

        /// <summary>
        /// 手駒
        /// </summary>
        private Hand[] hand = new Hand[Color.NB.ToInt()];

        /// <summary>
        /// 手番
        /// </summary>
        private Color sideToMove = Color.BLACK;

        /// <summary>
        /// 玉の位置
        /// </summary>
        private Square[] kingSquare = new Square[Color.NB.ToInt()];

        /// <summary>
        /// 初期局面からの手数(初期局面 == 1)
        /// </summary>
        private int gamePly = 1;

        /// <summary>
        /// 局面の付随情報
        /// st.previousで1手前の局面の情報が得られるので千日手判定などに用いる
        /// </summary>
        private StateInfo st;

        /// <summary>
        /// 局面の付随情報
        /// st.previousで1手前の局面の情報が得られるので千日手判定などに用いる
        /// </summary>
        public StateInfo State() { return st; }

        /// <summary>
        /// 現局面のhash key。
        /// </summary>
        /// <returns></returns>
        public HASH_KEY Key() { return st.Key; }

        // -------------------------------------------------------------------------

        /// <summary>
        /// 盤面上、sqの升にある駒の参照
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public ref Piece PieceOn(Square sq)
        {
            Debug.Assert(sq.IsOk());
            return ref board[sq.ToInt()];
        }

        /// <summary>
        /// c側の手駒の参照
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public ref Hand Hand(Color c)
        {
            return ref hand[c.ToInt()];
        }

        /// <summary>
        /// c側の玉のSquareへの参照
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public ref Square KingSquare(Color c)
        {
            return ref kingSquare[c.ToInt()];
        }

        // -------------------------------------------------------------------------

        /// <summary>
        /// 平手の初期局面のsfen形式
        /// </summary>
        public static readonly string SFEN_HIRATE = "lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1";

        /// <summary>
        /// 以下、各種駒落ちのsfen形式
        /// それぞれの意味については、BoardTypeのenumの定義を見ること
        /// </summary>
        public static readonly string SFEN_HANDICAP_KYO = "lnsgkgsn1/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_RIGHT_KYO = "1nsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_KAKU = "lnsgkgsnl/1r7/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_HISYA = "lnsgkgsnl/7b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_HISYA_KYO = "lnsgkgsn1/7b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_2 = "lnsgkgsnl/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_3 = "lnsgkgsn1/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_4 = "1nsgkgsn1/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_5 = "2sgkgsn1/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_LEFT_5 = "1nsgkgs2/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_6 = "2sgkgs2/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_8 = "3gkg3/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string SFEN_HANDICAP_10 = "4k4/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";

        /// <summary>
        /// 平手、駒落ちなどのsfen文字列をひとまとめにした配列。BoardTypeのenumと対応する。
        /// </summary>
        public static readonly string[] SFENS_OF_BOARDTYPE =
        {
            SFEN_HIRATE , SFEN_HANDICAP_KYO , SFEN_HANDICAP_RIGHT_KYO , SFEN_HANDICAP_KAKU ,
            SFEN_HANDICAP_HISYA , SFEN_HANDICAP_HISYA_KYO ,
            SFEN_HANDICAP_2 , SFEN_HANDICAP_3 , SFEN_HANDICAP_4 , SFEN_HANDICAP_5 , SFEN_HANDICAP_LEFT_5 ,
            SFEN_HANDICAP_6 , SFEN_HANDICAP_8 , SFEN_HANDICAP_10
        };

        // -------------------------------------------------------------------------

        /// <summary>
        /// このクラスを特定の局面で初期化する
        /// デフォルトでは平手で初期化
        /// boardTypeとして範囲外の値を指定した場合は例外が飛ぶ。
        /// </summary>
        public void InitBoard(BoardType boardType = BoardType.NoHandicap)
        {
            if (!boardType.IsOk())
                throw new PositionException("範囲外のBoardTypeを指定してPosition.init()を呼び出した");

            // 平手で初期化
            SetSfen(SFENS_OF_BOARDTYPE[boardType.ToInt()]);
        }

        /// <summary>
        /// 盤面を日本語形式で出力する。
        /// </summary>
        /// <returns></returns>
        public string Pretty()
        {
            var sb = new StringBuilder();

            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
            {
                for (File f = File.FILE_9; f >= File.FILE_1; --f)
                {
                    sb.Append(PieceOn(Util.MakeSquare(f, r)).Pretty());
                }
                sb.AppendLine();
            }

            // 手番
            sb.Append("【"+sideToMove.Pretty() + "番】 ");

            // 手駒
            for (Color c = Color.ZERO; c < Color.NB; ++c)
            {
                sb.Append(c.Pretty() + ":");
                sb.Append(Hand(c).Pretty());
                sb.Append("   ");
            }
            sb.AppendLine();

            // HashKey
            sb.AppendLine(Key().Pretty());

            // USI文字列出力
            sb.Append("sfen : ");
            sb.AppendLine(ToSfen());

            return sb.ToString();
        }

        /// <summary>
        /// SFEN形式で盤面を出力する。
        /// </summary>
        /// <returns></returns>
        public string ToSfen()
        {
            var sb = new StringBuilder();

            // --- 盤面
            int emptyCnt;
            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
            {
                for (File f = File.FILE_9; f >= File.FILE_1; --f)
                {
                    // それぞれの升に対して駒がないなら
                    // その段の、そのあとの駒のない升をカウントする
                    for (emptyCnt = 0; f >= File.FILE_1 && PieceOn(Util.MakeSquare(f,r)) == Piece.NO_PIECE; --f)
                        ++emptyCnt;

                    // 駒のなかった升の数を出力
                    if (emptyCnt != 0)
                        sb.Append(emptyCnt.ToString());

                    // 駒があったなら、それに対応する駒文字列を出力
                    if (f >= File.FILE_1)
                        sb.Append(PieceOn(Util.MakeSquare(f,r)).ToUsi());
                }

                // 最下段以外では次の行があるのでセパレーターである'/'を出力する。
                if (r < Rank.RANK_9)
                    sb.Append('/');
            }

            // --- 手番
            sb.Append(" " + sideToMove.ToUsi() + " ");

            // --- 手駒(UCIプロトコルにはないがUSIプロトコルにはある)

            bool found = false;
            for (Color c = Color.BLACK; c <= Color.WHITE; ++c)
            {
                var h = Hand(c);
                var s = h.ToUsi(c);

                if (!string.IsNullOrEmpty(s))
                {
                    found = true;
                    sb.Append(s);
                }
            }
            // 手駒がない場合
            sb.Append(found ? " " : "- ");

            // --- 初期局面からの手数
            sb.Append(gamePly.ToString());

            return sb.ToString();
        }

        /// <summary>
        /// sfen文字列でこのクラスを初期化する
        /// 
        /// 読み込みに失敗した場合、SfenException例外が投げられる。
        /// </summary>
        /// <param name="sfen"></param>
        public void SetSfen(string sfen)
        {
            st = new StateInfo()
            {
                previous = null
            };

            var split = sfen.Split(
                new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
            if (split.Count() < 3)
                throw new SfenException("SFEN形式の盤表現が正しくありません。");

            // --- 盤面

            Array.Clear(board, 0, board.Length);
            KingSquare(Color.BLACK) = KingSquare(Color.WHITE) = Square.NB;

            // 盤面左上から。Square型のレイアウトに依らずに処理を進めたいため、Square型は使わない。
            File f = File.FILE_9;
            Rank r = Rank.RANK_1;

            bool promoted = false;
            var board_sfen = split[0];
            foreach (var c in board_sfen)
            {
                if (r > Rank.RANK_9)
                    throw new SfenException("局面の段数が９を超えます。");

                if (c == '/')
                {
                    if (f.ToInt() >= 0)
                        throw new SfenException("SFEN形式の" + (r.ToInt()+1).ToString() + "段の駒数が合いません。");

                    r++;
                    f = File.FILE_9;
                    promoted = false;
                }
                else if (c == '+')
                {
                    promoted = true;
                }
                else if ('1' <= c && c <= '9')
                {
                    f -= (int)(c - '0');
                    promoted = false;
                }
                else
                {
                    if (f < File.FILE_1)
                        throw new SfenException("SFEN形式の" + (r.ToInt()+1).ToString() + "段の駒数が多すぎます。");

                    var piece = Util.FromUsiPiece(c);
                    if (piece == Piece.NO_PIECE)
                        throw new SfenException("SFEN形式の駒'" + c + "'が正しくありません。");

                    piece = piece + (promoted ? Piece.PROMOTE.ToInt() : 0);

                    PutPiece(Util.MakeSquare( f , r) , piece);
                    f -= 1;
                    promoted = false;
                }
            }

            if (f.ToInt() >= 0)
                throw new SfenException("SFEN形式の" + r.ToString() + "段の駒数が合いません。");

            // --- 持ち駒を読み込む

            var hand_sfen = split[2];
            if (string.IsNullOrEmpty(hand_sfen))
                throw new SfenException("SFEN形式の手駒がありません。");

            Array.Clear(hand, 0, hand.Length);

            // 手駒なしでなければ
            if (hand_sfen[0] != '-')
            {
                var count = 1;
                foreach (var c in hand_sfen)
                {
                    if ('1' <= c && c <= '9')
                    {
                        count = c - '0';
                    }
                    else
                    {
                        var piece = Util.FromUsiPiece(c);
                        if (piece == Piece.NO_PIECE)
                        {
                            throw new SfenException("SFEN形式の持ち駒'" + c + "'が正しくありません。");
                        }

                        var color = piece.PieceColor();

                        // 手駒を加算する
                        Hand(color).Add(piece.RawPieceType(), count);

                        count = 1;
                    }
                }
            }

            // --- 手番

            var turn_sfen = split[1];

            if (turn_sfen.Length != 1 || (turn_sfen[0] != 'b' && turn_sfen[0] != 'w'))
                throw new SfenException("SFEN形式の手番表現が正しくありません。");

            sideToMove = Util.FromUsiColor(turn_sfen[0]);

            // --- 手数

            try
            {
                gamePly = int.Parse(split[3]);
            } catch
            {
                // 手数が書かれていないときは、0にしておく。
                gamePly = 0;
            }

            // これをもって読み込みが成功したと言える。

            // StateInfoの更新
            SetState(st);
        }


        /// <summary>
        /// 指し手で盤面を1手進める
        /// </summary>
        /// <param name="m"></param>
        public void DoMove(Move m)
        {
            // ----------------------
            //    盤面の更新処理
            // ----------------------

            // 移動先の升
            Square to = m.To();
            if (!to.IsOk())
                throw new PositionException("DoMoveでtoが範囲外");

            // StateInfoの更新
            var newSt = new StateInfo
            {
                previous = st,
                Key = st.Key
            };
            st = newSt;

            if (m.IsDrop())
            {
                // --- 駒打ち

                // 盤上にその駒を置く。
                Piece pt = m.DroppedPiece();
                if (pt < Piece.PAWN || Piece.GOLD < pt || Hand(sideToMove).Count(pt) == 0)
                    throw new PositionException("Position.DoMove()で持っていない手駒" + pt.Pretty2() + "を打とうとした");

                Piece pc = Util.MakePiece(sideToMove, pt);
                PutPiece(to, pc);
                Hand(sideToMove).Sub(pt);

                // hash keyの更新
                st.Key -= Zobrist.Hand(sideToMove,pt);
                st.Key += Zobrist.Psq(to,pc);
            }
            else
            {
                // -- 駒の移動

                Square from = m.From();
                Piece moved_pc = RemovePiece(from);

                // 移動先の升にある駒

                Piece to_pc = PieceOn(to);
                if (to_pc != Piece.NO_PIECE)
                {
                    // 駒取り

                    // 自分の手駒になる
                    Piece pr = to_pc.RawPieceType();
                    if (!(Piece.PAWN <= pr && pr <= Piece.GOLD))
                        throw new PositionException("Position.DoMove()で取れない駒を取った(玉取り？)");

                    Hand(sideToMove).Add(pr);

                    // 捕獲された駒が盤上から消えるので局面のhash keyを更新する
                    st.Key -= Zobrist.Psq(to, to_pc);
                    st.Key += Zobrist.Hand(sideToMove,pr);
                }

                Piece moved_after_pc = (Piece)(moved_pc.ToInt() + (m.IsPromote() ? Piece.PROMOTE.ToInt() : 0));  
                PutPiece(to, moved_after_pc);

                // fromにあったmoved_pcがtoにmoved_after_pcとして移動した。
                st.Key -= Zobrist.Psq(from, moved_pc      );
                st.Key += Zobrist.Psq(to  , moved_after_pc);
            }

            sideToMove.Flip();

            // Zobrist.sideはp1==0が保証されているのでこれで良い
            st.Key.p0 ^= Zobrist.Side.p0;
        }

        /// <summary>
        /// 指し手で盤面を1手戻す
        /// </summary>
        /// <param name="move"></param>
        public void UndoMove(Move move)
        {

        }

        /// <summary>
        /// USIのpositionコマンドの"position"以降を解釈してその局面にする
        /// "position [sfen <sfenstring> | startpos ] moves <move1> ... <movei>"
        /// 
        /// 解釈で失敗した場合、例外が飛ぶ
        /// </summary>
        /// <param name="pos_cmd"></param>
        public void UsiPositionCmd(string pos_cmd)
        {
            // スペースをセパレータとして分離する
            var split = pos_cmd.Split(
                new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            // どうなっとるねん..
            if (split.Length == 0)
                return;

            // 現在の指し手が書かれている場所 split[cur_pos]
            var cur_pos = 1;
            if (split[0] == "sfen")
            {
                // "sfen ... moves ..."形式かな..
                // movesの手前までをくっつけてSetSfen()する
                while( cur_pos < split.Length && split[cur_pos] != "moves")
                {
                    ++cur_pos;
                }

                if (!(cur_pos== 4 || cur_pos == 5))
                    throw new PositionException("Position.UsiPositionCmd()に渡された文字列にmovesが出てこない");

                if (cur_pos == 4)
                    SetSfen(string.Format("{0} {1} {2}", split[1], split[2], split[3]));
                else // if (cur_pos == 5)
                    SetSfen(string.Format("{0} {1} {2} {3}", split[1], split[2], split[3], split[4]));

            } else if (split[0] == "startpos")
            {
                SetSfen(SFEN_HIRATE);
            }

            // "moves"以降の文字列をUSIの指し手と解釈しながら、局面を進める。
            if (cur_pos < split.Length && split[cur_pos] == "moves")
                for (int i = cur_pos + 1; i < split.Length; ++i)
                    DoMove(Util.FromUsiMove(split[i]));
        }

        // -------------------------------------------------------------------------
        // 以下、private methods
        // -------------------------------------------------------------------------

        /// <summary>
        /// StateInfoの値を初期化する。
        /// やねうら王から移植
        /// </summary>
        /// <param name="si"></param>
        private void SetState(StateInfo si)
        {
            // --- bitboard

            // この局面で自玉に王手している敵駒
            //st->checkersBB = attackers_to(~sideToMove, king_square(sideToMove));

            // 王手情報の初期化
            //set_check_info < false > (si);

            // --- hash keyの計算
            si.Key = sideToMove == Color.BLACK ? Zobrist.Zero : Zobrist.Side;
            for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
            {
                var pc = PieceOn(sq);
                si.Key += Zobrist.Psq(sq,pc);
            }
            for (Color c = Color.ZERO; c < Color.NB; ++c)
                for (Piece pr = Piece.PAWN; pr < Piece.HAND_NB; ++pr)
                    si.Key += Zobrist.Hand(c,pr) * Hand(c).Count(pr); // 手駒はaddにする(差分計算が楽になるため)
        }

        /// <summary>
        /// 盤面上のsqの升にpcを置く。
        /// そこが空き升でなければ例外を投げる
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="pr"></param>
        private void PutPiece(Square sq, Piece pc)
        {
            if (!sq.IsOk() || PieceOn(sq) != Piece.NO_PIECE)
                throw new PositionException("PutPiece(" + sq.Pretty() + "," + pc.Pretty() +")に失敗しました。");

            PieceOn(sq) = pc;

            // 玉であれば、KingSquareを更新する
            if (pc.PieceType() == Piece.KING)
                KingSquare(pc.PieceColor()) = sq;
        }

        /// <summary>
        /// 盤上のsqの升から駒を取り除く。sqにあった駒が返る。
        /// そこに駒がなければ例外を投げる
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        private Piece RemovePiece(Square sq)
        {
            if (!sq.IsOk() || PieceOn(sq) == Piece.NO_PIECE)
                throw new PositionException("RemovePieceに失敗しました。");

            Piece pc = PieceOn(sq);
            PieceOn(sq) = Piece.NO_PIECE;

            // 玉であれば、KingSquareを更新する
            if (pc.PieceType() == Piece.KING)
                KingSquare(pc.PieceColor()) = Square.NB;

            return pc;
        }
    }
}
