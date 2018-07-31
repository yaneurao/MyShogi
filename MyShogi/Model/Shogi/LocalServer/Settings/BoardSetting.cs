using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局の開始盤面の設定
    /// </summary>
    [DataContract]
    public class BoardSetting : NotifyObject
    {
        // -- DataMembers

        /// <summary>
        /// 開始局面。
        /// BoardCurrentがtrueなら、この値は無視される。
        /// この値がCurrent,Othersは許容しない。
        /// </summary>
        [DataMember]
        public BoardType BoardType
        {
            get { return GetValue<BoardType>("BoardType"); }
            set { SetValue("BoardType", value); }
        }

        /// <summary>
        /// BoardTypeの局面から開始するのかのフラグ
        /// BoardTypeEnableかBoardCurrentのどちらかがtrueのはず。
        /// </summary>
        [DataMember]
        public bool BoardTypeEnable
        {
            get { return GetValue<bool>("BoardTypeEnable"); }
            set { SetValue("BoardTypeEnable", value); }
        }

        /// <summary>
        /// 現在の局面から開始するのかのフラグ
        /// </summary>
        [DataMember]
        public bool BoardTypeCurrent
        {
            get { return GetValue<bool>("BoardTypeCurrent"); }
            set { SetValue("BoardTypeCurrent", value); }
        }

        // -- public methods

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
    }
}
