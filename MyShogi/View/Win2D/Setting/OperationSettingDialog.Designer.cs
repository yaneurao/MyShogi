namespace MyShogi.View.Win2D.Setting
{
    partial class OperationSettingDialog
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
            this.richSelector1 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.richSelector4 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector3 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector2 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.richSelector6 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector7 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector5 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.richSelector8 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.richSelector9 = new MyShogi.View.Win2D.Setting.RichSelector();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(790, 593);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richSelector1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(782, 567);
            this.tabPage1.TabIndex = 7;
            this.tabPage1.Text = "駒";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // richSelector1
            // 
            this.richSelector1.GroupBoxTitle = "駒をマウスのドラッグでも移動できるようにするか";
            this.richSelector1.Location = new System.Drawing.Point(6, 6);
            this.richSelector1.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector1.Name = "richSelector1";
            this.richSelector1.SelectionTexts = new string[] {
        "しない,dragged_move_style_0.png,マウスドラッグでの駒の移動を許容しません。",
        "する,dragged_move_style_1.png,マウスドラッグでの駒の移動を許容します。"};
            this.richSelector1.Size = new System.Drawing.Size(772, 110);
            this.richSelector1.TabIndex = 3;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.richSelector4);
            this.tabPage2.Controls.Add(this.richSelector3);
            this.tabPage2.Controls.Add(this.richSelector2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(782, 567);
            this.tabPage2.TabIndex = 8;
            this.tabPage2.Text = "棋譜";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // richSelector4
            // 
            this.richSelector4.GroupBoxTitle = "棋譜の最初進む/最後に進むに対応するキー";
            this.richSelector4.Location = new System.Drawing.Point(6, 116);
            this.richSelector4.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector4.Name = "richSelector4";
            this.richSelector4.SelectionTexts = new string[] {
        "なし,kifu_firstlastkey_0.png,なし。",
        "←と→,kifu_firstlastkey_1.png,カーソルキーの←と→に割当てます。棋譜の1手進む/戻るに対応するキーと重複している場合、無効になります。",
        "↑と↓,kifu_firstlastkey_2.png,カーソルキーの↑と↓に割当てます。棋譜の1手進む/戻るに対応するキーと重複している場合、無効になります。",
        "Page,kifu_firstlastkey_3.png,PageUpとPageDownに割当てます。"};
            this.richSelector4.Size = new System.Drawing.Size(772, 110);
            this.richSelector4.TabIndex = 6;
            // 
            // richSelector3
            // 
            this.richSelector3.GroupBoxTitle = "棋譜の1手進むに対応する特殊キー";
            this.richSelector3.Location = new System.Drawing.Point(6, 226);
            this.richSelector3.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector3.Name = "richSelector3";
            this.richSelector3.SelectionTexts = new string[] {
        "なし,kifu_next_specialkey_0.png,なし。",
        "スペース,kifu_next_specialkey_1.png,スペースキーに割当てます。",
        "Enter,kifu_next_specialkey_2.png,Enterキーに割当てます。"};
            this.richSelector3.Size = new System.Drawing.Size(772, 110);
            this.richSelector3.TabIndex = 5;
            // 
            // richSelector2
            // 
            this.richSelector2.GroupBoxTitle = "棋譜の1手進む/戻るに対応するキー";
            this.richSelector2.Location = new System.Drawing.Point(6, 6);
            this.richSelector2.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector2.Name = "richSelector2";
            this.richSelector2.SelectionTexts = new string[] {
        "なし,kifu_prevnextkey_0.png,なし",
        "←と→,kifu_prevnextkey_1.png,カーソルキーの左と右に割当てます。",
        "↑と↓,kifu_prevnextkey_2.png,カーソルキーの上と下に割当てます。"};
            this.richSelector2.Size = new System.Drawing.Size(772, 110);
            this.richSelector2.TabIndex = 4;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.richSelector6);
            this.tabPage3.Controls.Add(this.richSelector7);
            this.tabPage3.Controls.Add(this.richSelector5);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(782, 567);
            this.tabPage3.TabIndex = 9;
            this.tabPage3.Text = "検討";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // richSelector6
            // 
            this.richSelector6.GroupBoxTitle = "検討ウインドウで選択行をミニ盤面に反映させる";
            this.richSelector6.Location = new System.Drawing.Point(6, 226);
            this.richSelector6.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector6.Name = "richSelector6";
            this.richSelector6.SelectionTexts = new string[] {
        "なし,cons_sendpv_key_0.png,なし。",
        "スペース,cons_sendpv_key_1.png,スペースキーに割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "Enter,cons_sendpv_key_2.png,Enterキーに割当てます。棋譜操作のキーと重複している場合、無効化されます。"};
            this.richSelector6.Size = new System.Drawing.Size(772, 110);
            this.richSelector6.TabIndex = 5;
            // 
            // richSelector7
            // 
            this.richSelector7.GroupBoxTitle = "検討ウインドウで選択行の先頭/末尾移動";
            this.richSelector7.Location = new System.Drawing.Point(6, 116);
            this.richSelector7.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector7.Name = "richSelector7";
            this.richSelector7.SelectionTexts = new string[] {
        "なし,cons_headtailkey_0.png,なし",
        "Shift←→,cons_headtailkey_1.png,Shift+カーソルキーの左と右に割当てます。",
        "Shift↑↓,cons_headtailkey_2.png,Shift+カーソルキーの上と下に割当てます。",
        "←と→,cons_headtailkey_3.png,カーソルキーの左と右に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "↑と↓,cons_headtailkey_4.png,カーソルキーの上と下に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "，と ．,cons_headtailkey_5.png,  ，(カンマ)と ．(ピリオド)に割当てます。",
        "Page,cons_headtailkey_6.png,PageUpとPageDownに割当てます。棋譜操作のキーと重複している場合、無効化されます。"};
            this.richSelector7.Size = new System.Drawing.Size(772, 110);
            this.richSelector7.TabIndex = 4;
            // 
            // richSelector5
            // 
            this.richSelector5.GroupBoxTitle = "検討ウインドウで選択行の上下移動";
            this.richSelector5.Location = new System.Drawing.Point(6, 6);
            this.richSelector5.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector5.Name = "richSelector5";
            this.richSelector5.SelectionTexts = new string[] {
        "なし,cons_prevnextkey_0.png,なし",
        "Shift←→,cons_prevnextkey_1.png,Shift+カーソルキーの左と右に割当てます。",
        "Shift↑↓,cons_prevnextkey_2.png,Shift+カーソルキーの上と下に割当てます。",
        "←と→,cons_prevnextkey_3.png,カーソルキーの左と右に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "↑と↓,cons_prevnextkey_4.png,カーソルキーの上と下に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "，と ．,cons_prevnextkey_5.png,  ，(カンマ)と ．(ピリオド)に割当てます。",
        "Page,cons_prevnextkey_6.png,PageUpとPageDownに割当てます。棋譜操作のキーと重複している場合、無効化されます。"};
            this.richSelector5.Size = new System.Drawing.Size(772, 110);
            this.richSelector5.TabIndex = 4;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.richSelector8);
            this.tabPage4.Controls.Add(this.richSelector9);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(782, 567);
            this.tabPage4.TabIndex = 10;
            this.tabPage4.Text = "ミニ盤面";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // richSelector8
            // 
            this.richSelector8.GroupBoxTitle = "ミニ盤面で先頭/末尾移動";
            this.richSelector8.Location = new System.Drawing.Point(6, 116);
            this.richSelector8.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector8.Name = "richSelector8";
            this.richSelector8.SelectionTexts = new string[] {
        "なし,mini_headtailkey_0.png,なし",
        "Ctrl←→,mini_headtailkey_1.png,Ctrl+カーソルキーの左と右に割当てます。",
        "Ctrl↑↓,mini_headtailkey_2.png,Ctrl+カーソルキーの上と下に割当てます。",
        "←と→,mini_headtailkey_3.png,カーソルキーの左と右に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "↑と↓,mini_headtailkey_4.png,カーソルキーの上と下に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "，と ．,mini_headtailkey_5.png,  ，(カンマ)と ．(ピリオド)に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "Page,mini_headtailkey_6.png,PageUpとPageDownに割当てます。棋譜操作のキーと重複している場合、無効化されます。"};
            this.richSelector8.Size = new System.Drawing.Size(772, 110);
            this.richSelector8.TabIndex = 5;
            // 
            // richSelector9
            // 
            this.richSelector9.GroupBoxTitle = "ミニ盤面で一手戻る/進む";
            this.richSelector9.Location = new System.Drawing.Point(6, 6);
            this.richSelector9.Margin = new System.Windows.Forms.Padding(0);
            this.richSelector9.Name = "richSelector9";
            this.richSelector9.SelectionTexts = new string[] {
        "なし,mini_prevnextkey_0.png,なし",
        "Ctrl←→,mini_prevnextkey_1.png,Ctrl+カーソルキーの左と右に割当てます。",
        "Ctrl↑↓,mini_prevnextkey_2.png,Ctrl+カーソルキーの上と下に割当てます。",
        "←と→,mini_prevnextkey_3.png,カーソルキーの左と右に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "↑と↓,mini_prevnextkey_4.png,カーソルキーの上と下に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "，と ．,mini_prevnextkey_5.png,  ，(カンマ)と ．(ピリオド)に割当てます。棋譜操作のキーと重複している場合、無効化されます。",
        "Page,cons_prevnextkey_6.png,PageUpとPageDownに割当てます。棋譜操作のキーと重複している場合、無効化されます。"};
            this.richSelector9.Size = new System.Drawing.Size(772, 110);
            this.richSelector9.TabIndex = 6;
            // 
            // OperationSettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(790, 593);
            this.Controls.Add(this.tabControl1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OperationSettingDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "操作設定ダイアログ";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private RichSelector richSelector1;
        private System.Windows.Forms.TabPage tabPage2;
        private RichSelector richSelector2;
        private RichSelector richSelector3;
        private RichSelector richSelector4;
        private System.Windows.Forms.TabPage tabPage3;
        private RichSelector richSelector5;
        private RichSelector richSelector6;
        private RichSelector richSelector7;
        private System.Windows.Forms.TabPage tabPage4;
        private RichSelector richSelector8;
        private RichSelector richSelector9;
    }
}
