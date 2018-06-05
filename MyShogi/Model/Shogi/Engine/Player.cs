using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Shogi.Engine
{
    /// <summary>
    /// プレイヤーを抽象化したインターフェース
    /// 指し手を返す。
    /// </summary>
    public interface Player
    {
        // あとで考える
    }

    public class NullPlayer : Player
    {
    }

    /// <summary>
    /// プレイヤーのインスタンスの生成
    /// </summary>
    public static class PlayerBuilder
    {
        /// <summary>
        /// Playerの生成
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Player Create(PlayerType type)
        {
            switch(type)
            {
                case PlayerType.Null:
                    return new NullPlayer();

                case PlayerType.Human:
                    return new HumanPlayer();

                case PlayerType.UsiEngine:
                    return new UsiEnginePlayer();

            }
            return null;
        }
    }
}
