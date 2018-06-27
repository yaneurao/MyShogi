using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// Bitboard
    /// 駒の利きなどを表現する
    /// やねうら王から移植
    /// 
    /// classではなくstructなので注意。
    /// </summary>
    public struct Bitboard
    {
        /// <summary>
        /// Bitboardの実体
        /// </summary>
        public UInt128 p;

        // -------------------------------------------------------------------------
        // コンストラクタ
        // -------------------------------------------------------------------------

        /// <summary>
        /// 128bit構造体で初期化するコンストラクタ
        /// </summary>
        /// <param name="p_"></param>
        public Bitboard(UInt128 p_)
        {
            p = p_;
        }

        /// <summary>
        /// 64bit整数２つで初期化するコンストラクタ
        /// </summary>
        /// <param name="p_"></param>
        public Bitboard(UInt64 p0_, UInt64 p1_)
        {
            p = new UInt128(p0_, p1_);
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="b"></param>
        public Bitboard(Bitboard b)
        {
            p = b.p;
        }

        /// <summary>
        /// sqの升が1のBitboardとして初期化する。
        /// </summary>
        /// <param name="sq"></param>
        public Bitboard(Square sq)
        {
            p = SQUARE_BB[sq.ToInt()].p;
        }

        // -------------------------------------------------------------------------
        // bitboardに関するビット単位のand/or/xor演算
        // -------------------------------------------------------------------------

        public static Bitboard operator &(Bitboard c1, Bitboard c2)
        {
            return new Bitboard(c1.p & c2.p);
        }

        public static Bitboard operator |(Bitboard c1, Bitboard c2)
        {
            return new Bitboard(c1.p | c2.p);
        }

        public static Bitboard operator ^(Bitboard c1, Bitboard c2)
        {
            return new Bitboard(c1.p ^ c2.p);
        }

        // 単項演算子
        // →　NOTで書くと、使っていないbit(p[0]のbit63)がおかしくなるのでALL_BBでxorしないといけない。
        public static Bitboard operator ~(Bitboard a)
        {
            return a ^ ALL_BB;
        }


        public static Bitboard operator &(Bitboard c1, Square sq)
        {
            return new Bitboard(c1.p & SQUARE_BB[sq.ToInt()].p);
        }

        public static Bitboard operator |(Bitboard c1, Square sq)
        {
            return new Bitboard(c1.p | SQUARE_BB[sq.ToInt()].p);
        }

        public static Bitboard operator ^(Bitboard c1, Square sq)
        {
            return new Bitboard(c1.p ^ SQUARE_BB[sq.ToInt()].p);
        }

        public static Bitboard operator <<(Bitboard c1, int n)
        {
            // このbit shiftは、p[0]とp[1]をまたがない。
            return new Bitboard(c1.p << n);
        }

        public static Bitboard operator >>(Bitboard c1, int n)
        {
            // このbit shiftは、p[0]とp[1]をまたがない。
            return new Bitboard(c1.p >> n);
        }

        public static bool operator == (Bitboard lhs, Bitboard rhs)
        {
            return lhs.p == rhs.p;
        }

        public static bool operator !=(Bitboard lhs , Bitboard rhs)
        {
            return lhs.p != rhs.p;
        }
        
        public override bool Equals(object o)
        {
            return this.p == ((Bitboard)o).p;
        }

        public override int GetHashCode()
        {
            return p.GetHashCode();
        }

        /// <summary>
        /// 下位bitから1bit拾ってそのbit位置を返す。
        /// 少なくとも1bitは非0と仮定
        /// while(to = bb.Pop())
        ///   Util.MakeMove(from,to);
        /// のように用いる。
        /// </summary>
        /// <returns></returns>
        public Square Pop()
        {
            Debug.Assert(!IsZero());
            return (p.p0 != 0) ? (Square)(BitOp.LSB64(ref p.p0)) : (Square)(BitOp.LSB64(ref p.p1) + 63);
        }

        /// <summary>
        /// 1になっている数を数える
        /// </summary>
        /// <returns></returns>
        public int PopCount()
        {
            return p.p0.PopCount() + p.p1.PopCount();
        }

        /// <summary>
        /// 2bit以上あるかどうかを判定する。縦横斜め方向に並んだ駒が2枚以上であるかを判定する。この関係にないと駄目。
        /// この関係にある場合、Bitboard::merge()によって被覆しないことがBitboardのレイアウトから保証されている。
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool MoreThanOne(Bitboard bb)
        {
            // ASSERT_LV2(!bb.cross_over());
            return bb.Merge().PopCount() > 1;
        }

        // 2升に挟まれている升を返すためのテーブル(その2升は含まない)
        // この配列には直接アクセスせずにbetween_bb()を使うこと。
        // 配列サイズが大きくてcache汚染がひどいのでシュリンクしてある。
        private static Bitboard[] BetweenBB_; // =new Bitboard[785];
        private static UInt16 [,] BetweenIndex; // = new UInt16 [SQ_NB_PLUS1][SQ_NB_PLUS1];

        /// <summary>
        /// 2升に挟まれている升を表すBitboardを返す。sq1とsq2が縦横斜めの関係にないときはZERO_BBが返る。
        /// </summary>
        /// <param name="sq1"></param>
        /// <param name="sq2"></param>
        /// <returns></returns>
        public static Bitboard BetweenBB(Square sq1, Square sq2)
        {
            return BetweenBB_[BetweenIndex[(int)sq1,(int)sq2]];
        }

        // 2升を通過する直線を返すためのテーブル
        // 2つ目のindexは[0]:右上から左下、[1]:横方向、[2]:左上から右下、[3]:縦方向の直線。
        // この配列には直接アクセスせず、line_bb()を使うこと。
        private static Bitboard[,] LineBB_; //[SQ_NB][4];

        /// <summary>
        /// 2升を通過する直線を返すためのテーブル
        /// 2つ目のindexは[0]:右上から左下、[1]:横方向、[2]:左上から右下、[3]:縦方向の直線。
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Bitboard LineBB(Square sq,int type)
        {
            return LineBB_[(int)sq, type];
        }

        /// <summary>
        /// foreach(var sq in bb) .. のように書くためのもの。
        /// そこそこ遅いので速度が要求されるところで使わないこと。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Square> GetEnumerator()
        {
            var bb = this; // BitboardはstructなのでこれはClone()相当

            while (bb.IsNotZero())
                yield return bb.Pop();
        }

        // -------------------------------------------------------------------------
        // public methods
        // -------------------------------------------------------------------------

        /// <summary>
        /// sqの升のbitが立っているかを判定する。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public bool IsSet(Square sq)
        {
            return (p & SQUARE_BB[sq.ToInt()].p).ToU() != 0;
        }

        /// <summary>
        /// すべてのbitが0であるかどうかを判定する。
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return p.ToU() == 0;
        }

        /// <summary>
        /// 1bitでもbitが立っているかどうかを判定する。
        /// </summary>
        /// <returns></returns>
        public bool IsNotZero()
        {
            return p.ToU() != 0;
        }

        /// <summary>
        /// 1になっているbitが1つだけである。
        /// </summary>
        /// <returns></returns>
        public bool IsOne()
        {
            return PopCount() == 1;
        }

        /// <summary>
        /// bitboardを綺麗に出力する
        /// </summary>
        /// <returns></returns>
        public string Pretty()
        {
            var sb = new StringBuilder();

            for (Rank r = Rank.RANK_1; r <= Rank.RANK_9; ++r)
            {
                for (File f = File.FILE_9; f >= File.FILE_1; --f)
                {
                    sb.Append(IsSet(Util.MakeSquare(f, r)) ? '*' : '.');
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        // -------------------------------------------------------------------------
        // 利きを返すbitboardなど
        // -------------------------------------------------------------------------

        /// <summary>
        /// すべての升が1であるBitboard
        /// </summary>
        /// <returns></returns>
        public static Bitboard AllBB()
        {
            return ALL_BB;
        }

        /// <summary>
        /// すべての升が0であるBitboard
        /// </summary>
        /// <returns></returns>
        public static Bitboard ZeroBB()
        {
            return ZERO_BB;
        }

        /// <summary>
        /// 筋を表現するbitboardを返す
        /// </summary>
        /// <param name="f"></param>
        public static Bitboard FileBB(File f)
        {
            return FILE_BB[f.ToInt()];
        }

        /// <summary>
        /// 段を表すbitboardを返す
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Bitboard RankBB(Rank r)
        {
            return RANK_BB[r.ToInt()];
        }

        /// <summary>
        /// sqの升が1であるbitboardを返す
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Bitboard SquareBB(Square sq)
        {
            return SQUARE_BB[(int)sq];
        }

        // ForwardRanksBBの定義)
        //    c側の香の利き = 飛車の利き & ForwardRanksBB[c][rank_of(sq)]
        //
        // すなわち、
        // color == BLACKのとき、n段目よりWHITE側(1からn-1段目)を表現するBitboard。
        // color == WHITEのとき、n段目よりBLACK側(n+1から9段目)を表現するBitboard。
        // このアイデアはAperyのもの。
        public static Bitboard ForwardRanks(Color c, Rank r)
        {
            return ForwardRanksBB[(int)c, (int)r];
        }

        // --- 遠方駒(盤上の駒の状態を考慮しながら利きを求める)

        /// <summary>
        /// 角の右上と左下方向への利き
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        private static Bitboard BishopEffect0(Square sq, Bitboard occupied)
        {
            Bitboard block0 = new Bitboard(occupied & BishopEffectMask[0, (int)sq]);
            return BishopEffectBB[0, BishopEffectIndex[0, (int)sq] + (int)OccupiedToIndex(block0, BishopEffectMask[0, (int)sq])];
        }

        /// <summary>
        /// 角の左上と右下方向への利き
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        private static Bitboard BishopEffect1(Square sq, Bitboard occupied)
        {
            Bitboard block1 = new Bitboard(occupied & BishopEffectMask[1, (int)sq]);
            return BishopEffectBB[1, BishopEffectIndex[1, (int)sq] + (int)OccupiedToIndex(block1, BishopEffectMask[1, (int)sq])];
        }

        /// <summary>
        /// 角 : occupied bitboardを考慮しながら角の利きを求める
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        public static Bitboard BishopEffect(Square sq, Bitboard occupied)
        {
            return BishopEffect0(sq, occupied) | BishopEffect1(sq, occupied);
        }

        /// <summary>
        /// 馬 : occupied bitboardを考慮しながら香の利きを求める
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        public static Bitboard HorseEffect(Square sq, Bitboard occupied)
        {
            return BishopEffect(sq, occupied) | KingEffect(sq);
        }

        // 指定した升(Square)が Bitboard のどちらの u64 変数の要素に属するか。
        // 本ソースコードのように縦型Bitboardにおいては、香の利きを求めるのにBitboardの
        // 片側のp[x]を調べるだけで済むので、ある升がどちらに属するかがわかれば香の利きは
        // そちらを調べるだけで良いというAperyのアイデア。
        private static int Part(Square sq) { return (Square.SQ_79 < sq) ? 1 : 0; }

        /// <summary>
        /// 飛車の縦の利き
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        public static Bitboard RookFileEffect(Square sq, Bitboard occupied)
        {
            UInt64 occ = Part(sq) == 0 ? occupied.p.p0 : occupied.p.p1;
            int index = (int)((occ >> Slide[(int)sq]) & 0x7f);
            File f = sq.ToFile();
            return (f <= File.FILE_7) ?
                new Bitboard(RookFileEffectBB[(int)sq.ToRank(), index] << (int)Util.MakeSquare(f, Rank.RANK_1), 0) :
                new Bitboard(0, RookFileEffectBB[(int)sq.ToRank(), index] << (int)Util.MakeSquare((File)(f - File.FILE_8), Rank.RANK_1));
        }

        /// <summary>
        /// 飛車の横の利き
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        public static Bitboard RookRankEffect(Square sq, Bitboard occupied)
        {
            // 将棋盤をシフトして、SQ_71 , SQ_61 .. SQ_11に飛車の横方向の情報を持ってくる。
            // このbitを直列化して7bit取り出して、これがindexとなる。
            // しかし、r回の右シフトを以下の変数uに対して行なうと計算完了まで待たされるので、
            // PEXT64()の第二引数のほうを左シフトしておく。
            int r = (int)sq.ToRank();
            UInt64 u = (occupied.p.p1 << 6 * 9) + (occupied.p.p0 >> 9);
            UInt64 index = BitOp.PEXT64(u, 0b1000000001000000001000000001000000001000000001000000001UL << r);
            return RookRankEffectBB[(int)sq.ToFile(), index] << r;
        }

        /// <summary>
        /// 飛 : occupied bitboardを考慮しながら飛車の利きを求める
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        public static Bitboard RookEffect(Square sq, Bitboard occupied)
        {
            return RookFileEffect(sq, occupied) | RookRankEffect(sq, occupied);
        }

        /// <summary>
        /// 香 : occupied bitboardを考慮しながら香の利きを求める
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        public static Bitboard LanceEffect(Color c, Square sq, Bitboard occupied)
        {
            return RookFileEffect(sq, occupied) & LanceStepEffect(c, sq);
        }

        /// <summary>
        /// 龍 : occupied bitboardを考慮しながら香の利きを求める
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="occupied"></param>
        /// <returns></returns>
        public static Bitboard DragonEffect(Square sq, Bitboard occupied)
        {
            return RookEffect(sq, occupied) | KingEffect(sq);
        }

        /// <summary>
        /// sqに王をおいたときに利きがある升が1であるbitboardを返す
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Bitboard KingEffect(Square sq)
        {
            return KingEffectBB[sq.ToInt()];
        }

        /// <summary>
        /// 歩の利き
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Bitboard PawnEffect(Color c, Square sq)
        {
            return PawnEffectBB[(int)sq, (int)c];
        }

        // Bitboardに対する歩の利き
        // color = BLACKのとき、51の升は49の升に移動するので、注意すること。
        // (51の升にいる先手の歩は存在しないので、歩の移動に用いる分には問題ないが。)
        public static Bitboard PawnEffect(Color c, Bitboard bb)
        {
            // Apery型の縦型Bitboardにおいては歩の利きはbit shiftで済む。
            //ASSERT_LV3(is_ok(c));
            //return c == BLACK ? bb >> 1 : c == WHITE ? bb << 1
            //    : ZERO_BB;

            return ZERO_BB;
        }

        /// <summary>
        /// 桂の利き
        /// </summary>
        /// <returns></returns>
        public static Bitboard KnightEffect(Color c, Square sq)
        {
            //ASSERT_LV3(is_ok(c) && sq <= SQ_NB);
            return KnightEffectBB[(int)sq, (int)c];
        }

        /// <summary>
        /// 銀の利き
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Bitboard SilverEffect(Color c, Square sq)
        {
            //ASSERT_LV3(is_ok(c) && sq <= SQ_NB);
            return SilverEffectBB[(int)sq, (int)c];
        }

        /// <summary>
        /// 金の利き
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Bitboard GoldEffect(Color c, Square sq)
        {
            //ASSERT_LV3(is_ok(c) && sq <= SQ_NB);
            return GoldEffectBB[(int)sq, (int)c];
        }

        // --- 遠方仮想駒(盤上には駒がないものとして求める利き)

        /// <summary>
        /// 盤上の駒を考慮しない角の利き
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Bitboard BishopStepEffect(Square sq)
        {
            //ASSERT_LV3(sq <= SQ_NB);
            return BishopStepEffectBB[(int)sq];
        }

        /// <summary>
        /// 盤上の駒を考慮しない飛車の利き
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Bitboard RookStepEffect(Square sq)
        {
            //ASSERT_LV3(sq <= SQ_NB);
            return RookStepEffectBB[(int)sq];
        }

        /// <summary>
        /// 盤上の駒を考慮しない香の利き
        /// </summary>
        public static Bitboard LanceStepEffect(Color c, Square sq)
        {
            //ASSERT_LV3(is_ok(c) && sq <= SQ_NB);
            return LanceStepEffectBB[(int)sq, (int)c];
        }

        /// <summary>
        /// 盤上sqに駒pc(先後の区別あり)を置いたときの利き。
        /// pc == QUEENだと馬+龍の利きが返る。
        /// </summary>
        /// <returns></returns>
        public static Bitboard EffectsFrom(Piece pc, Square sq, Bitboard occ)
        {
            switch (pc)
            {
                case Piece.B_PAWN: return PawnEffect(Color.BLACK, sq);
                case Piece.B_LANCE: return LanceEffect(Color.BLACK, sq, occ);
                case Piece.B_KNIGHT: return KnightEffect(Color.BLACK, sq);
                case Piece.B_SILVER: return SilverEffect(Color.BLACK, sq);
                case Piece.B_GOLD: case Piece.B_PRO_PAWN: case Piece.B_PRO_LANCE: case Piece.B_PRO_KNIGHT: case Piece.B_PRO_SILVER: return GoldEffect(Color.BLACK, sq);

                case Piece.W_PAWN: return PawnEffect(Color.WHITE, sq);
                case Piece.W_LANCE: return LanceEffect(Color.WHITE, sq, occ);
                case Piece.W_KNIGHT: return KnightEffect(Color.WHITE, sq);
                case Piece.W_SILVER: return SilverEffect(Color.WHITE, sq);
                case Piece.W_GOLD: case Piece.W_PRO_PAWN: case Piece.W_PRO_LANCE: case Piece.W_PRO_KNIGHT: case Piece.W_PRO_SILVER: return GoldEffect(Color.WHITE, sq);

                //　先後同じ移動特性の駒
                case Piece.B_BISHOP: case Piece.W_BISHOP: return BishopEffect(sq, occ);
                case Piece.B_ROOK: case Piece.W_ROOK: return RookEffect(sq, occ);
                case Piece.B_HORSE: case Piece.W_HORSE: return HorseEffect(sq, occ);
                case Piece.B_DRAGON: case Piece.W_DRAGON: return DragonEffect(sq, occ);
                case Piece.B_KING: case Piece.W_KING: return KingEffect(sq);
                case Piece.B_QUEEN: case Piece.W_QUEEN: return HorseEffect(sq, occ) | DragonEffect(sq, occ);
                case Piece.NO_PIECE: case Piece.WHITE: return ZERO_BB; // これも入れておかないと初期化が面倒になる。

                default: /*UNREACHABLE;*/ return ALL_BB;
            }
        }

        /// <summary>
        /// 敵陣を表現するBitboard。
        /// </summary>
        private static Bitboard[] EnemyFieldBB; // = new Bitboard[(int)Color.NB]{ RANK1_BB | RANK2_BB | RANK3_BB, RANK7_BB | RANK8_BB | RANK9_BB };

        public static Bitboard EnemyField(Color c)
        {
            return EnemyFieldBB[(int)c];
        }

        // -------------------------------------------------------------------------
        // 以下、private methods / tables
        // -------------------------------------------------------------------------

        /// <summary>
        /// p[0]とp[1]をbitwise orしたものを返す。toU()相当。
        /// </summary>
        /// <returns></returns>
        private UInt64 Merge() { return p.p0 | p.p1; }

        // Haswellのpext()を呼び出す。occupied = occupied bitboard , mask = 利きの算出に絡む升が1のbitboard
        // この関数で戻ってきた値をもとに利きテーブルを参照して、遠方駒の利きを得る。
        private static UInt64 OccupiedToIndex(Bitboard occupied, Bitboard mask) { return BitOp.PEXT64(occupied.Merge(), mask.Merge()); }

        /// <summary>
        /// staticなテーブルの初期化
        /// 起動時にInitializerから一度だけ呼び出される。
        /// 普段は呼び出してはならない。
        /// </summary>
        public static void Init()
        {
            ALL_BB = new Bitboard(0x7FFFFFFFFFFFFFFFUL, 0x3FFFFUL);
            ZERO_BB = new Bitboard(0UL, 0UL);

            Bitboard FILE1_BB = new Bitboard((0x1ffUL) << (9 * 0), 0);
            Bitboard FILE2_BB = new Bitboard((0x1ffUL) << (9 * 1), 0);
            Bitboard FILE3_BB = new Bitboard((0x1ffUL) << (9 * 2), 0);
            Bitboard FILE4_BB = new Bitboard((0x1ffUL) << (9 * 3), 0);
            Bitboard FILE5_BB = new Bitboard((0x1ffUL) << (9 * 4), 0);
            Bitboard FILE6_BB = new Bitboard((0x1ffUL) << (9 * 5), 0);
            Bitboard FILE7_BB = new Bitboard((0x1ffUL) << (9 * 6), 0);
            Bitboard FILE8_BB = new Bitboard(0, 0x1ffUL << (9 * 0));
            Bitboard FILE9_BB = new Bitboard(0, 0x1ffUL << (9 * 1));

            FILE_BB = new Bitboard[(int)File.NB]
                { FILE1_BB,FILE2_BB,FILE3_BB,FILE4_BB,FILE5_BB,FILE6_BB,FILE7_BB,FILE8_BB,FILE9_BB };

            Bitboard RANK1_BB = new Bitboard((0x40201008040201UL) << 0, 0x201 << 0);
            Bitboard RANK2_BB = new Bitboard((0x40201008040201UL) << 1, 0x201 << 1);
            Bitboard RANK3_BB = new Bitboard((0x40201008040201UL) << 2, 0x201 << 2);
            Bitboard RANK4_BB = new Bitboard((0x40201008040201UL) << 3, 0x201 << 3);
            Bitboard RANK5_BB = new Bitboard((0x40201008040201UL) << 4, 0x201 << 4);
            Bitboard RANK6_BB = new Bitboard((0x40201008040201UL) << 5, 0x201 << 5);
            Bitboard RANK7_BB = new Bitboard((0x40201008040201UL) << 6, 0x201 << 6);
            Bitboard RANK8_BB = new Bitboard((0x40201008040201UL) << 7, 0x201 << 7);
            Bitboard RANK9_BB = new Bitboard((0x40201008040201UL) << 8, 0x201 << 8);

            RANK_BB = new Bitboard[(int)Rank.NB]
            { RANK1_BB, RANK2_BB, RANK3_BB, RANK4_BB, RANK5_BB, RANK6_BB, RANK7_BB, RANK8_BB, RANK9_BB };

            EnemyFieldBB = new Bitboard[(int)Color.NB]{ RANK1_BB | RANK2_BB | RANK3_BB, RANK7_BB | RANK8_BB | RANK9_BB };

            SQUARE_BB = new Bitboard[(int)Square.NB_PLUS1];

            ForwardRanksBB = new Bitboard[(int)Color.NB, (int)Rank.NB]
            {
              { ZERO_BB, RANK1_BB, RANK1_BB | RANK2_BB, RANK1_BB | RANK2_BB | RANK3_BB, RANK1_BB | RANK2_BB | RANK3_BB | RANK4_BB,
              ~(RANK9_BB | RANK8_BB | RANK7_BB | RANK6_BB), ~(RANK9_BB | RANK8_BB | RANK7_BB), ~(RANK9_BB | RANK8_BB), ~RANK9_BB },
              { ~RANK1_BB, ~(RANK1_BB | RANK2_BB), ~(RANK1_BB | RANK2_BB | RANK3_BB), ~(RANK1_BB | RANK2_BB | RANK3_BB | RANK4_BB),
              RANK9_BB | RANK8_BB | RANK7_BB | RANK6_BB, RANK9_BB | RANK8_BB | RANK7_BB, RANK9_BB | RANK8_BB, RANK9_BB, ZERO_BB }
            };

            // ２つの升のfileの差、rankの差のうち大きいほうの距離を返す。sq1,sq2のどちらかが盤外ならINT_MAXが返る。
            int dist(Square sq1, Square sq2)
            {
                return (!sq1.IsOk() || !sq2.IsOk()) ? int.MaxValue :
                    System.Math.Max(System.Math.Abs(sq1.ToFile() - sq2.ToFile()), System.Math.Abs(sq1.ToRank() - sq2.ToRank()));
            }

            BetweenBB_ = new Bitboard[785];
            BetweenIndex = new UInt16 [(int)Square.NB_PLUS1,(int)Square.NB_PLUS1];


            // 1) SquareWithWallテーブルの初期化。

            for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
                SquareWithWallExtensions.sqww_table[sq.ToInt()] = (SquareWithWall)
                    ((int)SquareWithWall.SQWW_11
                    + sq.ToFile().ToInt() * (int)SquareWithWall.SQWW_L
                    + sq.ToRank().ToInt() * (int)SquareWithWall.SQWW_D);

            // 2) direct_tableの初期化

            Util.direc_table = new Directions[(int)Square.NB_PLUS1 , (int)Square.NB_PLUS1];

            for (var sq1 = Square.ZERO; sq1 < Square.NB; ++ sq1)
                for (var dir = Direct.ZERO; dir < Direct.NB; ++dir)
                {
                    // dirの方角に壁にぶつかる(盤外)まで延長していく。このとき、sq1から見てsq2のDirectionsは (1 << dir)である。
                    var delta = (int)dir.ToDeltaWW();
                    for (var sq2 = sq1.ToSqww() + delta; sq2.IsOk(); sq2 += delta)
                        Util.direc_table[(int)sq1,(int)sq2.ToSquare()] = dir.ToDirections();
                }

            // 3) Square型のsqの指す升が1であるBitboardがSquareBB。これをまず初期化する。

            // SQUARE_BBは上記のRANK_BBとFILE_BBを用いて初期化すると楽。
            for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
            {
                File f = sq.ToFile();
                Rank r = sq.ToRank();

                // 筋と段が交差するところがSQUARE_BB
                SQUARE_BB[sq.ToInt()] = FILE_BB[f.ToInt()] & RANK_BB[r.ToInt()];
            }

            // 4) 遠方利きのテーブルの初期化
            //  thanks to Apery (Takuya Hiraoka)

            // 引数のindexをbits桁の2進数としてみなす。すなわちindex(0から2^bits-1)。
            // 与えられたmask(1の数がbitsだけある)に対して、1のbitのいくつかを(indexの値に従って)0にする。
            Bitboard indexToOccupied(int index, int bits, Bitboard mask)
            {
                var result = ZERO_BB;
                for (int i = 0; i < bits; ++i)
                {
                    Square sq = mask.Pop();
                    if ((index & (1 << i)) != 0)
                        result ^= new Bitboard(sq);
                }
                return result;
            }

            // Rook or Bishop の利きの範囲を調べて bitboard で返す。
            // occupied  障害物があるマスが 1 の bitboard
            // n = 0 右上から左下 , n = 1 左上から右下
            Bitboard effectCalc(Square square, Bitboard occupied, int n)
            {
                Bitboard result = ZERO_BB;

                // 角の利きのrayと飛車の利きのray

                SquareWithWall[] deltaArray;
                if (n == 0)
                    deltaArray = new SquareWithWall[2]
                    { SquareWithWall.SQWW_RU, SquareWithWall.SQWW_LD };
                else
                    deltaArray = new SquareWithWall[2]
                    { SquareWithWall.SQWW_RD, SquareWithWall.SQWW_LU };

                foreach (var delta in deltaArray)
                {
                    // 壁に当たるまでsqを利き方向に伸ばしていく
                    for (var sq = (SquareWithWall)(square.ToSqww().ToInt() + delta.ToInt()); sq.IsOk(); sq += delta.ToInt())
                    {
                        result ^= sq.ToSquare(); // まだ障害物に当っていないのでここまでは利きが到達している

                        if ((occupied & sq.ToSquare()).IsNotZero()) // sqの地点に障害物があればこのrayは終了。
                            break;
                    }
                }
                return result;
            }

            // pieceをsqにおいたときに利きを得るのに関係する升を返す
            Bitboard calcBishopEffectMask(Square sq, int n)
            {
                Bitboard result;
                result = ZERO_BB;

                // 外周は角の利きには関係ないのでそこは除外する。
                for (Rank r = Rank.RANK_2; r <= Rank.RANK_8; ++r)
                    for (File f = File.FILE_2; f <= File.FILE_8; ++f)
                    {
                        var dr = sq.ToRank() - r;
                        var df = sq.ToFile() - f;
                        // dr == dfとdr != dfとをnが0,1とで切り替える。
                        if (System.Math.Abs(dr) == System.Math.Abs(df)
                            && ((((int)dr == (int)df) ? 1 : 0) ^ n) != 0)
                            result ^= Util.MakeSquare(f, r);
                    }

                // sqの地点は関係ないのでクリアしておく。
                result &= ~new Bitboard(sq);

                return result;
            }

            // 角の利きテーブルの初期化
            for (int n = 0; n < 2; ++n)
            {
                int index = 0;
                for (var sq = Square.ZERO; sq < Square.NB; ++sq)
                {
                    // sqの升に対してテーブルのどこを見るかのindex
                    BishopEffectIndex[n, sq.ToInt()] = index;

                    // sqの地点にpieceがあるときにその利きを得るのに関係する升を取得する
                    var mask = calcBishopEffectMask(sq, n);
                    BishopEffectMask[n, sq.ToInt()] = mask;

                    // p[0]とp[1]が被覆していると正しく計算できないのでNG。
                    // Bitboardのレイアウト的に、正しく計算できるかのテスト。
                    // 縦型Bitboardであるならp[0]のbit63を余らせるようにしておく必要がある。
                    //ASSERT_LV3(!(mask.cross_over()));

                    // sqの升用に何bit情報を拾ってくるのか
                    int bits = mask.PopCount();

                    // 参照するoccupied bitboardのbit数と、そのbitの取りうる状態分だけ..
                    int num = 1 << bits;

                    for (int i = 0; i < num; ++i)
                    {
                        Bitboard occupied = indexToOccupied(i, bits, mask);
                        // 初期化するテーブル
                        BishopEffectBB[n, index + (int)OccupiedToIndex(occupied & mask, mask)] = effectCalc(sq, occupied, n);
                    }
                    index += num;
                }

                // 盤外(SQ_NB)に駒を配置したときに利きがZERO_BBとなるときのための処理
                BishopEffectIndex[n, (int)Square.NB] = index;

                // 何番まで使ったか出力してみる。(確保する配列をこのサイズに収めたいので)
                // cout << index << endl;
            }

            // 5. 飛車の縦方向の利きテーブルの初期化
            // ここでは飛車の利きを使わずに初期化しないといけない。

            for (Rank rank = Rank.RANK_1; rank <= Rank.RANK_9; ++rank)
            {
                // sq = SQ_11 , SQ_12 , ... , SQ_19
                Square sq = Util.MakeSquare(File.FILE_1, rank);

                const int num1s = 7;
                for (int i = 0; i < (1 << num1s); ++i)
                {
                    // iはsqに駒をおいたときに、その筋の2段～8段目の升がemptyかどうかを表現する値なので
                    // 1ビットシフトして、1～9段目の升を表現するようにする。
                    int ii = i << 1;
                    Bitboard bb = ZERO_BB;
                    for (int r = sq.ToRank().ToInt() - 1; r >= (int)Rank.RANK_1; --r)
                    {
                        bb |= Util.MakeSquare(sq.ToFile(), (Rank)r);
                        if ((ii & (1 << r)) != 0)
                            break;
                    }
                    for (int r = sq.ToRank().ToInt() + 1; r <= (int)Rank.RANK_9; ++r)
                    {
                        bb |= Util.MakeSquare(sq.ToFile(), (Rank)r);
                        if ((ii & (1 << r)) != 0)
                            break;
                    }
                    RookFileEffectBB[(int)rank, i] = bb.p.p0;
                    // RookEffectFile[RANK_NB][x] には値を代入していないがC++の規約によりゼロ初期化されている。
                }
            }

            // 飛車の横の利き
            for (File file = File.FILE_1; file <= File.FILE_9; ++file)
            {
                // sq = SQ_11 , SQ_21 , ... , SQ_NBまで
                Square sq = Util.MakeSquare(file, Rank.RANK_1);

                const int num1s = 7;
                for (int i = 0; i < (1 << num1s); ++i)
                {
                    int ii = i << 1;
                    Bitboard bb = ZERO_BB;
                    for (int f = (int)sq.ToFile() - 1; f >= (int)File.FILE_1; --f)
                    {
                        bb |= Util.MakeSquare((File)f, sq.ToRank());
                        if ((ii & (1 << f)) != 0)
                            break;
                    }
                    for (int f = (int)sq.ToFile() + 1; f <= (int)File.FILE_9; ++f)
                    {
                        bb |= Util.MakeSquare((File)f, sq.ToRank());
                        if ((ii & (1 << f)) != 0)
                            break;
                    }

                    RookRankEffectBB[(int)file, i] = bb;
                    // RookRankEffect[FILE_NB][x] には値を代入していないがC++の規約によりゼロ初期化されている。
                }
            }


            // 6. 近接駒(+盤上の利きを考慮しない駒)のテーブルの初期化。
            // 上で初期化した、香・馬・飛の利きを用いる。

            foreach (var sq in All.Squares())
            {
                // 玉は長さ1の角と飛車の利きを合成する
                KingEffectBB[(int)sq] = BishopEffect(sq, ALL_BB) | RookEffect(sq, ALL_BB);
            }

            foreach (var c in All.Colors())
                foreach (var sq in All.Squares())
                    // 障害物がないときの香の利き
                    // これを最初に初期化しないとlanceEffect()が使えない。
                    LanceStepEffectBB[(int)sq, (int)c] = RookFileEffect(sq, ZERO_BB) & ForwardRanks(c, sq.ToRank());

            foreach (var c in All.Colors())
                foreach (var sq in All.Squares())
                {
                    // 歩は長さ1の香の利きとして定義できる
                    PawnEffectBB[(int)sq,(int)c] = LanceEffect(c, sq, ALL_BB);

                    // 桂の利きは、歩の利きの地点に長さ1の角の利きを作って、前方のみ残す。
                    Bitboard tmp = ZERO_BB;
                    Bitboard pawn = LanceEffect(c, sq, ALL_BB);
                    if (pawn.IsNotZero())
                    {
                        Square sq2 = pawn.Pop();
                        Bitboard pawn2 = LanceEffect(c, sq2, ALL_BB); // さらに1つ前
                        if (pawn2.IsNotZero())
                            tmp = BishopEffect(sq2, ALL_BB) & RANK_BB[(int)pawn2.Pop().ToRank()];
                    }
                    KnightEffectBB[(int)sq,(int)c] = tmp;

                    // 銀は長さ1の角の利きと長さ1の香の利きの合成として定義できる。
                    SilverEffectBB[(int)sq,(int)c] = LanceEffect(c, sq, ALL_BB) | BishopEffect(sq, ALL_BB);

                    // 金は長さ1の角と飛車の利き。ただし、角のほうは相手側の歩の行き先の段でmaskしてしまう。
                    Bitboard e_pawn = LanceEffect(c.Not() , sq, ALL_BB);
                    Bitboard mask = ZERO_BB;
                    if (e_pawn.IsNotZero())
                        mask = RANK_BB[(int)e_pawn.Pop().ToRank()];
                    GoldEffectBB[(int)sq,(int)c] = (BishopEffect(sq, ALL_BB) & ~mask) | RookEffect(sq, ALL_BB);

                    // 障害物がないときの角と飛車の利き
                    BishopStepEffectBB[(int)sq] = BishopEffect(sq, ZERO_BB);
                    RookStepEffectBB[(int)sq] = RookEffect(sq, ZERO_BB);
                }


#if false
	// 7) 二歩用のテーブル初期化

	for (int i = 0; i < 0x80; ++i)
	{
		Bitboard b = ZERO_BB;
		for (int k = 0; k < 7; ++k)
			if ((i & (1 << k)) == 0)
				b |= FILE_BB[k];

		PAWN_DROP_MASK_BB[i].p[0] = b.p[0]; // 1～7筋
	}
	for (int i = 0; i < 0x4; ++i)
	{
		Bitboard b = ZERO_BB;
		for (int k = 0; k < 2; ++k)
			if ((i & (1 << k)) == 0)
				b |= FILE_BB[k+7];

		PAWN_DROP_MASK_BB[i].p[1] = b.p[1]; // 8,9筋
	}
#endif

            // 8) BetweenBB , LineBBの初期化
            {
                UInt16 between_index = 1;
                // BetweenBB[0] == ZERO_BBであることを保証する。

                foreach (var s1 in All.Squares())
                    foreach (var s2 in All.Squares())
                    {
                        // 十字方向か、斜め方向かだけを判定して、例えば十字方向なら
                        // rookEffect(sq1,Bitboard(s2)) & rookEffect(sq2,Bitboard(s1))
                        // のように初期化したほうが明快なコードだが、この初期化をそこに依存したくないので愚直にやる。

                        // これについてはあとで設定する。
                        if (s1 >= s2)
                            continue;

                        // 方角を用いるテーブルの初期化
                        if (Util.DirectionsOf(s1, s2) != Directions.ZERO)
                        {
                            Bitboard bb = ZERO_BB;
                            // 間に挟まれた升を1に
                            int delta = (s2 - s1) / dist(s1, s2);
                            for (Square s = s1 + delta; s != s2; s += delta)
                                bb |= s;

                            // ZERO_BBなら、このindexとしては0を指しておけば良いので書き換える必要ない。
                            if (bb.IsZero())
                                continue;

                            BetweenIndex[(int)s1, (int)s2] = between_index;
                            BetweenBB_[between_index++] = bb;
                        }
                    }

                //		    ASSERT_LV1(between_index == 785);

                // 対称性を考慮して、さらにシュリンクする。
                foreach (var s1 in All.Squares())
                    foreach (var s2 in All.Squares())
                        if (s1 > s2)
                            BetweenIndex[(int)s1, (int)s2] = BetweenIndex[(int)s2, (int)s1];


                LineBB_ = new Bitboard[(int)Square.NB, 4];

                for (var s1 = Square.ZERO; s1 < Square.NB; ++s1)
                    for (int d = 0; d < 4; ++d)
                    {
                        // BishopEffect0 , RookRankEffect , BishopEffect1 , RookFileEffectを用いて初期化したほうが
                        // 明快なコードだが、この初期化をそこに依存したくないので愚直にやる。

                        Square[] deltas = new Square[] { Square.SQ_RU, Square.SQ_R, Square.SQ_RD, Square.SQ_U };
                        int delta = (int)deltas[d];
                        Bitboard bb = new Bitboard(s1);

                        // 壁に当たるまでs1から-delta方向に延長
                        for (Square s = s1; dist(s, s - delta) <= 1; s -= delta) bb |= (s - delta);

                        // 壁に当たるまでs1から+delta方向に延長
                        for (Square s = s1; dist(s, s + delta) <= 1; s += delta) bb |= (s + delta);

                        LineBB_[(int)s1, d] = bb;
                    }
            }

        }

        /// <summary>
        /// すべてのSquareが1であるBitboard
        /// </summary>
        private static Bitboard ZERO_BB;

        /// <summary>
        /// すべてのSquareが1であるBitboard
        /// </summary>
        private static Bitboard ALL_BB;

        /// <summary>
        /// 筋を表現するBitboard
        /// </summary>
        private static Bitboard[] FILE_BB;

        /// <summary>
        /// 段を表現するBitboard
        /// </summary>
        private static Bitboard[] RANK_BB;

        /// <summary>
        /// Bitboard(Square)で用いるテーブル
        /// 配列のサイズはSquare.NB_PLUS1
        /// </summary>
        private static Bitboard[] SQUARE_BB;


        // ForwardRanksBBの定義)
        //    c側の香の利き = 飛車の利き & ForwardRanksBB[c][rank_of(sq)]
        //
        // すなわち、
        // color == BLACKのとき、n段目よりWHITE側(1からn-1段目)を表現するBitboard。
        // color == WHITEのとき、n段目よりBLACK側(n+1から9段目)を表現するBitboard。
        // このアイデアはAperyのもの。
        private static Bitboard[,] ForwardRanksBB; //   = new Bitboard[(int)Color.NB, (int)Rank.NB]

        /// <summary>
        /// 玉、金、銀、桂、歩の利き
        /// </summary>
        private static Bitboard[] KingEffectBB = new Bitboard[(int)Square.NB_PLUS1];
        private static Bitboard[,] GoldEffectBB = new Bitboard[(int)Square.NB_PLUS1,(int)Color.NB];
        private static Bitboard[,] SilverEffectBB = new Bitboard[(int)Square.NB_PLUS1,(int)Color.NB];
        private static Bitboard[,] KnightEffectBB = new Bitboard[(int)Square.NB_PLUS1,(int)Color.NB];
        private static Bitboard[,] PawnEffectBB = new Bitboard[(int)Square.NB_PLUS1,(int)Color.NB];

        // 盤上の駒をないものとして扱う、遠方駒の利き。香、角、飛
        private static Bitboard[,] LanceStepEffectBB = new Bitboard[(int)Square.NB_PLUS1,(int)Color.NB];
        private static Bitboard[] BishopStepEffectBB = new Bitboard[(int)Square.NB_PLUS1];
        private static Bitboard[] RookStepEffectBB = new Bitboard[(int)Square.NB_PLUS1];

        // 角の利き
        private static Bitboard[,] BishopEffectBB = new Bitboard[2,1856+1];
        private static Bitboard[,] BishopEffectMask = new Bitboard[2,(int)Square.NB_PLUS1];
        private static int[,] BishopEffectIndex = new int[2,(int)Square.NB_PLUS1];

        // 飛車の縦、横の利き

        // 飛車の縦方向の利きを求めるときに、指定した升sqの属するfileのbitをshiftし、
        // index を求める為に使用する。(from Apery)
        private static Byte[] Slide = new Byte[(int)Square.NB_PLUS1]
        {
              1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 ,
              10, 10, 10, 10, 10, 10, 10, 10, 10,
              19, 19, 19, 19, 19, 19, 19, 19, 19,
              28, 28, 28, 28, 28, 28, 28, 28, 28,
              37, 37, 37, 37, 37, 37, 37, 37, 37,
              46, 46, 46, 46, 46, 46, 46, 46, 46,
              55, 55, 55, 55, 55, 55, 55, 55, 55,
              1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 , 1 ,
              10, 10, 10, 10, 10, 10, 10, 10, 10,
              0 , // SQ_NB用
        };

        private static UInt64[,] RookFileEffectBB = new UInt64[(int)Rank.NB + 1,128];
        private static Bitboard[,] RookRankEffectBB = new Bitboard[(int)File.NB + 1,128];

    }
}
