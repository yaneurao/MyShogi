namespace MyShogi.Model.Test
{
    partial class EvalControlTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.LinearUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.NonlinearUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.WinRateUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.evalGraphControl1 = new MyShogi.View.Win2D.EvalGraphControl();
            this.GraphReverse = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateData = new System.Windows.Forms.ToolStripMenuItem();
            this.backwardPly = new System.Windows.Forms.ToolStripMenuItem();
            this.forwardPly = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UpdateData,
            this.LinearUpdate,
            this.NonlinearUpdate,
            this.WinRateUpdate,
            this.GraphReverse,
            this.backwardPly,
            this.forwardPly});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 38);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // LinearUpdate
            // 
            this.LinearUpdate.Name = "LinearUpdate";
            this.LinearUpdate.Size = new System.Drawing.Size(67, 34);
            this.LinearUpdate.Text = "線形";
            this.LinearUpdate.Click += new System.EventHandler(this.LinearUpdate_Click);
            // 
            // NonlinearUpdate
            // 
            this.NonlinearUpdate.Name = "NonlinearUpdate";
            this.NonlinearUpdate.Size = new System.Drawing.Size(88, 34);
            this.NonlinearUpdate.Text = "非線形";
            this.NonlinearUpdate.Click += new System.EventHandler(this.NonlinearUpdate_Click);
            // 
            // WinRateUpdate
            // 
            this.WinRateUpdate.Name = "WinRateUpdate";
            this.WinRateUpdate.Size = new System.Drawing.Size(67, 34);
            this.WinRateUpdate.Text = "勝率";
            this.WinRateUpdate.Click += new System.EventHandler(this.WinRateUpdate_Click);
            // 
            // evalGraphControl1
            // 
            this.evalGraphControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.evalGraphControl1.AutoScroll = true;
            this.evalGraphControl1.Location = new System.Drawing.Point(0, 41);
            this.evalGraphControl1.Name = "evalGraphControl1";
            this.evalGraphControl1.Size = new System.Drawing.Size(800, 409);
            this.evalGraphControl1.TabIndex = 0;
            // 
            // GraphReverse
            // 
            this.GraphReverse.Name = "GraphReverse";
            this.GraphReverse.Size = new System.Drawing.Size(67, 34);
            this.GraphReverse.Text = "反転";
            this.GraphReverse.Click += new System.EventHandler(this.GraphReverse_Click);
            // 
            // UpdateData
            // 
            this.UpdateData.Name = "UpdateData";
            this.UpdateData.Size = new System.Drawing.Size(67, 34);
            this.UpdateData.Text = "更新";
            this.UpdateData.Click += new System.EventHandler(this.UpdateData_Click);
            // 
            // backwardPly
            // 
            this.backwardPly.Name = "backwardPly";
            this.backwardPly.Size = new System.Drawing.Size(46, 34);
            this.backwardPly.Text = "◀";
            this.backwardPly.Click += new System.EventHandler(this.backwardPly_Click);
            // 
            // forwardPly
            // 
            this.forwardPly.Name = "forwardPly";
            this.forwardPly.Size = new System.Drawing.Size(46, 34);
            this.forwardPly.Text = "▶";
            this.forwardPly.Click += new System.EventHandler(this.forwardPly_Click);
            // 
            // EvalControlTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.evalGraphControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EvalControlTestForm";
            this.Text = "EvalControlTestForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private View.Win2D.EvalGraphControl evalGraphControl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem LinearUpdate;
        private System.Windows.Forms.ToolStripMenuItem NonlinearUpdate;
        private System.Windows.Forms.ToolStripMenuItem WinRateUpdate;
        private System.Windows.Forms.ToolStripMenuItem GraphReverse;
        private System.Windows.Forms.ToolStripMenuItem UpdateData;
        private System.Windows.Forms.ToolStripMenuItem backwardPly;
        private System.Windows.Forms.ToolStripMenuItem forwardPly;
    }
}
