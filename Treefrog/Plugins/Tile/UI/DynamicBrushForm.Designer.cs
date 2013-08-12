using Treefrog.Windows;
namespace Treefrog.Plugins.Tiles.UI
{
    partial class DynamicBrushForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent ()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DynamicBrushForm));
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._tilePanel = new Treefrog.Plugins.Tiles.UI.TilePoolPane();
            this._toggleErase = new System.Windows.Forms.CheckBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this._toggleDraw = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this._tileSizeList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._prototypeList = new System.Windows.Forms.ComboBox();
            this._nameField = new System.Windows.Forms.TextBox();
            this._layerControl = new Treefrog.Windows.Controls.LayerGraphicsControl();
            this._ViewportControl = new Treefrog.Windows.Controls.ViewportControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.Location = new System.Drawing.Point(444, 430);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this._buttonCancel_Click);
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Enabled = false;
            this._buttonOk.Location = new System.Drawing.Point(363, 430);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 2;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this._buttonOk_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._tilePanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this._toggleErase);
            this.splitContainer1.Panel2.Controls.Add(this._toggleDraw);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this._buttonOk);
            this.splitContainer1.Panel2.Controls.Add(this._buttonCancel);
            this.splitContainer1.Size = new System.Drawing.Size(771, 465);
            this.splitContainer1.SplitterDistance = 236;
            this.splitContainer1.TabIndex = 0;
            // 
            // _tilePanel
            // 
            this._tilePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tilePanel.Location = new System.Drawing.Point(0, 0);
            this._tilePanel.Name = "_tilePanel";
            this._tilePanel.Size = new System.Drawing.Size(236, 465);
            this._tilePanel.TabIndex = 0;
            // 
            // _toggleErase
            // 
            this._toggleErase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._toggleErase.Appearance = System.Windows.Forms.Appearance.Button;
            this._toggleErase.AutoCheck = false;
            this._toggleErase.AutoSize = true;
            this._toggleErase.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._toggleErase.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this._toggleErase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._toggleErase.ImageIndex = 1;
            this._toggleErase.ImageList = this.imageList1;
            this._toggleErase.Location = new System.Drawing.Point(31, 430);
            this._toggleErase.Name = "_toggleErase";
            this._toggleErase.Size = new System.Drawing.Size(22, 22);
            this._toggleErase.TabIndex = 8;
            this._toggleErase.UseVisualStyleBackColor = true;
            this._toggleErase.Click += new System.EventHandler(this._toggleErase_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "paint-brush16.png");
            this.imageList1.Images.SetKeyName(1, "eraser16.png");
            // 
            // _toggleDraw
            // 
            this._toggleDraw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._toggleDraw.Appearance = System.Windows.Forms.Appearance.Button;
            this._toggleDraw.AutoCheck = false;
            this._toggleDraw.AutoSize = true;
            this._toggleDraw.Checked = true;
            this._toggleDraw.CheckState = System.Windows.Forms.CheckState.Checked;
            this._toggleDraw.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            this._toggleDraw.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this._toggleDraw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._toggleDraw.ImageIndex = 0;
            this._toggleDraw.ImageList = this.imageList1;
            this._toggleDraw.Location = new System.Drawing.Point(3, 430);
            this._toggleDraw.Name = "_toggleDraw";
            this._toggleDraw.Size = new System.Drawing.Size(22, 22);
            this._toggleDraw.TabIndex = 7;
            this._toggleDraw.UseVisualStyleBackColor = true;
            this._toggleDraw.Click += new System.EventHandler(this._toggleDraw_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._ViewportControl);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this._tileSizeList);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._prototypeList);
            this.groupBox1.Controls.Add(this._nameField);
            this.groupBox1.Location = new System.Drawing.Point(3, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(516, 412);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Brush Detail";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(288, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Tile Size:";
            // 
            // _tileSizeList
            // 
            this._tileSizeList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tileSizeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._tileSizeList.FormattingEnabled = true;
            this._tileSizeList.Location = new System.Drawing.Point(379, 46);
            this._tileSizeList.Name = "_tileSizeList";
            this._tileSizeList.Size = new System.Drawing.Size(131, 21);
            this._tileSizeList.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(288, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Brush Prototype:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Name:";
            // 
            // _prototypeList
            // 
            this._prototypeList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._prototypeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._prototypeList.FormattingEnabled = true;
            this._prototypeList.Location = new System.Drawing.Point(379, 19);
            this._prototypeList.Name = "_prototypeList";
            this._prototypeList.Size = new System.Drawing.Size(131, 21);
            this._prototypeList.TabIndex = 4;
            // 
            // _nameField
            // 
            this._nameField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._nameField.Location = new System.Drawing.Point(80, 19);
            this._nameField.Name = "_nameField";
            this._nameField.Size = new System.Drawing.Size(177, 20);
            this._nameField.TabIndex = 3;
            // 
            // _layerControl
            // 
            this._layerControl.CanvasAlignment = Treefrog.Presentation.CanvasAlignment.Center;
            this._layerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layerControl.HeightSynced = false;
            this._layerControl.Location = new System.Drawing.Point(0, 0);
            this._layerControl.Name = "_layerControl";
            this._layerControl.Size = new System.Drawing.Size(487, 316);
            this._layerControl.TabIndex = 0;
            this._layerControl.Text = "layerControl1";
            this._layerControl.WidthSynced = false;
            // 
            // _ViewportControl
            // 
            this._ViewportControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ViewportControl.Control = this._layerControl;
            this._ViewportControl.Location = new System.Drawing.Point(6, 73);
            this._ViewportControl.Name = "_ViewportControl";
            this._ViewportControl.Size = new System.Drawing.Size(504, 333);
            this._ViewportControl.TabIndex = 9;
            // 
            // DynamicBrushForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 465);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DynamicBrushForm";
            this.Text = "Edit Dynamic Tile Brush";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Treefrog.Windows.Controls.LayerGraphicsControl _layerControl;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private TilePoolPane _tilePanel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox _tileSizeList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _prototypeList;
        private System.Windows.Forms.TextBox _nameField;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.CheckBox _toggleErase;
        private System.Windows.Forms.CheckBox _toggleDraw;
        private Treefrog.Windows.Controls.ViewportControl _ViewportControl;
    }
}