using Treefrog.Windows.Controls.WinEx;
namespace Treefrog.Windows
{
    partial class LayerPane
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Tile Layer 2", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Object Layer 1", 1);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Collision Layer", 0);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Tile Layer 1", 0);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerPane));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this._listControl = new LayerListView();
            this._colLayer = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._buttonAdd = new System.Windows.Forms.ToolStripDropDownButton();
            this._menuNewTileLayer = new System.Windows.Forms.ToolStripMenuItem();
            this._menuNewObjectLayer = new System.Windows.Forms.ToolStripMenuItem();
            this._buttonRemove = new System.Windows.Forms.ToolStripButton();
            this._buttonCopy = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._buttonUp = new System.Windows.Forms.ToolStripButton();
            this._buttonDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._buttonProperties = new System.Windows.Forms.ToolStripButton();
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
            this.toolStripContainer1.ContentPanel.Controls.Add(this._listControl);
            this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(248, 354);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(248, 379);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // _listControl
            // 
            this._listControl.CheckBoxes = true;
            this._listControl.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colLayer});
            this._listControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listControl.FullRowSelect = true;
            this._listControl.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this._listControl.HideSelection = false;
            listViewItem1.StateImageIndex = 0;
            listViewItem2.StateImageIndex = 0;
            listViewItem3.StateImageIndex = 0;
            listViewItem4.StateImageIndex = 0;
            listViewItem5.StateImageIndex = 0;
            this._listControl.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5});
            this._listControl.LabelEdit = true;
            this._listControl.Location = new System.Drawing.Point(0, 0);
            this._listControl.Margin = new System.Windows.Forms.Padding(0);
            this._listControl.MultiSelect = false;
            this._listControl.Name = "_listControl";
            this._listControl.Scrollable = false;
            this._listControl.ShowGroups = false;
            this._listControl.Size = new System.Drawing.Size(248, 354);
            this._listControl.SmallImageList = this.imageList1;
            this._listControl.TabIndex = 0;
            this._listControl.UseCompatibleStateImageBehavior = false;
            this._listControl.View = System.Windows.Forms.View.Details;
            // 
            // Layer
            // 
            this._colLayer.Text = "Layer";
            this._colLayer.Width = 244;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "grid.png");
            this.imageList1.Images.SetKeyName(1, "game.png");
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._buttonAdd,
            this._buttonRemove,
            this._buttonCopy,
            this.toolStripSeparator1,
            this._buttonUp,
            this._buttonDown,
            this.toolStripSeparator2,
            this._buttonProperties});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(248, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 0;
            // 
            // _buttonAdd
            // 
            this._buttonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuNewTileLayer,
            this._menuNewObjectLayer});
            this._buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("_buttonAdd.Image")));
            this._buttonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonAdd.Name = "_buttonAdd";
            this._buttonAdd.Size = new System.Drawing.Size(29, 22);
            this._buttonAdd.Text = "Add Layer";
            // 
            // _menuNewTileLayer
            // 
            this._menuNewTileLayer.Name = "_menuNewTileLayer";
            this._menuNewTileLayer.Size = new System.Drawing.Size(167, 22);
            this._menuNewTileLayer.Text = "New &Tile Layer";
            // 
            // _menuNewObjectLayer
            // 
            this._menuNewObjectLayer.Name = "_menuNewObjectLayer";
            this._menuNewObjectLayer.Size = new System.Drawing.Size(167, 22);
            this._menuNewObjectLayer.Text = "New &Object Layer";
            // 
            // _buttonRemove
            // 
            this._buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("_buttonRemove.Image")));
            this._buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonRemove.Name = "_buttonRemove";
            this._buttonRemove.Size = new System.Drawing.Size(23, 22);
            this._buttonRemove.Text = "Remove Layer";
            // 
            // _buttonCopy
            // 
            this._buttonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonCopy.Image = ((System.Drawing.Image)(resources.GetObject("_buttonCopy.Image")));
            this._buttonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonCopy.Name = "_buttonCopy";
            this._buttonCopy.Size = new System.Drawing.Size(23, 22);
            this._buttonCopy.Text = "Copy Layer";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _buttonUp
            // 
            this._buttonUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonUp.Image = ((System.Drawing.Image)(resources.GetObject("_buttonUp.Image")));
            this._buttonUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonUp.Name = "_buttonUp";
            this._buttonUp.Size = new System.Drawing.Size(23, 22);
            this._buttonUp.Text = "Move Layer Up";
            // 
            // _buttonDown
            // 
            this._buttonDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonDown.Image = ((System.Drawing.Image)(resources.GetObject("_buttonDown.Image")));
            this._buttonDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonDown.Name = "_buttonDown";
            this._buttonDown.Size = new System.Drawing.Size(23, 22);
            this._buttonDown.Text = "Move Layer Down";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // _buttonProperties
            // 
            this._buttonProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonProperties.Image = ((System.Drawing.Image)(resources.GetObject("_buttonProperties.Image")));
            this._buttonProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonProperties.Name = "_buttonProperties";
            this._buttonProperties.Size = new System.Drawing.Size(23, 22);
            this._buttonProperties.Text = "Selected Layer Properties";
            // 
            // LayerPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "LayerPane";
            this.Size = new System.Drawing.Size(248, 379);
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
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ColumnHeader _colLayer;
        private System.Windows.Forms.ToolStripButton _buttonRemove;
        private System.Windows.Forms.ToolStripButton _buttonUp;
        private System.Windows.Forms.ToolStripButton _buttonDown;
        private System.Windows.Forms.ToolStripButton _buttonCopy;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton _buttonAdd;
        private System.Windows.Forms.ToolStripMenuItem _menuNewTileLayer;
        private System.Windows.Forms.ToolStripMenuItem _menuNewObjectLayer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton _buttonProperties;
        private LayerListView _listControl;
    }
}
