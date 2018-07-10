namespace MyShogi.View.Win2D
{
    partial class GameSettingDialog
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.numericUpDown11 = new System.Windows.Forms.NumericUpDown();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.timeSettingControl2 = new MyShogi.View.Win2D.Setting.TimeSettingControl();
            this.timeSettingControl1 = new MyShogi.View.Win2D.Setting.TimeSettingControl();
            this.playerSettingControl1 = new MyShogi.View.Win2D.Setting.PlayerSettingControl();
            this.playerSettingControl2 = new MyShogi.View.Win2D.Setting.PlayerSettingControl();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown11)).BeginInit();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboBox3);
            this.groupBox3.Controls.Add(this.radioButton6);
            this.groupBox3.Controls.Add(this.radioButton5);
            this.groupBox3.Location = new System.Drawing.Point(12, 320);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(8);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(8);
            this.groupBox3.Size = new System.Drawing.Size(748, 154);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "開始局面";
            // 
            // comboBox3
            // 
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "平手",
            "香落ち",
            "右香落ち",
            "角落ち",
            "飛車落ち",
            "飛香落ち",
            "二枚落ち",
            "三枚落ち",
            "四枚落ち",
            "五枚落ち",
            "左五枚落ち",
            "六枚落ち",
            "八枚落ち",
            "十枚落ち"});
            this.comboBox3.Location = new System.Drawing.Point(246, 44);
            this.comboBox3.Margin = new System.Windows.Forms.Padding(6);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(216, 35);
            this.comboBox3.TabIndex = 9;
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(26, 92);
            this.radioButton6.Margin = new System.Windows.Forms.Padding(6);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(181, 31);
            this.radioButton6.TabIndex = 8;
            this.radioButton6.TabStop = true;
            this.radioButton6.Text = "現在の局面 ";
            this.radioButton6.UseVisualStyleBackColor = true;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Checked = true;
            this.radioButton5.Location = new System.Drawing.Point(26, 46);
            this.radioButton5.Margin = new System.Windows.Forms.Padding(6);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(151, 31);
            this.radioButton5.TabIndex = 8;
            this.radioButton5.TabStop = true;
            this.radioButton5.Text = "初期局面";
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(258, 890);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(200, 54);
            this.button1.TabIndex = 19;
            this.button1.Text = "対局開始";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(798, 572);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(6);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(367, 31);
            this.checkBox1.TabIndex = 20;
            this.checkBox1.Text = "後手の時間設定を個別にする";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1068, 890);
            this.button2.Margin = new System.Windows.Forms.Padding(6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(200, 54);
            this.button2.TabIndex = 29;
            this.button2.Text = "先後入替";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(24, 40);
            this.checkBox4.Margin = new System.Windows.Forms.Padding(6);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(335, 31);
            this.checkBox4.TabIndex = 20;
            this.checkBox4.Text = "指定手数で引き分けにする";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // numericUpDown11
            // 
            this.numericUpDown11.Location = new System.Drawing.Point(406, 40);
            this.numericUpDown11.Margin = new System.Windows.Forms.Padding(6);
            this.numericUpDown11.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numericUpDown11.Name = "numericUpDown11";
            this.numericUpDown11.Size = new System.Drawing.Size(120, 34);
            this.numericUpDown11.TabIndex = 30;
            this.numericUpDown11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown11.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.checkBox4);
            this.groupBox6.Controls.Add(this.numericUpDown11);
            this.groupBox6.Location = new System.Drawing.Point(776, 322);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox6.Size = new System.Drawing.Size(748, 224);
            this.groupBox6.TabIndex = 31;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "その他の設定";
            // 
            // timeSettingControl2
            // 
            this.timeSettingControl2.Location = new System.Drawing.Point(778, 622);
            this.timeSettingControl2.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.timeSettingControl2.Name = "timeSettingControl2";
            this.timeSettingControl2.Size = new System.Drawing.Size(758, 258);
            this.timeSettingControl2.TabIndex = 33;
            // 
            // timeSettingControl1
            // 
            this.timeSettingControl1.Location = new System.Drawing.Point(12, 618);
            this.timeSettingControl1.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.timeSettingControl1.Name = "timeSettingControl1";
            this.timeSettingControl1.Size = new System.Drawing.Size(762, 262);
            this.timeSettingControl1.TabIndex = 32;
            // 
            // playerSettingControl1
            // 
            this.playerSettingControl1.Location = new System.Drawing.Point(6, 0);
            this.playerSettingControl1.Margin = new System.Windows.Forms.Padding(12);
            this.playerSettingControl1.Name = "playerSettingControl1";
            this.playerSettingControl1.Size = new System.Drawing.Size(772, 298);
            this.playerSettingControl1.TabIndex = 34;
            // 
            // playerSettingControl2
            // 
            this.playerSettingControl2.Location = new System.Drawing.Point(770, 0);
            this.playerSettingControl2.Margin = new System.Windows.Forms.Padding(12);
            this.playerSettingControl2.Name = "playerSettingControl2";
            this.playerSettingControl2.Size = new System.Drawing.Size(772, 298);
            this.playerSettingControl2.TabIndex = 35;
            // 
            // GameSettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1542, 956);
            this.Controls.Add(this.playerSettingControl2);
            this.Controls.Add(this.playerSettingControl1);
            this.Controls.Add(this.timeSettingControl2);
            this.Controls.Add(this.timeSettingControl1);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.Font = new System.Drawing.Font("MS UI Gothic", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(8);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameSettingDialog";
            this.Text = "対局";
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown11)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.NumericUpDown numericUpDown11;
        private System.Windows.Forms.GroupBox groupBox6;
        private Setting.TimeSettingControl timeSettingControl1;
        private Setting.TimeSettingControl timeSettingControl2;
        private Setting.PlayerSettingControl playerSettingControl1;
        private Setting.PlayerSettingControl playerSettingControl2;
    }
}