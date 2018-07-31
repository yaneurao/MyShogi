using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// USI2.0で規定されているエンジン設定ファイル。
    /// これをxml形式にシリアライズしたものを思考エンジンの実行ファイルのフォルダに配置する。
    /// 
    /// GUI側では起動時にこのファイルを読み込み、これを活用する。
    /// </summary>
    [DataContract]
    public class EngineDefine
    {
        /// <summary>
        /// エンジン説明1行で
        /// </summary>
        [DataMember]
        public string DescriptionSimple { get; set; } = "エンジン説明";

        /// <summary>
        /// エンジン説明5行ぐらいで。
        /// エンジン選択のときに表示される。
        /// </summary>
        [DataMember]
        public string Description { get; set; } = "エンジン説明";

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
        /// 例えばこの値を"engine"としておくと、EngineExeFileName()を使った時に、AVX2用ならば"engine_avx2.exe"のようになる。
        /// 
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
        public List<CpuType> SupportedCpus { get; set; } = new List<CpuType>(new []
        { CpuType.NO_SSE, CpuType.SSE2 , CpuType.SSE41 , CpuType.SSE42 , CpuType.AVX2 });

        /// <summary>
        /// 使用するメモリ 探索で使用するメモリ(HASHは除く)単位は[MB]
        ///
        /// EvalHashが130MBほどあるのでUSI待受スレッド用(25MB)+定跡丸読み(50MB程度) ≒ 200MB。
        /// NO SSEの時はEvalHashは無効だが、まあ、少し多めに評価している分には問題ないか…。
        /// 定跡ファイルが大きいとこれよりメモリがたくさん必要になる。
        /// </summary>
        [DataMember]
        public Int64 WorkingMemory { get; set; } = 200;

        /// <summary>
        /// スレッドごとのサイズ。単位は[MB]
        /// Threads設定×この値分だけ余分に物理メモリを消費するものとする。
        /// </summary>
        [DataMember]
        public Int64 StackPerThread { get; set; } = 25;

        /// <summary>
        /// 評価関数用のメモリ。単位は[MB]
        /// (メモリ共有する場合は、2つ目のエンジンはこの分だけ減算される。)
        /// </summary>
        [DataMember]
        public Int64 EvalMemory { get; set; } = 470;

        /// <summary>
        /// 置換表(HASH)用の最小メモリ。これくらいはないとまともに動かないというライン。
        /// 単位は[MB]
        /// 
        /// ※　GUI側では、RequiredMemory + MinimumHashMemoryの分だけ空き物理メモリがないとエンジン選択時に警告を出す。
        /// </summary>
        [DataMember]
        public Int64 MinimumHashMemory { get; set; } = 100;

        /// <summary>
        /// エンジン選択画面での表示順。これが大きい順に表示することになっている。
        /// デフォルト0。(一番最後に表示される)
        /// 
        /// 商用版のために10000～19999を予約。
        /// 商用版(2018)は10000～10099を使用。
        /// </summary>
        [DataMember]
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// おまかせ設定集
        /// </summary>
        [DataMember]
        public List<EnginePreset> Presets;

        /// <summary>
        /// setoptionで設定出来るオプションに対する説明。
        /// nullであってもエンジンに対して"usi"を送信して、その時に返ってきたものを
        /// 表示するので問題はない。
        /// </summary>
        [DataMember]
        public List<EngineOptionDescription> EngineOptionDescriptions;

        /// <summary>
        /// USI拡張プロトコルのうちサポートしているものを列挙する。
        /// 
        /// なければないで良い。
        /// </summary>
        [DataMember]
        public List<ExtendedProtocol> SupportedExtendedProtocol;
    }

}
