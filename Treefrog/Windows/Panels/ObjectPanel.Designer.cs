namespace Treefrog.Windows.Panels
{
    partial class ObjectPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectPanel));
            this._listView = new System.Windows.Forms.ListView();
            this._listViewImages = new System.Windows.Forms.ImageList(this.components);
            this._toolStrip = new System.Windows.Forms.ToolStrip();
            this._buttonAddObject = new System.Windows.Forms.ToolStripButton();
            this._buttonRemoveObject = new System.Windows.Forms.ToolStripButton();
            this._toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _listView
            // 
            this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listView.HideSelection = false;
            this._listView.LargeImageList = this._listViewImages;
            this._listView.Location = new System.Drawing.Point(0, 25);
            this._listView.MultiSelect = false;
            this._listView.Name = "_listView";
            this._listView.ShowGroups = false;
            this._listView.Size = new System.Drawing.Size(237, 345);
            this._listView.SmallImageList = this._listViewImages;
            this._listView.TabIndex = 0;
            this._listView.TileSize = new System.Drawing.Size(64, 64);
            this._listView.UseCompatibleStateImageBehavior = false;
            // 
            // _listViewImages
            // 
            this._listViewImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this._listViewImages.ImageSize = new System.Drawing.Size(64, 64);
            this._listViewImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _toolStrip
            // 
            this._toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._buttonAddObject,
            this._buttonRemoveObject});
            this._toolStrip.Location = new System.Drawing.Point(0, 0);
            this._toolStrip.Name = "_toolStrip";
            this._toolStrip.Size = new System.Drawing.Size(237, 25);
            this._toolStrip.TabIndex = 1;
            this._toolStrip.Text = "toolStrip1";
            // 
            // _buttonAddObject
            // 
            this._buttonAddObject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonAddObject.Image = ((System.Drawing.Image)(resources.GetObject("_buttonAddObject.Image")));
            this._buttonAddObject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonAddObject.Name = "_buttonAddObject";
            this._buttonAddObject.Size = new System.Drawing.Size(23, 22);
            this._buttonAddObject.Text = "Add Object";
            // 
            // _buttonRemoveObject
            // 
            this._buttonRemoveObject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonRemoveObject.Image = ((System.Drawing.Image)(resources.GetObject("_buttonRemoveObject.Image")));
            this._buttonRemoveObject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonRemoveObject.Name = "_buttonRemoveObject";
            this._buttonRemoveObject.Size = new System.Drawing.Size(23, 22);
            this._buttonRemoveObject.Text = "Remove Object";
            // 
            // ObjectPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._listView);
            this.Controls.Add(this._toolStrip);
            this.Name = "ObjectPanel";
            this.Size = new System.Drawing.Size(237, 370);
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView _listView;
        private System.Windows.Forms.ImageList _listViewImages;
        private System.Windows.Forms.ToolStrip _toolStrip;
        private System.Windows.Forms.ToolStripButton _buttonAddObject;
        private System.Windows.Forms.ToolStripButton _buttonRemoveObject;
    }
}
