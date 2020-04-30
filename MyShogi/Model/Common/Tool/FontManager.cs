using System.Drawing;
using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Common.Tool
{
    /// <summary>
    /// 置換すべきフォント名などを一元的に管理している。
    /// </summary>
    public class FontManager : NotifyObject
    {
        public FontManager()
        {
            // フォントを";"区切りで複数書けるようにして、先頭から調べて行って存在するフォントが
            // 選ばれるような仕組みのほうがいいのかも…。
            // →　しかしその仕組みでMac/Linux対応するのは筋が良くない気がするのでやめとく。

            SettingDialog       = new FontData(FontList.SettingDialog ,  9f);
            MenuStrip           = new FontData(FontList.MenuStrip     ,  9f);
            MainToolStrip       = new FontData(FontList.MainToolStrip , 13f);
            SubToolStrip        = new FontData(FontList.SubToolStrip  ,  9f);
            MessageDialog       = new FontData(FontList.MessageDialog , 11f);
            MainWindow          = new FontData(FontList.MainWindow    ,  9f);
            KifuWindow          = new FontData(FontList.KifuWindow    ,  7f);
            ConsiderationWindow = new FontData(FontList.ConsiderationWindow , 9f);
            MiniBoardTab        = new FontData(FontList.MiniBoardTab  , 9f);
            ToolTip             = new FontData(FontList.ToolTip       , 11f);
            DebugWindow         = new FontData(FontList.DebugWindow   ,  9f);
            EvalGraphControl    = new FontData(FontList.EvalGraphControl, 12f);
        }

        /// <summary>
        /// フォントの最大サイズ
        /// </summary>
        public static readonly int MAX_FONT_SIZE = 40;

        /// <summary>
        /// フォントの最小サイズ
        /// </summary>
        public static readonly int MIN_FONT_SIZE = 6;

        /// <summary>
        /// 対局設定・エンジン設定ダイアログなどのフォント
        /// </summary>
        [DataMember]
        public FontData SettingDialog
        {
            get { return GetValue<FontData>("SettingDialog"); }
            set { SetValue("SettingDialog", value); }
        }

        /// <summary>
        /// メニューのフォント
        /// </summary>
        [DataMember]
        public FontData MenuStrip
        {
            get { return GetValue<FontData>("MenuStrip"); }
            set { SetValue("MenuStrip", value); }
        }

        /// <summary>
        /// メインウインドウに配置しているToolStripのフォント
        /// </summary>
        [DataMember]
        public FontData MainToolStrip
        {
            get { return GetValue<FontData>("MainToolStrip"); }
            set { SetValue("MainToolStrip", value); }
        }

        /// <summary>
        /// ミニ盤面下に配置しているToolStripのフォント
        /// </summary>
        [DataMember]
        public FontData SubToolStrip
        {
            get { return GetValue<FontData>("SubToolStrip"); }
            set { SetValue("SubToolStrip", value); }
        }

        /// <summary>
        /// メッセージダイアログのフォント
        /// </summary>
        [DataMember]
        public FontData MessageDialog
        {
            get { return GetValue<FontData>("MessageDialog"); }
            set { SetValue("MessageDialog", value); }
        }

        /// <summary>
        /// メインウインドウ上のフォント(対局者名など)
        /// </summary>
        [DataMember]
        public FontData MainWindow
        {
            get { return GetValue<FontData>("MainWindow"); }
            set { SetValue("MainWindow", value); }
        }

        /// <summary>
        /// 棋譜ウインドウのフォント
        /// </summary>
        [DataMember]
        public FontData KifuWindow
        {
            get { return GetValue<FontData>("KifuWindow"); }
            set { SetValue("KifuWindow", value); }
        }

        /// <summary>
        /// 検討ウインドウのフォント
        /// </summary>
        [DataMember]
        public FontData ConsiderationWindow
        {
            get { return GetValue<FontData>("ConsiderationWindow"); }
            set { SetValue("ConsiderationWindow", value); }
        }

        /// <summary>
        /// ミニ盤面の上のタブの文字列
        /// </summary>
        [DataMember]
        public FontData MiniBoardTab
        {
            get { return GetValue<FontData>("MiniBoardTab"); }
            set { SetValue("MiniBoardTab", value); }
        }

        /// <summary>
        /// ToolTip
        /// </summary>
        [DataMember]
        public FontData ToolTip
        {
            get { return GetValue<FontData>("ToolTip"); }
            set { SetValue("ToolTip", value); }
        }

        /// <summary>
        /// デバッグウインドウ用
        /// </summary>
        [DataMember]
        public FontData DebugWindow
        {
            get { return GetValue<FontData>("DebugWindow"); }
            set { SetValue("DebugWindow", value); }
        }

        /// <summary>
        /// 形勢グラフコントロール用
        /// </summary>
        [DataMember]
        public FontData EvalGraphControl
        {
            get { return GetValue<FontData>("EvalGraphControl"); }
            set { SetValue("EvalGraphControl", value); }
        }

        /// <summary>
        /// 仮想イベント。フォントが変更になった時にこれが呼び出される。
        /// (ようになっているものとする) 変更になった場所 ("DebugWindow")などがargs.valueに入ってくるものとする。
        /// </summary>
        //public string FontChanged;

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
        /// 現在のFontName,FontSize,FontStyleでFontを生成する。
        /// </summary>
        /// <returns></returns>
        public Font CreateFont()
        {
            // 無理ぽ。
            if (FontSize <= 0)
                FontSize = 1;

            return new Font(FontName, FontSize, FontStyle);
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
