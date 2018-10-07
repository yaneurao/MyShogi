namespace MyShogi.View.Win2D.Setting
{
    partial class DisplaySettingDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage10 = new System.Windows.Forms.TabPage();
            this.richSelector5 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector4 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector1 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector3 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector2 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.richSelector6 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage10.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(196, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 15);
            this.label1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage10);
            this.tabControl1.Location = new System.Drawing.Point(-4, -5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(792, 443);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richSelector5);
            this.tabPage1.Controls.Add(this.richSelector4);
            this.tabPage1.Controls.Add(this.richSelector1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(784, 414);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "盤面表示";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage10
            // 
            this.tabPage10.Controls.Add(this.richSelector3);
            this.tabPage10.Controls.Add(this.richSelector2);
            this.tabPage10.Location = new System.Drawing.Point(4, 25);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage10.Size = new System.Drawing.Size(784, 414);
            this.tabPage10.TabIndex = 1;
            this.tabPage10.Text = "棋譜表示";
            this.tabPage10.UseVisualStyleBackColor = true;
            // 
            // richSelector5
            // 
            this.richSelector5.GroupBoxTitle = "畳画像";
            this.richSelector5.Location = new System.Drawing.Point(6, 208);
            this.richSelector5.Name = "richSelector5";
            this.richSelector5.SelectionTexts = new string[] {
        "薄い,tatami_image_ver_1.png",
        "濃い,tatami_image_ver_2.png"};
            this.richSelector5.Size = new System.Drawing.Size(772, 110);
            this.richSelector5.TabIndex = 0;
            // 
            // richSelector4
            // 
            this.richSelector4.GroupBoxTitle = "盤画像";
            this.richSelector4.Location = new System.Drawing.Point(9, 106);
            this.richSelector4.Name = "richSelector4";
            this.richSelector4.SelectionTexts = new string[] {
        "白色,board_image_ver_1.png",
        "黄色,board_image_ver_2.png"};
            this.richSelector4.Size = new System.Drawing.Size(772, 110);
            this.richSelector4.TabIndex = 0;
            // 
            // richSelector1
            // 
            this.richSelector1.GroupBoxTitle = "筋・段の表示";
            this.richSelector1.Location = new System.Drawing.Point(6, 6);
            this.richSelector1.Name = "richSelector1";
            this.richSelector1.SelectionTexts = new string[] {
        "非表示,rank_style_0.png",
        "標準,rank_style_1.png",
        "Chess式,rank_style_2.png"};
            this.richSelector1.Size = new System.Drawing.Size(772, 110);
            this.richSelector1.TabIndex = 0;
            // 
            // richSelector3
            // 
            this.richSelector3.GroupBoxTitle = "検討ウィンドウの棋譜の表示形式";
            this.richSelector3.Location = new System.Drawing.Point(6, 113);
            this.richSelector3.Name = "richSelector3";
            this.richSelector3.SelectionTexts = new string[] {
        "標準(KI2),cons_window_kif_style_0.png",
        "簡易(KIF),cons_window_kif_style_1.png",
        "CSA,cons_window_kif_style_2.png",
        "SFEN,cons_window_kif_style_3.png"};
            this.richSelector3.Size = new System.Drawing.Size(772, 112);
            this.richSelector3.TabIndex = 1;
            // 
            // richSelector2
            // 
            this.richSelector2.GroupBoxTitle = "棋譜ウィンドウの棋譜の表示形式";
            this.richSelector2.Location = new System.Drawing.Point(6, 6);
            this.richSelector2.Name = "richSelector2";
            this.richSelector2.SelectionTexts = new string[] {
        "標準(KI2),kifwindow_kif_style_0.png",
        "簡易(KIF),kifwindow_kif_style_1.png",
        "CSA,kifwindow_kif_style_2.png",
        "SFEN,kifwindow_kif_style_3.png"};
            this.richSelector2.Size = new System.Drawing.Size(772, 112);
            this.richSelector2.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.richSelector6);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(784, 414);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "駒表示";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // richSelector6
            // 
            this.richSelector6.GroupBoxTitle = "駒画像";
            this.richSelector6.Location = new System.Drawing.Point(3, 3);
            this.richSelector6.Name = "richSelector6";
            this.richSelector6.SelectionTexts = new string[] {
        "二文字駒,piece_image_ver_1.png",
        "一文字駒,piece_image_ver_2.png",
        "英文字駒,piece_image_ver_3.png"};
            this.richSelector6.Size = new System.Drawing.Size(772, 110);
            this.richSelector6.TabIndex = 1;
            // 
            // DisplaySettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DisplaySettingDialog";
            this.Text = "表示設定ダイアログ";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage10.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage10;
        private RichSelector richSelector1;
        private RichSelector richSelector2;
        private RichSelector richSelector3;
        private RichSelector richSelector4;
        private RichSelector richSelector5;
        private System.Windows.Forms.TabPage tabPage2;
        private RichSelector richSelector6;
    }
}
