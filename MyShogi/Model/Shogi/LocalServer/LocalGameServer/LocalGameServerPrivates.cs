using System.Collections.Generic;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;
using MyShogi.Model.Shogi.Player;

namespace MyShogi.Model.Shogi.LocalServer
{
    public partial class LocalGameServer
    {
        #region private members

        /// <summary>
        /// 対局棋譜管理クラス
        /// </summary>
        private KifuManager kifuManager { get; set; } = new KifuManager();

        /// <summary>
        /// 対局しているプレイヤー
        /// 設定するときはSetPlayer()を用いるべし。
        /// 取得するときはPlayer()のほうを用いるべし。
        /// 
        /// 検討モードの時
        /// Players[0]は検討用のエンジン
        /// Players[1]は詰将棋用のエンジン
        /// </summary>
        private Player.Player[] Players = new Player.Player[2] { new NullPlayer(), new NullPlayer() };

        /// <summary>
        /// プレイヤーの消費時間管理クラス
        /// </summary>
        private PlayTimers PlayTimers = new PlayTimers();

        /// <summary>
        /// 残り時間を表現する文字列
        /// </summary>
        private string[] restTimeStrings = new string[2];
        private void SetRestTimeString(Color c, string time)
        {
            var changed = restTimeStrings[(int)c] != time;
            restTimeStrings[(int)c] = time;
            if (changed)
                RaisePropertyChanged("RestTimeChanged", c);
        }

        /// <summary>
        /// スレッドの終了フラグ。
        /// 終了するときにtrueになる。
        /// </summary>
        private bool workerStop = false;

        /// <summary>
        /// UIから渡されるコマンド
        /// </summary>
        private delegate void UICommand();
        private List<UICommand> UICommands = new List<UICommand>();
        // commandsのlock用
        private object UICommandLock = new object();

        /// <summary>
        /// 詰みの判定のために用いる指し手生成バッファ
        /// </summary>
        private Move[] moves = new Move[(int)Move.MAX_MOVES];

        #endregion
    }
}
