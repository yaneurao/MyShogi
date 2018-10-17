using System;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// SuspendLayout()～ResumeLayout()をusing構文のなかでやるためのもの。
    /// ResumeLayout()忘れてreturnするのを防ぐ。
    /// </summary>
    public class SuspendLayoutBlock : IDisposable
    {
        public SuspendLayoutBlock(Control c)
        {
            c_ = c;
            c_.SuspendLayout();
        }

        public void Dispose()
        {
            c_.ResumeLayout();
        }

        private Control c_;
    }
}
