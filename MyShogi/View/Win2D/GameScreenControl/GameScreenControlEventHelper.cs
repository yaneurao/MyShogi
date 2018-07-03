using System;
using System.Drawing;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.LocalServer;
using ShogiCore = MyShogi.Model.Shogi.Core;
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
        /// KifuControlのハンドラを設定する。
        /// </summary>
        private void SetKifuControlHandler()
        {
            kifuControl1.SelectedIndexChangedHandler =
                (selectedIndex) => { gameServer.KifuSelectedIndexChangedCommand(selectedIndex); };
            kifuControl1.Button1ClickedHandler =
                () => { gameServer.MainBranchButtonCommand(); };
            kifuControl1.Button2ClickedHandler =
                () => { gameServer.NextBranchButtonCommand(); };
            kifuControl1.Button3ClickedHandler =
                () => { gameServer.EraseBranchButtonCommand(); };
        }

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
            gameServer.AddPropertyChangedHandler("KifuList", kifuControl1.OnListChanged, Parent);
            gameServer.AddPropertyChangedHandler("Position", PositionChanged);
            gameServer.AddPropertyChangedHandler("TurnChanged", TurnChanged, Parent);
            gameServer.AddPropertyChangedHandler("InTheGame", InTheGameChanged, Parent);
            gameServer.AddPropertyChangedHandler("EngineInitializing", EngineInitializingChanged, Parent);
            gameServer.AddPropertyChangedHandler("RestTimeChanged", RestTimeChanged);
            gameServer.AddPropertyChangedHandler("SetKifuListIndex", SetKifuListIndex, Parent);
            gameServer.AddPropertyChangedHandler("InTheBoardEdit", InTheBoardEditChanged, Parent);
            gameServer.AddPropertyChangedHandler("GameServerStarted", UpdateMenuItems, Parent);
            gameServer.AddPropertyChangedHandler("BoardReverse", UpdateMenuItems, Parent);
            gameServer.AddPropertyChangedHandler("EngineInfo", engineInfoChanged , Parent);
        }

        /// <summary>
        /// NotifyObjectのイベントハンドラを解除
        /// </summary>
        public void RemoveEventHandlers()
        {
            gameServer.RemovePropertyChangedHandler("KifuList", kifuControl1.OnListChanged);
            gameServer.RemovePropertyChangedHandler("Position", PositionChanged);
            gameServer.RemovePropertyChangedHandler("TurnChanged", TurnChanged);
            gameServer.RemovePropertyChangedHandler("InTheGame", InTheGameChanged);
            gameServer.RemovePropertyChangedHandler("EngineInitializing", EngineInitializingChanged);
            gameServer.RemovePropertyChangedHandler("RestTimeChanged", RestTimeChanged);
            gameServer.RemovePropertyChangedHandler("SetKifuListIndex", SetKifuListIndex);
            gameServer.RemovePropertyChangedHandler("InTheBoardEdit", InTheBoardEditChanged);
            gameServer.RemovePropertyChangedHandler("GameServerStarted", UpdateMenuItems);
            gameServer.RemovePropertyChangedHandler("BoardReverse", UpdateMenuItems);
            gameServer.RemovePropertyChangedHandler("EngineInfo", engineInfoChanged);
        }

        /// <summary>
        /// Settingで渡されたSetButtonのハンドラを呼び出す。
        /// </summary>
        public void SetButton(ToolStripButtonEnum name, bool enable)
        {
            if (Setting != null && Setting.SetButton != null)
                Setting.SetButton(name, enable);
        }

        /// <summary>
        /// [UI Thread] : このGameScreenControlに紐づけられているメニューの更新を行う。
        /// </summary>
        public void UpdateMenuItems(PropertyChangedEventArgs args = null)
        {
            if (Setting != null && Setting.UpdateMenuItems != null)
                Setting.UpdateMenuItems(null);
        }

        /// <summary>
        /// [UI Thread] : GameServer.InTheBoardEditの変更ハンドラ
        /// </summary>
        /// <param name="args"></param>
        public void InTheBoardEditChanged(PropertyChangedEventArgs args = null)
        {
            UpdateKifuControlVisibility();
            UpdateMenuItems();
        }

        /// <summary>
        /// [UI thread] : 棋譜ウィンドウの可視/不可視を設定するハンドラ
        /// </summary>
        public void UpdateKifuControlVisibility(PropertyChangedEventArgs args = null)
        {
            if (kifuControl!=null)
                kifuControl.Visible =
                    gameServer != null
                    && !gameServer.InTheBoardEdit /*盤面編集中は非表示*/
                    && PieceTableVersion == 0 /* 通常の駒台でなければ(細長い駒台の時は)非表示 */
                ;
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
            kifuControl.UpdateButtonState((bool)args.value /* == inTheGame */); // 棋譜ボタンが変化するかもなので

            // Tooltipの◁▷本譜ボタンの状態更新
            UpdateTooltipButtons2();

            // メニュー項目の更新
            UpdateMenuItems();
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
            kifuControl1.KifuListSelectedIndex = selectedIndex;
        }

        /// <summary>
        /// [UI thread] : メニューのすぐ下に配置しているtooltip buttonを、現在の状態に応じてOn/Offする。
        /// </summary>
        private void UpdateTooltipButtons()
        {
            // この時、エンジン側の手番であるなら、メインウインドウのメニューの「急」ボタンをenableにしなければならない。
            var engineTurn = gameServer.EngineTurn;
            SetButton(ToolStripButtonEnum.MOVE_NOW, engineTurn);

            // この時、対局中でかつ、人間側の手番で、エンジン初期化中でなければ、
            // メインウインドウのメニューの「投」「待」ボタンをenableにしなければならない。
            var humanTurn = gameServer.InTheGame && gameServer.CanUserMove && !gameServer.EngineInitializing;
            SetButton(ToolStripButtonEnum.RESIGN, humanTurn);
            SetButton(ToolStripButtonEnum.UNDO_MOVE, humanTurn);

            // 「中」ボタンは、エンジン同士の対局時にも中断できるようにするため、対局中であればいつでも中断できる。
            var canInterrupt = !gameServer.EngineInitializing && gameServer.InTheGame;
            SetButton(ToolStripButtonEnum.INTERRUPT, canInterrupt);
        }

        /// <summary>
        /// Tooltipの◁▷本譜ボタンの状態更新
        /// </summary>
        private void UpdateTooltipButtons2()
        {
            var consideration = gameServer.GameMode.IsConsideration();
            SetButton(ToolStripButtonEnum.REWIND, consideration);
            SetButton(ToolStripButtonEnum.FORWARD, consideration);
            SetButton(ToolStripButtonEnum.MAIN_BRANCH, consideration);
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
        /// [UI Thread] : LocalGameServerのEngineInfoのプロパティが変更になった時に呼び出されるハンドラ。
        /// </summary>
        /// <param name="args"></param>
        private void engineInfoChanged(PropertyChangedEventArgs args)
        {
            //　移譲しておく。
            EngineInfoChanged?.Invoke(args);
        }

        private bool dirty;

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
            var inTheGame = gameServer!= null && gameServer.InTheGame;

            var point = new Point(229, 600);
            kifu.Location = Affine(point);
            var size = new Size(265, 423);
            kifu.Size = AffineScale(size);

            kifu.OnResize(AffineMatrix.Scale.X, inTheGame);

            // kifuControl内の文字サイズも変更しないといけない。
            // あとで考える。

            // 駒台が縦長のモードのときは、このコントロールは非表示にする。
            // (何か別の方法で描画する)
            UpdateKifuControlVisibility();
        }

        /// <summary>
        /// 現在のこのControlのWidth,Heightに対して、いい感じに描画できるようにaffine行列を設定する。
        /// </summary>
        public void FitToClientSize()
        {
            // 縦(h)を基準に横方向のクリップ位置を決める
            // 1920 * 1080が望まれる比率
            int w2 = Height * 1920 / 1080;

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

            var scale = (double)Height / board_img_size.Height;
            AffineMatrix.SetMatrix(scale, scale, (Width - w2) / 2, 0 /* Top (Formの場合)*/);

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
        /// sqの描画する場所を得る。
        /// Config.BoardReverseも反映されている。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        private Point PieceLocation(SquareHand sq)
        {
            var reverse = gameServer.BoardReverse;
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
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private Rectangle HandTableRectangle(ShogiCore.Color c)
        {
            var reverse = gameServer.BoardReverse;
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
                // 成れないので成る選択肢は消して良い。
                gameServer.DoMoveCommand(unpro_move);
                StateReset();
            }
            // 上記 2.
            else if (!canUnpro && canPromote)
            {
                // 成るしか出来ないので、不成は選択肢から消して良い。
                // 成れないので成る選択肢は消して良い。
                gameServer.DoMoveCommand(pro_move);
                StateReset();
            }
            // 上記 4.
            // これで、上記の1.～4.のすべての状態を網羅したことになる。
            else // if (canPromote && canUnPro)
            {
                state.state = GameScreenControlViewStateEnum.PromoteDialog;
                state.moved_piece_type = pos.PieceOn(from).PieceType();

                // この状態を初期状態にするのは少しおかしいが、どうせこのあとマウスを動かすであろうからいいや。
                state.promote_dialog_selection = PromoteDialogSelectionEnum.NO_SELECT;

                // toの近くに成り・不成のダイアログを描画してやる。
                // これは移動先の升の少し下に出す。
                // ただし、1段目であると画面外に行ってしまうので
                // 1,2段目であれば上に変位させる必要がある。
                // あと、棋譜ウィンドウもこの手前に描画できないので
                // ここも避ける必要がある。

                var dest = PieceLocation(to);
                if (dest.Y >= board_img_size.Height * 0.8) // 画面の下らへんである..
                    dest += new Size(-130 + 95 / 2, -200);
                else
                    dest += new Size(-103 + 95 / 2, +100);

                if (dest.X < board_location.X)
                    dest += new Size(150, 0);

                state.promote_dialog_location = dest;

                // toの升以外は暗くする。
                state.picked_piece_legalmovesto = new Bitboard((Square)to);

                Dirty = true;
            }
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
            var from_pc = pos.PieceOn(from);
            var from_pt = from_pc.PieceType();
            var from_pr = from_pc.RawPieceType();
            var to_pc = pos.PieceOn(to);
            var to_pr = to_pc.RawPieceType();

            if (to.IsBoardPiece())
            {
                if (from.IsBoardPiece())
                {
                    // -- 盤上と盤上の駒

                    // fromとtoの升の駒を交換する。
                    // fromに駒があることは確定している。toに駒があろうがなかろうが良い。
                    BoardEditCommand(raw =>
                    {
                        raw.board[(int)from] = to_pc;
                        raw.board[(int)to] = from_pc;
                    });
                }
                else if (from.IsHandPiece())
                {
                    // -- 手駒を盤面に

                    BoardEditCommand(raw =>
                    {
                        raw.hands[(int)from.PieceColor()].Sub(from_pt);
                        raw.board[(int)to] = from_pc;
                        // このtoの位置にもし駒があったとしたら、それは駒箱に移動する。
                        // (その駒が欠落するので..)
                    });
                }
                else if (from.IsPieceBoxPiece())
                {
                    // -- 駒箱の駒を盤面に

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

            StateReset();
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
                                    gameServer.DoMoveCommand(m);
                                    StateReset();
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

                    // 玉であっても裏返せる。
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
        /// </summary>
        /// <param name="func"></param>
        public void BoardEditCommand(Action<RawPosition> func)
        {
            var raw_pos = gameServer.Position.CreateRawPosition(); // Clone()
            func(raw_pos);
            var sfen = Position.SfenFromRawPosition(raw_pos);
            gameServer.SetSfenCommand(sfen);
        }

        /// <summary>
        /// [UI thread] : 盤面がクリックされたときに呼び出されるハンドラ
        /// </summary>
        /// <param name="p"></param>
        public void OnClick(Point p)
        {
            /// 座標系を、affine変換(逆変換)して、盤面座標系(0,0)-(board_img_width,board_image_height)にする。
            p = InverseAffine(p);

            // 盤面(手駒を含む)のどこの升がクリックされたのかを調べる
            SquareHand sq = BoardAxisToSquare(p);
            OnBoardClick(sq);

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

        /// <summary>
        /// マウスが移動したときに呼び出されるハンドラ
        /// </summary>
        /// <param name="p"></param>
        public void OnMouseMove(Point p)
        {
            // ダイアログを表示している場合、そこにマウスがhoverすると
            // 素材がhover状態の表示になるので、その変化を反映しなくてはならない。

            var state = viewState;

            // 成り・不成のダイアログを出している
            if (state.state == GameScreenControlViewStateEnum.PromoteDialog)
            {
                // 与えられたpointがどこに属するか判定する。
                // まず逆affine変換して、viewの(DrawSpriteなどで使っている)座標系にする
                var pt = InverseAffine(p);
                var zero = state.promote_dialog_location; // ここを原点とする
                pt = new Point(pt.X - zero.X, pt.Y - zero.Y);
                var selection = SPRITE.IsHoverPromoteDialog(pt);

                // 前回までと違う場所が選択されたのでselectionの値を更新して、画面の描画をしなおす。
                if (state.promote_dialog_selection != selection)
                {
                    state.promote_dialog_selection = selection;
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// 盤面座標系から、それを表現するSquareHand型に変換する。
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
                        if (new Rectangle(PieceLocation(sq), piece_img_size).Contains(p))
                            return sq;
                    }

                    // それ以外の駒台の位置である判定も必要。
                    // これを一番最後にしないと、この領域の部分領域が判定できなくなってしまう。
                    if (HandTableRectangle(c).Contains(p))
                        return Util.ToHandPiece(c, Piece.NO_PIECE);
                }

                // -- 駒箱であるかの判定

                if (gameServer.InTheBoardEdit)
                {
                    // 小さい駒の表示倍率
                    var ratio = 0.6f;

                    var config = TheApp.app.config;
                    var size = PieceTableVersion == 0 ?
                        piece_img_size :
                        new Size((int)(piece_img_size.Width * ratio), (int)(piece_img_size.Height * ratio));

                    for (var pc = Piece.PAWN; pc <= Piece.KING; ++pc)
                    {
                        var sq = Util.ToPieceBoxPiece(pc);
                        if (new Rectangle(PieceLocation(sq), size).Contains(p))
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
