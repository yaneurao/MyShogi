namespace MyShogi.View.Win2D.Info
{
    partial class EvalGraphDialog
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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.LinearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NonlinearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WinrateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReverseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.evalGraphControl = new MyShogi.View.Win2D.EvalGraphControl();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LinearToolStripMenuItem,
            this.NonlinearToolStripMenuItem,
            this.WinrateToolStripMenuItem,
            this.ReverseToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(10, 3, 0, 3);
            this.menuStrip.Size = new System.Drawing.Size(604, 25);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // LinearToolStripMenuItem
            // 
            this.LinearToolStripMenuItem.Name = "LinearToolStripMenuItem";
            this.LinearToolStripMenuItem.Size = new System.Drawing.Size(55, 19);
            this.LinearToolStripMenuItem.Text = "評価値";
            this.LinearToolStripMenuItem.Click += new System.EventHandler(this.LinearToolStripMenuItem_Click);
            // 
            // NonlinearToolStripMenuItem
            // 
            this.NonlinearToolStripMenuItem.Name = "NonlinearToolStripMenuItem";
            this.NonlinearToolStripMenuItem.Size = new System.Drawing.Size(55, 19);
            this.NonlinearToolStripMenuItem.Text = "広帯域";
            this.NonlinearToolStripMenuItem.Click += new System.EventHandler(this.NonlinearToolStripMenuItem_Click);
            // 
            // WinrateToolStripMenuItem
            // 
            this.WinrateToolStripMenuItem.Name = "WinrateToolStripMenuItem";
            this.WinrateToolStripMenuItem.Size = new System.Drawing.Size(43, 19);
            this.WinrateToolStripMenuItem.Text = "勝率";
            this.WinrateToolStripMenuItem.Click += new System.EventHandler(this.WinrateToolStripMenuItem_Click);
            // 
            // ReverseToolStripMenuItem
            // 
            this.ReverseToolStripMenuItem.Name = "ReverseToolStripMenuItem";
            this.ReverseToolStripMenuItem.Size = new System.Drawing.Size(43, 19);
            this.ReverseToolStripMenuItem.Text = "反転";
            this.ReverseToolStripMenuItem.Click += new System.EventHandler(this.ReverseToolStripMenuItem_Click);
            // 
            // evalGraphControl
            // 
            this.evalGraphControl.AutoScroll = true;
            this.evalGraphControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.evalGraphControl.Location = new System.Drawing.Point(0, 25);
            this.evalGraphControl.Margin = new System.Windows.Forms.Padding(0);
            this.evalGraphControl.Name = "evalGraphControl";
            this.evalGraphControl.Size = new System.Drawing.Size(604, 352);
            this.evalGraphControl.TabIndex = 0;
            // 
            // EvalGraphDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(604, 377);
            this.Controls.Add(this.evalGraphControl);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "EvalGraphDialog";
            this.Text = "形勢グラフ";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private EvalGraphControl evalGraphControl;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem LinearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem NonlinearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem WinrateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ReverseToolStripMenuItem;
    }
}
