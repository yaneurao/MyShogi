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
        /// この手番側の連続王手は何手前からやっているのか(連続王手の千日手の検出のときに必要)
        /// </summary>
        public int[] continuousCheck = new int[(int)Color.NB];

        /// <summary>
        /// 現局面で手番側に対して王手をしている駒のbitboard
        /// </summary>
        public Bitboard checkersBB;

        // 動かすと手番側の王に対して空き王手になるかも知れない駒の候補
        // チェスの場合、駒がほとんどが大駒なのでこれらを動かすと必ず開き王手となる。
        // 将棋の場合、そうとも限らないので移動方向について考えなければならない。
        // color = 手番側 なら pinされている駒(動かすと開き王手になる)
        // color = 相手側 なら 両王手の候補となる駒。

        /// <summary>
        /// 自玉に対して(敵駒によって)pinされている駒
        /// </summary>
        public Bitboard[] blockersForKing = new Bitboard[(int)Color.NB];

        /// <summary>
        /// 自玉に対してpinしている(可能性のある)敵の大駒。
        /// 自玉に対して上下左右方向にある敵の飛車、斜め十字方向にある敵の角、玉の前方向にある敵の香、…
        /// </summary>
        public Bitboard[] pinnersForKing = new Bitboard[(int)Color.NB];

        /// <summary>
        /// 自駒の駒種Xによって敵玉が王手となる升のbitboard
        /// </summary>
        public Bitboard[] checkSquares = new Bitboard[(int)Piece.WHITE];

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
        private PieceNo[] board_pn = new PieceNo[Square.NB_PLUS1.ToInt()];

        /// <summary>
        /// 手駒
        /// </summary>
        private Hand[] hand = new Hand[Color.NB.ToInt()];
        private PieceNo[,,] hand_pn = new PieceNo[(int)Color.NB, (int)Piece.HAND_NB, (int)PieceNo.PAWN_MAX];
        // →　どこまで使用してあるかは、Hand(Color,Piece)を用いればわかる。

        // 使用しているPieceNoの終端
        PieceNo lastPieceNo;

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

        // 盤上の先手/後手/両方の駒があるところが1であるBitboard
        public Bitboard[] byColorBB = new Bitboard[(int)Color.NB];

        // 駒が存在する升を表すBitboard。先後混在。
        // pieces()の引数と同じく、ALL_PIECES,HDKなどのPieceで定義されている特殊な定数が使える。
        public Bitboard[] byTypeBB = new Bitboard[(int)Piece.PIECE_BB_NB];

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
        /// 盤面上、sqの升にある駒のPieceNoの参照
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public ref PieceNo PieceNoOn(Square sq)
        {
            return ref board_pn[sq.ToInt()];
        }

        /// <summary>
        /// c側の手駒ptのno枚目の駒のPieceNoの参照
        /// 駒の枚数自体はHand(Color).Count()で取得できるのでそちらを用いること。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pt"></param>
        /// <param name="no"></param>
        /// <returns></returns>
        public ref PieceNo HandPieceNo(Color c, Piece pt, int no)
        {
            return ref hand_pn[(int)c,(int)pt,no];
        }

        /// <summary>
        /// c側の手駒の参照
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public ref Hand Hand(Color c)
        {
            return ref hand[(int)c];
        }

        /// <summary>
        /// c側の玉のSquareへの参照
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public ref Square KingSquare(Color c)
        {
            return ref kingSquare[(int)c];
        }

        /// <summary>
        /// 現局面で王手がかかっているか
        /// </summary>
        /// <returns></returns>
        public bool InCheck()
        {
            return Checkers().IsNotZero();
        }

        /// <summary>
        /// 合法な打ち歩か。
        /// 二歩でなく、かつ打ち歩詰めでないならtrueを返す。
        /// </summary>
        /// <param name="us"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool LegalPawnDrop(Color us,Square to)
        {
            return !(((Pieces(us, Piece.PAWN) & Bitboard.FileBB(to.ToFile())).IsNotZero())                   // 二歩
                || ((Bitboard.PawnEffect(us, to) == new Bitboard(KingSquare(us.Not())) && !LegalDrop(to)))); // 打ち歩詰め
        }

        /// <summary>
        /// toの地点に歩を打ったときに打ち歩詰めにならないならtrue。
        /// 歩をtoに打つことと、二歩でないこと、toの前に敵玉がいることまでは確定しているものとする。
        /// 二歩の判定もしたいなら、legal_pawn_drop()のほうを使ったほうがいい。
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool LegalDrop(Square to)
        {
            var us = sideToMove;

            // 打とうとする歩の利きに相手玉がいることは前提条件としてクリアしているはず。
            // ASSERT_LV3(pawnEffect(us, to) == Bitboard(king_square(~us)));

            // この歩に利いている自駒(歩を打つほうの駒)がなければ詰みには程遠いのでtrue
            if (!EffectedTo(us, to))
                return true;

            // ここに利いている敵の駒があり、その駒で取れるなら打ち歩詰めではない
            // ここでは玉は除外されるし、香が利いていることもないし、そういう意味では、特化した関数が必要。
            Bitboard b = AttackersToPawn(us.Not(), to);

            // このpinnedは敵のpinned pieces
            Bitboard pinned = PinnedPieces(us.Not());

            // pinされていない駒が1つでもあるなら、相手はその駒で取って何事もない。
            if ((b & (pinned.Not() | Bitboard.FileBB(to.ToFile()))).IsNotZero())
                return true;

            // 攻撃駒はすべてpinされていたということであり、
            // 王の頭に打たれた打ち歩をpinされている駒で取れるケースは、
            // いろいろあるが、例1),例2)のような場合であるから、例3)のケースを除き、
            // いずれも玉の頭方向以外のところからの玉頭方向への移動であるから、
            // pinされている方向への移動ということはありえない。
            // 例3)のケースを除くと、この歩は取れないことが確定する。
            // 例3)のケースを除外するために同じ筋のものはpinされていないものとして扱う。
            //    上のコードの　 " | FILE_BB[file_of(to)] " の部分がそれ。

            // 例1)
            // ^玉 ^角  飛
            //  歩

            // 例2)
            // ^玉
            //  歩 ^飛
            //          角

            // 例3)
            // ^玉
            //  歩
            // ^飛
            //  香

            // 玉の退路を探す
            // 自駒がなくて、かつ、to(はすでに調べたので)以外の地点

            // 相手玉の場所
            Square sq_king = KingSquare(us.Not());

            // LONG EFFECT LIBRARYがない場合、愚直に8方向のうち逃げられそうな場所を探すしかない。

            Bitboard escape_bb = Bitboard.KingEffect(sq_king) & Pieces(us.Not()).Not();
            escape_bb ^= to;
            var occ = Pieces() ^ to; // toには歩をおく前提なので、ここには駒があるものとして、これでの利きの遮断は考えないといけない。
            while (escape_bb.IsNotZero())
            {
                Square king_to = escape_bb.Pop();
                if (AttackersTo(us, king_to, occ).IsZero())
                    return true; // 退路が見つかったので打ち歩詰めではない。
            }

            // すべての検査を抜けてきたのでこれは打ち歩詰めの条件を満たしている。
            return false;
        }

        // -------------------------------------------------------------------------
        // occupied bitboardなど
        // -------------------------------------------------------------------------

        /// <summary>
        /// 先手か後手か、いずれかの駒がある場所が1であるBitboardが返る。
        /// </summary>
        /// <returns></returns>
        public Bitboard Pieces()
        {
            return byTypeBB[(int)Piece.ALL_PIECES];
        }

        /// <summary>
        /// c == BLACK : 先手の駒があるBitboardが返る
        /// c == WHITE : 後手の駒があるBitboardが返る
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Bitboard Pieces(Color c)
        {
            return byColorBB[(int)c];
        }

        /// <summary>
        /// 特定の駒種のBitboardを返す。
        /// </summary>
        /// <param name="pr"></param>
        /// <returns></returns>
        public Bitboard Pieces(Piece pr)
        {
            // ASSERT_LV3(pr<PIECE_BB_NB);
            return byTypeBB[(int)pr];
        }

        /// <summary>
        /// pr1とpr2の駒種を合成した(足し合わせた)Bitboardを返す。
        /// </summary>
        /// <param name="pr1"></param>
        /// <param name="pr2"></param>
        /// <returns></returns>
        public Bitboard Pieces(Piece pr1, Piece pr2)
        {
            return Pieces(pr1) | Pieces(pr2);
        }

        public Bitboard Pieces(Piece pr1, Piece pr2,Piece pr3)
        {
            return Pieces(pr1) | Pieces(pr2) | Pieces(pr3);
        }

        public Bitboard Pieces(Piece pr1, Piece pr2, Piece pr3 ,Piece pr4)
        {
            return Pieces(pr1) | Pieces(pr2) | Pieces(pr3) | Pieces(pr4);
        }

        public Bitboard Pieces(Piece pr1, Piece pr2, Piece pr3, Piece pr4 , Piece pr5)
        {
            return Pieces(pr1) | Pieces(pr2) | Pieces(pr3) | Pieces(pr4) | Pieces(pr5);
        }


        /// <summary>
        /// c側の駒種prのbitboardを返す
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pr"></param>
        /// <returns></returns>
        public Bitboard Pieces(Color c, Piece pr)
        {
            return Pieces(pr) & Pieces(c);
        }

        // 駒がない升が1になっているBitboardが返る
        public Bitboard Empties()
        {
            return Pieces() ^ Bitboard.AllBB();
        }

        // --- 王手

            /// <summary>
        /// 原局面で王手している駒のBitboardが返る
        /// </summary>
        /// <returns></returns>
        public Bitboard Checkers()
        {
            return st.checkersBB;
        }

        /// <summary>
        /// 移動させると(相手側＝非手番側)の玉に対して空き王手となる候補の(手番側)駒のbitboard。
        /// </summary>
        /// <returns></returns>
        public Bitboard DiscoveredCheckCandidates()
        {
            return st.blockersForKing[(int)sideToMove.Not()] & Pieces(sideToMove);
        }

        /// <summary>
        /// ピンされているc側の駒。下手な方向に移動させるとc側の玉が素抜かれる。
        /// 手番側のpinされている駒はpos.pinned_pieces(pos.side_to_move())のようにして取得できる。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Bitboard PinnedPieces(Color c)
        {
            // ASSERT_LV3(is_ok(c));
            return st.blockersForKing[(int)c] & Pieces(c);
        }

        /// <summary>
        /// 現局面で駒Ptを動かしたときに王手となる升を表現するBitboard
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
	    public Bitboard CheckSquares(Piece pt)
        {
            // ASSERT_LV3(pt!= NO_PIECE && pt<PIECE_WHITE);
            return st.checkSquares[(int)pt];
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
        /// PieceNoがどうなっているか表示する。(デバッグ用)
        /// </summary>
        /// <returns></returns>
        public string PrettyPieceNo()
        {
            var sb = new StringBuilder();

            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
            {
                for (File f = File.FILE_9; f >= File.FILE_1; --f)
                {
                    var pn = PieceNoOn(Util.MakeSquare(f,r));
                    sb.Append(string.Format("{0:D2} ",(int)pn));
                }
                sb.AppendLine();
            }

            for(Color c = Color.ZERO; c < Color.NB; ++c)
            {
                sb.Append(c.Pretty() + ":");
                for(Piece p = Piece.PAWN; p < Piece.HAND_NB; ++p)
                {
                    int count = Hand(c).Count(p);
                    if (count == 0)
                        continue;

                    sb.Append(p.Pretty());
                    for (int i = 0; i < count; ++i)
                    {
                        var pn = HandPieceNo(c,p,i);
                        sb.Append(string.Format("{0:D2} ", (int)pn));
                    }
                }
                sb.AppendLine();
            }

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

            KingSquare(Color.BLACK) = KingSquare(Color.WHITE) = Square.NB;

            // 各Bitboard配列のゼロクリア
            Array.Clear(board, 0, board.Length);
            Array.Clear(board_pn, 0, board_pn.Length);
            Array.Clear(hand_pn, 0, hand_pn.Length);
            Array.Clear(byColorBB, 0, byColorBB.Length);
            Array.Clear(byTypeBB, 0, byTypeBB.Length);

            // 盤面左上から。Square型のレイアウトに依らずに処理を進めたいため、Square型は使わない。
            File f = File.FILE_9;
            Rank r = Rank.RANK_1;
            lastPieceNo = PieceNo.ZERO;

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

                    PutPiece(Util.MakeSquare( f , r) , piece , lastPieceNo ++);
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
                        var pr = piece.RawPieceType();

                        // 手駒を加算する
                        Hand(color).Add(pr, count);

                        for(int i=0;i<count;++i)
                        {
                            HandPieceNo(color, pr, i) = lastPieceNo++;
                        }

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

            // PutPiece()などを呼び出したので更新する。
            UpdateBitboards();
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
                Hand(sideToMove).Sub(pt);

                // 打つ駒の駒番号取得して、これを盤面に置く駒に反映させておく。
                PieceNo pn = HandPieceNo(sideToMove, pt, Hand(sideToMove).Count(pt));

                PutPiece(to, pc , pn);

                // hash keyの更新
                st.Key -= Zobrist.Hand(sideToMove,pt);
                st.Key += Zobrist.Psq(to,pc);
            }
            else
            {
                // -- 駒の移動

                Square from = m.From();
                PieceNo pn = PieceNoOn(from);
                Piece moved_pc = RemovePiece(from);

                // 移動元の駒ナンバーをクリア
                PieceNoOn(from) = PieceNo.NONE;

                // 移動先の升にある駒

                Piece to_pc = PieceOn(to);
                if (to_pc != Piece.NO_PIECE)
                {
                    // 駒取り

                    // 自分の手駒になる
                    Piece pr = to_pc.RawPieceType();
                    if (!(Piece.PAWN <= pr && pr <= Piece.GOLD))
                        throw new PositionException("Position.DoMove()で取れない駒を取った(玉取り？)");

                    // 取る駒のPieceNoを盤上に反映させておく
                    PieceNo pn2 = PieceNoOn(to);
                    HandPieceNo(sideToMove, pr, Hand(sideToMove).Count(pr)) = pn2;

                    Hand(sideToMove).Add(pr);

                    // 捕獲された駒が盤上から消えるので局面のhash keyを更新する
                    st.Key -= Zobrist.Psq(to, to_pc);
                    st.Key += Zobrist.Hand(sideToMove,pr);
                }

                Piece moved_after_pc = (Piece)(moved_pc.ToInt() + (m.IsPromote() ? Piece.PROMOTE.ToInt() : 0));  
                PutPiece(to, moved_after_pc , pn);

                // fromにあったmoved_pcがtoにmoved_after_pcとして移動した。
                st.Key -= Zobrist.Psq(from, moved_pc      );
                st.Key += Zobrist.Psq(to  , moved_after_pc);
            }

            sideToMove.Flip();

            // Zobrist.sideはp1==0が保証されているのでこれで良い
            st.Key.p.p0 ^= Zobrist.Side.p.p0;

            // -- update

            // PutPiece()などを呼び出したので更新する。
            UpdateBitboards();

            // このタイミングで王手関係の情報を更新しておいてやる。
            SetCheckInfo(st);
        }

        /// <summary>
        /// 指し手で盤面を1手戻す
        /// </summary>
        /// <param name="move"></param>
        public void UndoMove(Move move)
        {

            // PutPiece()などを呼び出したので更新する。
            UpdateBitboards();
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

        /// <summary>
        /// ※　mがこの局面においてpseudo_legalかどうかを判定するための関数。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool IsLegal(Move m)
        {
            Color us = sideToMove;
            Square to = m.To(); // 移動先

            // 駒打ちと駒打ちでない指し手とで条件分離

            if (m.IsDrop())
            {
                // 打つ駒
                Piece pr = m.DroppedPiece();

                // 打てないはずの駒
                if (pr < Piece.PAWN && Piece.KING <= pr)
                    return false;

                // 打つ先の升が埋まっていたり、その手駒を持っていなかったりしたら駄目。
                if (PieceOn(to) != Piece.NO_PIECE || Hand(us).Count(pr) == 0)
                    return false;

                if (InCheck())
                {
                    // 王手されている局面なので合駒でなければならない
                    Bitboard target = Checkers();
                    Square checksq = target.Pop();

                    // 王手している駒を1個取り除いて、もうひとつあるということは王手している駒が
                    // 2つあったということであり、両王手なので合い利かず。
                    if (target.IsNotZero())
                        return false;

                    // 王と王手している駒との間の升に駒を打っていない場合、それは王手を回避していることに
                    // ならないので、これは非合法手。
                    if ((Bitboard.BetweenBB(checksq, KingSquare(us)) & to).IsZero())
                        return false;
                }

                // --- 移動できない升への歩・香・桂打ちについて
                switch (pr)
                {
                    case Piece.PAWN:
                        // 歩のとき、二歩および打ち歩詰めであるなら非合法手
                        if (!LegalPawnDrop(us, to))
                            return false;
                        if (to.ToRank() == (us == Color.BLACK ? Rank.RANK_1 : Rank.RANK_9))
                            return false;

                        break;

                    case Piece.LANCE:
                        if (to.ToRank() == (us == Color.BLACK ? Rank.RANK_1 : Rank.RANK_9))
                            return false;

                        break;

                    case Piece.KNIGHT:
                        if ((us == Color.BLACK && to.ToRank() <= Rank.RANK_2) ||
                            (us == Color.WHITE && to.ToRank() >= Rank.RANK_8))
                            return false;

                        break;
                }
            }
            else
            {
                // 移動させる指し手

                Square from = m.From();
                // 移動させる駒
                Piece pc = PieceOn(from);

                // 動かす駒が自駒でなければならない
                if (pc == Piece.NO_PIECE || pc.PieceColor() != us)
                    return false;

                // toに移動できないといけない。
                // (fromに駒を置いたときにtoに利きがないと駄目)
                if ((Bitboard.EffectsFrom(pc, from, Pieces()) & to).IsZero())
                    return false;

                // toの地点に自駒があるといけない
                if ((Pieces(us) & to).IsNotZero())
                    return false;

                Piece pt = pc.PieceType();
                if (m.IsPromote())
                {
                    // --- 成る指し手

                    // 成れない駒の成りではないことを確かめないといけない。
                    // static_assert(GOLD == 7, "GOLD must be 7.");
                    if (pt >= Piece.GOLD)
                        return false;

                    // 移動先が敵陣でないと成れない。
                    if ((Bitboard.EnemyField(us) & (new Bitboard(from) | new Bitboard(to))).IsZero())
                        return false;

                } else
                {
                    // --- 成らない指し手

                    // 先手の歩の1段目へ不成での移動などは出来ない。このチェック。

                    // --- 移動できない升への歩・香・桂打ちについて
                    switch (pt)
                    {
                        case Piece.PAWN:
                            // 歩のとき、二歩および打ち歩詰めであるなら非合法手
                            if (!LegalPawnDrop(us, to))
                                return false;

                            // 歩・香の2段目の不成も合法なので合法として扱う。
                            if (to.ToRank() == (us == Color.BLACK ? Rank.RANK_1 : Rank.RANK_9))
                                return false;

                            break;

                        case Piece.LANCE:
                            if (to.ToRank() == (us == Color.BLACK ? Rank.RANK_1 : Rank.RANK_9))
                                return false;

                            break;

                        case Piece.KNIGHT:
                            if ((us == Color.BLACK && to.ToRank() <= Rank.RANK_2) ||
                                (us == Color.WHITE && to.ToRank() >= Rank.RANK_8))
                                return false;

                            break;
                    }

                }

                // 王手している駒があるのか
                if (InCheck())
                {
                    // このとき、指し手生成のEVASIONで生成される指し手と同等以上の条件でなければならない。

                    // 動かす駒は王以外か？
                    if (pc.PieceType() != Piece.KING)
                    {
                        // 両王手なら王の移動をさせなければならない。
                        if (Bitboard.MoreThanOne(Checkers()))
                            return false;

                        // 指し手は、王手を遮断しているか、王手している駒の捕獲でなければならない。
                        // ※　王手している駒と王の間に王手している駒の升を足した升が駒の移動先であるか。
                        // 例) 王■■■^飛
                        // となっているときに■の升か、^飛 のところが移動先であれば王手は回避できている。
                        // (素抜きになる可能性はあるが、そのチェックはここでは不要)
                        if (((Bitboard.BetweenBB(Checkers().Pop(), KingSquare(us)) | Checkers()) & to).IsZero())
                            return false;
                    }
                }
            }

            // すべてのテストの合格したので合法手である
            return true;
        }


        /// <summary>
        /// 現局面でsqに利いているC側の駒を列挙する
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        public Bitboard AttackersTo(Color c, Square sq)
        {
            return AttackersTo(c, sq, Pieces());
        }

        public Bitboard AttackersTo(Color c, Square sq, Bitboard occ)
        {
            // assert(is_ok(c) && sq <= SQ_NB);

            Color them = c.Not();

            // sの地点に敵駒ptをおいて、その利きに自駒のptがあればsに利いているということだ。
            // 香の利きを求めるコストが惜しいのでrookEffect()を利用する。
            return
                ((Bitboard.PawnEffect(them, sq) & Pieces(Piece.PAWN))
                    | (Bitboard.KnightEffect(them, sq) & Pieces(Piece.KNIGHT))
                    | (Bitboard.SilverEffect(them, sq) & Pieces(Piece.SILVER_HDK))
                    | (Bitboard.GoldEffect(them, sq) & Pieces(Piece.GOLDS_HDK))
                    | (Bitboard.BishopEffect(sq, occ) & Pieces(Piece.BISHOP_HORSE))
                    | (Bitboard.RookEffect(sq, occ) & (
                            Pieces(Piece.ROOK_DRAGON)
                        | (Bitboard.LanceStepEffect(them, sq) & Pieces(Piece.LANCE))
                        ))
                    //  | (kingEffect(sq) & pieces(c, HDK));
                    // →　HDKは、銀と金のところに含めることによって、参照するテーブルを一個減らして高速化しようというAperyのアイデア。
                    ) & Pieces(c); // 先後混在しているのでc側の駒だけ最後にマスクする。
            ;

        }

        /// <summary>
        /// 現局面でsqに利いている駒を列挙する
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public Bitboard AttackersTo(Square sq)
        {
            return AttackersTo(sq, Pieces());
        }

        public Bitboard AttackersTo(Square sq, Bitboard occ)
        {
        //        ASSERT_LV3(sq <= SQ_NB);

                // sqの地点に敵駒ptをおいて、その利きに自駒のptがあればsqに利いているということだ。
                return
                    // 先手の歩・桂・銀・金・HDK
                    (((Bitboard.PawnEffect(Color.WHITE, sq) & Pieces(Piece.PAWN))
                        | (Bitboard.KnightEffect(Color.WHITE, sq) & Pieces(Piece.KNIGHT))
                        | (Bitboard.SilverEffect(Color.WHITE, sq) & Pieces(Piece.SILVER_HDK))
                        | (Bitboard.GoldEffect(Color.WHITE, sq) & Pieces(Piece.GOLDS_HDK))
                        ) & Pieces(Color.BLACK))
                    |

                    // 後手の歩・桂・銀・金・HDK
                    (((Bitboard.PawnEffect(Color.BLACK, sq) & Pieces(Piece.PAWN))
                        | (Bitboard.KnightEffect(Color.BLACK, sq) & Pieces(Piece.KNIGHT))
                        | (Bitboard.SilverEffect(Color.BLACK, sq) & Pieces(Piece.SILVER_HDK))
                        | (Bitboard.GoldEffect(Color.BLACK, sq) & Pieces(Piece.GOLDS_HDK))
                        ) & Pieces(Color.WHITE))

                    // 先後の角・飛・香
                    | (Bitboard.BishopEffect(sq, occ) & Pieces(Piece.BISHOP_HORSE))
                    | (Bitboard.RookEffect(sq, occ) & (
                           Pieces(Piece.ROOK_DRAGON)
                        | (Pieces(Color.BLACK, Piece.LANCE) & Bitboard.LanceStepEffect(Color.WHITE, sq))
                        | (Pieces(Color.WHITE, Piece.LANCE) & Bitboard.LanceStepEffect(Color.BLACK, sq))
                        // 香も、StepEffectでマスクしたあと飛車の利きを使ったほうが香の利きを求めなくて済んで速い。
                        ));
            }

        /// <summary>
        /// 打ち歩詰め判定に使う。王に打ち歩された歩の升をpawn_sqとして、c側(王側)のpawn_sqへ利いている駒を列挙する。香が利いていないことは自明。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pawn_sq"></param>
        /// <returns></returns>
        public Bitboard AttackersToPawn(Color c, Square pawn_sq)
        {
        //    ASSERT_LV3(is_ok(c) && pawn_sq <= SQ_NB);

            Color them = c.Not();
            Bitboard occ = Pieces();

            // 馬と龍
            Bitboard bb_hd = Bitboard.KingEffect(pawn_sq) & Pieces(Piece.HORSE, Piece.DRAGON);
            // 馬、龍の利きは考慮しないといけない。しかしここに玉が含まれるので玉は取り除く必要がある。
            // bb_hdは銀と金のところに加えてしまうことでテーブル参照を一回減らす。

            // sの地点に敵駒ptをおいて、その利きに自駒のptがあればsに利いているということだ。
            // 打ち歩詰め判定なので、その打たれた歩を歩、香、王で取れることはない。(王で取れないことは事前にチェック済)
            return
                ((Bitboard.KnightEffect(them, pawn_sq) & Pieces(Piece.KNIGHT))
                    | (Bitboard.SilverEffect(them, pawn_sq) & (Pieces(Piece.SILVER) | bb_hd))
                    | (Bitboard.GoldEffect(them, pawn_sq) & (Pieces(Piece.GOLDS) | bb_hd))
                    | (Bitboard.BishopEffect(pawn_sq, occ) & Pieces(Piece.BISHOP_HORSE))
                    | (Bitboard.RookEffect(pawn_sq, occ) & Pieces(Piece.ROOK_DRAGON))
                    ) & Pieces(c);
        }

        /// <summary>
        /// attackers_to()で駒があればtrueを返す版。(利きの情報を持っているなら、軽い実装に変更できる)
        /// kingSqの地点からは玉を取り除いての利きの判定を行なう。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        public bool EffectedTo(Color c, Square sq)
        {
            return AttackersTo(c, sq, Pieces()).IsNotZero();
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
        private void PutPiece(Square sq, Piece pc , PieceNo pn)
        {
            if (!sq.IsOk() || PieceOn(sq) != Piece.NO_PIECE)
                throw new PositionException("PutPiece(" + sq.Pretty() + "," + pc.Pretty() +")に失敗しました。");

            PieceOn(sq) = pc;
            PieceNoOn(sq) = pn;

            // 玉であれば、KingSquareを更新する
            if (pc.PieceType() == Piece.KING)
                KingSquare(pc.PieceColor()) = sq;

            XorPiece(pc, sq);
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

            XorPiece(pc, sq);

            return pc;
        }

        /// <summary>
        /// 駒を置く/取り除くときに呼び出すと、byColorBB,byTypeBBを更新する。
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="sq"></param>
        private void XorPiece(Piece pc, Square sq)
        {
            // 先手・後手の駒のある場所を示すoccupied bitboardの更新
            byColorBB[(int)pc.PieceColor()] ^= sq;

            // 先手 or 後手の駒のある場所を示すoccupied bitboardの更新
            byTypeBB[(int)Piece.ALL_PIECES] ^= sq;

            // 駒別のBitboardの更新
            // これ以外のBitboardの更新は、update_bitboards()で行なう。
            byTypeBB[(int)pc.PieceType()] ^= sq;
        }

        /// <summary>
        /// put_piece(),remove_piece(),xor_piece()を用いたあとに呼び出す必要がある。
        /// </summary>
        void UpdateBitboards()
        {
            // 王・馬・龍を合成したbitboard
            byTypeBB[(int)Piece.HDK] = Pieces(Piece.KING, Piece.HORSE, Piece.DRAGON);

            // 金と同じ移動特性を持つ駒
            byTypeBB[(int)Piece.GOLDS] = Pieces(Piece.GOLD, Piece.PRO_PAWN, Piece.PRO_LANCE, Piece.PRO_KNIGHT, Piece.PRO_SILVER);

            // 以下、attackers_to()で頻繁に用いるのでここで1回計算しておいても、トータルでは高速化する。

            // 角と馬
            byTypeBB[(int)Piece.BISHOP_HORSE] = Pieces(Piece.BISHOP, Piece.HORSE);

            // 飛車と龍
            byTypeBB[(int)Piece.ROOK_DRAGON] = Pieces(Piece.ROOK, Piece.DRAGON);

            // 銀とHDK
            byTypeBB[(int)Piece.SILVER_HDK] = Pieces(Piece.SILVER, Piece.HDK);

            // 金相当の駒とHDK
            byTypeBB[(int)Piece.GOLDS_HDK] = Pieces(Piece.GOLDS, Piece.HDK);
        }

        /// <summary>
        /// 升sに対して、c側の大駒に含まれる長い利きを持つ駒の利きを遮っている駒のBitboardを返す(先後の区別なし)
        /// ※　Stockfishでは、sildersを渡すようになっているが、大駒のcolorを渡す実装のほうが優れているので変更。
        /// [Out] pinnersとは、pinされている駒が取り除かれたときに升sに利きが発生する大駒である。これは返し値。
        /// また、升sにある玉は~c側のKINGであるとする。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        /// <param name="pinners"></param>
        /// <returns></returns>
        public Bitboard SliderBlockers(Color c, Square s, Bitboard pinners)
        {
            Bitboard result = Bitboard.ZeroBB();

            // pinnersは返し値。
            pinners = Bitboard.ZeroBB();

            // cが与えられていないと香の利きの方向を確定させることが出来ない。
            // ゆえに将棋では、この関数は手番を引数に取るべき。(チェスとはこの点において異なる。)

            // snipersとは、pinされている駒が取り除かれたときに升sに利きが発生する大駒である。
            Bitboard snipers =
                ((Pieces(Piece.ROOK_DRAGON) & Bitboard.RookStepEffect(s))
                | (Pieces(Piece.BISHOP_HORSE) & Bitboard.BishopStepEffect(s))
                // 香に関しては攻撃駒が先手なら、玉より下側をサーチして、そこにある先手の香を探す。
                | (Pieces(Piece.LANCE) & Bitboard.LanceStepEffect(c.Not(), s))
                ) & Pieces(c);

            while (snipers.IsNotZero())
            {
                Square sniperSq = snipers.Pop();
                Bitboard b = Bitboard.BetweenBB(s, sniperSq) & Pieces();

                // snipperと玉との間にある駒が1個であるなら。
                // (間にある駒が0個の場合、b == ZERO_BBとなり、何も変化しない。)
                if (!Bitboard.MoreThanOne(b))
                {
                    result |= b;
                    if ((b & Pieces(c.Not())).IsNotZero())
                        // sniperと玉に挟まれた駒が玉と同じ色の駒であるなら、pinnerに追加。
                        pinners |= sniperSq;
                }
            }
            return result;
        }


        /// <summary>
        /// StateInfoの初期化(初期化するときに内部的に用いる)
        /// </summary>
        /// <param name="si"></param>
        private void SetCheckInfo(StateInfo si)
        {
            // --- bitboard

            // この局面で自玉に王手している敵駒
            st.checkersBB = AttackersTo(sideToMove.Not(), KingSquare(sideToMove));

            // -- 王手情報の初期化

            //: si->blockersForKing[WHITE] = slider_blockers(pieces(BLACK), square<KING>(WHITE),si->pinnersForKing[WHITE]);
            //: si->blockersForKing[BLACK] = slider_blockers(pieces(WHITE), square<KING>(BLACK),si->pinnersForKing[BLACK]);

            // ↓Stockfishのこの部分の実装、将棋においては良くないので、以下のように変える。

            //if (!doNullMove)
            {
                // null moveのときは前の局面でこの情報は設定されているので更新する必要がない。
                si.blockersForKing[(int)Color.WHITE] = SliderBlockers(Color.BLACK, KingSquare(Color.WHITE), si.pinnersForKing[(int)Color.WHITE]);
                si.blockersForKing[(int)Color.BLACK] = SliderBlockers(Color.WHITE, KingSquare(Color.BLACK), si.pinnersForKing[(int)Color.BLACK]);
            }

            Square ksq = KingSquare(sideToMove.Not());

            // 駒種Xによって敵玉に王手となる升のbitboard

            // 歩であれば、自玉に敵の歩を置いたときの利きにある場所に自分の歩があればそれは敵玉に対して王手になるので、
            // そういう意味で(ksq,them)となっている。

            Bitboard occ = Pieces();
            Color them = sideToMove.Not();

            // この指し手が二歩でないかは、この時点でテストしない。指し手生成で除外する。なるべくこの手のチェックは遅延させる。
            si.checkSquares[(int)Piece.PAWN]   = Bitboard.PawnEffect(them, ksq);
            si.checkSquares[(int)Piece.KNIGHT] = Bitboard.KnightEffect(them, ksq);
            si.checkSquares[(int)Piece.SILVER] = Bitboard.SilverEffect(them, ksq);
            si.checkSquares[(int)Piece.BISHOP] = Bitboard.BishopEffect(ksq, occ);
            si.checkSquares[(int)Piece.ROOK]   = Bitboard.RookEffect(ksq, occ);
            si.checkSquares[(int)Piece.GOLD]   = Bitboard.GoldEffect(them, ksq);

            // 香で王手になる升は利きを求め直さずに飛車で王手になる升を香のstep effectでマスクしたものを使う。
            si.checkSquares[(int)Piece.LANCE] = si.checkSquares[(int)Piece.ROOK] & Bitboard.LanceStepEffect(them, ksq);

            // 王を移動させて直接王手になることはない。それは自殺手である。
            si.checkSquares[(int)Piece.KING] = Bitboard.ZeroBB();

            // 成り駒。この初期化は馬鹿らしいようだが、gives_check()は指し手ごとに呼び出されるので、その処理を軽くしたいので
            // ここでの初期化は許容できる。(このコードはdo_move()に対して1回呼び出されるだけなので)
            si.checkSquares[(int)Piece.PRO_PAWN]   = si.checkSquares[(int)Piece.GOLD];
            si.checkSquares[(int)Piece.PRO_LANCE]  = si.checkSquares[(int)Piece.GOLD];
            si.checkSquares[(int)Piece.PRO_KNIGHT] = si.checkSquares[(int)Piece.GOLD];
            si.checkSquares[(int)Piece.PRO_SILVER] = si.checkSquares[(int)Piece.GOLD];
            si.checkSquares[(int)Piece.HORSE]      = si.checkSquares[(int)Piece.BISHOP] | Bitboard.KingEffect(ksq);
            si.checkSquares[(int)Piece.DRAGON]     = si.checkSquares[(int)Piece.ROOK]   | Bitboard.KingEffect(ksq);
        }
    }
}
