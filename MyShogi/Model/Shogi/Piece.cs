namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 駒を表す定数
    /// </summary>
    /// <remarks>
    /// PAWN   : 歩
    /// LANCE  : 香
    /// KNIGHT : 桂
    /// SILVER : 銀
    /// BISHOP : 角
    /// ROOK   : 飛
    /// GOLD   : 金
    /// KING   : 王
    /// PRO_PAWN   : 成歩(と)
    /// PRO_LANCE  : 成香
    /// PRO_KNIGHT : 成桂
    /// PRO_SILVER : 成銀
    /// HORSE      : 馬
    /// DRAGON     : 龍
    /// QUEEN      : 成金(この駒は無いのでQUEENを当ててある)
    /// </remarks>
    public enum Piece : int
    {
        NO_PIECE, PAWN, LANCE, KNIGHT, SILVER, BISHOP, ROOK, GOLD,
        KING, PRO_PAWN, PRO_LANCE, PRO_KNIGHT, PRO_SILVER, HORSE, DRAGON, QUEEN,

        PROMOTE = 8, // 成りを表す
        WHITE = 16,  // 後手を表す

        ZERO = 0,    // Pieceの開始番号
        NB = 32,     // Pieceの終端を表す 
    };

    /// <summary>
    /// Pieceに関するextension methodsを書いておくクラス
    /// </summary>
    public static class PieceExtensions
    {
        /// <summary>
        /// 値が正常な範囲であるかを判定する。
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static bool IsOk(this Piece piece)
        {
            return Piece.ZERO <= piece && piece < Piece.NB;
        }

        /// <summary>
        /// 日本語の文字列にする。
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static string Pretty(this Piece piece)
        {
            // "□"(四角)は文字フォントによっては半分の幅しかない。"口"(くち)にする。
            string[] PIECE_KANJI = {
                " 口"," 歩"," 香"," 桂"," 銀"," 角"," 飛"," 金"," 玉"," と"," 杏"," 圭"," 全"," 馬"," 龍"," 菌"," 王",
          "^歩","^香","^桂","^銀","^角","^飛","^金","^玉","^と","^杏","^圭","^全","^馬","^龍","^菌","^王" };
            return PIECE_KANJI[piece.ToInt()];
        }

        /// <summary>
        /// pが先手の駒であるか、後手の駒であるかを返す。
        /// p==EMPTYの場合、先手の駒扱いをする。
        /// </summary>
        public static Color PieceColor(this Piece piece)
        {
            return (piece < Piece.WHITE) ? Color.BLACK : Color.WHITE;
        }

        /// <summary>
        /// 後手の歩→先手の歩のように、後手という属性を取り払った駒種を返す
        /// </summary>
        public static Piece PieceType(this Piece piece)
        {
            return ((Piece)((int)piece & ~(int)Piece.WHITE));
        }

        /// <summary>
        /// 成ってない駒を返す。後手という属性も消去する。
        /// 例) 成銀→銀 , 後手の馬→先手の角
        /// ただし、pc == KINGでの呼び出しはNO_PIECEが返るものとする。
        /// </summary>
        public static Piece RawPieceType(this Piece piece)
        {
            return (Piece)((int)piece & 7);
        }

        /// <summary>
        /// 成り駒であるかどうかを判定する
        /// </summary>
        public static bool IsPromote(this Piece piece)
        {
            return ((int)piece & (int)Piece.PROMOTE) != 0;
        }

        /// <summary>
        /// pieceをintの値で取り出したいときに用いる。
        /// </summary>
        /// <returns></returns>
        public static int ToInt(this Piece piece)
        {
            return (int)piece;
        }

        /// <summary>
        /// 手番を相手の手番に変更する。
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color Flip(this Color color)
        {
            return (Color)((int)color ^ 1);
        }

    }

}
