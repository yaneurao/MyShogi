namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// 棋譜ファイルのフォーマット
    /// </summary>
    public enum KifuFileType
    {
        KIF  , // KIF形式
        KI2  , // KI2形式
        CSA  , // CSA形式
        PSN  , // PSN形式
        PSN2 , // PSN2形式
        SFEN , // SFEN形式
        JKF, // JKF形式
        JSON , // JSON形式

        UNKNOWN , // それ以外。不明な時に用いる。
    }

    public static class KifuFileTypeExtensions
    {
        /// <summary>
        /// 拡張子で使う文字列に変換する
        /// </summary>
        /// <returns></returns>
        public static string ToExtensions(this KifuFileType type)
        {
            switch(type)
            {
                case KifuFileType.KIF: return ".kif";
                case KifuFileType.KI2: return ".ki2";
                case KifuFileType.CSA: return ".csa";
                case KifuFileType.PSN: return ".psn";
                case KifuFileType.PSN2: return ".psn2";
                case KifuFileType.SFEN: return ".sfen";
                case KifuFileType.JSON: return ".json";
                case KifuFileType.JKF: return ".jkf";
                case KifuFileType.UNKNOWN: return ".unknown";
            }
            return "";
        }

        /// <summary>
        /// 拡張子文字列を渡して、そのKifuFileTypeを返す。
        /// 末尾しか見ていないので拡張子ではなく、file pathでも良いる
        /// </summary>
        /// <param name="extension"></param>
        public static KifuFileType StringToKifuFileType(string extentions)
        {
            var ext = extentions.ToLower();
            if (ext.EndsWith("kif") || ext.EndsWith("kifu")) return KifuFileType.KIF;
            if (ext.EndsWith("ki2") || ext.EndsWith("kif2") || ext.EndsWith("ki2u") || ext.EndsWith("kif2u")) return KifuFileType.KI2;
            if (ext.EndsWith("csa")) return KifuFileType.CSA;
            if (ext.EndsWith("psn")) return KifuFileType.PSN;
            if (ext.EndsWith("psn2")) return KifuFileType.PSN2;
            if (ext.EndsWith("sfen")) return KifuFileType.SFEN;
            if (ext.EndsWith("json")) return KifuFileType.JSON;
            if (ext.EndsWith("jkf")) return KifuFileType.JKF;

            return KifuFileType.UNKNOWN;
        }
    }
}
