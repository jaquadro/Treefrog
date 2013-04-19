namespace Treefrog.Windows.Panels
{
    partial class MinimapPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MinimapPanel));
            Treefrog.Render.TextureCache textureCache1 = new Treefrog.Render.TextureCache();
            this._layerControl = new Treefrog.Windows.Controls.LayerGraphicsControl();
            this.SuspendLayout();
            // 
            // layerGraphicsControl1
            // 
            this._layerControl.BackColor = System.Drawing.Color.Black;
            this._layerControl.CanvasAlignment = Treefrog.Presentation.CanvasAlignment.Center;
            this._layerControl.ClearColor = ((Microsoft.Xna.Framework.Color)(resources.GetObject("layerGraphicsControl1.ClearColor")));
            this._layerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layerControl.HeightSynced = false;
            this._layerControl.Location = new System.Drawing.Point(0, 0);
            this._layerControl.Margin = new System.Windows.Forms.Padding(0);
            this._layerControl.Name = "layerGraphicsControl1";
            this._layerControl.ReferenceHeight = 0;
            this._layerControl.ReferenceOriginX = 0;
            this._layerControl.ReferenceOriginY = 0;
            this._layerControl.ReferenceWidth = 0;
            this._layerControl.RootLayer = null;
            this._layerControl.Size = new System.Drawing.Size(237, 234);
            this._layerControl.TabIndex = 0;
            textureCache1.GraphicsDevice = null;
            textureCache1.SourcePool = null;
            this._layerControl.TextureCache = textureCache1;
            this._layerControl.VSync = false;
            this._layerControl.WidthSynced = false;
            // 
            // MinimapPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._layerControl);
            this.Name = "MinimapPanel";
            this.Size = new System.Drawing.Size(237, 234);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.LayerGraphicsControl _layerControl;
    }
}
