using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Model;
using Treefrog.Presentation.Controllers;
using Treefrog.Presentation.Tools;

namespace Treefrog.Presentation.Layers
{
    public class TileSetLayerPresenter : RenderLayerPresenter, IPointerResponder
    {
        private TileSetLayer _layer;

        public TileSetLayerPresenter (TileSetLayer layer)
        {
            _layer = layer;
        }

        public ILevelGeometry LevelGeometry { get; set; }

        public new TileSetLayer Layer
        {
            get { return _layer; }
        }

        public int TileWidth
        {
            get { return _layer.TileWidth; }
        }

        public int TileHeight
        {
            get { return _layer.TileHeight; }
        }

        public override IEnumerable<DrawCommand> RenderCommands
        {
            get
            {
                if (Layer == null || LevelGeometry == null)
                    yield break;

                ILevelGeometry geometry = LevelGeometry;
                int tilesWide = TilesWide;

                int pointX = 0;
                int pointY = 0;

                foreach (Tile tile in _layer.Tiles) {
                    TileCoord tileLoc = tile.Pool.GetTileLocation(tile.Id);

                    yield return new DrawCommand() {
                        Texture = tile.Pool.TextureId,
                        SourceRect = new Rectangle(tileLoc.X * tile.Width, tileLoc.Y * tile.Height, tile.Width, tile.Height),
                        DestRect = new Rectangle(
                            pointX * (int)(_layer.TileWidth * geometry.ZoomFactor),
                            pointY * (int)(_layer.TileHeight * geometry.ZoomFactor),
                            (int)(_layer.TileWidth * geometry.ZoomFactor),
                            (int)(_layer.TileHeight * geometry.ZoomFactor)),
                        BlendColor = Colors.White,
                    };

                    if (++pointX >= tilesWide) {
                        pointX = 0;
                        pointY++;
                    }
                }
            }
        }

        public TileCoord TileToCoord (Tile tile)
        {
            int tilesWide = TilesWide;

            int pointX = 0;
            int pointY = 0;

            foreach (Tile t in _layer.Tiles) {
                if (tile == t) {
                    return new TileCoord(pointX, pointY);
                }

                if (++pointX >= tilesWide) {
                    pointX = 0;
                    pointY++;
                }
            }

            throw new ArgumentException("The tile does not exist in the layer.", "tile");
        }

        private int CoordToIndex (int x, int y)
        {
            return y * TilesWide + x;

            throw new InvalidOperationException("Can't convert tileset coordinate to index if width not synced");
        }

        private int TilesWide
        {
            get { return LevelGeometry.ViewportBounds.Width / _layer.TileWidth; }
        }

        public void HandleStartPointerSequence (PointerEventInfo info)
        {
            int tileX = (int)Math.Floor(info.X / Layer.TileWidth);
            int tileY = (int)Math.Floor(info.Y / Layer.TileHeight);

            int index = CoordToIndex(tileX, tileY);
            if (index >= 0 && index < _layer.Count) {
                Tile tile = _layer[index];
                OnTileSelected(new TileEventArgs(tile));
            }
        }

        public void HandleEndPointerSequence (PointerEventInfo info)
        { }

        public void HandleUpdatePointerSequence (PointerEventInfo info)
        { }

        public void HandlePointerPosition (PointerEventInfo info)
        { }

        public void HandlePointerLeaveField ()
        { }

        public event EventHandler<TileEventArgs> TileSelected;

        protected virtual void OnTileSelected (TileEventArgs e)
        {
            var ev = TileSelected;
            if (ev != null)
                ev(this, e);
        }
    }
}
