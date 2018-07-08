using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局の開始盤面の設定
    /// </summary>
    public class BoardSetting : NotifyObject
    {
        public BoardSetting()
        {
            BoardTypeEnable = true;
            BoardType = BoardType.NoHandicap;
            BoardTypeCurrent = false;
        }

        public BoardSetting Clone()
        {
            return (BoardSetting)MemberwiseClone();
        }

        /// <summary>
        /// 開始局面。
        /// BoardCurrentがtrueなら、この値は無視される。
        /// この値がCurrent,Othersは許容しない。
        /// </summary>
        public BoardType BoardType
        {
            get { return GetValue<BoardType>("BoardType"); }
            set { SetValue("BoardType", value); }
        }

        /// <summary>
        /// BoardTypeの局面から開始するのかのフラグ
        /// BoardTypeEnableかBoardCurrentのどちらかがtrueのはず。
        /// </summary>
        public bool BoardTypeEnable
        {
            get { return GetValue<bool>("BoardTypeEnable"); }
            set { SetValue("BoardTypeEnable", value); }
        }

        /// <summary>
        /// 現在の局面から開始するのかのフラグ
        /// </summary>
        public bool BoardTypeCurrent
        {
            get { return GetValue<bool>("BoardTypeCurrent"); }
            set { SetValue("BoardTypeCurrent", value); }
        }
    }
}
