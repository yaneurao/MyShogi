namespace MyShogi.View.Win2D
{
    partial class EngineConsiderationControl
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
            this.components = new System.ComponentModel.Container();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SendToMainKifu = new System.Windows.Forms.ToolStripMenuItem();
            this.ReplaceMainKifu = new System.Windows.Forms.ToolStripMenuItem();
            this.PastePvToClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.PasteKifToClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.listView1 = new MyShogi.View.Win2D.ListViewEx();
            this.toolTip1 = new MyShogi.View.Win2D.ToolTipEx(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(2, 2);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(176, 19);
            this.textBox1.TabIndex = 2;
            this.textBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.textBox1, "エンジン名が表示されています。");
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.Color.White;
            this.textBox2.Location = new System.Drawing.Point(178, 2);
            this.textBox2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(137, 19);
            this.textBox2.TabIndex = 2;
            this.textBox2.TabStop = false;
            this.toolTip1.SetToolTip(this.textBox2, "エンジンが予想している次手番の指し手です。");
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.Color.White;
            this.textBox3.Location = new System.Drawing.Point(315, 2);
            this.textBox3.Margin = new System.Windows.Forms.Padding(2);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(155, 19);
            this.textBox3.TabIndex = 2;
            this.textBox3.TabStop = false;
            this.toolTip1.SetToolTip(this.textBox3, "探索した局面数です。");
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.Color.White;
            this.textBox4.Location = new System.Drawing.Point(471, 2);
            this.textBox4.Margin = new System.Windows.Forms.Padding(2);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(125, 19);
            this.textBox4.TabIndex = 2;
            this.textBox4.TabStop = false;
            this.toolTip1.SetToolTip(this.textBox4, "1秒間の探索局面数です。(Nodes Per Second)");
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.Color.White;
            this.textBox5.Location = new System.Drawing.Point(597, 2);
            this.textBox5.Margin = new System.Windows.Forms.Padding(2);
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(125, 19);
            this.textBox5.TabIndex = 2;
            this.textBox5.TabStop = false;
            this.toolTip1.SetToolTip(this.textBox5, "エンジンのHASHの使用率です。\r\nこの値が大きいと探索効率が悪くなります。\r\n(コンピューターの棋力が下がります)\r\nこの値が大きい場合(50%以上)は、エンジ" +
        "ン設定の\r\nHASHの項目でHASHの割当を増やすなどして調整すべきです。\r\n");
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "候補手１手",
            "候補手２手",
            "候補手３手",
            "候補手４手",
            "候補手５手",
            "候補手６手",
            "候補手７手",
            "候補手８手",
            "候補手９手",
            "候補手10手",
            "候補手11手",
            "候補手12手",
            "候補手13手",
            "候補手14手",
            "候補手15手"});
            this.comboBox1.Location = new System.Drawing.Point(2, 26);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(100, 20);
            this.comboBox1.TabIndex = 3;
            this.comboBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.comboBox1, "表示する候補手の数。\r\nこれを増やすとたくさんの候補手が表示されるようになりますが、\r\n1つの候補手に費やす時間は相対的に減るため、読みの質は下がります。");
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(103, 26);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(55, 20);
            this.button1.TabIndex = 4;
            this.button1.Text = "着順";
            this.toolTip1.SetToolTip(this.button1, "「着順」だとエンジンから送られてきた順に読みを表示します。\r\n「R順」だと良い指し手から順番に表示されます。");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SendToMainKifu,
            this.ReplaceMainKifu,
            this.PastePvToClipboard,
            this.PasteKifToClipboard});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(361, 92);
            // 
            // SendToMainKifu
            // 
            this.SendToMainKifu.Name = "SendToMainKifu";
            this.SendToMainKifu.Size = new System.Drawing.Size(360, 22);
            this.SendToMainKifu.Text = "メイン棋譜にこの読み筋を分岐棋譜として送る(&S)";
            this.SendToMainKifu.Click += new System.EventHandler(this.SendToMainKifu_Click);
            // 
            // ReplaceMainKifu
            // 
            this.ReplaceMainKifu.Name = "ReplaceMainKifu";
            this.ReplaceMainKifu.Size = new System.Drawing.Size(360, 22);
            this.ReplaceMainKifu.Text = "メイン棋譜をこの読み筋で置き換える(&R)";
            this.ReplaceMainKifu.Click += new System.EventHandler(this.ReplaceMainKifu_Click);
            // 
            // PastePvToClipboard
            // 
            this.PastePvToClipboard.Name = "PastePvToClipboard";
            this.PastePvToClipboard.Size = new System.Drawing.Size(360, 22);
            this.PastePvToClipboard.Text = "読み筋を表示のままの文字列でクリップボードに貼り付ける(&P)";
            this.PastePvToClipboard.Click += new System.EventHandler(this.PastePvToClipboard_Click);
            // 
            // PasteKifToClipboard
            // 
            this.PasteKifToClipboard.Name = "PasteKifToClipboard";
            this.PasteKifToClipboard.Size = new System.Drawing.Size(360, 22);
            this.PasteKifToClipboard.Text = "読み筋をKIF形式でクリップボードに貼り付ける(&K)";
            this.PasteKifToClipboard.Click += new System.EventHandler(this.PasteKifToClipboard_Click);
            // 
            // listView1
            // 
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 92);
            this.listView1.Margin = new System.Windows.Forms.Padding(2);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(762, 116);
            this.listView1.TabIndex = 1;
            this.listView1.TabStop = false;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.listView1_ColumnWidthChanged);
            this.listView1.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView1_ItemSelectionChanged);
            this.listView1.ClientSizeChanged += new System.EventHandler(this.listView1_ClientSizeChanged);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown);
            // 
            // EngineConsiderationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listView1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EngineConsiderationControl";
            this.Size = new System.Drawing.Size(764, 210);
            this.Resize += new System.EventHandler(this.EngineConsiderationControl_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MyShogi.View.Win2D.ListViewEx listView1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private MyShogi.View.Win2D.ToolTipEx toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem SendToMainKifu;
        private System.Windows.Forms.ToolStripMenuItem ReplaceMainKifu;
        private System.Windows.Forms.ToolStripMenuItem PastePvToClipboard;
        private System.Windows.Forms.ToolStripMenuItem PasteKifToClipboard;
    }
}
