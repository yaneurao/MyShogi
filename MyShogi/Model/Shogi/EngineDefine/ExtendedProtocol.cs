namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// 思考エンジン側がサポートしているUSI拡張機能について表明するために用いる。
    /// 
    /// それぞれの詳しい意味、経緯については"docs/USI2.0.md"を参照のこと。
    /// </summary>
    public enum ExtendedProtocol
    {
        /// <summary>
        /// "go"コマンドでbbyoyomi , wbyoyomiとして先手と後手の秒読み設定を送ってもらう。
        /// また"go ponder" , "ponderhit" 時にも先後の残り時間がやってくる。
        /// </summary>
        GoCommandTimeExtention,

        /// <summary>
        /// USIプロトコルでは置換表サイズの設定に"USI_Hash"が使われているが、
        /// 将棋所は、個別に置換表サイズを設定できないため、思考エンジン側が独自のオプションで
        /// 置換表サイズの設定が出来るようにしてあるのが実状である。
        /// (やねうら王は、これに該当する。)
        /// 
        /// この拡張を有効にすると、GUI側は、思考エンジンに対して
        /// "USI_Hash"の代わりに"Hash"を送信する。
        /// </summary>
        UseHashCommandExtension,
    }
}
