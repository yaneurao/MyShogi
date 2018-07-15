using System.Collections.Generic;
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
    public class EnginePreset
    {
        public EnginePreset() { }

        public EnginePreset(string name)
        {
            Name = name;
        }

        public EnginePreset(string name , string description)
        {
            Name = name;
            Description = description;
        }

        public EnginePreset(string name, List<EngineOption> options)
        {
            Name = name;
            Options = new List<EngineOption>(options); // Clone
        }

        public EnginePreset(string name, EngineOption[] options)
        {
            Name = name;
            Options = new List<EngineOption>(options); // Copy
        }

        public EnginePreset(string name , string description , List<EngineOption> options)
        {
            Name = name;
            Description = description;
            Options = new List<EngineOption>(options); // Clone
        }

        public EnginePreset(string name, string description, EngineOption[] options)
        {
            Name = name;
            Description = description;
            Options = new List<EngineOption>(options); // Copy
        }

        /// <summary>
        /// 「初段」など、おまかせ設定の名前。
        /// これが対局設定ダイアログのComboBoxに出てくる。
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// このプリセットの説明。
        /// 例「初段用に合わせた調整です。」
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// このプリセットの時のオプション。
        /// ここで設定したもの以外は、エンジンの共通設定に従う。
        /// </summary>
        [DataMember]
        public List<EngineOption> Options { get; set; }
    }
}
