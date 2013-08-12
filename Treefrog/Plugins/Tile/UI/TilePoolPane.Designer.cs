using Treefrog.Windows.Controls;

namespace Treefrog.Plugins.Tiles.UI
{
    partial class TilePoolPane
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
            Treefrog.Render.TextureCache textureCache2 = new Treefrog.Render.TextureCache();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TilePoolPane));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.viewportControl1 = new Treefrog.Windows.Controls.ViewportControl();
            this._layerControl = new Treefrog.Windows.Controls.LayerGraphicsControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._buttonAdd = new System.Windows.Forms.ToolStripDropDownButton();
            this.importNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._buttonRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._buttonProperties = new System.Windows.Forms.ToolStripButton();
            this._poolComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.importMergeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.viewportControl1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(290, 448);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(290, 473);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // viewportControl1
            // 
            this.viewportControl1.AutoShowScrollbars = true;
            this.viewportControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.viewportControl1.Control = this._layerControl;
            this.viewportControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewportControl1.Location = new System.Drawing.Point(0, 0);
            this.viewportControl1.Margin = new System.Windows.Forms.Padding(0);
            this.viewportControl1.Name = "viewportControl1";
            this.viewportControl1.Size = new System.Drawing.Size(290, 448);
            this.viewportControl1.TabIndex = 0;
            // 
            // _layerControl
            // 
            this._layerControl.BackColor = System.Drawing.Color.Black;
            this._layerControl.CanvasAlignment = Treefrog.Presentation.CanvasAlignment.Center;
            this._layerControl.ClearColor = ((Microsoft.Xna.Framework.Color)(resources.GetObject("_layerControl.ClearColor")));
            this._layerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layerControl.HeightSynced = false;
            this._layerControl.Location = new System.Drawing.Point(0, 0);
            this._layerControl.Name = "_layerControl";
            this._layerControl.ReferenceHeight = 0;
            this._layerControl.ReferenceOriginX = 0;
            this._layerControl.ReferenceOriginY = 0;
            this._layerControl.ReferenceWidth = 0;
            this._layerControl.RootLayer = null;
            this._layerControl.Size = new System.Drawing.Size(288, 446);
            this._layerControl.TabIndex = 0;
            textureCache2.GraphicsDevice = null;
            textureCache2.SourcePool = null;
            this._layerControl.TextureCache = textureCache2;
            this._layerControl.VSync = false;
            this._layerControl.WidthSynced = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._buttonAdd,
            this._buttonRemove,
            this.toolStripSeparator1,
            this._buttonProperties,
            this._poolComboBox});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(290, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 0;
            // 
            // _buttonAdd
            // 
            this._buttonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importNewToolStripMenuItem,
            this.importMergeToolStripMenuItem});
            this._buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("_buttonAdd.Image")));
            this._buttonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonAdd.Name = "_buttonAdd";
            this._buttonAdd.Size = new System.Drawing.Size(29, 22);
            this._buttonAdd.Text = "Add Tile Pool";
            // 
            // importNewToolStripMenuItem
            // 
            this.importNewToolStripMenuItem.Name = "importNewToolStripMenuItem";
            this.importNewToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.importNewToolStripMenuItem.Text = "Import pool...";
            // 
            // _buttonRemove
            // 
            this._buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("_buttonRemove.Image")));
            this._buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonRemove.Name = "_buttonRemove";
            this._buttonRemove.Size = new System.Drawing.Size(23, 22);
            this._buttonRemove.Text = "Remove Selected Tile Pool";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _buttonProperties
            // 
            this._buttonProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonProperties.Image = ((System.Drawing.Image)(resources.GetObject("_buttonProperties.Image")));
            this._buttonProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonProperties.Name = "_buttonProperties";
            this._buttonProperties.Size = new System.Drawing.Size(23, 22);
            this._buttonProperties.Text = "Selected Tile Pool Properties";
            // 
            // _poolComboBox
            // 
            this._poolComboBox.Name = "_poolComboBox";
            this._poolComboBox.Size = new System.Drawing.Size(121, 25);
            // 
            // importMergeToolStripMenuItem
            // 
            this.importMergeToolStripMenuItem.Name = "importMergeToolStripMenuItem";
            this.importMergeToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.importMergeToolStripMenuItem.Text = "Import merge...";
            // 
            // TilePoolPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "TilePoolPane";
            this.Size = new System.Drawing.Size(290, 473);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private ViewportControl viewportControl1;
        private LayerGraphicsControl _layerControl;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripComboBox _poolComboBox;
        private System.Windows.Forms.ToolStripButton _buttonRemove;
        private System.Windows.Forms.ToolStripButton _buttonProperties;
        private System.Windows.Forms.ToolStripDropDownButton _buttonAdd;
        private System.Windows.Forms.ToolStripMenuItem importNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem importMergeToolStripMenuItem;
    }
}
