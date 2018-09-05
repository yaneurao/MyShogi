using System.Drawing;
using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Common.Tool
{
    /// <summary>
    /// 棋譜ウインドウや検討ウインドウのフローティングであるかなどの状態
    /// </summary>
    public enum DockState
    {
        /// <summary>
        /// MainWindowにControlが収納されている状態
        /// </summary>
        InTheMainWindow,

        /// <summary>
        /// MainWindowと相対位置を保って移動される状態
        /// </summary>
        DockedToMainWindow,

        /// <summary>
        /// MainWindowの所定の相対位置に追随する。
        /// </summary>
        FollowToMainWindow,

        /// <summary>
        /// MainWindowとは関係のない独立したウインドウ
        /// </summary>
        FloatingMode,
    }

    /// <summary>
    /// 棋譜ウインドウや検討ウインドウのフローティングのための構造体
    /// </summary>
    public class DockManager : NotifyObject
    {
        /// <summary>
        /// ウインドウの位置
        ///
        /// Stateの状態に対して。
        ///   InTheMainWindow    : このときはこのプロパティは無視される。
        ///   DockedToMainWindow : MainWindowからの相対位置
        ///   FollowToMainWindow : MainWindowからの相対位置
        ///   FloatingMode       : MainWindowからの相対位置
        /// </summary>
        [DataMember]
        public Point Location { get; set; }

        /// <summary>
        /// ウインドウのサイズ
        ///
        /// Stateの状態に対して。
        ///   InTheMainWindow    : このときはこのプロパティは無視される。
        ///   DockedToMainWindow : ウインドウのサイズ
        ///   FollowToMainWindow : ウインドウのサイズ
        ///   FloatingMode       : ウインドウのサイズ
        /// </summary>
        [DataMember]
        public Size Size { get; set; }

        /// <summary>
        /// Dock状態
        /// </summary>
        [DataMember]
        public DockState DockState
        {
            get { return GetValue<DockState>("DockState"); }
            set { SetValue("DockState", value); }
        }

    }
}
