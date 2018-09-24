using MyShogi.Model.Resource.Images;
using MyShogi.Model.Common.ObjectModel;
using SPRITE = MyShogi.Model.Resource.Images.SpriteManager;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.App;
using System.Drawing;
using MyShogi.Model.Shogi.LocalServer;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// Animator関連はここに書く。
    /// </summary>
    public partial class GameScreenControl
    {
        /// <summary>
        /// Animator管理クラス
        /// </summary>
        private AnimatorManager animatorManager = new AnimatorManager();

        #region PropertyChangedEventHandlers
        /// <summary>
        /// [UI Thread] 対局開始
        /// </summary>
        /// <param name="args"></param>
        private void GameStartEventHandler(PropertyChangedEventArgs args)
        {
            animatorManager.ClearAnimator(); // 他のanimatorをすべて削除

            // エフェクト無効なら、やらない。
            var enable = TheApp.app.Config.EnableGameEffect != 0;
            if (!enable)
                return;

            var continuousGame = args.value as ContinuousGame;

            // 振り駒画像を表示するのかのフラグ
            var enable_piece_toss = continuousGame.EnablePieceToss; //  true;
            // 振り駒を表示しているので「対局開始」の表示が後ろ倒しになる時間。[ms]
            var piece_toss_time = 0;

            if (enable_piece_toss)
            {
                // 振り駒イメージ画像
                animatorManager.AddAnimator(new Animator(
                    elapsed =>
                    { DrawSprite(game_piece_toss_pos, SPRITE.GamePieceToss()); }, false, 0, 500
                ));

                // 駒を5枚描画

                animatorManager.AddAnimator(new Animator(
                    elapsed =>
                    {
                        // ptpc[x] == trueならx枚目として歩、falseなら「と」を描画。
                        var ptpc = continuousGame.PieceTossPieceColor;
                        foreach (var i in All.Int(5))
                        {
                            var sprite = SPRITE.Piece(ptpc[i] ? Piece.PAWN : Piece.PRO_PAWN);
                            var dest = new Point(
                                board_img_size.Width/2 + (i-2) * 110 - piece_img_size.Width / 2,
                                board_img_size.Height / 2 - piece_img_size.Height * 3 / 4 );
                            DrawSprite(dest, sprite);
                        }

                    }, false, 500, 2500
                ));

                piece_toss_time = 1000;
            }

            // これだけでアニメーションできる。便利すぎ。
            animatorManager.AddAnimator(new Animator(
                elapsed => {

                    // 中断局など特定局面からの開始でかつ連続対局ではないならこれは再開局扱い。
                    var restart = gameServer.GameSetting.BoardSetting.BoardTypeCurrent
                        && !gameServer.continuousGame.IsContinuousGameSet();

                    // 「対局開始」もしくは「対局再開」
                    var sprite = restart ? SPRITE.GameRestart() : SPRITE.GameStart();
                    DrawSprite(game_start_pos , sprite );

                    var handicapped = continuousGame.Handicapped;

                    var black = handicapped ? SPRITE.GameShitate() : SPRITE.GameBlack();
                    var white = handicapped ? SPRITE.GameUwate()   : SPRITE.GameWhite();

                    if (gameServer.BoardReverse)
                        Utility.Swap(ref black , ref white);

                    // 「先手」/「下手」
                    DrawSprite(game_black_pos, black);
                    // 「後手」/「上手」
                    DrawSprite(game_white_pos, white);

                }, false , piece_toss_time , 2000
            ));
        }

        /// <summary>
        /// [UI Thread] 対局終了
        /// </summary>
        /// <param name="args"></param>
        private void GameEndEventHandler(PropertyChangedEventArgs args)
        {
            animatorManager.ClearAnimator(); // 他のanimatorをすべて削除

            // エフェクト無効なら、やらない。
            var enable = TheApp.app.Config.EnableGameEffect != 0;
            if (!enable)
                return;

            // 対局結果から、「勝ち」「負け」「引き分け」のいずれかを表示する。
            var result = (MoveGameResult)args.value;
            var win_lose_draw = result == MoveGameResult.WIN || result == MoveGameResult.LOSE || result == MoveGameResult.DRAW;
            var sprite = win_lose_draw ? SPRITE.GameResult(result):
                result == MoveGameResult.UNKNOWN ? SPRITE.GameEnd():
                result == MoveGameResult.INTERRUPT ? SPRITE.GameInterrupt():
                null;
            var pos = win_lose_draw ? game_result_pos : game_start_pos;

            // 1秒後以降に表示する。(すぐに表示されるとちょっと邪魔)
            animatorManager.AddAnimator(new Animator(
                elapsed => { DrawSprite(pos, sprite); }, false, 1000, 3500
            ));

        }
        #endregion
    }
}
