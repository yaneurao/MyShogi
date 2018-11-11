namespace MyShogi.View.Win2D
{
    partial class EngineConsiderationMainControl
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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.engineConsiderationControl1 = new MyShogi.View.Win2D.EngineConsiderationControl();
            this.engineConsiderationControl2 = new MyShogi.View.Win2D.EngineConsiderationControl();
            this.miniShogiBoard1 = new MyShogi.View.Win2D.MiniShogiBoard();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.miniShogiBoard1);
            this.splitContainer2.Size = new System.Drawing.Size(763, 235);
            this.splitContainer2.SplitterDistance = 571;
            this.splitContainer2.SplitterWidth = 2;
            this.splitContainer2.TabIndex = 1;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            this.splitContainer2.Resize += new System.EventHandler(this.splitContainer2_Resize);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.engineConsiderationControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.engineConsiderationControl2);
            this.splitContainer1.Size = new System.Drawing.Size(571, 235);
            this.splitContainer1.SplitterDistance = 126;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 1;
            // 
            // engineConsiderationControl1
            // 
            this.engineConsiderationControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.engineConsiderationControl1.EngineName = "";
            this.engineConsiderationControl1.Location = new System.Drawing.Point(0, 0);
            this.engineConsiderationControl1.Margin = new System.Windows.Forms.Padding(1);
            this.engineConsiderationControl1.Name = "engineConsiderationControl1";
            this.engineConsiderationControl1.RootSfen = null;
            this.engineConsiderationControl1.Size = new System.Drawing.Size(571, 126);
            this.engineConsiderationControl1.SortRanking = false;
            this.engineConsiderationControl1.TabIndex = 0;
            this.engineConsiderationControl1.TabStop = false;
            // 
            // engineConsiderationControl2
            // 
            this.engineConsiderationControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.engineConsiderationControl2.EngineName = "";
            this.engineConsiderationControl2.Location = new System.Drawing.Point(0, 0);
            this.engineConsiderationControl2.Margin = new System.Windows.Forms.Padding(1);
            this.engineConsiderationControl2.Name = "engineConsiderationControl2";
            this.engineConsiderationControl2.RootSfen = null;
            this.engineConsiderationControl2.Size = new System.Drawing.Size(571, 107);
            this.engineConsiderationControl2.SortRanking = false;
            this.engineConsiderationControl2.TabIndex = 0;
            this.engineConsiderationControl2.TabStop = false;
            // 
            // miniShogiBoard1
            // 
            this.miniShogiBoard1.BoardData = null;
            this.miniShogiBoard1.Location = new System.Drawing.Point(2, 10);
            this.miniShogiBoard1.Margin = new System.Windows.Forms.Padding(1);
            this.miniShogiBoard1.Name = "miniShogiBoard1";
            this.miniShogiBoard1.Size = new System.Drawing.Size(185, 202);
            this.miniShogiBoard1.TabIndex = 1;
            this.miniShogiBoard1.TabStop = false;
            // 
            // EngineConsiderationMainControl
            // 
            this.Controls.Add(this.splitContainer2);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EngineConsiderationMainControl";
            this.Size = new System.Drawing.Size(763, 235);
            this.Resize += new System.EventHandler(this.EngineConsiderationDialog_Resize);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private MyShogi.View.Win2D.EngineConsiderationControl engineConsiderationControl1;
        private MyShogi.View.Win2D.EngineConsiderationControl engineConsiderationControl2;
        private MyShogi.View.Win2D.MiniShogiBoard miniShogiBoard1;
    }
}
