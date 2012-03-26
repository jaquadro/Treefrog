using System.Windows.Forms;
using System.Drawing;
using Treefrog.View.Controls.WinEx;
namespace Treefrog.View
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectPanel));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.viewportControl1 = new Treefrog.View.Controls.ViewportControl();
            this.objectViewControl1 = new Treefrog.View.Controls.ObjectViewControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._addObjectButton = new System.Windows.Forms.ToolStripButton();
            this._viewButton = new System.Windows.Forms.ToolStripDropDownButton();
            this._viewItemLarge = new System.Windows.Forms.ToolStripMenuItem();
            this._viewItemMedium = new System.Windows.Forms.ToolStripMenuItem();
            this._viewItemSmall = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.autoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._removeObjectButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._addCatButton = new System.Windows.Forms.ToolStripButton();
            this._removeCatButton = new System.Windows.Forms.ToolStripButton();
            this._categoryBox = new System.Windows.Forms.ToolStripComboBox();
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
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(279, 451);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(279, 476);
            this.toolStripContainer1.TabIndex = 1;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // viewportControl1
            // 
            this.viewportControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.viewportControl1.Control = this.objectViewControl1;
            this.viewportControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewportControl1.Location = new System.Drawing.Point(0, 0);
            this.viewportControl1.Margin = new System.Windows.Forms.Padding(0);
            this.viewportControl1.Name = "viewportControl1";
            this.viewportControl1.Size = new System.Drawing.Size(279, 451);
            this.viewportControl1.TabIndex = 0;
            // 
            // objectViewControl1
            // 
            this.objectViewControl1.Alignment = Treefrog.View.Controls.LayerControlAlignment.Center;
            this.objectViewControl1.CanAutoScroll = false;
            this.objectViewControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectViewControl1.HeightSynced = false;
            this.objectViewControl1.Location = new System.Drawing.Point(0, 0);
            this.objectViewControl1.Margin = new System.Windows.Forms.Padding(0);
            this.objectViewControl1.Name = "objectViewControl1";
            this.objectViewControl1.Size = new System.Drawing.Size(260, 432);
            this.objectViewControl1.TabIndex = 0;
            this.objectViewControl1.Text = "objectViewControl1";
            this.objectViewControl1.WidthSynced = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._addObjectButton,
            this._viewButton,
            this._removeObjectButton,
            this.toolStripSeparator2,
            this._addCatButton,
            this._removeCatButton,
            this._categoryBox});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(279, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 0;
            // 
            // _addObjectButton
            // 
            this._addObjectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._addObjectButton.Image = ((System.Drawing.Image)(resources.GetObject("_addObjectButton.Image")));
            this._addObjectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._addObjectButton.Name = "_addObjectButton";
            this._addObjectButton.Size = new System.Drawing.Size(23, 22);
            this._addObjectButton.Text = "Add Object";
            // 
            // _viewButton
            // 
            this._viewButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._viewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._viewButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._viewItemLarge,
            this._viewItemMedium,
            this._viewItemSmall,
            this.toolStripSeparator1,
            this.autoToolStripMenuItem});
            this._viewButton.Image = ((System.Drawing.Image)(resources.GetObject("_viewButton.Image")));
            this._viewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._viewButton.Name = "_viewButton";
            this._viewButton.Size = new System.Drawing.Size(29, 22);
            this._viewButton.Text = "Change View";
            // 
            // _viewItemLarge
            // 
            this._viewItemLarge.Name = "_viewItemLarge";
            this._viewItemLarge.Size = new System.Drawing.Size(150, 22);
            this._viewItemLarge.Text = "Large Icons";
            // 
            // _viewItemMedium
            // 
            this._viewItemMedium.Name = "_viewItemMedium";
            this._viewItemMedium.Size = new System.Drawing.Size(150, 22);
            this._viewItemMedium.Text = "Medium Icons";
            // 
            // _viewItemSmall
            // 
            this._viewItemSmall.Name = "_viewItemSmall";
            this._viewItemSmall.Size = new System.Drawing.Size(150, 22);
            this._viewItemSmall.Text = "Small Icons";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(147, 6);
            // 
            // autoToolStripMenuItem
            // 
            this.autoToolStripMenuItem.Checked = true;
            this.autoToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoToolStripMenuItem.Name = "autoToolStripMenuItem";
            this.autoToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.autoToolStripMenuItem.Text = "Auto";
            // 
            // _removeObjectButton
            // 
            this._removeObjectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._removeObjectButton.Image = ((System.Drawing.Image)(resources.GetObject("_removeObjectButton.Image")));
            this._removeObjectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._removeObjectButton.Name = "_removeObjectButton";
            this._removeObjectButton.Size = new System.Drawing.Size(23, 22);
            this._removeObjectButton.Text = "Remove Selected Object";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // _addCatButton
            // 
            this._addCatButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._addCatButton.Image = ((System.Drawing.Image)(resources.GetObject("_addCatButton.Image")));
            this._addCatButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._addCatButton.Name = "_addCatButton";
            this._addCatButton.Size = new System.Drawing.Size(23, 22);
            this._addCatButton.Text = "Add Category";
            // 
            // _removeCatButton
            // 
            this._removeCatButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._removeCatButton.Image = ((System.Drawing.Image)(resources.GetObject("_removeCatButton.Image")));
            this._removeCatButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._removeCatButton.Name = "_removeCatButton";
            this._removeCatButton.Size = new System.Drawing.Size(23, 22);
            this._removeCatButton.Text = "Remove Selected Category";
            // 
            // _categoryBox
            // 
            this._categoryBox.BackColor = System.Drawing.SystemColors.Window;
            this._categoryBox.Name = "_categoryBox";
            this._categoryBox.Size = new System.Drawing.Size(121, 25);
            // 
            // ObjectPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ObjectPanel";
            this.Size = new System.Drawing.Size(279, 476);
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

        private Controls.ViewportControl viewportControl1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton _addObjectButton;
        private System.Windows.Forms.ToolStripDropDownButton _viewButton;
        private System.Windows.Forms.ToolStripMenuItem _viewItemLarge;
        private System.Windows.Forms.ToolStripMenuItem _viewItemMedium;
        private System.Windows.Forms.ToolStripMenuItem _viewItemSmall;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem autoToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton _removeObjectButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton _addCatButton;
        private System.Windows.Forms.ToolStripButton _removeCatButton;
        private System.Windows.Forms.ToolStripComboBox _categoryBox;
        private Controls.ObjectViewControl objectViewControl1;
    }
}
