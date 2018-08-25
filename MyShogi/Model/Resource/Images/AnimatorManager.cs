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
        /// elapsed_ms : 生成後(AnimatorManager.AddSpriteAnimatorから)、何ms経過しているか。
        /// </summary>
        /// <param name="elapsed_ms"></param>
        void OnDraw(long elapsed_ms);

        /// <summary>
        /// 動きがあるのか。(なければ1フレ描画するだけで、あとは解体時に描画すれば良い。)
        /// </summary>
        bool Animate { get; set; }

        /// <summary>
        /// 生存時間。elapsed_msがこれを上回ると自動的に解体される。
        /// </summary>
        long LifeTime { get; set; }
    }

    /// <summary>
    /// 汎用Animator。
    /// </summary>
    public class Animator : IAnimator
    {
        /// <summary>
        /// 引数で渡したmyOnDrawを、OnDraw()のなかで呼び出してくれる。
        /// </summary>
        /// <param name="myOnDraw_"></param>
        public Animator(OnDrawDelegate myOnDraw_ , bool animated_ , long lifeTime )
        {
            Debug.Assert(myOnDraw_ != null);
            myOnDraw = myOnDraw_;
            Animate = animated_;
            LifeTime = lifeTime;
        }

        public delegate void OnDrawDelegate(long frame);

        public void OnDraw(long elapsed_ms)
        {
            myOnDraw(elapsed_ms);
        }

        public bool Animate { get; set; }
        public long LifeTime { get; set; }

        /// <summary>
        /// これを設定すると、OnDraw()のときにこれが呼び出される。
        /// </summary>
        private OnDrawDelegate myOnDraw;
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
        /// 描画を行う。AddSpriteAnimatorしておいたlistに対して、順番にOnDraw()を呼び出す。
        /// </summary>
        public void OnDraw()
        {
            var frame = GetFrame();

            foreach (var a in list)
            {
                // 経過時間[ms]
                var elapsed_ms = frame - a.start_frame;
                var alive = elapsed_ms < a.animator.LifeTime;
                if (alive)
                    a.animator.OnDraw(elapsed_ms);
                // aliveでないものはこのフレームで描画せずに除外される。
                a.disposed = !alive;
            }

            // disposeフラグが立っているものを除外する。
            list.RemoveAll(e => e.disposed);
        }

        /// <summary>
        /// IAnimator派生クラスをlistに追加する。
        /// </summary>
        /// <param name="animator"></param>
        public void AddAnimator(IAnimator animator)
        {
            list.Add(new Animator(animator, GetFrame()));
        }

        /// <summary>
        /// 保持しているすべてのlistをクリアする。
        /// </summary>
        public void ClearAnimator()
        {
            list.Clear();
        }

        #endregion

        #region properties

        /// <summary>
        /// 描画すべきであるかのフラグ
        /// </summary>
        /// <returns></returns>
        public bool Dirty { get
            {
                var frame = GetFrame();
                var dirty = false;
                foreach (var a in list)
                {
                    var elapsed_ms = frame - a.start_frame;
                    var alive = elapsed_ms < a.animator.LifeTime;
                    dirty |= a.animator.Animate || !alive /* これ、次フレームで消滅するのでdirtyであるべき */;
                }

                return dirty;
            }
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
        /// このクラスが生成されてから何msが経過したかを返す。
        /// </summary>
        /// <returns></returns>
        private long GetFrame()
        {
            return stopwatch.ElapsedMilliseconds;
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
