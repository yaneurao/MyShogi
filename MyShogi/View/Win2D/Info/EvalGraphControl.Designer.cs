namespace MyShogi.View.Win2D
{
    partial class EvalGraphControl
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.evalGraphPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.evalGraphPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // evalGraphPictureBox
            // 
            this.evalGraphPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.evalGraphPictureBox.Location = new System.Drawing.Point(0, 0);
            this.evalGraphPictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.evalGraphPictureBox.Name = "evalGraphPictureBox";
            this.evalGraphPictureBox.Size = new System.Drawing.Size(150, 150);
            this.evalGraphPictureBox.TabIndex = 0;
            this.evalGraphPictureBox.TabStop = false;
            this.evalGraphPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.evalGraphPictureBox_MouseDown);
            this.evalGraphPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.evalGraphPictureBox_MouseMove);
            this.evalGraphPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.evalGraphPictureBox_MouseUp);
            // 
            // EvalGraphControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.Controls.Add(this.evalGraphPictureBox);
            this.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "EvalGraphControl";
            this.Scroll += new System.Windows.Forms.ScrollEventHandler(this.EvalGraphControl_Scroll);
            ((System.ComponentModel.ISupportInitialize)(this.evalGraphPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox evalGraphPictureBox;
    }
}
