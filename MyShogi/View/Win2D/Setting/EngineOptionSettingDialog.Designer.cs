namespace MyShogi.View.Win2D
{
    partial class EngineOptionSettingDialog
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.engineOptionSettingControl1 = new MyShogi.View.Win2D.EngineOptionSettingControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.engineOptionSettingControl2 = new MyShogi.View.Win2D.EngineOptionSettingControl();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("ＭＳ ゴシック", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(764, 668);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.engineOptionSettingControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(1);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(1);
            this.tabPage1.Size = new System.Drawing.Size(756, 639);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "共通設定";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // engineOptionSettingControl1
            // 
            this.engineOptionSettingControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.engineOptionSettingControl1.Font = new System.Drawing.Font("MS UI Gothic", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.engineOptionSettingControl1.Location = new System.Drawing.Point(1, 1);
            this.engineOptionSettingControl1.Margin = new System.Windows.Forms.Padding(1);
            this.engineOptionSettingControl1.Name = "engineOptionSettingControl1";
            this.engineOptionSettingControl1.Size = new System.Drawing.Size(754, 637);
            this.engineOptionSettingControl1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.engineOptionSettingControl2);
            this.tabPage2.Location = new System.Drawing.Point(4, 23);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(1);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(1);
            this.tabPage2.Size = new System.Drawing.Size(756, 641);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "個別設定";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // engineOptionSettingControl2
            // 
            this.engineOptionSettingControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.engineOptionSettingControl2.Font = new System.Drawing.Font("MS UI Gothic", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.engineOptionSettingControl2.Location = new System.Drawing.Point(1, 1);
            this.engineOptionSettingControl2.Margin = new System.Windows.Forms.Padding(1);
            this.engineOptionSettingControl2.Name = "engineOptionSettingControl2";
            this.engineOptionSettingControl2.Size = new System.Drawing.Size(754, 639);
            this.engineOptionSettingControl2.TabIndex = 0;
            // 
            // EngineOptionSettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(764, 668);
            this.Controls.Add(this.tabControl1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EngineOptionSettingDialog";
            this.Text = "エンジンオプション設定";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private EngineOptionSettingControl engineOptionSettingControl2;
        private EngineOptionSettingControl engineOptionSettingControl1;
    }
}
