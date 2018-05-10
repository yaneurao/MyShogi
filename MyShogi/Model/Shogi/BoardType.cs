using System;

namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 平手、二枚落ちなど盤面タイプを表現する
    /// </summary>
    public enum BoardType : Int32
    {
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
        /// BoardType型をInt32に変換する
        /// </summary>
        /// <param name="boardType"></param>
        /// <returns></returns>
        public static Int32 ToInt(this BoardType boardType)
        {
            return (Int32)boardType;
        }
    }
}
