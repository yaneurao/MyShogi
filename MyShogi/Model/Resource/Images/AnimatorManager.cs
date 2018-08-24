using System.Collections.Generic;
using System.Diagnostics;

namespace MyShogi.Model.Resource.Images
{
    /// <summary>
    /// スプライトを一定時間ごとに動かしたりするためのクラス。
    ///
    /// Spriteクラスとは直接関係を持っていない、汎用性のあるクラス設計になっている。
    /// </summary>
    public interface IAnimator
    {
        /// <summary>
        /// 描画が要求されたときに呼び出される。
        ///
        /// frame : 生成後(AnimatorManager.AddSpriteAnimatorから)、何フレーム目であるか。
        /// 
        /// 60fpsだと仮定して、経過時間が何フレーム目の描画であるかが代入される。
        /// 例) 生成されてから1秒経過していれば60が入ってくる。
        ///
        /// 返し値としてtrueを返すと次回、このクラスは、AnimatorManager.listから除外される。
        /// </summary>
        /// <param name="frame"></param>
        bool OnDraw(long frame);
    }

    /// <summary>
    /// IAnimatorを管理するためのクラス。
    /// IAnimator派生クラスをぶら下げて使う。
    /// 
    /// DrawSpriteHandlerを設定して使う。
    /// </summary>
    public class AnimatorManager
    {
        public AnimatorManager()
        {
            // 描画のframeを計測するためのタイマーを回しておく。
            stopwatch.Start();
        }

        #region public members

        /// <summary>
        /// SprietAnimatorをlistに追加する。
        /// </summary>
        /// <param name="animator"></param>
        public void AddSpriteAnimator(IAnimator animator)
        {
            list.Add(new Animator(animator , GetFrame() ));
        }

        /// <summary>
        /// 描画を行う。AddSpriteAnimatorしておいたlistに対して、順番にOnDraw()を呼び出す。
        /// </summary>
        public void OnDraw()
        {
            var frame = GetFrame();

            foreach (var a in list)
                a.disposed = a.animator.OnDraw(frame - a.start_frame /* 経過フレーム数 */ );

            // disposeフラグが立っているものを除外する。
            list.RemoveAll(e => e.disposed);
        }

        #endregion

        #region privates

        /// <summary>
        /// SpriteAnimatorManagerが内部的に保持しているList
        /// </summary>
        private class Animator
        {
            public Animator(IAnimator animator_, long start_frame_)
            {
                animator = animator_;
                start_frame = start_frame_;
                disposed = false;
            }

            /// <summary>
            /// IAnimator本体
            /// </summary>
            public IAnimator animator;

            /// <summary>
            /// このクラスが生成されたときのframe数。
            /// ここからの経過時間がOnDraw()のときに渡される。
            /// </summary>
            public long start_frame;

            /// <summary>
            /// 次回に削除されるためのフラグ
            /// trueになっているものは次回にlistから除外される。
            /// </summary>
            public bool disposed;
        }

        /// <summary>
        /// このクラスが生成されてから何フレームが経過したかを返す。
        /// </summary>
        /// <returns></returns>
        private long GetFrame()
        {
            return stopwatch.ElapsedMilliseconds * 60 / 1000;
        }

        /// <summary>
        /// 保持しているAnimatorのリスト。
        /// このリストに対して順番に描画イベントが送信される。
        /// </summary>
        private List<Animator> list = new List<Animator>();

        /// <summary>
        /// 描画のためのタイマー
        /// </summary>
        private Stopwatch stopwatch = new Stopwatch();

        #endregion
    }
}
