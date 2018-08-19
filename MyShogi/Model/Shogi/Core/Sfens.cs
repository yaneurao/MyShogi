namespace MyShogi.Model.Shogi.Core
{
    /// <summary>
    /// 平手、駒落ちなどのsfen文字列
    /// </summary>
    public static class Sfens
    {
        /// <summary>
        /// それぞれの意味については、BoardTypeのenumの定義を見ること
        /// </summary>
        public static readonly string HIRATE = "lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1";
        public static readonly string HANDICAP_KYO = "lnsgkgsn1/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_RIGHT_KYO = "1nsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_KAKU = "lnsgkgsnl/1r7/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_HISYA = "lnsgkgsnl/7b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_HISYA_KYO = "lnsgkgsn1/7b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_2 = "lnsgkgsnl/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_3 = "lnsgkgsn1/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_4 = "1nsgkgsn1/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_5 = "2sgkgsn1/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_LEFT_5 = "1nsgkgs2/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_6 = "2sgkgs2/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_8 = "3gkg3/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_10 = "4k4/9/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w - 1";
        public static readonly string HANDICAP_PAWN3 = "4k4/9/9/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL w 3p 1";

        public static readonly string MATE_1 = "4k4/9/9/9/9/9/9/9/9 b 18p4l4n4s4g2b2r 1";
        public static readonly string MATE_2 = "4k4/9/9/9/9/9/9/9/4K4 b 18p4l4n4s4g2b2r 1";
        public static readonly string MATE_3 = "4k4/9/9/9/9/9/9/9/4K4 b - 1";

        // あとで何か追加するかも。
    }
}
