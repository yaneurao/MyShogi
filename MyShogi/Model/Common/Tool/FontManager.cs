using System.Drawing;
using System.Runtime.Serialization;

namespace MyShogi.Model.Common.Tool
{
    /// <summary>
    /// 置換すべきフォント名などを一元的に管理している。
    /// </summary>
    public class FontManager
    {
        public FontManager()
        {
            SettingDialog       = new FontData(FontList.SettingDialog , 11f);
            MenuStrip           = new FontData(FontList.MenuStrip     , 9f);
            MainToolStrip       = new FontData(FontList.MainToolStrip , 13f);
            SubToolStrip        = new FontData(FontList.SubToolStrip  , 13f);
            MessageDialog       = new FontData(FontList.MessageDialog , 11f);
            MainWindow          = new FontData(FontList.MainWindow    , 9f);
            KifuWindow          = new FontData(FontList.KifuWindow    , 9f);
            ConsiderationWindow = new FontData(FontList.ConsiderationWindow , 11f);
            DebugWindow         = new FontData(FontList.DebugWindow   , 9f);
        }

        /// <summary>
        /// 対局設定・エンジン設定ダイアログなどのフォント
        /// </summary>
        [DataMember]
        public FontData SettingDialog { get; set; }

        /// <summary>
        /// メニューのフォント
        /// </summary>
        [DataMember]
        public FontData MenuStrip { get; set; }

        /// <summary>
        /// メインウインドウに配置しているToolStripのフォント
        /// </summary>
        [DataMember]
        public FontData MainToolStrip { get; set; }

        /// <summary>
        /// ミニ盤面下に配置しているToolStripのフォント
        /// </summary>
        [DataMember]
        public FontData SubToolStrip { get; set; }

        /// <summary>
        /// メッセージダイアログのフォント
        /// </summary>
        [DataMember]
        public FontData MessageDialog { get; set; }

        /// <summary>
        /// メインウインドウ上のフォント(対局者名など)
        /// </summary>
        [DataMember]
        public FontData MainWindow { get; set; }

        /// <summary>
        /// 棋譜ウインドウのフォント
        /// </summary>
        [DataMember]
        public FontData KifuWindow { get; set; }

        /// <summary>
        /// 検討ウインドウのフォント
        /// </summary>
        [DataMember]
        public FontData ConsiderationWindow { get; set; }

        /// <summary>
        /// デバッグウインドウ用
        /// </summary>
        [DataMember]
        public FontData DebugWindow { get; set; }

    }

    /// <summary>
    /// ユーザーによって設定されているフォント情報
    ///
    /// FontStyleが.NET Framework側の型なのでDataContract属性を(つけたくないけど)つけてある。
    /// </summary>
    [DataContract]
    public class FontData
    {
        public FontData(string fontName , float fontSize , FontStyle fontStyle = FontStyle.Regular)
        {
            FontName = fontName;
            FontSize = fontSize;
            FontStyle = fontStyle;
        }

        /// <summary>
        /// フォント名(FontFamily)
        /// </summary>
        [DataMember]
        public string FontName { get; set; }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        [DataMember]
        public float FontSize { get; set; }

        /// <summary>
        /// フォントスタイル
        /// </summary>
        [DataMember]
        public FontStyle FontStyle { get; set; }
    }
}
