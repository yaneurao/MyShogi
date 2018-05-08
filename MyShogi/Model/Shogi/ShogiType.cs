using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi
{
    // 将棋の盤面を表現するための基本的な型

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
    public enum PieceType : int
    {
        NO_PIECE, PAWN, LANCE, KNIGHT, SILVER, BISHOP, ROOK, GOLD,
        KING, PRO_PAWN, PRO_LANCE, PRO_KNIGHT, PRO_SILVER, HORSE, DRAGON, QUEEN,
        PIECE_PROMOTE = 8, // 成りを表す
        PIECE_WHITE = 16,  // 後手を表す
        PIECE_NB = 32,     // Pieceの終端を表す 
    };

    /// <summary>
    /// 先手・後手という手番を表す定数
    /// </summary>
    public enum Color : int
    {
        BLACK = 0,
        WHITE = 1,
    }

    /// <summary>
    /// 駒を表現する型
    /// </summary>
    public struct Piece
    {
        Piece(PieceType pt)
        {
            this.piece = pt;
        }

        /// <summary>
        /// pが先手の駒であるか、後手の駒であるかを返す。
        /// p==EMPTYの場合、先手の駒扱いをする。
        /// </summary>
        public Color Color { get { return (piece < PieceType.PIECE_WHITE) ? Color.BLACK : Color.WHITE; } }

        /// <summary>
        /// 後手の歩→先手の歩のように、後手という属性を取り払った駒種を返す
        /// </summary>
        public Piece Type { get { return new Piece((PieceType)((int)piece & ~(int)PieceType.PIECE_WHITE)); } }

        /// <summary>
        /// 成ってない駒を返す。後手という属性も消去する。
        /// 例) 成銀→銀 , 後手の馬→先手の角
        /// ただし、pc == KINGでの呼び出しはNO_PIECEが返るものとする。
        /// </summary>
        public Piece RawType { get { return new Piece((PieceType)((int)piece & 7)); } }

        /// <summary>
        /// 成り駒であるかどうかを判定する
        /// </summary>
        public bool Promote {  get { return ((int)piece & (int)PieceType.PIECE_PROMOTE) != 0; } }

        /// <summary>
        /// pieceをintの値で取り出したいときに用いる。
        /// </summary>
        /// <returns></returns>
        public int ToInt() { return (int)piece; }

        /// <summary>
        /// PieceTypeが取り出したいときはこれを用いる。
        /// </summary>
        public PieceType piece { get; set; }
    }

}
