namespace Treefrog.Windows.Controls
{
    partial class AlignmentControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent ()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlignmentControl));
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._buttonAlignBottomRight = new System.Windows.Forms.Button();
            this._buttonAlignBottomLeft = new System.Windows.Forms.Button();
            this._buttonAlignBottom = new System.Windows.Forms.Button();
            this._buttonAlignTopLeft = new System.Windows.Forms.Button();
            this._buttonAlignTop = new System.Windows.Forms.Button();
            this._buttonAlignRight = new System.Windows.Forms.Button();
            this._buttonAlignCenter = new System.Windows.Forms.Button();
            this._buttonAlignLeft = new System.Windows.Forms.Button();
            this._buttonAlignTopRight = new System.Windows.Forms.Button();
            this._arrowImages = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignBottomRight, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignBottomLeft, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignBottom, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignTopLeft, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignTop, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignRight, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignCenter, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignLeft, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this._buttonAlignTopRight, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(76, 76);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // _buttonAlignBottomRight
            // 
            this._buttonAlignBottomRight.Location = new System.Drawing.Point(51, 51);
            this._buttonAlignBottomRight.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignBottomRight.Name = "_buttonAlignBottomRight";
            this._buttonAlignBottomRight.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignBottomRight.TabIndex = 7;
            this._buttonAlignBottomRight.UseVisualStyleBackColor = true;
            this._buttonAlignBottomRight.Click += new System.EventHandler(this._buttonAlignBottomRight_Click);
            // 
            // _buttonAlignBottomLeft
            // 
            this._buttonAlignBottomLeft.Location = new System.Drawing.Point(1, 51);
            this._buttonAlignBottomLeft.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignBottomLeft.Name = "_buttonAlignBottomLeft";
            this._buttonAlignBottomLeft.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignBottomLeft.TabIndex = 8;
            this._buttonAlignBottomLeft.UseVisualStyleBackColor = true;
            this._buttonAlignBottomLeft.Click += new System.EventHandler(this._buttonAlignBottomLeft_Click);
            // 
            // _buttonAlignBottom
            // 
            this._buttonAlignBottom.Location = new System.Drawing.Point(26, 51);
            this._buttonAlignBottom.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignBottom.Name = "_buttonAlignBottom";
            this._buttonAlignBottom.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignBottom.TabIndex = 8;
            this._buttonAlignBottom.UseVisualStyleBackColor = true;
            this._buttonAlignBottom.Click += new System.EventHandler(this._buttonAlignBottom_Click);
            // 
            // _buttonAlignTopLeft
            // 
            this._buttonAlignTopLeft.Location = new System.Drawing.Point(1, 1);
            this._buttonAlignTopLeft.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignTopLeft.Name = "_buttonAlignTopLeft";
            this._buttonAlignTopLeft.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignTopLeft.TabIndex = 7;
            this._buttonAlignTopLeft.UseVisualStyleBackColor = true;
            this._buttonAlignTopLeft.Click += new System.EventHandler(this._buttonAlignTopLeft_Click);
            // 
            // _buttonAlignTop
            // 
            this._buttonAlignTop.Location = new System.Drawing.Point(26, 1);
            this._buttonAlignTop.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignTop.Name = "_buttonAlignTop";
            this._buttonAlignTop.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignTop.TabIndex = 7;
            this._buttonAlignTop.UseVisualStyleBackColor = true;
            this._buttonAlignTop.Click += new System.EventHandler(this._buttonAlignTop_Click);
            // 
            // _buttonAlignRight
            // 
            this._buttonAlignRight.Location = new System.Drawing.Point(51, 26);
            this._buttonAlignRight.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignRight.Name = "_buttonAlignRight";
            this._buttonAlignRight.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignRight.TabIndex = 4;
            this._buttonAlignRight.UseVisualStyleBackColor = true;
            this._buttonAlignRight.Click += new System.EventHandler(this._buttonAlignRight_Click);
            // 
            // _buttonAlignCenter
            // 
            this._buttonAlignCenter.Location = new System.Drawing.Point(26, 26);
            this._buttonAlignCenter.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignCenter.Name = "_buttonAlignCenter";
            this._buttonAlignCenter.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignCenter.TabIndex = 5;
            this._buttonAlignCenter.UseVisualStyleBackColor = true;
            this._buttonAlignCenter.Click += new System.EventHandler(this._buttonAlignCenter_Click);
            // 
            // _buttonAlignLeft
            // 
            this._buttonAlignLeft.Location = new System.Drawing.Point(1, 26);
            this._buttonAlignLeft.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignLeft.Name = "_buttonAlignLeft";
            this._buttonAlignLeft.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignLeft.TabIndex = 7;
            this._buttonAlignLeft.UseVisualStyleBackColor = true;
            this._buttonAlignLeft.Click += new System.EventHandler(this._buttonAlignLeft_Click);
            // 
            // _buttonAlignTopRight
            // 
            this._buttonAlignTopRight.Location = new System.Drawing.Point(51, 1);
            this._buttonAlignTopRight.Margin = new System.Windows.Forms.Padding(1);
            this._buttonAlignTopRight.Name = "_buttonAlignTopRight";
            this._buttonAlignTopRight.Size = new System.Drawing.Size(23, 23);
            this._buttonAlignTopRight.TabIndex = 7;
            this._buttonAlignTopRight.UseVisualStyleBackColor = true;
            this._buttonAlignTopRight.Click += new System.EventHandler(this._buttonAlignTopRight_Click);
            // 
            // _arrowImages
            // 
            this._arrowImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_arrowImages.ImageStream")));
            this._arrowImages.TransparentColor = System.Drawing.Color.Transparent;
            this._arrowImages.Images.SetKeyName(0, "arrow-135.png");
            this._arrowImages.Images.SetKeyName(1, "arrow-090.png");
            this._arrowImages.Images.SetKeyName(2, "arrow-045.png");
            this._arrowImages.Images.SetKeyName(3, "arrow-180.png");
            this._arrowImages.Images.SetKeyName(4, "arrow.png");
            this._arrowImages.Images.SetKeyName(5, "arrow-225.png");
            this._arrowImages.Images.SetKeyName(6, "arrow-270.png");
            this._arrowImages.Images.SetKeyName(7, "arrow-315.png");
            // 
            // AlignmentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "AlignmentControl";
            this.Size = new System.Drawing.Size(76, 76);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button _buttonAlignBottomRight;
        private System.Windows.Forms.Button _buttonAlignBottomLeft;
        private System.Windows.Forms.Button _buttonAlignBottom;
        private System.Windows.Forms.Button _buttonAlignTopLeft;
        private System.Windows.Forms.Button _buttonAlignTop;
        private System.Windows.Forms.Button _buttonAlignRight;
        private System.Windows.Forms.Button _buttonAlignCenter;
        private System.Windows.Forms.Button _buttonAlignLeft;
        private System.Windows.Forms.Button _buttonAlignTopRight;
        private System.Windows.Forms.ImageList _arrowImages;
    }
}
