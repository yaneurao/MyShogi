
namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// EngineDefine + "engine_define.xml"へのpathなどをくっつけたもの。
    /// </summary>
    public class EngineDefineEx
    {
        /// <summary>
        /// 読み込んだ"engine_define.xml"をデシリアライズしたもの。
        /// </summary>
        public EngineDefine EngineDefine;

        /// <summary>
        /// "engine_define.xml"のフォルダpath
        /// 
        /// 例)
        ///     "engine/qhapaq/engine_define.xml"
        ///     ならば、
        ///     "engine/qhapaq/"
        ///     
        /// シリアライズ時には、これをエンジンのID代わりに使っている。
        /// </summary>
        public string FolderPath;
    }
}
