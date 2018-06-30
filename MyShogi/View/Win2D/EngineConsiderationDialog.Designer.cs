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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.engineConsiderationControl1 = new MyShogi.View.Win2D.EngineConsiderationControl();
            this.engineConsiderationControl2 = new MyShogi.View.Win2D.EngineConsiderationControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
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
            this.splitContainer1.Size = new System.Drawing.Size(1499, 466);
            this.splitContainer1.SplitterDistance = 251;
            this.splitContainer1.TabIndex = 0;
            // 
            // engineConsiderationControl1
            // 
            this.engineConsiderationControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.engineConsiderationControl1.Location = new System.Drawing.Point(0, 0);
            this.engineConsiderationControl1.Name = "engineConsiderationControl1";
            this.engineConsiderationControl1.Size = new System.Drawing.Size(1499, 251);
            this.engineConsiderationControl1.TabIndex = 0;
            // 
            // engineConsiderationControl2
            // 
            this.engineConsiderationControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.engineConsiderationControl2.Location = new System.Drawing.Point(0, 0);
            this.engineConsiderationControl2.Name = "engineConsiderationControl2";
            this.engineConsiderationControl2.Size = new System.Drawing.Size(1499, 211);
            this.engineConsiderationControl2.TabIndex = 0;
            // 
            // EngineConsiderationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1499, 466);
            this.Controls.Add(this.splitContainer1);
            this.Name = "EngineConsiderationDialog";
            this.Text = "思考エンジン読み筋";
            this.TopMost = true;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private EngineConsiderationControl engineConsiderationControl1;
        private EngineConsiderationControl engineConsiderationControl2;
    }
}