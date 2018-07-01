namespace MyShogi.View.Win2D
{
    partial class EngineConsiderationDialog
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
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.miniShogiBoard1);
            this.splitContainer2.Size = new System.Drawing.Size(1499, 466);
            this.splitContainer2.SplitterDistance = 1125;
            this.splitContainer2.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer1.Size = new System.Drawing.Size(1125, 466);
            this.splitContainer1.SplitterDistance = 251;
            this.splitContainer1.TabIndex = 1;
            // 
            // engineConsiderationControl1
            // 
            this.engineConsiderationControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.engineConsiderationControl1.Location = new System.Drawing.Point(0, 0);
            this.engineConsiderationControl1.Name = "engineConsiderationControl1";
            this.engineConsiderationControl1.Size = new System.Drawing.Size(1125, 251);
            this.engineConsiderationControl1.TabIndex = 0;
            // 
            // engineConsiderationControl2
            // 
            this.engineConsiderationControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.engineConsiderationControl2.Location = new System.Drawing.Point(0, 0);
            this.engineConsiderationControl2.Name = "engineConsiderationControl2";
            this.engineConsiderationControl2.Size = new System.Drawing.Size(1125, 211);
            this.engineConsiderationControl2.TabIndex = 0;
            // 
            // miniShogiBoard1
            // 
            this.miniShogiBoard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.miniShogiBoard1.Location = new System.Drawing.Point(0, 0);
            this.miniShogiBoard1.Name = "miniShogiBoard1";
            this.miniShogiBoard1.Size = new System.Drawing.Size(370, 466);
            this.miniShogiBoard1.TabIndex = 1;
            // 
            // EngineConsiderationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1499, 466);
            this.Controls.Add(this.splitContainer2);
            this.Name = "EngineConsiderationDialog";
            this.Text = "思考エンジン読み筋";
            this.TopMost = true;
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
        private EngineConsiderationControl engineConsiderationControl1;
        private EngineConsiderationControl engineConsiderationControl2;
        private MiniShogiBoard miniShogiBoard1;
    }
}