using System.Collections.Generic;
using System.Linq;
using MyShogi.Model.Common.Utility;

namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// optionの各パラメータを保持します。
    /// 
    /// 要件
    /// ・default値を保持している。
    /// ・ユーザーが値を変更できる。
    /// 
    /// 翻訳名は、このクラスでは管理しない。
    /// ファイルへの保存は、UsiOptionMinを用いる。
    /// </summary>
    public sealed class UsiOption
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiOption()
        {
            ComboList = new List<string>();
            MinValue = long.MinValue;
            MaxValue = long.MaxValue;
        }

        /// <summary>
        /// このオブジェクトを複製して返す。
        /// </summary>
        /// <returns></returns>
        public UsiOption Clone()
        {
            return MemberwiseClone() as UsiOption;
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

        /// --- ↑のものは、Parse()の結果として設定される。
        /// --- ↓のものは、Parse()の結果として設定されるが、そのあと外部から値を変更することが出来る。

        /// <summary>
        /// デフォルト値(bool型)を設定/取得する。
        /// OptionTypeがCheckBoxの時に有効。
        /// </summary>
        public bool DefaultBool
        {
            get; set;
        }

        /// <summary>
        /// デフォルトテキストを取得する。
        /// OptionTypeがTextBoxの時に有効。
        /// </summary>
        public string DefaultText
        {
            get; set;
        }

        /// <summary>
        /// デフォルト値(long型)を設定/取得します。
        /// OptionTypeがSpinBoxの時に有効。
        /// </summary>
        public long DefaultValue
        {
            get { return defaultValue; }
            set
            {
                // valueがMinValue～MaxValueの間であることを保証する。
                value = System.Math.Max(value, MinValue);
                value = System.Math.Min(value, MaxValue);
                defaultValue = value;
            }
        }
        private long defaultValue;

        /// <summary>
        /// 文字列化する。これは表示に使うものではなく、デバッグに使うためのもの。
        /// </summary>
        public override string ToString()
        {
            return string.Format("(Name={0}, Value={1})", Name, GetDefault());
        }

        /// <summary>
        /// 各OptionTypeごとのデフォルト値を(文字列から)設定する。
        /// 
        /// このmethodを用いずにDefaultBoolやDefaultValueなどに直接設定することも出来る。
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
        /// 各OptionTypeごとのデフォルト値を文字列で取得する。
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
        /// USIのsetoptionの行のparseをする。
        /// </summary>
        public static UsiOption Parse(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new UsiException("Parse()でcommand == null");
            }

            var option = new UsiOption();

            // スペースをセパレータとして分離する
            var scanner = new Scanner(command);

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
                        break;
                    case "type":
                        option.OptionType = ParseType(scanner.ParseText());
                        break;
                    case "default":
                        value = scanner.ParseText();
                        break;
                    case "min":
                        option.MinValue = long.Parse(scanner.ParseText());
                        break;
                    case "max":
                        option.MaxValue = long.Parse(scanner.ParseText());
                        break;
                    case "var":
                        var varText = scanner.ParseText();
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

            return option;
        }


        /// <summary>
        /// 現在のこのクラスの状態に基づいて、USIのsetoption用の文字列を作成する。
        /// </summary>
        public string CreateSetOptionCommandString()
        {
            var result = new List<string>();

            result.Add("setoption");
            result.Add("name");
            result.Add(Name);

            if (OptionType != UsiOptionType.Button)
            {
                result.Add("value");
                result.Add(GetDefault());
            }

            return string.Join(" ", result);
        }

        /// <summary>
        /// 現在のこのクラスの状態を表現するUSIのoptionコマンド文字列を作成する。
        /// (エンジンのオプション設定のダイアログで用いる。USI思考エンジンに対して実際にこれを
        /// 送信するわけではない。)
        /// </summary>
        /// <returns></returns>
        public string CreateOptionCommandString()
        {
            var result = new List<string>();

            result.Add("option");
            result.Add("name");
            result.Add(Name);

            result.Add("type");
            result.Add(Util.ToUsiString(OptionType));

            if (OptionType != UsiOptionType.Button)
            {
                // "option"の時は"value"ではなく"default"値。
                result.Add("default");
                result.Add(GetDefault());
            }

            if (OptionType == UsiOptionType.SpinBox)
            {
                result.Add("min");
                result.Add(MinValue.ToString());
                result.Add("max");
                result.Add(MaxValue.ToString());
            }

            if (OptionType == UsiOptionType.ComboBox)
            {
                if (ComboList != null)
                {
                    foreach (var c in ComboList)
                    {
                        // 値ひとつずつ"var"をくっつけて出力しなければならない。
                        result.Add("var");
                        result.Add(c);
                    }
                }
            }

            return string.Join(" ", result);
        }

        /// <summary>
        /// Nameを無理やり書き換える時に用いる。特殊な用途で。
        /// "Hash_"を"USI_Hash"に置換する時などに用いている。
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            Name = name;
        }


        // --- 以下、private method

        /// <summary>
        /// 名前をパースします。
        /// </summary>
        /// <remarks>
        /// 名前に空白が含まれることがあるため、その対策を行います。
        /// </remarks>
        private static string ParseName(Scanner scanner)
        {
            var keywords = new string[]
            {
                "name", "type", "default", "min", "max", "var"
            };
            var result = new List<string>();

            while (true)
            {
                var peek = scanner.PeekText();
                if (peek == null)
                {
                    return string.Join(" ", result);
                }

                // 次の単語がキーワードであれば、次に進みます。
                if (keywords.Contains(peek))
                {
                    return string.Join(" ", result);
                }

                // キーワードでなければ名前の一部なので、
                // オプション名として処理します。
                scanner.ParseText();
                result.Add(peek);
            }
        }

        /// <summary>
        /// <paramref name="type"/>をオプションの各型に変換します。
        /// </summary>
        private static UsiOptionType ParseType(string type)
        {
            var result = Util.FromUsiString(type);

            if (result == UsiOptionType.None)
            {
                throw new UsiException(
                    type + ": オプションの型が不明です。");
            }

            return result;
        }

        // --- 以下、static field

        /// <summary>
        /// USI_Hashを扱うオプションです。
        /// </summary>
        public static readonly UsiOption USI_Hash = Parse("option name USI_Hash type spin default 256");

        /// <summary>
        /// USI_Ponderを扱うオプションです。
        /// </summary>
        public static readonly UsiOption USI_Ponder = Parse("option name USI_Ponder type check default false");

    }


}
