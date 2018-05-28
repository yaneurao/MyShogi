using System.Collections.Generic;
using MyShogi.Controller;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.ViewModel
{
    /// <summary>
    /// MainDialogを表示するときに必要な情報を保持しているクラス
    /// </summary>
    public class MainDialogViewModel
    {
        public MainDialogViewModel()
        {
            // デバッグ中。あとで削除する。
            // Pos.InitBoard(BoardType.NoHandicap);

            Pos.SetSfen("l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w RGgsn5p 1");
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

        /// <summary>
        /// 盤面。あとで書き直す。デバッグ用。
        /// </summary>
        public Position Pos { get; private set; } = new Position();

    }
}
