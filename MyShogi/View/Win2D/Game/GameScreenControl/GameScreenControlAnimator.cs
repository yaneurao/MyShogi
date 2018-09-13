using MyShogi.Model.Resource.Images;
using MyShogi.Model.Common.ObjectModel;
using SPRITE = MyShogi.Model.Resource.Images.SpriteManager;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;

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
        /// 対局開始
        /// </summary>
        /// <param name="args"></param>
        private void GameStartEventHandler(PropertyChangedEventArgs args)
        {
            animatorManager.ClearAnimator(); // 他のanimatorをすべて削除

            // これだけでアニメーションできる。便利すぎ。

            animatorManager.AddAnimator(new Animator(
                elapsed => {

                    // 中断局など特定局面からの開始でかつ連続対局ではないならこれは再開局扱い。
                    var restart = gameServer.GameSetting.BoardSetting.BoardTypeCurrent
                        && !gameServer.continuousGame.IsContinuousGameSet();

                    // 「対局開始」もしくは「対局再開」
                    var sprite = restart ? SPRITE.GameRestart() : SPRITE.GameStart();
                    DrawSprite(game_start_pos , sprite );

                    var handicapped = (bool)args.value;

                    var black = handicapped ? SPRITE.GameShitate() : SPRITE.GameBlack();
                    var white = handicapped ? SPRITE.GameUwate()   : SPRITE.GameWhite();

                    if (gameServer.BoardReverse)
                        Utility.Swap(ref black , ref white);

                    // 「先手」/「下手」
                    DrawSprite(game_black_pos, black);
                    // 「後手」/「上手」
                    DrawSprite(game_white_pos, white);

                }, false , 0 , 2000
            ));
        }

        /// <summary>
        /// 対局終了
        /// </summary>
        /// <param name="args"></param>
        private void GameEndEventHandler(PropertyChangedEventArgs args)
        {
            animatorManager.ClearAnimator(); // 他のanimatorをすべて削除

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
