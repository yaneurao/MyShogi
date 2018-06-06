namespace MyShogi.Model.Shogi.Usi
{
    /// <summary>
    /// UsiOptionの各パラメータを保存するために使います。
    /// nameとvalue(文字列)だけあれば設定としては十分なので、ファイルに設定を保存するときは、こちらを用いる。
    /// </summary>
    public sealed class UsiOptionMin
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiOptionMin()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiOptionMin(UsiOption option)
        {
            OptionType = option.OptionType;
            Name = option.Name;
            Value = option.GetDefault();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UsiOptionMin(UsiOptionType type, string name, string value)
        {
            OptionType = type;
            Name = name;
            Value = value;
        }

        /// <summary>
        /// オプションの型を取得または設定します。
        /// </summary>
        public UsiOptionType OptionType
        {
            get;
            private set;
        }

        /// <summary>
        /// 名前を取得または設定します。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 値を取得または設定します。
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// 文字列化します。(デバッグ用)
        /// </summary>
        public override string ToString()
        {
            return string.Format(
                "(Name={0}, Value={1})",
                Name, Value);
        }
    }
}
