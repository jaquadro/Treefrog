namespace Treefrog.Windows.Forms
{
    partial class NewLevel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent ()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._originYField = new System.Windows.Forms.NumericUpDown();
            this._originXField = new System.Windows.Forms.NumericUpDown();
            this._buttonOK = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._levelHeight = new System.Windows.Forms.NumericUpDown();
            this._levelWidth = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this._name = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._originYField)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._originXField)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._levelHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._levelWidth)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 68);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(123, 78);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Origin";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._originYField, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this._originXField, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(111, 53);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Y:";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "X:";
            // 
            // _originYField
            // 
            this._originYField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._originYField.Location = new System.Drawing.Point(58, 29);
            this._originYField.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this._originYField.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this._originYField.Name = "_originYField";
            this._originYField.Size = new System.Drawing.Size(50, 20);
            this._originYField.TabIndex = 3;
            // 
            // _originXField
            // 
            this._originXField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._originXField.Location = new System.Drawing.Point(58, 3);
            this._originXField.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this._originXField.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this._originXField.Name = "_originXField";
            this._originXField.Size = new System.Drawing.Size(50, 20);
            this._originXField.TabIndex = 3;
            // 
            // _buttonOK
            // 
            this._buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOK.Location = new System.Drawing.Point(107, 152);
            this._buttonOK.Name = "_buttonOK";
            this._buttonOK.Size = new System.Drawing.Size(75, 23);
            this._buttonOK.TabIndex = 1;
            this._buttonOK.Text = "OK";
            this._buttonOK.UseVisualStyleBackColor = true;
            this._buttonOK.Click += new System.EventHandler(this._buttonOK_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(188, 152);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel2);
            this.groupBox2.Location = new System.Drawing.Point(141, 68);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(123, 78);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Level Size";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._levelHeight, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this._levelWidth, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(111, 53);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Height:";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Width:";
            // 
            // _levelHeight
            // 
            this._levelHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._levelHeight.Location = new System.Drawing.Point(58, 29);
            this._levelHeight.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this._levelHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._levelHeight.Name = "_levelHeight";
            this._levelHeight.Size = new System.Drawing.Size(50, 20);
            this._levelHeight.TabIndex = 3;
            this._levelHeight.Value = new decimal(new int[] {
            480,
            0,
            0,
            0});
            // 
            // _levelWidth
            // 
            this._levelWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._levelWidth.Location = new System.Drawing.Point(58, 3);
            this._levelWidth.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this._levelWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._levelWidth.Name = "_levelWidth";
            this._levelWidth.Size = new System.Drawing.Size(50, 20);
            this._levelWidth.TabIndex = 3;
            this._levelWidth.Value = new decimal(new int[] {
            800,
            0,
            0,
            0});
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel3);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(251, 50);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Level Details";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.91667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.08334F));
            this.tableLayoutPanel3.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this._name, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 17);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(240, 27);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Name:";
            // 
            // _name
            // 
            this._name.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._name.Location = new System.Drawing.Point(57, 3);
            this._name.Name = "_name";
            this._name.Size = new System.Drawing.Size(180, 20);
            this._name.TabIndex = 1;
            // 
            // NewLevel
            // 
            this.AcceptButton = this._buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(275, 186);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonOK);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewLevel";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Add New Level";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._originYField)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._originXField)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._levelHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._levelWidth)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button _buttonOK;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown _originYField;
        private System.Windows.Forms.NumericUpDown _originXField;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown _levelHeight;
        private System.Windows.Forms.NumericUpDown _levelWidth;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox _name;
    }
}