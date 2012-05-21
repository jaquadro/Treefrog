using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Presentation.Layers;
using Treefrog.Model;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Drawing;
using Treefrog.Framework;
using Amphibian.Drawing;

namespace Treefrog.View.Controls
{
    using XColor = Microsoft.Xna.Framework.Color;
    using XRectangle = Microsoft.Xna.Framework.Rectangle;

    public enum ObjectViewSize
    {
        Small,
        Medium,
        Large,
    }

    //public class ObjectViewControlState
    //{
    //    private ObjectViewSize _previewSize;

    //    private TileRegistry _registry;
    //    private TilePool _smallPool;
    //    private TilePool _mediumPool;
    //    private TilePool _largePool;

    //    private TileSetLayer _smallLayer;
    //    private TileSetLayer _mediumLayer;
    //    private TileSetLayer _largeLayer;

    //    private TileSetControlLayer _smallControlLayer;
    //    private TileSetControlLayer _mediumControlLayer;
    //    private TileSetControlLayer _largeControlLayer;

    //    private TileSetLayer _activeLayer;
    //    private TileSetControlLayer _activeControl;

    //    private ObjectPool _pool;
    //    private ObjectClass _selectedObject;

    //    public ObjectViewControlState (ObjectPool pool);

    //    public ObjectViewSize PreviewSize { get; set; }

    //    //public event EventHandler<ObjectClassEventArgs> SelectionChanged = (s, e) => { };

    //    //protected virtual void OnSelectionChanged (ObjectClassEventArgs e);

    //    private void ObjectPool_ObjectAdded (object sender, ObjectClassEventArgs e);
    //    private void ObjectPool_ObjectRemoved (object sender, ObjectClassEventArgs e);

    //    private void ObjectClass_ImageChanged (object sender, EventHandler e);

    //    private byte[] BuildPreviewTexture (Texture2D source, int tileW, int tileH);
    //}

    public class ObjectViewControl : LayerControl
    {
        private bool _initialized;

        //private TileRegistry _registry;
        private TilePool _smallPool;
        private TilePool _mediumPool;
        private TilePool _largePool;

        private TileSetLayer _smallLayer;
        private TileSetLayer _mediumLayer;
        private TileSetLayer _largeLayer;

        private TileSetControlLayer _smallControlLayer;
        private TileSetControlLayer _mediumControlLayer;
        private TileSetControlLayer _largeControlLayer;

        private TileSetLayer _activeLayer;
        private TileSetControlLayer _activeControl;

        public ObjectViewControl ()
            : base()
        {
            ControlInitialized += GraphicsDeviceControl_ControlInitialized;
        }

        private void GraphicsDeviceControl_ControlInitialized (object sender, EventArgs e)
        {
            //_registry = new TileRegistry(this.GraphicsDevice);
            /*_smallPool = new TilePool("Small View Source", _registry, 32, 32);
            _mediumPool = new TilePool("Medium View Source", _registry, 64, 64);
            _largePool = new TilePool("Large View Source", _registry, 128, 128);*/

            _smallLayer = new TileSetLayer("Small View", _smallPool);
            _mediumLayer = new TileSetLayer("Medium View", _mediumPool);
            _largeLayer = new TileSetLayer("Large View", _largePool);

            _smallControlLayer = new TileSetControlLayer(this, _smallLayer);
            _mediumControlLayer = new TileSetControlLayer(this, _mediumLayer);
            _largeControlLayer = new TileSetControlLayer(this, _largeLayer);

            this.WidthSynced = true;
            this.Alignment = LayerControlAlignment.UpperLeft;

            _smallControlLayer.UseInVirtualSizeCalculation = false;
            _mediumControlLayer.UseInVirtualSizeCalculation = false;

            _largeControlLayer.ShouldDrawContent = LayerCondition.Always;
            _largeControlLayer.ShouldRespondToInput = LayerCondition.Always;
            _largeControlLayer.Selected = true;

            _largeControlLayer.MouseTileDown += TileControlMouseDownHandler;
            _largeControlLayer.PreDrawContent += DrawSelectedTileIndicators;
            _largeControlLayer.DrawExtraCallback += DrawSelectedTileIndicatorsBorder;

            _activeControl = _largeControlLayer;
            _activeLayer = _largeLayer;

            _initialized = true;

            for (int i = 0; i < 70; i++) {
                AddObject(@"E:\Workspace\Image Projects\Graphic Rips\Paper Mario 2\Environment Textures\Individual\Creepy Steeple\" + i + ".png");
            }
            //AddObject(@"E:\Workspace\Image Projects\Graphic Rips\Paper Mario 2\Environment Textures\Individual\Creepy Steeple\24.png");
            //AddObject(@"E:\Workspace\Image Projects\Graphic Rips\Paper Mario 2\Environment Textures\Individual\Creepy Steeple\25.png");
        }

        /*public event EventHandler<ObjectClassEventArgs> ObjectMouseDown = (s, e) => { };

        protected virtual void OnObjectMouseDown (ObjectClassEventArgs e)
        {
            ObjectMouseDown(this, e);
        }*/

        private void TileControl_MouseDown (object sender, TileMouseEventArgs e)
        {
            //OnObjectMouseDown(e.
        }


        private Tile _selectedTile;
        private TileCoord _selectedTileCoord;

        private void TileControlMouseDownHandler (object sender, TileMouseEventArgs e)
        {
            //if (_controller != null) {
            //    _controller.ActionSelectTile(e.Tile);
            //}
            _selectedTile = e.Tile;
            _selectedTileCoord = e.TileLocation;
        }

        Amphibian.Drawing.Brush _selectBrush;
        Amphibian.Drawing.Pen _borderBrush;
        private void DrawSelectedTileIndicators (object sender, DrawLayerEventArgs e)
        {
            //if (_controller == null) {
            //    return;
            //}

            Tile selectedTile = _selectedTile; // _controller.SelectedTile;
            if (_activeLayer != null && selectedTile != null) {
                if (_selectBrush == null) {
                    Color c1 = SystemColors.Highlight;
                    Color c2 = SystemColors.Window;
                    XColor x1 = new XColor(c1.R / 255f, c1.G / 255f, c1.B / 255f, 1f);
                    XColor x2 = new XColor(c2.R / 255f, c2.G / 255f, c2.B / 255f, 1f);
                    _selectBrush = this.CreateSolidColorBrush(XColor.Lerp(x2, XColor.Lerp(x1, x2, 0.2f), 0.8f));
                }

                int x = _selectedTileCoord.X;
                int y = _selectedTileCoord.Y;

                Draw2D.FillRectangle(e.SpriteBatch, new XRectangle(
                    (int)(_selectedTileCoord.X * selectedTile.Width * this.Zoom),
                    (int)(_selectedTileCoord.Y * selectedTile.Height * this.Zoom),
                    (int)(selectedTile.Width * this.Zoom),
                    (int)(selectedTile.Height * this.Zoom)),
                    _selectBrush);
            }
        }

        private void DrawSelectedTileIndicatorsBorder (object sender, DrawLayerEventArgs e)
        {
            //if (_controller == null) {
            //    return;
            //}

            Tile selectedTile = _selectedTile; // _controller.SelectedTile;
            if (_activeLayer != null && selectedTile != null) {
                if (_borderBrush == null) {
                    Color c = SystemColors.Highlight;
                    //_borderBrush = new Pen(new SolidColorBrush(GraphicsDevice, new XColor(c.R / 255f, c.G / 255f, c.B / 255f, 1f)), 1);
                }

                int x = _selectedTileCoord.X;
                int y = _selectedTileCoord.Y;

                Draw2D.DrawRectangle(e.SpriteBatch, new XRectangle(
                    (int)(_selectedTileCoord.X * selectedTile.Width * this.Zoom),
                    (int)(_selectedTileCoord.Y * selectedTile.Height * this.Zoom),
                    (int)(selectedTile.Width * this.Zoom),
                    (int)(selectedTile.Height * this.Zoom)),
                    _borderBrush);
            }
        }

        private void AddObject (String filename)
        {
            using (FileStream fs = File.OpenRead(filename)) {
                AddObject(fs);
            }
        }

        private void AddObject (Stream stream)
        {
            using (Bitmap bmp = new Bitmap(stream)) {
                _smallPool.AddTile(BuildPreviewTexture(bmp, 32, 32));
                _mediumPool.AddTile(BuildPreviewTexture(bmp, 64, 64));
                _largePool.AddTile(BuildPreviewTexture(bmp, 128, 128));
            }
        }

        private byte[] BuildPreviewTexture (Bitmap bmp, int tileW, int tileH)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            if (w >= h && w > tileW) {
                h = (int)(h * ((float)tileW / w));
                w = tileW;
            }
            else if (h >= w && h > tileH) {
                w = (int)(w * ((float)tileH / h));
                h = tileH;
            }

            int offX = (tileW - w) / 2;
            int offY = (tileH - h) / 2;

            using (Bitmap finalBmp = new Bitmap(tileW, tileH)) {
                using (Bitmap scaledBmp = new Bitmap(bmp, w, h)) {
                    using (Graphics g = Graphics.FromImage(finalBmp)) {
                        g.DrawImage(scaledBmp, offX, offY, new Rectangle(0, 0, w, h), GraphicsUnit.Pixel);
                    }
                }

                byte[] data = new byte[4 * tileW * tileH];
                for (int y = 0; y < tileH; y++) {
                    for (int x = 0; x < tileW; x++) {
                        Color c = finalBmp.GetPixel(x, y);
                        data[(y * tileW + x) * 4 + 0] = c.R;
                        data[(y * tileW + x) * 4 + 1] = c.G;
                        data[(y * tileW + x) * 4 + 2] = c.B;
                        data[(y * tileW + x) * 4 + 3] = c.A;
                    }
                }

                return data;
            }
        }
    }
}
