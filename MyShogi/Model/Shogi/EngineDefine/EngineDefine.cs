using System;
using System.Runtime.Serialization;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// USI2.0で規定されているエンジン設定ファイル。
    /// これをxml形式にシリアライズしたものを思考エンジンの実行ファイルのフォルダに配置する。
    /// </summary>
    [DataContract]
    public class EngineDefine
    {
        /// <summary>
        /// エンジンのバナー : 横512px×縦160pxのpng形式 推奨。
        /// このファイルがあるフォルダ相対
        /// </summary>
        [DataMember]
        public string BannerFileName { get; set; } = "banner.png";

        /// <summary>
        /// エンジンの表示名
        /// 
        /// この名前が画面に表示される。
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; } = "思考エンジン";

        /// <summary>
        /// エンジンの実行ファイル名。
        /// 
        /// 例えば"engine"としておくと、AVX2用ならば"engine_avx2.exe"のようになる。
        /// 例)
        ///     "engine_nosse.exe"  : 32bit版
        ///     "engine_sse2.exe"   : 64bit版sse2対応
        ///     "engine_sse41.exe"  : 64bit版sse4.1対応
        ///     "engine_sse42.exe"  : 64bit版sse4.2対応
        ///     "engine_avx2.exe"   : 64bit版avx2対応
        ///     "engine_avx512.exe" : 64bit版avx512対応
        /// </summary>
        [DataMember]
        public string EngineExeName { get; set; } = "engine";

        /// <summary>
        /// エンジンがサポートしているCPUを列挙する。
        /// 
        /// 例えば、思考エンジンがSSE2をサポートしていて、SSE4.1をサポートしていなくて、
        /// 動作環境がSSE4.1なら、SSE2の実行ファイルを呼び出せば良いとわかる。
        /// 
        /// ※　EngineUtility.EngineExeFileName()がそういう処理を行っている。
        /// </summary>
        [DataMember]
        public Cpu[] SupportedCpus { get; set; } = { Cpu.NO_SSE, Cpu.SSE2 , Cpu.SSE41 , Cpu.SSE42 , Cpu.AVX2 };

        /// <summary>
        /// 使用するメモリ 評価関数が使用するメモリ＋探索で使用するメモリ(HASHは除く)
        /// 単位は[MB]
        /// </summary>
        [DataMember]
        public Int64 RequiredMemory { get; set; } = 500;

        /// <summary>
        /// おまかせ設定集
        /// </summary>
        [DataMember]
        public EngineAutoSetting[] AutoSettings;
    }

}
