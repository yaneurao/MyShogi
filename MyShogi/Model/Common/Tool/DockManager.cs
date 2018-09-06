using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;
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
        /// このときLocationは無視される。
        /// </summary>
        FollowToMainWindow,

        /// <summary>
        /// MainWindowとは関係のない独立したウインドウ
        /// </summary>
        FloatingMode,
    }

    /// <summary>
    /// メインウインドウに対してDockさせる位置。
    /// </summary>
    public enum DockPosition
    {
        Top,
        Left,
        Bottom,
        Right,
    }

    /// <summary>
    /// 棋譜ウインドウや検討ウインドウのフローティングのための構造体
    /// </summary>
    public class DockManager : NotifyObject
    {
        /// <summary>
        /// State == DockedToMainWindowのときの本ウインドウのMainWindowからの相対位置
        /// </summary>
        [DataMember]
        public Point LocationOnDocked { get; set; }

        /// <summary>
        /// State == FloatingModeのときの本ウインドウの位置。
        /// Desktop上の絶対位置。
        /// </summary>
        [DataMember]
        public Point LocationOnFloating { get; set; }

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

        /// <summary>
        /// 現在のDockStateに基づき、dockWindowの位置とサイズを決定して反映させる。
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="dockWindow"></param>
        /// <param name="pos"></param>
        public void InitDockWindowLocation(Form mainWindow,Form dockWindow , DockPosition dockPos)
        {
            dockWindow.Size = this.Size;

            switch (this.DockState)
            {
                case DockState.FloatingMode:
                    dockWindow.Location = this.LocationOnFloating; /* これScreenがなくなってると画面外の可能性が.. */
                    break;

                case DockState.DockedToMainWindow:
                case DockState.FollowToMainWindow:
                    UpdateDockWindowLocation(mainWindow, dockWindow, dockPos);
                    break;
            }
        }

        /// <summary>
        /// メインウインドウの位置・サイズが変更になったときにそれに応じて、dockされているWindowの位置を移動させる。
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="dockWindow"></param>
        /// <param name="dockPos"></param>
        public void UpdateDockWindowLocation(Form mainWindow,Form dockWindow , DockPosition dockPos)
        {
            switch (this.DockState)
            {
                case DockState.DockedToMainWindow:
                    dockWindow.Location = new Point(
                        this.LocationOnDocked.X + mainWindow.Location.X,
                        this.LocationOnDocked.Y + mainWindow.Location.Y);
                    break;

                case DockState.FollowToMainWindow:
                    switch (dockPos)
                    {
                        case DockPosition.Top:
                            dockWindow.Location = new Point(mainWindow.Location.X, mainWindow.Location.Y - dockWindow.Height); break;
                        case DockPosition.Left:
                            dockWindow.Location = new Point(mainWindow.Location.X - dockWindow.Width, mainWindow.Location.Y); break;
                        case DockPosition.Bottom:
                            dockWindow.Location = new Point(mainWindow.Location.X, mainWindow.Location.Y + mainWindow.Height); break;
                        case DockPosition.Right:
                            dockWindow.Location = new Point(mainWindow.Location.X + mainWindow.Width, mainWindow.Location.Y); break;
                    }
                    break;
            }
        }

    }
}
