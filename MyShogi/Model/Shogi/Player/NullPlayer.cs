namespace MyShogi.Model.Shogi.Player
{
    /// <summary>
    /// 何もしないプレイヤー
    /// </summary>
    public class NullPlayer : Player
    {
        public PlayerTypeEnum PlayerType
        {
            get { return PlayerTypeEnum.Null; }
        }

        public string Name
        {
            get { return "NullPlayer"; }
        }

        public string DisplayName
        {
            get { return Name; }
        }
    }
}
