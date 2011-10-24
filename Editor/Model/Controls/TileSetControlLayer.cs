using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Editor.Model.Controls
{
    public class TileSetControlLayer : TileControlLayer
    {
        #region Fields

        private TileSetLayer _layer;

        #endregion

        #region Constructors

        public TileSetControlLayer (LayerControl control)
            : base(control)
        {
            //Control.VirtualSizeChanged += ControlVirtualSizeChangedHandler;
            Control.SizeChanged += ControlSizeChangedHandler;
        }

        public TileSetControlLayer (LayerControl control, TileSetLayer layer)
            : this(control)
        {
            Layer = layer;
        }

        public TileSetControlLayer (LayerControl control, Layer layer)
            : this(control, layer as TileSetLayer)
        {
        }

        #endregion

        #region Properties

        public new TileSetLayer Layer
        {
            get { return _layer; }
            set
            {
                _layer = value;
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

                if (Control.HeightSynced) {
                    return (Control.Height / _layer.TileHeight) * _layer.TileHeight;
                }
                else if (Control.WidthSynced) {
                    int tilesWide = Control.Width / _layer.TileWidth;
                    return ((_layer.Capacity + tilesWide - 1) / tilesWide) * _layer.TileHeight;
                }

                return (int)Math.Ceiling(Math.Sqrt(_layer.Capacity)) * _layer.TileHeight;
            }
        }

        public override int VirtualWidth
        {
            get
            {
                if (_layer == null) {
                    return 0;
                }

                if (Control.WidthSynced) {
                    return (Control.Width / _layer.TileWidth) * _layer.TileWidth;
                }
                else if (Control.HeightSynced) {
                    int tilesHigh = Control.Height / _layer.TileHeight;
                    return ((_layer.Capacity + tilesHigh - 1) / tilesHigh) * _layer.TileWidth;
                }

                return (int)Math.Ceiling(Math.Sqrt(_layer.Capacity)) * _layer.TileWidth;
            }
        }

        #endregion

        #region Event Handlers

        /*private void ControlVirtualSizeChangedHandler (object sender, EventArgs e)
        {
            if ((Control.HeightSynced && Control.WidthSynced) || _tileSource == null) {
                return;
            }

            Control.VirtualSizeChanged -= ControlVirtualSizeChangedHandler;

            CalculateVirtualSize();

            Control.VirtualSizeChanged += ControlVirtualSizeChangedHandler;
        }*/

        private void ControlSizeChangedHandler (object sender, EventArgs e)
        {
            OnVirutalSizeChanged(EventArgs.Empty);
        }

        #endregion

        #region Event Dispatchers

        protected override void OnMouseTileClick (TileMouseEventArgs e)
        {
            int index = CoordToIndex(e.TileLocation.X, e.TileLocation.Y);
            if (index >= 0 && index < _layer.Count) {
                e = new TileMouseEventArgs(e, e.TileLocation, _layer[index]);
            }

            base.OnMouseTileClick(e);
        }

        #endregion

        private int CoordToIndex (int x, int y)
        {
            if (Control.WidthSynced) {
                int tilesWide = Control.Width / _layer.TileWidth;
                return y * tilesWide + x;
            }

            throw new InvalidOperationException("Can't convert tileset coordinate to index if width not synced");
        }

        /*private void CalculateVirtualSize ()
        {
            if ((Control.HeightSynced && Control.WidthSynced) || _tileSource == null) {
                return;
            }

            if (Control.HeightSynced) {
                int tilesHigh = Control.VirtualHeight / _tileSource.TileHeight;
                int height = tilesHigh * _tileSource.TileHeight;

                Control.VirtualWidth = ((_tileSource.Capacity + tilesHigh - 1) / tilesHigh) * _tileSource.TileWidth;
            }
            else if (Control.WidthSynced) {
                int tilesWide = Control.VirtualWidth / _tileSource.TileWidth;
                int width = tilesWide * _tileSource.TileWidth;

                Control.VirtualHeight = ((_tileSource.Capacity + tilesWide - 1) / tilesWide) * _tileSource.TileHeight;
            }
        }*/

        protected override void DrawTiles (SpriteBatch spriteBatch, Rectangle tileRegion)
        {
            base.DrawTiles(spriteBatch, tileRegion);

            int tilesWide = Control.Width / _layer.TileWidth;

            int pointX = 0;
            int pointY = 0;

            // TODO: We are drawing tiles even if they are scrolled off-screen

            foreach (Tile tile in _layer.Tiles) {
                Rectangle dest = new Rectangle(
                    pointX * (int)(_layer.TileWidth * Control.Zoom),
                    pointY * (int)(_layer.TileHeight * Control.Zoom),
                    (int)(_layer.TileWidth * Control.Zoom),
                    (int)(_layer.TileHeight * Control.Zoom));
                tile.Draw(spriteBatch, dest);

                if (++pointX >= tilesWide) {
                    pointX = 0;
                    pointY++;
                }
            }
        }

        protected override Func<int, int, bool> TileInRegionPredicate (Rectangle tileRegion)
        {
            int tilesWide = Control.Width / _layer.TileWidth;
            int subW = (Control.Width % _layer.TileWidth) > 0 ? 1 : 0;
            int subH = 0; // (Control.Height % _layer.TileHeight) > 0 ? 1 : 0;

            return (int x, int y) =>
            {
                return (x >= tileRegion.X) && (x < tileRegion.X + tileRegion.Width - subW) &&
                    (y >= tileRegion.Y) && (y < tileRegion.Y + tileRegion.Height - subH) &&
                    (y * tilesWide + x < _layer.Count);
            };
        }
    }
}
