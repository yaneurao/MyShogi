using System.Drawing;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Shogi.Core;
using ShogiCore = MyShogi.Model.Shogi.Core;
using SPRITE = MyShogi.Model.Resource.SpriteManager;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// GameScreenに関するイベントハンドラ
    /// マウスがクリックされた時の処理など
    /// </summary>
    public partial class GameScreen
    {
        /// <summary>
        /// 盤面情報が更新された時に呼び出されるハンドラ。
        /// </summary>
        public void PositionChanged(PropertyChangedEventArgs args)
        {
            ViewModel.dirty = true;
        }

        /// <summary>
        /// 手番が変わったので何はともあれ、いま掴んでいる駒はいったん離す
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
        /// 残り時間が変更になった時に呼び出されるハンドラ
        /// 最小更新矩形を設定して更新すべき。
        /// </summary>
        /// <param name="args"></param>
        public void RestTimeChanged(PropertyChangedEventArgs args)
        {
            ViewModel.dirty = true;
        }

        /// <summary>
        /// メニューのすぐ下に配置しているtooltip buttonを、現在の状態に応じてOn/Offする。
        /// </summary>
        private void UpdateTooltipButtons()
        {
            // この時、エンジン側の手番であるなら、メインウインドウのメニューの「急」ボタンをenableにしなければならない。
            var gameServer = ViewModel.ViewModel.gameServer;
            var engineTurn = gameServer.EngineTurn;
            SetButton(MainDialogButtonEnum.MOVE_NOW, engineTurn);

            // この時、人間側の手番であるなら、メインウインドウのメニューの「投」「待」ボタンをenableにしなければならない。
            var humanTurn = gameServer.CanUserMove && !gameServer.EngineInitializing;
            SetButton(MainDialogButtonEnum.RESIGN   , humanTurn);
            SetButton(MainDialogButtonEnum.UNDO_MOVE, humanTurn);

            // 「中」ボタンは、エンジン同士の対局時にも中断できるようにするため、対局中であればいつでも中断できる。
            var canInterrupt = !gameServer.EngineInitializing && gameServer.InTheGame;
            SetButton(MainDialogButtonEnum.INTERRUPT, canInterrupt);
        }

        /// <summary>
        /// ボタンのEnable/Disableを切り替えたい時のcallback用のデリゲート
        /// </summary>
        /// <param name="name"></param>
        /// <param name="enable"></param>
        public delegate void SetButtonHandler(MainDialogButtonEnum name, bool enable);

        public SetButtonHandler SetButton { get; set; }

        /// <summary>
        /// エンジン初期化中の状態が変更になった時に呼び出されるハンドラ。
        /// エンジン初期化中のダイアログを描画している/していないはずなので、それを新しい状態に応じて再描画する必要がある。
        /// </summary>
        /// <param name="args"></param>
        public void EngineInitializingChanged(PropertyChangedEventArgs args)
        {
            UpdateTooltipButtons();
            ViewModel.dirty = true;
        }

        /// <summary>
        /// 画面が汚れているかどうかのフラグ。
        /// これを定期的に監視して、trueになっていれば、親からOnDraw()を呼び出してもらうものとする。
        /// </summary>
        public bool Dirty
        {
            get { return ViewModel.dirty; }
        }

        /// <summary>
        /// Formのリサイズに応じて棋譜コントロールの移動などを行う。
        /// </summary>
        public void ResizeKifuControl()
        {
            var kifu = ViewModel.kifuControl;

            var point = new Point(229, 600);
            kifu.Location = Affine(point);
            var size = new Size(265, 423);
            kifu.Size = AffineScale(size);

            kifu.OnResize(ViewModel.AffineMatrix.Scale.X);

            // kifuControl内の文字サイズも変更しないといけない。
            // あとで考える。

            // 駒台が縦長のモードのときは、このコントロールは非表示にする。
            // (何か別の方法で描画する)
            kifu.Visible = TheApp.app.config.KomadaiImageVersion == 1;
        }

        /// <summary>
        /// 描画できる領域が与えられるので、ここにうまく収まるようにaffine行列を設定する。
        /// </summary>
        /// <param name="screenSize"></param>
        public void FitToClientSize(Rectangle screenRect)
        {
            // 縦(h)を基準に横方向のクリップ位置を決める
            // 1920 * 1080が望まれる比率
            int w2 = screenRect.Height * 1920 / 1080;

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

            var scale = (double)screenRect.Height / board_img_size.Height;
            ViewModel.AffineMatrix.SetMatrix(scale, scale, (screenRect.Width - w2) / 2, screenRect.Top);

            set_komadai(screenRect);
        }

        /// <summary>
        ///  縦長の画面なら駒台を縦長にする。
        /// </summary>
        private void set_komadai(Rectangle screenRect)
        {
            double ratio = (double)screenRect.Width / screenRect.Height;
            //Console.WriteLine(ratio);

            TheApp.app.config.KomadaiImageVersion = (ratio < 1.36) ? 2 : 1;
        }

        /// <summary>
        /// sqの描画する場所を得る。
        /// Config.BoardReverseも反映されている。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        private Point PieceLocation(SquareHand sq)
        {
            var reverse = TheApp.app.config.BoardReverse;
            var color = sq.PieceColor();
            Point dest;

            if (color == ShogiCore.Color.NB)
            {
                // 盤面の升
                Square sq2 = reverse ? ((Square)sq).Inv() : (Square)sq;
                int f = 8 - (int)sq2.ToFile();
                int r = (int)sq2.ToRank();

                dest = new Point(board_location.X + piece_img_size.Width * f, board_location.Y + piece_img_size.Height * r);
            }
            else
            {
                if (reverse)
                    color = color.Not();

                var v = (TheApp.app.config.KomadaiImageVersion == 1) ? 0 : 1;

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

            return dest;
        }

        /// <summary>
        /// 指し手生成用のバッファ
        /// UIスレッドからしか使わない。マウスクリックのときの合法手を表示するために使う。
        /// </summary>
        private Move[] moves_buf { get; } = new Move[(int)Move.MAX_MOVES];

        /// <summary>
        /// sqの駒を掴む
        /// sqの駒が自駒であることは確定している。
        /// 行き先の候補の升情報を更新する。
        /// </summary>
        /// <param name="sq"></param>
        public void pick_up(SquareHand sq)
        {
            var gameServer = ViewModel.ViewModel.gameServer;
            if (!(gameServer.CanUserMove && !gameServer.EngineInitializing) )
                return;

            var pos = ViewModel.ViewModel.gameServer.Position;

            // この駒をユーザーが掴んで動かそうとしていることを示す
            ViewModel.viewState.picked_from = sq;
            ViewModel.viewState.picked_to = SquareHand.NB;
            ViewModel.viewState.state = GameScreenViewStateEnum.PiecePickedUp;

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

            var is_drop = sq.IsDrop();
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
                } else
                {
                    // 駒の移動できる先
                    if (!m.IsDrop() && m.From() == (Square)sq)
                        bb |= m.To();
                }
            }

            ViewModel.viewState.picked_piece_legalmovesto = bb;
            ViewModel.viewState.state = GameScreenViewStateEnum.PiecePickedUp;

            // この値が変わったことで画面の状態が変わるので、次回、OnDraw()が呼び出されなくてはならない。
            ViewModel.dirty = true;

        }

        /// <summary>
        /// 駒の移動
        /// ただし成り・不成が選べるときはここでそれを尋ねるダイアログが出る。
        /// また、連続王手の千日手局面に突入するときもダイアログが出る。
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void move_piece(SquareHand from,SquareHand to)
        {
            var state = ViewModel.viewState;

            // デバッグ用に表示する。
            //Console.WriteLine(from.Pretty() + "→" + to.Pretty());

            // この成る手を生成して、それが合法手であるなら、成り・不成のダイアログを出す必要がある。
            // また、1段目に進む「歩」などは、不成はまずいので選択がいらない。

            // Promoteの判定
            var pos = ViewModel.ViewModel.gameServer.Position;
            var pro_move = Util.MakeMove(from, to , true);
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
                ViewModel.ViewModel.gameServer.DoMoveCommand(unpro_move);
                StateReset();
            }
            // 上記 2.
            else if (!canUnpro && canPromote)
            {
                // 成るしか出来ないので、不成は選択肢から消して良い。
                // 成れないので成る選択肢は消して良い。
                ViewModel.ViewModel.gameServer.DoMoveCommand(pro_move);
                StateReset();
            }
            // 上記 4.
            // これで、上記の1.～4.のすべての状態を網羅したことになる。
            else // if (canPromote && canUnPro)
            {
                state.state = GameScreenViewStateEnum.PromoteDialog;
                state.moved_piece_type = pos.PieceOn(from).PieceType();

                // この状態を初期状態にするのは少しおかしいが、どうせこのあとマウスを動かすであろうからいいや。
                state.promote_dialog_selection = Model.Resource.PromoteDialogSelectionEnum.NO_SELECT;

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

                ViewModel.dirty = true;
            }
        }

        /// <summary>
        /// ViewModel.viewStateをNormalの状態に戻す
        /// (これは駒を再度掴める状態)
        /// </summary>
        public void StateReset()
        {
            var state = ViewModel.viewState;
            state.Reset();

            // 画面上の状態が変わるのでリセットするために再描画が必要
            ViewModel.dirty = true;
        }

        /// <summary>
        /// 盤面のsqの升(手駒も含む)がクリックされた
        /// </summary>
        /// <param name="sq"></param>
        public void OnBoardClick(SquareHand sq)
        {
            var pos = ViewModel.ViewModel.gameServer.Position;
            var state = ViewModel.viewState;
            var pc = pos.PieceOn(sq);

            //Console.WriteLine(sq.Pretty());

            switch (state.state)
            {
                case GameScreenViewStateEnum.Normal:
                    {
                        // 掴んだのが自分の駒であるか
                        if (pc != Piece.NO_PIECE && pc.PieceColor() == pos.sideToMove)
                            pick_up(sq); // sqの駒を掴んで行き先の候補の升情報を更新する

                        break;
                    }
                case GameScreenViewStateEnum.PiecePickedUp:
                    {
                        // 次の4つのケースが考えられる
                        // 1.駒を掴んでいる状態なので移動先のクリック
                        // 2.自駒を再度掴んだ(掴んでいたのをキャンセルする)
                        // 3.別の自分の駒を掴んだ(掴み直し)
                        // 4.無縁の升をクリックした(掴んでいたのをキャンセルする)

                        // 1. 駒の移動

                        // いま掴んでいる駒の移動できる先であるのか。
                        var bb = state.picked_piece_legalmovesto;
                        if (!sq.IsDrop() && bb.IsSet((Square)sq))
                        {
                            state.picked_to = sq;
                            move_piece(state.picked_from, state.picked_to);
                        }
                        // 2. 掴んでいた駒の再クリック
                        else if (sq == state.picked_from)
                            StateReset();

                        // 3. 別の駒のクリック
                        else if (pc != Piece.NO_PIECE && pc.PieceColor() == pos.sideToMove)
                            pick_up(sq);

                        // 4. 掴む動作のキャンセル
                        else
                            StateReset();

                        break;
                    }
                case GameScreenViewStateEnum.PromoteDialog:
                    {
                        // PromoteDialogを出していたのであれば、
                        switch (state.promote_dialog_selection)
                        {
                            case Model.Resource.PromoteDialogSelectionEnum.NO_SELECT:
                                break; // 無視
                            case Model.Resource.PromoteDialogSelectionEnum.CANCEL:
                                // キャンセルするので移動の駒の選択可能状態に戻してやる。
                                StateReset();
                                break;
                                // 成り・不成を選んでクリックしたのでそれに応じた移動を行う。
                            case Model.Resource.PromoteDialogSelectionEnum.UNPROMOTE:
                            case Model.Resource.PromoteDialogSelectionEnum.PROMOTE:
                                var m = Util.MakeMove(state.picked_from, state.picked_to,
                                    state.promote_dialog_selection == Model.Resource.PromoteDialogSelectionEnum.PROMOTE);
                                ViewModel.ViewModel.gameServer.DoMoveCommand(m);
                                StateReset();
                                break;
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// 盤面がクリックされたときに呼び出されるハンドラ
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
            } else
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

            var state = ViewModel.viewState;

            // 成り・不成のダイアログを出している
            if (state.state == GameScreenViewStateEnum.PromoteDialog)
            {
                // 与えられたpointがどこに属するか判定する。
                // まず逆affine変換して、viewの(DrawSpriteなどで使っている)座標系にする
                var pt = InverseAffine(p);
                var zero = state.promote_dialog_location; // ここを原点とする
                pt = new Point(pt.X - zero.X,pt.Y - zero.Y);
                var selection = SPRITE.IsHoverPromoteDialog(pt);

                // 前回までと違う場所が選択されたのでselectionの値を更新して、画面の描画をしなおす。
                if (state.promote_dialog_selection != selection)
                {
                    state.promote_dialog_selection = selection;
                    ViewModel.dirty = true;
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
            var config = TheApp.app.config;

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
                    if (config.BoardReverse)
                        sq = sq.Inv();

                    return (SquareHand)sq;
                }
            }
            else
            {
                // 手駒かどうかの判定
                // 細長駒台があるのでわりと面倒。何も考えずに描画位置で判定する。

                for (var sq = SquareHand.Hand; sq < SquareHand.HandNB; ++sq)
                    if (new Rectangle(PieceLocation(sq), piece_img_size).Contains(p))
                        return sq;
            }

            // not found
            return SquareHand.NB;
        }

    }
}
