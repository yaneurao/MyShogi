using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// エンジンのoptionとその値のペア
    /// ユーザーの設定した値を保存するのに用いる。
    /// </summary>
    [DataContract]
    public class EngineOption 
    {
        public EngineOption(string name_, string value_)
        {
            Name = name_;
            Value = value_;
        }

        /// <summary>
        /// オプション名
        /// </summary>
        [DataMember]
        public string Name;

        /// <summary>
        /// そこに設定する値
        /// 
        /// 数字なども文字列化してセットする。
        /// type : check のときは、"true"/"false"
        /// UsiOptionクラスに従う。
        /// </summary>
        [DataMember]
        public string Value;
    }

    /// <summary>
    /// EngineOptionの配列
    /// 
    /// これは、思考エンジンのプリセットで用いる。
    /// </summary>
    [DataContract]
    public class EngineOptions
    {
        public EngineOptions()
        {
            Options = new List<EngineOption>();
        }

        public EngineOptions(List<EngineOption> options)
        {
            Options = options;
        }

        /// <summary>
        /// nullであれば、丸ごとエンジンの個別設定＋共通設定に従う。
        /// nullでなければ、こちらが優先され、設定していない項目は、エンジンの個別設定＋共通設定に従う。
        /// </summary>
        [DataMember]
        public List<EngineOption> Options;
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class EngineOptionDescription
    {
        public EngineOptionDescription(string name,string displayName , 
            string descriptionSimple , string description)
        {
            Name = name;
            DisplayName = displayName;
            DescriptionSimple = descriptionSimple;
            Description = description;
        }

        /// <summary>
        /// (元の)オプション名
        /// ここがnullなら、単なる項目名としてDisplayNameを表示する。
        /// </summary>
        [DataMember]
        public string Name;

        /// <summary>
        /// オプションの表示名(日本語にしておくとわかりやすい)
        /// </summary>
        [DataMember]
        public string DisplayName;

        /// <summary>
        /// 説明文。ダイアログ上に表示される。1文ぐらい。
        /// </summary>
        [DataMember]
        public string DescriptionSimple;

        /// <summary>
        /// 説明文。ダイアログ上に表示される。
        /// </summary>
        [DataMember]
        public string Description;
    }


    /// <summary>
    /// エンジンオプションの共通設定で用いる用。
    /// こちらは、UI上から設定するため、説明文や、type、min-maxなどが必要。
    /// ゆえに、
    /// ・EngineOptionと同じinterfaceを持ち
    /// さらに、
    /// ・UsiOptionをbuild出来る文字列
    /// ・説明文
    /// を持っている。
    /// 
    /// 説明文はEngineOptionDescriptionsで与えられるから不要か…。
    /// </summary>
    [DataContract]
    public class EngineOptionForSetting
    {
        public EngineOptionForSetting(string name , string value , string buildString)
        {
            Name = name;
            Value = value;
            BuildString = buildString;
        }

        /// <summary>
        /// オプション名
        /// </summary>
        [DataMember]
        public string Name;

        /// <summary>
        /// そこに設定する値
        /// 
        /// 数字なども文字列化してセットする。
        /// type : check のときは、"true"/"false"
        /// UsiOptionクラスに従う。
        /// </summary>
        [DataMember]
        public string Value;

        /// <summary>
        /// UsiOptionオブジェクトを構築するための文字列。
        /// エンジン共通設定の時のみ有効。
        /// (エンジン個別設定の時は、エンジンから"option"をもらってこの文字列を構築する。)
        /// 
        /// "option name USI_Hash type spin default 256"の
        /// "type spin default 256"
        /// の部分。
        /// 
        /// エンジン共通設定としては、
        /// default値は無視されてValueのほうが採用される。
        /// default値にリセットする時に、default値が採用される。
        /// </summary>
        [DataMember]
        public string BuildString;

        /// <summary>
        /// エンジン共通設定に従うのか
        /// (エンジン個別設定の時のみ有効)
        /// </summary>
        [DataMember]
        public bool FollowCommonSetting;
    }

    /// <summary>
    /// エンジンの共通設定/個別設定に使う用。
    /// </summary>
    [DataContract]
    public class EngineOptionsForSetting
    {
        [DataMember]
        public List<EngineOptionForSetting> Options;

        [DataMember]
        /// <summary>
        /// エンジンのオプションの説明文
        /// これが与えられている場合、この順番で表示され、ここにないoptionは表示されない。
        /// </summary>
        public List<EngineOptionDescription> Descriptions;
    }

}
