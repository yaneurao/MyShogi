using System;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// SuspendLayout()～ResumeLayout()をusing構文のなかでやるためのもの。
    /// ResumeLayout()忘れてreturnするのを防ぐ。
    ///
    /// 入れ子にして使うかも知れないので参照カウントを持っている。
    /// </summary>
    public class SuspendLayoutBlock : IDisposable
    {
        public SuspendLayoutBlock(Control c)
        {
            c_ = c;
            lock (lockObject)
                if (refCount ++ == 0)
                    c_.SuspendLayout();
        }

        public void Dispose()
        {
            lock(lockObject)
                if (--refCount == 0)
                    c_.ResumeLayout();
        }

        private Control c_;
        private static int refCount = 0;
        private static object lockObject = new object();
    }
}
