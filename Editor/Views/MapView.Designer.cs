using Editor.Model.Controls;
namespace Editor
{
    partial class MapView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapView));
            this.mainContainer = new System.Windows.Forms.SplitContainer();
            this.sideContainer = new System.Windows.Forms.SplitContainer();
            this.upperTabControl = new System.Windows.Forms.TabControl();
            this.tilesetPage = new System.Windows.Forms.TabPage();
            this.tileTableMain = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.viewportControl1 = new Editor.ViewportControl();
            this.tilepoolPage = new System.Windows.Forms.TabPage();
            this._tilePoolPane = new Editor.TilePoolPane();
            this.objectPage = new System.Windows.Forms.TabPage();
            this.lowerTabControl = new System.Windows.Forms.TabControl();
            this.layerPage = new System.Windows.Forms.TabPage();
            this._layerPane = new Editor.Views.LayerPane();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.propertyPane1 = new Editor.Views.PropertyPane();
            this.tabControl1 = new TabControlEx();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.viewportControl2 = new Editor.ViewportControl();
            this.tilemapControl = new Editor.Model.Controls.LayerControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
            this.mainContainer.Panel1.SuspendLayout();
            this.mainContainer.Panel2.SuspendLayout();
            this.mainContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sideContainer)).BeginInit();
            this.sideContainer.Panel1.SuspendLayout();
            this.sideContainer.Panel2.SuspendLayout();
            this.sideContainer.SuspendLayout();
            this.upperTabControl.SuspendLayout();
            this.tilesetPage.SuspendLayout();
            this.tileTableMain.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tilepoolPage.SuspendLayout();
            this.lowerTabControl.SuspendLayout();
            this.layerPage.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
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
            // 
            // mainContainer.Panel2
            // 
            this.mainContainer.Panel2.Controls.Add(this.tabControl1);
            this.mainContainer.Size = new System.Drawing.Size(943, 612);
            this.mainContainer.SplitterDistance = 225;
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
            this.sideContainer.Size = new System.Drawing.Size(225, 612);
            this.sideContainer.SplitterDistance = 343;
            this.sideContainer.TabIndex = 0;
            // 
            // upperTabControl
            // 
            this.upperTabControl.Controls.Add(this.tilesetPage);
            this.upperTabControl.Controls.Add(this.tilepoolPage);
            this.upperTabControl.Controls.Add(this.objectPage);
            this.upperTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.upperTabControl.Location = new System.Drawing.Point(0, 0);
            this.upperTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.upperTabControl.Name = "upperTabControl";
            this.upperTabControl.SelectedIndex = 0;
            this.upperTabControl.Size = new System.Drawing.Size(225, 343);
            this.upperTabControl.TabIndex = 0;
            // 
            // tilesetPage
            // 
            this.tilesetPage.Controls.Add(this.tileTableMain);
            this.tilesetPage.Location = new System.Drawing.Point(4, 22);
            this.tilesetPage.Margin = new System.Windows.Forms.Padding(0);
            this.tilesetPage.Name = "tilesetPage";
            this.tilesetPage.Size = new System.Drawing.Size(217, 317);
            this.tilesetPage.TabIndex = 0;
            this.tilesetPage.Text = "Tile Sets";
            this.tilesetPage.UseVisualStyleBackColor = true;
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
            this.tileTableMain.Size = new System.Drawing.Size(217, 317);
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
            this.toolStrip2.Size = new System.Drawing.Size(217, 28);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.BackColor = System.Drawing.SystemColors.Window;
            this.toolStripComboBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(150, 24);
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
            this.toolStripButton2.Size = new System.Drawing.Size(23, 20);
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
            this.viewportControl1.Control = null;
            this.viewportControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewportControl1.Location = new System.Drawing.Point(0, 28);
            this.viewportControl1.Margin = new System.Windows.Forms.Padding(0);
            this.viewportControl1.Name = "viewportControl1";
            this.viewportControl1.Size = new System.Drawing.Size(217, 289);
            this.viewportControl1.TabIndex = 1;
            // 
            // tilepoolPage
            // 
            this.tilepoolPage.Controls.Add(this._tilePoolPane);
            this.tilepoolPage.Location = new System.Drawing.Point(4, 22);
            this.tilepoolPage.Name = "tilepoolPage";
            this.tilepoolPage.Padding = new System.Windows.Forms.Padding(0, 0, 2, 1);
            this.tilepoolPage.Size = new System.Drawing.Size(217, 317);
            this.tilepoolPage.TabIndex = 2;
            this.tilepoolPage.Text = "Tile Pools";
            this.tilepoolPage.UseVisualStyleBackColor = true;
            // 
            // _tilePoolPane
            // 
            this._tilePoolPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tilePoolPane.Location = new System.Drawing.Point(0, 0);
            this._tilePoolPane.Margin = new System.Windows.Forms.Padding(0);
            this._tilePoolPane.Name = "_tilePoolPane";
            this._tilePoolPane.Size = new System.Drawing.Size(215, 316);
            this._tilePoolPane.TabIndex = 0;
            // 
            // objectPage
            // 
            this.objectPage.Location = new System.Drawing.Point(4, 22);
            this.objectPage.Name = "objectPage";
            this.objectPage.Size = new System.Drawing.Size(217, 317);
            this.objectPage.TabIndex = 1;
            this.objectPage.Text = "Objects";
            this.objectPage.UseVisualStyleBackColor = true;
            // 
            // lowerTabControl
            // 
            this.lowerTabControl.Controls.Add(this.layerPage);
            this.lowerTabControl.Controls.Add(this.tabPage1);
            this.lowerTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lowerTabControl.Location = new System.Drawing.Point(0, 0);
            this.lowerTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.lowerTabControl.Name = "lowerTabControl";
            this.lowerTabControl.SelectedIndex = 0;
            this.lowerTabControl.Size = new System.Drawing.Size(225, 265);
            this.lowerTabControl.TabIndex = 0;
            // 
            // layerPage
            // 
            this.layerPage.Controls.Add(this._layerPane);
            this.layerPage.Location = new System.Drawing.Point(4, 22);
            this.layerPage.Margin = new System.Windows.Forms.Padding(0);
            this.layerPage.Name = "layerPage";
            this.layerPage.Padding = new System.Windows.Forms.Padding(0, 0, 2, 1);
            this.layerPage.Size = new System.Drawing.Size(217, 239);
            this.layerPage.TabIndex = 0;
            this.layerPage.Text = "Layers";
            this.layerPage.UseVisualStyleBackColor = true;
            // 
            // _layerPane
            // 
            this._layerPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layerPane.Location = new System.Drawing.Point(0, 0);
            this._layerPane.Margin = new System.Windows.Forms.Padding(0);
            this._layerPane.Name = "_layerPane";
            this._layerPane.Size = new System.Drawing.Size(215, 238);
            this._layerPane.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.propertyPane1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 1);
            this.tabPage1.Size = new System.Drawing.Size(217, 239);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "Properties";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // propertyPane1
            // 
            this.propertyPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyPane1.Location = new System.Drawing.Point(0, 0);
            this.propertyPane1.Margin = new System.Windows.Forms.Padding(0);
            this.propertyPane1.Name = "propertyPane1";
            this.propertyPane1.PropertyProvider = null;
            this.propertyPane1.Size = new System.Drawing.Size(215, 238);
            this.propertyPane1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(714, 612);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.viewportControl2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.tabPage2.Size = new System.Drawing.Size(706, 586);
            this.tabPage2.TabIndex = 0;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // viewportControl2
            // 
            this.viewportControl2.Control = this.tilemapControl;
            this.viewportControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewportControl2.Location = new System.Drawing.Point(0, 0);
            this.viewportControl2.Margin = new System.Windows.Forms.Padding(0);
            this.viewportControl2.Name = "viewportControl2";
            this.viewportControl2.Size = new System.Drawing.Size(706, 585);
            this.viewportControl2.TabIndex = 0;
            // 
            // tilemapControl
            // 
            this.tilemapControl.Alignment = Editor.Model.Controls.LayerControlAlignment.Center;
            this.tilemapControl.CanAutoScroll = false;
            this.tilemapControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tilemapControl.HeightSynced = false;
            this.tilemapControl.Location = new System.Drawing.Point(0, 0);
            this.tilemapControl.Margin = new System.Windows.Forms.Padding(0);
            this.tilemapControl.Name = "tilemapControl";
            this.tilemapControl.Size = new System.Drawing.Size(689, 568);
            this.tilemapControl.TabIndex = 0;
            this.tilemapControl.Text = "tilemapControl";
            this.tilemapControl.WidthSynced = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(706, 586);
            this.tabPage3.TabIndex = 1;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // MapView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainContainer);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "MapView";
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
            this.tilesetPage.ResumeLayout(false);
            this.tileTableMain.ResumeLayout(false);
            this.tileTableMain.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tilepoolPage.ResumeLayout(false);
            this.lowerTabControl.ResumeLayout(false);
            this.layerPage.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer mainContainer;
        private System.Windows.Forms.SplitContainer sideContainer;
        private System.Windows.Forms.TabControl upperTabControl;
        private System.Windows.Forms.TabPage tilesetPage;
        private System.Windows.Forms.TableLayoutPanel tileTableMain;
        public System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.TabControl lowerTabControl;
        private System.Windows.Forms.TabPage layerPage;
        private ViewportControl viewportControl1;
        //private TileControl2D tilesetControl;
        private ViewportControl viewportControl2;
        private LayerControl tilemapControl;
        private System.Windows.Forms.TabPage objectPage;
        private System.Windows.Forms.TabPage tilepoolPage;
        private TilePoolPane _tilePoolPane;
        private Views.LayerPane _layerPane;
        private System.Windows.Forms.TabPage tabPage1;
        private Views.PropertyPane propertyPane1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;

    }
}
