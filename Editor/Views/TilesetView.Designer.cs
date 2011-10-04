namespace Editor
{
    partial class TilesetView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TilesetView));
            this.mainContainer = new System.Windows.Forms.SplitContainer();
            this.sideContainer = new System.Windows.Forms.SplitContainer();
            this.upperTabControl = new System.Windows.Forms.TabControl();
            this.tilePoolPage = new System.Windows.Forms.TabPage();
            this.tileTableMain = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.viewportControl1 = new Editor.ViewportControl();
            this.tilepoolControl = new Editor.TileControl1D();
            this.lowerTabControl = new System.Windows.Forms.TabControl();
            this.setListPage = new System.Windows.Forms.TabPage();
            this.viewportControl2 = new Editor.ViewportControl();
            this.tilesetControl = new Editor.TileControl2D();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolButtonSelect = new System.Windows.Forms.ToolStripButton();
            this.toolButtonDraw = new System.Windows.Forms.ToolStripButton();
            this.toolButtonErase = new System.Windows.Forms.ToolStripButton();
            this.toolButtonFill = new System.Windows.Forms.ToolStripButton();
            this.toolButtonStamp = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
            this.mainContainer.Panel1.SuspendLayout();
            this.mainContainer.Panel2.SuspendLayout();
            this.mainContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sideContainer)).BeginInit();
            this.sideContainer.Panel1.SuspendLayout();
            this.sideContainer.Panel2.SuspendLayout();
            this.sideContainer.SuspendLayout();
            this.upperTabControl.SuspendLayout();
            this.tilePoolPage.SuspendLayout();
            this.tileTableMain.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.lowerTabControl.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainContainer
            // 
            this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainer.Location = new System.Drawing.Point(0, 0);
            this.mainContainer.Margin = new System.Windows.Forms.Padding(0);
            this.mainContainer.Name = "mainContainer";
            // 
            // mainContainer.Panel1
            // 
            this.mainContainer.Panel1.Controls.Add(this.sideContainer);
            this.mainContainer.Panel1MinSize = 120;
            // 
            // mainContainer.Panel2
            // 
            this.mainContainer.Panel2.Controls.Add(this.viewportControl2);
            this.mainContainer.Panel2MinSize = 120;
            this.mainContainer.Size = new System.Drawing.Size(943, 612);
            this.mainContainer.SplitterDistance = 220;
            this.mainContainer.TabIndex = 4;
            this.mainContainer.TabStop = false;
            // 
            // sideContainer
            // 
            this.sideContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sideContainer.Location = new System.Drawing.Point(0, 0);
            this.sideContainer.Margin = new System.Windows.Forms.Padding(0);
            this.sideContainer.Name = "sideContainer";
            this.sideContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // sideContainer.Panel1
            // 
            this.sideContainer.Panel1.Controls.Add(this.upperTabControl);
            // 
            // sideContainer.Panel2
            // 
            this.sideContainer.Panel2.Controls.Add(this.lowerTabControl);
            this.sideContainer.Size = new System.Drawing.Size(220, 612);
            this.sideContainer.SplitterDistance = 343;
            this.sideContainer.TabIndex = 0;
            // 
            // upperTabControl
            // 
            this.upperTabControl.Controls.Add(this.tilePoolPage);
            this.upperTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.upperTabControl.Location = new System.Drawing.Point(0, 0);
            this.upperTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.upperTabControl.Name = "upperTabControl";
            this.upperTabControl.SelectedIndex = 0;
            this.upperTabControl.Size = new System.Drawing.Size(220, 343);
            this.upperTabControl.TabIndex = 0;
            // 
            // tilePoolPage
            // 
            this.tilePoolPage.Controls.Add(this.tileTableMain);
            this.tilePoolPage.Location = new System.Drawing.Point(4, 22);
            this.tilePoolPage.Margin = new System.Windows.Forms.Padding(0);
            this.tilePoolPage.Name = "tilePoolPage";
            this.tilePoolPage.Size = new System.Drawing.Size(212, 317);
            this.tilePoolPage.TabIndex = 0;
            this.tilePoolPage.Text = "Tile Pools";
            this.tilePoolPage.UseVisualStyleBackColor = true;
            // 
            // tileTableMain
            // 
            this.tileTableMain.ColumnCount = 1;
            this.tileTableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tileTableMain.Controls.Add(this.toolStrip2, 0, 0);
            this.tileTableMain.Controls.Add(this.viewportControl1, 0, 1);
            this.tileTableMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tileTableMain.Location = new System.Drawing.Point(0, 0);
            this.tileTableMain.Margin = new System.Windows.Forms.Padding(0);
            this.tileTableMain.Name = "tileTableMain";
            this.tileTableMain.RowCount = 2;
            this.tileTableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tileTableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tileTableMain.Size = new System.Drawing.Size(212, 317);
            this.tileTableMain.TabIndex = 1;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox1,
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripButton3});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Padding = new System.Windows.Forms.Padding(2);
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip2.Size = new System.Drawing.Size(212, 28);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.BackColor = System.Drawing.SystemColors.Window;
            this.toolStripComboBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(138, 24);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 21);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 21);
            this.toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 20);
            this.toolStripButton3.Text = "toolStripButton3";
            // 
            // viewportControl1
            // 
            this.viewportControl1.Control = this.tilepoolControl;
            this.viewportControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewportControl1.Location = new System.Drawing.Point(0, 28);
            this.viewportControl1.Margin = new System.Windows.Forms.Padding(0);
            this.viewportControl1.Name = "viewportControl1";
            this.viewportControl1.Size = new System.Drawing.Size(212, 289);
            this.viewportControl1.TabIndex = 1;
            // 
            // tilepoolControl
            // 
            this.tilepoolControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tilepoolControl.Location = new System.Drawing.Point(0, 0);
            this.tilepoolControl.Mode = Editor.TileControlMode.Click;
            this.tilepoolControl.Name = "tilepoolControl";
            this.tilepoolControl.SelectBoxBrush = null;
            this.tilepoolControl.Size = new System.Drawing.Size(195, 272);
            this.tilepoolControl.TabIndex = 1;
            this.tilepoolControl.Text = "tilepoolControl";
            this.tilepoolControl.TileSelectionBrush = null;
            this.tilepoolControl.TileSource = null;
            this.tilepoolControl.Zoom = 1F;
            // 
            // lowerTabControl
            // 
            this.lowerTabControl.Controls.Add(this.setListPage);
            this.lowerTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lowerTabControl.Location = new System.Drawing.Point(0, 0);
            this.lowerTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.lowerTabControl.Name = "lowerTabControl";
            this.lowerTabControl.SelectedIndex = 0;
            this.lowerTabControl.Size = new System.Drawing.Size(220, 265);
            this.lowerTabControl.TabIndex = 0;
            // 
            // setListPage
            // 
            this.setListPage.Location = new System.Drawing.Point(4, 22);
            this.setListPage.Name = "setListPage";
            this.setListPage.Padding = new System.Windows.Forms.Padding(3);
            this.setListPage.Size = new System.Drawing.Size(212, 239);
            this.setListPage.TabIndex = 0;
            this.setListPage.Text = "Tilesets";
            this.setListPage.UseVisualStyleBackColor = true;
            // 
            // viewportControl2
            // 
            this.viewportControl2.Control = this.tilesetControl;
            this.viewportControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewportControl2.Location = new System.Drawing.Point(0, 0);
            this.viewportControl2.Margin = new System.Windows.Forms.Padding(0);
            this.viewportControl2.Name = "viewportControl2";
            this.viewportControl2.Size = new System.Drawing.Size(719, 612);
            this.viewportControl2.TabIndex = 0;
            // 
            // tilesetControl
            // 
            this.tilesetControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tilesetControl.Location = new System.Drawing.Point(0, 0);
            this.tilesetControl.Mode = Editor.TileControlMode.Click;
            this.tilesetControl.Name = "tilesetControl";
            this.tilesetControl.SelectBoxBrush = null;
            this.tilesetControl.Size = new System.Drawing.Size(702, 595);
            this.tilesetControl.TabIndex = 0;
            this.tilesetControl.TileSelectionBrush = null;
            this.tilesetControl.TileSource = null;
            this.tilesetControl.Zoom = 1F;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "color-swatch.png");
            this.imageList1.Images.SetKeyName(1, "table.png");
            this.imageList1.Images.SetKeyName(2, "map.png");
            // 
            // toolButtonSelect
            // 
            this.toolButtonSelect.Checked = true;
            this.toolButtonSelect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolButtonSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonSelect.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonSelect.Image")));
            this.toolButtonSelect.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButtonSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonSelect.Name = "toolButtonSelect";
            this.toolButtonSelect.Size = new System.Drawing.Size(23, 22);
            this.toolButtonSelect.Text = "Select Tool";
            // 
            // toolButtonDraw
            // 
            this.toolButtonDraw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonDraw.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonDraw.Image")));
            this.toolButtonDraw.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButtonDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonDraw.Name = "toolButtonDraw";
            this.toolButtonDraw.Size = new System.Drawing.Size(23, 22);
            this.toolButtonDraw.Text = "Draw Tool";
            // 
            // toolButtonErase
            // 
            this.toolButtonErase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonErase.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonErase.Image")));
            this.toolButtonErase.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButtonErase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonErase.Name = "toolButtonErase";
            this.toolButtonErase.Size = new System.Drawing.Size(23, 22);
            this.toolButtonErase.Text = "Erase Tool";
            // 
            // toolButtonFill
            // 
            this.toolButtonFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonFill.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonFill.Image")));
            this.toolButtonFill.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButtonFill.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonFill.Name = "toolButtonFill";
            this.toolButtonFill.Size = new System.Drawing.Size(23, 22);
            this.toolButtonFill.Text = "Flood Fill Tool";
            // 
            // toolButtonStamp
            // 
            this.toolButtonStamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButtonStamp.Image = ((System.Drawing.Image)(resources.GetObject("toolButtonStamp.Image")));
            this.toolButtonStamp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButtonStamp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButtonStamp.Name = "toolButtonStamp";
            this.toolButtonStamp.Size = new System.Drawing.Size(23, 22);
            this.toolButtonStamp.Text = "Stamp Tool";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButtonSelect,
            this.toolButtonDraw,
            this.toolButtonErase,
            this.toolButtonFill,
            this.toolButtonStamp});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(127, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // TilesetView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainContainer);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TilesetView";
            this.Size = new System.Drawing.Size(943, 612);
            this.mainContainer.Panel1.ResumeLayout(false);
            this.mainContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).EndInit();
            this.mainContainer.ResumeLayout(false);
            this.sideContainer.Panel1.ResumeLayout(false);
            this.sideContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sideContainer)).EndInit();
            this.sideContainer.ResumeLayout(false);
            this.upperTabControl.ResumeLayout(false);
            this.tilePoolPage.ResumeLayout(false);
            this.tileTableMain.ResumeLayout(false);
            this.tileTableMain.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.lowerTabControl.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer mainContainer;
        private System.Windows.Forms.SplitContainer sideContainer;
        private System.Windows.Forms.TabControl upperTabControl;
        private System.Windows.Forms.TabPage tilePoolPage;
        private System.Windows.Forms.TableLayoutPanel tileTableMain;
        public System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.TabControl lowerTabControl;
        private System.Windows.Forms.TabPage setListPage;
        private ViewportControl viewportControl1;
        private TileControl1D tilepoolControl;
        private System.Windows.Forms.ImageList imageList1;
        private ViewportControl viewportControl2;
        private TileControl2D tilesetControl;
        private System.Windows.Forms.ToolStripButton toolButtonSelect;
        private System.Windows.Forms.ToolStripButton toolButtonDraw;
        private System.Windows.Forms.ToolStripButton toolButtonErase;
        private System.Windows.Forms.ToolStripButton toolButtonFill;
        private System.Windows.Forms.ToolStripButton toolButtonStamp;
        private System.Windows.Forms.ToolStrip toolStrip1;

    }
}
