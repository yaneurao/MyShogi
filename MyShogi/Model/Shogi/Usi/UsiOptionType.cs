using System;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// USIエンジンのoption項目の種別
    /// </summary>
    public enum UsiOptionType
    {
        /// <summary>
        /// エラー or 不明
        /// </summary>
        None,

        //[LabelDescription(Label = "check")]
        CheckBox,

        //[LabelDescription(Label = "spin")]
        SpinBox,

        //[LabelDescription(Label = "combo")]
        ComboBox,

        //[LabelDescription(Label = "button")]
        Button,

        //[LabelDescription(Label = "string")]
        TextBox,
    }

    public static class Util
    {
        /// <summary>
        /// UsiOptionTypeを文字列化する
        /// 
        /// 移植性を考慮してattribute使いたくないので手動で..。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToUsiString(this UsiOptionType type)
        {
            switch (type)
            {
                case UsiOptionType.CheckBox: return "check";
                case UsiOptionType.SpinBox : return "spin";
                case UsiOptionType.ComboBox: return "combo";
                case UsiOptionType.Button  : return "button";
                case UsiOptionType.TextBox : return "string";
            }
            return "null";
        }

        /// <summary>
        /// 文字列をUsiOptionTypeに変換する。
        /// 変換できないときは、UsiOptionType.Noneが返る。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static UsiOptionType FromUsiString(string s)
        {
            switch (s)
            {
                case "check" : return UsiOptionType.CheckBox;
                case "spin"  : return UsiOptionType.SpinBox;
                case "combo" : return UsiOptionType.ComboBox;
                case "button": return UsiOptionType.Button;
                case "string": return UsiOptionType.TextBox;
            }
            return UsiOptionType.None;
        }

        internal static Move FromUsiMove(string moveSfen)
        {
            throw new NotImplementedException();
        }
    }
}
