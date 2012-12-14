namespace Treefrog.Windows.Panels
{
    partial class TileBrushPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TileBrushPanel));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._buttonAdd = new System.Windows.Forms.ToolStripDropDownButton();
            this._buttonRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._buttonFilter = new System.Windows.Forms.ToolStripButton();
            this._filterSelection = new System.Windows.Forms.ToolStripComboBox();
            this._listView = new System.Windows.Forms.ListView();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.CanOverflow = false;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._buttonAdd,
            this._buttonRemove,
            this.toolStripSeparator1,
            this._buttonFilter,
            this._filterSelection});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(229, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _buttonAdd
            // 
            this._buttonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("_buttonAdd.Image")));
            this._buttonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonAdd.Name = "_buttonAdd";
            this._buttonAdd.Size = new System.Drawing.Size(29, 22);
            this._buttonAdd.Text = "toolStripDropDownButton1";
            // 
            // _buttonRemove
            // 
            this._buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("_buttonRemove.Image")));
            this._buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonRemove.Name = "_buttonRemove";
            this._buttonRemove.Size = new System.Drawing.Size(23, 22);
            this._buttonRemove.Text = "toolStripButton1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _buttonFilter
            // 
            this._buttonFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonFilter.Image = ((System.Drawing.Image)(resources.GetObject("_buttonFilter.Image")));
            this._buttonFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonFilter.Name = "_buttonFilter";
            this._buttonFilter.Size = new System.Drawing.Size(23, 22);
            this._buttonFilter.Text = "toolStripButton2";
            // 
            // _filterSelection
            // 
            this._filterSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._filterSelection.Name = "_filterSelection";
            this._filterSelection.Size = new System.Drawing.Size(121, 25);
            // 
            // _listView
            // 
            this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listView.HideSelection = false;
            this._listView.Location = new System.Drawing.Point(0, 25);
            this._listView.MultiSelect = false;
            this._listView.Name = "_listView";
            this._listView.Size = new System.Drawing.Size(229, 276);
            this._listView.TabIndex = 1;
            this._listView.UseCompatibleStateImageBehavior = false;
            // 
            // TileBrushPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._listView);
            this.Controls.Add(this.toolStrip1);
            this.Name = "TileBrushPanel";
            this.Size = new System.Drawing.Size(229, 301);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton _buttonAdd;
        private System.Windows.Forms.ToolStripButton _buttonRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton _buttonFilter;
        private System.Windows.Forms.ToolStripComboBox _filterSelection;
        private System.Windows.Forms.ListView _listView;
    }
}
