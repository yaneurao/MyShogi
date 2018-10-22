using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public SuspendLayoutBlock(Control c_)
        {
            lock (lockObject)
            {
                c = c_;

                if (list.ContainsKey(c))
                    ++list[c]; // 参照カウントのインクリメント
                else
                {
                    list.Add(c, 1);
                    c.SuspendLayout();
                }
            }
        }

        public void Dispose()
        {
            lock (lockObject)
            {
                Debug.Assert(list.ContainsKey(c));
                if (--list[c] == 0)
                {
                    list.Remove(c);
                    c.ResumeLayout();
                }
            }
        }

        private Control c;
        private static object lockObject = new object();
        private static Dictionary<Control, int> list = new Dictionary<Control, int>();
    }
}
