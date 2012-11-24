using Treefrog.Windows.Controls.WinEx;

namespace Treefrog.Windows.Forms
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusCoord = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusZoomText = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusZoomOut = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusZoomIn = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this._tabTilePools = new System.Windows.Forms.TabPage();
            this.tilePoolPane1 = new Treefrog.Windows.TilePoolPane();
            this._tabObjects = new System.Windows.Forms.TabPage();
            this.objectPanel1 = new Treefrog.Windows.Panels.ObjectPanel();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this._tabLayers = new System.Windows.Forms.TabPage();
            this.layerPane1 = new Treefrog.Windows.LayerPane();
            this._tabProperties = new System.Windows.Forms.TabPage();
            this.propertyPane1 = new Treefrog.Windows.PropertyPane();
            this.tabControlEx1 = new Treefrog.Windows.Controls.WinEx.TabControlEx();
            this.menuBar.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this._tabTilePools.SuspendLayout();
            this._tabObjects.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this._tabLayers.SuspendLayout();
            this._tabProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuBar
            // 
            this.menuBar.BackColor = System.Drawing.SystemColors.MenuBar;
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuBar.Size = new System.Drawing.Size(1024, 24);
            this.menuBar.TabIndex = 1;
            this.menuBar.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator1,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator2,
            this.selectAllToolStripMenuItem,
            this.selectNoneToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.undoToolStripMenuItem.Text = "&Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.redoToolStripMenuItem.Text = "&Redo";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(208, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.cutToolStripMenuItem.Text = "Cu&t";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.pasteToolStripMenuItem.Text = "&Paste";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.deleteToolStripMenuItem.Text = "&Delete";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(208, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            // 
            // selectNoneToolStripMenuItem
            // 
            this.selectNoneToolStripMenuItem.Name = "selectNoneToolStripMenuItem";
            this.selectNoneToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.A)));
            this.selectNoneToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.selectNoneToolStripMenuItem.Text = "Select &None";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusCoord,
            this.toolStripStatusLabel1,
            this.statusZoomText,
            this.statusZoomOut,
            this.statusZoomIn});
            this.statusBar.Location = new System.Drawing.Point(0, 628);
            this.statusBar.Name = "statusBar";
            this.statusBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusBar.Size = new System.Drawing.Size(1024, 24);
            this.statusBar.TabIndex = 2;
            this.statusBar.Text = "statusStrip1";
            // 
            // statusCoord
            // 
            this.statusCoord.AutoSize = false;
            this.statusCoord.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statusCoord.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusCoord.Name = "statusCoord";
            this.statusCoord.Size = new System.Drawing.Size(70, 19);
            this.statusCoord.Text = "0, 0";
            this.statusCoord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(858, 19);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusZoomText
            // 
            this.statusZoomText.AutoSize = false;
            this.statusZoomText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusZoomText.Margin = new System.Windows.Forms.Padding(6, 3, 0, 2);
            this.statusZoomText.Name = "statusZoomText";
            this.statusZoomText.Size = new System.Drawing.Size(35, 19);
            this.statusZoomText.Text = "100%";
            // 
            // statusZoomOut
            // 
            this.statusZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusZoomOut.Name = "statusZoomOut";
            this.statusZoomOut.Size = new System.Drawing.Size(23, 19);
            this.statusZoomOut.Text = "ZO";
            // 
            // statusZoomIn
            // 
            this.statusZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusZoomIn.Name = "statusZoomIn";
            this.statusZoomIn.Size = new System.Drawing.Size(17, 19);
            this.statusZoomIn.Text = "ZI";
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1024, 603);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // toolStripContainer1.LeftToolStripPanel
            // 
            this.toolStripContainer1.LeftToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.RightToolStripPanel
            // 
            this.toolStripContainer1.RightToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripContainer1.Size = new System.Drawing.Size(1024, 628);
            this.toolStripContainer1.TabIndex = 6;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControlEx1);
            this.splitContainer1.Size = new System.Drawing.Size(1024, 603);
            this.splitContainer1.SplitterDistance = 291;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.TabStop = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabControl2);
            this.splitContainer2.Size = new System.Drawing.Size(291, 603);
            this.splitContainer2.SplitterDistance = 294;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this._tabTilePools);
            this.tabControl1.Controls.Add(this._tabObjects);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(291, 294);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.TabStop = false;
            // 
            // _tabTilePools
            // 
            this._tabTilePools.Controls.Add(this.tilePoolPane1);
            this._tabTilePools.Location = new System.Drawing.Point(4, 22);
            this._tabTilePools.Margin = new System.Windows.Forms.Padding(0);
            this._tabTilePools.Name = "_tabTilePools";
            this._tabTilePools.Padding = new System.Windows.Forms.Padding(0, 0, 2, 1);
            this._tabTilePools.Size = new System.Drawing.Size(283, 268);
            this._tabTilePools.TabIndex = 0;
            this._tabTilePools.Text = "Tile Pools";
            this._tabTilePools.UseVisualStyleBackColor = true;
            // 
            // tilePoolPane1
            // 
            this.tilePoolPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tilePoolPane1.Location = new System.Drawing.Point(0, 0);
            this.tilePoolPane1.Margin = new System.Windows.Forms.Padding(0);
            this.tilePoolPane1.Name = "tilePoolPane1";
            this.tilePoolPane1.Size = new System.Drawing.Size(281, 267);
            this.tilePoolPane1.TabIndex = 0;
            this.tilePoolPane1.TabStop = false;
            // 
            // _tabObjects
            // 
            this._tabObjects.Controls.Add(this.objectPanel1);
            this._tabObjects.Location = new System.Drawing.Point(4, 22);
            this._tabObjects.Name = "_tabObjects";
            this._tabObjects.Padding = new System.Windows.Forms.Padding(0, 0, 2, 1);
            this._tabObjects.Size = new System.Drawing.Size(283, 268);
            this._tabObjects.TabIndex = 1;
            this._tabObjects.Text = "Objects";
            this._tabObjects.UseVisualStyleBackColor = true;
            // 
            // objectPanel1
            // 
            this.objectPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectPanel1.Location = new System.Drawing.Point(0, 0);
            this.objectPanel1.Name = "objectPanel1";
            this.objectPanel1.Size = new System.Drawing.Size(281, 267);
            this.objectPanel1.TabIndex = 0;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this._tabLayers);
            this.tabControl2.Controls.Add(this._tabProperties);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(291, 305);
            this.tabControl2.TabIndex = 0;
            this.tabControl2.TabStop = false;
            // 
            // _tabLayers
            // 
            this._tabLayers.Controls.Add(this.layerPane1);
            this._tabLayers.Location = new System.Drawing.Point(4, 22);
            this._tabLayers.Margin = new System.Windows.Forms.Padding(0);
            this._tabLayers.Name = "_tabLayers";
            this._tabLayers.Padding = new System.Windows.Forms.Padding(0, 0, 2, 1);
            this._tabLayers.Size = new System.Drawing.Size(283, 279);
            this._tabLayers.TabIndex = 0;
            this._tabLayers.Text = "Layers";
            this._tabLayers.UseVisualStyleBackColor = true;
            // 
            // layerPane1
            // 
            this.layerPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerPane1.Location = new System.Drawing.Point(0, 0);
            this.layerPane1.Margin = new System.Windows.Forms.Padding(0);
            this.layerPane1.Name = "layerPane1";
            this.layerPane1.Size = new System.Drawing.Size(281, 278);
            this.layerPane1.TabIndex = 0;
            this.layerPane1.TabStop = false;
            // 
            // _tabProperties
            // 
            this._tabProperties.Controls.Add(this.propertyPane1);
            this._tabProperties.Location = new System.Drawing.Point(4, 22);
            this._tabProperties.Margin = new System.Windows.Forms.Padding(0);
            this._tabProperties.Name = "_tabProperties";
            this._tabProperties.Padding = new System.Windows.Forms.Padding(0, 0, 2, 1);
            this._tabProperties.Size = new System.Drawing.Size(283, 279);
            this._tabProperties.TabIndex = 1;
            this._tabProperties.Text = "Properties";
            this._tabProperties.UseVisualStyleBackColor = true;
            // 
            // propertyPane1
            // 
            this.propertyPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyPane1.Location = new System.Drawing.Point(0, 0);
            this.propertyPane1.Margin = new System.Windows.Forms.Padding(0);
            this.propertyPane1.Name = "propertyPane1";
            this.propertyPane1.Size = new System.Drawing.Size(281, 278);
            this.propertyPane1.TabIndex = 0;
            this.propertyPane1.TabStop = false;
            // 
            // tabControlEx1
            // 
            this.tabControlEx1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlEx1.Location = new System.Drawing.Point(0, 0);
            this.tabControlEx1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControlEx1.Name = "tabControlEx1";
            this.tabControlEx1.SelectedIndex = 0;
            this.tabControlEx1.Size = new System.Drawing.Size(729, 603);
            this.tabControlEx1.TabIndex = 0;
            this.tabControlEx1.TabStop = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 652);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.statusBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "Form1";
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this._tabTilePools.ResumeLayout(false);
            this._tabObjects.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this._tabLayers.ResumeLayout(false);
            this._tabProperties.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel statusZoomText;
        private System.Windows.Forms.ToolStripStatusLabel statusZoomOut;
        private System.Windows.Forms.ToolStripStatusLabel statusZoomIn;
        private System.Windows.Forms.ToolStripStatusLabel statusCoord;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNoneToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabControl tabControl2;
        private TabControlEx tabControlEx1;
        private System.Windows.Forms.TabPage _tabTilePools;
        private TilePoolPane tilePoolPane1;
        private System.Windows.Forms.TabPage _tabLayers;
        private LayerPane layerPane1;
        private System.Windows.Forms.TabPage _tabProperties;
        private PropertyPane propertyPane1;
        private System.Windows.Forms.TabPage _tabObjects;
        private Panels.ObjectPanel objectPanel1;
    }
}

