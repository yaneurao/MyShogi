using System;
using System.Runtime.Serialization;

namespace MyShogi.Model
{
    /// <summary>
    /// CSAプロトコルでやりとりするサーバーへの接続情報
    /// </summary>
    [Serializable]
    [DataContract]
    public sealed class CsaConnectData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CsaConnectData()
        {
            Host = "localhost";

            // サーバーのdefault porは4081らしい
            PortNumber = 4081;
        }

        /// <summary>
        /// クローンを作成
        /// </summary>
        public CsaConnectData Clone()
        {
            return (CsaConnectData)this.MemberwiseClone();
        }

        /// <summary>
        /// 接続先のサーバーアドレスを取得または設定します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// ポート番号の文字列を取得または設定します。
        /// PortNumberが型安全なので、そちらを使ってください。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public string Port
        {
            get;
            set;
        }

        /// <summary>
        /// ポート番号を数値で取得します。
        /// </summary>
        public int PortNumber
        {
            get
            {
                if (Port == null)
                    return -1;
                try
                {
                    // xmlファイルを読み込んだときにポート文字列が不正である可能性がある。
                    return int.Parse(Port);
                } catch
                {
                    return -1;
                }
            }
            
            set
            {
                Port = value.ToString();
                // Portに文字列として反映させておく。
            }
        }

        /// <summary>
        /// ログイン名を取得または設定します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public string LoginName
        {
            get;
            set;
        }

        /// <summary>
        /// パスワードを取得または設定します。
        /// </summary>
        [DataMember(Order = 4, IsRequired = true)]
        public string Password
        {
            get;
            set;
        }
    }

}
