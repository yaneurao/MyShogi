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
        /// 生存開始時刻。これを上回ると生存を開始してOnDraw()を呼び出す。
        /// </summary>
        long StartTime { get; set; }

        /// <summary>
        /// 生存終了時刻。elapsed_msがこれを上回ると自動的に解体される。
        /// </summary>
        long EndTime { get; set; }
    }

    /// <summary>
    /// 汎用Animator。
    ///
    /// 実際の使用例がGameScreenControlAnimator.csにあるので、
    /// そちらを参考にすること。
    /// </summary>
    public class Animator : IAnimator
    {
        /// <summary>
        /// 引数で渡したmyOnDrawを、OnDraw()のなかで呼び出してくれる。
        /// </summary>
        /// <param name="myOnDraw_">描画用のdelegate</param>
        /// <param name="animated_">これをtrueにすると毎フレーム描画される。(動きのあるものを表現するときに使う)</param>
        /// <param name="startTime">表示を開始する時刻</param>
        /// <param name="durationTime">表示する時間(開始からこの時間が経過すると、自動的に解体される)</param>
        public Animator(OnDrawDelegate myOnDraw_ , bool animated_ , long startTime , long durationTime )
        {
            Debug.Assert(myOnDraw_ != null);
            myOnDraw = myOnDraw_;
            Animate = animated_;
            StartTime = startTime;
            EndTime = startTime + durationTime;
        }

        public delegate void OnDrawDelegate(long frame);

        public void OnDraw(long elapsed_ms)
        {
            myOnDraw(elapsed_ms);
        }

        public bool Animate { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }

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
                // 開始しているのか
                var started = a.animator.StartTime <= elapsed_ms;
                var alive = elapsed_ms < a.animator.EndTime;
                if (started && alive)
                {
                    a.animator.OnDraw(elapsed_ms);
                    a.first = false;
                }
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
                    var started = a.animator.StartTime <= elapsed_ms;
                    var alive = elapsed_ms < a.animator.EndTime;
                    dirty |=
                        a.animator.Animate  || // 動かさないといけないので毎フレーム呼び出される。
                        (started && a.first) || // 描画開始時刻をすぎているのに未描画のAnimatorが存在する。
                        !alive /* これ、次フレームで消滅するのでdirtyであるべき */;
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
                first = true;
            }

            /// <summary>
            /// IAnimator本体
            /// </summary>
            public IAnimator animator;

            /// <summary>
            /// このクラスが生成されたときの。
            /// ここからの経過時間がOnDraw()のときに渡される。
            /// </summary>
            public long start_frame;

            /// <summary>
            /// まだ一度もOnDraw()を呼び出していないときのフラグ。
            /// animator.StartTimeを上回った時に描画される。
            /// </summary>
            public bool first;

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
