namespace Treefrog.Plugins.Object.UI
{
    partial class ObjectLayerForm
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
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this._gridColorButton = new Treefrog.Windows.Controls.WinEx.ColorButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._gridWidthField = new System.Windows.Forms.NumericUpDown();
            this._gridHeightField = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._opacityField = new System.Windows.Forms.NumericUpDown();
            this._opacitySlider = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._nameField = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridWidthField)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridHeightField)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._opacityField)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._opacitySlider)).BeginInit();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Location = new System.Drawing.Point(128, 172);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 7;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this._okButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(209, 172);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this._gridColorButton);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this._gridWidthField);
            this.groupBox2.Controls.Add(this._gridHeightField);
            this.groupBox2.Location = new System.Drawing.Point(12, 92);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(272, 74);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Grid";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Grid Color:";
            // 
            // _gridColorButton
            // 
            this._gridColorButton.Color = System.Drawing.Color.Black;
            this._gridColorButton.Location = new System.Drawing.Point(208, 45);
            this._gridColorButton.Name = "_gridColorButton";
            this._gridColorButton.Size = new System.Drawing.Size(58, 23);
            this._gridColorButton.TabIndex = 3;
            this._gridColorButton.Text = "colorButton1";
            this._gridColorButton.UseVisualStyleBackColor = true;
            this._gridColorButton.Click += new System.EventHandler(this._gridColorButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(153, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Height:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Width:";
            // 
            // _gridWidthField
            // 
            this._gridWidthField.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this._gridWidthField.Location = new System.Drawing.Point(62, 19);
            this._gridWidthField.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this._gridWidthField.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._gridWidthField.Name = "_gridWidthField";
            this._gridWidthField.Size = new System.Drawing.Size(58, 20);
            this._gridWidthField.TabIndex = 1;
            this._gridWidthField.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // _gridHeightField
            // 
            this._gridHeightField.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this._gridHeightField.Location = new System.Drawing.Point(208, 19);
            this._gridHeightField.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this._gridHeightField.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._gridHeightField.Name = "_gridHeightField";
            this._gridHeightField.Size = new System.Drawing.Size(58, 20);
            this._gridHeightField.TabIndex = 0;
            this._gridHeightField.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._opacityField);
            this.groupBox1.Controls.Add(this._opacitySlider);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._nameField);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 74);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Layer Details";
            // 
            // _opacityField
            // 
            this._opacityField.DecimalPlaces = 2;
            this._opacityField.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this._opacityField.Location = new System.Drawing.Point(208, 45);
            this._opacityField.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._opacityField.Name = "_opacityField";
            this._opacityField.Size = new System.Drawing.Size(58, 20);
            this._opacityField.TabIndex = 4;
            this._opacityField.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._opacityField.ValueChanged += new System.EventHandler(this._opacityField_ValueChanged);
            // 
            // _opacitySlider
            // 
            this._opacitySlider.AutoSize = false;
            this._opacitySlider.LargeChange = 10;
            this._opacitySlider.Location = new System.Drawing.Point(62, 45);
            this._opacitySlider.Maximum = 100;
            this._opacitySlider.Name = "_opacitySlider";
            this._opacitySlider.Size = new System.Drawing.Size(142, 23);
            this._opacitySlider.TabIndex = 3;
            this._opacitySlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this._opacitySlider.Value = 100;
            this._opacitySlider.Scroll += new System.EventHandler(this._opacitySlider_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Opacity:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name:";
            // 
            // _nameField
            // 
            this._nameField.Location = new System.Drawing.Point(62, 19);
            this._nameField.Name = "_nameField";
            this._nameField.Size = new System.Drawing.Size(204, 20);
            this._nameField.TabIndex = 0;
            // 
            // ObjectLayerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 206);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ObjectLayerForm";
            this.Text = "Object Layer";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridWidthField)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridHeightField)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._opacityField)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._opacitySlider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private Treefrog.Windows.Controls.WinEx.ColorButton _gridColorButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown _gridWidthField;
        private System.Windows.Forms.NumericUpDown _gridHeightField;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown _opacityField;
        private System.Windows.Forms.TrackBar _opacitySlider;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _nameField;
    }
}