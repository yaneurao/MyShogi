using System.Collections.Generic;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// optionの各パラメータを保持します。
    /// </summary>
    public sealed class UsiOption
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UsiOption()
        {
            ComboList = new List<string>();
            MinValue = long.MinValue;
            MaxValue = long.MaxValue;
        }

        /// <summary>
        /// USIのsetoptionの行のparseをする。
        /// 
        /// translationDic : option名に対応する翻訳名があるなら、それをTranslateNameとして設定する。
        /// </summary>
        public static UsiOption Parse(string command,
                                      Dictionary<string, string> translationDic = null)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new UsiException("Parse()でcommand == null");
            }

            var option = new UsiOption();

#if false
            var scanner = new Scanner(command);
            scanner.SetDelimiters(" ");

            if (scanner.ParseText() != "option")
            {
                throw new UsiException(
                    command + ": invalid command.");
            }

            var value = string.Empty;
            while (!scanner.IsEof)
            {
                switch (scanner.ParseText())
                {
                    case "name":
                        option.Name = ParseName(scanner);
                        option.TranslatedName = Translate(option.Name, null, translationDic);
                        break;
                    case "type":
                        option.OptionType = ParseType(scanner.ParseText());
                        break;
                    case "default":
                        value = scanner.ParseText();
                        value = Translate(value, value, translationDic);
                        break;
                    case "min":
                        option.MinValue = int.Parse(scanner.ParseText());
                        break;
                    case "max":
                        option.MaxValue = int.Parse(scanner.ParseText());
                        break;
                    case "var":
                        var varText = scanner.ParseText();
                        varText = Translate(varText, varText, translationDic);
                        option.ComboList.Add(varText);
                        break;
                    default:
                        throw new UsiException(
                            "invalid command: " + command);
                }
            }

            // 範囲調整を行っているため、値は最後に設定する。
            if (!string.IsNullOrEmpty(value))
            {
                option.SetDefault(value);
            }
#endif

            return option;
        }


        /// <summary>
        /// オプション名を取得します。
        /// 
        /// これは、Engineから直接渡された名前で、
        /// USIプロトコルでエンジンに渡すときはこれを渡す
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// オプションの翻訳名を取得します。
        /// 
        /// 日本語テキストなどがある場合は、これが設定される。
        /// </summary>
        public string TranslatedName
        {
            get;
            private set;
        }

        /// <summary>
        /// オプションの表示名を取得する。
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(TranslatedName))
                {
                    return TranslatedName;
                }

                return Name;
            }
        }

        /// <summary>
        /// 最小値を取得する。
        /// </summary>
        public long MinValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 最大値を取得する。
        /// </summary>
        public long MaxValue
        {
            get;
            private set;
        }

        /// <summary>
        /// コンボボックスのリストを取得する。
        /// </summary>
        public List<string> ComboList
        {
            get;
            private set;
        }

        /// <summary>
        /// オプションの型を取得する。
        /// </summary>
        public UsiOptionType OptionType
        {
            get;
            private set;
        }

        /// <summary>
        /// デフォルト値(bool型)を取得する。
        /// OptionTypeがCheckBoxの時に有効。
        /// </summary>
        public bool DefaultBool
        {
            get; private set;
        }

        /// <summary>
        /// デフォルトテキストを取得する。
        /// OptionTypeがTextBoxの時に有効。
        /// </summary>
        public string DefaultText
        {
            get; private set;
        }

        /// <summary>
        /// デフォルト値(long型)を設定/取得します。
        /// OptionTypeがSpinBoxの時に有効。
        /// </summary>
        public long DefaultValue
        {
            get { return this.defaultValue; }
            private set
            {
                // valueがMinValue～MaxValueの間であることを保証する。
                value = System.Math.Max(value, MinValue);
                value = System.Math.Min(value, MaxValue);
                this.defaultValue = value;
            }
        }
        private long defaultValue;


        /// <summary>
        /// 文字列化する。これは表示に使うものではなく、デバッグに使うためのもの。
        /// </summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(TranslatedName))
            {
                return string.Format(
                    "(Name={0}, Value={1})",
                    Name, GetDefault());
            }
            else
            {
                return string.Format(
                    "(Name={0}, DisplayName={1}, Value={2})",
                    Name, DisplayName, GetDefault());
            }
        }


        /// <summary>
        /// 各オプション型ごとのデフォルト値を設定します。
        /// </summary>
        public void SetDefault(string text)
        {
            switch (OptionType)
            {
                case UsiOptionType.CheckBox:
                    DefaultBool = bool.Parse(text);
                    break;
                case UsiOptionType.SpinBox:
                    DefaultValue = int.Parse(text);
                    break;
                case UsiOptionType.ComboBox:
                    DefaultText = text;
                    break;
                case UsiOptionType.Button:
                    DefaultText = text;
                    break;
                case UsiOptionType.TextBox:
                    DefaultText = (text == "<empty>" ? string.Empty : text);
                    break;
                default:
                    throw new UsiException(
                        string.Format(
                            "{0}: unknown option type",
                            OptionType));
            }
        }

        /// <summary>
        /// 各オプション型ごとのデフォルト値を文字列で取得します。
        /// </summary>
        public string GetDefault()
        {
            switch (OptionType)
            {
                case UsiOptionType.CheckBox:
                    return (DefaultBool ? "true" : "false");
                case UsiOptionType.SpinBox:
                    return DefaultValue.ToString();
                case UsiOptionType.ComboBox:
                    return DefaultText;
                case UsiOptionType.Button:
                    return DefaultText;
                case UsiOptionType.TextBox:
                    return (string.IsNullOrEmpty(DefaultText) ?
                        "<empty>" :
                        DefaultText);
                default:
                    throw new UsiException(
                        string.Format(
                            "{0}: GetDefault()に失敗しました。",
                            OptionType));
            }
        }

        /// <summary>
        /// USIで出力するコマンド
        /// </summary>
        public string UsiCommand
        {
            get
            {
                return string.Format("option name {0} type {1} default {2}" , Name,Util.ToUsiString(OptionType) , DefaultText);
            }
        }

        // --- 以下、static field

        ///// <summary>
        ///// USI_Hashを扱うオプションです。
        ///// </summary>
        //public static readonly UsiOption USI_Hash = new UsiOption("option name USI_Hash type spin default 256");

        ///// <summary>
        ///// USI_Ponderを扱うオプションです。
        ///// </summary>
        //public static readonly UsiOption USI_Ponder = new UsiOption("option name USI_Ponder type check default false");

    }


}
