namespace Treefrog.Plugins.Object.UI
{
    partial class ImportObject
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._textSource = new System.Windows.Forms.TextBox();
            this._buttonBrowse = new System.Windows.Forms.Button();
            this._textObjectName = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._numMaskBottom = new System.Windows.Forms.NumericUpDown();
            this._numMaskRight = new System.Windows.Forms.NumericUpDown();
            this._numMaskTop = new System.Windows.Forms.NumericUpDown();
            this._numMaskLeft = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this._numOriginY = new System.Windows.Forms.NumericUpDown();
            this._numOriginX = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOK = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numMaskBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numMaskRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numMaskTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numMaskLeft)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numOriginY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numOriginX)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._textSource);
            this.groupBox1.Controls.Add(this._buttonBrowse);
            this.groupBox1.Controls.Add(this._textObjectName);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(333, 71);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "New Object Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Object Name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Source File:";
            // 
            // _textSource
            // 
            this._textSource.Location = new System.Drawing.Point(84, 19);
            this._textSource.Name = "_textSource";
            this._textSource.Size = new System.Drawing.Size(162, 20);
            this._textSource.TabIndex = 0;
            // 
            // _buttonBrowse
            // 
            this._buttonBrowse.Location = new System.Drawing.Point(252, 17);
            this._buttonBrowse.Name = "_buttonBrowse";
            this._buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this._buttonBrowse.TabIndex = 1;
            this._buttonBrowse.Text = "Browse";
            this._buttonBrowse.UseVisualStyleBackColor = true;
            this._buttonBrowse.Click += new System.EventHandler(this._buttonBrowse_Click);
            // 
            // _textObjectName
            // 
            this._textObjectName.Location = new System.Drawing.Point(84, 45);
            this._textObjectName.Name = "_textObjectName";
            this._textObjectName.Size = new System.Drawing.Size(243, 20);
            this._textObjectName.TabIndex = 0;
            this._textObjectName.TextChanged += new System.EventHandler(this._textObjectName_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Location = new System.Drawing.Point(351, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(199, 199);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Preview";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._numMaskBottom);
            this.groupBox3.Controls.Add(this._numMaskRight);
            this.groupBox3.Controls.Add(this._numMaskTop);
            this.groupBox3.Controls.Add(this._numMaskLeft);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(12, 89);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(333, 71);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Mask Bounds";
            // 
            // _numMaskBottom
            // 
            this._numMaskBottom.Location = new System.Drawing.Point(266, 45);
            this._numMaskBottom.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this._numMaskBottom.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this._numMaskBottom.Name = "_numMaskBottom";
            this._numMaskBottom.Size = new System.Drawing.Size(61, 20);
            this._numMaskBottom.TabIndex = 11;
            this._numMaskBottom.ValueChanged += new System.EventHandler(this._numMaskBottom_ValueChanged);
            // 
            // _numMaskRight
            // 
            this._numMaskRight.Location = new System.Drawing.Point(266, 20);
            this._numMaskRight.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this._numMaskRight.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this._numMaskRight.Name = "_numMaskRight";
            this._numMaskRight.Size = new System.Drawing.Size(61, 20);
            this._numMaskRight.TabIndex = 10;
            this._numMaskRight.ValueChanged += new System.EventHandler(this._numMaskRight_ValueChanged);
            // 
            // _numMaskTop
            // 
            this._numMaskTop.Location = new System.Drawing.Point(84, 45);
            this._numMaskTop.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this._numMaskTop.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this._numMaskTop.Name = "_numMaskTop";
            this._numMaskTop.Size = new System.Drawing.Size(61, 20);
            this._numMaskTop.TabIndex = 9;
            this._numMaskTop.ValueChanged += new System.EventHandler(this._numMaskTop_ValueChanged);
            // 
            // _numMaskLeft
            // 
            this._numMaskLeft.Location = new System.Drawing.Point(84, 19);
            this._numMaskLeft.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this._numMaskLeft.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this._numMaskLeft.Name = "_numMaskLeft";
            this._numMaskLeft.Size = new System.Drawing.Size(61, 20);
            this._numMaskLeft.TabIndex = 8;
            this._numMaskLeft.ValueChanged += new System.EventHandler(this._numMaskLeft_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(186, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Bottom:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(186, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Right:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Top:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Left:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this._numOriginY);
            this.groupBox4.Controls.Add(this._numOriginX);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Location = new System.Drawing.Point(12, 166);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(333, 45);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Origin";
            // 
            // _numOriginY
            // 
            this._numOriginY.Location = new System.Drawing.Point(266, 19);
            this._numOriginY.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this._numOriginY.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this._numOriginY.Name = "_numOriginY";
            this._numOriginY.Size = new System.Drawing.Size(61, 20);
            this._numOriginY.TabIndex = 13;
            this._numOriginY.ValueChanged += new System.EventHandler(this._numOriginY_ValueChanged);
            // 
            // _numOriginX
            // 
            this._numOriginX.Location = new System.Drawing.Point(84, 19);
            this._numOriginX.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this._numOriginX.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this._numOriginX.Name = "_numOriginX";
            this._numOriginX.Size = new System.Drawing.Size(61, 20);
            this._numOriginX.TabIndex = 12;
            this._numOriginX.ValueChanged += new System.EventHandler(this._numOriginX_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(186, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Y:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "X:";
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(475, 217);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 4;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this._buttonCancel_Click);
            // 
            // _buttonOK
            // 
            this._buttonOK.Location = new System.Drawing.Point(386, 217);
            this._buttonOK.Name = "_buttonOK";
            this._buttonOK.Size = new System.Drawing.Size(75, 23);
            this._buttonOK.TabIndex = 5;
            this._buttonOK.Text = "OK";
            this._buttonOK.UseVisualStyleBackColor = true;
            this._buttonOK.Click += new System.EventHandler(this._buttonOK_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel1.Location = new System.Drawing.Point(6, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(187, 174);
            this.panel1.TabIndex = 0;
            // 
            // ImportObject
            // 
            this.AcceptButton = this._buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(562, 252);
            this.Controls.Add(this._buttonOK);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportObject";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Import Object From Image";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numMaskBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numMaskRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numMaskTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numMaskLeft)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numOriginY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numOriginX)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _textSource;
        private System.Windows.Forms.Button _buttonBrowse;
        private System.Windows.Forms.TextBox _textObjectName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOK;
        private System.Windows.Forms.NumericUpDown _numMaskLeft;
        private System.Windows.Forms.NumericUpDown _numMaskBottom;
        private System.Windows.Forms.NumericUpDown _numMaskRight;
        private System.Windows.Forms.NumericUpDown _numMaskTop;
        private System.Windows.Forms.NumericUpDown _numOriginY;
        private System.Windows.Forms.NumericUpDown _numOriginX;
        private System.Windows.Forms.Panel panel1;
    }
}