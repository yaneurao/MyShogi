using System.Collections.Generic;
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

        // -- 以下、Presetで使う時は設定不要。詳細設定ダイアログで使う。

        /// <summary>
        /// 詳細設定ダイアログでこのデータメンバーを表示するのかどうか。
        /// </summary>
        [DataMember]
        public bool Visible;

        /// <summary>
        /// このoption項目に対する説明文。詳細設定ダイアログにはこれが表示される。
        /// </summary>
        [DataMember]
        public string Description;

        /// <summary>
        /// この項目のダイアログ上での表示順。
        /// 数字が小さい順で詳細設定ダイアログには表示される。
        /// </summary>
        [DataMember]
        public int DisplayOrder;

        /// <summary>
        /// エンジン共通設定のValueに従う。このインスタンスのValueの値は使われない。
        /// </summary>
        [DataMember]
        public bool FollowCommonSetting;
    }

    /// <summary>
    /// EngineOptionの配列
    /// </summary>
    [DataContract]
    public class EngineOptions
    {
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
}
