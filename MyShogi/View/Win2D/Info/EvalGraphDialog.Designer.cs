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
            this.panel1 = new System.Windows.Forms.Panel();
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
            this.menuStrip.Size = new System.Drawing.Size(604, 30);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // LinearToolStripMenuItem
            // 
            this.LinearToolStripMenuItem.Font = new System.Drawing.Font("Yu Gothic UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.LinearToolStripMenuItem.Name = "LinearToolStripMenuItem";
            this.LinearToolStripMenuItem.Size = new System.Drawing.Size(66, 24);
            this.LinearToolStripMenuItem.Text = "評価値";
            this.LinearToolStripMenuItem.Click += new System.EventHandler(this.LinearToolStripMenuItem_Click);
            // 
            // NonlinearToolStripMenuItem
            // 
            this.NonlinearToolStripMenuItem.Font = new System.Drawing.Font("Yu Gothic UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.NonlinearToolStripMenuItem.Name = "NonlinearToolStripMenuItem";
            this.NonlinearToolStripMenuItem.Size = new System.Drawing.Size(66, 24);
            this.NonlinearToolStripMenuItem.Text = "広帯域";
            this.NonlinearToolStripMenuItem.Click += new System.EventHandler(this.NonlinearToolStripMenuItem_Click);
            // 
            // WinrateToolStripMenuItem
            // 
            this.WinrateToolStripMenuItem.Font = new System.Drawing.Font("Yu Gothic UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.WinrateToolStripMenuItem.Name = "WinrateToolStripMenuItem";
            this.WinrateToolStripMenuItem.Size = new System.Drawing.Size(51, 24);
            this.WinrateToolStripMenuItem.Text = "勝率";
            this.WinrateToolStripMenuItem.Click += new System.EventHandler(this.WinrateToolStripMenuItem_Click);
            // 
            // ReverseToolStripMenuItem
            // 
            this.ReverseToolStripMenuItem.Font = new System.Drawing.Font("Yu Gothic UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ReverseToolStripMenuItem.Name = "ReverseToolStripMenuItem";
            this.ReverseToolStripMenuItem.Size = new System.Drawing.Size(51, 24);
            this.ReverseToolStripMenuItem.Text = "反転";
            this.ReverseToolStripMenuItem.Click += new System.EventHandler(this.ReverseToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(3, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(600, 343);
            this.panel1.TabIndex = 2;
            // 
            // EvalGraphDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(604, 377);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EvalGraphDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "形勢グラフ";
            this.Move += new System.EventHandler(this.EvalGraphDialog_Move);
            this.Resize += new System.EventHandler(this.EvalGraphDialog_Resize);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem LinearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem NonlinearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem WinrateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ReverseToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
    }
}
