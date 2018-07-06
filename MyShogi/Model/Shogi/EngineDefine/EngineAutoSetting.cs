using System.Runtime.Serialization;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// エンジンの棋力のおまかせ設定
    /// 
    /// 例 : 「初段」なら、初段の時にすべきEngineOptionを持っている。
    /// 
    /// </summary>
    [DataContract]
    public class EngineAutoSetting
    {
        public EngineAutoSetting() { }

        public EngineAutoSetting(string name , EngineOption[] options)
        {
            Name = name;
            Options = options;
        }

        /// <summary>
        /// 「初段」など、おまかせ設定の名前。
        /// これが対局設定ダイアログのComboBoxに出てくる。
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public EngineOption[] Options { get; set; }
    }
}
