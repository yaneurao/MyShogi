using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局時間設定 先後の両方の分
    /// </summary>
    public class TimeSettings
    {
        public TimeSettings()
        {
            Players = new TimeSetting[2] { new TimeSetting(), new TimeSetting() };
            WhiteSame = true;
        }

        public TimeSettings(TimeSetting[] players , bool WhiteSame_)
        {
            Players = players;
            WhiteSame = WhiteSame_;
        }

        public TimeSettings Clone()
        {
            return new TimeSettings(
                new TimeSetting[2] { Players[0].Clone() , Players[1].Clone() },
                WhiteSame
                );
        }

        /// <summary>
        /// 対局時間設定、先後分
        /// </summary>
        public TimeSetting[] Players;

        /// <summary>
        /// c側の対局設定。
        /// ただし、WhiteSame == trueである時は、後手側の内容を無視して、先手側の対局に従うのでPlayers[0]のほうが返るので注意！
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public TimeSetting Player(Color c)
        {
            if (WhiteSame)
                c = Color.BLACK;

            return Players[(int)c];
        }

        /// <summary>
        /// 後手の対局時間設定も先手に従う
        /// </summary>
        public bool WhiteSame;

    }
}
