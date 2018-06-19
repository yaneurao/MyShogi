using System;
using System.Windows.Forms;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Player;

namespace MyShogi.Model.Shogi.LocalServer
{
    public partial class LocalGameServer
    {
        #region UI側からのコマンド

        /*
         * UI側からのコマンドは、 delegateで渡され、対局監視スレッド側で実行される。
         * delegateのなかのkifuManager.PositionやkifuManager.KifuListは、無名関数の束縛の性質から、
         * 現在のものであって過去のPositionやKifuListのへ参照ではない。
         * 
         * また、処理するのは対局監視スレッドであるから(対局監視スレッドはシングルスレッドでかつ、対局監視スレッド側でしか
         * Position.DoMove()は行わないので)、これが処理されるタイミングでは、kifuManager.Positionは最新のPositionであり、
         * これを調べているときに他のスレッドが勝手にPosition.DoMove()を行ったり、他のコマンドを受け付けたり、持ち時間切れに
         * なったりすることはない。
         */

        /// <summary>
        /// 対局スタート
        /// </summary>
        public void GameStartCommand(GameSetting gameSetting)
        {
            AddCommand(
            () =>
            {
                // いったんリセット
                GameEnd();
                GameStart(gameSetting);

                // エンジンの初期化が終わったタイミングで自動的にNotifyTurnChanged()が呼び出される。
            });
        }

        /// <summary>
        /// ユーザーから指し手が指されたときにUI側から呼び出す。
        /// 
        /// ユーザーがマウス操作によってmの指し手を入力した。
        /// ユーザーはこれを合法手だと思っているが、これが受理されるかどうかは別の話。
        /// (時間切れなどがあるので)
        /// </summary>
        /// <param name="m"></param>
        public void DoMoveCommand(Move m)
        {
            AddCommand(
            () =>
            {
                var stm = kifuManager.Position.sideToMove;
                var stmPlayer = Player(stm);

                // Human以外であれば受理しない。
                if (stmPlayer.PlayerType == PlayerTypeEnum.Human)
                {
                    // これを積んでおけばworker_threadのほうでいずれ処理される。(かも)
                    // 仮に、すでに次の局面になっていたとしても、次にこのユーザーの手番になったときに
                    // BestMove = Move.NONEとされるのでその時に破棄される。
                    stmPlayer.BestMove = m;
                }
            }
            );
        }

        /// <summary>
        /// エンジンに対して、いますぐに指させる。
        /// 受理されるかどうかは別。
        /// </summary>
        public void MoveNowCommand()
        {
            AddCommand(
            () =>
            {
                var stm = kifuManager.Position.sideToMove;
                var stmPlayer = Player(stm);

                // エンジン以外であれば受理しない。
                if (stmPlayer.PlayerType == PlayerTypeEnum.UsiEngine)
                {
                    var enginePlayer = stmPlayer as UsiEnginePlayer;
                    enginePlayer.MoveNow();
                }
            });
        }

        /// <summary>
        /// ユーザーによる対局中の2手戻し
        /// 受理できるかどうかは別
        /// </summary>
        public void UndoCommand()
        {
            AddCommand(
            () =>
            {
                var stm = kifuManager.Position.sideToMove;
                var stmPlayer = Player(stm);

                // 人間の手番でなければ受理しない
                if (stmPlayer.PlayerType == PlayerTypeEnum.Human)
                {
                    // 棋譜を消すUndo()
                    kifuManager.UndoMoveInTheGame();
                    kifuManager.UndoMoveInTheGame();

                    // 時刻を巻き戻さないといけない
                    PlayTimers.SetKifuMoveTimes(kifuManager.Tree.GetKifuMoveTime());

                    // これにより、2手目の局面などであれば1手しかundoできずに手番が変わりうるので手番の更新を通知。
                    NotifyTurnChanged();
                }
            });
        }

        /// <summary>
        /// UI側からの中断要求。
        /// </summary>
        public void GameInterruptCommand()
        {
            AddCommand(
            () =>
            {
                // コンピューター同士の対局中であっても人間判断で中断できなければならないので常に受理する。
                var stm = kifuManager.Position.sideToMove;
                var stmPlayer = Player(stm);

                // 中断の指し手
                stmPlayer.BestMove = Move.INTERRUPT;
            });
        }

        /// <summary>
        /// 棋譜の選択行が変更になった。
        /// 対局中でなければ、現在局面をその棋譜の局面に変更する。
        /// </summary>
        public void KifuSelectedIndexChangedCommand(int selectedIndex)
        {
            AddCommand(
            () =>
            {
                if (!InTheGame)
                {
                    // 現在の局面と違う行であるかを判定して、同じ行である場合は、
                    // このイベントを処理してはならない。

                    // 無理やりではあるが棋譜のN行目に移動出来るのであった…。
                    kifuManager.Tree.GotoSelectedIndex(selectedIndex);
                    PlayTimers.SetKifuMoveTimes(kifuManager.Tree.GetKifuMoveTime());

                }
            });
        }

        /// <summary>
        /// 棋譜の読み込みコマンド
        /// </summary>
        /// <param name="kifuText"></param>
        public void KifuReadCommand(string kifuText)
        {
            AddCommand(
            () =>
            {
                if (!InTheGame)
                {
                    var error = kifuManager.FromString(kifuText);
                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show("棋譜の読み込みに失敗しました。\n" + error, "読み込みエラー");

                        kifuManager.Init(); // 不正な局面のままになるとまずいので初期化。

                    } else
                    {
                        // 末尾の局面に..
                        //Console.WriteLine(kifuManager.Position.Pretty());
                    }
                }
            });
        }

        /// <summary>
        /// UI側から、worker threadで実行して欲しいコマンドを渡す。
        /// View-ViewModelアーキテクチャにおいてViewからViewModelにcommandを渡す感じ。
        /// ここで渡されたコマンドは、CheckUICommand()で吸い出されて実行される。
        /// </summary>
        /// <param name="command"></param>
        private void AddCommand(UICommand command)
        {
            lock (UICommandLock)
            {
                UICommands.Add(command);
            }
        }

        #endregion
    }
}

