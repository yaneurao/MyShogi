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
        FollowToMainWindow,

        /// <summary>
        /// MainWindowの所定の相対位置に追随する。
        /// このときLocationは無視される。
        /// </summary>
        DockedToMainWindow,

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
        /// 表示/非表示の切り替え。非表示状態のときは、これをfalseにする。
        /// </summary>
        [DataMember]
        public bool Visible { get; set; } = true;

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
        /// DockState == DockedToMainWindowのときにどこにdockするか。
        /// </summary>
        [DataMember]
        public DockPosition DockPosition
        {
            get { return GetValue<DockPosition>("DockPosition"); }
            set { SetValue("DockPosition", value); }
        }

        /// <summary>
        /// DockStateとDockPositionとを一括して設定する。
        /// 変更イベントは一度しか発生しない。
        /// stateかpositionに変更があった場合は、DockStateの変更イベントが発生する。
        /// </summary>
        /// <param name="state"></param>
        /// <param name="position"></param>
        public void SetState(DockState state , DockPosition position)
        {
            var raise = DockState != state || DockPosition != position;

            PropertyChangedEventEnable = false;

            DockState = state;
            DockPosition = position;

            PropertyChangedEventEnable = true;
            if (raise)
                RaisePropertyChanged("DockState", state);
        }

        /// <summary>
        /// 現在のDockStateに基づき、dockWindowの位置とサイズを決定して反映させる。
        ///
        /// dockWindow.Show()の前に呼び出されるものとする。
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="dockWindow"></param>
        /// <param name="pos"></param>
        public void InitDockWindowLocation(Form mainWindow,Form dockWindow)
        {
            Point? location = null;
            switch (this.DockState)
            {
                case DockState.FloatingMode:
                    location = this.LocationOnFloating; /* これScreenがなくなってると画面外の可能性が.. */
                    break;

                case DockState.DockedToMainWindow:
                case DockState.FollowToMainWindow:
                    location = CalcDockWindowLocation(mainWindow);
                    break;
            }

            if (location == null)
                return;

            // Sizeに突っ込むと、その瞬間resizeイベントが発生して、そのときのlocationが記録されてしまう。
            dockWindow.Size = this.Size;
            dockWindow.Location = location.Value;
            dockWindow.StartPosition = FormStartPosition.Manual; // Show()の前に設定された内容が反映されるように。
        }

        /// <summary>
        /// メインウインドウの位置・サイズが変更になったときにそれに応じて、dockされているWindowの位置を移動させる。
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="dockWindow"></param>
        /// <param name="dockPos"></param>
        public void UpdateDockWindowLocation(Form mainWindow, Form dockWindow)
        {
            var location = CalcDockWindowLocation(mainWindow);
            if (location != null)
                dockWindow.Location = location.Value;
        }

        /// <summary>
        /// メインウインドウの位置・サイズが変更になったときにそれに応じて、dockされているWindowの位置を計算する。
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="dockWindow"></param>
        /// <param name="dockPos"></param>
        public Point? CalcDockWindowLocation(Form mainWindow)
        {
            switch (this.DockState)
            {
                case DockState.FollowToMainWindow:
                    return new Point(
                        this.LocationOnDocked.X + mainWindow.Location.X,
                        this.LocationOnDocked.Y + mainWindow.Location.Y);

                case DockState.DockedToMainWindow:
                    switch (DockPosition)
                    {
                        // dockWindowの値ではなく、このクラスが保持している値に基づいて計算しないといけない。
                        case DockPosition.Top:
                            return new Point(mainWindow.Location.X                           , mainWindow.Location.Y - /*dockWindow.*/ Size.Height);
                        case DockPosition.Left:
                            return new Point(mainWindow.Location.X - /*dockWindow*/Size.Width, mainWindow.Location.Y);
                        case DockPosition.Bottom:
                            return new Point(mainWindow.Location.X                           , mainWindow.Location.Y + mainWindow.Height);
                        case DockPosition.Right:
                            return new Point(mainWindow.Location.X + mainWindow.Width        , mainWindow.Location.Y);
                    }
                    break;
            }
            return null;
        }

        /// <summary>
        /// DockWindowの位置、サイズが変更になったのでそれを記録しておく。
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        public void SaveWindowLocation(Form mainWindow , Form dockWindow)
        {
            // 最小化、最大化時
            var minimized = mainWindow.WindowState != FormWindowState.Normal;
            if (minimized)
                return;

            var minimized2 = dockWindow.WindowState != FormWindowState.Normal; // 最小化、最大化時
            if (minimized2 || !dockWindow.Visible /* 非表示のときの値は用いない*/)
                return;

            Size = dockWindow.Size; // いずれにせよサイズはそのまま保存
            switch (this.DockState)
            {
                case DockState.FloatingMode      : LocationOnFloating = dockWindow.Location; break;
                case DockState.DockedToMainWindow: /* 記録の必要なし */ ; break;
                case DockState.FollowToMainWindow: LocationOnDocked = new Point(
                    dockWindow.Location.X - mainWindow.Location.X,
                    dockWindow.Location.Y - mainWindow.Location.Y
                    ); break;
            }
        }
    }
}
