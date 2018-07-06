using System.Runtime.Serialization;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// エンジンのoptionとその値のペア
    /// </summary>
    [DataContract]
    public class EngineOption
    {
        public EngineOption(string name_ , string value_)
        {
            name = name_;
            value = value_;
        }

        /// <summary>
        /// オプション名
        /// </summary>
        [DataMember]
        public string name;

        /// <summary>
        /// そこに設定する値
        /// </summary>
        [DataMember]
        public string value;
    }
}
