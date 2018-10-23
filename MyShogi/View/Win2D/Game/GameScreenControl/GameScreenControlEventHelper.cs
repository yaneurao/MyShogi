using System;
using System.Drawing;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Tool;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.LocalServer;
using ShogiCore = MyShogi.Model.Shogi.Core;
using SColor = MyShogi.Model.Shogi.Core.Color;
using SPRITE = MyShogi.Model.Resource.Images.SpriteManager;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// GameScreenに関するイベントハンドラ
    /// マウスがクリックされた時の処理など
    /// </summary>
    public partial class GameScreenControl
    {
        /// <summary>
        /// 初期化する。
        /// このとき、イベントハンドラを設定する。
        /// </summary>
        public void Init()
        {
            SetEventHandlers();
            handler_set = true;
        }

        /// <summary>
        /// InitでSetEventHandlersを呼び出したか。
        /// (呼び出していればDispose()で解除する)
        /// </summary>
        private bool handler_set;

        public new void Dispose()
        {
            if (handler_set)
            {
                RemoveEventHandlers();
                handler_set = false;
            }
        }

        /// <summary>
        /// NotifyObjectのイベントハンドラを設定
        /// </summary>
        public void SetEventHandlers()
        {
            // イベントハンドラを設定する。
            gameServer.AddPropertyChangedHandler("Position", PositionChanged);
            gameServer.AddPropertyChangedHandler("TurnChanged", TurnChanged, Parent);
            gameServer.AddPropertyChangedHandler("InTheGame", InTheGameChanged, Parent);
            gameServer.AddPropertyChangedHandler("GameMode", UpdateMenuItems , Parent);
            gameServer.AddPropertyChangedHandler("EngineInitializing", EngineInitializingChanged, Parent);
            gameServer.AddPropertyChangedHandler("RestTimeChanged", RestTimeChanged);
            gameServer.AddPropertyChangedHandler("InTheBoardEdit", InTheBoardEditChanged, Parent);
            gameServer.AddPropertyChangedHandler("BoardReverse", UpdateMenuItems, Parent);

            // このメソッドはUIスレッドから呼び出してはならない。
            // これは、queuingしてUIに反映させないと、連続対局のときに更新が間に合わないからである。
            gameServer.AddPropertyChangedHandler("ThinkReport", thinkReportChanged /*, Parent */ );

            // data-bind
            gameServer.Bind("KifuList", kifuControl1.ViewModel, DataBindWay.OneWay);
            gameServer.Bind("KifuListAdded", kifuControl1.ViewModel, DataBindWay.OneWay);
            gameServer.Bind("KifuListRemoved", kifuControl1.ViewModel, DataBindWay.OneWay);
            gameServer.Bind("KifuListSelectedIndex", kifuControl1.ViewModel , DataBindWay.TwoWay );

            // initialize kifu control
            kifuControl1.InitViewModel(Parent as Form);

            kifuControl1.ViewModel.AddPropertyChangedHandler("MainBranchButtonClicked" , gameServer.MainBranchButtonCommand);
            kifuControl1.ViewModel.AddPropertyChangedHandler("NextBranchButtonClicked" , gameServer.NextBranchButtonCommand);
            kifuControl1.ViewModel.AddPropertyChangedHandler("EraseBranchButtonClicked", gameServer.EraseBranchButtonCommand);
            kifuControl1.ViewModel.AddPropertyChangedHandler("RemoveLastMoveClicked"   , gameServer.RemoveLastMoveButtonCommand);

            // Game Effects

            gameServer.AddPropertyChangedHandler("GameStartEvent", GameStartEventHandler , Parent);
            gameServer.AddPropertyChangedHandler("GameEndEvent"  , GameEndEventHandler , Parent);
        }

        /// <summary>
        /// NotifyObjectのイベントハンドラを解除
        /// </summary>
        public void RemoveEventHandlers()
        {
            gameServer.RemovePropertyChangedHandler("Position", PositionChanged);
            gameServer.RemovePropertyChangedHandler("TurnChanged", TurnChanged);
            gameServer.RemovePropertyChangedHandler("InTheGame", InTheGameChanged);
            gameServer.RemovePropertyChangedHandler("GameMode", UpdateMenuItems);
            gameServer.RemovePropertyChangedHandler("EngineInitializing", EngineInitializingChanged);
            gameServer.RemovePropertyChangedHandler("RestTimeChanged", RestTimeChanged);
            gameServer.RemovePropertyChangedHandler("SetKifuListIndex", SetKifuListIndex);
            gameServer.RemovePropertyChangedHandler("InTheBoardEdit", InTheBoardEditChanged);
            gameServer.RemovePropertyChangedHandler("BoardReverse", UpdateMenuItems);
            gameServer.RemovePropertyChangedHandler("ThinkReport", thinkReportChanged);

            // data-bind
            gameServer.Unbind("KifuList", kifuControl1.ViewModel);
            gameServer.Unbind("KifuListAdded", kifuControl1.ViewModel);
            gameServer.Unbind("KifuListRemoved", kifuControl1.ViewModel);
            gameServer.Unbind("KifuListSelectedIndex", kifuControl1.ViewModel);

            kifuControl1.ViewModel.RemovePropertyChangedHandler("MainBranchButtonClicked", gameServer.MainBranchButtonCommand);
            kifuControl1.ViewModel.RemovePropertyChangedHandler("NextBranchButtonClicked", gameServer.NextBranchButtonCommand);
            kifuControl1.ViewModel.RemovePropertyChangedHandler("EraseBranchButtonClicked", gameServer.EraseBranchButtonCommand);
        }

        /// <summary>
        /// Settingで渡されたSetButtonのハンドラを呼び出す。
        /// </summary>
        public void SetButton(MainDialogToolStripButtonEnum name, bool enable)
        {
            if (Setting != null && Setting.SetButton != null)
                Setting.SetButton(name, enable);
        }

        /// <summary>
        /// [UI Thread] : このGameScreenControlに紐づけられているメニューの更新を行う。
        /// </summary>
        public void UpdateMenuItems(PropertyChangedEventArgs args)
        {
            if (Setting != null && Setting.UpdateMenuItems != null)
                Setting.UpdateMenuItems(args);
            else
                Dirty = true; // 描画の更新イベントは呼び出しておかないといけない。
        }

        /// <summary>
        /// [UI Thread] : GameServer.InTheBoardEditの変更ハンドラ
        /// </summary>
        /// <param name="args"></param>
        public void InTheBoardEditChanged(PropertyChangedEventArgs args = null)
        {
            UpdateKifuControlVisibility();
        }

        /// <summary>
        /// [UI thread] : 棋譜ウィンドウの可視/不可視を設定するハンドラ
        /// </summary>
        public void UpdateKifuControlVisibility(PropertyChangedEventArgs args = null)
        {
            if (kifuControl != null)
            {
                // メインウインドウの埋め込まれているときしかkifuControlのvisiblityを操作しない。
                if (TheApp.app.Config.KifuWindowDockManager.DockState != DockState.InTheMainWindow)
                    return;

                kifuControl.Visible =
                    gameServer != null
                    && !gameServer.InTheBoardEdit /*盤面編集中は非表示*/
                    && PieceTableVersion == 0 /* 通常の駒台でなければ(細長い駒台の時は)非表示 */
                ;
            }
        }


        /// <summary>
        /// 盤面情報が更新された時に呼び出されるハンドラ。
        /// </summary>
        public void PositionChanged(PropertyChangedEventArgs args)
        {
            Dirty = true;
        }

        /// <summary>
        /// [UI thread] : 手番が変わったので何はともあれ、いま掴んでいる駒はいったん離す
        /// (逆の手番側の駒を掴んだままになるとおかしいので)
        /// </summary>
        /// <param name="args"></param>
        public void TurnChanged(PropertyChangedEventArgs args)
        {
            StateReset();

            // TooltipにあるButtonの状態更新
            UpdateTooltipButtons();
        }

        /// <summary>
        /// [UI thread] : InTheGameの値が変わった時のハンドラ
        /// </summary>
        /// <param name="args"></param>
        public void InTheGameChanged(PropertyChangedEventArgs args)
        {
            TurnChanged(args);
            kifuControl.ViewModel.SetValueAndRaisePropertyChanged("InTheGame", args.value); // 棋譜ボタンが変化するかもなので

            // Tooltipの◁▷本譜ボタンの状態更新
            UpdateTooltipButtons2();
        }

        /// <summary>
        /// 残り時間が変更になった時に呼び出されるハンドラ
        /// 最小更新矩形を設定して更新すべき。
        /// </summary>
        /// <param name="args"></param>
        public void RestTimeChanged(PropertyChangedEventArgs args)
        {
            //ViewModel.dirtyRestTime = true;
            // あとでちゃんと書き直す。

            Dirty = true;
        }

        /// <summary>
        /// [UI thread] : 棋譜の読み込み時など、LocalServer側の要請により、棋譜ウィンドウを指定行に
        /// フォーカスを当てるためのハンドラ
        /// </summary>
        /// <param name="args"></param>
        public void SetKifuListIndex(PropertyChangedEventArgs args)
        {
            var selectedIndex = (int)args.value;
            kifuControl1.ViewModel.KifuListSelectedIndex = selectedIndex;
        }

        /// <summary>
        /// [UI thread] : メニューのすぐ下に配置しているtooltip buttonを、現在の状態に応じてOn/Offする。
        /// </summary>
        private void UpdateTooltipButtons()
        {
            var inTheGame = gameServer == null ? false : gameServer.InTheGame;

            // この時、エンジン側の手番であるなら、メインウインドウのメニューの「急」ボタンをenableにしなければならない。
            var engineTurn = gameServer.EngineTurn && inTheGame;
            SetButton(MainDialogToolStripButtonEnum.MOVE_NOW, engineTurn);

            // この時、対局中でかつ、人間側の手番で、エンジン初期化中でなければ、
            // メインウインドウのメニューの「投」「待」ボタンをenableにしなければならない。
            var humanTurn = gameServer.InTheGame && gameServer.CanUserMove && !gameServer.EngineInitializing;
            SetButton(MainDialogToolStripButtonEnum.RESIGN, humanTurn);
            SetButton(MainDialogToolStripButtonEnum.UNDO_MOVE, humanTurn);

            // 「中」ボタンは、エンジン同士の対局時にも中断できるようにするため、対局中であればいつでも中断できる。
            var canInterrupt = !gameServer.EngineInitializing && gameServer.InTheGame;
            SetButton(MainDialogToolStripButtonEnum.INTERRUPT, canInterrupt);
        }

        /// <summary>
        /// Tooltipの◁▷本譜ボタンの状態更新
        /// </summary>
        private void UpdateTooltipButtons2()
        {
            var consideration = gameServer.GameMode.IsConsideration();
            SetButton(MainDialogToolStripButtonEnum.REWIND, consideration);
            SetButton(MainDialogToolStripButtonEnum.FORWARD, consideration);
            SetButton(MainDialogToolStripButtonEnum.MAIN_BRANCH, consideration);
        }

        /// <summary>
        /// [UI thread] : エンジン初期化中の状態が変更になった時に呼び出されるハンドラ。
        /// エンジン初期化中のダイアログを描画している/していないはずなので、それを新しい状態に応じて再描画する必要がある。
        /// </summary>
        /// <param name="args"></param>
        public void EngineInitializingChanged(PropertyChangedEventArgs args)
        {
            UpdateTooltipButtons();
            Dirty = true;
        }

        /// <summary>
        /// LocalGameServerのThinkReportのプロパティが変更になった時に呼び出されるハンドラ。
        ///
        /// 注意) これを呼び出すスレッドはUI Threadではない。
        /// これは、queuingしてUIに反映させないと、連続対局のときに更新が間に合わないからである。
        /// </summary>
        /// <param name="args"></param>
        private void thinkReportChanged(PropertyChangedEventArgs args)
        {
            //　移譲しておく。
            ThinkReportChanged?.Invoke(args);
        }

        // 持ち時間が減っていくときに、持ち時間の部分だけの再描画をしたいのでそのためのフラグ
        //public bool DirtyRestTime
        //{
        //    get { return ViewModel.dirtyRestTime; }
        //}

        /// <summary>
        /// [UI thread] : Formのリサイズに応じて棋譜コントロールの移動などを行う。
        /// </summary>
        public void ResizeKifuControl()
        {
            var kifu = kifuControl;
            if (kifu.ViewModel.DockState == DockState.InTheMainWindow)
            {
                using (var slb = new SuspendLayoutBlock(kifu))
                {
                    // (起動時) 先にLocationを移動させておかないとSizeの変更で変なところに表示されて見苦しい。
                    // ※　理由はよくわからない。DockStyle.Noneにした影響か何か。
                    kifu.Size = Size.Empty; // resizeイベントが生起するようにいったん(0,0)にする。
                    kifu.Location = CalcKifuWindowLocation();
                    kifu.Size = CalcKifuWindowSize();
                }
            }

            // 駒台が縦長のモードのときは、このコントロールは非表示にする。
            // (何か別の方法で描画する)
            UpdateKifuControlVisibility();
        }

        /// <summary>
        /// 棋譜ウインドウの表示サイズを計算して返す。
        /// </summary>
        /// <returns></returns>
        public Size CalcKifuWindowSize()
        {
            // 棋譜ウィンドウの横幅の倍率
            float w_rate = TheApp.app.Config.KifuWindowWidthType * 0.25f;
            // 棋譜ウィンドウを横にどれだけ延ばすのか
            int w_offset = (int)(w_rate * 265);

            var size = new Size(265 + w_offset, 423);
            return AffineScale(size);
        }

        /// <summary>
        /// 棋譜ウインドウの位置を計算して返す。
        /// </summary>
        /// <returns></returns>
        public Point CalcKifuWindowLocation()
        {
            // 棋譜ウィンドウの横幅の倍率
            float w_rate = TheApp.app.Config.KifuWindowWidthType * 0.25f;
            // 棋譜ウィンドウを横にどれだけ延ばすのか
            int w_offset = (int)(w_rate * 265);

            var point = new Point(229 - w_offset, 600);
            return Affine(point);
        }

        /// <summary>
        /// 現在のこのControlのWidth,Heightに対して、いい感じに描画できるようにaffine行列を設定する。
        /// </summary>
        public void FitToClientSize()
        {
            // ちらつかずにウインドウのaspect ratioを保つのは.NET Frameworkの範疇では不可能。
            // ClientSizeをResizeイベント中に変更するのは安全ではない。
            // cf.
            //   https://qiita.com/yu_ka1984/items/b4a3ce9ed7750bd67b86
            // →　あきらめる

#if false
            // このコード、有効にするとハングする。
            double ratio = (double)h / w;
            if (ratio < 0.563)
            {
                w = (int)(h / 0.563);
                ClientSize = new Size(w, h + menu_height);
            }
            else if (ratio > 0.726)
            {
                w = (int)(h / 0.726);
                ClientSize = new Size(w, h + menu_height);
            }
#endif

            var scale = Math.Min((float)Height / board_img_size.Height, (float)Width / board_vert_size.Width);
            AffineMatrix.SetMatrix(scale, scale, (int)(Width - board_img_size.Width * scale) / 2, 0 /* Top (Formの場合)*/);

            //  縦長の画面なら駒台を縦長にする。

            double ratio = (double)Width / Height;
            //Console.WriteLine(ratio);

            PieceTableVersion = (ratio < 1.36) ? 1 : 0;
        }

        /// <summary>
        /// 強制的に再描画する。
        /// </summary>
        public void ForceRedraw()
        {
            Dirty = true;
            Invalidate();
        }

        /// <summary>
        /// リサイズイベントと等価な処理。
        /// </summary>
        public void ForceRedraw2()
        {
            GameScreenControl_SizeChanged(null,null);
        }

        /// <summary>
        /// sqの描画する場所を得る。
        /// reverse : 盤面を180度回転するのかのフラグ
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        private Point PieceLocation(SquareHand sq , bool reverse)
        {
            Point dest;

            if (sq.IsBoardPiece())
            {
                // -- 盤上の升

                Square sq2 = reverse ? ((Square)sq).Inv() : (Square)sq;
                int f = 8 - (int)sq2.ToFile();
                int r = (int)sq2.ToRank();

                dest = new Point(board_location.X + piece_img_size.Width * f, board_location.Y + piece_img_size.Height * r);
            }
            else if (sq.IsHandPiece())
            {
                // -- 手駒

                var color = sq.PieceColor();
                if (reverse)
                    color = color.Not();

                var v = PieceTableVersion;

                var pc = sq.ToPiece();

                if (color == ShogiCore.Color.BLACK)
                    // Point型同士の加算は定義されていないので、第二項をSize型にcastしている。
                    dest = hand_table_pos[v, (int)color] + (Size)hand_piece_pos[v, (int)pc - 1];
                else
                    // 180度回転させた位置を求める
                    // 後手も駒の枚数は右肩に描画するのでそれに合わせて左右のmarginを調整する。
                    dest = new Point(
                        hand_table_pos[v, (int)color].X + (hand_table_size[v].Width - hand_piece_pos[v, (int)pc - 1].X - piece_img_size.Width - 10),
                        hand_table_pos[v, (int)color].Y + (hand_table_size[v].Height - hand_piece_pos[v, (int)pc - 1].Y - piece_img_size.Height + 0)
                    );
            }
            else
            {
                // -- 駒箱の駒

                // 並び替える
                // pc : 歩=0,香=1,桂=2,銀=3,金=4,角=5,飛=6,玉=7にする。
                var pc = (int)sq.ToPiece() - 1;
                if (pc == 6)
                    pc = 4;
                else if (pc == 4 || pc == 5)
                    ++pc;

                if (PieceTableVersion == 0)
                {
                    // 駒箱の1段目に3枚、2段目に2枚、3段目に3枚表示する。

                    // 5を欠番にして2段目を2枚にする。
                    if (pc >= 5)
                        ++pc;

                    int file = pc % 3;
                    int rank = pc / 3;

                    int x = (int)(file * piece_img_size.Width * .8);
                    int y = (int)(rank * piece_img_size.Height * .88);

                    if (rank == 1)
                        x += (int)(piece_img_size.Width / 2 * 0.8);

                    dest = new Point(
                        hand_box_pos[0].X + x,
                        hand_box_pos[0].Y + y
                        );
                }
                else
                {
                    int file = pc % 2;
                    int rank = pc / 2;

                    int x = (int)(file * piece_img_size.Width * .5);
                    int y = (int)(rank * piece_img_size.Height * .65);

                    dest = new Point(
                        hand_box_pos[1].X + x,
                        hand_box_pos[1].Y + y
                        );
                }
            }

            return dest;
        }

        /// <summary>
        /// c側の駒台の領域を返す。
        ///
        /// reverse : 盤面を180度回転するのかのフラグ
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private Rectangle HandTableRectangle(ShogiCore.Color c , bool reverse)
        {
            if (reverse)
                c = c.Not();

            var v = PieceTableVersion;
            return new Rectangle(hand_table_pos[v, (int)c], hand_table_size[v]);
        }


        /// <summary>
        /// 駒箱の領域を返す。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private Rectangle PieceBoxRectangle()
        {
            var v = PieceTableVersion;
            return new Rectangle(piece_box_pos[v], piece_box_size[v]);
        }

        /// <summary>
        /// 指し手生成用のバッファ
        /// UIスレッドからしか使わない。マウスクリックのときの合法手を表示するために使う。
        /// </summary>
        private Move[] moves_buf { get; } = new Move[(int)ShogiCore.Move.MAX_MOVES];

        /// <summary>
        /// sqの駒を掴む
        /// sqの駒が自駒であることは確定している。
        /// 行き先の候補の升情報を更新する。
        /// </summary>
        /// <param name="sq"></param>
        public void pick_up(SquareHand sq)
        {
            if (!(gameServer.CanUserMove && !gameServer.EngineInitializing))
                return;

            var pos = gameServer.Position;

            // この駒をユーザーが掴んで動かそうとしていることを示す
            viewState.picked_from = sq;
            viewState.picked_to = SquareHand.NB;
            viewState.state = GameScreenControlViewStateEnum.PiecePickedUp;

            // デバッグ用に出力する。
            //Console.WriteLine("pick up : " + sq.Pretty() );

            // 簡単に利きを表示する。
            // ここで連続王手による千日手などを除外すると
            // 「ユーザーが駒が動かせない、バグだ」をみたいなことを言い出しかねない。
            // 移動後に連続王手の千日手を回避していないという警告を出すようにしなくてはならない。

            // 合法手を生成して、そこに含まれるものだけにする。
            // この生成、局面が変わったときに1回でいいような気はするが..
            // 何回もクリックしまくらないはずなのでまあいいや。

            int n = MoveGen.LegalAll(pos, moves_buf, 0);

            var is_drop = sq.IsHandPiece();
            var pt = pos.PieceOn(sq).PieceType();
            Bitboard bb = Bitboard.ZeroBB();

            // 生成されたすべての合法手に対して移動元の升が合致する指し手の移動先の升を
            // Bitboardに反映させていく。
            for (int i = 0; i < n; ++i)
            {
                var m = moves_buf[i];
                if (is_drop)
                {
                    // 駒の打てる先。これは合法手でなければならない。
                    // 二歩とか打ち歩詰めなどがあるので合法手のみにしておく。
                    // (打ち歩詰めなので打てませんの警告ダイアログ、用意してないので…)

                    // 合法手には自分の手番の駒しか含まれないのでこれでうまくいくはず
                    if (m.IsDrop() && m.DroppedPiece() == pt)
                        bb |= m.To();
                }
                else
                {
                    // 駒の移動できる先
                    if (!m.IsDrop() && m.From() == (Square)sq)
                        bb |= m.To();
                }
            }

            viewState.picked_piece_legalmovesto = bb;
            viewState.state = GameScreenControlViewStateEnum.PiecePickedUp;

            // この値が変わったことで画面の状態が変わるので、次回、OnDraw()が呼び出されなくてはならない。
            Dirty = true;

        }

        /// <summary>
        /// sqの駒を掴む(盤面編集用に)
        /// </summary>
        /// <param name="sq"></param>
        public void pick_up_for_edit(SquareHand sq)
        {
            var pos = gameServer.Position;

            // この駒をユーザーが掴んで動かそうとしていることを示す
            viewState.picked_from = sq;
            viewState.picked_to = SquareHand.NB;
            viewState.state = GameScreenControlViewStateEnum.PiecePickedUp;

            // 生成されたすべての合法手に対して移動元の升が合致する指し手の移動先の升を
            viewState.picked_piece_legalmovesto = Bitboard.ZeroBB();
            viewState.state = GameScreenControlViewStateEnum.PiecePickedUp;

            // この値が変わったことで画面の状態が変わるので、次回、OnDraw()が呼び出されなくてはならない。
            Dirty = true;
        }

        /// <summary>
        /// 駒の移動
        /// ただし成り・不成が選べるときはここでそれを尋ねるダイアログが出る。
        /// また、連続王手の千日手局面に突入するときもダイアログが出る。
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void move_piece(SquareHand from, SquareHand to)
        {
            var state = viewState;
            var reverse = gameServer.BoardReverse;

            // デバッグ用に表示する。
            //Console.WriteLine(from.Pretty() + "→" + to.Pretty());

            // この成る手を生成して、それが合法手であるなら、成り・不成のダイアログを出す必要がある。
            // また、1段目に進む「歩」などは、不成はまずいので選択がいらない。

            // Promoteの判定
            var pos = gameServer.Position;
            var pro_move = Util.MakeMove(from, to, true);
            // 成りの指し手が合法であるかどうか
            var canPromote = pos.IsLegal(pro_move);

            // 不成の指し手が合法であるかどうか
            var unpro_move = Util.MakeMove(from, to, false);
            var canUnpro = pos.IsLegal(unpro_move);

            // canUnproとcanPromoteの組み合わせは4通り。
            // 1. (false, false)
            // 2. (false,  true)
            // 3. (true , false)
            // 4. (true ,  true)

            // 上記 1.と3.
            if (!canPromote)
            {
                surpressDraw = true; StateReset();

                // 成れないので成る選択肢は消して良い。
                DoMoveCommand(unpro_move);
            }
            // 上記 2.
            else if (!canUnpro && canPromote)
            {
                surpressDraw = true; StateReset();

                // 成るしか出来ないので、不成は選択肢から消して良い。
                // 成れないので成る選択肢は消して良い。
                DoMoveCommand(pro_move);
            }
            // 上記 4.
            // これで、上記の1.～4.のすべての状態を網羅したことになる。
            else // if (canPromote && canUnPro)
            {
                // 成り・不成の選択ダイアログを出す

                state.state = GameScreenControlViewStateEnum.PromoteDialog;
                state.moved_piece_type = pos.PieceOn(from).PieceType();
                state.moved_piece_color = pos.PieceOn(from).PieceColor();

                // この状態を初期状態にするのは少しおかしいが、どうせこのあとマウスを動かすであろうからいいや。
                state.promote_dialog_selection = PromoteDialogSelectionEnum.NO_SELECT;

                // toの升以外は暗くする。
                state.picked_piece_legalmovesto = new Bitboard((Square)to);

                Dirty = true;
            }
        }

        /// <summary>
        /// 移動先の升を与えて、成り・不成のダイアログを表示する位置を計算する。
        /// </summary>
        /// <param name="movedPieceColor"></param>
        /// <param name="to"></param>
        /// <param name="reverse"></param>
        /// <param name="flip">反転させて表示するのかのフラグ。</param>
        /// <returns></returns>
        public Point CalcPromoteDialogLocation(GameScreenControlViewState state, bool reverse , out bool flip)
        {
            // toの近くに成り・不成のダイアログを描画してやる。
            // これは移動先の升の少し下に出す。
            // ただし、1段目であると画面外に行ってしまうので
            // 1,2段目であれば上に変位させる必要がある。
            // あと、棋譜ウィンドウもこの手前に描画できないので
            // ここも避ける必要がある。

            var dest = PieceLocation(state.picked_to , reverse);

            // このダイアログを反転表示するのか
            var config = TheApp.app.Config;
            flip = config.FlipWhitePromoteDialog == 1 &&
                state.moved_piece_color == (!reverse ? SColor.WHITE : SColor.BLACK);
            if (flip)
            {
                // 移動先の升より少し上の座標に
                if (dest.Y <= board_img_size.Height * 0.2) // 画面の上らへんである..
                    dest += new Size(-130 + 95 / 2, +100);
                else
                    dest += new Size(-103 + 95 / 2, -200);
            }
            else
            {
                // 移動先の升より少し下の座標に
                if (dest.Y >= board_img_size.Height * 0.8) // 画面の下らへんである..
                    dest += new Size(-130 + 95 / 2, -200);
                else
                    dest += new Size(-103 + 95 / 2, +100);
            }

            if (dest.X < board_location.X)
                dest += new Size(150, 0);

            // flip表示のときは起点が異なるのでこのダイアログサイズ分だけずらして描画する必要がある。
            if (flip)
                dest += promote_dialog_size;

            return dest;
        }

        /// <summary>
        /// 駒の移動 盤面編集
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void move_piece_for_edit(SquareHand from, SquareHand to)
        {
            var pos = gameServer.Position;

            // 何にせよ、移動、もしくは交換をする。
            // →　交換をする実装、不評なので、将棋所・ShogiGUIに倣い、手駒に移動させる。

            var from_pc = pos.PieceOn(from);
            var from_pt = from_pc.PieceType();
            var from_pr = from_pc.RawPieceType();
            var to_pc = pos.PieceOn(to);
            var to_pr = to_pc.RawPieceType();

            // 移動元と移動先が同じなら何もする必要はない。
            if (from == to)
            {
                // このケースを除外しておかないと、toの駒を手駒に移動させる処理などで
                // from == toだと手駒が増えることになる。

                // しかし、駒を掴んでいる状態が変化するのでこのときだけ再描画は必要。
                Dirty = true;
            }
            else if (to.IsBoardPiece())
            {
                if (from.IsBoardPiece())
                {
                    // -- 盤上と盤上の駒

#if false // 2駒交換する実装 →　不評なので手駒に移動させることに変更。
                    // fromとtoの升の駒を交換する。
                    // fromに駒があることは確定している。toに駒があろうがなかろうが良い。
                    BoardEditCommand(raw =>
                    {
                        raw.board[(int)from] = to_pc;
                        raw.board[(int)to] = from_pc;
                    });
#endif

#if true // toのほうが手駒に移動する実装
                    BoardEditCommand(raw =>
                    {
                        raw.board[(int)from] = Piece.NO_PIECE;
                        raw.board[(int)to] = from_pc;
                        if (to_pr != Piece.NO_PIECE && to_pr != Piece.KING)
                        {
                            // 移動元の駒のcolorの手駒に移動させる。玉は、(欠落するので)勝手に駒箱に移動する。
                            raw.hands[(int)from_pc.PieceColor()].Add(to_pr);
                        }
                    });
#endif

                }
                else if (from.IsHandPiece())
                {
                    // -- 手駒を盤面に

                    BoardEditCommand(raw =>
                    {
#if false // 駒箱に移動する実装　→　不評なので手駒になるように変更
                        raw.hands[(int)from.PieceColor()].Sub(from_pt);
                        raw.board[(int)to] = from_pc;
                        // このtoの位置にもし駒があったとしたら、それは駒箱に移動する。
                        // (その駒が欠落するので..)
#endif

#if true // toのほうが手駒に移動する実装
                        raw.hands[(int)from.PieceColor()].Sub(from_pt);
                        raw.board[(int)to] = from_pc;

                        if (to_pr != Piece.NO_PIECE && to_pr != Piece.KING)
                        {
                            // 移動元の駒のcolorの手駒に移動させる。玉は、(欠落するので)勝手に駒箱に移動する。
                            raw.hands[(int)from.PieceColor()].Add(to_pr);
                        }
#endif

                    });
                }
                else if (from.IsPieceBoxPiece())
                {
                    // -- 駒箱の駒を盤面に
                    // toにあった駒は駒箱に戻ってくるが、これは仕方がない。

                    BoardEditCommand(raw => raw.board[(int)to] = from_pc);
                }
            }
            else if (to.IsHandPiece())
            {
                if (from.IsBoardPiece())
                {
                    // -- 盤上の駒を手駒に移動させる。

                    // 手駒に出来る駒種でなければキャンセル
                    if (from_pt == Piece.KING)
                        return;

                    BoardEditCommand(raw =>
                    {
                        raw.board[(int)from] = Piece.NO_PIECE;
                        raw.hands[(int)to.PieceColor()].Add(from_pr);
                    });
                }
                else if (from.IsHandPiece())
                {
                    // -- 手駒を手駒に。手番が違うならこれは合法。

                    if (from.PieceColor() != to.PieceColor())
                    {
                        BoardEditCommand(raw =>
                        {
                            // 同種の駒が駒台から駒台に移動するので、to_prは関係ない。
                            raw.hands[(int)from.PieceColor()].Sub(from_pr);
                            raw.hands[(int)to.PieceColor()].Add(from_pr);
                        });
                    }
                    else
                    {
                        // 手駒を同じ駒台に移動させることは出来ないし、
                        // 選び直しているのでは？

                        if (to_pr != Piece.NO_PIECE)
                        {
                            pick_up_for_edit(to);
                            return;
                        }

                    }
                }
                else if (from.IsPieceBoxPiece())
                {
                    // -- 駒箱の駒を手駒に

                    // 玉は移動手駒に出来ない
                    if (from_pt != Piece.KING)
                        BoardEditCommand(raw => raw.hands[(int)to.PieceColor()].Add(from_pr));
                }
            }
            else if (to.IsPieceBoxPiece())
            {
                if (from.IsBoardPiece())
                {
                    // -- 盤上の駒を駒箱に
                    BoardEditCommand(raw => raw.board[(int)from] = Piece.NO_PIECE);

                }
                else if (from.IsHandPiece())
                {
                    // -- 駒台の駒を駒箱に

                    BoardEditCommand(raw => raw.hands[(int)from.PieceColor()].Sub(from_pr));

                }
                else if (from.IsPieceBoxPiece())
                {
                    // 駒箱の駒を移動させることは出来ないし、
                    // 選び直しているのでは？

                    if (to_pr != Piece.NO_PIECE)
                    {
                        pick_up_for_edit(to);
                        return;
                    }
                }

            }

        }

        /// <summary>
        /// ViewModel.viewStateをNormalの状態に戻す
        /// (これは駒を再度掴める状態)
        /// </summary>
        public void StateReset()
        {
            viewState.Reset();

            // 画面上の状態が変わるのでリセットするために再描画が必要
            Dirty = true;
        }

        /// <summary>
        /// [UI thread] : 盤面のsqの升(手駒も含む)がクリックされた
        /// </summary>
        /// <param name="sq"></param>
        public void OnBoardClick(SquareHand sq)
        {
            var pos = gameServer.Position;
            var state = viewState;
            var pc = pos.PieceOn(sq);

            //Console.WriteLine(sq.Pretty());

            if (gameServer.InTheBoardEdit)
            {
                // -- 盤面編集中

                switch (state.state)
                {
                    case GameScreenControlViewStateEnum.Normal:
                        {
                            // 盤面編集中はどの駒でも掴める
                            if (pc != Piece.NO_PIECE)
                                pick_up_for_edit(sq);
                            break;
                        }
                    case GameScreenControlViewStateEnum.PiecePickedUp:
                        {
                            // 盤面編集中はどの駒でも掴める
                            state.picked_to = sq;
                            move_piece_for_edit(state.picked_from, state.picked_to);
                            break;
                        }
                }

            }
            else
            {
                // -- 対局中、もしくは、対局終了後である。

                switch (state.state)
                {
                    case GameScreenControlViewStateEnum.Normal:
                        {
                            // 掴んだのが自分の駒であるか
                            if (pc != Piece.NO_PIECE && pc.PieceColor() == pos.sideToMove && !sq.IsPieceBoxPiece())
                                pick_up(sq); // sqの駒を掴んで行き先の候補の升情報を更新する

                            break;
                        }
                    case GameScreenControlViewStateEnum.PiecePickedUp:
                        {
                            // 次の4つのケースが考えられる
                            // 1.駒を掴んでいる状態なので移動先のクリック
                            // 2.自駒を再度掴んだ(掴んでいたのをキャンセルする)
                            // 3.別の自分の駒を掴んだ(掴み直し)
                            // 4.無縁の升をクリックした(掴んでいたのをキャンセルする)

                            // 1. 駒の移動

                            // いま掴んでいる駒の移動できる先であるのか。
                            var bb = state.picked_piece_legalmovesto;
                            if (sq.IsBoardPiece() && bb.IsSet((Square)sq))
                            {
                                state.picked_to = sq;
                                move_piece(state.picked_from, state.picked_to);
                            }
                            // 2. 掴んでいた駒の再クリック
                            else if (sq == state.picked_from)
                                StateReset();

                            // 3. 別の駒のクリック
                            else if (pc != Piece.NO_PIECE && pc.PieceColor() == pos.sideToMove && !sq.IsPieceBoxPiece())
                                pick_up(sq);

                            // 4. 掴む動作のキャンセル
                            else
                                StateReset();

                            break;
                        }
                    case GameScreenControlViewStateEnum.PromoteDialog:
                        {
                            // PromoteDialogを出していたのであれば、
                            switch (state.promote_dialog_selection)
                            {
                                case PromoteDialogSelectionEnum.NO_SELECT:
                                    break; // 無視
                                case PromoteDialogSelectionEnum.CANCEL:
                                    // キャンセルするので移動の駒の選択可能状態に戻してやる。
                                    StateReset();
                                    break;
                                // 成り・不成を選んでクリックしたのでそれに応じた移動を行う。
                                case PromoteDialogSelectionEnum.UNPROMOTE:
                                case PromoteDialogSelectionEnum.PROMOTE:
                                    var m = Util.MakeMove(state.picked_from, state.picked_to,
                                        state.promote_dialog_selection == PromoteDialogSelectionEnum.PROMOTE);
                                    DoMoveCommand(m);
                                    break;
                            }

                            break;
                        }
                }
            }
        }

        /// <summary>
        /// [UI thread] : 盤面のsqの升(手駒・駒箱の駒も含む)が右クリックされた
        /// </summary>
        /// <param name="sq"></param>
        public void OnBoardRightClick(SquareHand sq)
        {
            if (gameServer.InTheBoardEdit)
            {
                // -- 盤面編集中

                var pos = gameServer.Position;

                // 盤上の駒はクリックされるごとに先手→先手成駒→後手→後手成駒のように駒の変化
                if (sq.IsBoardPiece())
                {
                    var pc = pos.PieceOn(sq);
                    var rp = pc.RawPieceType();

                    // 玉であっても手番変更は出来るのでNO_PIECE以外であれば処理対象。
                    if (pc != Piece.NO_PIECE)
                    {
                        Piece nextPc;
                        // 成っていない駒なら、成駒に。成っている駒なら相手番の成っていない駒に。
                        if (pc.CanPromote())
                            nextPc = pc.ToPromotePiece();
                        else
                            nextPc = Util.MakePiece(pc.PieceColor().Not() /*相手番の駒に*/, pc.RawPieceType());

                        BoardEditCommand((raw_pos) => { raw_pos.board[(int)sq] = nextPc; });
                    }
                }

                //Console.WriteLine(sq.Pretty());
            }
        }

        /// <summary>
        /// 盤面を編集する時に用いる。
        /// BoardEditCommand((raw_pos) => { raw_pos.board[(int)sq] = nextPc; });
        /// のように書くと、その内容の盤面の更新依頼をGameServerのほうに依頼する。
        /// gamePlyは必ず1になる。
        /// </summary>
        /// <param name="func"></param>
        public void BoardEditCommand(Action<RawPosition> func)
        {
            var raw_pos = gameServer.Position.CreateRawPosition(); // Clone()
            func(raw_pos);

            // 盤面編集をしたのでgamePly == 1に変更してやる。
            raw_pos.gamePly = 1;

            var sfen = Position.SfenFromRawPosition(raw_pos);
            SetSfenCommand(sfen);
        }

        /// <summary>
        /// SetSfenを行う。LocalGameServer.SetSfenCommand()を直接呼び出さずに、
        /// 必ずこのメソッドを経由すること。
        /// </summary>
        /// <param name="sfen"></param>
        public void SetSfenCommand(string sfen)
        {
            // 描画の抑制。
            // これを行うことにより、見かけ上のatomic性を担保する。
            surpressDraw = true;
            StateReset();

            gameServer.SetSfenCommand(sfen,
                () => surpressDraw = false // 終了ハンドラで描画の抑制を解除する。
                );
        }

        /// <summary>
        /// DoMove()を行う。LocalGameServer.DoMoveCommand()を直接呼び出さずに、
        /// 必ずこのメソッドを経由すること。
        /// </summary>
        /// <param name="sfen"></param>
        public void DoMoveCommand(Move m)
        {
            // 描画の抑制。
            // これを行うことにより、見かけ上のatomic性を担保する。
            surpressDraw = true;
            StateReset();

            gameServer.DoMoveCommand(m ,
                () => surpressDraw = false // 終了ハンドラで描画の抑制を解除する。
                );
        }

        /// <summary>
        /// 最後にクリックされた升(ドラッグ動作である場合は、起点)
        /// </summary>
        private SquareHand lastClickedSq = SquareHand.NB;

        /// <summary>
        /// [UI thread] : 盤面がクリックされたときに呼び出されるハンドラ
        /// </summary>
        /// <param name="p"></param>
        public void OnClick(Point p , bool dragged = false)
        {
            var config = TheApp.app.Config;

            // マウスドラッグが許可されていないなら無視する。
            if (dragged && config.EnableMouseDrag == 0)
            {
                return ;
            }

            /// 座標系を、affine変換(逆変換)して、盤面座標系(0,0)-(board_img_width,board_image_height)にする。
            p = InverseAffine(p);

            // 盤面(手駒を含む)のどこの升がクリックされたのかを調べる
            SquareHand sq = BoardAxisToSquare(p);

            if (lastClickedSq == sq && dragged)
            {
                // クリックするときにマウスが微小に動き、ドラッグ動作になっているだけだと思われるので、
                // 操作性の観点から、このクリックはすべて無効化されるべき。

            } else
            {
                // 移動不可の升に移動させている場合、それはユーザーの誤操作の可能性があるので
                // このクリックを無視する。
                if (dragged &&
                    viewState.state == GameScreenControlViewStateEnum.PiecePickedUp &&
                    sq < SquareHand.SquareNB &&
                    !gameServer.InTheBoardEdit && // 盤面編集中だけは不法な移動を許可
                    !viewState.picked_piece_legalmovesto.IsSet((Square)sq)
                    )
                {
                    return;
                }

                OnBoardClick(sq);
            }

            // クリックの起点なので升を記録しておく。
            if (!dragged)
                lastClickedSq = sq;

            // デバッグ用にクリックされた升の名前を出力する。
            //Console.WriteLine(sq.Pretty());
        }

        /// <summary>
        /// [UI thread] : 盤面が右クリックされたときに呼び出されるハンドラ
        /// </summary>
        /// <param name="p"></param>
        public void OnRightClick(Point p)
        {
            p = InverseAffine(p);
            SquareHand sq = BoardAxisToSquare(p);
            OnBoardRightClick(sq);
        }

        // Drag処理、不評。単にD&Dの始点・終点の2点がクリックされたかのように扱うほうが適切であるようだ。

#if false
        /// <summary>
        /// 盤面がドラッグされたときに呼び出されるハンドラ
        /// </summary>
        /// <param name="p1">ドラッグ開始点</param>
        /// <param name="p2">ドラッグ終了点</param>
        public void OnDrag(Point p1, Point p2)
        {
            /// 座標系を、affine変換(逆変換)して、盤面座標系(0,0)-(board_img_width,board_image_height)にする。
            var p1_t = InverseAffine(p1);
            var p2_t = InverseAffine(p2);

            // 盤面(手駒を含む)のどこの升からどこの升にドラッグされたのかを調べる
            SquareHand sq1 = BoardAxisToSquare(p1_t);
            SquareHand sq2 = BoardAxisToSquare(p2_t);

            // 同じ升がクリックされていれば、これはOnClick()として処理してやる。
            // 単一クリックが間違えて、ドラッグになってしまった可能性が高い。
            // sq1 == SquareHand.NBの場合もそう。
            if (sq1 == sq2)
            {
                // affine変換前のもの
                OnClick(p1);
                return;
            }
            else
            {
                // 通常の移動であるが、これが駒の移動の条件を満たすことを確認しなければならない。

                // p1がクリックされたあとにp2がクリックされたことにしてお茶を濁す
                OnClick(p1);
                OnClick(p2);

                // 簡単なhackだが、これでだいたい意図通りの動作になるようだ。
            }

            // デバッグ用にドラッグされた升の名前を出力する。
            //Console.WriteLine(sq1.Pretty() + '→' + sq2.Pretty());
        }
#endif

        /// <summary>
        /// マウスが移動したときに呼び出されるハンドラ
        /// </summary>
        /// <param name="p"></param>
        public void OnMouseMove(Point p)
        {
            // ダイアログを表示している場合、そこにマウスがhoverすると
            // 素材がhover状態の表示になるので、その変化を反映しなくてはならない。

            var state = viewState;
            var pt = InverseAffine(p);
            var reverse = gameServer.BoardReverse;

            // 成り・不成のダイアログを出している
            if (state.state == GameScreenControlViewStateEnum.PromoteDialog)
            {
                // 与えられたpointがどこに属するか判定する。
                // まず逆affine変換して、viewの(DrawSpriteなどで使っている)座標系にする
                bool flip;
                var zero = CalcPromoteDialogLocation(state , reverse , out flip); // ここを原点とする
                pt = !flip ? new Point(pt.X - zero.X, pt.Y - zero.Y) : new Point(zero.X - pt.X , zero.Y - pt.Y);
                var selection = SPRITE.IsHoverPromoteDialog(pt);

                // 前回までと違う場所が選択されたのでselectionの値を更新して、画面の描画をしなおす。
                if (state.promote_dialog_selection != selection)
                {
                    state.promote_dialog_selection = selection;
                    Dirty = true;
                }

            } else {
                // 成り・不成のダイアログを出していないので、この座標を保存しておく。
                MouseClientLocation = pt;
                MouseClientLocationReverse = reverse; // この保存されたときにreverseであったかも併せて保存しておく。

                if (state.state == GameScreenControlViewStateEnum.PiecePickedUp && TheApp.app.Config.PickedMoveDisplayStyle == 1)
                {

                    // 駒をマウスカーソルに追随させるモードである
                    // マウスカーソルが移動しているので、このとき再描画が必要になる。
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// 盤面座標系から、それを表現するSquareHand型に変換する。
        ///
        /// reverse : 盤面を180度回転するのかのフラグ
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        SquareHand BoardAxisToSquare(Point p)
        {
            var reverse = gameServer.BoardReverse;

            // 盤上の升かどうかの判定
            var board_rect = new Rectangle(board_location.X, board_location.Y, piece_img_size.Width * 9, piece_img_size.Height * 9);
            if (board_rect.Contains(p))
            {
                // 筋と段
                var f = (File)(8 - (p.X - board_location.X) / piece_img_size.Width);
                var r = (Rank)((p.Y - board_location.Y) / piece_img_size.Height);
                if (File.ZERO <= f && f < File.NB && Rank.ZERO <= r && r < Rank.NB)
                {
                    var sq = Util.MakeSquare(f, r);
                    if (reverse)
                        sq = sq.Inv();

                    return (SquareHand)sq;
                }
            }
            else
            {
                // -- 手駒かどうかの判定

                foreach (var c in All.Colors())
                {
                    for (var pc = Piece.PAWN; pc < Piece.KING; ++pc)
                    {
                        var sq = Util.ToHandPiece(c, pc);
                        // 細長駒台があるのでわりと面倒。何も考えずに描画位置で判定する。
                        if (new Rectangle(PieceLocation(sq , reverse), piece_img_size).Contains(p))
                            return sq;
                    }

                    // それ以外の駒台の位置である判定も必要。
                    // これを一番最後にしないと、この領域の部分領域が判定できなくなってしまう。
                    if (HandTableRectangle(c , reverse ).Contains(p))
                        return Util.ToHandPiece(c, Piece.NO_PIECE);
                }

                // -- 駒箱であるかの判定

                if (gameServer.InTheBoardEdit)
                {
                    // 小さい駒の表示倍率
                    var ratio = 0.6f;

                    var config = TheApp.app.Config;
                    var size = PieceTableVersion == 0 ?
                        piece_img_size :
                        new Size((int)(piece_img_size.Width * ratio), (int)(piece_img_size.Height * ratio));

                    for (var pc = Piece.PAWN; pc <= Piece.KING; ++pc)
                    {
                        var sq = Util.ToPieceBoxPiece(pc);
                        if (new Rectangle(PieceLocation(sq , reverse), size).Contains(p))
                            return sq;
                    }

                    // それ以外の駒箱の位置である判定も必要。
                    if (PieceBoxRectangle().Contains(p))
                        return Util.ToPieceBoxPiece(Piece.NO_PIECE);
                }

            }

            // not found
            return SquareHand.NB;
        }

    }
}
