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
                    // 「対局開始」
                    DrawSprite(game_start_pos , SPRITE.GameStart());

                    var black = SPRITE.GameBlack();
                    var white = SPRITE.GameWhite();
                    if (gameServer.BoardReverse)
                        Utility.Swap(ref black , ref white);

                    // 「先手」
                    DrawSprite(game_black_pos, black);
                    // 「後手」
                    DrawSprite(game_white_pos, white);

                }, false , 2000
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
            var sprite = SPRITE.GameResult((MoveGameResult)args.value);
            if (sprite != null)
            {
                animatorManager.AddAnimator(new Animator(
                    elapsed =>
                    {
                        // 1秒後以降に表示する。(すぐに表示されるとちょっと邪魔)
                        if (elapsed >= 1000)
                            DrawSprite(game_result_pos, sprite);
                    }, true, 3500
                ));
            }
            else
            {
                // 「対局終了」の素材を表示する。
                animatorManager.AddAnimator(new Animator(
                    elapsed =>
                    {
                        // 1秒後以降に表示する。(すぐに表示されるとちょっと邪魔)
                        if (elapsed >= 1000)
                            DrawSprite(game_start_pos, SPRITE.GameEnd());
                    }, true, 3500
                ));
            }

        }
        #endregion
    }
}
