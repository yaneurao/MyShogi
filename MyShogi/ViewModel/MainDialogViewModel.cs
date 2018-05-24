using System.Collections.Generic;
using MyShogi.Controller;

namespace MyShogi.ViewModel
{
    /// <summary>
    /// MainDialogを表示するときに必要な情報を保持しているクラス
    /// </summary>
    public class MainDialogViewModel
    {
        public MainDialogViewModel()
        {
        }

        /// <summary>
        /// 新しく対局controller(GameController)を、このクラスの管理下に加える。
        /// </summary>
        /// <param name="game"></param>
        public void Add(GameController game)
        {
            Games.Add(game);
            if (VisibleGames.Count == 0)
                VisibleGames.Add(game);
            if (ActiveGame == null)
                ActiveGame = game;
        }

        // 対局はN個、並列対局できるので、GameControllerのインスタンスをN個保持している
        public List<GameController> Games { get; private set; } = new List<GameController>();

        /// <summary>
        /// これを画面上に表示させるものとする。これは、Gamesのなかの、画面に表示させたいインスタンスが格納されている。
        /// VisibleGames.Length == 1のときは対局盤面は1つ。
        /// VisibleGames.Length >= 2のときは対局盤面が2つ以上。(工夫して表示させる)
        /// </summary>
        public List<GameController> VisibleGames { get; private set; } = new List<GameController>();

        /// <summary>
        /// 現在activeなゲーム。対局棋譜ウィンドゥは、singletonであることを想定しているので
        /// activeなゲームに関する情報しか表示できない。なので、どのGameがActiveであるかを選択できるようになっている。
        /// VisibleGamesのなかのいずれかのインスタンス。
        /// </summary>
        public GameController ActiveGame;

    }
}
