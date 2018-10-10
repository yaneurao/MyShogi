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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameSettingDialog));
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.playerSettingControl2 = new MyShogi.View.Win2D.PlayerSettingControl();
            this.playerSettingControl1 = new MyShogi.View.Win2D.PlayerSettingControl();
            this.timeSettingControl2 = new MyShogi.View.Win2D.TimeSettingControl();
            this.timeSettingControl1 = new MyShogi.View.Win2D.TimeSettingControl();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboBox2);
            this.groupBox3.Controls.Add(this.radioButton6);
            this.groupBox3.Controls.Add(this.radioButton5);
            this.groupBox3.Location = new System.Drawing.Point(6, 190);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(374, 77);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "開始局面";
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
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
            this.comboBox2.Location = new System.Drawing.Point(123, 22);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(110, 20);
            this.comboBox2.TabIndex = 9;
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(13, 46);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(85, 16);
            this.radioButton6.TabIndex = 8;
            this.radioButton6.TabStop = true;
            this.radioButton6.Text = "現在の局面 ";
            this.toolTip1.SetToolTip(this.radioButton6, "現在の局面から対局を開始したい場合に使います。");
            this.radioButton6.UseVisualStyleBackColor = true;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Checked = true;
            this.radioButton5.Location = new System.Drawing.Point(13, 23);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(71, 16);
            this.radioButton5.TabIndex = 8;
            this.radioButton5.TabStop = true;
            this.radioButton5.Text = "初期局面";
            this.toolTip1.SetToolTip(this.radioButton5, "初期局面として平手・駒落ちを選べます。");
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(129, 487);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 27);
            this.button1.TabIndex = 19;
            this.button1.Text = "対局開始";
            this.toolTip1.SetToolTip(this.button1, "この設定で対局を開始します。");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(399, 331);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(167, 16);
            this.checkBox1.TabIndex = 20;
            this.checkBox1.Text = "後手の時間設定を個別にする";
            this.toolTip1.SetToolTip(this.checkBox1, "後手の持ち時間を先手とは異なる持ち時間設定にしたい時にオンにしてください。");
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(534, 487);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 27);
            this.button2.TabIndex = 29;
            this.button2.Text = "先後入替";
            this.toolTip1.SetToolTip(this.button2, "先手と後手のプレイヤーを入替えます。");
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(12, 20);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(153, 16);
            this.checkBox4.TabIndex = 20;
            this.checkBox4.Text = "指定手数で引き分けにする";
            this.toolTip1.SetToolTip(this.checkBox4, "指定手数を超えると引き分け扱いにします。\r\nコンピューターと対局するときは、「詳細設定」の「MaxMovesToDraw」を\r\nここで設定したのと同じ値に変更して" +
        "おいてやる必要があります。");
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(234, 20);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(60, 19);
            this.numericUpDown2.TabIndex = 30;
            this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown2.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label3);
            this.groupBox6.Controls.Add(this.label2);
            this.groupBox6.Controls.Add(this.checkBox3);
            this.groupBox6.Controls.Add(this.checkBox2);
            this.groupBox6.Controls.Add(this.numericUpDown1);
            this.groupBox6.Controls.Add(this.label1);
            this.groupBox6.Controls.Add(this.comboBox1);
            this.groupBox6.Controls.Add(this.checkBox4);
            this.groupBox6.Controls.Add(this.numericUpDown2);
            this.groupBox6.Location = new System.Drawing.Point(388, 190);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(374, 136);
            this.groupBox6.TabIndex = 31;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "その他の設定";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(298, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 36;
            this.label3.Text = "局";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(298, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 35;
            this.label2.Text = "手";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(32, 72);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(205, 16);
            this.checkBox3.TabIndex = 33;
            this.checkBox3.Text = "連続対局のときに手番を入れ替えない";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(12, 46);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(179, 16);
            this.checkBox2.TabIndex = 33;
            this.checkBox2.Text = "連続対局を行う(COM同士の時)";
            this.toolTip1.SetToolTip(this.checkBox2, "プレイヤーがコンピュータ同士の対局であるとき、連続対局を行います。\r\n対局結果一覧はメニューの「ウィンドウ」→「対局結果ウインドウ」を選ぶと確認できます。");
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(234, 46);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(60, 19);
            this.numericUpDown1.TabIndex = 34;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown1.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 32;
            this.label1.Text = "入玉ルール";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "入玉ルールなし",
            "24点法(CSAルール)",
            "27点法(CSAルール)",
            "トライルール"});
            this.comboBox1.Location = new System.Drawing.Point(91, 101);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(138, 20);
            this.comboBox1.TabIndex = 31;
            this.toolTip1.SetToolTip(this.comboBox1, resources.GetString("comboBox1.ToolTip"));
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(17, 276);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(202, 16);
            this.checkBox5.TabIndex = 39;
            this.checkBox5.Text = "コンピューターは1手に必ずこれだけ使う";
            this.toolTip1.SetToolTip(this.checkBox5, "コンピューターは1手に必ずこれだけ使います。\r\n(思考が終わっていても指すのを待機します)\r\n単位は[ms]なので1秒を指定したいなら「1000」と入力してくださ" +
        "い。");
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(17, 301);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(131, 16);
            this.checkBox6.TabIndex = 39;
            this.checkBox6.Text = "先後は振り駒で決める";
            this.toolTip1.SetToolTip(this.checkBox6, "対局前に先手・後手を振り駒で決めます。\r\n(先手・後手がランダムで決まります)");
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(343, 277);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 12);
            this.label4.TabIndex = 38;
            this.label4.Text = "[ms]";
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown3.Location = new System.Drawing.Point(272, 274);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(69, 19);
            this.numericUpDown3.TabIndex = 37;
            this.numericUpDown3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // playerSettingControl2
            // 
            this.playerSettingControl2.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.playerSettingControl2.Location = new System.Drawing.Point(385, 0);
            this.playerSettingControl2.Margin = new System.Windows.Forms.Padding(6);
            this.playerSettingControl2.Name = "playerSettingControl2";
            this.playerSettingControl2.Size = new System.Drawing.Size(383, 190);
            this.playerSettingControl2.TabIndex = 35;
            // 
            // playerSettingControl1
            // 
            this.playerSettingControl1.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.playerSettingControl1.Location = new System.Drawing.Point(3, 0);
            this.playerSettingControl1.Margin = new System.Windows.Forms.Padding(6);
            this.playerSettingControl1.Name = "playerSettingControl1";
            this.playerSettingControl1.Size = new System.Drawing.Size(384, 190);
            this.playerSettingControl1.TabIndex = 34;
            // 
            // timeSettingControl2
            // 
            this.timeSettingControl2.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.timeSettingControl2.Location = new System.Drawing.Point(389, 353);
            this.timeSettingControl2.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.timeSettingControl2.Name = "timeSettingControl2";
            this.timeSettingControl2.Size = new System.Drawing.Size(379, 129);
            this.timeSettingControl2.TabIndex = 33;
            // 
            // timeSettingControl1
            // 
            this.timeSettingControl1.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.timeSettingControl1.Location = new System.Drawing.Point(6, 351);
            this.timeSettingControl1.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.timeSettingControl1.Name = "timeSettingControl1";
            this.timeSettingControl1.Size = new System.Drawing.Size(381, 131);
            this.timeSettingControl1.TabIndex = 32;
            // 
            // GameSettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(771, 520);
            this.Controls.Add(this.checkBox6);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDown3);
            this.Controls.Add(this.playerSettingControl2);
            this.Controls.Add(this.playerSettingControl1);
            this.Controls.Add(this.timeSettingControl2);
            this.Controls.Add(this.timeSettingControl1);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameSettingDialog";
            this.Text = "対局";
            this.Load += new System.EventHandler(this.GameSettingDialog_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.GroupBox groupBox6;
        private TimeSettingControl timeSettingControl1;
        private TimeSettingControl timeSettingControl2;
        private PlayerSettingControl playerSettingControl1;
        private PlayerSettingControl playerSettingControl2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
    }
}
