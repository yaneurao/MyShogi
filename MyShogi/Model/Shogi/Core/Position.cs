using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// Position(局面)の付随情報を格納する構造体
    /// </summary>
    public class StateInfo
    {
        /// <summary>
        /// 現在の局面のhash key
        /// </summary>
        public HASH_KEY key;

        /// <summary>
        /// この手番側の連続王手は何手前からやっているのか(連続王手の千日手の検出のときに必要)
        /// </summary>
        public int[] continuousCheck = new int[(int)Color.NB];

        /// <summary>
        /// Position.DoMove()する直前の指し手。
        /// デバッグ時などにおいてその局面までの手順を表示出来ると便利なことがあるのでそのための機能
        /// あと、棋譜を表示するときに「同歩」のように直前の指し手のto(行き先の升)が分からないといけないのでこれを用いると良い。
        /// </summary>
        public Move lastMove;

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
        /// この局面で捕獲された駒。先後の区別あり。
        /// ※　次の局面にDoMove()で進むときにこの値が設定される
        /// </summary>
        public Piece capturedPiece;

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
        // 生の配列が欲しい時に用いる。(SfenFromRawdata()などで。普段はこのメソッドは呼び出さない。)
        public Piece[] RawBoard { get { return board; } }
        private PieceNo[] board_pn = new PieceNo[Square.NB_PLUS1.ToInt()];

        /// <summary>
        /// 手駒
        /// </summary>
        private Hand[] hands = new Hand[(int)Color.NB + 1/*駒箱*/];
        // 生の配列が欲しい時に用いる。(SfenFromRawdata()などで。普段はこのメソッドは呼び出さない。)
        public Hand[] RawHands { get { return hands; } }
        private PieceNo[,,] hand_pn = new PieceNo[(int)Color.NB, (int)Piece.HAND_NB, (int)PieceNo.PAWN_MAX];
        // →　どこまで使用してあるかは、Hand(Color,Piece)を用いればわかる。

        // 使用しているPieceNoの終端
        public PieceNo lastPieceNo { get; private set; }

        /// <summary>
        /// 手番
        /// </summary>
        public Color sideToMove { get; private set; } = Color.BLACK;

        /// <summary>
        /// 玉の位置
        /// </summary>
        private Square[] kingSquare = new Square[Color.NB.ToInt()];

        /// <summary>
        /// 初期局面からの手数(初期局面 == 1)
        /// </summary>
        public int gamePly { get; private set; } = 1;

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
        public HASH_KEY Key() { return st.key; }

        // 盤上の先手/後手/両方の駒があるところが1であるBitboard
        public Bitboard[] byColorBB = new Bitboard[(int)Color.NB];

        // 駒が存在する升を表すBitboard。先後混在。
        // pieces()の引数と同じく、ALL_PIECES,HDKなどのPieceで定義されている特殊な定数が使える。
        public Bitboard[] byTypeBB = new Bitboard[(int)Piece.PIECE_BB_NB];

        /// <summary>
        /// 駒落ちであるかのフラグ
        /// 盤面を初期化した時に、駒箱に駒があれば駒落ちと判定。
        /// (片玉は駒落ちとして扱わない)
        /// </summary>
        public bool Handicapped;

        // -------------------------------------------------------------------------

        /// <summary>
        /// このオブジェクトをコピーする。
        /// immutableなオブジェクトが欲しいときに用いる。
        /// </summary>
        /// <returns></returns>
        public Position Clone()
        {
            var pos = new Position();

            // C#では多次元配列もこの方法でCopy()出来ることは保証されている。

            Array.Copy(board, pos.board, board.Length);
            Array.Copy(board_pn, pos.board_pn, board_pn.Length);
            Array.Copy(hands, pos.hands, hands.Length);
            Array.Copy(hand_pn, pos.hand_pn, hand_pn.Length);
            pos.lastPieceNo = lastPieceNo;
            pos.sideToMove = sideToMove;
            Array.Copy(kingSquare, pos.kingSquare, kingSquare.Length);
            pos.gamePly = gamePly;
            pos.st = st; // stの先は参照透明。DoMove()の時に新規に作られ、更新はこのタイミングでしか行われないので。
            Array.Copy(byColorBB, pos.byColorBB, byColorBB.Length);
            Array.Copy(byTypeBB, pos.byTypeBB, byTypeBB.Length);
            pos.Handicapped = Handicapped;

            return pos;
        }

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
        /// 盤上、手駒上のsqの位置にある駒。
        /// 手駒の場合、その手駒を持っていなければPiece.NO_PIECEが返る。
        /// また、後手の場合、後手の駒(Piece.W_PAWNなど)が返る。
        /// SquareHand.NBに対してもPiece.NO_PIECEが返る。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public Piece PieceOn(SquareHand sq)
        {
            if (sq.IsBoardPiece())
            {
                // -- 盤上の駒

                return PieceOn((Square)sq);
            }
            else if (sq.IsHandPiece())
            {
                // -- 手駒

                var pt = sq.ToPiece();
                var c = sq.PieceColor();
                if (Hand(c).Count(pt) > 0)
                    return Util.MakePiece(c, pt);

                // この手駒を持っていないなら、ここを抜けてPiece.NO_PIECEが返る。

            } else if (sq.IsPieceBox())
            {
                // -- 駒箱の駒

                var pt = sq.ToPiece();

                // 玉に関しては駒箱(Hand[Color.NB])に入っていないので、
                // Square.NBにあれば、駒箱に入っているものとして扱い、Piece.KINGを返す。
                if (pt == Piece.KING)
                    return (KingSquare(Color.BLACK) == Square.NB || KingSquare(Color.WHITE) == Square.NB) ? Piece.KING : Piece.NO_PIECE;
                // 玉以外の駒であれば駒箱を見て、1枚以上あるならそのpiece typeを返す。
                else
                    return Hand(Color.NB).Count(pt) > 0 ? pt : Piece.NO_PIECE;

            } else // if (sq == SquareHand.NB)
            {
                //  return Piece.NO_PIECE;
            }

            return Piece.NO_PIECE;
        }

        /// <summary>
        /// 駒箱にある駒の数を返す。
        /// </summary>
        /// <param name="pt">Piece.PAWN～KINGまで。</param>
        /// <returns></returns>
        public int PieceBoxCount(Piece pt)
        {
            Debug.Assert(Piece.PAWN <= pt && pt <= Piece.KING);

            if (pt == Piece.KING)
                return (KingSquare(Color.BLACK) == Square.NB ? 1 : 0) +
                       (KingSquare(Color.WHITE) == Square.NB ? 1 : 0);
            else
                return hands[(int)Color.NB].Count(pt);
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
        /// 
        /// c==Color.NBを渡すと駒箱にある駒が返る。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public ref Hand Hand(Color c)
        {
            return ref hands[(int)c];
        }

        /// <summary>
        /// c側の玉のSquareへの参照
        /// 
        /// 玉が盤上にいない場合はSquare.NBが返ることが保証されている。
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
            if ((b & (~pinned | Bitboard.FileBB(to.ToFile()))).IsNotZero())
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

            Bitboard escape_bb = Bitboard.KingEffect(sq_king) & ~Pieces(us.Not());
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

        public Bitboard Pieces(Color c, Piece pr1, Piece pr2)
        {
            return Pieces(pr1, pr2) & Pieces(c);
        }

        public Bitboard Pieces(Color c, Piece pr1, Piece pr2, Piece pr3)
        {
            return Pieces(pr1, pr2, pr3) & Pieces(c);
        }

        public Bitboard Pieces(Color c, Piece pr1, Piece pr2, Piece pr3, Piece pr4)
        {
            return Pieces(pr1, pr2, pr3, pr4) & Pieces(c);
        }

        public Bitboard Pieces(Color c, Piece pr1, Piece pr2, Piece pr3, Piece pr4, Piece pr5)
        {
            return Pieces(pr1, pr2, pr3, pr4, pr5) & Pieces(c);
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
        public static readonly string SFEN_HIRATE = Sfens.HIRATE;
        
        /// それ以外の駒落ちなどのsfen文字列については、
        /// BoardType.ToSfen()などで取得すること。

        // -------------------------------------------------------------------------

        /// <summary>
        /// このクラスを特定の局面で初期化する
        /// デフォルトでは平手で初期化
        /// boardTypeとして範囲外の値を指定した場合は例外が飛ぶ。
        /// </summary>
        public void InitBoard(BoardType boardType = BoardType.NoHandicap)
        {
            if (!boardType.IsSfenOk())
                throw new PositionException("範囲外のBoardTypeを指定してPosition.init()を呼び出した");

            // 平手で初期化
            SetSfen(boardType.ToSfen());
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
            foreach (var c in All.Colors())
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

            foreach (var c in All.Colors())
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

            Array.Clear(hands, 0, hands.Length);

            // 手駒なしでなければ
            if (hand_sfen[0] != '-')
            {
                var count = 0;
                foreach (var c in hand_sfen)
                {
                    if ('0' <= c && c <= '9')
                    {
                        count = count * 10 + (c - '0');
                        if (count == 0 || count > 18)
                        {
                            throw new SfenException("持ち駒の枚数指定が正しくありません。");
                        }
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

                        if (count == 0) count = 1;

                        // 手駒を加算する
                        Hand(color).Add(pr, count);

                        for(int i=0;i<count;++i)
                        {
                            HandPieceNo(color, pr, i) = lastPieceNo++;
                        }

                        count = 0;
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

            // -- update

            // hash keyの更新
            SetState(st);

            // PutPiece()などを呼び出したので更新する。
            UpdateBitboards();

            // このタイミングで王手関係の情報を更新しておいてやる。
            SetCheckInfo(st);

            // -- 駒落ちであるかの判定

            // 不要駒は駒箱に入っているものとして処理する。
            {
                var h = Core.Hand.ALL;

                foreach (var sq in All.Squares())
                {
                    var pt = PieceOn(sq).RawPieceType();
                    if (pt != Piece.NO_PIECE && pt!=Piece.KING && h.Count(pt) >= 1) // 0以下ならこれ以上削れない。玉もノーカウント
                        h.Sub(pt);
                }

                foreach (var c in All.Colors())
                    for (Piece pt = Piece.PAWN; pt < Piece.HAND_NB; ++pt)
                    {
                        int count = Hand(c).Count(pt);

                        if (h.Count(pt) < count)
                            h.Sub(pt, h.Count(pt)); // 無い駒は引けないので0にしておく。
                        else
                            h.Sub(pt, count);
                    }

                // 駒箱に駒があるので駒落ちの局面である。(片玉の場合は駒落ちとして扱わない)
                Handicapped = h != 0;

                // 駒箱の駒
                hands[(int)Color.NB] = h;
            }

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
                key = st.key
            };
            st = newSt;

            var us = sideToMove;
            var them = us.Not();

            if (m.IsDrop())
            {
                // --- 駒打ち

                // 盤上にその駒を置く。
                Piece pt = m.DroppedPiece();
                if (pt < Piece.PAWN || Piece.GOLD < pt || Hand(us).Count(pt) == 0)
                    throw new PositionException("Position.DoMove()で持っていない手駒" + pt.Pretty2() + "を打とうとした");

                Piece pc = Util.MakePiece(us, pt);
                Hand(us).Sub(pt);

                // 打つ駒の駒番号取得して、これを盤面に置く駒に反映させておく。
                PieceNo pn = HandPieceNo(us, pt, Hand(us).Count(pt));

                PutPiece(to, pc , pn);

                // hash keyの更新
                st.key -= Zobrist.Hand(us,pt);
                st.key += Zobrist.Psq(to,pc);

                // 駒打ちは捕獲した駒がない。
                st.capturedPiece = Piece.NO_PIECE;
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
                    HandPieceNo(us, pr, Hand(us).Count(pr)) = pn2;

                    Hand(us).Add(pr);

                    // 捕獲された駒が盤上から消えるので局面のhash keyを更新する
                    st.key -= Zobrist.Psq(to, to_pc);
                    st.key += Zobrist.Hand(us,pr);

                    // toの地点から元あった駒をいったん取り除く
                    RemovePiece(to);

                    // 駒打ちは捕獲した駒がない。
                    st.capturedPiece = to_pc;
                } else
                {
                    st.capturedPiece = Piece.NO_PIECE;
                }

                Piece moved_after_pc = (Piece)(moved_pc.ToInt() + (m.IsPromote() ? Piece.PROMOTE.ToInt() : 0));
                PutPiece(to, moved_after_pc , pn);

                // fromにあったmoved_pcがtoにmoved_after_pcとして移動した。
                st.key -= Zobrist.Psq(from, moved_pc      );
                st.key += Zobrist.Psq(to  , moved_after_pc);
            }

            sideToMove = us.Not();

            // -- update

            // PutPiece()などを呼び出したので更新する。
            UpdateBitboards();

            // このタイミングで王手関係の情報を更新しておいてやる。
            SetCheckInfo(st);

            // 直前の指し手の更新
            st.lastMove = m;

            // Zobrist.sideはp1==0が保証されているのでこれで良い
            st.key.p.p0 ^= Zobrist.Side.p.p0;

            gamePly++;
        }

        /// <summary>
        /// 指し手で盤面を1手戻す
        /// </summary>
        public void UndoMove()
        {
            // Usは1手前の局面での手番
            var us = sideToMove.Not();
            var m = st.lastMove;

           var to = m.To();
            //ASSERT_LV2(is_ok(to));

            // --- 移動後の駒

            Piece moved_after_pc = PieceOn(to);

            // 移動前の駒
            Piece moved_pc = m.IsPromote() ? (moved_after_pc - (int)Piece.PROMOTE) : moved_after_pc;

            if (m.IsDrop())
            {
                // --- 駒打ち

                // toの場所にある駒を手駒に戻す
                Piece pt = moved_after_pc.RawPieceType();

                var pn = PieceNoOn(to);
                HandPieceNo(us, pt, hands[(int)us].Count(pt)) = pn;

                hands[(int)us].Add(pt);

                // toの場所から駒を消す
                RemovePiece(to);
                PieceNoOn(to) = PieceNo.NONE;
            }
            else
            {
                // --- 通常の指し手

                var from = m.From();
                //ASSERT_LV2(is_ok(from));

                // toの場所にあった駒番号
                var pn = PieceNoOn(to);

                // toの場所から駒を消す
                RemovePiece(to);

                // toの地点には捕獲された駒があるならその駒が盤面に戻り、手駒から減る。
                // 駒打ちの場合は捕獲された駒があるということはありえない。
                // (なので駒打ちの場合は、st->capturedTypeを設定していないから参照してはならない)
                if (st.capturedPiece != Piece.NO_PIECE)
                {
                    Piece to_pc = st.capturedPiece;
                    Piece pr = to_pc.RawPieceType();

                    // 盤面のtoの地点に捕獲されていた駒を復元する
                    var pn2 = HandPieceNo(us, pr, hands[(int)us].Count(pr) - 1);
                    PutPiece(to, to_pc , pn2);
                    PutPiece(from, moved_pc , pn);

                    // 手駒から減らす
                    hands[(int)us].Sub(pr);
                }
                else
                {
                    PutPiece(from, moved_pc , pn);
                    PieceNoOn(to) = PieceNo.NONE;
                }
            }

            // PutPiece()などを呼び出したので更新する。
            UpdateBitboards();

            // --- 相手番に変更
            sideToMove = us; // Usは先後入れ替えて呼び出されているはず。

            // --- StateInfoを巻き戻す
            st = st.previous;

            --gamePly;
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
                {
                    // デバッグ用に盤面を出力
                    //Console.WriteLine(Pretty());

                    var move = Util.FromUsiMove(split[i]);
                    if (!IsLegal(move))
                        throw new PositionException(string.Format("{0}手目が非合法手です。", i - cur_pos));

                    DoMove(move);
                }
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

            // 駒打ちと駒打ちでない指し手とで条件分岐

            // toの場所に来るPieceType
            Piece toPcType;

            if (m.IsDrop())
            {
                // 打つ駒
                Piece pr = toPcType = m.DroppedPiece();

                // 打てないはずの駒
                if (pr < Piece.PAWN && Piece.KING <= pr)
                    return false;

                // 打つ先の升が埋まっていたり、その手駒を持っていなかったりしたら駄目。
                if (PieceOn(to) != Piece.NO_PIECE || Hand(us).Count(pr) == 0)
                    return false;

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
                        case Piece.LANCE:
                            // 歩・香の2段目の不成も合法なので合法として扱う。
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

                // 王の自殺チェック
                if (pc.PieceType() == Piece.KING)
                {
                    // もし移動させる駒が玉であるなら、行き先の升に相手側の利きがないかをチェックする。
                    if (EffectedTo(us.Not(), to, from))
                        return false;
                } else
                {
                    // 王手がされているとき/いないとき、共通の処理
                    // 王以外を動かすケースについて

                    var b = (PinnedPieces(us) & from).IsZero() // ピンされていない駒の移動は自由である
                            || Util.IsAligned(from, to, KingSquare(us)); // ピンされている方角への移動は合法

                    if (!b)
                        return false;
                }

                toPcType = pc.PieceType();
            }

            // 王手がされているなら
            // 王手回避手になっているかどうかのチェックが必要

            if (InCheck() && toPcType != Piece.KING)
            {
                Bitboard target = Checkers();
                Square checksq = target.Pop();

                // 王手している駒を1個取り除いて、もうひとつあるということは王手している駒が
                // 2つあったということであり、両王手なので合い利かず。
                if (target.IsNotZero())
                    return false;

                // 王と王手している駒との間の升に駒を打っていない場合、それは王手を回避していることに
                // ならないので、これは非合法手。

                // 王手している駒が1つなら、王手している駒を取る指し手であるか、
                // 遮断する指し手でなければならない

                if (!((Bitboard.BetweenBB(checksq, KingSquare(us)) & to).IsNotZero() || checksq == to))
                    return false;
            }

            // すべてのテストの合格したので合法手である
            return true;
        }

        /// <summary>
        /// 連続王手の千日手等で引き分けかどうかを返す
        /// 千日手でなければRepetitionState.NONEが返る。
        /// </summary>
        /// <returns></returns>
        public RepetitionState IsRepetition()
        {
            // 現在の局面と同じhash keyを持つ局面が4回あれば、それは千日手局面であると判定する。

            // n回st.previousを辿るlocal method
            StateInfo prev(StateInfo si, int n)
            {
                for (int i = 0; i < n; ++i)
                {
                    si = si.previous;
                    if (si == null)
                        break;
                }
                return si;
            };

            // 4手かけないと千日手にはならないから、4手前から調べていく。
            StateInfo stp = prev(st, 4);
            // 遡った手数のトータル
            int t = 4;

            // 同一である局面が出現した回数
            int cnt = 0;

            //Console.WriteLine("--Start--");
            //Console.WriteLine(st.key.Pretty());

            while (stp != null)
            {
                //Console.WriteLine(stp.key.Pretty());

                // HashKeyは128bitもあるのでこのチェックで現実的には間違いないだろう。
                if (stp.key == st.key)
                {
                    // 同一局面が4回出現した時点で千日手が成立
                    if (++cnt == 3)
                    {
                        // 自分が王手をしている連続王手の千日手なのか？
                        if (t <= st.continuousCheck[(int)sideToMove])
                            return RepetitionState.LOSE;

                        // 相手が王手をしている連続王手の千日手なのか？
                        if (t <= st.continuousCheck[(int)sideToMove.Not()])
                            return RepetitionState.WIN;

                        return RepetitionState.DRAW;
                    }
                }
                // ここから2手ずつ遡る
                stp = prev(stp, 2);
                t += 2;
            }

            // 同じhash keyの局面が見つからなかったので…。
            return RepetitionState.NONE;
        }

        /// <summary>
        /// この局面で手番側が詰んでいるか(合法な指し手がないか)
        /// 実際に指し手生成をして判定を行うので、引数として指し手生成バッファを渡してやる必要がある。
        /// </summary>
        /// <returns></returns>
        public bool IsMated(Move[] moves)
        {
            return InCheck() && MoveGen.LegalAll(this, moves, 0) == 0;
        }

        /// <summary>
        /// 捕獲する指し手であるか
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool IsCapture(Move m)
        {
            return PieceOn(m.To()) != Piece.NO_PIECE;
        }

        /// <summary>
        /// 宣言勝ちできる局面であるかを判定する。
        ///
        /// 宣言勝ちできる局面でなければMove.NONEが返る。
        /// 宣言勝ちできる局面であればMove.WINが返る。
        ///
        /// ruleでトライルール(TRY_RULE)を指定している場合は、トライ(玉を51の升に移動させること)出来る条件を
        /// 満たしているなら、その指し手を返す。
        /// </summary>
        /// <returns></returns>
        public Move DeclarationWin(EnteringKingRule rule)
        {
            switch (rule)
            {
                // 入玉ルールなし
                case EnteringKingRule.NONE: return Move.NONE;

                // CSAルールに基づく宣言勝ちの条件を満たしているか
                // 満たしているならば非0が返る。返し値は駒点の合計。
                // cf.http://www.computer-shogi.org/protocol/tcp_ip_1on1_11.html
                case EnteringKingRule.POINT24: // 24点法(31点以上で宣言勝ち)
                case EnteringKingRule.POINT27: // 27点法 == CSAルール
                    {
                        /*
                        「入玉宣言勝ち」の条件(第13回選手権で使用のもの):
                        次の条件が成立する場合、勝ちを宣言できる(以下「入玉宣言勝ち」と云う)。
                        条件:
                        (a) 宣言側の手番である。
                        (b) 宣言側の玉が敵陣三段目以内に入っている。
                        (c) 宣言側が(大駒5点小駒1点の計算で)
                        ・先手の場合28点以上の持点がある。
                        ・後手の場合27点以上の持点がある。
                        ・点数の対象となるのは、宣言側の持駒と敵陣三段目
                        以内に存在する玉を除く宣言側の駒のみである。
                        (d) 宣言側の敵陣三段目以内の駒は、玉を除いて10枚以上存在する。
                        (e) 宣言側の玉に王手がかかっていない。
                        (詰めろや必死であることは関係ない)
                        (f) 宣言側の持ち時間が残っている。(切れ負けの場合)
                        以上1つでも条件を満たしていない場合、宣言した方が負けとなる。
                        (注) このルールは、日本将棋連盟がアマチュアの公式戦で使用しているものである。
                        以上の宣言は、コンピュータが行い、画面上に明示する。
                        */
                        // (a)宣言側の手番である。
                        // →　手番側でこの関数を呼び出して判定するのでそうだろう。

                        Color us = sideToMove;

                        // 敵陣
                        Bitboard ef = Bitboard.EnemyField(us);

                        // (b)宣言側の玉が敵陣三段目以内に入っている。
                        if ((ef & KingSquare(us)).IsZero())
                            return Move.NONE;

                        // (e)宣言側の玉に王手がかかっていない。
                        if (InCheck())
                            return Move.NONE;


                        // (d)宣言側の敵陣三段目以内の駒は、玉を除いて10枚以上存在する。
                        int p1 = (Pieces(us) & ef).PopCount();
                        // p1には玉も含まれているから11枚以上ないといけない
                        if (p1 < 11)
                            return Move.NONE;

                        // 敵陣にいる大駒の数
                        int p2 = ((Pieces(us, Piece.BISHOP_HORSE, Piece.ROOK_DRAGON)) & ef).PopCount();

                        // 小駒1点、大駒5点、玉除く
                        // ＝　敵陣の自駒 + 敵陣の自駒の大駒×4 - 玉

                        // (c)
                        // ・先手の場合28点以上の持点がある。
                        // ・後手の場合27点以上の持点がある。
                        Hand h = Hand(us);
                        int score = p1 + p2 * 4 - 1
                            + h.Count(Piece.PAWN) + h.Count(Piece.LANCE) + h.Count(Piece.KNIGHT) + h.Count(Piece.SILVER)
                            + h.Count(Piece.GOLD) + (h.Count(Piece.BISHOP) + h.Count(Piece.ROOK)) * 5;

                        // rule==EKR_27_POINTならCSAルール。rule==EKR_24_POINTなら24点法(30点以下引き分けなので31点以上あるときのみ勝ち扱いとする)
                        if (score < (rule == EnteringKingRule.POINT27 ? (us == Color.BLACK ? 28 : 27) : 31))
                            return Move.NONE;

                        // 評価関数でそのまま使いたいので非0のときは駒点を返しておく。
                        return Move.WIN;
                    }

                // トライルールの条件を満たしているか。
                case EnteringKingRule.TRY_RULE:
                    {
                        Color us = sideToMove;
                        Square king_try_sq = (us == Color.BLACK ? Square.SQ_51 : Square.SQ_59);
                        Square king_sq = KingSquare(us);

                        // 1) 初期陣形で敵玉がいた場所に自玉が移動できるか。
                        if ((Bitboard.KingEffect(king_sq) & king_try_sq).IsZero())
                            return Move.NONE;

                        // 2) トライする升に自駒がないか。
                        if ((Pieces(us) & king_try_sq).IsNotZero())
                            return Move.NONE;

                        // 3) トライする升に移動させたときに相手に取られないか。
                        if (EffectedTo(us.Not(), king_try_sq, king_sq))
                            return Move.NONE;

                        // 王の移動の指し手により勝ちが確定する
                        return Util.MakeMove(king_sq, king_try_sq);
                    }

            }

            return Move.NONE;
        }

        /// <summary>
        /// 盤面と手駒、手番を与えて、そのsfenを返す。
        /// </summary>
        /// <param name="board"></param>
        /// <param name=""></param>
        /// <param name="hands"></param>
        /// <param name=""></param>
        /// <param name="turn"></param>
        /// <param name="gamePly"></param>
        /// <returns></returns>
        public static string SfenFromRawdata(Piece[/*81*/] board, Hand[/*2 or 3*/] hands, Color turn, int gamePly)
        {
            // 内部的な構造体にコピーして、sfen()を呼べば、変換過程がそこにしか依存していないならば
            // これで正常に変換されるのでは…。
            var pos = new Position();

            Array.Copy(board, pos.board , 81);
            Array.Copy(hands, pos.hands  , 2);
            pos.sideToMove = turn;
            pos.gamePly = gamePly;

            return pos.ToSfen();

            // ↑の実装、美しいが、いかんせん遅い。
            // 棋譜を大量に読み込ませて学習させるときにここがボトルネックになるので直接unpackする関数を書く。(べき)
        }

        // -------------------------------------------------------------------------
        // 利き
        // -------------------------------------------------------------------------

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
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        public bool EffectedTo(Color c, Square sq)
        {
            return AttackersTo(c, sq, Pieces()).IsNotZero();
        }

        /// <summary>
        /// kingSqの地点からは玉を取り除いての利きの判定を行なう。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <param name="kingSq"></param>
        /// <returns></returns>
        public bool EffectedTo(Color c, Square sq, Square kingSq)
        {
            return AttackersTo(c, sq, Pieces() ^ kingSq).IsNotZero();
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
            si.key = sideToMove == Color.BLACK ? Zobrist.Zero : Zobrist.Side;
            foreach (var sq in All.Squares())
            {
                var pc = PieceOn(sq);
                si.key += Zobrist.Psq(sq, pc);
            }
            foreach (var c in All.Colors())
                for (Piece pr = Piece.PAWN; pr < Piece.HAND_NB; ++pr)
                    si.key += Zobrist.Hand(c, pr) * Hand(c).Count(pr); // 手駒はaddにする(差分計算が楽になるため)
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

            if (st.previous != null)
            {
                // DoMove()前の手番 == them
                var us = them.Not();

                // 王手しているなら連続王手の値を更新する
                st.continuousCheck[(int)them] = (st.checkersBB.IsNotZero()) ? st.previous.continuousCheck[(int)them] + 2 : 0;

                // 相手番のほうは関係ないので前ノードの値をそのまま受け継ぐ。
                st.continuousCheck[(int)us] = st.previous.continuousCheck[(int)us];
            }
        }
    }
}
