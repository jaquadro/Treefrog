using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;

namespace Editor.Model.Controls
{
    public class MultiTileControlLayer : TileControlLayer
    {
        #region Fields

        MultiTileGridLayer _layer;

        #endregion

        #region Constructors

        public MultiTileControlLayer (LayerControl control)
            : base(control)
        {
        }

        public MultiTileControlLayer (LayerControl control, MultiTileGridLayer layer)
            : this(control)
        {
            Layer = layer;
        }

        public MultiTileControlLayer (LayerControl control, Layer layer)
            : this(control, layer as MultiTileGridLayer)
        {
        }

        #endregion

        #region Properties

        public new MultiTileGridLayer Layer
        {
            get { return _layer; }
            set
            {
                if (_layer != null) {
                    _layer.LayerSizeChanged -= TileLayerSizeChangedHandler;
                }

                _layer = value;
                if (_layer != null) {
                    _layer.LayerSizeChanged += TileLayerSizeChangedHandler;
                }

                base.Layer = value;

                OnVirutalSizeChanged(EventArgs.Empty);
            }
        }

        public override int VirtualHeight
        {
            get 
            {
                if (_layer == null) {
                    return 0;
                }
                return _layer.LayerHeight * _layer.TileHeight;
            }
        }

        public override int VirtualWidth
        {
            get 
            {
                if (_layer == null) {
                    return 0;
                } 
                return _layer.LayerWidth * _layer.TileWidth;
            }
        }

        #endregion

        #region Event Handlers

        private void TileLayerSizeChangedHandler (object sender, EventArgs e)
        {
            OnVirutalSizeChanged(EventArgs.Empty);
        }

        #endregion

        protected override void DrawTiles (SpriteBatch spriteBatch, Rectangle tileRegion)
        {
            base.DrawTiles(spriteBatch, tileRegion);

            RenderTarget2D target = null;
            if (_layer.Opacity < 1f) {
                target = new RenderTarget2D(spriteBatch.GraphicsDevice, 
                    spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height, 
                    false, SurfaceFormat.Color, DepthFormat.None);

                spriteBatch.GraphicsDevice.SetRenderTarget(target);
                spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            }

            Vector2 offset = BeginDraw(spriteBatch);

            foreach (LocatedTile locTile in _layer.TilesAt(tileRegion)) {
                Rectangle dest = new Rectangle(
                    locTile.X * (int)(_layer.TileWidth * Control.Zoom),
                    locTile.Y * (int)(_layer.TileHeight * Control.Zoom),
                    (int)(_layer.TileWidth * Control.Zoom),
                    (int)(_layer.TileHeight * Control.Zoom));
                locTile.Tile.Draw(spriteBatch, dest);
            }

            EndDraw(spriteBatch);

            if (_layer.Opacity < 1f) {
                spriteBatch.GraphicsDevice.SetRenderTarget(null);

                BeginDraw(spriteBatch);
                spriteBatch.Draw(target, new Vector2(-offset.X, -offset.Y), new Color(1f, 1f, 1f, _layer.Opacity));
                EndDraw(spriteBatch);
            }
        }

        protected override Func<int, int, bool> TileInRegionPredicate (Rectangle tileRegion)
        {
            return (int x, int y) =>
            {
                return (x >= tileRegion.X) && (x < tileRegion.X + tileRegion.Width) &&
                    (y >= tileRegion.Y) && (y < tileRegion.Y + tileRegion.Height);
            };
        }
    }
}
