namespace MyShogi.View.Win2D
{
    partial class MiniShogiBoard
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
            this.gameScreenControl1 = new MyShogi.View.Win2D.GameScreenControl();
            this.SuspendLayout();
            // 
            // gameScreenControl1
            // 
            this.gameScreenControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gameScreenControl1.Location = new System.Drawing.Point(0, 0);
            this.gameScreenControl1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.gameScreenControl1.Name = "gameScreenControl1";
            this.gameScreenControl1.Setting = null;
            this.gameScreenControl1.Size = new System.Drawing.Size(525, 402);
            this.gameScreenControl1.TabIndex = 0;
            // 
            // MiniShogiBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.gameScreenControl1);
            this.Name = "MiniShogiBoard";
            this.Size = new System.Drawing.Size(525, 402);
            this.ResumeLayout(false);

        }

        #endregion

        private GameScreenControl gameScreenControl1;
    }
}
