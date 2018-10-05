namespace MyShogi.View.Win2D
{
    partial class DockWindow
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
            this.SuspendLayout();
            // 
            // DockWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "DockWindow";
            this.Text = "DockWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DockWindow_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DockWindow_KeyDown);
            this.Move += new System.EventHandler(this.DockWindow_Move);
            this.Resize += new System.EventHandler(this.DockWindow_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
