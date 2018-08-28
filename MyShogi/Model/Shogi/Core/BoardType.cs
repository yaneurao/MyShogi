using System;

namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 平手、二枚落ちなど盤面タイプを表現する
    /// </summary>
    public enum BoardType : Int32
    {
        /// <summary>
        /// 平手
        /// </summary>
        // [LabelDescription(Label = "平手")]
        NoHandicap,
        
        /// <summary>
        /// 香落ち
        /// </summary>
        //[LabelDescription(Label = "香落ち")]
        HandicapKyo,

        /// <summary>
        /// 右香落ち
        /// </summary>
        //[LabelDescription(Label = "右香落ち")]
        HandicapRightKyo,
        
        /// <summary>
        /// 角落ち
        /// </summary>
        //[LabelDescription(Label = "角落ち")]
        HandicapKaku,
        
        /// <summary>
        /// 飛車落ち
        /// </summary>
        //[LabelDescription(Label = "飛車落ち")]
        HandicapHisya,

        /// <summary>
        /// 飛香落ち
        /// </summary>
        //[LabelDescription(Label = "飛香落ち")]
        HandicapHisyaKyo,

        /// <summary>
        /// 二枚落ち
        /// </summary>
        //[LabelDescription(Label = "二枚落ち")]
        Handicap2,

        /// <summary>
        /// 三枚落ち
        /// </summary>
        //[LabelDescription(Label = "三枚落ち")]
        Handicap3,

        /// <summary>
        /// 四枚落ち
        /// </summary>
        //[LabelDescription(Label = "四枚落ち")]
        Handicap4,
        
        /// <summary>
        /// 五枚落ち
        /// </summary>
        //[LabelDescription(Label = "五枚落ち")]
        Handicap5,
        
        /// <summary>
        /// 左五枚落ち
        /// </summary>
        //[LabelDescription(Label = "左五枚落ち")]
        HandicapLeft5,

        /// <summary>
        /// 六枚落ち
        /// </summary>
        //[LabelDescription(Label = "六枚落ち")]
        Handicap6,

        /// <summary>
        /// 八枚落ち
        /// </summary>
        //[LabelDescription(Label = "八枚落ち")]
        Handicap8,

        /// <summary>
        /// 十枚落ち
        /// </summary>
        //[LabelDescription(Label = "十枚落ち")]
        Handicap10,

        /// <summary>
        /// 歩三枚
        /// </summary>
        //[LabelDescription(Label = "歩三枚")]
        HandicapPawn3,

        /// <summary>
        /// 詰将棋用の局面
        /// (玉は後手玉が51にいて、あとの手駒はすべて後手側に)
        /// </summary>
        Mate1,

        /// <summary>
        /// 双玉詰将棋用の局面
        /// (玉が51,59にいて、あとの手駒はすべて後手側に)
        /// </summary>
        Mate2,

        /// <summary>
        /// 双玉で玉以外すべて駒箱に
        /// (玉が51,59にいて、あとの手駒はすべて駒箱に)
        /// </summary>
        Mate3,

        /// <summary>
        /// それ以外の局面図
        /// </summary>
        //[LabelDescription(Label = "任意局面")]
        Others,

        /// <summary>
        /// 現在の(画面上の)局面図
        /// </summary>
        //[LabelDescription(Label = "現在の局面")]
        Current,

        // 終わり
        NB,
        ZERO = 0,
    }


    /// <summary>
    /// BoardTypeに対するextension methods
    /// </summary>
    public static class BoardTypeExtension
    {
        /// <summary>
        /// BoardType型が正当な値の範囲であるかをテストする
        /// </summary>
        /// <param name="boardType"></param>
        /// <returns></returns>
        public static bool IsOk(this BoardType boardType)
        {
            return BoardType.ZERO <= boardType && boardType < BoardType.NB;
        }

        /// <summary>
        /// BoardType型がBoardType.ToSfen()でsfen化できる範囲にあるかをテストする。
        /// </summary>
        /// <param name="boardType"></param>
        /// <returns></returns>
        public static bool IsSfenOk(this BoardType boardType)
        {
            return BoardType.ZERO <= boardType && boardType < BoardType.Others;
        }

        /// <summary>
        /// BoardTypeに対応するsfen文字列を得る。
        /// BoardType.OthersとBoardType.Currentに対してはnullが返る。
        /// </summary>
        /// <param name="boardType"></param>
        /// <returns></returns>
        public static string ToSfen(this BoardType boardType)
        {
            // 範囲外
            if (!boardType.IsSfenOk())
                return null;

            return SFENS_OF_BOARDTYPE[(int)boardType];
        }

        /// <summary>
        /// BoardType型をInt32に変換する
        /// </summary>
        /// <param name="boardType"></param>
        /// <returns></returns>
        public static Int32 ToInt(this BoardType boardType)
        {
            return (Int32)boardType;
        }

        /// <summary>
        /// 駒落ちであるかを判定して返す。
        /// </summary>
        /// <param name="boardType"></param>
        /// <returns></returns>
        public static bool IsHandicapped(this BoardType boardType)
        {
            return !(boardType == BoardType.NoHandicap || boardType == BoardType.Current);
        }

        /// <summary>
        /// 平手、駒落ちなどのsfen文字列をひとまとめにした配列。BoardTypeのenumと対応する。
        /// </summary>
        public static readonly string[] SFENS_OF_BOARDTYPE =
        {
            Sfens.HIRATE , Sfens.HANDICAP_KYO , Sfens.HANDICAP_RIGHT_KYO , Sfens.HANDICAP_KAKU ,
            Sfens.HANDICAP_HISYA , Sfens.HANDICAP_HISYA_KYO ,
            Sfens.HANDICAP_2 , Sfens.HANDICAP_3 , Sfens.HANDICAP_4 , Sfens.HANDICAP_5 , Sfens.HANDICAP_LEFT_5 ,
            Sfens.HANDICAP_6 , Sfens.HANDICAP_8 , Sfens.HANDICAP_10 , Sfens.HANDICAP_PAWN3 , Sfens.MATE_1 , Sfens.MATE_2, Sfens.MATE_3,
        };
    }
}
