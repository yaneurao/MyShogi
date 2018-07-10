using System.Runtime.Serialization;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// エンジン別のオプション設定
    /// </summary>
    [DataContract]
    public class IndivisualEngineOption
    {
        /// <summary>
        /// EngineDefineEx.FolderPathと同等。
        /// </summary>
        [DataMember]
        public string FolderPath;

        /// <summary>
        /// このエンジンに対する個別設定。
        /// 共通設定をこれで上書きする。
        /// </summary>
        [DataMember]
        public EngineOptions Options;
    }

    /// <summary>
    /// エンジンの共通設定＋個別設定
    /// "MyShogi.engine.xml"としてMyShogi.exeの存在するフォルダにシリアライズして書き出す。
    /// </summary>
    [DataContract]
    public class EngineConfig
    {
        /// <summary>
        /// エンジンの共通設定
        /// </summary>
        [DataMember]
        public EngineOptions CommonOptions;

        /// <summary>
        /// 各エンジンの個別設定
        /// </summary>
        [DataMember]
        public IndivisualEngineOption[] IndivisualEngineOption;

    }

    /// <summary>
    /// EngineConfigのシリアライズ/デシリアライズを行う補助クラス
    /// </summary>
    public static class EngineConfigUtility
    {

    }
}
