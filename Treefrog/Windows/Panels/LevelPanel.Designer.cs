using Treefrog.Windows.Controls;

namespace Treefrog.Windows
{
    partial class LevelPanel
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
            this._viewportControl = new ViewportControl();
            this.layerControl1 = new LayerControl();
            this.SuspendLayout();
            // 
            // viewportControl1
            // 
            this._viewportControl.Control = this.layerControl1;
            this._viewportControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._viewportControl.Location = new System.Drawing.Point(0, 0);
            this._viewportControl.Margin = new System.Windows.Forms.Padding(0);
            this._viewportControl.Name = "viewportControl1";
            this._viewportControl.Size = new System.Drawing.Size(765, 692);
            this._viewportControl.TabIndex = 0;
            // 
            // layerControl1
            // 
            this.layerControl1.Alignment = LayerControlAlignment.Center;
            this.layerControl1.CanAutoScroll = false;
            this.layerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerControl1.HeightSynced = false;
            this.layerControl1.Location = new System.Drawing.Point(0, 0);
            this.layerControl1.Margin = new System.Windows.Forms.Padding(0);
            this.layerControl1.Name = "layerControl1";
            this.layerControl1.Size = new System.Drawing.Size(748, 675);
            this.layerControl1.TabIndex = 0;
            this.layerControl1.Text = "layerControl1";
            this.layerControl1.WidthSynced = false;
            // 
            // LevelPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._viewportControl);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "LevelPanel";
            this.Size = new System.Drawing.Size(765, 692);
            this.ResumeLayout(false);

        }

        #endregion

        private ViewportControl _viewportControl;
        private LayerControl layerControl1;
    }
}
