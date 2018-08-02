namespace MyShogi.View.Win2D
{
    partial class GameScreenControl
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
            this.kifuControl1 = new MyShogi.View.Win2D.KifuControl();
            this.SuspendLayout();
            // 
            // kifuControl1
            // 
            this.kifuControl1.Location = new System.Drawing.Point(148, 546);
            this.kifuControl1.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.kifuControl1.Name = "kifuControl1";
            this.kifuControl1.Size = new System.Drawing.Size(100, 100);
            this.kifuControl1.TabIndex = 0;
            // 
            // GameScreenControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.kifuControl1);
            this.DoubleBuffered = true;
            this.Name = "GameScreenControl";
            this.Size = new System.Drawing.Size(400, 400);
            this.SizeChanged += new System.EventHandler(this.GameScreenControl_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GameScreenControl_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GameScreenControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GameScreenControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GameScreenControl_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private KifuControl kifuControl1;
    }
}
