using System;
using System.Text;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// Bitboard
    /// 駒の利きなどを表現する
    /// やねうら王から移植
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
            return (p.p0 != 0) ? (Square)(BitOp.LSB64(ref p.p0)) : (Square)(BitOp.LSB64(ref p.p1) + 63);
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
        // 以下、利きを返すbitboardなど
        // -------------------------------------------------------------------------

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
            return SQUARE_BB[sq.ToInt()];
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

#if false
// 具体的なPiece名を指定することがほとんどなので1本の配列になっているメリットがあまりないので配列を分ける。

extern Bitboard GoldEffectBB[SQ_NB_PLUS1][COLOR_NB];
extern Bitboard SilverEffectBB[SQ_NB_PLUS1][COLOR_NB];
extern Bitboard KnightEffectBB[SQ_NB_PLUS1][COLOR_NB];
extern Bitboard PawnEffectBB[SQ_NB_PLUS1][COLOR_NB];

// 盤上の駒をないものとして扱う、遠方駒の利き。香、角、飛
extern Bitboard LanceStepEffectBB[SQ_NB_PLUS1][COLOR_NB];
extern Bitboard BishopStepEffectBB[SQ_NB_PLUS1];
extern Bitboard RookStepEffectBB[SQ_NB_PLUS1];

// --- 角の利き
extern Bitboard BishopEffect[2][1856+1];
extern Bitboard BishopEffectMask[2][SQ_NB_PLUS1];
extern int		BishopEffectIndex[2][SQ_NB_PLUS1];

// --- 飛車の縦、横の利き

// 飛車の縦方向の利きを求めるときに、指定した升sqの属するfileのbitをshiftし、
// index を求める為に使用する。(from Apery)
extern u8		Slide[SQ_NB_PLUS1];

extern u64      RookFileEffect[RANK_NB + 1][128];
extern Bitboard RookRankEffect[FILE_NB + 1][128];
#endif

        // -------------------------------------------------------------------------
        // 以下、private methods / tables
        // -------------------------------------------------------------------------

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

            SQUARE_BB = new Bitboard[(int)Square.NB_PLUS1];

            // SQUARE_BBは上記のRANK_BBとFILE_BBを用いて初期化すると楽。
            for(Square sq = Square.ZERO; sq < Square.NB; ++sq)
            {
                File f = sq.ToFile();
                Rank r = sq.ToRank();

                // 筋と段が交差するところがSQUARE_BB
                SQUARE_BB[sq.ToInt()] = FILE_BB[f.ToInt()] & RANK_BB[r.ToInt()];
            }

            // 1) SquareWithWallテーブルの初期化。

            for (Square sq = Square.ZERO; sq < Square.NB; ++sq)
                SquareWithWallExtensions.sqww_table[sq.ToInt()] = (SquareWithWall)
                    ((int)SquareWithWall.SQWW_11
                    + sq.ToFile().ToInt() * (int)SquareWithWall.SQWW_L 
                    + sq.ToRank().ToInt() * (int)SquareWithWall.SQWW_D);


            // 4) 遠方利きのテーブルの初期化
            //  thanks to Apery (Takuya Hiraoka)

            // 引数のindexをbits桁の2進数としてみなす。すなわちindex(0から2^bits-1)。
            // 与えられたmask(1の数がbitsだけある)に対して、1のbitのいくつかを(indexの値に従って)0にする。
            Func<int,int,Bitboard,Bitboard> indexToOccupied = (int index, int bits, Bitboard mask) =>
        	{
                var result = ZERO_BB;
                for (int i = 0; i < bits; ++i)
                {
                    Square sq = mask.Pop();
                    if ((index & (1 << i))!=0)
                        result ^= new Bitboard(sq);
                }
                return result;
            };

            // Rook or Bishop の利きの範囲を調べて bitboard で返す。
            // occupied  障害物があるマスが 1 の bitboard
            // n = 0 右上から左下 , n = 1 左上から右下
            Func<Square,Bitboard,int,Bitboard> effectCalc = (Square square, Bitboard occupied, int n) =>
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
	        };

            // pieceをsqにおいたときに利きを得るのに関係する升を返す
            Func<Square,int,Bitboard> calcBishopEffectMask = (Square sq, int n) =>
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
                        if (Math.Abs(dr) == Math.Abs(df)
                            && ((((int)dr == (int)df) ? 1:0) ^ n) != 0)
                            result ^= Util.MakeSquare(f , r);
                    }

                // sqの地点は関係ないのでクリアしておく。
                result &= ~ new Bitboard(sq);

                return result;
            };


#if false
            // 角の利きテーブルの初期化
            for (int n = 0; n < 2; ++ n)
        	{
                int index = 0;
                for (var sq = Square.ZERO; sq < Square.NB; ++sq)
                {
                    // sqの升に対してテーブルのどこを見るかのindex
                    BishopEffectIndex[n][sq] = index;

                    // sqの地点にpieceがあるときにその利きを得るのに関係する升を取得する
                    auto & mask = BishopEffectMask[n][sq];
                    mask = calcBishopEffectMask(sq, n);

                    // p[0]とp[1]が被覆していると正しく計算できないのでNG。
                    // Bitboardのレイアウト的に、正しく計算できるかのテスト。
                    // 縦型Bitboardであるならp[0]のbit63を余らせるようにしておく必要がある。
                    ASSERT_LV3(!(mask.cross_over()));

                    // sqの升用に何bit情報を拾ってくるのか
                    const int bits = mask.pop_count();

                    // 参照するoccupied bitboardのbit数と、そのbitの取りうる状態分だけ..
                    const int num = 1 << bits;

                    for (int i = 0; i < num; ++i)
                    {
                        Bitboard occupied = indexToOccupied(i, bits, mask);
                        // 初期化するテーブル
                        BishopEffect[n][index + occupiedToIndex(occupied & mask, mask)] = effectCalc(sq, occupied, n);
                    }
                    index += num;
        }

        // 盤外(SQ_NB)に駒を配置したときに利きがZERO_BBとなるときのための処理
        BishopEffectIndex[n][Square.NB] = index;

                // 何番まで使ったか出力してみる。(確保する配列をこのサイズに収めたいので)
                // cout << index << endl;
            }
#endif

#if false

  

  
	// 5. 飛車の縦方向の利きテーブルの初期化
	// ここでは飛車の利きを使わずに初期化しないといけない。

	for (Rank rank = RANK_1; rank <= RANK_9 ; ++rank)
	{
		// sq = SQ_11 , SQ_12 , ... , SQ_19
		Square sq = FILE_1 | rank;

		const int num1s = 7;
		for (int i = 0; i < (1 << num1s); ++i)
		{
			// iはsqに駒をおいたときに、その筋の2段～8段目の升がemptyかどうかを表現する値なので
			// 1ビットシフトして、1～9段目の升を表現するようにする。
			int ii = i << 1;
			Bitboard bb = ZERO_BB;
			for (int r = rank_of(sq) - 1; r >= RANK_1; --r)
			{
				bb |= file_of(sq) | (Rank)r;
				if (ii & (1 << r))
					break;
			}
			for (int r = rank_of(sq) + 1; r <= RANK_9; ++r)
			{
				bb |= file_of(sq) | (Rank)r;
				if (ii & (1 << r))
					break;
			}
			RookFileEffect[rank][i] = bb.p[0];
			// RookEffectFile[RANK_NB][x] には値を代入していないがC++の規約によりゼロ初期化されている。
		}
	}

	// 飛車の横の利き
	for (File file = FILE_1 ; file <= FILE_9 ; ++file )
	{
		// sq = SQ_11 , SQ_21 , ... , SQ_NBまで
		Square sq = file | RANK_1;
		
		const int num1s = 7;
		for (int i = 0; i < (1 << num1s); ++i)
		{
			int ii = i << 1;
			Bitboard bb = ZERO_BB;
			for (int f = file_of(sq) - 1; f >= FILE_1; --f)
			{
				bb |= (File)f | rank_of(sq);
				if (ii & (1 << f))
					break;
			}
			for (int f = file_of(sq) + 1; f <= FILE_9; ++f)
			{
				bb |= (File)f | rank_of(sq);
				if (ii & (1 << f))
					break;
			}
			RookRankEffect[file][i] = bb;
			// RookRankEffect[FILE_NB][x] には値を代入していないがC++の規約によりゼロ初期化されている。
		}
	}

	// 6. 近接駒(+盤上の利きを考慮しない駒)のテーブルの初期化。
	// 上で初期化した、香・馬・飛の利きを用いる。

	for (auto sq : SQ)
	{
		// 玉は長さ1の角と飛車の利きを合成する
		KingEffectBB[sq] = bishopEffect(sq, ALL_BB) | rookEffect(sq, ALL_BB);
	}

	for (auto c : COLOR)
		for(auto sq : SQ)
			// 障害物がないときの香の利き
			// これを最初に初期化しないとlanceEffect()が使えない。
			LanceStepEffectBB[sq][c] = rookFileEffect(sq,ZERO_BB) & ForwardRanksBB[c][rank_of(sq)];

	for (auto c : COLOR)
		for (auto sq : SQ)
		{
			// 歩は長さ1の香の利きとして定義できる
			PawnEffectBB[sq][c] = lanceEffect(c, sq, ALL_BB);

			// 桂の利きは、歩の利きの地点に長さ1の角の利きを作って、前方のみ残す。
			Bitboard tmp = ZERO_BB;
			Bitboard pawn = lanceEffect(c, sq, ALL_BB);
			if (pawn)
			{
				Square sq2 = pawn.pop();
				Bitboard pawn2 = lanceEffect(c, sq2, ALL_BB); // さらに1つ前
				if (pawn2)
					tmp = bishopEffect(sq2, ALL_BB) & RANK_BB[rank_of(pawn2.pop())];
			}
			KnightEffectBB[sq][c] = tmp;

			// 銀は長さ1の角の利きと長さ1の香の利きの合成として定義できる。
			SilverEffectBB[sq][c] = lanceEffect(c, sq, ALL_BB) | bishopEffect(sq, ALL_BB);

			// 金は長さ1の角と飛車の利き。ただし、角のほうは相手側の歩の行き先の段でmaskしてしまう。
			Bitboard e_pawn = lanceEffect(~c, sq, ALL_BB);
			Bitboard mask = ZERO_BB;
			if (e_pawn)
				mask = RANK_BB[rank_of(e_pawn.pop())];
			GoldEffectBB[sq][c]= (bishopEffect(sq, ALL_BB) & ~mask) | rookEffect(sq, ALL_BB);

			// 障害物がないときの角と飛車の利き
			BishopStepEffectBB[sq] = bishopEffect(sq, ZERO_BB);
			RookStepEffectBB[sq]   = rookEffect(sq, ZERO_BB);

			// --- 以下のbitboard、あまり頻繁に呼び出さないので他のbitboardを合成して代用する。

			// 盤上の駒がないときのqueenの利き
			// StepEffectsBB[sq][c][PIECE_TYPE_BITBOARD_QUEEN] = bishopEffect(sq, ZERO_BB) | rookEffect(sq, ZERO_BB);

			// 長さ1の十字
			// StepEffectsBB[sq][c][PIECE_TYPE_BITBOARD_CROSS00] = rookEffect(sq, ALL_BB);

			// 長さ1の斜め
			// StepEffectsBB[sq][c][PIECE_TYPE_BITBOARD_CROSS45] = bishopEffect(sq, ALL_BB);
		}

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

	// 8) BetweenBB , LineBBの初期化
	{
		u16 between_index = 1;
		// BetweenBB[0] == ZERO_BBであることを保証する。

		for (auto s1 : SQ)
			for (auto s2 : SQ)
			{
				// 十字方向か、斜め方向かだけを判定して、例えば十字方向なら
				// rookEffect(sq1,Bitboard(s2)) & rookEffect(sq2,Bitboard(s1))
				// のように初期化したほうが明快なコードだが、この初期化をそこに依存したくないので愚直にやる。
					
				// これについてはあとで設定する。
				if (s1 >= s2)
					continue;

				// 方角を用いるテーブルの初期化
				if (Effect8::directions_of(s1, s2))
				{
					Bitboard bb = ZERO_BB;
					// 間に挟まれた升を1に
					Square delta = (s2 - s1) / dist(s1, s2);
					for (Square s = s1 + delta; s != s2; s += delta)
						bb |= s;

					// ZERO_BBなら、このindexとしては0を指しておけば良いので書き換える必要ない。
					if (!bb)
						continue;

					BetweenIndex[s1][s2] = between_index;
					BetweenBB[between_index++] = bb;
				}
			}

		ASSERT_LV1(between_index == 785);

		// 対称性を考慮して、さらにシュリンクする。
		for (auto s1 : SQ)
			for (auto s2 : SQ)
				if (s1 > s2)
					BetweenIndex[s1][s2] = BetweenIndex[s2][s1];

	}
	for (auto s1 : SQ)
		for (int d = 0; d < 4; ++d)
		{
			// BishopEffect0 , RookRankEffect , BishopEffect1 , RookFileEffectを用いて初期化したほうが
			// 明快なコードだが、この初期化をそこに依存したくないので愚直にやる。

			const Square deltas[4] = { SQ_RU , SQ_R , SQ_RD , SQ_U };
			const Square delta = deltas[d];
			Bitboard bb = Bitboard(s1);

			// 壁に当たるまでs1から-delta方向に延長
			for (Square s = s1; dist(s, s - delta) <= 1; s -= delta) bb |= (s - delta);

			// 壁に当たるまでs1から+delta方向に延長
			for (Square s = s1; dist(s, s + delta) <= 1; s += delta) bb |= (s + delta);

			LineBB[s1][d] = bb;
		}


	// 9) 王手となる候補の駒のテーブル初期化(王手の指し手生成に必要。やねうら王nanoでは削除予定)

define FOREACH_KING(BB, EFFECT ) { for(auto sq : BB){ target|= EFFECT(sq); } }
define FOREACH(BB, EFFECT ) { for(auto sq : BB){ target|= EFFECT(them,sq); } }
define FOREACH_BR(BB, EFFECT ) { for(auto sq : BB) { target|= EFFECT(sq,ZERO_BB); } }

	for (auto Us : COLOR)
		for (auto ksq : SQ)
		{
			Color them = ~Us;
			auto enemyGold = goldEffect(them, ksq) & enemy_field(Us);
			Bitboard target;

			// 歩で王手になる可能性のあるものは、敵玉から２つ離れた歩(不成での移動) + ksqに敵の金をおいた範囲(enemyGold)に成りで移動できる
			target = ZERO_BB;
			FOREACH(pawnEffect(them, ksq), pawnEffect);
			FOREACH(enemyGold, pawnEffect);
			CheckCandidateBB[ksq][PAWN - 1][Us] = target & ~Bitboard(ksq);

			// 香で王手になる可能性のあるものは、ksqに敵の香をおいたときの利き。(盤上には何もないものとする)
			// と、王が1から3段目だと成れるので王の両端に香を置いた利きも。
			target = lanceStepEffect(them, ksq);
			if (enemy_field(Us) & ksq)
			{
				if (file_of(ksq) != FILE_1)
					target |= lanceStepEffect(them, ksq + SQ_R);
				if (file_of(ksq) != FILE_9)
					target |= lanceStepEffect(them, ksq + SQ_L);
			}
			CheckCandidateBB[ksq][LANCE - 1][Us] = target;

			// 桂で王手になる可能性のあるものは、ksqに敵の桂をおいたところに移動できる桂(不成) + ksqに金をおいた範囲(enemyGold)に成りで移動できる桂
			target = ZERO_BB;
			FOREACH(knightEffect(them, ksq) | enemyGold, knightEffect);
			CheckCandidateBB[ksq][KNIGHT - 1][Us] = target & ~Bitboard(ksq);

			// 銀も同様だが、2,3段目からの引き成りで王手になるパターンがある。(4段玉と5段玉に対して)
			target = ZERO_BB;
			FOREACH(silverEffect(them, ksq), silverEffect);
			FOREACH(enemyGold, silverEffect); // 移動先が敵陣 == 成れる == 金になるので、敵玉の升に敵の金をおいた利きに成りで移動すると王手になる。
			FOREACH(goldEffect(them, ksq), enemy_field(Us) & silverEffect); // 移動元が敵陣 == 成れる == 金になるので、敵玉の升に敵の金をおいた利きに成りで移動すると王手になる。
			CheckCandidateBB[ksq][SILVER - 1][Us] = target & ~Bitboard(ksq);

			// 金
			target = ZERO_BB;
			FOREACH(goldEffect(them, ksq), goldEffect);
			CheckCandidateBB[ksq][GOLD - 1][Us] = target & ~Bitboard(ksq);

			// 角
			target = ZERO_BB;
			FOREACH_BR(bishopEffect(ksq, ZERO_BB), bishopEffect);
			FOREACH_BR(kingEffect(ksq) & enemy_field(Us), bishopEffect); // 移動先が敵陣 == 成れる == 王の動き
			FOREACH_BR(kingEffect(ksq), enemy_field(Us) & bishopEffect); // 移動元が敵陣 == 成れる == 王の動き
			CheckCandidateBB[ksq][BISHOP - 1][Us] = target & ~Bitboard(ksq);

			// 飛・龍は無条件全域。
			// ROOKのところには馬のときのことを格納

			// 馬
			target = ZERO_BB;
			FOREACH_BR(horseEffect(ksq, ZERO_BB), horseEffect);
			CheckCandidateBB[ksq][ROOK - 1][Us] = target & ~Bitboard(ksq);

			// 王(24近傍が格納される)
			target = ZERO_BB;
			FOREACH_KING(kingEffect(ksq), kingEffect);
			CheckCandidateKingBB[ksq] = target & ~Bitboard(ksq);
		}

#endif


    // 王の利きの初期化
    KingEffectBB = new Bitboard[(int)Square.NB_PLUS1];
            // このコード、あとで書く


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
        private static Bitboard [] FILE_BB;

        /// <summary>
        /// 段を表現するBitboard
        /// </summary>
        private static Bitboard [] RANK_BB;

        /// <summary>
        /// Bitboard(Square)で用いるテーブル
        /// 配列のサイズはSquare.NB_PLUS1
        /// </summary>
        private static Bitboard[] SQUARE_BB;

        /// <summary>
        /// 王の利きを表現するBitboard
        /// 配列のサイズはSquare.NB_PLUS1
        /// </summary>
        private static Bitboard[] KingEffectBB;
    }
}
