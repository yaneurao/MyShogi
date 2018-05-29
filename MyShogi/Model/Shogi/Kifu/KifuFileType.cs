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
        JSON , // JSON形式
    }
}
