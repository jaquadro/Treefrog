namespace Treefrog.Windows.Forms
{
    partial class ResizeLevelForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResizeLevelForm));
            this._fieldWidth = new System.Windows.Forms.NumericUpDown();
            this._fieldHeight = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._oldHeightLabel = new System.Windows.Forms.Label();
            this._oldWidthLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._fieldRight = new System.Windows.Forms.NumericUpDown();
            this._fieldLeft = new System.Windows.Forms.NumericUpDown();
            this._fieldBottom = new System.Windows.Forms.NumericUpDown();
            this._fieldTop = new System.Windows.Forms.NumericUpDown();
            this._checkResetOrigin = new System.Windows.Forms.CheckBox();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOK = new System.Windows.Forms.Button();
            this._alignmentControl = new Treefrog.Windows.Controls.AlignmentControl();
            ((System.ComponentModel.ISupportInitialize)(this._fieldWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._fieldHeight)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fieldRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._fieldLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._fieldBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._fieldTop)).BeginInit();
            this.SuspendLayout();
            // 
            // _fieldWidth
            // 
            this._fieldWidth.Location = new System.Drawing.Point(78, 71);
            this._fieldWidth.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._fieldWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._fieldWidth.Name = "_fieldWidth";
            this._fieldWidth.Size = new System.Drawing.Size(62, 20);
            this._fieldWidth.TabIndex = 0;
            this._fieldWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._fieldWidth.ValueChanged += new System.EventHandler(this._fieldWidth_ValueChanged);
            // 
            // _fieldHeight
            // 
            this._fieldHeight.Location = new System.Drawing.Point(78, 97);
            this._fieldHeight.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._fieldHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._fieldHeight.Name = "_fieldHeight";
            this._fieldHeight.Size = new System.Drawing.Size(62, 20);
            this._fieldHeight.TabIndex = 1;
            this._fieldHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._fieldHeight.ValueChanged += new System.EventHandler(this._fieldHeight_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._oldHeightLabel);
            this.groupBox1.Controls.Add(this._oldWidthLabel);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this._fieldWidth);
            this.groupBox1.Controls.Add(this._fieldHeight);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(146, 125);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Dimensions";
            // 
            // _oldHeightLabel
            // 
            this._oldHeightLabel.Location = new System.Drawing.Point(78, 47);
            this._oldHeightLabel.Name = "_oldHeightLabel";
            this._oldHeightLabel.Size = new System.Drawing.Size(62, 13);
            this._oldHeightLabel.TabIndex = 7;
            this._oldHeightLabel.Text = "0";
            this._oldHeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _oldWidthLabel
            // 
            this._oldWidthLabel.Location = new System.Drawing.Point(78, 21);
            this._oldWidthLabel.Name = "_oldWidthLabel";
            this._oldWidthLabel.Size = new System.Drawing.Size(62, 13);
            this._oldWidthLabel.TabIndex = 6;
            this._oldWidthLabel.Text = "0";
            this._oldWidthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 47);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Old Height:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Old Width:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "New Width:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "New Height:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._alignmentControl);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this._fieldRight);
            this.groupBox2.Controls.Add(this._fieldLeft);
            this.groupBox2.Controls.Add(this._fieldBottom);
            this.groupBox2.Controls.Add(this._fieldTop);
            this.groupBox2.Location = new System.Drawing.Point(164, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(140, 207);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Placement";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Right:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Left:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Bottom:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Top:";
            // 
            // _fieldRight
            // 
            this._fieldRight.Location = new System.Drawing.Point(72, 97);
            this._fieldRight.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._fieldRight.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this._fieldRight.Name = "_fieldRight";
            this._fieldRight.Size = new System.Drawing.Size(62, 20);
            this._fieldRight.TabIndex = 4;
            this._fieldRight.ValueChanged += new System.EventHandler(this._fieldRight_ValueChanged);
            // 
            // _fieldLeft
            // 
            this._fieldLeft.Location = new System.Drawing.Point(72, 71);
            this._fieldLeft.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._fieldLeft.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this._fieldLeft.Name = "_fieldLeft";
            this._fieldLeft.Size = new System.Drawing.Size(62, 20);
            this._fieldLeft.TabIndex = 4;
            this._fieldLeft.ValueChanged += new System.EventHandler(this._fieldLeft_ValueChanged);
            // 
            // _fieldBottom
            // 
            this._fieldBottom.Location = new System.Drawing.Point(72, 45);
            this._fieldBottom.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._fieldBottom.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this._fieldBottom.Name = "_fieldBottom";
            this._fieldBottom.Size = new System.Drawing.Size(62, 20);
            this._fieldBottom.TabIndex = 4;
            this._fieldBottom.ValueChanged += new System.EventHandler(this._fieldBottom_ValueChanged);
            // 
            // _fieldTop
            // 
            this._fieldTop.Location = new System.Drawing.Point(72, 19);
            this._fieldTop.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._fieldTop.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this._fieldTop.Name = "_fieldTop";
            this._fieldTop.Size = new System.Drawing.Size(62, 20);
            this._fieldTop.TabIndex = 4;
            this._fieldTop.ValueChanged += new System.EventHandler(this._fieldTop_ValueChanged);
            // 
            // _checkResetOrigin
            // 
            this._checkResetOrigin.AutoSize = true;
            this._checkResetOrigin.Location = new System.Drawing.Point(13, 144);
            this._checkResetOrigin.Name = "_checkResetOrigin";
            this._checkResetOrigin.Size = new System.Drawing.Size(118, 17);
            this._checkResetOrigin.TabIndex = 4;
            this._checkResetOrigin.Text = "Reset origin to (0,0)";
            this._checkResetOrigin.UseVisualStyleBackColor = true;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(228, 225);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 5;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this._buttonCancel_Click);
            // 
            // _buttonOK
            // 
            this._buttonOK.Location = new System.Drawing.Point(147, 225);
            this._buttonOK.Name = "_buttonOK";
            this._buttonOK.Size = new System.Drawing.Size(75, 23);
            this._buttonOK.TabIndex = 6;
            this._buttonOK.Text = "OK";
            this._buttonOK.UseVisualStyleBackColor = true;
            this._buttonOK.Click += new System.EventHandler(this._buttonOK_Click);
            // 
            // _alignmentControl
            // 
            this._alignmentControl.Alignment = Treefrog.Windows.Controls.Alignment.None;
            this._alignmentControl.Location = new System.Drawing.Point(32, 125);
            this._alignmentControl.Name = "_alignmentControl";
            this._alignmentControl.NewSize = new System.Drawing.Size(0, 0);
            this._alignmentControl.OldSize = new System.Drawing.Size(0, 0);
            this._alignmentControl.Size = new System.Drawing.Size(76, 76);
            this._alignmentControl.SourceIcon = null;
            this._alignmentControl.TabIndex = 9;
            // 
            // ResizeLevelForm
            // 
            this.AcceptButton = this._buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(315, 260);
            this.Controls.Add(this._buttonOK);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._checkResetOrigin);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResizeLevelForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Change Level Size";
            ((System.ComponentModel.ISupportInitialize)(this._fieldWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._fieldHeight)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fieldRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._fieldLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._fieldBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._fieldTop)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown _fieldWidth;
        private System.Windows.Forms.NumericUpDown _fieldHeight;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label _oldHeightLabel;
        private System.Windows.Forms.Label _oldWidthLabel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown _fieldRight;
        private System.Windows.Forms.NumericUpDown _fieldLeft;
        private System.Windows.Forms.NumericUpDown _fieldBottom;
        private System.Windows.Forms.NumericUpDown _fieldTop;
        private System.Windows.Forms.CheckBox _checkResetOrigin;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOK;
        private Controls.AlignmentControl _alignmentControl;
    }
}