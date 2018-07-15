using System.Collections.Generic;
using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// エンジンのoptionとその値のペア
    /// ユーザーの設定した値を保存するのに用いる。
    /// エンジン共通設定用。
    /// </summary>
    [DataContract]
    public class EngineOption
    {
        public EngineOption(string name, string value)
        {
            Name = name;
            Value = value;
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
    /// エンジンのoptionとその値のペア
    /// ユーザーの設定した値を保存するのに用いる。
    /// エンジン個別設定用。
    /// </summary>
    [DataContract]
    public class EngineOptionForIndivisual
    {
        public EngineOptionForIndivisual(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public EngineOptionForIndivisual(string name, string value, bool followCommonSetting)
        {
            Name = name;
            Value = value;
            FollowCommonSetting = followCommonSetting;
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
        /// エンジン共通設定に従うのか
        /// (エンジン個別設定の時のみ有効)
        /// </summary>
        [DataMember]
        public bool FollowCommonSetting;
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class EngineOptionDescription
    {
        public EngineOptionDescription(string name,string displayName = null, 
            string descriptionSimple = null , string description = null , string usiBuildString = null)
        {
            Name = name;
            DisplayName = displayName;
            DescriptionSimple = descriptionSimple;
            Description = description;
            UsiBuildString = usiBuildString;
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

        /// <summary>
        /// ComboBoxに表示する値に対応する日本語名
        /// 
        /// "var1,日本語1,var2,日本語2"
        /// のようにカンマ区切りで書くとvar1,var2が「日本語1」「日本語2」と日本語化される。
        /// </summary>
        [DataMember]
        public string ComboboxDisplayName;

        /// <summary>
        /// この項目が、エンジン側にない時に、Controlを構築するためにUsiOptionのインスタンスが必要なので、
        /// それを構築するためのUSI文字列。
        /// </summary>
        [DataMember]
        public string UsiBuildString;

        /// <summary>
        /// このオプション項目をエンジン設定ダイアログの表示から隠す
        /// 表示から隠したい項目に対しては、これをtrueにしたEngineOptionDescriptionを用意する。
        /// </summary>
        [DataMember]
        public bool Hide;
    }

    /// <summary>
    /// エンジンオプションの共通設定/個別設定でGUI側から用いる用。
    /// data bindできるようにするためにNotifyObjectになっている。
    /// 
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
    public class EngineOptionForSetting : NotifyObject
    {
        public EngineOptionForSetting(string name , string usiBuildString)
        {
            Name = name;
            UsiBuildString = usiBuildString;
        }

        /// <summary>
        /// オプション名
        /// </summary>
        public string Name;

        /// <summary>
        /// データバインドしている値
        /// </summary>
        public string Value
        {
            get { return GetValue<string>("Value"); }
            set { SetValue<string>("Value", value); }
        }

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
        public string UsiBuildString;

        /// <summary>
        /// エンジン共通設定に従うのか
        /// (エンジン個別設定の時のみ有効)
        /// </summary>
        [DataMember]
        public bool FollowCommonSetting
        {
            get { return GetValue<bool>("FollowCommonSetting"); }
            set { SetValue<bool>("FollowCommonSetting", value); }
        }

        /// <summary>
        /// 「共通設定に従う」のチェックボックスを表示するのか。
        /// (共通設定のほうに同じ名前の項目がなければチェックボックスを出せない。)
        /// </summary>
        public bool EnableFollowCommonSetting;
    }

    /// <summary>
    /// エンジンの共通設定/個別設定に使う用。
    /// EngineOptionSettingDialogに対して設定するのに用いる。
    /// これをこのままシリアライズすることはない。
    /// </summary>
    public class EngineOptionsForSetting
    {
        public List<EngineOptionForSetting> Options;

        /// <summary>
        /// エンジンのオプションの説明文
        /// これが与えられている場合、この順番で表示され、ここにないoptionは表示されない。
        /// </summary>
        public List<EngineOptionDescription> Descriptions;

        /// <summary>
        /// エンジン個別設定であるか。
        /// これをtrueにすると各option項目に対して
        /// 「共通設定に従う」のオプションがダイアログ表示される。
        /// </summary>
        public bool IndivisualSetting;

        /// <summary>
        /// Options == nullのときに
        /// DescriptionsからOptionsを設定する。
        /// 
        /// エンジン共通設定で使う時がこれ。
        /// </summary>
        public void BuildOptionsFromDescriptions()
        {
            // DescriptionsからOptionsを構築する。

            var options = new List<EngineOptionForSetting>();

            foreach (var desc in Descriptions)
                if (!desc.Hide)
                {
                    var option = new EngineOptionForSetting(desc.Name, desc.Name)
                    {
                        UsiBuildString = desc.UsiBuildString
                    };
                    options.Add(option);
                }

            Options = options;
        }

        /// <summary>
        /// OptionsのValueを上書きする(そのNameのentryがOptionsにあれば)
        /// </summary>
        /// <param name="options"></param>
        public void OverwriteEngineOptions(List<EngineOption> options )
        {
            foreach (var option in options)
            {
                var opt = Options.Find(x => x.Name == option.Name);
                if (opt == null)
                    continue;
                opt.Value = option.Value;
                //opt.FollowCommonSetting = option.FollowCommonSetting;
            }
        }

        public void OverwriteEngineOptions(List<EngineOptionForIndivisual> options , EngineOptionsForSetting commonSetting)
        {
            // 前回なかった(ユーザーの選択が保存されていない)新規要素で、
            // かつ、これが共通設定にあるのなら、デフォルトでは共通設定に従うべきだから、全部いったん、そう設定する。
            foreach (var option in Options)
            {
                var exist = commonSetting.Options.Exists(x => x.Name == option.Name);
                option.EnableFollowCommonSetting = exist;
                option.FollowCommonSetting = exist;
            }

            foreach (var option in options)
            {
                var opt = Options.Find(x => x.Name == option.Name);

                if (opt == null)
                    continue;

                opt.Value = option.Value;
                opt.FollowCommonSetting = option.FollowCommonSetting;
            }
        }

        // エンジン共通設定に従う設定であっても、
        // エンジン個別設定のほうの値域を守らないといけないという話はあるか…。まあいいか…。あとで考える。

        /// <summary>
        /// このメンバの持つOptionsの
        /// NameとValueのペアをEngineOptionsとして書き出す
        /// </summary>
        /// <returns></returns>
        public List<EngineOption> ToEngineOptions()
        {
            var options = new List<EngineOption>();

            foreach (var opt in Options)
                options.Add(new EngineOption(opt.Name, opt.Value));

            return options;
        }

        public List<EngineOptionForIndivisual> ToEngineOptionsForIndivisual()
        {
            var options = new List<EngineOptionForIndivisual>();

            foreach (var opt in Options)
                options.Add(new EngineOptionForIndivisual(
                    opt.Name, opt.Value, opt.FollowCommonSetting));

            return options;
        }
    }
}
