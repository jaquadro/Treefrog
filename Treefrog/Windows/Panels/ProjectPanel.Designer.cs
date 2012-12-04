namespace Treefrog.Windows.Panels
{
    partial class ProjectPanel
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Level 1", 8, 8);
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Level 2", 8, 8);
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Levels", 2, 2, new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Collision 16", 7, 7);
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Tilesets", 4, 4, new System.Windows.Forms.TreeNode[] {
            treeNode4});
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Default", 6, 6);
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Object Groups", 3, 3, new System.Windows.Forms.TreeNode[] {
            treeNode6});
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Project", 0, 0, new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode5,
            treeNode7});
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectPanel));
            this._tree = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // _tree
            // 
            this._tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tree.ImageIndex = 0;
            this._tree.ImageList = this.imageList1;
            this._tree.Location = new System.Drawing.Point(0, 0);
            this._tree.Name = "_tree";
            treeNode1.ImageIndex = 8;
            treeNode1.Name = "Node7";
            treeNode1.SelectedImageIndex = 8;
            treeNode1.Text = "Level 1";
            treeNode2.ImageIndex = 8;
            treeNode2.Name = "Node10";
            treeNode2.SelectedImageIndex = 8;
            treeNode2.Text = "Level 2";
            treeNode3.ImageIndex = 2;
            treeNode3.Name = "Node1";
            treeNode3.SelectedImageIndex = 2;
            treeNode3.Text = "Levels";
            treeNode4.ImageIndex = 7;
            treeNode4.Name = "Node8";
            treeNode4.SelectedImageIndex = 7;
            treeNode4.Text = "Collision 16";
            treeNode5.ImageIndex = 4;
            treeNode5.Name = "Node5";
            treeNode5.SelectedImageIndex = 4;
            treeNode5.Text = "Tilesets";
            treeNode6.ImageIndex = 6;
            treeNode6.Name = "Node9";
            treeNode6.SelectedImageIndex = 6;
            treeNode6.Text = "Default";
            treeNode7.ImageIndex = 3;
            treeNode7.Name = "Node6";
            treeNode7.SelectedImageIndex = 3;
            treeNode7.Text = "Object Groups";
            treeNode8.ImageIndex = 0;
            treeNode8.Name = "Node0";
            treeNode8.SelectedImageIndex = 0;
            treeNode8.Text = "Project";
            this._tree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode8});
            this._tree.SelectedImageIndex = 0;
            this._tree.ShowRootLines = false;
            this._tree.Size = new System.Drawing.Size(213, 303);
            this._tree.TabIndex = 0;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "application");
            this.imageList1.Images.SetKeyName(1, "folder");
            this.imageList1.Images.SetKeyName(2, "folder-map");
            this.imageList1.Images.SetKeyName(3, "folder-game");
            this.imageList1.Images.SetKeyName(4, "folder-grid");
            this.imageList1.Images.SetKeyName(5, "folder-layer");
            this.imageList1.Images.SetKeyName(6, "game");
            this.imageList1.Images.SetKeyName(7, "grid");
            this.imageList1.Images.SetKeyName(8, "map");
            // 
            // ProjectPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._tree);
            this.Name = "ProjectPanel";
            this.Size = new System.Drawing.Size(213, 303);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView _tree;
        private System.Windows.Forms.ImageList imageList1;
    }
}
