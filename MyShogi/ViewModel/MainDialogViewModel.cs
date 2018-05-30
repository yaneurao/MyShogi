using System.Collections.Generic;
using System.Text;
using MyShogi.Controller;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Kifu;

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

            // 指し手生成祭りの局面
            Pos.SetSfen("l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w RGgsn5p 1");

            // 入玉局面
            // Pos.SetSfen("ln6+R/1+P2GKGBR/p1ppp+P+PP+P/1k7/1p7/9/PPPPP4/1B7/LNSG1GSNL b 2SNL3P 75");

#if false
            // psnの読み込み
            var manager = new KifuManager();
            var pos = new Position();
            manager.Bind(pos);

            var psn = System.IO.File.ReadAllText("kif/4.psn", Encoding.GetEncoding("Shift_JIS"));
            var error = manager.FromString(psn);

            Pos = pos;
#endif
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

        /// <summary>
        /// 対局者氏名。あとで書き直す。
        /// </summary>
        public string Player1Name { get; private set; } = "ワイ";
        public string Player2Name { get; private set; } = "あんた";

    }
}
