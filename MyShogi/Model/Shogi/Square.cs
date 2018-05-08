namespace MyShogi.Model.Shogi
{
    /// <summary>
    /// 盤上の升を表現する型
    /// </summary>
    public enum Square
    {
        // 以下、盤面の右上から左下までの定数。
        // これを定義していなくとも問題ないのだが、デバッガでSquare型を見たときに
        // どの升であるかが表示されることに価値がある。
        SQ_11, SQ_12, SQ_13, SQ_14, SQ_15, SQ_16, SQ_17, SQ_18, SQ_19,
        SQ_21, SQ_22, SQ_23, SQ_24, SQ_25, SQ_26, SQ_27, SQ_28, SQ_29,
        SQ_31, SQ_32, SQ_33, SQ_34, SQ_35, SQ_36, SQ_37, SQ_38, SQ_39,
        SQ_41, SQ_42, SQ_43, SQ_44, SQ_45, SQ_46, SQ_47, SQ_48, SQ_49,
        SQ_51, SQ_52, SQ_53, SQ_54, SQ_55, SQ_56, SQ_57, SQ_58, SQ_59,
        SQ_61, SQ_62, SQ_63, SQ_64, SQ_65, SQ_66, SQ_67, SQ_68, SQ_69,
        SQ_71, SQ_72, SQ_73, SQ_74, SQ_75, SQ_76, SQ_77, SQ_78, SQ_79,
        SQ_81, SQ_82, SQ_83, SQ_84, SQ_85, SQ_86, SQ_87, SQ_88, SQ_89,
        SQ_91, SQ_92, SQ_93, SQ_94, SQ_95, SQ_96, SQ_97, SQ_98, SQ_99,

        // ゼロと末尾
        ZERO = 0, NB = 81,
        NB_PLUS1 = NB + 1, // 玉がいない場合、SQ_NBに移動したものとして扱うため、配列をSQ_NB+1で確保しないといけないときがあるのでこの定数を用いる。

        // 方角に関する定数。StockfishだとNORTH=北=盤面の下を意味するようだが、
        // わかりにくいのでやねうら王ではストレートな命名に変更する。
        SQ_D = +1, // 下(Down)
        SQ_R = -9, // 右(Right)
        SQ_U = -1, // 上(Up)
        SQ_L = +9, // 左(Left)
    }

    /// <summary>
    /// Square型のためのextension methods
    /// </summary>
    public static class SquareExtensions
    {
        /// <summary>
        /// 値の範囲が正常か調べる。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static bool IsOk(this Square sq)
        {
            return Square.ZERO <= sq && sq <= Square.NB;
        }

        /// <summary>
        /// sqが盤面の内側を指しているかを判定する。assert()などで使う用。玉は盤上にないときにSQ_NBを取るのでこの関数が必要。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static bool IsOkPlus1(this Square sq)
        {
            return Square.ZERO <= sq && sq < Square.NB_PLUS1;
        }

        // 与えられたSquareに対応する筋を返すテーブル。ToFile()で用いる。
        private static readonly File[] SquareToFile_ =
        {
            File.FILE_1, File.FILE_1, File.FILE_1, File.FILE_1, File.FILE_1, File.FILE_1, File.FILE_1, File.FILE_1, File.FILE_1,
            File.FILE_2, File.FILE_2, File.FILE_2, File.FILE_2, File.FILE_2, File.FILE_2, File.FILE_2, File.FILE_2, File.FILE_2,
            File.FILE_3, File.FILE_3, File.FILE_3, File.FILE_3, File.FILE_3, File.FILE_3, File.FILE_3, File.FILE_3, File.FILE_3,
            File.FILE_4, File.FILE_4, File.FILE_4, File.FILE_4, File.FILE_4, File.FILE_4, File.FILE_4, File.FILE_4, File.FILE_4,
            File.FILE_5, File.FILE_5, File.FILE_5, File.FILE_5, File.FILE_5, File.FILE_5, File.FILE_5, File.FILE_5, File.FILE_5,
            File.FILE_6, File.FILE_6, File.FILE_6, File.FILE_6, File.FILE_6, File.FILE_6, File.FILE_6, File.FILE_6, File.FILE_6,
            File.FILE_7, File.FILE_7, File.FILE_7, File.FILE_7, File.FILE_7, File.FILE_7, File.FILE_7, File.FILE_7, File.FILE_7,
            File.FILE_8, File.FILE_8, File.FILE_8, File.FILE_8, File.FILE_8, File.FILE_8, File.FILE_8, File.FILE_8, File.FILE_8,
            File.FILE_9, File.FILE_9, File.FILE_9, File.FILE_9, File.FILE_9, File.FILE_9, File.FILE_9, File.FILE_9, File.FILE_9,
            File.NB, // 玉が盤上にないときにこの位置に移動させることがあるので
        };
        
        /// <summary>
        /// int型に変換する。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static int ToInt(this Square sq)
        {
            return (int)sq;
        }

        /// <summary>
        /// その升の属する筋を返す。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static File ToFile(this Square sq)
        {
            return SquareToFile_[sq.ToInt()];
        }

        /// <summary>
        /// SquareからRankに変換するためのテーブル。
        /// </summary>
        private static readonly Rank[] SquareToRank_ =
        {
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.RANK_1, Rank.RANK_2, Rank.RANK_3, Rank.RANK_4, Rank.RANK_5, Rank.RANK_6, Rank.RANK_7, Rank.RANK_8, Rank.RANK_9,
            Rank.NB, // 玉が盤上にないときにこの位置に移動させることがあるので
        };

        /// <summary>
        /// その升の属する段を返す。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Rank ToRank(this Square sq)
        {
            return SquareToRank_[sq.ToInt()];
        }

        /// <summary>
        /// 盤面を180°回したときの升目を返す
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Square Inv(this Square sq)
        {
            return (Square)(((int)Square.NB - 1) - sq.ToInt());
        }

        /// <summary>
        /// 盤面をミラーしたときの升目を返す
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static Square Mir(this Square sq)
        {
            return Util.MakeSquare( (File)(8 - sq.ToFile().ToInt()) , sq.ToRank());
        }

        /// <summary>
        /// Squareを綺麗に出力する(USI形式ではない)
        /// 日本語文字での表示になる。例 → ８八
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static string Pretty(this Square sq)
        {
            return sq.ToFile().Pretty() + sq.ToRank().Pretty();
        }

        /// <summary>
        /// USI形式でSquareを出力する
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public static string ToUSI(this Square sq)
        {
            return sq.ToFile().ToUSI() + sq.ToRank().ToUSI();
        }
    }
}
